namespace Sundew.Xaml.Controls;

using System.ComponentModel;
using System.Windows.Media;

[TypeConverter(typeof(SideBrushesConverter))]
public class SideBrushes
{
    public SideBrushes()
    : this(Brushes.Black)
    {
    }

    public SideBrushes(Brush? brush)
        : this(brush, brush, brush, brush)
    {
    }

    public SideBrushes(Brush? topLeft, Brush? bottomRight)
        : this(topLeft, topLeft, bottomRight, bottomRight)
    {
    }

    public SideBrushes(Brush? left, Brush? top, Brush? bottom, Brush? right)
    {
        this.Left = left;
        this.Top = top;
        this.Right = bottom;
        this.Bottom = right;
    }

    public Brush? Bottom { get; set; }

    public Brush? Right { get; set; }

    public Brush? Top { get; set; }

    public Brush? Left { get; set; }
}