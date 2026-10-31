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
    /// Raised when the process backing the tester ends on its own.
    /// </summary>
    public event EventHandler? Exited;

    /// <summary>
    /// Raised with the url a browser head is serving on, once it says what it is.
    /// </summary>
    public event EventHandler<string>? Served;

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

        var projectPath = Path.Combine(source, this.tester.ProjectName);
        var nativeBinary = this.FindNativeBinary(projectPath);
        var startInfo = nativeBinary == null
            ? new ProcessStartInfo("dotnet", this.GetArguments(projectPath))
            : new ProcessStartInfo(nativeBinary, this.applicationArguments ?? string.Empty);
        startInfo.WorkingDirectory = source;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;

        var startedBy = nativeBinary == null ? "dotnet " + startInfo.Arguments : "native " + nativeBinary;
        this.Output?.Invoke(this, startedBy);
        this.process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        this.process.OutputDataReceived += this.OnData;
        this.process.ErrorDataReceived += this.OnData;
        this.process.Exited += (_, _) =>
        {
            this.ExitCode = this.process?.ExitCode ?? 0;
            if (this.tester.Kind == TesterKind.Android && this.tester.SupportsNativeAot && this.ExitCode == 0)
            {
                this.InstallAndStart(projectPath);
            }

            this.Exited?.Invoke(this, EventArgs.Empty);
        };
        if (!this.process.Start())
        {
            return false;
        }

        // Into the job before it has a chance to start anything of its own, so that the whole tree is covered.
        this.job.Add(this.process);
        this.process.BeginOutputReadLine();
        this.process.BeginErrorReadLine();
        return true;
    }

    /// <summary>
    /// Stops the tester, and anything the build started on its behalf.
    /// </summary>
    public void Stop()
    {
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
    /// Finds the native binary of a head that has been published ahead of time, which is run in preference to the
    /// CLI. Running it through the CLI would start the same build under the JIT, so a head that says it is native
    /// has to be started as one.
    /// </summary>
    /// <param name="projectPath">The project directory.</param>
    /// <returns>The binary, or null when the head cannot be published that way or has not been.</returns>
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
        return File.Exists(path) ? path : null;
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
