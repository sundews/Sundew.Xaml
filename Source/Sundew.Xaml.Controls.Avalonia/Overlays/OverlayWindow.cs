// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OverlayWindow.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Media;
using global::Avalonia.Platform;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

/// <summary>
/// A transparent overlay window.
/// </summary>
/// <remarks>
/// The window covers the work area of whichever monitor it opens on, is always on top, and takes no mouse input at
/// all, so that it can be drawn over another application without getting in its way. Everything about it but the
/// click through is framework level and so holds wherever Avalonia runs; the click through is a window manager
/// matter, said to each in its own words: an extended window style on Windows, ignoresMouseEvents on macOS, and an
/// empty input shape on X11. A Wayland session proper has no way to say it from here, and there the overlay takes
/// the mouse.
/// </remarks>
public class OverlayWindow : Window
{
    /// <summary>The reveal brush property.</summary>
    public static readonly StyledProperty<RadialGradientBrush?> RevealBrushProperty =
        AvaloniaProperty.Register<OverlayWindow, RadialGradientBrush?>(nameof(RevealBrush));

    /// <summary>The monitor width property.</summary>
    public static readonly DirectProperty<OverlayWindow, double> MonitorWidthProperty =
        AvaloniaProperty.RegisterDirect<OverlayWindow, double>(nameof(MonitorWidth), overlayWindow => overlayWindow.MonitorWidth);

    /// <summary>The monitor height property.</summary>
    public static readonly DirectProperty<OverlayWindow, double> MonitorHeightProperty =
        AvaloniaProperty.RegisterDirect<OverlayWindow, double>(nameof(MonitorHeight), overlayWindow => overlayWindow.MonitorHeight);

    private readonly Window? ownerWindow;

    /// <summary>
    /// How often the cursor is asked after where a window manager will not say. Often enough that the reveal
    /// keeps up with the pointer, and only while there is a reveal to move.
    /// </summary>
    private static readonly TimeSpan CursorPollingInterval = TimeSpan.FromMilliseconds(16);

    private double monitorWidth = double.PositiveInfinity;
    private double monitorHeight = double.PositiveInfinity;

    /// <summary>
    /// The cursor hook, held as a plain disposable so that this cross platform type carries no Windows only member.
    /// </summary>
    private IDisposable? mouseHook;
    private DispatcherTimer? cursorTimer;
    private Visual? trackedRevealTarget;
    private Screen? currentScreen;
    private Point? trackedCursorPosition;
    private PixelPoint latestCursorPosition;
    private bool isCursorUpdatePending;
    private bool isClickThrough;
    private nint windowInFrontBeforeOpening;

    static OverlayWindow()
    {
        // Overridden as the default rather than set on the window, so that a theme or a caller naming a background
        // still wins. A templated overlay paints the background wherever its template says, which for the subtitle
        // overlay is the pill behind the text and not the window.
        BackgroundProperty.OverrideDefaultValue<OverlayWindow>(Brushes.Transparent);
    }

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

    /// <summary>
    /// Gets the monitor width.
    /// </summary>
    public double MonitorWidth
    {
        get => this.monitorWidth;
        private set => this.SetAndRaise(MonitorWidthProperty, ref this.monitorWidth, value);
    }

    /// <summary>
    /// Gets the monitor height.
    /// </summary>
    public double MonitorHeight
    {
        get => this.monitorHeight;
        private set => this.SetAndRaise(MonitorHeightProperty, ref this.monitorHeight, value);
    }

    /// <summary>
    /// Gets or sets the reveal brush.
    /// </summary>
    /// <remarks>
    /// The brush is used as an opacity mask on the reveal target and is re-centered on the cursor as it moves, so
    /// that a gradient reaching black at its edge leaves a hole in the overlay around the cursor. Unlike WPF,
    /// Avalonia lets a radial gradient state its radius in either device independent pixels or as a fraction of the
    /// target, so whichever the brush declares is used as it stands.
    /// </remarks>
    public RadialGradientBrush? RevealBrush
    {
        get => this.GetValue(RevealBrushProperty);
        set => this.SetValue(RevealBrushProperty, value);
    }

    /// <summary>
    /// Gets the target for the reveal effect.
    /// </summary>
    /// <returns>The target for the reveal effect.</returns>
    protected virtual Visual? GetRevealTarget()
    {
        return this.RevealBrush == null ? null : this;
    }

