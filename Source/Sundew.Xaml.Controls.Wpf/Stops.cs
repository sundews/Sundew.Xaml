namespace Sundew.Xaml.Controls;

using System.ComponentModel;

[TypeConverter(typeof(StopsConverter))]
public class Stops
{
    public Stops()
    {
    }

    public Stops(double first, double second)
    {
        this.First = first;
        this.Second = second;
    }

    public double First { get; set; }

    public double Second { get; set; }
}