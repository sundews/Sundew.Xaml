// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Border.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System.Windows;
using System.Windows.Media;

/// <summary>
/// Provides a border control that supports independently customizable brushes and gradient stops for each side and corner, enabling advanced border rendering scenarios beyond the standard Border control.
/// </summary>
public class Border : System.Windows.Controls.Border
{
    private static readonly Pen BackgroundPen = new Pen(null, 0);
    private static readonly Stops DefaultStops = new Stops(0.2, 0.8);

    /// <summary>
    /// Identifies the LeftBrush dependency property.
    /// </summary>
    public static readonly DependencyProperty LeftBrushProperty = DependencyProperty.Register(
        nameof(LeftBrush), typeof(Brush), typeof(Border), new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender, OnFreshRenderRequired));

    /// <summary>
    /// Identifies the TopBrush dependency property.
    /// </summary>
    public static readonly DependencyProperty TopBrushProperty = DependencyProperty.Register(
        nameof(TopBrush), typeof(Brush), typeof(Border), new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender, OnFreshRenderRequired));

    /// <summary>
    /// Identifies the RightBrush dependency property.
    /// </summary>
    public static readonly DependencyProperty RightBrushProperty = DependencyProperty.Register(
        nameof(RightBrush), typeof(Brush), typeof(Border), new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender, OnFreshRenderRequired));

    /// <summary>
    /// Identifies the BottomBrush dependency property.
    /// </summary>
    public static readonly DependencyProperty BottomBrushProperty = DependencyProperty.Register(
        nameof(BottomBrush), typeof(Brush), typeof(Border), new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender, OnFreshRenderRequired));

    /// <summary>
    /// Identifies the TopLeftStops dependency property.
    /// </summary>
    public static readonly DependencyProperty TopLeftStopsProperty = DependencyProperty.Register(
        nameof(TopLeftStops), typeof(Stops), typeof(Border), new FrameworkPropertyMetadata(
            DefaultStops,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
            OnFreshRenderRequired));

    /// <summary>
    /// Identifies the TopRightStops dependency property.
    /// </summary>
    public static readonly DependencyProperty TopRightStopsProperty = DependencyProperty.Register(
        nameof(TopRightStops), typeof(Stops), typeof(Border), new FrameworkPropertyMetadata(
            DefaultStops,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
            OnFreshRenderRequired));

    /// <summary>
    /// Identifies the BottomRightStops dependency property.
    /// </summary>
    public static readonly DependencyProperty BottomRightStopsProperty = DependencyProperty.Register(
        nameof(BottomRightStops), typeof(Stops), typeof(Border), new FrameworkPropertyMetadata(
            DefaultStops,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
            OnFreshRenderRequired));

    /// <summary>
    /// Identifies the BottomLeftStops dependency property.
    /// </summary>
    public static readonly DependencyProperty BottomLeftStopsProperty = DependencyProperty.Register(
        nameof(BottomLeftStops), typeof(Stops), typeof(Border), new FrameworkPropertyMetadata(
            DefaultStops,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
            OnFreshRenderRequired));

    /// <summary>
    /// Identifies the ClipContentToCornerRadius dependency property.
    /// </summary>
    /// <remarks>
    /// The clip is applied to the <see cref="System.Windows.Controls.Decorator.Child"/> rather than to the border itself,
    /// so that an <see cref="System.Windows.UIElement.Effect"/> on the border, such as a drop shadow or a glow,
    /// still renders outside the border's bounds.
    /// </remarks>
    public static readonly DependencyProperty ClipContentToCornerRadiusProperty = DependencyProperty.Register(
        nameof(ClipContentToCornerRadius),
        typeof(bool),
        typeof(Border),
        new FrameworkPropertyMetadata(false, OnClipContentToCornerRadiusChanged));

    private Corners<CornerBlend>? cornerBlends = null;

    static Border()
    {
        BackgroundPen.Freeze();
    }

    /// <summary>
    /// Gets or sets the side brushes.
    /// </summary>
    public SideBrushes? SideBrushes
    {
        get => new SideBrushes(this.LeftBrush, this.TopBrush, this.RightBrush, this.BottomBrush);
        set
        {
            if (value == null)
            {
                this.LeftBrush = null;
                this.TopBrush = null;
                this.RightBrush = null;
                this.BottomBrush = null;
                return;
            }

            this.LeftBrush = value.Left;
            this.TopBrush = value.Top;
            this.RightBrush = value.Right;
            this.BottomBrush = value.Bottom;
        }
    }

    /// <summary>
    /// Gets or sets the corner stops.
    /// </summary>
    public CornerStops CornerStops
    {
        get => new(this.TopLeftStops, this.TopRightStops, this.BottomRightStops, this.BottomLeftStops);
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
    /// Gets or sets a value indicating whether the content is clipped to the <see cref="System.Windows.Controls.Border.CornerRadius"/>.
    /// </summary>
    /// <remarks>
    /// While this is enabled, the border owns the <see cref="System.Windows.UIElement.Clip"/> of its
    /// <see cref="System.Windows.Controls.Decorator.Child"/>.
    /// </remarks>
    public bool ClipContentToCornerRadius
    {
        get => (bool)this.GetValue(ClipContentToCornerRadiusProperty);
        set => this.SetValue(ClipContentToCornerRadiusProperty, value);
    }

    /// <summary>
    /// Renders the border.
    /// </summary>
    /// <param name="drawingContext">The drawing context.</param>
    protected override void OnRender(DrawingContext drawingContext)
    {
        var metrics = this.GetMetrics();
        var layout = BorderLayoutCalculator.CalculateBorder(metrics);
        var sideBrushes = new Sides<Brush?>(this.LeftBrush, this.TopBrush, this.RightBrush, this.BottomBrush);
        var cornerBlends = this.EnsureCornerBlends(sideBrushes);

        var drawingGroup = new DrawingGroup { GuidelineSet = layout.Guidelines.ToGuidelineSet() };

        drawingGroup.Children.Add(new GeometryDrawing(this.Background, BackgroundPen, layout.Background.ToGeometry()));

        // A see through corner is composited as a whole, so a side running under it would show through as a darker
        // band along the overlap. Those corners keep the plain shared cut instead.
        var sides = AreCornersOpaque(cornerBlends)
            ? new Sides<SideQuad>(
                layout.Sides.Left.ExtendIntoCorners(metrics.Scale),
                layout.Sides.Top.ExtendIntoCorners(metrics.Scale),
                layout.Sides.Right.ExtendIntoCorners(metrics.Scale),
                layout.Sides.Bottom.ExtendIntoCorners(metrics.Scale))
            : layout.Sides;

        AddSide(drawingGroup, sides.Left, sideBrushes.Left);
        AddSide(drawingGroup, sides.Top, sideBrushes.Top);
        AddSide(drawingGroup, sides.Right, sideBrushes.Right);
        AddSide(drawingGroup, sides.Bottom, sideBrushes.Bottom);

        AddCorner(drawingGroup, layout.Corners.TopLeft, cornerBlends.TopLeft, this.TopLeftStops, metrics.Scale);
        AddCorner(drawingGroup, layout.Corners.TopRight, cornerBlends.TopRight, this.TopRightStops, metrics.Scale);
        AddCorner(drawingGroup, layout.Corners.BottomRight, cornerBlends.BottomRight, this.BottomRightStops, metrics.Scale);
        AddCorner(drawingGroup, layout.Corners.BottomLeft, cornerBlends.BottomLeft, this.BottomLeftStops, metrics.Scale);

        drawingContext.DrawDrawing(drawingGroup);
        this.UpdateContentClip();
    }

    private static bool AreCornersOpaque(Corners<CornerBlend> blends)
    {
        return blends.TopLeft.IsOpaque && blends.TopRight.IsOpaque && blends.BottomRight.IsOpaque && blends.BottomLeft.IsOpaque;
    }

    private static void AddSide(DrawingGroup drawingGroup, SideQuad sideQuad, Brush? brush)
    {
        if (brush == null || !sideQuad.HasArea)
        {
            return;
        }

        drawingGroup.Children.Add(new GeometryDrawing(brush, BackgroundPen, sideQuad.ToGeometry()));
    }

    /// <summary>
    /// Sweeps the blend between the two sides meeting at a corner around that corner, a slice at a time.
    /// </summary>
    /// <remarks>
    /// One linear gradient across the whole corner cannot do this without a seam: its bands of constant colour would
    /// have to run along the cut it shares with each of the two sides, and those two cuts are at right angles. Each
    /// slice is narrow enough that one gradient across it is indistinguishable from a sweep, and the slices at either
    /// end come out as the side's own colour along the whole of the cut.
    /// </remarks>
    /// <param name="drawingGroup">The drawing group.</param>
    /// <param name="cornerWedge">The corner.</param>
    /// <param name="blend">The blend between the two sides meeting at the corner.</param>
    /// <param name="stops">The stops controlling how abrupt the transition is.</param>
    /// <param name="scale">The device pixel scale, which decides how fine the sweep has to be.</param>
    private static void AddCorner(DrawingGroup drawingGroup, CornerWedge cornerWedge, CornerBlend blend, Stops stops, Scale scale)
    {
        if (!blend.IsBlended)
        {
            if (blend.Brush != null)
            {
                drawingGroup.Children.Add(new GeometryDrawing(blend.Brush, BackgroundPen, cornerWedge.ToGeometry()));
            }

            return;
        }

        // Where the slices overlap they would blend with each other, which a see through border would show as a band
        // at every seam. Taking the alpha off the slices and giving it to a group of their own composites the whole
        // corner once, at which point the overlaps are opaque and leave nothing behind.
        var target = drawingGroup;
        if (blend.Opacity < 1)
        {
            target = new DrawingGroup { Opacity = blend.Opacity };
            drawingGroup.Children.Add(target);
        }

        var count = cornerWedge.GetSliceCount(scale);
        for (var index = 0; index < count; index++)
        {
            var from = (double)index / count;
            var to = (double)(index + 1) / count;
            var slice = cornerWedge.GetSlice(from, to);

            // The slice is filled a little past its own end, and the next one then paints over that overrun.
            var filled = cornerWedge.GetSlice(from, System.Math.Min(1, to + (CornerWedge.SliceOverlap / count)));
            target.Children.Add(new GeometryDrawing(
                CreateSliceBrush(slice, blend, stops, from, to),
                BackgroundPen,
                filled.ToGeometry()));
        }
    }

    private static Brush CreateSliceBrush(CornerWedge slice, CornerBlend blend, Stops stops, double from, double to)
    {
        var startColor = blend.ColorAt(stops, from);
        var endColor = blend.ColorAt(stops, to);
        Brush brush = startColor == endColor
            ? new SolidColorBrush(startColor)
            : new LinearGradientBrush(
                startColor,
                endColor,
                slice.StartCutCentre.ToPoint(),
                slice.EndCutCentre.ToPoint())
            {
                MappingMode = BrushMappingMode.Absolute,
            };
        brush.Freeze();
        return brush;
    }

    private static Color GetStartColor(LinearGradientBrush brush, bool reverse)
    {
        if (brush.GradientStops.Count == 0)
        {
            return Colors.Transparent;
        }

        return reverse ? brush.GradientStops[brush.GradientStops.Count - 1].Color : brush.GradientStops[0].Color;
    }

    private static Color GetEndColor(LinearGradientBrush brush, bool reverse)
    {
        if (brush.GradientStops.Count == 0)
        {
            return Colors.Transparent;
        }

        return reverse ? brush.GradientStops[0].Color : brush.GradientStops[brush.GradientStops.Count - 1].Color;
    }

    private static void OnClipContentToCornerRadiusChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not Border border)
        {
            return;
        }

        if (e.NewValue is true)
        {
            border.UpdateContentClip();
        }
        else
        {
            border.Child?.ClearValue(ClipProperty);
        }
    }

    private static void OnFreshRenderRequired(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Border border)
        {
            border.ClearCache();
        }
    }

    private BorderMetrics GetMetrics()
    {
        return new BorderMetrics(
            this.ActualWidth,
            this.ActualHeight,
            this.BorderThickness.ToSides(),
            this.CornerRadius.ToCorners(),
            VisualTreeHelper.GetDpi(this).ToScale());
    }

    private void UpdateContentClip()
    {
        if (!this.ClipContentToCornerRadius)
        {
            return;
        }

        var child = this.Child;
        if (child == null)
        {
            return;
        }

        child.Clip = this.CreateContentClipGeometry();
    }

    /// <summary>
    /// Creates the geometry of the area inside the border's stroke, expressed in the child's coordinate space.
    /// </summary>
    /// <returns>The geometry, or <see cref="Geometry.Empty"/> when there is no room for content.</returns>
    private Geometry CreateContentClipGeometry()
    {
        var metrics = this.GetMetrics();
        if (!BorderLayoutCalculator.TryCalculateContentClip(metrics, this.Padding.ToSides(), out var figure))
        {
            return Geometry.Empty;
        }

        var geometry = figure.ToGeometry();
        geometry.Freeze();
        return geometry;
    }

    private void ClearCache()
    {
        this.cornerBlends = null;
    }

    private Corners<CornerBlend> EnsureCornerBlends(Sides<Brush?> sides)
    {
        if (this.cornerBlends.HasValue)
        {
            return this.cornerBlends.Value;
        }

        this.cornerBlends = new Corners<CornerBlend>(
            GetCornerBlend(sides.Left, sides.Top, Corner.TopLeft),
            GetCornerBlend(sides.Top, sides.Right, Corner.TopRight),
            GetCornerBlend(sides.Right, sides.Bottom, Corner.BottomRight),
            GetCornerBlend(sides.Bottom, sides.Left, Corner.BottomLeft));
        return this.cornerBlends.Value;
    }

    /// <summary>
    /// Works out the two colours the side entering a corner is blended from and the side leaving it is blended to.
    /// </summary>
    /// <param name="first">The brush of the side entering the corner.</param>
    /// <param name="second">The brush of the side leaving the corner.</param>
    /// <param name="corner">The corner.</param>
    /// <returns>The blend, or <paramref name="first"/> on its own when the two brushes cannot be blended.</returns>
    private static CornerBlend GetCornerBlend(Brush? first, Brush? second, Corner corner)
    {
        var gradient = CornerGradient.For(corner);
        switch (first, second)
        {
            case (SolidColorBrush firstBrush, SolidColorBrush secondBrush):
                return new CornerBlend(firstBrush.Color, secondBrush.Color);
            case (SolidColorBrush firstBrush, LinearGradientBrush secondBrush):
                return new CornerBlend(firstBrush.Color, GetEndColor(secondBrush, gradient.ReverseSecond));
            case (LinearGradientBrush firstBrush, SolidColorBrush secondBrush):
                return new CornerBlend(GetStartColor(firstBrush, gradient.ReverseFirst), secondBrush.Color);
            case (LinearGradientBrush firstBrush, LinearGradientBrush secondBrush):
                return new CornerBlend(
                    GetStartColor(firstBrush, gradient.ReverseFirst),
                    GetEndColor(secondBrush, gradient.ReverseSecond));
            default:
                return new CornerBlend(first);
        }
    }

    /// <summary>
    /// The two colours a corner is blended between, or the brush to fall back to when the two sides meeting there
    /// cannot be blended.
    /// </summary>
    private readonly record struct CornerBlend
    {
        private readonly Color start;
        private readonly Color end;

        public CornerBlend(Color start, Color end)
        {
            this.start = start;
            this.end = end;
            this.Brush = null;
            this.IsBlended = true;
        }

        public CornerBlend(Brush? brush)
        {
            this.start = default;
            this.end = default;
            this.Brush = brush;
            this.IsBlended = false;
        }

        public Brush? Brush { get; }

        public bool IsBlended { get; }

        /// <summary>
        /// Gets a value indicating whether the corner is drawn with no transparency, in which case a side may run
        /// underneath it without the overlap showing.
        /// </summary>
        public bool IsOpaque => this.IsBlended && this.start.A == byte.MaxValue && this.end.A == byte.MaxValue;

        /// <summary>
        /// Gets the alpha the corner as a whole is drawn at, which is that of whichever of the two colours is the
        /// more opaque, so that <see cref="ColorAt"/> never has to raise one of them.
        /// </summary>
        public double Opacity => System.Math.Max(this.start.A, this.end.A) / 255.0;

        public Color ColorAt(Stops stops, double position)
        {
            var amount = stops.GetBlendAmount(position);
            var peak = System.Math.Max(this.start.A, this.end.A);
            return Color.FromArgb(
                peak == 0 ? (byte)0 : (byte)System.Math.Round(Channel(this.start.A, this.end.A, amount) * 255.0 / peak),
                Channel(this.start.R, this.end.R, amount),
                Channel(this.start.G, this.end.G, amount),
                Channel(this.start.B, this.end.B, amount));
        }

        private static byte Channel(byte start, byte end, double amount)
        {
            return (byte)System.Math.Round(start + ((end - start) * amount));
        }
    }
}
