namespace Sundew.Xaml.Controls.Overlays;

using System.Globalization;
using System.Windows.Data;

public sealed class AvoidNaNConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Convert(value);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Convert(value);
    }

    private static object? Convert(object? value)
    {
        if (value is not double length)
        {
            return 0;
        }

        if (double.IsNaN(length) || double.IsInfinity(length))
        {
            return 0;
        }

        return length;
    }
}