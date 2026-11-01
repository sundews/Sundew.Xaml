// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OverlayHostWindow.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Content;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Windows.Graphics;

/// <summary>
/// A window of our own making, which the subtitle is drawn into as a XAML island.
/// </summary>
/// <remarks>
/// <para>
/// A window is only drawn through where the window manager has given it no opaque surface to be drawn onto, and
/// that is settled once and for all when the window is created, by WS_EX_NOREDIRECTIONBITMAP. WinUI creates its
/// own windows without it and the style cannot be added afterwards, so a transparent overlay cannot be a
/// <see cref="Window"/>; it has to be a window created here, with the style, hosting the content through a
/// <see cref="DesktopWindowXamlSource"/>.
/// </para>
/// <para>
/// The rest of what an overlay is follows from the styles the window is created with as well: it is a popup, so it
/// has no border or title bar; topmost, so it stays above everything; a tool window, so it is in neither the task
/// bar nor the alt tab list; layered and transparent, which together are what let the mouse through, neither being
/// enough on its own; and never activated, so it cannot take the focus from what is underneath.
/// </para>
/// </remarks>
internal sealed class OverlayHostWindow : IDisposable
{
    private const string WindowClassName = "SundewXamlOverlayHostWindow";

#pragma warning disable SA1310
    private const int WS_POPUP = unchecked((int)0x80000000);
    private const int WS_EX_NOREDIRECTIONBITMAP = 0x00200000;
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_LAYERED = 0x00080000;
    private const uint LWA_ALPHA = 0x00000002;
    private const int SW_SHOWNOACTIVATE = 4;
    private const int SW_HIDE = 0;
    private const int MONITOR_DEFAULTTONEAREST = 2;
    private const uint WM_DESTROY = 0x0002;
    private const uint WM_SETTINGCHANGE = 0x001A;
    private const uint WM_DISPLAYCHANGE = 0x007E;
    private const uint WM_DPICHANGED = 0x02E0;
    private const nint SPI_SETWORKAREA = 0x002F;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_NOZORDER = 0x0004;
#pragma warning restore SA1310

    /// <summary>
    /// Kept in a field of its own because the window manager is handed a pointer to it and holds that pointer for
    /// as long as the class is registered, which nothing here tells the collector.
    /// </summary>
    private static readonly WindowProcedure ClassWindowProcedure = OnWindowMessage;

    private static readonly Dictionary<nint, OverlayHostWindow> HostWindows = new();

    private static readonly object Gate = new();

    private static ushort windowClass;

    private readonly DesktopWindowXamlSource island;

    private readonly nint ownerHandle;

    private nint handle;

    /// <summary>
    /// Initializes a new instance of the <see cref="OverlayHostWindow"/> class.
    /// </summary>
    /// <param name="ownerHandle">The window the overlay belongs to, whose display the overlay is laid over. Zero
    /// where there is none, which leaves the overlay on the display it happens to be created on.</param>
    public OverlayHostWindow(nint ownerHandle)
    {
        this.ownerHandle = ownerHandle;
        var instance = GetModuleHandle(null);
        EnsureWindowClass(instance);
        this.handle = CreateWindowEx(
            WS_EX_NOREDIRECTIONBITMAP | WS_EX_TOPMOST | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_LAYERED,
            WindowClassName,
            string.Empty,
            WS_POPUP,
            0,
            0,
            0,
            0,
            0,
            0,
            instance,
            0);

        if (this.handle == 0)
        {
            throw new InvalidOperationException($"The overlay window could not be created: {Marshal.GetLastWin32Error()}.");
        }

        lock (Gate)
        {
            HostWindows[this.handle] = this;
        }

        // A layered window is not shown at all until it is told how opaque it is, and it is wanted wholly opaque:
        // what is see through about the overlay is the content it draws, not the window.
        SetLayeredWindowAttributes(this.handle, 0, 255, LWA_ALPHA);

        this.island = new DesktopWindowXamlSource();
        this.island.Initialize(Win32Interop.GetWindowIdFromWindow(this.handle));

        // Sized here rather than left to follow the parent window. Letting it follow is a step behind the window
        // it follows, which showed as an overlay that now and then came up laid out to the wrong size.
        this.island.SiteBridge.ResizePolicy = ContentSizePolicy.None;
        this.UpdateBounds(true);
    }

