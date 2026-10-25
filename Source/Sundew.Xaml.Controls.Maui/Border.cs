// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Border.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using Microsoft.Maui.Controls.Shapes;
using Sundew.Xaml.Controls.Backends;
using DeviceScale = Sundew.Xaml.Controls.Scale;

/// <summary>
/// Provides a border control that supports independently customizable brushes and gradient stops for each side and corner, enabling advanced border rendering scenarios beyond the standard Border control.
/// </summary>
/// <remarks>
/// MAUI does not let a control paint itself, so this is a <see cref="Grid"/> holding two children in the one cell:
/// an <see cref="IBorderSurface"/> the border is drawn on, and a <see cref="ContentView"/> holding the content,
/// inset by the thickness. The renderer behind it is the same source the WinUI and Uno library runs.
/// </remarks>
[ContentProperty(nameof(Child))]
public class Border : Grid
{
    /// <summary>
    /// Static fields are initialized in declaration order, so this has to come before the properties that
    /// default to it.
    /// </summary>
    private static readonly Stops DefaultStops = new Stops(0.2, 0.8);

    /// <summary>
    /// Identifies the <see cref="Fill"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FillProperty = BindableProperty.Create(
        nameof(Fill), typeof(Brush), typeof(Border), null, propertyChanged: OnRedrawRequired);

    /// <summary>
    /// Identifies the <see cref="BorderThickness"/> bindable property.
    /// </summary>
    public static readonly BindableProperty BorderThicknessProperty = BindableProperty.Create(
        nameof(BorderThickness), typeof(Thickness), typeof(Border), default(Thickness), propertyChanged: OnRelayoutRequired);

    /// <summary>
    /// Identifies the <see cref="CornerRadius"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
        nameof(CornerRadius), typeof(CornerRadius), typeof(Border), default(CornerRadius), propertyChanged: OnRelayoutRequired);

    /// <summary>
    /// Identifies the <see cref="ContentPadding"/> bindable property.
    /// </summary>
    /// <remarks>
    /// This is not called Padding, because <see cref="Layout"/> already has one, which insets every child of the
    /// grid and so would move the drawing surface along with the content.
    /// </remarks>
    public static readonly BindableProperty ContentPaddingProperty = BindableProperty.Create(
        nameof(ContentPadding), typeof(Thickness), typeof(Border), default(Thickness), propertyChanged: OnRelayoutRequired);

    /// <summary>
    /// Identifies the <see cref="LeftBrush"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LeftBrushProperty = BindableProperty.Create(
        nameof(LeftBrush), typeof(Brush), typeof(Border), null, propertyChanged: OnFreshRenderRequired);

    /// <summary>
    /// Identifies the <see cref="TopBrush"/> bindable property.
    /// </summary>
    public static readonly BindableProperty TopBrushProperty = BindableProperty.Create(
        nameof(TopBrush), typeof(Brush), typeof(Border), null, propertyChanged: OnFreshRenderRequired);

    /// <summary>
    /// Identifies the <see cref="RightBrush"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RightBrushProperty = BindableProperty.Create(
        nameof(RightBrush), typeof(Brush), typeof(Border), null, propertyChanged: OnFreshRenderRequired);

    /// <summary>
    /// Identifies the <see cref="BottomBrush"/> bindable property.
    /// </summary>
    public static readonly BindableProperty BottomBrushProperty = BindableProperty.Create(
        nameof(BottomBrush), typeof(Brush), typeof(Border), null, propertyChanged: OnFreshRenderRequired);

    /// <summary>
    /// Identifies the <see cref="TopLeftStops"/> bindable property.
    /// </summary>
    public static readonly BindableProperty TopLeftStopsProperty = BindableProperty.Create(
        nameof(TopLeftStops), typeof(Stops), typeof(Border), DefaultStops, propertyChanged: OnFreshRenderRequired);

    /// <summary>
    /// Identifies the <see cref="TopRightStops"/> bindable property.
    /// </summary>
    public static readonly BindableProperty TopRightStopsProperty = BindableProperty.Create(
        nameof(TopRightStops), typeof(Stops), typeof(Border), DefaultStops, propertyChanged: OnFreshRenderRequired);

    /// <summary>
    /// Identifies the <see cref="BottomRightStops"/> bindable property.
    /// </summary>
    public static readonly BindableProperty BottomRightStopsProperty = BindableProperty.Create(
        nameof(BottomRightStops), typeof(Stops), typeof(Border), DefaultStops, propertyChanged: OnFreshRenderRequired);

    /// <summary>
    /// Identifies the <see cref="BottomLeftStops"/> bindable property.
    /// </summary>
    public static readonly BindableProperty BottomLeftStopsProperty = BindableProperty.Create(
        nameof(BottomLeftStops), typeof(Stops), typeof(Border), DefaultStops, propertyChanged: OnFreshRenderRequired);

    /// <summary>
    /// Identifies the <see cref="ClipContentToCornerRadius"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ClipContentToCornerRadiusProperty = BindableProperty.Create(
        nameof(ClipContentToCornerRadius), typeof(bool), typeof(Border), false, propertyChanged: OnRelayoutRequired);

    private readonly ContentView contentHost = new ContentView();
    private readonly IBorderSurface surface;
    private Corners<CornerBlend>? cornerBlends;

    /// <summary>
    /// Initializes a new instance of the <see cref="Border"/> class.
    /// </summary>
    public Border()
    {
        this.surface = BorderSurfaceFactory.Create((session, size, scale) => this.Draw(session, size, scale));
        this.Children.Add(this.surface.Element);
        this.Children.Add(this.contentHost);
    }

    /// <summary>
    /// Gets or sets the brush filling the area inside the border.
    /// </summary>
    /// <remarks>
    /// This is not the inherited Background, which MAUI paints itself as a plain rectangle behind the whole
    /// control and which would therefore show square corners outside the rounded border. The border paints this
    /// one on its own surface instead, following the corner radius and stopping at the inside of the ring.
    /// </remarks>
    public Brush? Fill
    {
        get => (Brush?)this.GetValue(FillProperty);
        set => this.SetValue(FillProperty, value);
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
    public Thickness ContentPadding
    {
        get => (Thickness)this.GetValue(ContentPaddingProperty);
        set => this.SetValue(ContentPaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the content of the border.
    /// </summary>
    public View? Child
    {
        get => this.contentHost.Content;
        set => this.contentHost.Content = value;
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
    /// The border normally draws onto its own surface. This lets it be drawn onto one belonging to someone else,
    /// which is what the development tester uses to capture a sheet of borders, because what a canvas has
    /// composited cannot be read back.
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
    protected override Size ArrangeOverride(Rect bounds)
    {
        var arranged = base.ArrangeOverride(bounds);
        this.UpdateContentHost(arranged);
        this.surface.Invalidate();
        return arranged;
    }

    private static void OnRedrawRequired(BindableObject bindable, object oldValue, object newValue)
    {
        (bindable as Border)?.surface.Invalidate();
    }

    private static void OnFreshRenderRequired(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Border border)
        {
            border.cornerBlends = null;
            border.surface.Invalidate();
        }
    }

    private static void OnRelayoutRequired(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Border border)
        {
            border.ApplyInsets();
            border.InvalidateMeasure();
        }
    }

    private void ApplyInsets()
    {
        // The grid lays both children over the same cell, so the content is inset from the border by giving it a
        // margin of the thickness rather than by arranging it separately. The surface keeps the whole cell.
        this.contentHost.Margin = this.BorderThickness;
        this.contentHost.Padding = this.ContentPadding;
    }

    private BorderMetrics GetMetrics(Size size, double? rasterizationScale = null)
    {
        return new BorderMetrics(
            size.Width,
            size.Height,
            this.BorderThickness.ToSides(),
            this.CornerRadius.ToCorners(),
            DeviceScale.Uniform(rasterizationScale ?? 1.0));
    }

    private void UpdateContentHost(Size size)
    {
        var thickness = this.BorderThickness;
        var contentWidth = Math.Max(0, size.Width - thickness.HorizontalThickness);
        var contentHeight = Math.Max(0, size.Height - thickness.VerticalThickness);

        // Unlike WinUI, MAUI clips to any shape, so the rounded clip is stated directly rather than left to the
        // corner radius of a plain border holding the content.
        this.contentHost.Clip = this.ClipContentToCornerRadius
            ? new RoundRectangleGeometry(this.GetInnerCornerRadius(size), new Rect(0, 0, contentWidth, contentHeight))
            : new RectangleGeometry(new Rect(0, 0, contentWidth, contentHeight));
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

        // MAUI orders a corner radius top left, top right, bottom left, bottom right, where WinUI goes round the
        // other way.
        return new CornerRadius(
            figure.Edges.Top.Start.X - left,
            right - figure.Edges.Top.End.X,
            figure.Edges.Bottom.End.X - left,
            right - figure.Edges.Bottom.Start.X);
    }

    private void Draw(IBorderDrawingSession session, Size size, double? rasterizationScale)
    {
        var sideBrushes = new Sides<Brush?>(this.LeftBrush, this.TopBrush, this.RightBrush, this.BottomBrush);
        BorderRenderer.Render(
            session,
            this.GetMetrics(size, rasterizationScale),
            BrushResolver.ResolvePaint(this.Fill),
            new Sides<Paint?>(
                BrushResolver.ResolvePaint(sideBrushes.Left),
                BrushResolver.ResolvePaint(sideBrushes.Top),
                BrushResolver.ResolvePaint(sideBrushes.Right),
                BrushResolver.ResolvePaint(sideBrushes.Bottom)),
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
