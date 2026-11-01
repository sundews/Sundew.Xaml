// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FullWindow.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

/// <summary>
/// A window with no non client area, so that the caption, and the minimize, maximize and close buttons in it, are
/// drawn as content like anything else.
/// </summary>
/// <remarks>
/// <para>
/// The non client area is taken away by a <see cref="WindowChrome"/> rather than by hiding it, so that the window is
/// still a window to the window manager: it resizes from its edges, snaps, maximizes to the work area and animates
/// as any other does. What is gone is only what was drawn there. The client area is the whole window, and the
/// content drawn into it reaches the very edge.
/// </para>
/// <para>
/// <see cref="CaptionHeight"/> hands a strip at the top of the content back to the window manager as the caption, so
/// that dragging it moves the window, double clicking it maximizes or restores it, and right clicking it opens the
/// system menu, none of which has to be wired up. A button inside that strip has to say
/// <c>WindowChrome.IsHitTestVisibleInChrome="True"</c>, or the caption takes the click before the button hears about
/// it. Left at zero there is no caption at all, and <see cref="Window.DragMove"/> is how a caption of one's own
/// moves the window.
/// </para>
/// <para>
/// <see cref="MinimizeCommand"/> and the rest are what the buttons of such a caption are bound to. Each knows when
/// it cannot be used, so a button bound to one is disabled rather than doing nothing: a window that cannot resize
/// cannot be maximized, and a maximized window cannot be maximized again.
/// </para>
/// <para>
/// <see cref="Backdrop"/> asks the window manager to draw something of its own behind the content, Mica say, which a
/// window with none of the system's chrome left over it is the very place for. Asking hands the window's background
/// over: it is made transparent while a backdrop is in effect and given back to the theme where none could be had,
/// so that a system without backdrops shows an ordinary window rather than a hole. <see cref="ActualBackdrop"/> is
/// what came of the asking. Whether the backdrop is tinted light or dark follows the theme mode, which is WPF's to
/// tell the window manager about and not this window's.
/// </para>
/// <para>
/// The window manager draws a backdrop only while the window is the one in front, and puts a flat colour in its
/// place while it is not. Nothing here can say otherwise: that is settled by the backdrop being the window
/// manager's, and the only way round it is to run the backdrop controller oneself, which is a thing WinUI 3 windows
/// can do and this one cannot.
/// </para>
/// </remarks>
public class FullWindow : Window
{
#pragma warning disable SA1310
    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
    private const int DWMSBT_NONE = 1;
    private const int DWMSBT_MAINWINDOW = 2;
    private const int DWMSBT_TRANSIENTWINDOW = 3;
    private const int DWMSBT_TABBEDWINDOW = 4;
#pragma warning restore SA1310

