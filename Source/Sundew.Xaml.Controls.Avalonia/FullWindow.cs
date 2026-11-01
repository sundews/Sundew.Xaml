// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FullWindow.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System.Windows.Input;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Media;

/// <summary>
/// A window with no decorations of the system's, so that the caption, and the minimize, maximize and close buttons
/// in it, are drawn as content like anything else.
/// </summary>
/// <remarks>
/// <para>
/// Only the title bar is given up, not the border: the window keeps <see cref="WindowDecorations.BorderOnly"/>, so
/// that it is still a window to the window manager and resizes from its edges, snaps and maximizes to the work
/// area, while nothing is drawn above the content. The client area is then extended over where the decorations
/// were, which is what lets the content reach the top of the window.
/// </para>
/// <para>
/// A window with no border at all is <see cref="Window.WindowDecorations"/> set to
/// <see cref="WindowDecorations.None"/>, which leaves nothing for the window manager to resize from either;
/// <see cref="Window.BeginResizeDrag"/> is then how a grip of one's own resizes it, as
/// <see cref="Window.BeginMoveDrag"/> is how a caption of one's own moves it.
/// </para>
/// <para>
/// <see cref="MinimizeCommand"/> and the rest are what the buttons of such a caption are bound to. Each knows when
/// it cannot be used, so a button bound to one is disabled rather than doing nothing: a window that cannot resize
/// cannot be maximized, and a maximized window cannot be maximized again.
/// </para>
/// <para>
/// <see cref="Backdrop"/> asks the window manager to draw something of its own behind the content, Mica say, which a
/// window with none of the system's chrome left over it is the very place for. It is asked for as a transparency
/// level, so a system that has no such thing to give, which is every system but Windows 11, simply gives none and
/// the window is as opaque as any other. Asking hands the window's background over: it is made transparent while a
/// backdrop is in effect and given back to the theme where none could be had, so that the fallback is an ordinary
/// window rather than a hole. <see cref="ActualBackdrop"/> is what came of the asking.
/// </para>
/// </remarks>
public class FullWindow : Window
{
    /// <summary>
    /// Identifies the <see cref="Backdrop"/> property.
    /// </summary>
    public static readonly StyledProperty<WindowBackdrop> BackdropProperty =
        AvaloniaProperty.Register<FullWindow, WindowBackdrop>(nameof(Backdrop));

    /// <summary>
    /// Identifies the <see cref="ActualBackdrop"/> property.
    /// </summary>
    public static readonly DirectProperty<FullWindow, WindowBackdrop> ActualBackdropProperty =
        AvaloniaProperty.RegisterDirect<FullWindow, WindowBackdrop>(nameof(ActualBackdrop), fullWindow => fullWindow.ActualBackdrop);

    /// <summary>
    /// Identifies the <see cref="IsMaximized"/> property.
    /// </summary>
    public static readonly DirectProperty<FullWindow, bool> IsMaximizedProperty =
        AvaloniaProperty.RegisterDirect<FullWindow, bool>(nameof(IsMaximized), fullWindow => fullWindow.IsMaximized);

    private readonly WindowCommand minimizeCommand;
    private readonly WindowCommand maximizeCommand;
    private readonly WindowCommand restoreCommand;
    private readonly WindowCommand toggleMaximizedCommand;
    private readonly WindowCommand closeCommand;

    private bool isMaximized;
    private WindowBackdrop actualBackdrop;
    private bool isBackgroundTransparent;

