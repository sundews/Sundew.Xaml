// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OverlayWindow.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

/// <summary>
/// A transparent overlay window.
/// </summary>
public class OverlayWindow : Window
{
#pragma warning disable SA1310
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x80000;
#pragma warning restore SA1310

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(nint hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(nint hwnd, int index, int newStyle);

    private readonly Window? ownerWindow;

    /// <summary>
    /// Initializes a new instance of the <see cref="OverlayWindow"/> class.
    /// </summary>
    public OverlayWindow()
    {
        this.ConfigureWindow();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OverlayWindow"/> class.
    /// </summary>
    /// <param name="ownerWindow">The owner window.</param>
    public OverlayWindow(Window ownerWindow)
    {
        this.ownerWindow = ownerWindow;
        this.ownerWindow.Closed += this.OnOwnerWindowClosed;
        this.ConfigureWindow();
    }

    private void ConfigureWindow()
    {
        this.WindowState = WindowState.Maximized;
        this.WindowStyle = WindowStyle.None;
        this.ResizeMode = ResizeMode.NoResize;

        this.AllowsTransparency = true;

        this.Topmost = true;

        this.ShowInTaskbar = false;

        this.SourceInitialized += this.OnSourceInitialized;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        WindowInteropHelper helper = new WindowInteropHelper(this);
        var hwnd = helper.Handle;
        var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
    }

    private void OnOwnerWindowClosed(object? sender, EventArgs e)
    {
        this.ownerWindow?.Closed -= this.OnOwnerWindowClosed;
        this.Close();
    }
}