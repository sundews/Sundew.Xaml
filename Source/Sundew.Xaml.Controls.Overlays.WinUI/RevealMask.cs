// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RevealMask.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

/// <summary>
/// Punches a hole in an element around the cursor.
/// </summary>
/// <remarks>
/// WPF says this with an opacity mask and Avalonia with the same, but WinUI has no such thing on an element, so it is
/// said in composition instead: the element is captured onto a visual surface, that surface is drawn through a mask
/// brush whose mask is a radial gradient, and the sprite carrying the result is put in the element's place. The
/// element itself stays in the tree, laid out as before and drawn at no opacity, so that nothing is seen twice.
/// </remarks>
internal sealed class RevealMask : IDisposable
{
    private readonly FrameworkElement host;
    private readonly FrameworkElement source;
    private readonly RadialGradientBrush revealBrush;
    private readonly CompositionVisualSurface visualSurface;
    private readonly CompositionVisualSurface gradientSurface;
    private readonly SpriteVisual gradientVisual;
    private readonly CompositionRadialGradientBrush gradientBrush;
    private readonly SpriteVisual spriteVisual;

    private Point position;

    /// <summary>
    /// Initializes a new instance of the <see cref="RevealMask"/> class.
    /// </summary>
    /// <param name="host">The element the masked copy is drawn into, which is an ancestor of the source.</param>
    /// <param name="source">The element being masked.</param>
    /// <param name="revealBrush">The gradient the mask is made of.</param>
    public RevealMask(FrameworkElement host, FrameworkElement source, RadialGradientBrush revealBrush)
    {
        this.host = host;
        this.source = source;
        this.revealBrush = revealBrush;

        var sourceVisual = ElementCompositionPreview.GetElementVisual(source);
        var compositor = sourceVisual.Compositor;

        this.visualSurface = compositor.CreateVisualSurface();
        this.visualSurface.SourceVisual = sourceVisual;

        this.gradientBrush = compositor.CreateRadialGradientBrush();
        this.gradientBrush.MappingMode = CompositionMappingMode.Absolute;
        foreach (var gradientStop in revealBrush.GradientStops)
        {
            this.gradientBrush.ColorStops.Add(compositor.CreateColorGradientStop((float)gradientStop.Offset, gradientStop.Color));
        }

        // A mask brush takes a surface brush and nothing else, so the gradient is painted onto a visual of its own,
        // which is never parented anywhere and exists only to be captured.
        this.gradientVisual = compositor.CreateSpriteVisual();
        this.gradientVisual.Brush = this.gradientBrush;
        this.gradientSurface = compositor.CreateVisualSurface();
        this.gradientSurface.SourceVisual = this.gradientVisual;

        var maskBrush = compositor.CreateMaskBrush();
        maskBrush.Source = compositor.CreateSurfaceBrush(this.visualSurface);
        maskBrush.Mask = compositor.CreateSurfaceBrush(this.gradientSurface);

        this.spriteVisual = compositor.CreateSpriteVisual();
        this.spriteVisual.Brush = maskBrush;

        this.source.Opacity = 0;
        ElementCompositionPreview.SetElementChildVisual(this.host, this.spriteVisual);
        this.Update(this.position);
    }

    /// <summary>
    /// Moves the hole to where the cursor is and follows the source if it has moved or resized.
    /// </summary>
    /// <param name="positionInSource">The cursor position, in the source's own coordinates.</param>
    public void Update(Point positionInSource)
    {
        this.position = positionInSource;
        var size = new Vector2((float)this.source.ActualWidth, (float)this.source.ActualHeight);
        if (size.X <= 0 || size.Y <= 0)
        {
            return;
        }

        this.visualSurface.SourceSize = size;
        this.gradientSurface.SourceSize = size;
        this.gradientVisual.Size = size;
        this.spriteVisual.Size = size;

        var origin = this.source.TransformToVisual(this.host).TransformPoint(default);
        this.spriteVisual.Offset = new Vector3((float)origin.X, (float)origin.Y, 0);

        this.gradientBrush.EllipseRadius = this.GetRadius(size);
        this.gradientBrush.EllipseCenter = new Vector2((float)positionInSource.X, (float)positionInSource.Y);
        this.gradientBrush.GradientOriginOffset = Vector2.Zero;
    }

    /// <summary>
    /// Takes the masked copy away and gives the source back its opacity.
    /// </summary>
    public void Dispose()
    {
        ElementCompositionPreview.SetElementChildVisual(this.host, null!);
        this.source.Opacity = 1;
        this.spriteVisual.Dispose();
        this.visualSurface.Dispose();
        this.gradientSurface.Dispose();
        this.gradientVisual.Dispose();
        this.gradientBrush.Dispose();
    }

    /// <summary>
    /// Reads the radius the way the brush states it, which the framework takes either as a fraction of the element
    /// or, where the brush says so, as device independent pixels.
    /// </summary>
    private Vector2 GetRadius(Vector2 size)
    {
        return this.revealBrush.MappingMode == BrushMappingMode.Absolute
            ? new Vector2((float)this.revealBrush.RadiusX, (float)this.revealBrush.RadiusY)
            : new Vector2((float)this.revealBrush.RadiusX * size.X, (float)this.revealBrush.RadiusY * size.Y);
    }
}
