// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FullWindow.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System.Windows.Input;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;

/// <summary>
/// A window with no title bar, so that the caption, and the minimize, maximize and close buttons in it, are drawn
/// as content like anything else.
/// </summary>
/// <remarks>
/// <para>
/// This is not <see cref="Window.ExtendsContentIntoTitleBar"/>, which only draws the content up behind the title
/// bar and leaves the system's caption buttons sitting on top of it. The title bar is taken away outright, through
/// the window's presenter, so there is nothing of it left to draw over and the buttons are the application's to
/// make.
/// </para>
/// <para>
/// The border is kept, as <see cref="HasBorder"/> says by default, so that the window is still a window to the
/// window manager: it resizes from its edges, snaps and maximizes to the work area.
/// </para>
/// <para>
/// A window without a title bar is given no caption either, so <see cref="SetCaption"/> names the parts of the
/// content that are to be one. The window manager treats a press on them as a press on a title bar, which moves the
/// window, maximizes and restores it on a double click, and opens the system menu on a right click, all of it
/// snapping and docking as any other window does. Anything inside a caption that is to be clicked instead, a button
/// of the caption say, is left outside the elements named.
/// </para>
/// <para>
/// <see cref="MinimizeCommand"/> and the rest are what the buttons of such a caption are bound to. Each knows when
/// it cannot be used, so a button bound to one is disabled rather than doing nothing: a window the presenter says
/// cannot be maximized cannot be maximized, and a maximized window cannot be maximized again.
/// </para>
/// <para>
/// <see cref="Backdrop"/> asks the window manager to draw something of its own behind the content, Mica say, which a
/// window with none of the system's chrome left over it is the very place for. A system that cannot draw the one
/// asked for is not asked at all and the window keeps the background its content gives it;
/// <see cref="ActualBackdrop"/> says which of the two happened. Whether the backdrop is tinted light or dark follows
/// the theme of the content, which the framework keeps the backdrop in step with on its own. The window manager
/// also draws a backdrop only while the window is in the foreground, and puts a flat colour in its place while it
/// is not, unless <see cref="IsBackdropAlwaysDrawn"/> says otherwise.
/// </para>
/// <para>
/// Uno's Windows head draws a backdrop as well, but only once the application has told it not to draw through
/// OpenGL: <c>FeatureConfiguration.Rendering.UseVulkanOnWin32 = true</c>, or
/// <c>UseOpenGLOnWin32 = false</c> for the software one, said before the host is built. A backdrop makes the
/// window's surface see through, and its OpenGL path never clears such a surface, so everything drawn over the
/// window stays on it: a closed combo box leaves the list it dropped behind, and the content grows heavier the
/// longer it is drawn. Which renderer is in use is the application's to say and cannot be read back, so this is
/// left for it to get right rather than guessed at here.
/// </para>
/// <para>
/// A backdrop costs something else on Uno that nothing here can mend: that head draws no solid colour named as a
/// <c>Background</c>. A border or a panel given a solid brush to paint itself with is not drawn at all, while its
/// child is, and the same colour as a shape's fill is drawn, as is a gradient named as a background. So a slider
/// on such a window opens with no track behind its thumb and no fill in it, those being the parts the theme
/// paints with a solid background, and draws in full the moment the pointer is over it, because the theme sets
/// the brush again on hovering it. Setting such a brush over again from code, once the window is up, draws it
/// too. Any window of that framework with a backdrop does this, one of its own as much as this one; WinUI draws
/// them all from the first frame.
/// </para>
/// <para>
/// A backdrop is only seen where the content does not cover it, and the root element of a window is usually a panel
/// with a background of its own. Such a window leaves that background unset, or sets it to something the backdrop
/// can be seen through.
/// </para>
/// </remarks>
public class FullWindow : Window
{
    private readonly WindowCommand minimizeCommand;
    private readonly WindowCommand maximizeCommand;
    private readonly WindowCommand restoreCommand;
    private readonly WindowCommand toggleMaximizedCommand;
    private readonly WindowCommand closeCommand;

    private readonly OverlappedPresenter? presenter;

    private FrameworkElement[] captionElements = [];
    private bool hasBorder = true;
    private bool isMaximized;
    private WindowBackdrop backdrop;
    private WindowBackdrop actualBackdrop;
    private bool isBackdropAlwaysDrawn;

    /// <summary>
    /// Initializes a new instance of the <see cref="FullWindow"/> class.
    /// </summary>
    public FullWindow()
    {
        this.minimizeCommand = new WindowCommand(this.Minimize, () => this.presenter?.IsMinimizable == true);
        this.maximizeCommand = new WindowCommand(this.Maximize, () => this.CanMaximize && !this.IsMaximized);
        this.restoreCommand = new WindowCommand(this.Restore, () => this.presenter?.State != OverlappedPresenterState.Restored);
        this.toggleMaximizedCommand = new WindowCommand(this.ToggleMaximized, () => this.CanMaximize);
        this.closeCommand = new WindowCommand(this.Close, () => true);

        // A window is overlapped to begin with, which is the only presenter that has a title bar to give up. One
        // put into full screen or compact overlay has none in the first place, and there is nothing to say to it.
        this.presenter = this.AppWindow.Presenter as OverlappedPresenter;
        this.UpdateTitleBar();
        this.ReadState();
        this.AppWindow.Changed += this.OnAppWindowChanged;

        // Where the caption is said in pixels, so it is said again whenever the window is a different size.
        this.SizeChanged += this.OnSizeChanged;
    }

