// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AlwaysDrawnBackdrop.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

/// <summary>
/// A backdrop that goes on being drawn while the window is not the one in front.
/// </summary>
/// <remarks>
/// The backdrops the framework hands out follow the window: they are told the window has lost the input, and the
/// window manager puts a flat colour in the backdrop's place until it has it back. This one runs the very same
/// controller, and says the window has the input whatever happens, which is what keeps the backdrop drawn. The
/// light or dark of it is still followed, since that is this window's to tell and not the window manager's.
/// </remarks>
internal sealed class AlwaysDrawnBackdrop : SystemBackdrop
{
    private readonly WindowBackdrop backdrop;

    private ISystemBackdropControllerWithTargets? controller;
    private SystemBackdropConfiguration? configuration;
    private FrameworkElement? themeSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlwaysDrawnBackdrop"/> class.
    /// </summary>
    /// <param name="backdrop">The backdrop to draw, which has already been found to be one this system has.</param>
    public AlwaysDrawnBackdrop(WindowBackdrop backdrop)
    {
        this.backdrop = backdrop;
    }

    /// <summary>
    /// Called when the window has something to draw the backdrop into.
    /// </summary>
    /// <param name="connectedTarget">What to draw into.</param>
    /// <param name="xamlRoot">The root of the content, which is what the theme is read from.</param>
    protected override void OnTargetConnected(ICompositionSupportsSystemBackdrop connectedTarget, XamlRoot xamlRoot)
    {
        base.OnTargetConnected(connectedTarget, xamlRoot);
        this.configuration = new SystemBackdropConfiguration { IsInputActive = true };
        this.themeSource = xamlRoot.Content as FrameworkElement;
        if (this.themeSource != null)
        {
            this.themeSource.ActualThemeChanged += this.OnActualThemeChanged;
        }

        this.UpdateTheme();
        this.controller = CreateController(this.backdrop);
        this.controller.SetSystemBackdropConfiguration(this.configuration);
        this.controller.AddSystemBackdropTarget(connectedTarget);
    }

    /// <summary>
    /// Called when there is nothing left to draw into, the window having closed or been given another backdrop.
    /// </summary>
    /// <param name="disconnectedTarget">What was being drawn into.</param>
    protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop disconnectedTarget)
    {
        base.OnTargetDisconnected(disconnectedTarget);
        this.controller?.RemoveSystemBackdropTarget(disconnectedTarget);
        this.controller?.Dispose();
        this.controller = null;
        if (this.themeSource != null)
        {
            this.themeSource.ActualThemeChanged -= this.OnActualThemeChanged;
            this.themeSource = null;
        }

        this.configuration = null;
    }

    private static ISystemBackdropControllerWithTargets CreateController(WindowBackdrop backdrop)
    {
        if (backdrop == WindowBackdrop.Acrylic)
        {
            return new DesktopAcrylicController();
        }

#if SUNDEW_WINUI
        return new MicaController { Kind = backdrop == WindowBackdrop.MicaAlt ? MicaKind.BaseAlt : MicaKind.Base };
#else
        // Uno has no say over which Mica it asks for.
        return new MicaController();
#endif
    }

    private void OnActualThemeChanged(FrameworkElement sender, object args)
    {
        this.UpdateTheme();
    }

    private void UpdateTheme()
    {
        if (this.configuration == null)
        {
            return;
        }

        this.configuration.Theme = this.themeSource?.ActualTheme switch
        {
            ElementTheme.Dark => SystemBackdropTheme.Dark,
            ElementTheme.Light => SystemBackdropTheme.Light,
            _ => SystemBackdropTheme.Default,
        };
    }
}
