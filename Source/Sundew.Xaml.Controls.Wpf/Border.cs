namespace Sundew.Xaml.Controls;

using System.Windows;
using System.Windows.Media;

public class Border : System.Windows.Controls.Border
{
    public static readonly DependencyProperty SideBrushesProperty = DependencyProperty.Register(
        nameof(SideBrushes), typeof(SideBrushes), typeof(Border), new FrameworkPropertyMetadata(new SideBrushes(Brushes.Black), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CornerStopsProperty = DependencyProperty.Register(
        nameof(CornerStops), typeof(CornerStops), typeof(Border), new FrameworkPropertyMetadata(
            new CornerStops(new Stops(0.2, 0.8)),
            FrameworkPropertyMetadataOptions.AffectsMeasure |
            FrameworkPropertyMetadataOptions.AffectsRender));

    private static readonly Point TopRight = new Point(1, 0);
    private static readonly Point BottomLeft = new Point(0, 1);
    private static readonly Point TopLeft = new Point(0, 0);
    private static readonly Point BottomRight = new Point(1, 1);

    public Border()
    {
        this.UseLayoutRounding = true;
        this.SnapsToDevicePixels = true;
    }

    public SideBrushes? SideBrushes
    {
        get { return (SideBrushes)this.GetValue(SideBrushesProperty); }
        set { this.SetValue(SideBrushesProperty, value); }
    }

    public CornerStops CornerStops
    {
        get { return (CornerStops)this.GetValue(CornerStopsProperty); }
        set { this.SetValue(CornerStopsProperty, value); }
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (this.SideBrushes == null)
        {
            return;
        }

        var leftBrush = this.SideBrushes.Left;
        var topBrush = this.SideBrushes.Top;
        var rightBrush = this.SideBrushes.Right;
        var bottomBrush = this.SideBrushes.Bottom;
        var cornerBrushes = this.GetCornerBrushes(this.SideBrushes, this.CornerStops);

        var leftOffset = this.BorderThickness.Left / 2.0;
        var topOffset = this.BorderThickness.Top / 2.0;

        var rightOffset = this.BorderThickness.Right / 2.0;
        var bottomOffset = this.BorderThickness.Bottom / 2.0;

        var maxWidthOrHeight = Math.Min(this.ActualWidth, this.ActualHeight) / 2;
        var maxTopLeft = Math.Min(Math.Max(this.CornerRadius.TopLeft, Math.Max(this.BorderThickness.Top, this.BorderThickness.Left)), maxWidthOrHeight);
        var maxTopRight = Math.Min(Math.Max(this.CornerRadius.TopRight, Math.Max(this.BorderThickness.Top, this.BorderThickness.Right)), maxWidthOrHeight);
        var maxBottomRight = Math.Min(Math.Max(this.CornerRadius.BottomRight, Math.Max(this.BorderThickness.Bottom, this.BorderThickness.Right)), maxWidthOrHeight);
        var maxBottomLeft = Math.Min(Math.Max(this.CornerRadius.BottomLeft, Math.Max(this.BorderThickness.Bottom, this.BorderThickness.Left)), maxWidthOrHeight);

        const double overdraw = 0.0;
        var leftStart = new Point(leftOffset, this.ActualHeight - maxBottomLeft + overdraw);
        var leftEnd = new Point(leftOffset, maxTopLeft - overdraw);
        var leftDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(leftBrush, this.BorderThickness.Left),
                new LineGeometry(leftStart, leftEnd));

        var topLeftStart = new Point(leftOffset, maxTopLeft);
        var topLeftAnchor = new Point(leftOffset, topOffset);
        var topLeftEnd = new Point(maxTopLeft, topOffset);
        var topLeftDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(cornerBrushes.TopLeft, (this.BorderThickness.Top + this.BorderThickness.Left) / 2.0),
                new PathGeometry([new PathFigure(topLeftStart, [new QuadraticBezierSegment(topLeftAnchor, topLeftEnd, true),], false)]));

        var topStart = new Point(maxTopLeft - overdraw, topOffset);
        var topEnd = new Point(this.ActualWidth - maxTopRight + overdraw, topOffset);
        var topDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(topBrush, this.BorderThickness.Top),
                new LineGeometry(topStart, topEnd));

