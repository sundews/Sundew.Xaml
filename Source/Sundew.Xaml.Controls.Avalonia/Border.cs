// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Border.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using global::Avalonia.Media.Immutable;

/// <summary>
/// Provides a border control that supports independently customizable brushes and gradient stops for each side and corner, enabling advanced border rendering scenarios beyond the standard Border control.
/// </summary>
/// <remarks>
/// This derives from <see cref="Decorator"/> rather than from <see cref="global::Avalonia.Controls.Border"/>, because
/// the latter seals its rendering. The properties it shares with the standard border are the very same
/// <see cref="AvaloniaProperty"/> instances, so styles and theme resources written against the standard border apply
/// to this one too.
/// </remarks>
public class Border : Decorator
{
    /// <summary>
    /// How far a transform may be off axis before it is treated as rotated or skewed, which leaves no grid of pixels
    /// to line the geometry up with.
    /// </summary>
    private const double AxisAlignedTolerance = 0.0001;

    /// <summary>
    /// Static fields are initialized in declaration order, so this has to come before the properties defaulting to it.
    /// </summary>
    private static readonly Stops DefaultStops = new Stops(0.2, 0.8);

    /// <summary>
    /// Identifies the <see cref="Background"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        global::Avalonia.Controls.Border.BackgroundProperty.AddOwner<Border>();

    /// <summary>
    /// Identifies the <see cref="BorderThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        global::Avalonia.Controls.Border.BorderThicknessProperty.AddOwner<Border>();

    /// <summary>
    /// Identifies the <see cref="CornerRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        global::Avalonia.Controls.Border.CornerRadiusProperty.AddOwner<Border>();

    /// <summary>
    /// Identifies the <see cref="LeftBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> LeftBrushProperty =
        AvaloniaProperty.Register<Border, IBrush?>(nameof(LeftBrush));

    /// <summary>
    /// Identifies the <see cref="TopBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> TopBrushProperty =
        AvaloniaProperty.Register<Border, IBrush?>(nameof(TopBrush));

    /// <summary>
    /// Identifies the <see cref="RightBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> RightBrushProperty =
        AvaloniaProperty.Register<Border, IBrush?>(nameof(RightBrush));

    /// <summary>
    /// Identifies the <see cref="BottomBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BottomBrushProperty =
        AvaloniaProperty.Register<Border, IBrush?>(nameof(BottomBrush));

    /// <summary>
    /// Identifies the <see cref="TopLeftStops"/> property.
    /// </summary>
    public static readonly StyledProperty<Stops> TopLeftStopsProperty =
        AvaloniaProperty.Register<Border, Stops>(nameof(TopLeftStops), DefaultStops);

    /// <summary>
    /// Identifies the <see cref="TopRightStops"/> property.
    /// </summary>
    public static readonly StyledProperty<Stops> TopRightStopsProperty =
        AvaloniaProperty.Register<Border, Stops>(nameof(TopRightStops), DefaultStops);

    /// <summary>
    /// Identifies the <see cref="BottomRightStops"/> property.
    /// </summary>
    public static readonly StyledProperty<Stops> BottomRightStopsProperty =
        AvaloniaProperty.Register<Border, Stops>(nameof(BottomRightStops), DefaultStops);

    /// <summary>
    /// Identifies the <see cref="BottomLeftStops"/> property.
    /// </summary>
    public static readonly StyledProperty<Stops> BottomLeftStopsProperty =
        AvaloniaProperty.Register<Border, Stops>(nameof(BottomLeftStops), DefaultStops);

    /// <summary>
    /// Identifies the <see cref="SideBrushes"/> property.
    /// </summary>
    public static readonly DirectProperty<Border, SideBrushes?> SideBrushesProperty =
        AvaloniaProperty.RegisterDirect<Border, SideBrushes?>(
            nameof(SideBrushes),
            border => border.SideBrushes,
            (border, value) => border.SideBrushes = value);

    /// <summary>
    /// Identifies the <see cref="CornerStops"/> property.
    /// </summary>
    public static readonly DirectProperty<Border, CornerStops> CornerStopsProperty =
        AvaloniaProperty.RegisterDirect<Border, CornerStops>(
            nameof(CornerStops),
            border => border.CornerStops,
            (border, value) => border.CornerStops = value);

    /// <summary>
    /// Identifies the <see cref="ClipContentToCornerRadius"/> property.
    /// </summary>
    /// <remarks>
    /// The clip is applied to the <see cref="global::Avalonia.Controls.Decorator.Child"/> rather than to the border
    /// itself, so that a <see cref="Visual.Effect"/> on the border, such as a drop shadow or a glow, still renders
    /// outside the bounds of the border.
    /// </remarks>
    public static readonly StyledProperty<bool> ClipContentToCornerRadiusProperty =
        AvaloniaProperty.Register<Border, bool>(nameof(ClipContentToCornerRadius));

