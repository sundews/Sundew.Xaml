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
    private static readonly Point TopRight = new Point(1, 0);
    private static readonly Point BottomLeft = new Point(0, 1);
    private static readonly Point TopLeft = new Point(0, 0);
    private static readonly Point BottomRight = new Point(1, 1);
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

    private Pens? pens = null;
    private Corners? cornerBrushes = null;

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
    /// Renders the border.
    /// </summary>
    /// <param name="drawingContext">The drawing context.</param>
    protected override void OnRender(DrawingContext drawingContext)
    {
        var sidesBrushes = new Sides(this.LeftBrush, this.TopBrush, this.RightBrush, this.BottomBrush);
        var dpiScale = VisualTreeHelper.GetDpi(this);
        var dpiScaleX = dpiScale.DpiScaleX;
        var dpiScaleY = dpiScale.DpiScaleY;
        var pens = this.EnsurePens(sidesBrushes, dpiScale);

        // Round all base measurements to device pixels
        var actualWidth = RoundToDevicePixel(this.ActualWidth, dpiScaleX);
        var actualHeight = RoundToDevicePixel(this.ActualHeight, dpiScaleY);

        var leftThickness = RoundToDevicePixel(this.BorderThickness.Left, dpiScaleX);
        var topThickness = RoundToDevicePixel(this.BorderThickness.Top, dpiScaleY);
        var rightThickness = RoundToDevicePixel(this.BorderThickness.Right, dpiScaleX);
        var bottomThickness = RoundToDevicePixel(this.BorderThickness.Bottom, dpiScaleY);

        var leftOffset = leftThickness / 2.0;
        var topOffset = topThickness / 2.0;
        var rightOffset = rightThickness / 2.0;
        var bottomOffset = bottomThickness / 2.0;

        // Round corner radius values
        var topLeftRadius = RoundToDevicePixel(this.CornerRadius.TopLeft, Math.Min(dpiScaleX, dpiScaleY));
        var topRightRadius = RoundToDevicePixel(this.CornerRadius.TopRight, Math.Min(dpiScaleX, dpiScaleY));
        var bottomRightRadius = RoundToDevicePixel(this.CornerRadius.BottomRight, Math.Min(dpiScaleX, dpiScaleY));
        var bottomLeftRadius = RoundToDevicePixel(this.CornerRadius.BottomLeft, Math.Min(dpiScaleX, dpiScaleY));

        var maxWidthOrHeight = Math.Min(actualWidth, actualHeight) / 2;
        var maxTopLeft = RoundToDevicePixel(Math.Min(Math.Max(topLeftRadius, Math.Max(topThickness, leftThickness)), maxWidthOrHeight), Math.Min(dpiScaleX, dpiScaleY));
        var maxTopRight = RoundToDevicePixel(Math.Min(Math.Max(topRightRadius, Math.Max(topThickness, rightThickness)), maxWidthOrHeight), Math.Min(dpiScaleX, dpiScaleY));
        var maxBottomRight = RoundToDevicePixel(Math.Min(Math.Max(bottomRightRadius, Math.Max(bottomThickness, rightThickness)), maxWidthOrHeight), Math.Min(dpiScaleX, dpiScaleY));
        var maxBottomLeft = RoundToDevicePixel(Math.Min(Math.Max(bottomLeftRadius, Math.Max(bottomThickness, leftThickness)), maxWidthOrHeight), Math.Min(dpiScaleX, dpiScaleY));

        var right = actualWidth - rightOffset;
        var bottom = actualHeight - bottomOffset;

        // Calculate exact endpoints for perfect alignment
        var leftEdgeTop = RoundToDevicePixel(maxTopLeft, dpiScaleY);
        var leftEdgeBottom = RoundToDevicePixel(actualHeight - maxBottomLeft, dpiScaleY);
        var topEdgeLeft = RoundToDevicePixel(maxTopLeft, dpiScaleX);
        var topEdgeRight = RoundToDevicePixel(actualWidth - maxTopRight, dpiScaleX);
        var rightEdgeTop = RoundToDevicePixel(maxTopRight, dpiScaleY);
        var rightEdgeBottom = RoundToDevicePixel(actualHeight - maxBottomRight, dpiScaleY);
        var bottomEdgeLeft = RoundToDevicePixel(maxBottomLeft, dpiScaleX);
        var bottomEdgeRight = RoundToDevicePixel(actualWidth - maxBottomRight, dpiScaleX);

        if (leftEdgeTop > leftEdgeBottom)
        {
            leftEdgeTop = leftEdgeBottom;
        }

        if (rightEdgeTop > rightEdgeBottom)
        {
            rightEdgeTop = rightEdgeBottom;
        }

        if (topEdgeLeft > topEdgeRight)
        {
            topEdgeLeft = topEdgeRight;
        }

        if (bottomEdgeLeft > bottomEdgeRight)
        {
            bottomEdgeLeft = bottomEdgeRight;
        }

        // Edge coordinates - ensure exact alignment at corner boundaries
        var leftStart = new Point(leftOffset, leftEdgeBottom);
        var leftEnd = new Point(leftOffset, leftEdgeTop);

        var topLeftStart = new Point(leftOffset, leftEdgeTop);
        var topLeftAnchor = new Point(leftOffset, topOffset);
        var topLeftEnd = new Point(topEdgeLeft, topOffset);

        var topStart = new Point(topEdgeLeft, topOffset);
        var topEnd = new Point(topEdgeRight, topOffset);

        var topRightStart = new Point(topEdgeRight, topOffset);
        var topRightAnchor = new Point(right, topOffset);
        var topRightEnd = new Point(right, rightEdgeTop);

        var rightStart = new Point(right, rightEdgeTop);
        var rightEnd = new Point(right, rightEdgeBottom);

        var bottomRightStart = new Point(right, rightEdgeBottom);
        var bottomRightAnchor = new Point(right, bottom);
        var bottomRightEnd = new Point(bottomEdgeRight, bottom);

        var bottomStart = new Point(bottomEdgeRight, bottom);
        var bottomEnd = new Point(bottomEdgeLeft, bottom);

        var bottomLeftStart = new Point(leftOffset, leftEdgeBottom);
        var bottomLeftAnchor = new Point(leftOffset, bottom);
        var bottomLeftEnd = new Point(bottomEdgeLeft, bottom);

        // Create guidelines based on actual coordinates used
        var guidelines = new GuidelineSet();

        // Add X guidelines for all vertical edges
        AddUniqueGuideline(guidelines.GuidelinesX, leftOffset);
        AddUniqueGuideline(guidelines.GuidelinesX, right);
        AddUniqueGuideline(guidelines.GuidelinesX, topEdgeLeft);
        AddUniqueGuideline(guidelines.GuidelinesX, topEdgeRight);
        AddUniqueGuideline(guidelines.GuidelinesX, bottomEdgeLeft);
        AddUniqueGuideline(guidelines.GuidelinesX, bottomEdgeRight);

        // Add Y guidelines for all horizontal edges
        AddUniqueGuideline(guidelines.GuidelinesY, topOffset);
        AddUniqueGuideline(guidelines.GuidelinesY, bottom);
        AddUniqueGuideline(guidelines.GuidelinesY, leftEdgeTop);
        AddUniqueGuideline(guidelines.GuidelinesY, leftEdgeBottom);
        AddUniqueGuideline(guidelines.GuidelinesY, rightEdgeTop);
        AddUniqueGuideline(guidelines.GuidelinesY, rightEdgeBottom);

        var drawingGroup = new DrawingGroup() { GuidelineSet = guidelines };

        // Background geometry - ensure proper closure
        const double backgroundOverdraw = 0.5;
        var background = new GeometryDrawing(this.Background, BackgroundPen, new PathGeometry(
        [
            new PathFigure(
                leftStart with { X = leftStart.X + leftOffset },
                [
                    new LineSegment(leftEnd with { X = leftEnd.X + leftOffset }, true),
                    new QuadraticBezierSegment(
                        new Point(topLeftAnchor.X + leftOffset - backgroundOverdraw, topLeftAnchor.Y + topOffset - backgroundOverdraw),                        topStart with { Y = topStart.Y + topOffset }, true),
                    new LineSegment(topEnd with { Y = topEnd.Y + topOffset }, true),
                    new QuadraticBezierSegment(
                        new Point(topRightAnchor.X - rightOffset + backgroundOverdraw, topRightAnchor.Y + topOffset - backgroundOverdraw), rightStart with { X = rightStart.X - rightOffset }, true),
                    new LineSegment(rightEnd with { X = rightEnd.X - rightOffset }, true),
                    new QuadraticBezierSegment(
                        new Point(bottomRightAnchor.X - rightOffset + backgroundOverdraw, bottomRightAnchor.Y - bottomOffset + backgroundOverdraw), bottomStart with { Y = bottomStart.Y - bottomOffset }, true),
                    new LineSegment(bottomEnd with { Y = bottomEnd.Y - bottomOffset }, true),
                    new QuadraticBezierSegment(
                        new Point(bottomLeftAnchor.X + leftOffset - backgroundOverdraw, bottomLeftAnchor.Y - bottomOffset + backgroundOverdraw), leftStart with { X = leftStart.X + leftOffset }, true),],
                true)
        ]));

        drawingGroup.Children.Add(background);

        // Only draw edges if they have non-zero length
        var hasLeft = Math.Abs(leftStart.Y - leftEnd.Y) > 0.01;
        if (hasLeft)
        {
            var leftDrawing = new GeometryDrawing(
                null,
                pens.Left,
                new LineGeometry(leftStart, leftEnd));
            drawingGroup.Children.Add(leftDrawing);
        }

        var hasTop = Math.Abs(topStart.X - topEnd.X) > 0.01;
        if (hasTop)
        {
            var topDrawing = new GeometryDrawing(
                null,
                pens.Top,
                new LineGeometry(topStart, topEnd));
            drawingGroup.Children.Add(topDrawing);
        }

        var hasRight = Math.Abs(rightStart.Y - rightEnd.Y) > 0.01;
        if (hasRight)
        {
            var rightDrawing = new GeometryDrawing(
                null,
                pens.Right,
                new LineGeometry(rightStart, rightEnd));
            drawingGroup.Children.Add(rightDrawing);
        }

        var hasBottom = Math.Abs(bottomStart.X - bottomEnd.X) > 0.01;
        if (hasBottom)
        {
            var bottomDrawing = new GeometryDrawing(
                null,
                pens.Bottom,
                new LineGeometry(bottomStart, bottomEnd));
            drawingGroup.Children.Add(bottomDrawing);
        }

        // Always draw corners
        var topLeftDrawing = new GeometryDrawing(
            null,
            pens.TopLeft,
            new PathGeometry([new PathFigure(topLeftStart, [new QuadraticBezierSegment(topLeftAnchor, topLeftEnd, true)], false)]));
        drawingGroup.Children.Add(topLeftDrawing);

        var topRightDrawing = new GeometryDrawing(
            null,
            pens.TopRight,
            new PathGeometry([new PathFigure(topRightStart, [new QuadraticBezierSegment(topRightAnchor, topRightEnd, true)], false)]));
        drawingGroup.Children.Add(topRightDrawing);

        var bottomRightDrawing = new GeometryDrawing(
            null,
            pens.BottomRight,
            new PathGeometry([new PathFigure(bottomRightStart, [new QuadraticBezierSegment(bottomRightAnchor, bottomRightEnd, true)], false)]));
        drawingGroup.Children.Add(bottomRightDrawing);

        var bottomLeftDrawing = new GeometryDrawing(
            null,
            pens.BottomLeft,
            new PathGeometry([new PathFigure(bottomLeftStart, [new QuadraticBezierSegment(bottomLeftAnchor, bottomLeftEnd, true)], false)]));
        drawingGroup.Children.Add(bottomLeftDrawing);

        drawingContext.DrawDrawing(drawingGroup);
    }

    private static void AddUniqueGuideline(DoubleCollection guidelines, double value)
    {
        var rounded = Math.Round(value, 1); // Round to 0.1 precision
        if (!guidelines.Contains(rounded))
        {
            guidelines.Add(rounded);
        }
    }

    private static void OnFreshRenderRequired(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Border border)
        {
            border.ClearCache();
        }
    }

    private static double RoundToDevicePixel(double value, double dpiScale)
    {
        return Math.Round(value * dpiScale) / dpiScale;
    }

    private Pens EnsurePens(Sides sides, DpiScale dpiScale)
    {
        var cornerBrushes = this.EnsureCornerBrushes(sides);
        var dpiScaleFactor = (dpiScale.DpiScaleX + dpiScale.DpiScaleY) / 2;

        // Cannot support non-uniform border thickness with current implementation.
        this.pens ??= new Pens(
            new Pen(sides.Left, RoundToDevicePixel(this.BorderThickness.Left, dpiScale.DpiScaleX))
            {
                DashCap = PenLineCap.Flat,
                StartLineCap = PenLineCap.Flat,
                EndLineCap = PenLineCap.Flat,
            },
            new Pen(cornerBrushes.TopLeft, RoundToDevicePixel((this.BorderThickness.Top + this.BorderThickness.Left) / 2.0, dpiScaleFactor))
            {
                DashCap = PenLineCap.Flat,
                StartLineCap = PenLineCap.Flat,
                EndLineCap = PenLineCap.Flat,
            },
            new Pen(sides.Top, RoundToDevicePixel(this.BorderThickness.Top, dpiScale.DpiScaleY))
            {
                DashCap = PenLineCap.Flat,
                StartLineCap = PenLineCap.Flat,
                EndLineCap = PenLineCap.Flat,
            },
            new Pen(cornerBrushes.TopRight, RoundToDevicePixel((this.BorderThickness.Top + this.BorderThickness.Right) / 2.0, dpiScaleFactor))
            {
                DashCap = PenLineCap.Flat,
                StartLineCap = PenLineCap.Flat,
                EndLineCap = PenLineCap.Flat,
            },
            new Pen(sides.Right, RoundToDevicePixel(this.BorderThickness.Right, dpiScale.DpiScaleX))
            {
                DashCap = PenLineCap.Flat,
                StartLineCap = PenLineCap.Flat,
                EndLineCap = PenLineCap.Flat,
            },
            new Pen(cornerBrushes.BottomRight, RoundToDevicePixel((this.BorderThickness.Bottom + this.BorderThickness.Right) / 2.0, dpiScaleFactor))
            {
                DashCap = PenLineCap.Flat,
                StartLineCap = PenLineCap.Flat,
                EndLineCap = PenLineCap.Flat,
            },
            new Pen(sides.Bottom, RoundToDevicePixel(this.BorderThickness.Bottom, dpiScale.DpiScaleY))
            {
                DashCap = PenLineCap.Flat,
                StartLineCap = PenLineCap.Flat,
                EndLineCap = PenLineCap.Flat,
            },
            new Pen(cornerBrushes.BottomLeft, RoundToDevicePixel((this.BorderThickness.Bottom + this.BorderThickness.Left) / 2.0, dpiScaleFactor))
            {
                DashCap = PenLineCap.Flat,
                StartLineCap = PenLineCap.Flat,
                EndLineCap = PenLineCap.Flat,
            });

        if (sides.Left?.IsFrozen ?? false)
        {
            this.pens.Left.Freeze();
        }

        this.pens.TopLeft?.Freeze();

        if (sides.Top?.IsFrozen ?? false)
        {
            this.pens.Top.Freeze();
        }

        this.pens.TopRight?.Freeze();

        if (sides.Right?.IsFrozen ?? false)
        {
            this.pens.Right.Freeze();
        }

        this.pens.BottomRight?.Freeze();

        if (sides.Bottom?.IsFrozen ?? false)
        {
            this.pens.Bottom.Freeze();
        }

        this.pens.BottomLeft?.Freeze();

        return this.pens;
    }

    private void ClearCache()
    {
        this.pens = null;
        this.cornerBrushes = null;
    }

    private Corners EnsureCornerBrushes(Sides sides)
    {
        if (this.cornerBrushes.HasValue)
        {
            return this.cornerBrushes.Value;
        }

        var topLeftBrush = this.GetCornerBrush(sides.Left, sides.Top, this.TopLeftStops, BottomLeft, TopRight, Corner.TopLeft);
        topLeftBrush?.Freeze();
        var topRightBrush = this.GetCornerBrush(sides.Top, sides.Right, this.TopRightStops, TopLeft, BottomRight, Corner.TopRight);
        topRightBrush?.Freeze();
        var bottomRightBrush = this.GetCornerBrush(sides.Right, sides.Bottom, this.BottomRightStops, TopRight, BottomLeft, Corner.BottomRight);
        bottomRightBrush?.Freeze();
        var bottomLeftBrush = this.GetCornerBrush(sides.Bottom, sides.Left, this.BottomLeftStops, BottomRight, TopLeft, Corner.BottomLeft);
        bottomLeftBrush?.Freeze();
        this.cornerBrushes = new(topLeftBrush, topRightBrush, bottomRightBrush, bottomLeftBrush);
        return this.cornerBrushes.Value;
    }

    private Brush? GetCornerBrush(Brush? first, Brush? second, Stops stops, Point firstPoint, Point secondPoint, Corner corner)
    {
        switch ((first, second))
        {
            case (SolidColorBrush firstBrush1, SolidColorBrush secondBrush1):
                var startColor1 = firstBrush1.Color;
                var endColor1 = secondBrush1.Color;
                return new LinearGradientBrush(
                    new GradientStopCollection([
                        new GradientStop(startColor1, 0),
                        new GradientStop(startColor1, stops.First),
                        new GradientStop(endColor1, stops.Second),
                        new GradientStop(endColor1, 1)
                    ]),
                    firstPoint,
                    secondPoint);
            case (SolidColorBrush firstBrush2, LinearGradientBrush secondBrush2):
                var reverseSecond2 = corner is Corner.TopLeft or Corner.TopRight;
                var endColor2 = this.GetSecond(secondBrush2, reverseSecond2);
                var startColor2 = firstBrush2.Color;
                return new LinearGradientBrush(
                    new GradientStopCollection([
                        new GradientStop(startColor2, 0),
                        new GradientStop(startColor2, stops.First),
                        new GradientStop(endColor2, stops.Second),
                        new GradientStop(endColor2, 1)]),
                    firstPoint,
                    secondPoint);
            case (LinearGradientBrush firstBrush, SolidColorBrush secondBrush):
                var reverseFirst3 = corner is Corner.TopRight or Corner.BottomRight;
                var startColor3 = this.GetFirst(firstBrush, reverseFirst3);
                var endColor3 = secondBrush.Color;
                return new LinearGradientBrush(
                    new GradientStopCollection([
                        new GradientStop(startColor3, 0),
                        new GradientStop(startColor3, stops.First),
                        new GradientStop(endColor3, stops.Second),
                        new GradientStop(endColor3, 1)
                    ]),
                    firstPoint,
                    secondPoint);
            case (LinearGradientBrush firstBrush, LinearGradientBrush secondBrush):
                var reverseFirst4 = corner is Corner.TopRight or Corner.BottomRight;
                var reverseSecond4 = corner is Corner.TopLeft or Corner.TopRight;
                var startColor4 = this.GetFirst(firstBrush, reverseFirst4);
                var endColor4 = this.GetSecond(secondBrush, reverseSecond4);
                return new LinearGradientBrush(
                    new GradientStopCollection([
                        new GradientStop(startColor4, 0),
                        new GradientStop(startColor4, stops.First),
                        new GradientStop(endColor4, stops.Second),
                        new GradientStop(endColor4, 1)
                    ]),
                    firstPoint,
                    secondPoint);
        }

        return first;
    }

    private Color GetFirst(LinearGradientBrush brush, bool reverse)
    {
        if (reverse)
        {
            return brush.GradientStops.Count > 0 ? brush.GradientStops[^1].Color : Colors.Transparent;
        }

        return brush.GradientStops.Count > 0 ? brush.GradientStops[0].Color : Colors.Transparent;
    }

    private Color GetSecond(LinearGradientBrush brush, bool reverse)
    {
        if (reverse)
        {
            return brush.GradientStops.Count > 0 ? brush.GradientStops[0].Color : Colors.Transparent;
        }

        return brush.GradientStops.Count > 0 ? brush.GradientStops[^1].Color : Colors.Transparent;
    }

    private enum Corner
    {
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft,
    }

    private readonly record struct Sides(Brush? Left, Brush? Top, Brush? Right, Brush? Bottom);

    private readonly record struct Corners(Brush? TopLeft, Brush? TopRight, Brush? BottomRight, Brush? BottomLeft);

    private sealed class Pens(
        Pen left,
        Pen topLeft,
        Pen top,
        Pen topRight,
        Pen right,
        Pen bottomRight,
        Pen bottom,
        Pen bottomLeft)
    {
        public Pen Left { get; } = left;

        public Pen TopLeft { get; } = topLeft;

        public Pen Top { get; } = top;

        public Pen TopRight { get; } = topRight;

        public Pen Right { get; } = right;

        public Pen BottomRight { get; } = bottomRight;

        public Pen Bottom { get; } = bottom;

        public Pen BottomLeft { get; } = bottomLeft;
    }
}