    static FullWindow()
    {
        // Overridden as the default rather than set on the window, so that a window naming its own decorations,
        // WindowDecorations.None say, still wins.
        WindowDecorationsProperty.OverrideDefaultValue<FullWindow>(WindowDecorations.BorderOnly);
        ExtendClientAreaToDecorationsHintProperty.OverrideDefaultValue<FullWindow>(true);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FullWindow"/> class.
    /// </summary>
    public FullWindow()
    {
        this.minimizeCommand = new WindowCommand(this.Minimize, this.CanMinimizeNow);
        this.maximizeCommand = new WindowCommand(this.Maximize, () => this.CanMaximizeNow() && this.WindowState != WindowState.Maximized);
        this.restoreCommand = new WindowCommand(this.Restore, () => this.WindowState != WindowState.Normal);
        this.toggleMaximizedCommand = new WindowCommand(this.ToggleMaximized, this.CanMaximizeNow);
        this.closeCommand = new WindowCommand(this.Close, () => true);
    }

    /// <summary>
    /// Gets or sets the backdrop asked of the window manager, which <see cref="ActualBackdrop"/> says the answer to.
    /// </summary>
    public WindowBackdrop Backdrop
    {
        get => this.GetValue(BackdropProperty);
        set => this.SetValue(BackdropProperty, value);
    }

    /// <summary>
    /// Gets the backdrop the window actually has, which is <see cref="WindowBackdrop.None"/> on every system that
    /// has none to give. Mica asked for and Mica Alt asked for both end as <see cref="WindowBackdrop.Mica"/>, the
    /// one being all this framework can say to the window manager.
    /// </summary>
    public WindowBackdrop ActualBackdrop
    {
        get => this.actualBackdrop;
        private set => this.SetAndRaise(ActualBackdropProperty, ref this.actualBackdrop, value);
    }

    /// <summary>
    /// Gets a value indicating whether the window is maximized, which is what a button offering to maximize or
    /// restore reads to know which of the two it is showing.
    /// </summary>
    public bool IsMaximized
    {
        get => this.isMaximized;
        private set => this.SetAndRaise(IsMaximizedProperty, ref this.isMaximized, value);
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
    /// <param name="change">The change.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WindowStateProperty)
        {
            this.IsMaximized = this.WindowState == WindowState.Maximized;
            this.UpdateCommands();
        }
        else if (change.Property == CanResizeProperty || change.Property == CanMaximizeProperty || change.Property == CanMinimizeProperty)
        {
            this.UpdateCommands();
        }
        else if (change.Property == BackdropProperty)
        {
            this.AskForBackdrop();
        }
        else if (change.Property == ActualTransparencyLevelProperty)
        {
            this.ReadBackdrop();
        }
    }

    private bool CanMinimizeNow()
    {
        return this.CanMinimize;
    }

    private bool CanMaximizeNow()
    {
        return this.CanMaximize && this.CanResize;
    }

    /// <summary>
    /// Says which transparency levels would do for the backdrop asked for, in the order they would do. The platform
    /// takes the first of them it can give and reports None when it can give none, which is what the fallback is.
    /// </summary>
    private void AskForBackdrop()
    {
        if (this.Backdrop == WindowBackdrop.Default)
        {
            // Nothing was asked for, so nothing is said and a window naming its own transparency level keeps it.
            return;
        }

        IReadOnlyList<WindowTransparencyLevel> transparencyLevels = this.Backdrop switch
        {
            // Mica Alt is a tint of Mica rather than a level of its own, and this framework says only the one.
            WindowBackdrop.Mica or WindowBackdrop.MicaAlt => [WindowTransparencyLevel.Mica],
            WindowBackdrop.Acrylic => [WindowTransparencyLevel.AcrylicBlur],
            _ => [WindowTransparencyLevel.None],
        };

        // Set outright rather than as a current value. The asking happens while the window is being built, before
        // the theme has had its say, and a current value set that early is one the theme can still overrule.
        this.SetValue(TransparencyLevelHintProperty, transparencyLevels);
    }

    /// <summary>
    /// Reads what the platform gave, and leaves the background to match: out of the way of a backdrop that is
    /// actually there, and back to the theme where there is none.
    /// </summary>
    private void ReadBackdrop()
    {
        var actualTransparencyLevel = this.ActualTransparencyLevel;
        this.ActualBackdrop = actualTransparencyLevel == WindowTransparencyLevel.Mica
            ? WindowBackdrop.Mica
            : actualTransparencyLevel == WindowTransparencyLevel.AcrylicBlur
                ? WindowBackdrop.Acrylic
                : WindowBackdrop.None;

        if (this.Backdrop == WindowBackdrop.Default)
        {
            return;
        }

        if (this.ActualBackdrop != WindowBackdrop.None)
        {
            this.SetValue(BackgroundProperty, Brushes.Transparent);
            this.isBackgroundTransparent = true;
        }
        else if (this.isBackgroundTransparent)
        {
            this.ClearValue(BackgroundProperty);
            this.isBackgroundTransparent = false;
        }
    }

    private void UpdateCommands()
    {
        this.minimizeCommand.RaiseCanExecuteChanged();
        this.maximizeCommand.RaiseCanExecuteChanged();
        this.restoreCommand.RaiseCanExecuteChanged();
        this.toggleMaximizedCommand.RaiseCanExecuteChanged();
    }
}