    private delegate nint WindowProcedure(nint windowHandle, uint message, nint wParam, nint lParam);

    /// <summary>
    /// Occurs when the work area the window covers has changed, whether because the display changed, the window
    /// moved to another one, or the work area on it was resized.
    /// </summary>
    public event EventHandler? BoundsChanged;

    /// <summary>
    /// Gets the window handle, which is zero once the window has been taken down.
    /// </summary>
    public nint Handle => this.handle;

    /// <summary>
    /// Gets or sets the content the window draws.
    /// </summary>
    public UIElement? Content
    {
        get => this.island.Content;
        set => this.island.Content = value;
    }

    /// <summary>
    /// Gets the width of the work area the window covers, in device independent pixels.
    /// </summary>
    public double Width { get; private set; } = double.PositiveInfinity;

    /// <summary>
    /// Gets the height of the work area the window covers, in device independent pixels.
    /// </summary>
    public double Height { get; private set; } = double.PositiveInfinity;

    /// <summary>
    /// Gets the scale the display reports, which turns the screen pixels the window manager speaks in into the
    /// device independent pixels everything above it does.
    /// </summary>
    public double Scale { get; private set; } = 1;

    /// <summary>
    /// Gets where the window sits, in screen pixels.
    /// </summary>
    public (int X, int Y) Position { get; private set; }

    /// <summary>
    /// Shows the window, without taking the focus from whatever has it.
    /// </summary>
    public void Show()
    {
        if (this.handle == 0)
        {
            return;
        }

        this.island.SiteBridge.Show();
        ShowWindow(this.handle, SW_SHOWNOACTIVATE);

        // Said again now that the window is up: the island only has child windows of its own once it is shown, and
        // a child does not inherit being transparent to the mouse from the window it sits in.
        this.UpdateBounds(true);
    }

    /// <summary>
    /// Hides the window, leaving it able to be shown again.
    /// </summary>
    public void Hide()
    {
        if (this.handle != 0)
        {
            ShowWindow(this.handle, SW_HIDE);
        }
    }

