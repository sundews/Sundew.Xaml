// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Android.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Launcher;

using System.Diagnostics;
using System.IO;

/// <summary>
/// The parts of the Android SDK the launcher needs: which devices are attached, which tablets can be started, and
/// how to stop an app once it is running on one.
/// </summary>
public static class Android
{
    /// <summary>
    /// Gets the path to adb, or null when no Android SDK can be found.
    /// </summary>
    public static string? AdbPath => Tool("platform-tools", "adb.exe");

    /// <summary>
    /// Gets the path to the emulator, or null when no Android SDK can be found.
    /// </summary>
    public static string? EmulatorPath => Tool("emulator", "emulator.exe");

    /// <summary>
    /// Lists the serial numbers of the attached devices.
    /// </summary>
    /// <returns>The serial numbers.</returns>
    public static IReadOnlyList<string> GetDevices()
    {
        var devices = new List<string>();
        foreach (var line in Run(AdbPath, "devices").Split('\n'))
        {
            var parts = line.Trim().Split('\t');
            if (parts.Length == 2 && parts[1].Trim() == "device")
            {
                devices.Add(parts[0].Trim());
            }
        }

        return devices;
    }

    /// <summary>
    /// Lists the virtual devices that can be started.
    /// </summary>
    /// <returns>The names, tablets first, because those are what the sheets are meant to be compared on.</returns>
    public static IReadOnlyList<string> GetVirtualDevices()
    {
        var names = Run(EmulatorPath, "-list-avds")
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.Contains(' '))
            .ToList();
        return names
            .OrderByDescending(name => name.Contains("tablet", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Stops an app that is running on a device.
    /// </summary>
    /// <param name="device">The serial number, or null for the only attached device.</param>
    /// <param name="applicationId">The package.</param>
    public static void ForceStop(string? device, string applicationId)
    {
        var target = string.IsNullOrEmpty(device) ? string.Empty : "-s " + device + " ";
        Run(AdbPath, target + "shell am force-stop " + applicationId);
    }

    /// <summary>
    /// Asks a device which architecture it runs, so that the right one is published for it. The emulators are
    /// x64 and real devices are arm64, and NativeAOT has a runtime pack for each.
    /// </summary>
    /// <param name="device">The serial number, or null for the only attached device.</param>
    /// <returns>The runtime identifier.</returns>
    public static string GetRuntimeIdentifier(string? device)
    {
        var target = string.IsNullOrEmpty(device) ? string.Empty : "-s " + device + " ";
        var abi = Run(AdbPath, target + "shell getprop ro.product.cpu.abi").Trim();
        return abi.StartsWith("arm", StringComparison.OrdinalIgnoreCase) ? "android-arm64" : "android-x64";
    }

    /// <summary>
    /// Installs an apk on a device, replacing whatever is there.
    /// </summary>
    /// <param name="device">The serial number, or null for the only attached device.</param>
    /// <param name="apk">The apk.</param>
    /// <returns>What adb said, which names the failure when there was one.</returns>
    public static string Install(string? device, string apk)
    {
        var target = string.IsNullOrEmpty(device) ? string.Empty : "-s " + device + " ";
        return Run(AdbPath, target + "install -r -t \"" + apk + "\"");
    }

    /// <summary>
    /// Starts an installed app through its launcher activity.
    /// </summary>
    /// <param name="device">The serial number, or null for the only attached device.</param>
    /// <param name="applicationId">The package.</param>
    /// <returns>What adb said.</returns>
    public static string Start(string? device, string applicationId)
    {
        var target = string.IsNullOrEmpty(device) ? string.Empty : "-s " + device + " ";
        return Run(AdbPath, target + "shell monkey -p " + applicationId + " -c android.intent.category.LAUNCHER 1");
    }

    /// <summary>
    /// Says whether an app is still running on a device, which is the only way to tell that one was closed on
    /// the device itself: nothing on this machine is running it, so no process here ends when it does.
    /// </summary>
    /// <param name="device">The serial number, or null for the only attached device.</param>
    /// <param name="applicationId">The package.</param>
    /// <returns><c>true</c> while the app has a process on the device.</returns>
    public static bool IsRunning(string? device, string applicationId)
    {
        var target = string.IsNullOrEmpty(device) ? string.Empty : "-s " + device + " ";
        return Run(AdbPath, target + "shell pidof " + applicationId).Trim().Length > 0;
    }

    /// <summary>
    /// Says whether an app is running on any attached device. The launcher stops the app everywhere before it
    /// installs, so only the one it started can be up, and asking every device avoids depending on the serial
    /// number having been tracked correctly. Getting that wrong declares a running tester stopped and then stops
    /// it for real, which is worse than being slow to notice one that has gone.
    /// </summary>
    /// <param name="applicationId">The package.</param>
    /// <returns><c>true</c> while any device is running it.</returns>
    public static bool IsRunningAnywhere(string applicationId)
    {
        foreach (var device in GetDevices())
        {
            if (IsRunning(device, applicationId))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Waits until a device has finished booting.
    /// </summary>
    /// <param name="device">The serial number, or null for the only attached device.</param>
    /// <param name="timeout">How long to wait.</param>
    /// <returns><c>true</c> if it booted in time.</returns>
    public static bool WaitForBoot(string? device, TimeSpan timeout)
    {
        var target = string.IsNullOrEmpty(device) ? string.Empty : "-s " + device + " ";
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (Run(AdbPath, target + "shell getprop sys.boot_completed").Trim() == "1")
            {
                return true;
            }

            Thread.Sleep(2000);
        }

        return false;
    }

    /// <summary>
    /// Asks a running device which virtual device it was started from.
    /// </summary>
    /// <param name="device">The serial number.</param>
    /// <returns>The name, or null when it is not an emulator.</returns>
    public static string? GetVirtualDeviceName(string device)
    {
        var name = Run(AdbPath, "-s " + device + " emu avd name")
            .Split(new[] { (char)10, (char)13 }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
        return string.IsNullOrWhiteSpace(name) || name.Trim() == "OK" ? null : name.Trim();
    }

    /// <summary>
    /// Starts another emulator.
    /// </summary>
    /// <remarks>
    /// A virtual device can only be started once, because the running instance locks it, unless every instance
    /// of it is started read only. That is what lets several testers be compared at once without an AVD each.
    /// </remarks>
    /// <param name="virtualDevice">The virtual device to start.</param>
    /// <param name="port">The port, which also gives the device its serial number.</param>
    /// <param name="readOnly">Whether to start it without taking the lock, which all instances must be.</param>
    /// <returns>The process, so that the caller can keep it alive no longer than itself.</returns>
    public static Process? StartEmulator(string virtualDevice, int port, bool readOnly)
    {
        if (EmulatorPath == null)
        {
            return null;
        }

        var arguments = "-avd " + virtualDevice + " -port " + port + " -no-boot-anim";
        if (readOnly)
        {
            arguments += " -read-only";
        }

        return Process.Start(new ProcessStartInfo(EmulatorPath, arguments)
        {
            UseShellExecute = false,
            CreateNoWindow = false,
        });
    }

    /// <summary>
    /// Finds a port no attached device is using, which emulators take in twos.
    /// </summary>
    /// <returns>The port.</returns>
    public static int GetFreePort()
    {
        var taken = GetDevices()
            .Where(device => device.StartsWith("emulator-", StringComparison.Ordinal))
            .Select(device => int.TryParse(device.Substring(9), out var value) ? value : 0)
            .ToHashSet();
        for (var port = 5554; port < 5680; port += 2)
        {
            if (!taken.Contains(port))
            {
                return port;
            }
        }

        return 5554;
    }

    private static string? Tool(string folder, string fileName)
    {
        foreach (var variable in new[] { "ANDROID_HOME", "ANDROID_SDK_ROOT" })
        {
            var root = Environment.GetEnvironmentVariable(variable);
            if (!string.IsNullOrEmpty(root))
            {
                var path = Path.Combine(root, folder, fileName);
                if (File.Exists(path))
                {
                    return path;
                }
            }
        }

        return null;
    }

    private static string Run(string? fileName, string arguments)
    {
        if (fileName == null)
        {
            return string.Empty;
        }

        try
        {
            using var process = Process.Start(new ProcessStartInfo(fileName, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            if (process == null)
            {
                return string.Empty;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(30000);
            return output;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
