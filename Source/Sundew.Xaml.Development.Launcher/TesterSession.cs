// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TesterSession.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Launcher;

using System.Diagnostics;
using System.IO;

/// <summary>
/// One tester, started and running.
/// </summary>
public sealed class TesterSession : IDisposable
{
    private readonly Tester tester;
    private readonly string? device;
    private readonly string? applicationArguments;
    private bool browserOpened;
    private bool browserSeen;
    private readonly JobObject job = new JobObject();
    private Process? process;
    private string source = string.Empty;
    private string projectPath = string.Empty;
    private bool publishing;
    private bool stopped;

    /// <summary>
    /// Initializes a new instance of the <see cref="TesterSession"/> class.
    /// </summary>
    /// <param name="tester">The tester.</param>
    /// <param name="device">The Android device to deploy to, where that applies.</param>
    /// <param name="applicationArguments">Anything the tester itself should be told, rather than the build.</param>
    public TesterSession(Tester tester, string? device, string? applicationArguments = null)
    {
        this.tester = tester;
        this.device = device;
        this.applicationArguments = applicationArguments;
    }

    /// <summary>
    /// Raised for every line the build and the tester write.
    /// </summary>
    public event EventHandler<string>? Output;

    /// <summary>
    /// Gets a value indicating whether the head that was started is the one compiled ahead of time, rather than
    /// the same code run by the just in time compiler.
    /// </summary>
    public bool IsNative { get; private set; }

    /// <summary>
    /// Gets a value indicating whether what is running is the publish a head compiled ahead of time needs,
    /// rather than the tester itself. The tester follows once the publish has finished.
    /// </summary>
    public bool IsPublishing => this.publishing;

    /// <summary>
    /// Raised when the process backing the tester ends on its own.
    /// </summary>
    public event EventHandler? Exited;

    /// <summary>
    /// Raised with the url a browser head is serving on, once it says what it is.
    /// </summary>
    public event EventHandler<string>? Served;

    /// <summary>
    /// Raised once the process that is the tester itself has started, which for a head compiled ahead of time is
    /// after the publish it needed. Until then nothing can say how it is going to run.
    /// </summary>
    public event EventHandler? Running;

