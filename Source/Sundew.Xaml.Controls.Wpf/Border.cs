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

        // Get DPI scale for device pixel alignment
        var dpi = VisualTreeHelper.GetDpi(this);
        double scaleX = dpi.DpiScaleX;
        double scaleY = dpi.DpiScaleY;

        // Helper to snap to device pixels
        static double SnapToDevice(double value, double scale) => Math.Round(value * scale) / scale;
        Point SnapPoint(Point pt) => new Point(SnapToDevice(pt.X, scaleX), SnapToDevice(pt.Y, scaleY));

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
        var leftStart = SnapPoint(new Point(leftOffset, maxTopLeft - overdraw));
        var leftEnd = SnapPoint(new Point(leftOffset, this.ActualHeight - maxBottomLeft + overdraw));
        var leftDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(leftBrush, BorderThickness.Left),
                new LineGeometry(leftStart, leftEnd));

        var topLeftStart = SnapPoint(new Point(leftOffset, maxTopLeft));
        var topLeftAnchor = SnapPoint(new Point(leftOffset, topOffset));
        var topLeftEnd = SnapPoint(new Point(maxTopLeft, topOffset));
        var topLeftDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(cornerBrushes.TopLeft, (BorderThickness.Top + BorderThickness.Left) / 2.0),
                new PathGeometry([new PathFigure(topLeftStart, [new QuadraticBezierSegment(topLeftAnchor, topLeftEnd, true),], false)]));

        var topStart = SnapPoint(new Point(maxTopLeft - overdraw, topOffset));
        var topEnd = SnapPoint(new Point(this.ActualWidth - maxTopRight + overdraw, topOffset));
        var topDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(topBrush, BorderThickness.Top),
                new LineGeometry(topStart, topEnd));

        var topRightStart = SnapPoint(new Point(this.ActualWidth - maxTopRight, topOffset));
        var topRightAnchor = SnapPoint(new Point(this.ActualWidth - rightOffset, topOffset));
        var topRightEnd = SnapPoint(new Point(this.ActualWidth - rightOffset, maxTopRight));
        var topRightDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(cornerBrushes.TopRight, (BorderThickness.Top + BorderThickness.Right) / 2.0),
                new PathGeometry([new PathFigure(topRightStart, [new QuadraticBezierSegment(topRightAnchor, topRightEnd, true),], false)]));

        var rightStart = SnapPoint(new Point(this.ActualWidth - rightOffset, maxTopRight - overdraw));
        var rightEnd = SnapPoint(new Point(this.ActualWidth - rightOffset, this.ActualHeight - maxBottomRight + overdraw));
        var rightDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(rightBrush, BorderThickness.Right),
                new LineGeometry(rightStart, rightEnd)
            );

        var bottomRightStart = SnapPoint(new Point(this.ActualWidth - rightOffset, this.ActualHeight - maxBottomRight));
        var bottomRightAnchor = SnapPoint(new Point(this.ActualWidth - rightOffset, this.ActualHeight - bottomOffset));
        var bottomRightEnd = SnapPoint(new Point(this.ActualWidth - maxBottomRight, this.ActualHeight - bottomOffset));
        var bottomRightDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(cornerBrushes.BottomRight, (BorderThickness.Bottom + BorderThickness.Right) / 2.0),
                new PathGeometry([new PathFigure(bottomRightStart, [new QuadraticBezierSegment(bottomRightAnchor, bottomRightEnd, true),], false)]));

        var bottomStart = SnapPoint(new Point(maxBottomLeft - overdraw, this.ActualHeight - bottomOffset));
        var bottomEnd = SnapPoint(new Point(this.ActualWidth - maxBottomRight + overdraw, this.ActualHeight - bottomOffset));
        var bottomDrawing =
            new GeometryDrawing(
                Brushes.Transparent,
                new Pen(bottomBrush, BorderThickness.Bottom),
                new LineGeometry(bottomStart, bottomEnd));

        var bottomLeftStart = SnapPoint(new Point(leftOffset, this.ActualHeight - maxBottomLeft));
        var bottomLeftAnchor = SnapPoint(new Point(leftOffset, this.ActualHeight - bottomOffset));
        var bottomLeftEnd = SnapPoint(new Point(maxBottomLeft, this.ActualHeight - bottomOffset));
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
                    SnapPoint(new Point(leftEnd.X + leftOffset, leftEnd.Y)),
                    [
                        new LineSegment(SnapPoint(new Point(leftStart.X + leftOffset, leftStart.Y)), true),
                        new QuadraticBezierSegment(SnapPoint(new Point(topLeftAnchor.X + leftOffset - backgroundOverdraw, topLeftAnchor.Y + topOffset - backgroundOverdraw)), SnapPoint(new Point(topStart.X, topStart.Y + topOffset)), true),
                        new LineSegment(SnapPoint(new Point(topEnd.X, topEnd.Y + topOffset)), true),
                        new QuadraticBezierSegment(SnapPoint(new Point(topRightAnchor.X - rightOffset + backgroundOverdraw, topRightAnchor.Y + topOffset - backgroundOverdraw)), SnapPoint(new Point(rightStart.X - rightOffset, rightStart.Y)), true),
                        new LineSegment(SnapPoint(new Point(rightEnd.X - rightOffset, rightEnd.Y)), true),
                        new QuadraticBezierSegment(SnapPoint(new Point(bottomRightAnchor.X - rightOffset + backgroundOverdraw, bottomRightAnchor.Y - bottomOffset + backgroundOverdraw)), SnapPoint(new Point(bottomEnd.X, bottomEnd.Y - bottomOffset)), true),
                        new LineSegment(SnapPoint(new Point(bottomStart.X, bottomStart.Y - bottomOffset)), true),
                        new QuadraticBezierSegment(SnapPoint(new Point(bottomLeftAnchor.X + leftOffset - backgroundOverdraw, bottomLeftAnchor.Y - bottomOffset + backgroundOverdraw)), SnapPoint(new Point(leftEnd.X + leftOffset, leftEnd.Y)), true),
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