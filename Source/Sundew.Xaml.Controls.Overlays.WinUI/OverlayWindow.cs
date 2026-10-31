// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OverlayWindow.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Windows.Foundation;

/// <summary>
/// A transparent overlay window.
/// </summary>
/// <remarks>
/// <para>
/// The window has no border and no title bar, covers the work area of whichever display it is on, and stays above
/// every other window. It is also kept out of the task bar and the alt tab list, is transparent per pixel, and
/// takes no mouse input and no focus at all, so that whatever is underneath it stays usable.
/// </para>
/// <para>
/// Unlike the WPF and Avalonia overlays this is not a window of the framework's. A <see cref="Window"/> is given an
/// opaque surface to be drawn onto when the window manager creates it, and nothing said afterwards takes that away,
/// so a transparent overlay cannot be one; it is a window created here instead, drawn into as a XAML island. See
/// <see cref="OverlayHostWindow"/> for what that means and why.
/// </para>
/// <para>
/// Being no window of the framework's, it also does not keep the application running the way a
/// <see cref="Window"/> does. An overlay is something shown over the rest of an application, so there is expected
/// to be a window of that application keeping it alive.
/// </para>
/// </remarks>
public class OverlayWindow : DependencyObject
{
    private readonly OverlayHostWindow hostWindow;

    private readonly DispatcherQueue dispatcherQueue;

    private readonly Window? ownerWindow;

    private WindowsMouseHook? mouseHook;

    private bool isClosed;

    /// <summary>
    /// Initializes a new instance of the <see cref="OverlayWindow"/> class.
    /// </summary>
    public OverlayWindow()
    {
        this.dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        this.hostWindow = new OverlayHostWindow(0);
        this.hostWindow.BoundsChanged += this.OnHostWindowBoundsChanged;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OverlayWindow"/> class.
    /// </summary>
    /// <param name="ownerWindow">The owner window, whose display the overlay covers and which it closes with.</param>
    public OverlayWindow(Window ownerWindow)
    {
        this.dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        this.ownerWindow = ownerWindow;
        this.hostWindow = new OverlayHostWindow(GetWindowHandle(ownerWindow));
        this.hostWindow.BoundsChanged += this.OnHostWindowBoundsChanged;
        this.ownerWindow.Closed += this.OnOwnerWindowClosed;
    }

    /// <summary>
    /// Occurs when the overlay has closed.
    /// </summary>
    public event EventHandler? Closed;

    /// <summary>
    /// Gets the handle of the window the overlay is drawn in, for whatever has to be said to the window manager
    /// directly. It is zero once the overlay has closed.
    /// </summary>
    public nint WindowHandle => this.hostWindow.Handle;

    /// <summary>
    /// Gets or sets the content the overlay draws.
    /// </summary>
    public UIElement? Content
    {
        get => this.hostWindow.Content;
        set => this.hostWindow.Content = value;
    }

    /// <summary>
    /// Gets the monitor width, in device independent pixels.
    /// </summary>
    public double MonitorWidth => this.hostWindow.Width;

    /// <summary>
    /// Gets the monitor height, in device independent pixels.
    /// </summary>
    public double MonitorHeight => this.hostWindow.Height;

    /// <summary>
    /// Gets a value indicating whether the cursor is being followed.
    /// </summary>
    protected bool IsCursorTracking => this.mouseHook != null;

    /// <summary>
    /// Gets the scale the display reports, which turns the screen pixels the window manager speaks in into the
    /// device independent pixels everything above it does.
    /// </summary>
    protected double RasterizationScale => this.hostWindow.Scale;

    /// <summary>
    /// Shows the overlay.
    /// </summary>
    /// <remarks>
    /// Named as the framework names it, although nothing is activated: an overlay never takes the focus from what
    /// is underneath it.
    /// </remarks>
    public void Activate()
    {
        if (!this.isClosed)
        {
            this.hostWindow.Show();
        }
    }

    /// <summary>
    /// Hides the overlay, leaving it able to be shown again.
    /// </summary>
    public void Hide()
    {
        if (!this.isClosed)
        {
            this.hostWindow.Hide();
        }
    }

    /// <summary>
    /// Closes the overlay for good.
    /// </summary>
    public void Close()
    {
        if (this.isClosed)
        {
            return;
        }

        this.isClosed = true;
        this.StopCursorTracking();
        this.hostWindow.BoundsChanged -= this.OnHostWindowBoundsChanged;
        if (this.ownerWindow != null)
        {
            this.ownerWindow.Closed -= this.OnOwnerWindowClosed;
        }

        this.hostWindow.Dispose();
        this.Closed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the work area the overlay covers has changed.
    /// </summary>
    protected virtual void OnMonitorBoundsChanged()
    {
    }

    /// <summary>
    /// Called with the cursor position in the overlay's own coordinates, while cursor tracking is on.
    /// </summary>
    /// <param name="position">The position.</param>
    protected virtual void OnCursorMoved(Point position)
    {
    }

    /// <summary>
    /// Starts following the cursor.
    /// </summary>
    /// <remarks>
    /// The overlay is given no mouse messages, which is the whole point of it, so the cursor is heard about through
    /// a low level hook instead.
    /// </remarks>
    protected void StartCursorTracking()
    {
        if (this.mouseHook != null || this.isClosed)
        {
            return;
        }

        this.mouseHook = new WindowsMouseHook(this.OnHookedMouseMove);
    }

    /// <summary>
    /// Stops following the cursor.
    /// </summary>
    protected void StopCursorTracking()
    {
        this.mouseHook?.Dispose();
        this.mouseHook = null;
    }

    /// <summary>
    /// Reads the work area the overlay covers again, which is what the content is allowed to grow to.
    /// </summary>
    protected void UpdateMonitorBounds()
    {
        if (!this.isClosed)
        {
            this.hostWindow.UpdateBounds();
        }
    }

    private static nint GetWindowHandle(Window? window)
    {
        return window?.AppWindow != null ? Microsoft.UI.Win32Interop.GetWindowFromWindowId(window.AppWindow.Id) : 0;
    }

    private void OnHostWindowBoundsChanged(object? sender, EventArgs e)
    {
        if (!this.isClosed)
        {
            this.OnMonitorBoundsChanged();
        }
    }

    private void OnHookedMouseMove(int x, int y)
    {
        this.dispatcherQueue.TryEnqueue(() =>
        {
            if (this.isClosed)
            {
                return;
            }

            var scale = this.RasterizationScale;
            var origin = this.hostWindow.Position;
            this.OnCursorMoved(new Point((x - origin.X) / scale, (y - origin.Y) / scale));
        });
    }

    private void OnOwnerWindowClosed(object sender, WindowEventArgs args)
    {
        this.Close();
    }
}