        var topRightStart = new Point(this.ActualWidth - maxTopRight, topOffset);
        var rightX = this.ActualWidth - rightOffset;
        var topRightAnchor = new Point(rightX, topOffset);
        var topRightEnd = new Point(rightX, maxTopRight);
        var topRightDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(cornerBrushes.TopRight, (this.BorderThickness.Top + this.BorderThickness.Right) / 2.0),
                new PathGeometry([new PathFigure(topRightStart, [new QuadraticBezierSegment(topRightAnchor, topRightEnd, true),], false)]));

        var rightStart = new Point(rightX, maxTopRight - overdraw);
        var rightEnd = new Point(rightX, this.ActualHeight - maxBottomRight + overdraw);
        var rightDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(rightBrush, this.BorderThickness.Right),
                new LineGeometry(rightStart, rightEnd)
            );

        var bottom = this.ActualHeight - bottomOffset;
        var bottomRightStart = new Point(rightX, this.ActualHeight - maxBottomRight);
        var bottomRightAnchor = new Point(rightX, bottom);
        var bottomRightEnd = new Point(this.ActualWidth - maxBottomRight, bottom);
        var bottomRightDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(cornerBrushes.BottomRight, (this.BorderThickness.Bottom + this.BorderThickness.Right) / 2.0),
                new PathGeometry([new PathFigure(bottomRightStart, [new QuadraticBezierSegment(bottomRightAnchor, bottomRightEnd, true),], false)]));

        var bottomStart = new Point(maxBottomLeft - overdraw, bottom);
        var bottomEnd = new Point(this.ActualWidth - maxBottomRight + overdraw, bottom);
        var bottomDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(bottomBrush, this.BorderThickness.Bottom),
                new LineGeometry(bottomStart, bottomEnd));

        var bottomLeftStart = new Point(leftOffset, this.ActualHeight - maxBottomLeft);
        var bottomLeftAnchor = new Point(leftOffset, bottom);
        var bottomLeftEnd = new Point(maxBottomLeft, bottom);
        var bottomLeftDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(cornerBrushes.BottomLeft, (this.BorderThickness.Bottom + this.BorderThickness.Left) / 2.0),
                new PathGeometry([
                    new PathFigure(bottomLeftStart, [new QuadraticBezierSegment(bottomLeftAnchor, bottomLeftEnd, true),], false)]));

        const double backgroundOverdraw = 0.64;
        var background = new GeometryDrawing(this.Background, new Pen(Brushes.Transparent, 0), new PathGeometry(
        [
            new PathFigure(
                leftStart with { X = leftStart.X + leftOffset},
                    [
                        new LineSegment(leftEnd with { X = leftEnd.X + leftOffset}, true),
                        new QuadraticBezierSegment(new Point(topLeftAnchor.X + leftOffset - backgroundOverdraw, topLeftAnchor.Y + topOffset - backgroundOverdraw), topStart with{ Y = topStart.Y + topOffset}, true),
                        new LineSegment(topEnd with { Y = topEnd.Y + topOffset}, true),
                        new QuadraticBezierSegment(new Point(topRightAnchor.X - rightOffset+ backgroundOverdraw, topRightAnchor.Y + topOffset - backgroundOverdraw), rightStart with { X = rightStart.X - rightOffset }, true),
                        new LineSegment(rightEnd  with { X = rightEnd.X - rightOffset}, true),
                        new QuadraticBezierSegment(new Point(bottomRightAnchor.X - rightOffset + backgroundOverdraw, bottomRightAnchor.Y - bottomOffset+ backgroundOverdraw), bottomEnd with { Y = bottomEnd.Y - bottomOffset}, true),
                        new LineSegment(bottomStart  with { Y = bottomStart.Y - bottomOffset}, true),
                        new QuadraticBezierSegment(new Point(bottomLeftAnchor.X + leftOffset - backgroundOverdraw,  bottomLeftAnchor.Y - bottomOffset+ backgroundOverdraw), leftStart with { X = leftStart.X + leftOffset}, true),
                    ], true)
        ]));

        drawingContext.DrawDrawing(background);
        DrawingGroup aDrawingGroup = new DrawingGroup();
        aDrawingGroup.Children.Add(leftDrawing);
        aDrawingGroup.Children.Add(topDrawing);
        aDrawingGroup.Children.Add(topLeftDrawing);
        aDrawingGroup.Children.Add(rightDrawing);
        aDrawingGroup.Children.Add(topRightDrawing);
        aDrawingGroup.Children.Add(bottomDrawing);
        aDrawingGroup.Children.Add(bottomRightDrawing);
        aDrawingGroup.Children.Add(bottomLeftDrawing);
        drawingContext.DrawDrawing(aDrawingGroup);
    }

    private (Brush TopLeft, Brush TopRight, Brush BottomRight, Brush BottomLeft) GetCornerBrushes(SideBrushes sideBrushes, CornerStops cornerStops)
    {
        var topLeftBrush = this.GetBrush(sideBrushes.Left, sideBrushes.Top, cornerStops.TopLeft, BottomLeft, TopRight, Corner.TopLeft);
        var topRightBrush = this.GetBrush(sideBrushes.Top, sideBrushes.Right, cornerStops.TopRight, TopLeft, BottomRight, Corner.TopRight);
        var bottomRightBrush = this.GetBrush(sideBrushes.Right, sideBrushes.Bottom, cornerStops.BottomRight, TopRight, BottomLeft, Corner.BottomRight);
        var bottomLeftBrush = this.GetBrush(sideBrushes.Bottom, sideBrushes.Left, cornerStops.BottomLeft, BottomRight, TopLeft, Corner.BottomLeft);
        return (topLeftBrush, topRightBrush, bottomRightBrush, bottomLeftBrush);
    }

    private Brush GetBrush(Brush first, Brush second, Stops stops, Point firstPoint, Point secondPoint, Corner corner)
    {
        switch ((first, second))
        {
            case (SolidColorBrush firstBrush1, SolidColorBrush secondBrush1):
                return new LinearGradientBrush(
                    new GradientStopCollection([
                        new GradientStop(firstBrush1.Color, 0),
                        new GradientStop(firstBrush1.Color, stops.First),
                        new GradientStop(secondBrush1.Color, stops.Second),
                        new GradientStop(secondBrush1.Color, 1)
                    ]), firstPoint, secondPoint);
            case (SolidColorBrush firstBrush2, LinearGradientBrush secondBrush2):
                var reverseSecond2 = corner is Corner.TopLeft or Corner.TopRight;
                var endColor2 = this.GetSecond(secondBrush2, reverseSecond2);
                return new LinearGradientBrush(
                    new GradientStopCollection([
                        new GradientStop(firstBrush2.Color, 0),
                        new GradientStop(firstBrush2.Color, stops.First),
                        new GradientStop(endColor2, stops.Second),
                        new GradientStop(endColor2, 1)
                    ]), firstPoint, secondPoint);
            case (LinearGradientBrush firstBrush, SolidColorBrush secondBrush):
                var reverseFirst3 = corner is Corner.TopRight or Corner.BottomRight;
                var firstColor3 = this.GetFirst(firstBrush, reverseFirst3);
                return new LinearGradientBrush(
                    new GradientStopCollection([
                        new GradientStop(firstColor3, 0),
                        new GradientStop(firstColor3, stops.First),
                        new GradientStop(secondBrush.Color, stops.Second),
                        new GradientStop(secondBrush.Color, 1)
                    ]), firstPoint, secondPoint);
            case (LinearGradientBrush firstBrush, LinearGradientBrush secondBrush):
                var reverseFirst4 = corner is Corner.TopRight or Corner.BottomRight;
                var reverseSecond4 = corner is Corner.TopLeft or Corner.TopRight;
                var firstColor4 = this.GetFirst(firstBrush, reverseFirst4);
                var endColor4 = this.GetSecond(secondBrush, reverseSecond4);
                return new LinearGradientBrush(
                    new GradientStopCollection([
                        new GradientStop(firstColor4, 0),
                        new GradientStop(firstColor4, stops.First),
                        new GradientStop(endColor4, stops.Second),
                        new GradientStop(endColor4, 1)
                    ]), firstPoint, secondPoint);
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
}