    /// <summary>
    /// Occurs when the window has been maximized or has stopped being maximized, which is when a button offering
    /// to maximize or restore changes which of the two it is showing.
    /// </summary>
    public event EventHandler? IsMaximizedChanged;

    /// <summary>
    /// Gets or sets a value indicating whether the window manager still gives the window a border, which is what
    /// it resizes the window from. It does by default; a window without one can only be resized and moved by the
    /// application itself.
    /// </summary>
    public bool HasBorder
    {
        get => this.hasBorder;

        set
        {
            if (this.hasBorder == value)
            {
                return;
            }

            this.hasBorder = value;
            this.UpdateTitleBar();
            this.UpdateCommands();
        }
    }

    /// <summary>
    /// Gets or sets the backdrop asked of the window manager, which <see cref="ActualBackdrop"/> says the answer to.
    /// </summary>
    public WindowBackdrop Backdrop
    {
        get => this.backdrop;

        set
        {
            if (this.backdrop == value)
            {
                return;
            }

            this.backdrop = value;
            this.ApplyBackdrop();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the backdrop is to go on being drawn while the window is not the
    /// one in front. It is not by default, that being what the window manager does with a backdrop of its own.
    /// </summary>
    /// <remarks>
    /// Every backdrop can be had this way in WinUI 3, Mica as well as acrylic: it is the same backdrop, run by a
    /// controller of this window's own that never tells it the window has lost the input. Uno has the types that
    /// controller is made of and none of what they do, so there this is ignored and the backdrop goes on
    /// following the window.
    /// </remarks>
    public bool IsBackdropAlwaysDrawn
    {
        get => this.isBackdropAlwaysDrawn;

        set
        {
            if (this.isBackdropAlwaysDrawn == value)
            {
                return;
            }

            this.isBackdropAlwaysDrawn = value;
            this.ApplyBackdrop();
        }
    }

    /// <summary>
    /// Gets the backdrop the window actually has, which is <see cref="WindowBackdrop.None"/> on a system that has
    /// none to give.
    /// </summary>
    public WindowBackdrop ActualBackdrop => this.actualBackdrop;

    /// <summary>
    /// Gets a value indicating whether the window is maximized.
    /// </summary>
    public bool IsMaximized => this.isMaximized;

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

    private bool CanMaximize => this.presenter?.IsMaximizable == true;

    /// <summary>
    /// Names the parts of the content that are the window's caption, which is what a window with no title bar has
    /// instead of one. Naming none of them leaves the window with no caption at all.
    /// </summary>
    /// <param name="elements">The elements that are the caption.</param>
    /// <remarks>
    /// Said to the window manager as regions rather than answered by the application, so that the press begins a
    /// move the way a press on a title bar does. Telling the window manager about the press afterwards, from the
    /// pointer handler of an element, does not: the content of a window of this framework is drawn in an island of
    /// its own, which still holds the pointer, and the move begins without the button being down.
    /// </remarks>
    public void SetCaption(params FrameworkElement[] elements)
    {
        foreach (var captionElement in this.captionElements)
        {
            captionElement.SizeChanged -= this.OnCaptionElementSizeChanged;
        }

        this.captionElements = elements;
        foreach (var captionElement in this.captionElements)
        {
            captionElement.SizeChanged += this.OnCaptionElementSizeChanged;
        }

        this.UpdateCaption();
    }

    /// <summary>
    /// Minimizes the window.
    /// </summary>
    public void Minimize()
    {
        this.presenter?.Minimize();
    }

    /// <summary>
    /// Maximizes the window.
    /// </summary>
    public void Maximize()
    {
        this.presenter?.Maximize();
    }

    /// <summary>
    /// Restores the window to the size it had before it was maximized or minimized.
    /// </summary>
    public void Restore()
    {
        this.presenter?.Restore();
    }

    /// <summary>
    /// Maximizes the window, or restores it where it already is maximized.
    /// </summary>
    public void ToggleMaximized()
    {
        if (this.IsMaximized)
        {
            this.Restore();
        }
        else
        {
            this.Maximize();
        }
    }

    /// <summary>
    /// Says what the window manager is to leave off: the title bar always, and the border where it was asked for.
    /// </summary>
    private void UpdateTitleBar()
    {
        this.presenter?.SetBorderAndTitleBar(this.hasBorder, false);
    }

    /// <summary>
    /// Tells the window manager where the caption is, in the pixels of the display the window is on.
    /// </summary>
    private void UpdateCaption()
    {
        if (this.Content is not FrameworkElement root || root.XamlRoot == null)
        {
            return;
        }

        var scale = root.XamlRoot.RasterizationScale;
        var rectangles = new List<RectInt32>();
        foreach (var captionElement in this.captionElements)
        {
            if (captionElement.ActualWidth <= 0 || captionElement.ActualHeight <= 0)
            {
                continue;
            }

            var origin = captionElement.TransformToVisual(root).TransformPoint(default);
            rectangles.Add(new RectInt32
            {
                X = (int)(origin.X * scale),
                Y = (int)(origin.Y * scale),
                Width = (int)(captionElement.ActualWidth * scale),
                Height = (int)(captionElement.ActualHeight * scale),
            });
        }

        InputNonClientPointerSource.GetForWindowId(this.AppWindow.Id)
            .SetRegionRects(NonClientRegionKind.Caption, rectangles.ToArray());
    }

    /// <summary>
    /// Hands the window the backdrop it asked for, where the system has that one to draw.
    /// </summary>
    /// <remarks>
    /// A controller of our own is not needed for this. Naming the backdrop on the window is enough: the framework
    /// runs the controller behind it, follows the theme of the content with it, and lets go of it when the window
    /// closes.
    /// </remarks>
    private void ApplyBackdrop()
    {
        var newActualBackdrop = WindowBackdrop.None;
        switch (this.backdrop)
        {
            case WindowBackdrop.Mica when MicaController.IsSupported():
                newActualBackdrop = WindowBackdrop.Mica;
                break;
            case WindowBackdrop.MicaAlt when MicaController.IsSupported():
#if SUNDEW_WINUI
                newActualBackdrop = WindowBackdrop.MicaAlt;
#else
                // Uno has no say over which Mica it asks for, so the alternative tint is the ordinary one there.
                newActualBackdrop = WindowBackdrop.Mica;
#endif
                break;
            case WindowBackdrop.Acrylic when DesktopAcrylicController.IsSupported():
                newActualBackdrop = WindowBackdrop.Acrylic;
                break;
        }

        // Nothing asked for is nothing said, so that a window given a backdrop by anything else keeps it.
        if (this.backdrop != WindowBackdrop.Default)
        {
            this.SystemBackdrop = this.CreateBackdrop(newActualBackdrop);
        }

        this.actualBackdrop = newActualBackdrop;
    }

    /// <summary>
    /// Makes the backdrop to put on the window, asked for in whichever of the two ways was wanted.
    /// </summary>
    /// <param name="backdrop">The backdrop, which has already been found to be one this system has.</param>
    /// <returns>The backdrop, or nothing where the system has none to give.</returns>
    private SystemBackdrop? CreateBackdrop(WindowBackdrop backdrop)
    {
        if (backdrop == WindowBackdrop.None)
        {
            return null;
        }

#if SUNDEW_WINUI
        if (this.isBackdropAlwaysDrawn)
        {
            return new AlwaysDrawnBackdrop(backdrop);
        }
#endif

        return CreateFrameworkBackdrop(backdrop);
    }

    /// <summary>
    /// Makes the backdrop the framework hands out, which follows the window: it stops being drawn while the window
    /// is not the one in front.
    /// </summary>
    /// <param name="backdrop">The backdrop, which has already been found to be one this system has.</param>
    /// <returns>The backdrop to put on the window.</returns>
    private static SystemBackdrop CreateFrameworkBackdrop(WindowBackdrop backdrop)
    {
        if (backdrop == WindowBackdrop.Acrylic)
        {
            return new DesktopAcrylicBackdrop();
        }

#if SUNDEW_WINUI
        // Base is the kind a Mica backdrop is of to begin with, so it is left unsaid.
        return backdrop == WindowBackdrop.MicaAlt ? new MicaBackdrop { Kind = MicaKind.BaseAlt } : new MicaBackdrop();
#else
        return new MicaBackdrop();
#endif
    }

    private void OnCaptionElementSizeChanged(object sender, SizeChangedEventArgs e)
    {
        this.UpdateCaption();
    }

    private void OnSizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        this.UpdateCaption();
    }

    private void OnAppWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (args.DidPresenterChange || args.DidSizeChange || args.DidPositionChange || args.DidVisibilityChange)
        {
            this.ReadState();
            this.UpdateCommands();
        }
    }

    private void ReadState()
    {
        var newIsMaximized = this.presenter?.State == OverlappedPresenterState.Maximized;
        if (this.isMaximized == newIsMaximized)
        {
            return;
        }

        this.isMaximized = newIsMaximized;
        this.IsMaximizedChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateCommands()
    {
        this.minimizeCommand.RaiseCanExecuteChanged();
        this.maximizeCommand.RaiseCanExecuteChanged();
        this.restoreCommand.RaiseCanExecuteChanged();
        this.toggleMaximizedCommand.RaiseCanExecuteChanged();
    }
}
