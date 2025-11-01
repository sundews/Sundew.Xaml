// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OverlayWindow.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using static Sundew.Xaml.Controls.Overlays.MonitorInfo;

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

    private IntPtr hookId = IntPtr.Zero;
    private SubtitleOverlay.LowLevelMouseProc? mouseProcedure;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(nint hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(nint hwnd, int index, int newStyle);

    private IntPtr currentMonitor = IntPtr.Zero;
    private readonly Window? ownerWindow;

    private static readonly DependencyPropertyKey MonitorWidthPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(MonitorWidth), typeof(double), typeof(OverlayWindow), new PropertyMetadata(default(double)));

    private static readonly DependencyPropertyKey MonitorHeightPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(MonitorHeight), typeof(double), typeof(OverlayWindow), new PropertyMetadata(default(double)));

    /// <summary>The monitor width property.</summary>
    public static readonly DependencyProperty MonitorWidthProperty = MonitorWidthPropertyKey.DependencyProperty;

    /// <summary>The monitor height property.</summary>
    public static readonly DependencyProperty MonitorHeightProperty = MonitorHeightPropertyKey.DependencyProperty;

    /// <summary>The reveal brush property.</summary>
    public static readonly DependencyProperty RevealBrushProperty = DependencyProperty.Register(
        nameof(RevealBrush),
        typeof(RadialGradientBrush),
        typeof(OverlayWindow),
        new FrameworkPropertyMetadata(
            default,
            FrameworkPropertyMetadataOptions.AffectsRender,
            (o, args) =>
            {
                if (o is SubtitleOverlay subtitleOverlay)
                {
                    subtitleOverlay.UpdateRevealing();
                }
            }));

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
        get { return (double)this.GetValue(MonitorWidthProperty); }
    }

    /// <summary>
    /// Gets the monitor height.
    /// </summary>
    public double MonitorHeight
    {
        get { return (double)this.GetValue(MonitorHeightProperty); }
    }

    /// <summary>
    /// Gets or sets the reveal brush.
    /// </summary>
    public RadialGradientBrush? RevealBrush
    {
        get { return (RadialGradientBrush?)this.GetValue(RevealBrushProperty); }
        set { this.SetValue(RevealBrushProperty, value); }
    }

    /// <summary>
    /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        this.UpdateRevealing();
    }

    /// <summary>
    /// Gets the target for the reveal effect.
    /// </summary>
    /// <returns>The target for the reveal effect.</returns>
    protected virtual FrameworkElement? GetRevealTarget()
    {
        return this.RevealBrush == null ? null : this;
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
        this.LocationChanged += this.OnLocationChanged;
        this.Closed += (s, e) => UnhookWindowsHookEx(this.hookId);
    }

    private void UpdateRevealing()
    {
        if (this.RevealBrush == null)
        {
            UnhookWindowsHookEx(this.hookId);
            this.hookId = IntPtr.Zero;
            return;
        }

        var targetFrameworkElement = this.GetRevealTarget();
        if (targetFrameworkElement is null)
        {
            return;
        }

        targetFrameworkElement.SizeChanged -= this.OnTargetSizeChanged;
        targetFrameworkElement.SizeChanged += this.OnTargetSizeChanged;

        if (this.hookId == IntPtr.Zero)
        {
            this.mouseProcedure = this.OnMouseMoveProcedure;
            this.hookId = this.SetHook(this.mouseProcedure);
        }
    }

    private void OnTargetSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not FrameworkElement targetFrameworkElement)
        {
            return;
        }

        GetCursorPos(out var point);
        var position = targetFrameworkElement.PointFromScreen(new Point(point.x, point.y));
        this.UpdateRevealMask(targetFrameworkElement, position);
    }

    private void UpdateMonitorBounds(IntPtr windowHandle)
    {
        var source = PresentationSource.FromVisual(this);
        if (source?.CompositionTarget != null)
        {
            var scaleX = source.CompositionTarget.TransformFromDevice.M11;
            var scaleY = source.CompositionTarget.TransformFromDevice.M22;

            var monitor = MonitorInfo.MonitorFromWindow(windowHandle, MonitorInfo.MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new MONITORINFO();
                MonitorInfo.GetMonitorInfo(monitor, monitorInfo);

                var workArea = monitorInfo.rcWork;
                this.SetValue(MonitorWidthPropertyKey, scaleX * (workArea.Right - workArea.Left));
                this.SetValue(MonitorHeightPropertyKey, scaleY * (workArea.Bottom - workArea.Top));
            }
        }
    }

    private void OnLocationChanged(object? sender, EventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        var monitor = MonitorFromWindow(helper.Handle, MONITOR_DEFAULTTONEAREST);

        if (this.currentMonitor != IntPtr.Zero && this.currentMonitor != monitor)
        {
            this.UpdateMonitorBounds(helper.Handle);
        }

        this.currentMonitor = monitor;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        var hwnd = helper.Handle;
        var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
        this.UpdateMonitorBounds(helper.Handle);
    }

    private void OnOwnerWindowClosed(object? sender, EventArgs e)
    {
        this.ownerWindow?.Closed -= this.OnOwnerWindowClosed;
        this.Close();
    }

    private IntPtr SetHook(LowLevelMouseProc lowLevelMouseProc)
    {
        using (var currentProcess = Process.GetCurrentProcess())
        using (var currentProcessMainModule = currentProcess.MainModule)
        {
            if (currentProcessMainModule != null)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, lowLevelMouseProc, GetModuleHandle(currentProcessMainModule.ModuleName!), 0);
            }
        }

        return IntPtr.Zero;
    }

    private IntPtr OnMouseMoveProcedure(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEMOVE)
        {
            var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

            // Check if mouse is over your window
            var screenPoint = new Point(hookStruct.pt.x, hookStruct.pt.y);
            var windowPoint = this.PointFromScreen(screenPoint);

            if (windowPoint.X >= 0 && windowPoint.X <= this.ActualWidth &&
                windowPoint.Y >= 0 && windowPoint.Y <= this.ActualHeight)
            {
                this.Dispatcher.Invoke(() =>
                {
                    // Handle mouse move event
                    this.OnCustomMouseMove(windowPoint);
                });
            }
        }

        return CallNextHookEx(this.hookId, nCode, wParam, lParam);
    }

    private void OnCustomMouseMove(Point position)
    {
        var targetFrameworkElement = this.GetRevealTarget();
        if (targetFrameworkElement is null)
        {
            return;
        }

        var hitTestResult = VisualTreeHelper.HitTest(this, position);
        if (hitTestResult == null)
        {
            targetFrameworkElement.OpacityMask = null;
            return;
        }

        this.UpdateRevealMask(targetFrameworkElement, targetFrameworkElement.PointFromScreen(this.PointToScreen(position)));
    }

    private void UpdateRevealMask(FrameworkElement target, Point position)
    {
        if (this.RevealBrush == null)
        {
            return;
        }

        var brush = (RadialGradientBrush)target.OpacityMask;
        if (brush == null)
        {
            target.OpacityMask = brush = this.RevealBrush.IsFrozen ? this.RevealBrush.Clone() : this.RevealBrush;
        }

        var x = position.X / target.ActualWidth;
        var y = position.Y / target.ActualHeight;

        var radiusX = this.RevealBrush.RadiusX / target.ActualWidth;
        var radiusY = this.RevealBrush.RadiusY / target.ActualHeight;

        brush.RadiusX = radiusX;
        brush.RadiusY = radiusY;
        brush.Center = new Point(x, y);
        brush.GradientOrigin = new Point(x, y);
    }

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

#pragma warning disable SA1310
    private const int WH_MOUSE_LL = 14;
    private const int WM_MOUSEMOVE = 0x0200;
#pragma warning restore SA1310

#pragma warning disable SA1307
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
#pragma warning restore SA1307
}