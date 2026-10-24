// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Border.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Sundew.Xaml.Controls.Backends;
using Windows.Foundation;
using DeviceScale = Sundew.Xaml.Controls.Scale;
using NativeBorder = Microsoft.UI.Xaml.Controls.Border;

/// <summary>
/// Provides a border control that supports independently customizable brushes and gradient stops for each side and corner, enabling advanced border rendering scenarios beyond the standard Border control.
/// </summary>
/// <remarks>
/// Neither WinUI nor Uno lets a control paint itself, so this is a <see cref="Panel"/> holding two children: an
/// <see cref="IBorderSurface"/> that the border is drawn on, and a plain <see cref="NativeBorder"/> that holds the
/// content and clips it. The surface is the only part that differs per platform, which is what lets WinUI on Windows
/// draw with Win2D and every other Uno target draw with Skia from this one implementation.
/// </remarks>
[ContentProperty(Name = nameof(Child))]
public partial class Border : Panel
{
    /// <summary>
    /// Static fields are initialized in declaration order, so this has to come before the properties that
    /// default to it.
    /// </summary>
    private static readonly Stops DefaultStops = new Stops(0.2, 0.8);

    /// <summary>
    /// Identifies the Background dependency property.
    /// </summary>
    /// <remarks>
    /// This deliberately hides the Background of <see cref="Panel"/>, which a panel paints as a plain rectangle
    /// behind its children and which would therefore show square corners outside the rounded border. The base
    /// property is left unset and this one is painted on the surface, following the corner radius.
    /// </remarks>
    public static new readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
        nameof(Background), typeof(Brush), typeof(Border), new PropertyMetadata(null, OnRedrawRequired));

    /// <summary>
    /// Identifies the <see cref="BorderThickness"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(
        nameof(BorderThickness), typeof(Thickness), typeof(Border), new PropertyMetadata(default(Thickness), OnRelayoutRequired));

    /// <summary>
    /// Identifies the <see cref="CornerRadius"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius), typeof(CornerRadius), typeof(Border), new PropertyMetadata(default(CornerRadius), OnRelayoutRequired));

    /// <summary>
    /// Identifies the <see cref="Padding"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
        nameof(Padding), typeof(Thickness), typeof(Border), new PropertyMetadata(default(Thickness), OnRelayoutRequired));

    /// <summary>
    /// Identifies the <see cref="LeftBrush"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty LeftBrushProperty = DependencyProperty.Register(
        nameof(LeftBrush), typeof(Brush), typeof(Border), new PropertyMetadata(null, OnFreshRenderRequired));

    /// <summary>
    /// Identifies the <see cref="TopBrush"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TopBrushProperty = DependencyProperty.Register(
        nameof(TopBrush), typeof(Brush), typeof(Border), new PropertyMetadata(null, OnFreshRenderRequired));

    /// <summary>
    /// Identifies the <see cref="RightBrush"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty RightBrushProperty = DependencyProperty.Register(
        nameof(RightBrush), typeof(Brush), typeof(Border), new PropertyMetadata(null, OnFreshRenderRequired));

    /// <summary>
    /// Identifies the <see cref="BottomBrush"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BottomBrushProperty = DependencyProperty.Register(
        nameof(BottomBrush), typeof(Brush), typeof(Border), new PropertyMetadata(null, OnFreshRenderRequired));

    /// <summary>
    /// Identifies the <see cref="TopLeftStops"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TopLeftStopsProperty = DependencyProperty.Register(
        nameof(TopLeftStops), typeof(Stops), typeof(Border), new PropertyMetadata(DefaultStops, OnFreshRenderRequired));

    /// <summary>
    /// Identifies the <see cref="TopRightStops"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TopRightStopsProperty = DependencyProperty.Register(
        nameof(TopRightStops), typeof(Stops), typeof(Border), new PropertyMetadata(DefaultStops, OnFreshRenderRequired));

    /// <summary>
    /// Identifies the <see cref="BottomRightStops"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BottomRightStopsProperty = DependencyProperty.Register(
        nameof(BottomRightStops), typeof(Stops), typeof(Border), new PropertyMetadata(DefaultStops, OnFreshRenderRequired));

    /// <summary>
    /// Identifies the <see cref="BottomLeftStops"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BottomLeftStopsProperty = DependencyProperty.Register(
        nameof(BottomLeftStops), typeof(Stops), typeof(Border), new PropertyMetadata(DefaultStops, OnFreshRenderRequired));

    /// <summary>
    /// Identifies the <see cref="ClipContentToCornerRadius"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ClipContentToCornerRadiusProperty = DependencyProperty.Register(
        nameof(ClipContentToCornerRadius), typeof(bool), typeof(Border), new PropertyMetadata(false, OnRelayoutRequired));

    private readonly NativeBorder contentHost = new NativeBorder();
    private IBorderSurface surface;
    private Corners<CornerBlend>? cornerBlends;

    /// <summary>
    /// Initializes a new instance of the <see cref="Border"/> class.
    /// </summary>
    public Border()
    {
        this.surface = BorderSurfaceFactory.Create((session, size) => this.Draw(session, size, null));
        this.Children.Add(this.surface.Element);
        this.Children.Add(this.contentHost);
        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;
    }

    /// <summary>
    /// Gets or sets the brush filling the area inside the border.
    /// </summary>
    public new Brush? Background
    {
        get => (Brush?)this.GetValue(BackgroundProperty);
        set => this.SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness of each side of the border.
    /// </summary>
    public Thickness BorderThickness
    {
        get => (Thickness)this.GetValue(BorderThicknessProperty);
        set => this.SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the radius of each corner of the border.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)this.GetValue(CornerRadiusProperty);
        set => this.SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the distance between the border and its content.
    /// </summary>
    public Thickness Padding
    {
        get => (Thickness)this.GetValue(PaddingProperty);
        set => this.SetValue(PaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the content of the border.
    /// </summary>
    public UIElement? Child
    {
        get => this.contentHost.Child;
        set => this.contentHost.Child = value;
    }

    /// <summary>
    /// Gets or sets the side brushes.
    /// </summary>
    public SideBrushes? SideBrushes
    {
        get => new SideBrushes(this.LeftBrush, this.TopBrush, this.RightBrush, this.BottomBrush);
        set
        {
            this.LeftBrush = value?.Left;
            this.TopBrush = value?.Top;
            this.RightBrush = value?.Right;
            this.BottomBrush = value?.Bottom;
        }
    }

    /// <summary>
    /// Gets or sets the corner stops.
    /// </summary>
    public CornerStops CornerStops
    {
        get => new CornerStops(this.TopLeftStops, this.TopRightStops, this.BottomRightStops, this.BottomLeftStops);
        set
        {
            this.TopLeftStops = value.TopLeft;
            this.TopRightStops = value.TopRight;
            this.BottomRightStops = value.BottomRight;
            this.BottomLeftStops = value.BottomLeft;
        }
    }

    /// <summary>
    /// Gets or sets the brush for the left side of the border.
    /// </summary>
    public Brush? LeftBrush
    {
        get => (Brush?)this.GetValue(LeftBrushProperty);
        set => this.SetValue(LeftBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush for the top of the border.
    /// </summary>
    public Brush? TopBrush
    {
        get => (Brush?)this.GetValue(TopBrushProperty);
        set => this.SetValue(TopBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush for the right side of the border.
    /// </summary>
    public Brush? RightBrush
    {
        get => (Brush?)this.GetValue(RightBrushProperty);
        set => this.SetValue(RightBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush for the bottom of the border.
    /// </summary>
    public Brush? BottomBrush
    {
        get => (Brush?)this.GetValue(BottomBrushProperty);
        set => this.SetValue(BottomBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the gradient stops for the top-left corner.
    /// </summary>
    public Stops TopLeftStops
    {
        get => (Stops)this.GetValue(TopLeftStopsProperty);
        set => this.SetValue(TopLeftStopsProperty, value);
    }

    /// <summary>
    /// Gets or sets the gradient stops for the top-right corner.
    /// </summary>
    public Stops TopRightStops
    {
        get => (Stops)this.GetValue(TopRightStopsProperty);
        set => this.SetValue(TopRightStopsProperty, value);
    }

    /// <summary>
    /// Gets or sets the gradient stops for the bottom-right corner.
    /// </summary>
    public Stops BottomRightStops
    {
        get => (Stops)this.GetValue(BottomRightStopsProperty);
        set => this.SetValue(BottomRightStopsProperty, value);
    }

    /// <summary>
    /// Gets or sets the gradient stops for the bottom-left corner.
    /// </summary>
    public Stops BottomLeftStops
    {
        get => (Stops)this.GetValue(BottomLeftStopsProperty);
        set => this.SetValue(BottomLeftStopsProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the content is clipped to the <see cref="CornerRadius"/>.
    /// </summary>
    public bool ClipContentToCornerRadius
    {
        get => (bool)this.GetValue(ClipContentToCornerRadiusProperty);
        set => this.SetValue(ClipContentToCornerRadiusProperty, value);
    }

    /// <summary>
    /// Draws the border onto the given session, exactly as it paints itself.
    /// </summary>
    /// <remarks>
    /// The border normally draws onto its own surface. This lets it be drawn onto someone else's instead, which is
    /// what the development tester uses to capture a sheet of borders: neither WinUI nor Uno can capture what a
    /// drawing surface has composited.
    /// </remarks>
    /// <param name="session">The session to draw on.</param>
    /// <param name="size">The size to draw at.</param>
    public void RenderTo(IBorderDrawingSession session, Size size)
    {
        this.Draw(session, size, null);
    }

    /// <summary>
    /// Draws the border onto the given session at a stated device pixel scale, rather than the one of the display it
    /// happens to be on.
    /// </summary>
    /// <remarks>
    /// The border rounds its geometry to the pixel grid of its display, so two sheets drawn on displays of different
    /// scale are rounded differently and are not comparable. Stating the scale is what makes a captured sheet the
    /// same wherever it was taken.
    /// </remarks>
    /// <param name="session">The session to draw on.</param>
    /// <param name="size">The size to draw at.</param>
    /// <param name="rasterizationScale">The device pixel scale to round to.</param>
    public void RenderTo(IBorderDrawingSession session, Size size, double rasterizationScale)
    {
        this.Draw(session, size, rasterizationScale);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var thickness = this.BorderThickness;
        var horizontal = thickness.Left + thickness.Right;
        var vertical = thickness.Top + thickness.Bottom;

        this.contentHost.Padding = this.Padding;
        this.contentHost.Measure(new Size(
            Math.Max(0, availableSize.Width - horizontal),
            Math.Max(0, availableSize.Height - vertical)));

        var desired = new Size(
            this.contentHost.DesiredSize.Width + horizontal,
            this.contentHost.DesiredSize.Height + vertical);

        // The surface covers the whole border, and is measured to that rather than to the available size, which may
        // be infinite. A canvas sizes its own drawing surface from what it was measured to, so measuring it at
        // nothing would leave it with nothing to draw on.
        this.surface.Element.Measure(desired);
        return desired;
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        this.surface.Element.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

        var thickness = this.BorderThickness;
        var width = Math.Max(0, finalSize.Width - thickness.Left - thickness.Right);
        var height = Math.Max(0, finalSize.Height - thickness.Top - thickness.Bottom);
        this.contentHost.Arrange(new Rect(thickness.Left, thickness.Top, width, height));

        this.UpdateContentHost(finalSize, new Size(width, height));
        this.surface.Invalidate();
        return finalSize;
    }

    private static void OnRedrawRequired(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        (dependencyObject as Border)?.surface.Invalidate();
    }

    private static void OnFreshRenderRequired(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is Border border)
        {
            border.cornerBlends = null;
            border.surface.Invalidate();
        }
    }

    private static void OnRelayoutRequired(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        (dependencyObject as Border)?.InvalidateMeasure();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (this.Children.Count != 0 && ReferenceEquals(this.Children[0], this.surface.Element))
        {
            return;
        }

        // The surface was released when the border left the visual tree, so it needs building again.
        this.surface = BorderSurfaceFactory.Create((session, size) => this.Draw(session, size, null));
        this.Children.Insert(0, this.surface.Element);
        this.surface.Invalidate();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this.Children.Remove(this.surface.Element);
        this.surface.Release();
    }

    private BorderMetrics GetMetrics(Size size, double? rasterizationScale = null)
    {
        return new BorderMetrics(
            size.Width,
            size.Height,
            this.BorderThickness.ToSides(),
            this.CornerRadius.ToCorners(),
            DeviceScale.Uniform(rasterizationScale ?? this.XamlRoot?.RasterizationScale ?? 1.0));
    }

    private void UpdateContentHost(Size size, Size contentSize)
    {
        // WinUI only lets Clip be a rectangle, so the rounded part of the clip is left to the corner radius of the
        // plain border holding the content, which both WinUI and Uno clip to natively.
        this.contentHost.Clip = new RectangleGeometry { Rect = new Rect(0, 0, contentSize.Width, contentSize.Height) };
        this.contentHost.CornerRadius = this.ClipContentToCornerRadius
            ? this.GetInnerCornerRadius(size)
            : default;
    }

    /// <summary>
    /// Works out the corner radius of the inside of the ring, which is what the content has to be clipped to.
    /// </summary>
    /// <remarks>
    /// The radius is read back off the clip figure the calculator produces rather than worked out again here, so that
    /// the clip can never drift from the boundary the border is actually drawn down to.
    /// </remarks>
    /// <param name="size">The size of the border.</param>
    /// <returns>The corner radius, in the coordinate space of the content host.</returns>
    private CornerRadius GetInnerCornerRadius(Size size)
    {
        var metrics = this.GetMetrics(size);
        if (!BorderLayoutCalculator.TryCalculateContentClip(metrics, default, out var figure))
        {
            return default;
        }

        var left = figure.Edges.Left.Start.X;
        var right = figure.Edges.Right.Start.X;
        return new CornerRadius(
            figure.Edges.Top.Start.X - left,
            right - figure.Edges.Top.End.X,
            right - figure.Edges.Bottom.Start.X,
            figure.Edges.Bottom.End.X - left);
    }

    private void Draw(IBorderDrawingSession session, Size size, double? rasterizationScale)
    {
        var sideBrushes = new Sides<Brush?>(this.LeftBrush, this.TopBrush, this.RightBrush, this.BottomBrush);
        BorderRenderer.Render(
            session,
            this.GetMetrics(size, rasterizationScale),
            Paint.From(this.Background),
            new Sides<Paint?>(
                Paint.From(sideBrushes.Left),
                Paint.From(sideBrushes.Top),
                Paint.From(sideBrushes.Right),
                Paint.From(sideBrushes.Bottom)),
            this.EnsureCornerBlends(sideBrushes),
            new Corners<Stops>(this.TopLeftStops, this.TopRightStops, this.BottomRightStops, this.BottomLeftStops));
    }

    private Corners<CornerBlend> EnsureCornerBlends(Sides<Brush?> sides)
    {
        if (this.cornerBlends.HasValue)
        {
            return this.cornerBlends.Value;
        }

        this.cornerBlends = new Corners<CornerBlend>(
            BrushResolver.ResolveBlend(sides.Left, sides.Top, Corner.TopLeft),
            BrushResolver.ResolveBlend(sides.Top, sides.Right, Corner.TopRight),
            BrushResolver.ResolveBlend(sides.Right, sides.Bottom, Corner.BottomRight),
            BrushResolver.ResolveBlend(sides.Bottom, sides.Left, Corner.BottomLeft));
        return this.cornerBlends.Value;
    }
}
