namespace Sundew.Xaml.Controls;

using System.ComponentModel;

[TypeConverter(typeof(CornerStopsConverter))]
public class CornerStops
{
    public CornerStops(Stops stops)
        : this(stops, stops, stops, stops)
    {
    }


    public CornerStops(Stops topLeft, Stops bottomRight)
        : this(topLeft, topLeft, bottomRight, bottomRight)
    {
    }

    public CornerStops(Stops topLeft, Stops topRight, Stops bottomRight, Stops bottomLeft)
    {
        this.TopLeft = topLeft;
        this.TopRight = topRight;
        this.BottomRight = bottomRight;
        this.BottomLeft = bottomLeft;
    }

    public Stops TopLeft { get; set; }

    public Stops TopRight { get; set; }

    public Stops BottomRight { get; set; }

    public Stops BottomLeft { get; set; }
}