    /// <summary>
    /// Gets what the tester ended with, once it has.
    /// </summary>
    public int ExitCode { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the window this session opened onto a browser head is still there. The
    /// browser the launcher starts hands over to a process of its own and exits, so the window is looked for
    /// among what the job holds rather than through anything this session started itself.
    /// </summary>
    public bool? HasBrowserWindow
    {
        get
        {
            if (!this.browserOpened)
            {
                // Not opened yet, which is nothing to conclude from either way.
                return null;
            }

            foreach (var id in this.job.GetProcessIds())
            {
                try
                {
                    using var process = Process.GetProcessById(id);
                    if (process.ProcessName is "msedge" or "chrome")
                    {
                        this.browserSeen = true;
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                    // Gone between being listed and being looked at, which is simply one that is not it.
                }
            }

            return !this.browserSeen;
        }
    }

    /// <summary>
    /// Lists what the session has running, which is how windows it owns but cannot be found by
    /// name, such as a browser opened in app mode, are tiled with the rest.
    /// </summary>
    /// <returns>The process ids.</returns>
    public IReadOnlyList<int> GetProcessIds()
    {
        return this.job.GetProcessIds();
    }

    /// <summary>
    /// Finds the Source directory of the repository, by walking up from wherever the launcher was built to.
    /// </summary>
    /// <returns>The directory, or null when it is not above us.</returns>
    public static string? FindSourceDirectory()
    {
        // Normally the launcher runs from its own build output inside the repository. Falling back to the
        // working directory lets it be run from elsewhere, which is what any harness around it will do.
        return SearchUpwards(AppContext.BaseDirectory) ?? SearchUpwards(Environment.CurrentDirectory);
    }

    private static string? SearchUpwards(string from)
    {
        var directory = new DirectoryInfo(from);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Sundew.Xaml.slnx")))
            {
                return directory.FullName;
            }

            var nested = Path.Combine(directory.FullName, "Source", "Sundew.Xaml.slnx");
            if (File.Exists(nested))
            {
                return Path.GetDirectoryName(nested);
            }

            directory = directory.Parent;
        }

        return null;
    }

    /// <summary>
    /// Builds and starts the tester.
    /// </summary>
    /// <returns><c>true</c> when it started.</returns>
    public bool Start()
    {
        var source = FindSourceDirectory();
        if (source == null)
        {
            this.Output?.Invoke(this, "Could not find the Source directory above " + AppContext.BaseDirectory);
            return false;
        }

        this.source = source;
        this.projectPath = Path.Combine(source, this.tester.ProjectName);
        var nativeBinary = this.FindNativeBinary(this.projectPath);

        // With nothing published to run, a head that can be compiled ahead of time is published rather than
        // started through the CLI. Starting it that way runs the very same build under the JIT, which is not what
        // launching a head that says it is native asks for, and the publish is the only thing that was missing.
        // The tester itself follows once the publish has finished, the way an Android head is installed once its
        // own publish has.
        if (nativeBinary == null && this.CanPublish)
        {
            this.publishing = true;
            return this.StartProcess(this.CreatePublishStartInfo());
        }

        return this.StartTester(nativeBinary);
    }

    /// <summary>
    /// Stops the tester, and anything the build started on its behalf.
    /// </summary>
    public void Stop()
    {
        // Before the job goes, so that a publish that is killed with it is not taken for one that finished and
        // the tester started behind the back of whoever asked for it to stop.
        this.stopped = true;

        // An Android tester is not a child process at all; it runs on the device, so it has to be told to stop.
        if (this.tester.Kind == TesterKind.Android && this.tester.ApplicationId != null)
        {
            Android.ForceStop(this.device, this.tester.ApplicationId);
        }

        this.job.Dispose();
        this.process = null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Stop();
    }

    /// <summary>
    /// Gets a value indicating whether this is a head the launcher can publish ahead of time itself. An Android
    /// head is published too, but as part of being deployed, which is a path of its own.
    /// </summary>
    private bool CanPublish =>
        this.tester.Kind == TesterKind.Windows && this.tester.SupportsNativeAot && this.tester.ProcessName != null;

    /// <summary>
    /// Starts the tester, either the binary published ahead of time or the same build under the JIT.
    /// </summary>
    /// <param name="nativeBinary">The published binary, or null to go through the CLI.</param>
    /// <returns><c>true</c> when it started.</returns>
    private bool StartTester(string? nativeBinary)
    {
        this.IsNative = nativeBinary != null;
        var startInfo = nativeBinary == null
            ? new ProcessStartInfo("dotnet", this.GetArguments(this.projectPath))
            : new ProcessStartInfo(nativeBinary, this.applicationArguments ?? string.Empty);
        if (!this.StartProcess(startInfo))
        {
            return false;
        }

        this.Running?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// Starts one of the processes this session is made of, and takes its output as the session's own.
    /// </summary>
    /// <param name="startInfo">What to start.</param>
    /// <returns><c>true</c> when it started.</returns>
    private bool StartProcess(ProcessStartInfo startInfo)
    {
        startInfo.WorkingDirectory = this.source;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        this.Output?.Invoke(
            this,
            startInfo.FileName == "dotnet" ? "dotnet " + startInfo.Arguments : "native " + startInfo.FileName);
        var started = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        started.OutputDataReceived += this.OnData;
        started.ErrorDataReceived += this.OnData;
        started.Exited += (_, _) => this.OnExited(started);
        this.process = started;
        if (!started.Start())
        {
            return false;
        }

        // Into the job before it has a chance to start anything of its own, so that the whole tree is covered.
        this.job.Add(started);
        started.BeginOutputReadLine();
        started.BeginErrorReadLine();
        return true;
    }

    /// <summary>
    /// Takes one of this session's processes ending, which for the publish is where the tester is started and
    /// for the tester itself is where the session is over.
    /// </summary>
    /// <param name="ended">The process that ended.</param>
    private void OnExited(Process ended)
    {
        if (this.stopped)
        {
            return;
        }

        this.ExitCode = ended.ExitCode;
        if (this.publishing)
        {
            this.publishing = false;
            this.StartPublished();
            return;
        }

        if (this.tester.Kind == TesterKind.Android && this.tester.SupportsNativeAot && this.ExitCode == 0)
        {
            this.InstallAndStart(this.projectPath);
        }

        this.Exited?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Starts what the publish produced. Whatever came of it, the tester is still worth having, so anything
    /// short of a binary to run falls back to the CLI, which is how these heads were started before there was a
    /// publish to prefer. The sheet then says it is under the JIT, and says so truthfully.
    /// </summary>
    private void StartPublished()
    {
        if (this.ExitCode == 0)
        {
            // Looked for again rather than taken for granted: a publish can finish without leaving anything
            // where the binary is expected.
            var published = this.FindNativeBinary(this.projectPath);
            if (published != null)
            {
                this.StartTester(published);
                return;
            }

            this.Output?.Invoke(this, "Published, but no native binary was found to run");
        }
        else
        {
            this.Output?.Invoke(
                this, "Could not publish ahead of time, so the same build is being run under the JIT");
        }

        this.StartTester(null);
    }

    /// <summary>
    /// The publish that produces the native binary, which is the Debug publish of this one head that the
    /// message printed in its place used to ask whoever read it to run by hand.
    /// </summary>
    /// <returns>What to start.</returns>
    private ProcessStartInfo CreatePublishStartInfo()
    {
        var arguments = "publish \"" + this.projectPath + "\" -f " + this.tester.TargetFramework
                        + " -c Debug -p:SppDisable=true";
        if (!string.IsNullOrEmpty(this.tester.ExtraArguments))
        {
            arguments += " " + this.tester.ExtraArguments;
        }

        var startInfo = new ProcessStartInfo("dotnet", arguments);

        // The native link step finds the C++ toolchain by calling vswhere, which it expects on the PATH; it is
        // not there by default, even where Visual Studio is installed. What is written when it cannot be found
        // goes into the same output the path to the linker is then read out of, so the link is attempted with
        // that complaint in front of it and fails. A developer command prompt has the installer directory on the
        // PATH and the publish works there, so the publish the launcher runs is given it too.
        var installer = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Microsoft Visual Studio",
            "Installer");
        if (Directory.Exists(installer))
        {
            startInfo.Environment["PATH"] =
                startInfo.Environment.TryGetValue("PATH", out var path) && !string.IsNullOrEmpty(path)
                    ? path + Path.PathSeparator + installer
                    : installer;
        }

        return startInfo;
    }

    /// <summary>
    /// Finds the native binary of a head that has been published ahead of time, which is run in preference to the
    /// CLI. Running it through the CLI would start the same build under the JIT, so a head that says it is native
    /// has to be started as one.
    /// </summary>
    /// <param name="projectPath">The project directory.</param>
    /// <returns>The binary, or null when the head cannot be published that way, has not been, or was published
    /// before the code it was published from was last built.</returns>
    private string? FindNativeBinary(string projectPath)
    {
        if (!this.tester.SupportsNativeAot || this.tester.ProcessName == null)
        {
            return null;
        }

        // Debug because that is what the launcher starts; a head published for Release sits beside this one and is
        // left alone.
        var path = Path.Combine(
            projectPath, "bin", "Debug", this.tester.TargetFramework, "win-x64", "publish", this.tester.ProcessName + ".exe");
        if (!File.Exists(path))
        {
            return null;
        }

        // A published head is whatever it was when it was published, and nothing republishes it: building the
        // solution leaves it exactly as it was. Running it regardless means running code that may be days behind
        // the source, and looking at the result as though it were the change just made. So it is used only while
        // it is no older than the assembly an ordinary build writes, which the publish does not touch, since it
        // writes its own under the runtime identifier.
        //
        // When it was published is taken from the folder rather than from the binary in it. Publishing copies the
        // binary with the time it was compiled at, which can be older than a build that came after it and left
        // the binary alone; the folder is written to whenever something is actually published into it.
        var assembly = Path.Combine(
            projectPath, "bin", "Debug", this.tester.TargetFramework, this.tester.ProcessName + ".dll");
        var publishedAt = File.GetLastWriteTimeUtc(path);
        var publishedInto = Directory.GetLastWriteTimeUtc(Path.GetDirectoryName(path)!);
        if (publishedInto > publishedAt)
        {
            publishedAt = publishedInto;
        }

        if (File.Exists(assembly) && File.GetLastWriteTimeUtc(assembly) > publishedAt)
        {
            this.Output?.Invoke(
                this,
                "The head published ahead of time is older than the last build, so it is being published again");
            return null;
        }

        return path;
    }

    /// <summary>
    /// Installs what was published onto the device and starts it. A published head is an apk and nothing more, so
    /// unlike the fast deployment this replaces there is no build tooling left to start it: adb does both.
    /// </summary>
    /// <param name="projectPath">The project directory.</param>
    private void InstallAndStart(string projectPath)
    {
        var apk = this.FindApk(projectPath);
        if (apk == null)
        {
            this.Output?.Invoke(this, "Published, but no apk was found to install");
            return;
        }

        // An emulator resumed from a snapshot often comes up already running the app from whenever the snapshot
        // was taken, on every device started from it. Those are not this session and they are running whatever
        // code was current then, so they are stopped everywhere before this one is put on.
        foreach (var attached in Android.GetDevices())
        {
            Android.ForceStop(attached, this.tester.ApplicationId!);
        }

        this.Output?.Invoke(this, "Installing " + Path.GetFileName(apk));
        var installed = Android.Install(this.device, apk).Trim();
        if (!installed.Contains("Success", StringComparison.OrdinalIgnoreCase))
        {
            this.Output?.Invoke(this, installed.Length == 0 ? "adb install said nothing" : installed);
            return;
        }

        if (this.tester.ApplicationId != null)
        {
            Android.Start(this.device, this.tester.ApplicationId);
            this.Output?.Invoke(this, "Started " + this.tester.ApplicationId);
        }

        this.RepairRestore(projectPath);
    }

    /// <summary>
    /// Puts back what the project had restored before it was published. Publishing a head ahead of time has to
    /// restore it as a single framework project, both because the native compiler only joins in then and because
    /// the publish hooks do not survive the cross targeting dispatch. That leaves the shared obj saying the other
    /// heads do not exist, and the next head started without building finds nothing to run.
    /// </summary>
    /// <param name="projectPath">The project directory.</param>
    private void RepairRestore(string projectPath)
    {
        try
        {
            using var restore = Process.Start(new ProcessStartInfo("dotnet", "restore \"" + projectPath + "\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            restore?.WaitForExit();
            this.Output?.Invoke(
                this,
                restore?.ExitCode == 0 ? "Restored the other heads" : "Could not restore the other heads");
        }
        catch (System.ComponentModel.Win32Exception exception)
        {
            this.Output?.Invoke(this, "Could not restore the other heads: " + exception.Message);
        }
    }

    /// <summary>
    /// Finds the apk to install, which is the one the publish just wrote for this device architecture.
    /// </summary>
    /// <remarks>
    /// It has to be that one and no other. An ordinary build of the solution also produces an apk, beside the
    /// framework folder rather than under the runtime one, and that apk carries no native library: taking
    /// whichever is newest installed it whenever the solution had been built since the last publish, and the
    /// head then came up saying it was running under the JIT.
    /// </remarks>
    /// <param name="projectPath">The project directory.</param>
    /// <returns>The apk, or null when the publish produced none.</returns>
    private string? FindApk(string projectPath)
    {
        var runtimeIdentifier = Android.GetRuntimeIdentifier(this.device);
        var published = Path.Combine(
            projectPath, "bin", "Debug", this.tester.TargetFramework, runtimeIdentifier, "publish");
        if (!Directory.Exists(published))
        {
            return null;
        }

        return Directory.EnumerateFiles(published, "*-Signed.apk", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private string GetArguments(string projectPath)
    {
        var quoted = '"' + projectPath + '"';
        var androidRuntimeIdentifier = this.tester.Kind == TesterKind.Android && this.tester.SupportsNativeAot
            ? Android.GetRuntimeIdentifier(this.device)
            : string.Empty;

        // An Android head compiled ahead of time cannot be deployed the fast way: fast deployment pushes
        // assemblies for the runtime to load, and a native head has none, its code being a library inside the apk.
        // Such a head is published, installed and started instead. The rest go the fast way, -t:Run being how a
        // .NET Android app is normally deployed and started; it builds as well, which there is no separating.
        //
        // The publish target is invoked directly rather than through the publish verb, naming the framework as a
        // property. A project with several frameworks is cross targeting, and the publish verb dispatches to an
        // inner build for the one asked for; the NativeAOT hooks do not survive that dispatch, so the native link
        // never runs and packaging then looks for a library nothing produced. Naming it as a property builds the
        // project as the single framework one it has to be, and leaves what was restored for the other heads be.
        var aotPublish = "publish " + quoted + " -f " + this.tester.TargetFramework
                         + " -r " + androidRuntimeIdentifier + " -c Debug -p:SundewAndroidAot=true";
        var arguments = this.tester.Kind != TesterKind.Android
            ? "run --no-build --project " + quoted + " -f " + this.tester.TargetFramework
            : this.tester.SupportsNativeAot
                ? aotPublish
                : "build " + quoted + " -f " + this.tester.TargetFramework + " -t:Run";

        if (this.tester.Kind == TesterKind.Android && !this.tester.SupportsNativeAot
            && !string.IsNullOrEmpty(this.device))
        {
            arguments += " -p:AdbTarget=\"-s " + this.device + "\"";
        }

        if (!string.IsNullOrEmpty(this.tester.ExtraArguments))
        {
            arguments += " " + this.tester.ExtraArguments;
        }

        // The testers are not packable, but the libraries they reference are, and the packaging step races itself
        // on a local build. Nothing here needs a package.
        arguments += " -p:SppDisable=true";
        if (!string.IsNullOrEmpty(this.applicationArguments))
        {
            // Everything after the two dashes belongs to the tester rather than to the build.
            arguments += " -- " + this.applicationArguments;
        }

        return arguments;
    }

    private void OnData(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null)
        {
            return;
        }

        this.Output?.Invoke(this, e.Data);
        if (this.browserOpened || this.tester.Kind != TesterKind.Browser)
        {
            return;
        }

        // Both browser heads serve a page and print where, but neither opens anything.
        var match = System.Text.RegularExpressions.Regex.Match(e.Data, "http://[^ ]+");
        if (!match.Success)
        {
            return;
        }

        this.browserOpened = true;
        var browser = Browser.Open(match.Value, this.tester.ProjectName);
        if (browser != null)
        {
            // Into the session job, so that stopping the tester closes the window it is being looked at in.
            // The browser hands over to a process of its own and exits, and job membership is inherited, so the
            // window that appears is in the job even though this process is not.
            this.job.Add(browser);
        }

        this.Served?.Invoke(this, match.Value);
    }
}
