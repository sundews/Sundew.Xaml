// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Launcher;

using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

public sealed partial class MainWindow : Window
{
    private readonly ObservableCollection<TesterItem> items = new();

    /// <summary>
    /// Everything the launcher started itself, so that closing it takes all of it down while leaving alone
    /// whatever was already running.
    /// </summary>
    private readonly JobObject ownStartedProcesses = new();

    private readonly DeviceAllocator devices;
    private readonly Microsoft.UI.Dispatching.DispatcherQueueTimer statusTimer;

    /// <summary>
    /// The window queue, kept because reading it off the window from a worker thread throws, and the throw goes
    /// nowhere when it happens in a task nothing awaits.
    /// </summary>
    private readonly Microsoft.UI.Dispatching.DispatcherQueue queue;

    /// <summary>
    /// Whether a round of asking is already under way, so that a slow one does not pile up behind itself.
    /// </summary>
    private bool polling;

    /// <summary>
    /// How many times running it has to be found gone before it counts. Deploying restarts the app, so one
    /// absence proves nothing; at a two second tick this waits about six seconds.
    /// </summary>
    private const int MissedReadingsBeforeStopped = 3;

    public MainWindow()
    {
        this.InitializeComponent();
        this.Title = "Sundew.Xaml tester launcher";
        this.SystemBackdrop = new MicaBackdrop();
        this.queue = this.DispatcherQueue;
        this.devices = new DeviceAllocator(this.ownStartedProcesses);
        this.devices.Output += (_, line) => this.Log(line);

        foreach (var tester in TesterCatalog.All)
        {
            this.items.Add(new TesterItem(tester));
        }

        this.TesterList.ItemsSource = this.items;
        this.Refresh();
        this.Closed += this.OnClosed;

        // dotnet run keeps running for as long as the tester does, so the only way to tell that a tester is up
        // rather than still building is to look for its process.
        this.statusTimer = this.queue.CreateTimer();
        this.statusTimer.Interval = TimeSpan.FromSeconds(2);
        this.statusTimer.Tick += (_, _) => this.UpdateRunningStates();
        this.statusTimer.Start();

        // Starting everything from the command line is both a convenience and the only way to exercise the
        // button without a hand on the mouse.
        if (Environment.GetCommandLineArgs().Contains("--launch-all", StringComparer.OrdinalIgnoreCase))
        {
            this.queue.TryEnqueue(() => this.OnLaunchAll(this, new RoutedEventArgs()));
        }
    }

    private string? DefaultVirtualDevice => this.VirtualDeviceBox.SelectedItem as string;

    private void Log(string line)
    {
        LogFile.Write(line);

        this.queue.TryEnqueue(() =>
        {
            this.LogBox.Text += line + Environment.NewLine;
            this.LogScroller.ChangeView(null, this.LogScroller.ScrollableHeight, null);
        });
    }

    private void Refresh()
    {
        var virtualDevices = Android.GetVirtualDevices();
        var selected = this.DefaultVirtualDevice;
        this.VirtualDeviceBox.ItemsSource = virtualDevices;
        this.VirtualDeviceBox.SelectedItem = selected;
        if (this.VirtualDeviceBox.SelectedItem == null && virtualDevices.Count > 0)
        {
            this.VirtualDeviceBox.SelectedIndex = 0;
        }

        foreach (var item in this.items)
        {
            item.SetVirtualDevices(virtualDevices);
        }

        var attached = Android.GetDevices();
        this.DeviceSummary.Text = attached.Count == 0
            ? Android.AdbPath == null ? "no Android SDK, set ANDROID_HOME" : "no device attached"
            : attached.Count + " attached: " + string.Join(", ", attached);
    }

    /// <summary>
    /// Keeps the list in step with what is actually running. It has to look both ways: a tester closed from
    /// inside itself tells the launcher nothing, and for two of the three kinds nothing on this machine ends
    /// when it does, so there is no exit to be notified of.
    /// </summary>
    /// <remarks>
    /// The asking is done off the UI thread. Finding out whether an Android head is still up means running adb,
    /// once per device, and waiting on three processes every couple of seconds is not something to do on the
    /// thread that is drawing.
    /// </remarks>
    private void UpdateRunningStates()
    {
        if (this.polling)
        {
            return;
        }

        this.polling = true;
        var running = this.items.Where(item => item.IsRunning).ToList();
        Task.Run(() =>
        {
            var states = running.Select(item => (Item: item, Alive: IsAlive(item))).ToList();
            this.queue.TryEnqueue(() =>
            {
                foreach (var (item, alive) in states)
                {
                    this.Apply(item, alive);
                }

                this.polling = false;
            });
        });
    }