    /// <summary>
    /// Reads the work area of the display the window belongs on and lays the window, and the island in it, over it.
    /// </summary>
    /// <param name="force">Whether to lay it out again even where the work area is the size it already was. Said
    /// when the window has just been created or shown, neither of which has it laid out yet.</param>
    public void UpdateBounds(bool force = false)
    {
        if (this.handle == 0)
        {
            return;
        }

        // The display of the window the overlay belongs to, not of the overlay itself: an overlay is created
        // before it is placed anywhere, so asking which display it is on would answer the primary one however far
        // away the window it is meant to cover happens to be.
        var monitor = MonitorFromWindow(this.ownerHandle != 0 ? this.ownerHandle : this.handle, MONITOR_DEFAULTTONEAREST);
        var monitorInfo = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
        if (!GetMonitorInfo(monitor, ref monitorInfo))
        {
            return;
        }

        var workArea = monitorInfo.rcWork;
        var width = workArea.right - workArea.left;
        var height = workArea.bottom - workArea.top;
        var scale = GetDpiForWindow(this.handle) / 96.0;
        this.Scale = scale > 0 ? scale : 1;
        this.Position = (workArea.left, workArea.top);
        var newWidth = width / this.Scale;
        var newHeight = height / this.Scale;
        if (!force && newWidth.Equals(this.Width) && newHeight.Equals(this.Height))
        {
            return;
        }

        this.Width = newWidth;
        this.Height = newHeight;
        SetWindowPos(this.handle, 0, workArea.left, workArea.top, width, height, SWP_NOACTIVATE | SWP_NOZORDER);
        this.island.SiteBridge.MoveAndResize(new RectInt32 { X = 0, Y = 0, Width = width, Height = height });
        this.BoundsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Takes the window down.
    /// </summary>
    public void Dispose()
    {
        var windowHandle = this.handle;
        this.handle = 0;
        this.island.Dispose();
        if (windowHandle != 0)
        {
            DestroyWindow(windowHandle);
        }
    }

    /// <summary>
    /// Registers the window class the overlay windows are created from, which the window manager keeps for the
    /// life of the process and so is only ever registered once.
    /// </summary>
    /// <param name="instance">The module the class belongs to.</param>
    private static void EnsureWindowClass(nint instance)
    {
        lock (Gate)
        {
            if (windowClass != 0)
            {
                return;
            }

            var newWindowClass = new WNDCLASSEXW
            {
                cbSize = Marshal.SizeOf<WNDCLASSEXW>(),
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(ClassWindowProcedure),
                hInstance = instance,
                lpszClassName = WindowClassName,

                // No background brush, because a window with nothing to paint onto must paint nothing. One here
                // would be the opaque base the whole arrangement exists to be rid of.
                hbrBackground = 0,
            };

            windowClass = RegisterClassEx(ref newWindowClass);
            if (windowClass == 0)
            {
                throw new InvalidOperationException($"The overlay window class could not be registered: {Marshal.GetLastWin32Error()}.");
            }
        }
    }

    /// <summary>
    /// Hears the few messages that say the overlay no longer covers what it should.
    /// </summary>
    private static nint OnWindowMessage(nint windowHandle, uint message, nint wParam, nint lParam)
    {
        OverlayHostWindow? hostWindow;
        lock (Gate)
        {
            HostWindows.TryGetValue(windowHandle, out hostWindow);
        }

        if (hostWindow != null)
        {
            switch (message)
            {
                case WM_DPICHANGED:
                case WM_DISPLAYCHANGE:
                    hostWindow.UpdateBounds(true);
                    break;

                case WM_SETTINGCHANGE:
                    if (wParam == SPI_SETWORKAREA)
                    {
                        hostWindow.UpdateBounds();
                    }

                    break;

                case WM_DESTROY:
                    lock (Gate)
                    {
                        HostWindows.Remove(windowHandle);
                    }

                    break;
            }
        }

        return DefWindowProc(windowHandle, message, wParam, lParam);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern nint GetModuleHandle(string? name);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern ushort RegisterClassEx(ref WNDCLASSEXW windowClass);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "CreateWindowExW")]
    private static extern nint CreateWindowEx(int exStyle, string className, string windowName, int style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);

    [DllImport("user32.dll")]
    private static extern nint DefWindowProc(nint windowHandle, uint message, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(nint windowHandle, int command);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(nint windowHandle);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(nint windowHandle, nint insertAfter, int x, int y, int width, int height, uint flags);

    [DllImport("user32.dll")]
    private static extern nint MonitorFromWindow(nint windowHandle, int flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetMonitorInfo(nint monitor, ref MONITORINFO monitorInfo);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(nint windowHandle);

    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(nint windowHandle, int colorKey, byte alpha, uint flags);

#pragma warning disable SA1307
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASSEXW
    {
        public int cbSize;

        public int style;

        public nint lpfnWndProc;

        public int cbClsExtra;

        public int cbWndExtra;

        public nint hInstance;

        public nint hIcon;

        public nint hCursor;

        public nint hbrBackground;

        public string? lpszMenuName;

        public string? lpszClassName;

        public nint hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;

        public int top;

        public int right;

        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;

        public RECT rcMonitor;

        public RECT rcWork;

        public int dwFlags;
    }
#pragma warning restore SA1307
}