    /// <summary>
    /// Identifies the <see cref="CaptionHeight"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CaptionHeightProperty = DependencyProperty.Register(
        nameof(CaptionHeight), typeof(double), typeof(FullWindow), new FrameworkPropertyMetadata(0d, OnChromeChanged));

    /// <summary>
    /// Identifies the <see cref="ResizeBorderThickness"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ResizeBorderThicknessProperty = DependencyProperty.Register(
        nameof(ResizeBorderThickness), typeof(Thickness), typeof(FullWindow), new FrameworkPropertyMetadata(SystemParameters.WindowResizeBorderThickness, OnChromeChanged));

    /// <summary>
    /// Identifies the <see cref="Backdrop"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BackdropProperty = DependencyProperty.Register(
        nameof(Backdrop), typeof(WindowBackdrop), typeof(FullWindow), new FrameworkPropertyMetadata(WindowBackdrop.Default, OnBackdropChanged));

    private static readonly DependencyPropertyKey IsMaximizedPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsMaximized), typeof(bool), typeof(FullWindow), new PropertyMetadata(false));

    /// <summary>
    /// Identifies the <see cref="IsMaximized"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsMaximizedProperty = IsMaximizedPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey ActualBackdropPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(ActualBackdrop), typeof(WindowBackdrop), typeof(FullWindow), new PropertyMetadata(WindowBackdrop.None));

    /// <summary>
    /// Identifies the <see cref="ActualBackdrop"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ActualBackdropProperty = ActualBackdropPropertyKey.DependencyProperty;

    private readonly WindowCommand minimizeCommand;
    private readonly WindowCommand maximizeCommand;
    private readonly WindowCommand restoreCommand;
    private readonly WindowCommand toggleMaximizedCommand;
    private readonly WindowCommand closeCommand;

    private bool isBackgroundTransparent;
    private bool isGlassFrameExtended;

    /// <summary>
    /// Initializes a new instance of the <see cref="FullWindow"/> class.
    /// </summary>
    public FullWindow()
    {
        this.minimizeCommand = new WindowCommand(this.Minimize, () => this.CanMinimize);
        this.maximizeCommand = new WindowCommand(this.Maximize, () => this.CanMaximize && this.WindowState != WindowState.Maximized);
        this.restoreCommand = new WindowCommand(this.Restore, () => this.WindowState != WindowState.Normal);
        this.toggleMaximizedCommand = new WindowCommand(this.ToggleMaximized, () => this.CanMaximize);
        this.closeCommand = new WindowCommand(this.Close, () => true);

        // The styles the window manager draws the non client area from are what is dropped; the chrome stated below
        // then says what is left in their place.
        this.WindowStyle = WindowStyle.None;
        this.UpdateWindowChrome();
        this.StateChanged += this.OnStateChanged;

        // The backdrop is asked for on the way up rather than as soon as there is a handle: the window manager has
        // nothing to say about a window it has not shown yet, and turns the asking down.
        this.Loaded += this.OnLoaded;
    }

    /// <summary>
    /// Gets or sets the height of the strip at the top of the content that the window manager treats as the caption.
    /// Zero, the default, leaves the whole window ordinary content.
    /// </summary>
    public double CaptionHeight
    {
        get => (double)this.GetValue(CaptionHeightProperty);
        set => this.SetValue(CaptionHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets how far into the window the edges it is resized from reach. It defaults to what the system uses
    /// for an ordinary window.
    /// </summary>
    public Thickness ResizeBorderThickness
    {
        get => (Thickness)this.GetValue(ResizeBorderThicknessProperty);
        set => this.SetValue(ResizeBorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the backdrop asked of the window manager. It is asked for when the window is first shown and
    /// again whenever this changes, and <see cref="ActualBackdrop"/> says what came of it.
    /// </summary>
    public WindowBackdrop Backdrop
    {
        get => (WindowBackdrop)this.GetValue(BackdropProperty);
        set => this.SetValue(BackdropProperty, value);
    }

    /// <summary>
    /// Gets the backdrop the window actually has, which is <see cref="WindowBackdrop.None"/> until one has been
    /// asked for and granted. A system that does not know the backdrop asked of it leaves the window with none.
    /// </summary>
    public WindowBackdrop ActualBackdrop
    {
        get => (WindowBackdrop)this.GetValue(ActualBackdropProperty);
    }

    /// <summary>
    /// Gets a value indicating whether the window is maximized, which is what a button offering to maximize or
    /// restore reads to know which of the two it is showing.
    /// </summary>
    public bool IsMaximized
    {
        get => (bool)this.GetValue(IsMaximizedProperty);
    }

    /// <summary>
    /// Gets the command that minimizes the window.
    /// </summary>
    public ICommand MinimizeCommand => this.minimizeCommand;

    /// <summary>
    /// Gets the command that maximizes the window.
    /// </summary>
    public ICommand MaximizeCommand => this.maximizeCommand;

    /// <summary>
    /// Gets the command that restores the window to the size it had before it was maximized or minimized.
    /// </summary>
    public ICommand RestoreCommand => this.restoreCommand;

    /// <summary>
    /// Gets the command that maximizes the window, or restores it where it already is maximized.
    /// </summary>
    public ICommand ToggleMaximizedCommand => this.toggleMaximizedCommand;

    /// <summary>
    /// Gets the command that closes the window.
    /// </summary>
    public ICommand CloseCommand => this.closeCommand;

    private bool CanMinimize => this.ResizeMode is ResizeMode.CanMinimize or ResizeMode.CanResize or ResizeMode.CanResizeWithGrip;

    private bool CanMaximize => this.ResizeMode is ResizeMode.CanResize or ResizeMode.CanResizeWithGrip;

    /// <summary>
    /// Minimizes the window.
    /// </summary>
    public void Minimize()
    {
        this.WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// Maximizes the window.
    /// </summary>
    public void Maximize()
    {
        this.WindowState = WindowState.Maximized;
    }

    /// <summary>
    /// Restores the window to the size it had before it was maximized or minimized.
    /// </summary>
    public void Restore()
    {
        this.WindowState = WindowState.Normal;
    }

    /// <summary>
    /// Maximizes the window, or restores it where it already is maximized.
    /// </summary>
    public void ToggleMaximized()
    {
        if (this.WindowState == WindowState.Maximized)
        {
            this.Restore();
        }
        else
        {
            this.Maximize();
        }
    }

    /// <summary>
    /// Called when a property has changed.
    /// </summary>
    /// <param name="e">The event args.</param>
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == ResizeModeProperty)
        {
            this.UpdateCommands();
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(nint windowHandle, int attribute, ref int value, int valueSize);

    private static void OnChromeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is FullWindow fullWindow)
        {
            fullWindow.UpdateWindowChrome();
        }
    }

    private static void OnBackdropChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is FullWindow fullWindow)
        {
            fullWindow.ApplyBackdrop();
        }
    }

    /// <summary>
    /// States what is left of the non client area, which is the edges the window is resized from and whatever of
    /// the content is to count as the caption. Everything that was drawn there is gone: no border, no title bar and
    /// none of the system's caption buttons.
    /// </summary>
    /// <remarks>
    /// The glass frame is the one thing that comes back when a backdrop is in effect. A backdrop is drawn into the
    /// frame the window manager keeps behind the window, and a window with no frame to speak of has nowhere for it
    /// to land; a thickness of minus one extends that frame over the whole window, which is what makes the backdrop
    /// reach the content. Nothing of the frame is seen, the window having no non client area left to draw it in.
    /// </remarks>
    private void UpdateWindowChrome()
    {
        WindowChrome.SetWindowChrome(
            this,
            new WindowChrome
            {
                CaptionHeight = Math.Max(0, this.CaptionHeight),
                ResizeBorderThickness = this.ResizeBorderThickness,
                GlassFrameThickness = this.isGlassFrameExtended ? new Thickness(-1) : default,
                CornerRadius = default,
                NonClientFrameEdges = NonClientFrameEdges.None,
                UseAeroCaptionButtons = false,
            });
    }

    /// <summary>
    /// Asks the window manager for the backdrop, and makes room for it where it was granted.
    /// </summary>
    private void ApplyBackdrop()
    {
        if (this.Backdrop == WindowBackdrop.Default)
        {
            // Nothing was asked for, so nothing is said: a window left to the framework keeps the background and
            // the render target the framework gave it.
            return;
        }

        if (PresentationSource.FromVisual(this) is not HwndSource hwndSource)
        {
            // There is no window to tell yet. OnSourceInitialized comes back here once there is.
            return;
        }

        var systemBackdropType = this.Backdrop switch
        {
            WindowBackdrop.Mica => DWMSBT_MAINWINDOW,
            WindowBackdrop.MicaAlt => DWMSBT_TABBEDWINDOW,
            WindowBackdrop.Acrylic => DWMSBT_TRANSIENTWINDOW,
            _ => DWMSBT_NONE,
        };

        // A system with no backdrops to give says so rather than failing silently, and the window is then left as
        // ordinary as it would have been.
        var isApplied = DwmSetWindowAttribute(hwndSource.Handle, DWMWA_SYSTEMBACKDROP_TYPE, ref systemBackdropType, sizeof(int)) == 0
                        && systemBackdropType != DWMSBT_NONE;

        // The glass frame is what the backdrop is drawn into, so it is wanted exactly when there is one.
        this.isGlassFrameExtended = isApplied;
        if (isApplied)
        {
            // WPF clears the window to a colour of its own before any content is drawn, and that colour is what
            // would be seen instead of the backdrop. The render target and the window's background are both made
            // transparent, so that what the window manager drew is what shows where no content covers it.
            if (hwndSource.CompositionTarget is { } compositionTarget)
            {
                compositionTarget.BackgroundColor = Colors.Transparent;
            }

            this.SetCurrentValue(BackgroundProperty, Brushes.Transparent);
            this.isBackgroundTransparent = true;
        }
        else if (this.isBackgroundTransparent)
        {
            this.ClearValue(BackgroundProperty);
            this.isBackgroundTransparent = false;
        }

        this.SetValue(ActualBackdropPropertyKey, isApplied ? this.Backdrop : WindowBackdrop.None);

        // The chrome says how far the frame the backdrop is drawn into reaches, so it is stated again now that
        // there is a backdrop, or none, to state it for.
        this.UpdateWindowChrome();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.ApplyBackdrop();
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        this.SetValue(IsMaximizedPropertyKey, this.WindowState == WindowState.Maximized);
        this.UpdateCommands();
    }

    private void UpdateCommands()
    {
        this.minimizeCommand.RaiseCanExecuteChanged();
        this.maximizeCommand.RaiseCanExecuteChanged();
        this.restoreCommand.RaiseCanExecuteChanged();
        this.toggleMaximizedCommand.RaiseCanExecuteChanged();
    }
}