    /// <summary>
    /// Called when the template has been applied.
    /// </summary>
    /// <param name="e">The event args.</param>
    protected override void OnApplyTemplate(global::Avalonia.Controls.Primitives.TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        this.UpdateRevealing();
    }

    /// <summary>
    /// Called when a property has changed.
    /// </summary>
    /// <param name="change">The change.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RevealBrushProperty)
        {
            this.UpdateRevealing();
        }
    }

    /// <summary>
    /// Reads a radius the way the brush states it, which is either device independent pixels or a fraction of
    /// what it is painting.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <param name="extent">What it is painting, across the axis in question.</param>
    /// <returns>The radius in device independent pixels.</returns>
    private static double Resolve(RelativeScalar radius, double extent)
    {
        return radius.Unit == RelativeUnit.Absolute ? radius.Scalar : radius.Scalar * extent;
    }

    private static RadialGradientBrush CloneBrush(RadialGradientBrush revealBrush)
    {
        var clone = new RadialGradientBrush
        {
            Center = revealBrush.Center,
            GradientOrigin = revealBrush.GradientOrigin,
            RadiusX = revealBrush.RadiusX,
            RadiusY = revealBrush.RadiusY,
            SpreadMethod = revealBrush.SpreadMethod,
            Opacity = revealBrush.Opacity,
            Transform = revealBrush.Transform,
            TransformOrigin = revealBrush.TransformOrigin,
        };

        foreach (var gradientStop in revealBrush.GradientStops)
        {
            clone.GradientStops.Add(new GradientStop(gradientStop.Color, gradientStop.Offset));
        }

        return clone;
    }

    private void ConfigureWindow()
    {
        this.WindowDecorations = WindowDecorations.None;
        this.TransparencyLevelHint = new WindowTransparencyLevelCollection(new[] { WindowTransparencyLevel.Transparent });
        this.Topmost = true;
        this.ShowInTaskbar = false;

        // Shown without being activated, so that whatever was being used stays in front and keeps the keyboard.
        // An overlay that takes the focus is one that Alt+F4 closes, which is not what the key was pressed for.
        // Asking is not enough on its own, so what was in front is remembered here and put back once the window
        // exists and can be told not to be activated again.
        this.ShowActivated = false;
        if (OperatingSystem.IsWindows())
        {
            this.windowInFrontBeforeOpening = WindowsOverlayInterop.GetWindowInFront();
        }

        this.CanResize = false;
        this.WindowState = WindowState.Maximized;

        this.Opened += this.OnOpened;
        this.PositionChanged += this.OnPositionChanged;
        this.Closed += this.OnClosed;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        this.isClickThrough = this.TryMakeClickThrough();
        this.UpdateMonitorBounds();
        this.UpdateRevealing();
    }

    /// <summary>
    /// Takes the window out of the way of the mouse, in whichever way the window manager underneath understands.
    /// </summary>
    /// <returns><c>true</c>, if it could be said, otherwise <c>false</c>, which leaves the overlay taking the
    /// mouse and so hearing about the cursor the ordinary way.</returns>
    private bool TryMakeClickThrough()
    {
        if (this.TryGetPlatformHandle() is not { } platformHandle || platformHandle.Handle == 0)
        {
            return false;
        }

        if (OperatingSystem.IsWindows())
        {
            WindowsOverlayInterop.MakeClickThrough(platformHandle.Handle);
            WindowsOverlayInterop.RestoreWindowInFront(this.windowInFrontBeforeOpening, platformHandle.Handle);
            return true;
        }

        // The descriptor is asked about rather than the operating system alone, because it says which windowing
        // system is actually underneath. A Wayland session answers neither of these, and is left alone.
        if (OperatingSystem.IsMacOS() && platformHandle.HandleDescriptor is "NSView" or "NSWindow")
        {
            return MacOverlayInterop.TryMakeClickThrough(platformHandle.Handle);
        }

        if (OperatingSystem.IsLinux() && platformHandle.HandleDescriptor == "XID")
        {
            return X11OverlayInterop.TryMakeClickThrough(platformHandle.Handle);
        }

        return false;
    }

    /// <summary>
    /// Gets where the cursor is, in screen pixels, by asking whoever knows.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns><c>true</c>, if the position could be read, otherwise <c>false</c>.</returns>
    private static bool TryGetCursorPosition(out int x, out int y)
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowsOverlayInterop.TryGetCursorPosition(out x, out y);
        }

        if (OperatingSystem.IsMacOS())
        {
            return MacOverlayInterop.TryGetCursorPosition(out x, out y);
        }

        if (OperatingSystem.IsLinux())
        {
            return X11OverlayInterop.TryGetCursorPosition(out x, out y);
        }

        x = 0;
        y = 0;
        return false;
    }

    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        var screen = this.Screens.ScreenFromWindow(this);
        if (screen != null && !Equals(screen, this.currentScreen))
        {
            this.currentScreen = screen;
            this.UpdateMonitorBounds();
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        this.StopTracking();
        this.TrackRevealTarget(null);
    }

    private void OnOwnerWindowClosed(object? sender, EventArgs e)
    {
        if (this.ownerWindow != null)
        {
            this.ownerWindow.Closed -= this.OnOwnerWindowClosed;
        }

        this.Close();
    }

    /// <summary>
    /// Reports the work area of the monitor the window is on, in device independent pixels, which is what a template
    /// constrains the overlay content by.
    /// </summary>
    private void UpdateMonitorBounds()
    {
        var screen = this.Screens.ScreenFromWindow(this) ?? this.Screens.Primary;
        if (screen == null)
        {
            return;
        }

        this.currentScreen = screen;
        var workingArea = screen.WorkingArea;
        this.MonitorWidth = workingArea.Width / screen.Scaling;
        this.MonitorHeight = workingArea.Height / screen.Scaling;
    }

    private void UpdateRevealing()
    {
        if (this.RevealBrush == null)
        {
            this.StopTracking();
            this.TrackRevealTarget(null);
            return;
        }

        var revealTarget = this.GetRevealTarget();
        if (revealTarget == null)
        {
            return;
        }

        this.TrackRevealTarget(revealTarget);
        this.StartTracking();
    }

    /// <summary>
    /// Follows the reveal target's size, because the target is centered and so the cursor sits somewhere else within
    /// it once it has resized, even when the cursor itself has not moved.
    /// </summary>
    private void TrackRevealTarget(Visual? revealTarget)
    {
        if (ReferenceEquals(this.trackedRevealTarget, revealTarget))
        {
            return;
        }

        if (this.trackedRevealTarget != null)
        {
            this.trackedRevealTarget.PropertyChanged -= this.OnRevealTargetPropertyChanged;
            this.trackedRevealTarget.OpacityMask = null;
        }

        this.trackedRevealTarget = revealTarget;
        if (this.trackedRevealTarget != null)
        {
            this.trackedRevealTarget.PropertyChanged += this.OnRevealTargetPropertyChanged;
        }
    }

    private void OnRevealTargetPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != BoundsProperty || sender is not Visual revealTarget)
        {
            return;
        }

        if (this.isClickThrough && TryGetCursorPosition(out var x, out var y))
        {
            this.UpdateRevealMask(revealTarget, this.PointToClient(new PixelPoint(x, y)));
            return;
        }

        if (this.trackedCursorPosition.HasValue)
        {
            this.UpdateRevealMask(revealTarget, this.trackedCursorPosition.Value);
        }
    }

    /// <summary>
    /// Starts following the cursor, which a click through window has to be told about rather than hear itself.
    /// </summary>
    /// <remarks>
    /// Windows will say so as it happens, through a low level hook. The others are asked where the cursor is
    /// instead, often enough to follow it: neither a macOS window that ignores the mouse nor an X window with no
    /// input shape is sent anything to listen for. Where the window could not be made click through at all it
    /// gets the moves itself, like any other window.
    /// </remarks>
    private void StartTracking()
    {
        if (!this.isClickThrough)
        {
            this.PointerMoved -= this.OnPointerMovedOverOverlay;
            this.PointerMoved += this.OnPointerMovedOverOverlay;
            return;
        }

        // Dropped in case tracking had started before the window opened, when it was not yet known that the
        // window would be click through at all.
        this.PointerMoved -= this.OnPointerMovedOverOverlay;
        if (OperatingSystem.IsWindows())
        {
            this.mouseHook ??= new WindowsMouseHook(this.OnHookedMouseMove);
            return;
        }

        if (this.cursorTimer == null)
        {
            this.cursorTimer = new DispatcherTimer(DispatcherPriority.Input) { Interval = CursorPollingInterval };
            this.cursorTimer.Tick += this.OnCursorPolled;
        }

        this.cursorTimer.Start();
    }

    private void StopTracking()
    {
        this.mouseHook?.Dispose();
        this.mouseHook = null;
        if (this.cursorTimer != null)
        {
            this.cursorTimer.Stop();
            this.cursorTimer.Tick -= this.OnCursorPolled;
            this.cursorTimer = null;
        }

        this.PointerMoved -= this.OnPointerMovedOverOverlay;
        this.trackedCursorPosition = null;
    }

    /// <summary>
    /// Reads where the cursor is and carries it on as though it had been reported.
    /// </summary>
    private void OnCursorPolled(object? sender, EventArgs e)
    {
        if (TryGetCursorPosition(out var x, out var y) && new PixelPoint(x, y) != this.latestCursorPosition)
        {
            this.latestCursorPosition = new PixelPoint(x, y);
            this.OnCursorMoved(this.PointToClient(this.latestCursorPosition));
        }
    }

    /// <summary>
    /// Called on whichever thread the move arrived on, which is not the one the overlay may be touched from.
    /// </summary>
    /// <remarks>
    /// Only the latest position is kept, and only one of these is ever waiting at a time. A mouse reports itself
    /// far more often than the screen is drawn, and moving the hole for a position already overtaken by the next
    /// one is work whose result is never seen.
    /// </remarks>
    private void OnHookedMouseMove(int x, int y)
    {
        this.latestCursorPosition = new PixelPoint(x, y);
        if (this.isCursorUpdatePending)
        {
            return;
        }

        this.isCursorUpdatePending = true;
        Dispatcher.UIThread.Post(
            () =>
            {
                this.isCursorUpdatePending = false;
                this.OnCursorMoved(this.PointToClient(this.latestCursorPosition));
            },
            DispatcherPriority.Input);
    }

    private void OnPointerMovedOverOverlay(object? sender, PointerEventArgs e)
    {
        this.OnCursorMoved(e.GetPosition(this));
    }

    private void OnCursorMoved(Point position)
    {
        var revealTarget = this.GetRevealTarget();
        if (revealTarget == null)
        {
            return;
        }

        this.trackedCursorPosition = position;

        // The mask stays on and only its middle moves. Asking what is under the cursor and taking the mask away
        // when the answer is nothing cannot work here: the hole the mask makes is itself nothing to hit, so the
        // answer flips between the subtitle and nothing as the hole follows the cursor, and the hole flickers in
        // and out. It does not need taking away in any case. Beyond its last stop the gradient is opaque, so a
        // cursor away from the subtitle leaves the subtitle as it was.
        var positionInTarget = this.TranslatePoint(position, revealTarget);
        if (positionInTarget.HasValue)
        {
            this.UpdateRevealMask(revealTarget, positionInTarget.Value);
        }
    }

    private void UpdateRevealMask(Visual revealTarget, Point position)
    {
        var revealBrush = this.RevealBrush;
        if (revealBrush == null)
        {
            return;
        }

        // Nothing is drawn differently where the hole cannot reach. Beyond its outermost stop the gradient is
        // opaque, so a cursor further from the subtitle than the hole is wide leaves it looking exactly as it
        // would with no mask at all; the mask is dropped there and the subtitle is left alone until the cursor
        // comes back. This is worked out from where things are rather than by asking what is under the cursor:
        // the hole is itself nothing to hit, so asking flips between the subtitle and nothing as the hole
        // follows the cursor, which is what made it flicker.
        var reach = new Rect(revealTarget.Bounds.Size).Inflate(
            new Thickness(Resolve(revealBrush.RadiusX, revealTarget.Bounds.Width), Resolve(revealBrush.RadiusY, revealTarget.Bounds.Height)));
        if (!reach.Contains(position))
        {
            if (revealTarget.OpacityMask != null)
            {
                revealTarget.OpacityMask = null;
            }

            return;
        }

        // A copy each time, rather than moving the middle of the one already there. A brush handed in may well be
        // a shared resource, so it is never the one that is moved; and changing where an opacity mask is centred
        // does not on its own tell the element using it to draw again, which left the hole behind where it was
        // first put. Handing over a different brush does tell it.
        var centre = new RelativePoint(position, RelativeUnit.Absolute);
        if (revealTarget.OpacityMask is RadialGradientBrush current && current.Center.Point.NearlyEquals(position))
        {
            return;
        }

        var mask = CloneBrush(revealBrush);
        mask.Center = centre;
        mask.GradientOrigin = centre;
        revealTarget.OpacityMask = mask;
    }
}