    private Corners<CornerBlend>? cornerBlends;

    static Border()
    {
        AffectsRender<Border>(
            BackgroundProperty,
            BorderThicknessProperty,
            CornerRadiusProperty,
            LeftBrushProperty,
            TopBrushProperty,
            RightBrushProperty,
            BottomBrushProperty,
            TopLeftStopsProperty,
            TopRightStopsProperty,
            BottomRightStopsProperty,
            BottomLeftStopsProperty);

        // The content clip is produced during arrange, because Avalonia forbids invalidating a visual while rendering.
        AffectsArrange<Border>(CornerRadiusProperty, ClipContentToCornerRadiusProperty);
        AffectsMeasure<Border>(
            BorderThicknessProperty,
            TopLeftStopsProperty,
            TopRightStopsProperty,
            BottomRightStopsProperty,
            BottomLeftStopsProperty);

        LeftBrushProperty.Changed.AddClassHandler<Border>(OnFreshRenderRequired);
        TopBrushProperty.Changed.AddClassHandler<Border>(OnFreshRenderRequired);
        RightBrushProperty.Changed.AddClassHandler<Border>(OnFreshRenderRequired);
        BottomBrushProperty.Changed.AddClassHandler<Border>(OnFreshRenderRequired);
        TopLeftStopsProperty.Changed.AddClassHandler<Border>(OnFreshRenderRequired);
        TopRightStopsProperty.Changed.AddClassHandler<Border>(OnFreshRenderRequired);
        BottomRightStopsProperty.Changed.AddClassHandler<Border>(OnFreshRenderRequired);
        BottomLeftStopsProperty.Changed.AddClassHandler<Border>(OnFreshRenderRequired);
        ClipContentToCornerRadiusProperty.Changed.AddClassHandler<Border>(OnClipContentToCornerRadiusChanged);
    }

