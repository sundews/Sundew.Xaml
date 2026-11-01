// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MonitorInfo.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using System.Runtime.InteropServices;

internal class MonitorInfo
{
    /// <summary>
    /// Find nearest monitor.
    /// </summary>
#pragma warning disable SA1310
    public const uint MONITOR_DEFAULTTONEAREST = 2;
#pragma warning restore SA1310

    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

    [StructLayout(LayoutKind.Sequential)]
    public class MONITORINFO
    {
#pragma warning disable SA1307
#pragma warning disable SA1401
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
#pragma warning restore SA1307
#pragma warning restore SA1401
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}