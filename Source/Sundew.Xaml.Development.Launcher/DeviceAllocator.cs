// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceAllocator.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Launcher;

/// <summary>
/// Hands each Android tester a device of its own, starting more emulators when there are not enough.
/// </summary>
/// <remarks>
/// Comparing the Android testers means running all three at once, which means a device each. A virtual device only
/// starts once, because the running instance locks it, so every instance after the first is started read only.
/// </remarks>
public sealed class DeviceAllocator
{
    private readonly JobObject job;
    private readonly object gate = new();
    private readonly HashSet<string> taken = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceAllocator"/> class.
    /// </summary>
    /// <param name="job">The job started emulators are put in, so they go down with the launcher.</param>
    public DeviceAllocator(JobObject job)
    {
        this.job = job;
    }

    /// <summary>
    /// Raised with anything worth telling the user while a device is being made ready.
    /// </summary>
    public event EventHandler<string>? Output;

    /// <summary>
    /// Forgets which devices are in use, so that the next launch can take them again.
    /// </summary>
    public void Release()
    {
        lock (this.gate)
        {
            this.taken.Clear();
        }
    }

    /// <summary>
    /// Forgets one device.
    /// </summary>
    /// <param name="device">The serial number.</param>
    public void Release(string device)
    {
        lock (this.gate)
        {
            this.taken.Remove(device);
        }
    }

    /// <summary>
    /// Starts another instance of the given virtual device and reserves it.
    /// </summary>
    /// <remarks>
    /// The three Android testers ask for a device at the same time, so the choosing is done under a lock and the
    /// serial number is reserved there and then. Only the waiting, which is however long an emulator takes to
    /// boot, happens outside it, so that three emulators come up together rather than one after another.
    /// </remarks>
    /// <param name="virtualDevice">The virtual device to run on.</param>
    /// <returns>The serial number, or null when none could be started.</returns>
    public string? Acquire(string virtualDevice)
    {
        string serial;
        lock (this.gate)
        {
            // Every tester gets an emulator of its own rather than sharing one that happens to be attached, so
            // that the three of them can be seen side by side.
            var port = this.GetFreePort(Android.GetDevices());
            if (port == 0)
            {
                this.Output?.Invoke(this, "No free emulator port");
                return null;
            }

            serial = "emulator-" + port;
            this.taken.Add(serial);
        }

        this.Output?.Invoke(this, "Starting " + virtualDevice + " as " + serial);
        var port2 = int.Parse(serial.Substring("emulator-".Length));

        // Always read only. A virtual device locks itself, and the emulator refuses a second instance unless
        // every instance of it, the first one included, was started this way.
        var process = Android.StartEmulator(virtualDevice, port2, true);
        if (process == null)
        {
            this.Output?.Invoke(this, "Could not start the emulator. Is ANDROID_HOME set?");
            this.Release(serial);
            return null;
        }

        // Started by us, so it goes down with us; one that was already running is left alone.
        this.job.Add(process);
        if (!Android.WaitForBoot(serial, TimeSpan.FromMinutes(8)))
        {
            this.Output?.Invoke(this, serial + " did not finish booting");
            this.Release(serial);
            return null;
        }

        this.Output?.Invoke(this, serial + " is ready");
        return serial;
    }

    private int GetFreePort(IReadOnlyList<string> attached)
    {
        var used = attached
            .Where(device => device.StartsWith("emulator-", StringComparison.Ordinal))
            .Concat(this.taken)
            .Select(device => int.TryParse(device.Replace("emulator-", string.Empty), out var value) ? value : 0)
            .ToHashSet();
        for (var port = 5554; port < 5680; port += 2)
        {
            if (!used.Contains(port))
            {
                return port;
            }
        }

        return 0;
    }
}