    /// <summary>
    /// Takes what was found out about one tester and brings the list into line with it.
    /// </summary>
    /// <param name="item">The tester.</param>
    /// <param name="alive">Whether it is up, or null when nothing could say yet.</param>
    private void Apply(TesterItem item, bool? alive)
    {
        if (!item.IsRunning || alive == null)
        {
            return;
        }

        if (alive == true)
        {
            item.SeenAlive = true;
            item.MissedReadings = 0;
            if (!item.Started)
            {
                item.Started = true;
                item.Status = item.Device == null ? "Running" : "Running on " + item.Device;
            }

            return;
        }

        // Only once it has been seen up. Between starting a tester and its window appearing it is not running
        // either, and calling that stopped would take it off the list before it ever showed up.
        if (!item.SeenAlive)
        {
            return;
        }

        item.MissedReadings++;
        if (item.MissedReadings >= MissedReadingsBeforeStopped)
        {
            this.Log(item.Name + " stopped on its own");
            this.Stop(item);
            item.Status = "Stopped";
        }
    }

    /// <summary>
    /// Says whether a tester is up, asked of whatever actually knows: the device for an Android head, the
    /// browser window for a browser head, and the process itself for one that runs here. Null means nothing can
    /// say yet, which is not the same as stopped and must never be taken for it.
    /// </summary>
    /// <param name="item">The tester.</param>
    /// <returns><c>true</c> while it is running, <c>false</c> once it is not, null while unknowable.</returns>
    private static bool? IsAlive(TesterItem item)
    {
        switch (item.Tester.Kind)
        {
            case TesterKind.Android:
                // Nothing is concluded until the deployment has finished, which is what sets Started for an
                // Android head. Before a device has been handed out there is nothing to ask, and asking anyway
                // would reach whichever device happens to be attached and answer about the wrong one. Worse, an
                // emulator resumed from a snapshot is often still running the app from the last session, so an
                // answer given before this one was deployed is about the previous one and latches the wrong
                // thing: deploying then stops it to install, and that looks like it went away.
                return !item.Started || item.Device == null || item.Tester.ApplicationId == null
                    ? null
                    : Android.IsRunning(item.Device, item.Tester.ApplicationId);

            case TesterKind.Browser:
                // The head serves a page and the launcher opened a window onto it. Closing that window is how a
                // browser head is stopped; the server would otherwise go on serving nobody.
                return item.Session?.HasBrowserWindow;

            default:
                return item.Tester.ProcessName == null
                    ? null
                    : System.Diagnostics.Process.GetProcessesByName(item.Tester.ProcessName).Length > 0;
        }
    }

    private void Launch(TesterItem item)
    {
        if (item.IsRunning)
        {
            return;
        }

        item.IsRunning = true;
        if (item.Tester.Kind != TesterKind.Android)
        {
            this.Start(item, null);
            return;
        }

        var virtualDevice = item.VirtualDevice == TesterItem.DefaultVirtualDevice
            ? this.DefaultVirtualDevice
            : item.VirtualDevice;
        if (virtualDevice == null)
        {
            this.Log(item.Name + ": no emulator to run on");
            item.IsRunning = false;
            return;
        }

        // Finding or starting a device blocks for as long as an emulator takes to boot, so it happens off the
        // window's thread and the tester is started once it has one.
        item.Status = "Waiting for " + virtualDevice;
        Task.Run(() =>
        {
            var device = this.devices.Acquire(virtualDevice);
            this.queue.TryEnqueue(() =>
            {
                if (device == null)
                {
                    item.Status = "No device";
                    item.IsRunning = false;
                    return;
                }

                this.Start(item, device);
                this.Refresh();
            });
        });
    }