    /// <summary>
    /// Gets or sets the brush filling the area inside the border.
    /// </summary>
    public IBrush? Background
    {
        get => this.GetValue(BackgroundProperty);
        set => this.SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness of each side of the border.
    /// </summary>
    public Thickness BorderThickness
    {
        get => this.GetValue(BorderThicknessProperty);
        set => this.SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the radius of each corner of the border.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => this.GetValue(CornerRadiusProperty);
        set => this.SetValue(CornerRadiusProperty, value);
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
    public IBrush? LeftBrush
    {
        get => this.GetValue(LeftBrushProperty);
        set => this.SetValue(LeftBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush for the top of the border.
    /// </summary>
    public IBrush? TopBrush
    {
        get => this.GetValue(TopBrushProperty);
        set => this.SetValue(TopBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush for the right side of the border.
    /// </summary>
    public IBrush? RightBrush
    {
        get => this.GetValue(RightBrushProperty);
        set => this.SetValue(RightBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush for the bottom of the border.
    /// </summary>
    public IBrush? BottomBrush
    {
        get => this.GetValue(BottomBrushProperty);
        set => this.SetValue(BottomBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the gradient stops for the top-left corner.
    /// </summary>
    public Stops TopLeftStops
    {
        get => this.GetValue(TopLeftStopsProperty);
        set => this.SetValue(TopLeftStopsProperty, value);
    }

    /// <summary>
    /// Gets or sets the gradient stops for the top-right corner.
    /// </summary>
    public Stops TopRightStops
    {
        get => this.GetValue(TopRightStopsProperty);
        set => this.SetValue(TopRightStopsProperty, value);
    }

    /// <summary>
    /// Gets or sets the gradient stops for the bottom-right corner.
    /// </summary>
    public Stops BottomRightStops
    {
        get => this.GetValue(BottomRightStopsProperty);
        set => this.SetValue(BottomRightStopsProperty, value);
    }

    /// <summary>
    /// Gets or sets the gradient stops for the bottom-left corner.
    /// </summary>
    public Stops BottomLeftStops
    {
        get => this.GetValue(BottomLeftStopsProperty);
        set => this.SetValue(BottomLeftStopsProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the content is clipped to the <see cref="global::Avalonia.Controls.Border.CornerRadius"/>.
    /// </summary>
    /// <remarks>
    /// The border owns the <see cref="Visual.Clip"/> of its
    /// <see cref="global::Avalonia.Controls.Decorator.Child"/> either way: the content is always held to the room it
    /// was given, as WPF's layout clip does, and this decides whether that clip follows the corners or is the plain
    /// content rectangle.
    /// </remarks>
    public bool ClipContentToCornerRadius
    {
        get => this.GetValue(ClipContentToCornerRadiusProperty);
        set => this.SetValue(ClipContentToCornerRadiusProperty, value);
    }

    /// <summary>
    /// Renders the border.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    public override void Render(DrawingContext context)
    {
        var alignment = this.GetDeviceAlignment();
        var metrics = this.GetMetrics(this.Bounds.Size, alignment.Scale);
        var layout = BorderLayoutCalculator.CalculateBorder(metrics);
        var sideBrushes = new Sides<IBrush?>(this.LeftBrush, this.TopBrush, this.RightBrush, this.BottomBrush);
        var cornerBlends = this.EnsureCornerBlends(sideBrushes);

        // Avalonia has no GuidelineSet, so the nudge that puts the geometry onto whole device pixels is applied here.
        using var snap = context.PushTransform(Matrix.CreateTranslation(alignment.Offset.X, alignment.Offset.Y));

        context.DrawGeometry(this.Background, null, layout.Background.ToGeometry());

        // A see through corner is composited as a whole, so a side running under it would show through as a darker
        // band along the overlap. Those corners keep the plain shared cut instead.
        var sides = AreCornersOpaque(cornerBlends)
            ? new Sides<SideQuad>(
                layout.Sides.Left.ExtendIntoCorners(metrics.Scale),
                layout.Sides.Top.ExtendIntoCorners(metrics.Scale),
                layout.Sides.Right.ExtendIntoCorners(metrics.Scale),
                layout.Sides.Bottom.ExtendIntoCorners(metrics.Scale))
            : layout.Sides;

        DrawSide(context, sides.Left, sideBrushes.Left);
        DrawSide(context, sides.Top, sideBrushes.Top);
        DrawSide(context, sides.Right, sideBrushes.Right);
        DrawSide(context, sides.Bottom, sideBrushes.Bottom);

        DrawCorner(context, layout.Corners.TopLeft, cornerBlends.TopLeft, this.TopLeftStops, metrics.Scale);
        DrawCorner(context, layout.Corners.TopRight, cornerBlends.TopRight, this.TopRightStops, metrics.Scale);
        DrawCorner(context, layout.Corners.BottomRight, cornerBlends.BottomRight, this.BottomRightStops, metrics.Scale);
        DrawCorner(context, layout.Corners.BottomLeft, cornerBlends.BottomLeft, this.BottomLeftStops, metrics.Scale);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        return LayoutHelper.MeasureChild(this.Child, availableSize, this.Padding, this.BorderThickness);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = LayoutHelper.ArrangeChild(this.Child, finalSize, this.Padding, this.BorderThickness);

        // Bounds is only assigned once arrange returns, so the clip has to be built from the size just arranged to.
        this.UpdateContentClip(size);
        return size;
    }

    private static bool AreCornersOpaque(Corners<CornerBlend> blends)
    {
        return blends.TopLeft.IsOpaque && blends.TopRight.IsOpaque && blends.BottomRight.IsOpaque && blends.BottomLeft.IsOpaque;
    }

    private static void DrawSide(DrawingContext context, SideQuad sideQuad, IBrush? brush)
    {
        if (brush == null || !sideQuad.HasArea)
        {
            return;
        }

        context.DrawGeometry(brush, null, sideQuad.ToGeometry());
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
    /// <param name="context">The drawing context.</param>
    /// <param name="cornerWedge">The corner.</param>
    /// <param name="blend">The blend between the two sides meeting at the corner.</param>
    /// <param name="stops">The stops controlling how abrupt the transition is.</param>
    /// <param name="scale">The device pixel scale, which decides how fine the sweep has to be.</param>
    private static void DrawCorner(DrawingContext context, CornerWedge cornerWedge, CornerBlend blend, Stops stops, Scale scale)
    {
        if (!blend.IsBlended)
        {
            if (blend.Brush != null)
            {
                context.DrawGeometry(blend.Brush, null, cornerWedge.ToGeometry());
            }

            return;
        }

        // Where the slices overlap they would blend with each other, which a see through border would show as a band
        // at every seam. Taking the alpha off the slices and applying it to the corner as a whole composites it once,
        // at which point the overlaps are opaque and leave nothing behind. It has to be a mask rather than
        // PushOpacity, because that one multiplies each slice on its own instead of making a layer of them.
        var layer = blend.Opacity < 1
            ? context.PushOpacityMask(new ImmutableSolidColorBrush(Colors.White, blend.Opacity), GetExtent(cornerWedge))
            : default(DrawingContext.PushedState?);
        try
        {
            var count = cornerWedge.GetSliceCount(scale);
            for (var index = 0; index < count; index++)
            {
                var from = (double)index / count;
                var to = (double)(index + 1) / count;
                var slice = cornerWedge.GetSlice(from, to);

                // The slice is filled a little past its own end, and the next one then paints over that overrun.
                var filled = cornerWedge.GetSlice(from, System.Math.Min(1, to + (CornerWedge.SliceOverlap / count)));
                context.DrawGeometry(CreateSliceBrush(slice, blend, stops, from, to), null, filled.ToGeometry());
            }
        }
        finally
        {
            layer?.Dispose();
        }
    }

    /// <summary>
    /// Gets the box a corner fits in, which every arc does because a quadratic stays within its three points.
    /// </summary>
    /// <param name="cornerWedge">The corner.</param>
    /// <returns>The box.</returns>
    private static Rect GetExtent(CornerWedge cornerWedge)
    {
        var left = Min(cornerWedge.Outer.Start.X, cornerWedge.Outer.Control.X, cornerWedge.Outer.End.X, cornerWedge.Inner.Control.X);
        var top = Min(cornerWedge.Outer.Start.Y, cornerWedge.Outer.Control.Y, cornerWedge.Outer.End.Y, cornerWedge.Inner.Control.Y);
        var right = Max(cornerWedge.Outer.Start.X, cornerWedge.Outer.Control.X, cornerWedge.Outer.End.X, cornerWedge.Inner.Control.X);
        var bottom = Max(cornerWedge.Outer.Start.Y, cornerWedge.Outer.Control.Y, cornerWedge.Outer.End.Y, cornerWedge.Inner.Control.Y);
        return new Rect(left, top, right - left, bottom - top);

        static double Min(double first, double second, double third, double fourth)
        {
            return System.Math.Min(System.Math.Min(first, second), System.Math.Min(third, fourth));
        }

        static double Max(double first, double second, double third, double fourth)
        {
            return System.Math.Max(System.Math.Max(first, second), System.Math.Max(third, fourth));
        }
    }

    private static IBrush CreateSliceBrush(CornerWedge slice, CornerBlend blend, Stops stops, double from, double to)
    {
        var startColor = blend.ColorAt(stops, from);
        var endColor = blend.ColorAt(stops, to);
        if (startColor == endColor)
        {
            return new ImmutableSolidColorBrush(startColor);
        }

        return new LinearGradientBrush
        {
            StartPoint = slice.StartCutCentre.ToAbsolutePoint(),
            EndPoint = slice.EndCutCentre.ToAbsolutePoint(),
            GradientStops =
            [
                new GradientStop(startColor, 0),
                new GradientStop(endColor, 1),
            ],
        }.ToImmutable();
    }

    private static Color GetStartColor(IGradientBrush brush, bool reverse)
    {
        var stops = brush.GradientStops;
        if (stops.Count == 0)
        {
            return Colors.Transparent;
        }

        return reverse ? stops[stops.Count - 1].Color : stops[0].Color;
    }

    private static Color GetEndColor(IGradientBrush brush, bool reverse)
    {
        var stops = brush.GradientStops;
        if (stops.Count == 0)
        {
            return Colors.Transparent;
        }

        return reverse ? stops[0].Color : stops[stops.Count - 1].Color;
    }

    private static void OnFreshRenderRequired(Border border, AvaloniaPropertyChangedEventArgs e)
    {
        border.ClearCache();
    }

    private static void OnClipContentToCornerRadiusChanged(Border border, AvaloniaPropertyChangedEventArgs e)
    {
        border.UpdateContentClip(border.Bounds.Size);
    }

    private BorderMetrics GetMetrics(Size size, Scale scale)
    {
        return new BorderMetrics(
            size.Width,
            size.Height,
            this.BorderThickness.ToSides(),
            this.CornerRadius.ToCorners(),
            scale);
    }

    /// <summary>
    /// Works out how many device pixels one unit of this border's own coordinate space covers, and how far the border
    /// has to move for its own origin to sit on a whole device pixel.
    /// </summary>
    /// <remarks>
    /// <see cref="TopLevel.RenderScaling"/> on its own is not enough. Anything scaling the border from above, a zoom
    /// through a <see cref="global::Avalonia.Controls.LayoutTransformControl"/> say, is applied after the border has
    /// drawn itself, so geometry rounded against the render scaling alone is then multiplied by that zoom and lands
    /// between pixels. Rounding against the whole scale puts the edges a whole number of device pixels apart, and the
    /// offset puts the first of them on a pixel boundary rather than part way across one. Together these are what
    /// WPF gets from a <c>GuidelineSet</c>, which Avalonia has no equivalent of.
    /// </remarks>
    /// <returns>The scale, and the offset to draw at.</returns>
    private DeviceAlignment GetDeviceAlignment()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var scaling = topLevel?.RenderScaling ?? 1.0;
        var unaligned = new DeviceAlignment(Scale.Uniform(scaling), default);
        if (topLevel == null)
        {
            return unaligned;
        }

        var transform = this.TransformToVisual(topLevel);
        if (!transform.HasValue)
        {
            return unaligned;
        }

        var matrix = transform.Value;

        // A rotation or a skew leaves no axis aligned grid of pixels to line the geometry up with.
        if (Math.Abs(matrix.M12) > AxisAlignedTolerance || Math.Abs(matrix.M21) > AxisAlignedTolerance)
        {
            return unaligned;
        }

        var scaleX = Math.Abs(matrix.M11) * scaling;
        var scaleY = Math.Abs(matrix.M22) * scaling;
        if (scaleX <= 0 || scaleY <= 0)
        {
            return unaligned;
        }

        var originX = matrix.M31 * scaling;
        var originY = matrix.M32 * scaling;
        return new DeviceAlignment(
            new Scale(scaleX, scaleY),
            new Position((Math.Round(originX) - originX) / scaleX, (Math.Round(originY) - originY) / scaleY));
    }

    /// <summary>
    /// Holds the content to the room the border gave it.
    /// </summary>
    /// <remarks>
    /// WPF clips a child to the space it was arranged into whenever the child wanted more, so content that does not
    /// fit is cut off at the border rather than drawn over it and beyond. Avalonia has no such layout clip, so the
    /// border applies the same thing itself and the two platforms agree. With
    /// <see cref="ClipContentToCornerRadius"/> the clip follows the corners instead of the plain content rectangle.
    /// </remarks>
    /// <param name="size">The size the border has just been arranged to.</param>
    private void UpdateContentClip(Size size)
    {
        var child = this.Child;
        if (child == null)
        {
            return;
        }

        if (this.ClipContentToCornerRadius)
        {
            var metrics = this.GetMetrics(size, this.GetDeviceAlignment().Scale);
            child.Clip = BorderLayoutCalculator.TryCalculateContentClip(metrics, this.Padding.ToSides(), out var figure)
                ? figure.ToGeometry()
                : StreamGeometry.Parse("M 0,0 Z");
            return;
        }

        var inset = this.BorderThickness + this.Padding;
        child.Clip = new RectangleGeometry(new Rect(
            0,
            0,
            Math.Max(0, size.Width - inset.Left - inset.Right),
            Math.Max(0, size.Height - inset.Top - inset.Bottom)));
    }

    private void ClearCache()
    {
        this.cornerBlends = null;
    }

    private Corners<CornerBlend> EnsureCornerBlends(Sides<IBrush?> sides)
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
    private static CornerBlend GetCornerBlend(IBrush? first, IBrush? second, Corner corner)
    {
        var gradient = CornerGradient.For(corner);
        switch (first, second)
        {
            case (ISolidColorBrush firstBrush, ISolidColorBrush secondBrush):
                return new CornerBlend(firstBrush.Color, secondBrush.Color);
            case (ISolidColorBrush firstBrush, IGradientBrush secondBrush):
                return new CornerBlend(firstBrush.Color, GetEndColor(secondBrush, gradient.ReverseSecond));
            case (IGradientBrush firstBrush, ISolidColorBrush secondBrush):
                return new CornerBlend(GetStartColor(firstBrush, gradient.ReverseFirst), secondBrush.Color);
            case (IGradientBrush firstBrush, IGradientBrush secondBrush):
                return new CornerBlend(
                    GetStartColor(firstBrush, gradient.ReverseFirst),
                    GetEndColor(secondBrush, gradient.ReverseSecond));
            default:
                return new CornerBlend(first);
        }
    }

    /// <summary>
    /// What it takes to line this border's geometry up with the device pixels it ends up on.
    /// </summary>
    /// <param name="Scale">The device pixels one unit of the border's own coordinate space covers, on each axis.</param>
    /// <param name="Offset">How far the border has to be drawn from where it sits for its origin to land on a whole
    /// device pixel, in the border's own coordinate space.</param>
    private readonly record struct DeviceAlignment(Scale Scale, Position Offset);

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

        public CornerBlend(IBrush? brush)
        {
            this.start = default;
            this.end = default;
            this.Brush = brush;
            this.IsBlended = false;
        }

        public IBrush? Brush { get; }

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
