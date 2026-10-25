// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TesterItem.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Launcher;

using System.ComponentModel;
using Microsoft.UI.Xaml;
using System.Runtime.CompilerServices;

/// <summary>
/// One row of the list: a tester, and whatever it is currently doing.
/// </summary>
public sealed class TesterItem : INotifyPropertyChanged
{
    /// <summary>
    /// What the per tester selector shows when the tester should simply use the one chosen at the top.
    /// </summary>
    public const string DefaultVirtualDevice = "(default)";

    private string status = "Not running";
    private string virtualDevice = DefaultVirtualDevice;
    private bool isRunning;
    private bool started;

    /// <summary>
    /// Initializes a new instance of the <see cref="TesterItem"/> class.
    /// </summary>
    /// <param name="tester">The tester.</param>
    public TesterItem(Tester tester)
    {
        this.Tester = tester;
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the tester.
    /// </summary>
    public Tester Tester { get; }

    /// <summary>
    /// Gets the name shown in the list.
    /// </summary>
    public string Name => this.Tester.Name;

    /// <summary>
    /// Gets the head shown in the list.
    /// </summary>
    public string TargetFramework => this.Tester.TargetFramework;

    /// <summary>
    /// Gets a value indicating whether this tester runs on an Android device, and so has one to choose.
    /// </summary>
    public bool IsAndroid => this.Tester.Kind == TesterKind.Android;

    /// <summary>
    /// Gets whether the virtual device selector is shown, which is only for the Android testers.
    /// </summary>
    public Visibility AndroidVisibility => this.IsAndroid ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// Gets the virtual devices this tester can be launched on, the first being the one chosen at the top.
    /// </summary>
    public IReadOnlyList<string> VirtualDevices { get; private set; } = [DefaultVirtualDevice];

    /// <summary>
    /// Gets or sets the virtual device this tester is launched on, or <see cref="DefaultVirtualDevice"/>.
    /// </summary>
    public string VirtualDevice
    {
        get => this.virtualDevice;
        set => this.Set(ref this.virtualDevice, value);
    }

    /// <summary>
    /// Offers the virtual devices that can be chosen for this tester.
    /// </summary>
    /// <param name="names">The virtual devices the machine has.</param>
    public void SetVirtualDevices(IReadOnlyList<string> names)
    {
        var chosen = this.virtualDevice;
        this.VirtualDevices = new[] { DefaultVirtualDevice }.Concat(names).ToList();
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.VirtualDevices)));
        this.VirtualDevice = this.VirtualDevices.Contains(chosen) ? chosen : DefaultVirtualDevice;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the tester itself is up, rather than still being built.
    /// </summary>
    public bool Started
    {
        get => this.started;
        set => this.Set(ref this.started, value);
    }

    /// <summary>
    /// Gets or sets the device this tester was launched on, while it is running.
    /// </summary>
    public string? Device { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this tester has been seen running at least once, which is what
    /// separates one that has since been closed from one that has not come up yet.
    /// </summary>
    public bool SeenAlive { get; set; }

    /// <summary>
    /// Gets or sets how many times in a row it has been found not running. Deploying to a device stops the app
    /// and starts it again, and an emulator resumed from a snapshot may be running the last one deployed to it,
    /// so a single absence means very little; several in a row mean it has gone.
    /// </summary>
    public int MissedReadings { get; set; }

    /// <summary>
    /// Gets or sets the session, while it is running.
    /// </summary>
    public TesterSession? Session { get; set; }

    /// <summary>
    /// Gets or sets what the tester is currently doing.
    /// </summary>
    public string Status
    {
        get => this.status;
        set => this.Set(ref this.status, value);
    }

    /// <summary>
    /// Gets a value indicating whether the tester can be started, which is simply that it is not running.
    /// </summary>
    public bool CanLaunch => !this.isRunning;

    /// <summary>
    /// Gets or sets a value indicating whether the tester has been started.
    /// </summary>
    public bool IsRunning
    {
        get => this.isRunning;
        set
        {
            this.Set(ref this.isRunning, value);
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CanLaunch)));
        }
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value))
        {
            return;
        }

        field = value;
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