    private void Start(TesterItem item, string? device)
    {
        var session = new TesterSession(item.Tester, device);
        session.Output += (_, line) => this.Log(item.Name + ": " + line);
        session.Served += (_, url) => this.queue.TryEnqueue(() =>
        {
            item.Started = true;
            item.Status = "Serving at " + url;
        });
        session.Exited += (_, _) => this.queue.TryEnqueue(() =>
        {
            // An Android deployment ends as soon as the app is on the device, so the tester outlives it.
            if (item.Tester.Kind == TesterKind.Android)
            {
                item.Started = true;
                item.Status = "Running on " + (device ?? "device");
            }
            else
            {
                item.Status = "Exited with " + session.ExitCode;
                item.IsRunning = false;
            }
        });

        item.Device = device;
        item.Status = device == null ? "Building and starting" : "Deploying to " + device;
        item.Session = session;
        if (!session.Start())
        {
            item.Status = "Could not start";
            item.IsRunning = false;
            item.Session = null;
        }
    }

    /// <summary>
    /// The processes the sessions are running, which is how a browser window opened in app mode is tiled: it
    /// has no process name of its own to be found by.
    /// </summary>
    /// <returns>The process ids.</returns>
    private IEnumerable<int> BrowserProcessIds()
    {
        return this.items
            .Where(item => item.Session != null)
            .SelectMany(item => item.Session!.GetProcessIds());
    }

    private void Stop(TesterItem item)
    {
        item.Session?.Stop();
        item.Session = null;
        if (item.Device != null)
        {
            this.devices.Release(item.Device);
            item.Device = null;
        }

        item.IsRunning = false;
        item.Started = false;
        item.Status = "Not running";
    }

    private void LaunchAll(params TesterKind[] kinds)
    {
        foreach (var item in this.items.Where(item => kinds.Contains(item.Tester.Kind)))
        {
            this.Launch(item);
        }
    }

    private void OnLaunch(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is TesterItem item)
        {
            this.Launch(item);
        }
    }

    private void OnStop(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is TesterItem item)
        {
            this.Stop(item);
        }
    }

    private async void OnLaunchAll(object sender, RoutedEventArgs e)
    {
        if (this.BuildFirstBox.IsChecked == true)
        {
            this.Log("Building the solution once, so that what the testers share is not built for each of them.");
            var built = await Task.Run(() => SolutionBuilder.Build(this.Log));
            this.Log(built ? "Solution build succeeded" : "Solution build failed, starting the testers anyway");
        }

        this.LaunchAll(TesterKind.Windows, TesterKind.Android, TesterKind.Browser);
        await this.TileWhenEverythingIsUp();
    }

    /// <summary>
    /// Waits for everything that was started to be up, and lays it out then.
    /// </summary>
    /// <remarks>
    /// Laying out before then would place the few windows that had appeared across the whole screen and leave the
    /// rest to open wherever they liked.
    /// </remarks>
    /// <returns>A task that completes once the windows have been laid out.</returns>
    private async Task TileWhenEverythingIsUp()
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromMinutes(12);
        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            if (this.items.All(item => !item.IsRunning || item.Started))
            {
                break;
            }
        }

        // A window takes a moment more to be worth moving once its process is there.
        await Task.Delay(TimeSpan.FromSeconds(3));
        var moved = await WindowTiler.TileAsync(WinRT.Interop.WindowNative.GetWindowHandle(this), this.BrowserProcessIds());
        this.Log("Tiled " + moved + " windows");
    }

    private void OnLaunchWindows(object sender, RoutedEventArgs e) => this.LaunchAll(TesterKind.Windows);

    private void OnLaunchAndroid(object sender, RoutedEventArgs e) => this.LaunchAll(TesterKind.Android);

    private void OnLaunchBrowser(object sender, RoutedEventArgs e) => this.LaunchAll(TesterKind.Browser);

    private void OnStopAll(object sender, RoutedEventArgs e)
    {
        foreach (var item in this.items)
        {
            this.Stop(item);
        }

        this.devices.Release();
    }

    private void OnRefresh(object sender, RoutedEventArgs e) => this.Refresh();

    private async void OnTile(object sender, RoutedEventArgs e)
    {
        var moved = await WindowTiler.TileAsync(WinRT.Interop.WindowNative.GetWindowHandle(this), this.BrowserProcessIds());
        this.Log(moved == 0 ? "No tester windows to tile yet" : "Tiled " + moved + " windows");
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        foreach (var item in this.items)
        {
            this.Stop(item);
        }

        this.ownStartedProcesses.Dispose();
    }
}
