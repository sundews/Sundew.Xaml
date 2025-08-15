namespace Sundew.Xaml.Controls.Wpf;

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


    public SideBrushes? SideBrushes
    {
        get { return (SideBrushes)GetValue(SideBrushesProperty); }
        set { SetValue(SideBrushesProperty, value); }
    }

    public CornerStops CornerStops
    {
        get { return (CornerStops)GetValue(CornerStopsProperty); }
        set { SetValue(CornerStopsProperty, value); }
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (SideBrushes == null)
        {
            return;
        }

        var leftBrush = SideBrushes.Left;
        var topBrush = SideBrushes.Top;
        var rightBrush = SideBrushes.Right;
        var bottomBrush = SideBrushes.Bottom;
        var cornerBrushes = GetCornerBrushes(SideBrushes, CornerStops);

        var leftOffset = BorderThickness.Left / 2.0;
        var topOffset = BorderThickness.Top / 2.0;

        var rightOffset = BorderThickness.Right / 2.0;
        var bottomOffset = BorderThickness.Bottom / 2.0;

        var maxWidthOrHeight = Math.Min(ActualWidth, ActualHeight) / 2;
        var maxTopLeft = Math.Min(Math.Max(this.CornerRadius.TopLeft, Math.Max(BorderThickness.Top, BorderThickness.Left)), maxWidthOrHeight);
        var maxTopRight = Math.Min(Math.Max(this.CornerRadius.TopRight, Math.Max(BorderThickness.Top, BorderThickness.Right)), maxWidthOrHeight);
        var maxBottomRight = Math.Min(Math.Max(this.CornerRadius.BottomRight, Math.Max(BorderThickness.Bottom, BorderThickness.Right)), maxWidthOrHeight);
        var maxBottomLeft = Math.Min(Math.Max(this.CornerRadius.BottomLeft, Math.Max(BorderThickness.Bottom, BorderThickness.Left)), maxWidthOrHeight);

        const double overdraw = 0.04;
        var leftStart = new Point(leftOffset, maxTopLeft - overdraw);
        var leftEnd = new Point(leftOffset, this.ActualHeight - maxBottomLeft + overdraw);
        var leftDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(leftBrush, BorderThickness.Left),
                new LineGeometry(leftStart, leftEnd));

        var topLeftStart = new Point(leftOffset, maxTopLeft);
        var topLeftAnchor = new Point(leftOffset, topOffset);
        var topLeftEnd = new Point(maxTopLeft, topOffset);
        var topLeftDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(cornerBrushes.TopLeft, (BorderThickness.Top + BorderThickness.Left) / 2.0),
                new PathGeometry([new PathFigure(topLeftStart, [new QuadraticBezierSegment(topLeftAnchor, topLeftEnd, true),], false)]));

        var topStart = new Point(maxTopLeft - overdraw, topOffset);
        var topEnd = new Point(this.ActualWidth - maxTopRight + overdraw, topOffset);
        var topDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(topBrush, BorderThickness.Top),
                new LineGeometry(topStart, topEnd));

        var topRightStart = new Point(this.ActualWidth - maxTopRight, topOffset);
        var topRightAnchor = new Point(this.ActualWidth - rightOffset, topOffset);
        var topRightEnd = new Point(this.ActualWidth - rightOffset, maxTopRight);
        var topRightDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(cornerBrushes.TopRight, (BorderThickness.Top + BorderThickness.Right) / 2.0),
                new PathGeometry([new PathFigure(topRightStart, [new QuadraticBezierSegment(topRightAnchor, topRightEnd, true),], false)]));

        var rightStart = new Point(this.ActualWidth - rightOffset, maxTopRight - overdraw);
        var rightEnd = new Point(this.ActualWidth - rightOffset, this.ActualHeight - maxBottomRight + overdraw);
        var rightDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(rightBrush, BorderThickness.Right),
                new LineGeometry(rightStart, rightEnd)
            );

        var bottomRightStart = new Point(this.ActualWidth - rightOffset, this.ActualHeight - maxBottomRight);
        var bottomRightAnchor = new Point(this.ActualWidth - rightOffset, this.ActualHeight - bottomOffset);
        var bottomRightEnd = new Point(this.ActualWidth - maxBottomRight, this.ActualHeight - bottomOffset);
        var bottomRightDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(cornerBrushes.BottomRight, (BorderThickness.Bottom + BorderThickness.Right) / 2.0),
                new PathGeometry([new PathFigure(bottomRightStart, [new QuadraticBezierSegment(bottomRightAnchor, bottomRightEnd, true),], false)]));

        var bottomStart = new Point(maxBottomLeft - overdraw, this.ActualHeight - bottomOffset);
        var bottomEnd = new Point(this.ActualWidth - maxBottomRight + overdraw, this.ActualHeight - bottomOffset);
        var bottomDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(bottomBrush, BorderThickness.Bottom),
                new LineGeometry(bottomStart, bottomEnd));

        var bottomLeftStart = new Point(leftOffset, this.ActualHeight - maxBottomLeft);
        var bottomLeftAnchor = new Point(leftOffset, this.ActualHeight - bottomOffset);
        var bottomLeftEnd = new Point(maxBottomLeft, this.ActualHeight - bottomOffset);
        var bottomLeftDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(cornerBrushes.BottomLeft, (BorderThickness.Bottom + BorderThickness.Left) / 2.0),
                new PathGeometry([
                    new PathFigure(bottomLeftStart, [new QuadraticBezierSegment(bottomLeftAnchor, bottomLeftEnd, true),], false)]));

        const double backgroundOverdraw = 0.64;
        var background = new GeometryDrawing(Background, new Pen(Brushes.Transparent, 0), new PathGeometry(
        [
            new PathFigure(
                    leftEnd with { X = leftEnd.X + leftOffset},
                    [
                        new LineSegment(leftStart with { X = leftStart.X + leftOffset}, true),
                        new QuadraticBezierSegment(new Point(topLeftAnchor.X + leftOffset - backgroundOverdraw, topLeftAnchor.Y + topOffset - backgroundOverdraw), topStart with{ Y = topStart.Y + topOffset}, true),
                        new LineSegment(topEnd with { Y = topEnd.Y + topOffset}, true),
                        new QuadraticBezierSegment(new Point(topRightAnchor.X - rightOffset+ backgroundOverdraw, topRightAnchor.Y + topOffset - backgroundOverdraw), rightStart with { X = rightStart.X - rightOffset }, true),
                        new LineSegment(rightEnd  with { X = rightEnd.X - rightOffset}, true),
                        new QuadraticBezierSegment(new Point(bottomRightAnchor.X - rightOffset + backgroundOverdraw, bottomRightAnchor.Y - bottomOffset+ backgroundOverdraw), bottomEnd with { Y =bottomEnd.Y - bottomOffset}, true),
                        new LineSegment(bottomStart  with { Y = bottomStart.Y - bottomOffset}, true),
                        new QuadraticBezierSegment(new Point(bottomLeftAnchor.X + leftOffset - backgroundOverdraw,  bottomLeftAnchor.Y - bottomOffset+ backgroundOverdraw), leftEnd with { X =leftEnd.X + leftOffset}, true),
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
        var topLeftBrush = GetBrush(sideBrushes.Top, sideBrushes.Left, cornerStops.TopLeft, TopRight, BottomLeft);
        var topRightBrush = GetBrush(sideBrushes.Top, sideBrushes.Right, cornerStops.TopRight, TopLeft, BottomRight);
        var bottomRightBrush = GetBrush(sideBrushes.Bottom, sideBrushes.Right, cornerStops.BottomRight, BottomLeft, TopRight);
        var bottomLeftBrush = GetBrush(sideBrushes.Bottom, sideBrushes.Left, cornerStops.BottomLeft, BottomRight, TopLeft);
        return (topLeftBrush, topRightBrush, bottomRightBrush, bottomLeftBrush);
    }

    private Brush GetBrush(Brush first, Brush second, Stops stops, Point firstPoint, Point secondPoint)
    {
        switch ((first, second))
        {
            case (SolidColorBrush firstBrush, SolidColorBrush secondBrush):
                return new LinearGradientBrush(
                    new GradientStopCollection([
                        new GradientStop(firstBrush.Color, 0),
                            new GradientStop(firstBrush.Color, stops.First),
                            new GradientStop(secondBrush.Color, stops.Second),
                            new GradientStop(secondBrush.Color, 1)
                    ]), firstPoint, secondPoint);
        }

        return first;
    }
}