namespace Sundew.Xaml.Controls.Wpf;

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