namespace Sundew.Xaml.Controls;

using System.ComponentModel;
using System.Globalization;
using System.Text;

public class StopsConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? typeDescriptorContext, Type sourceType)
    {
        var typeCode = Type.GetTypeCode(sourceType);
        return typeCode switch
        {
            TypeCode.String or TypeCode.Decimal or TypeCode.Single or TypeCode.Double or TypeCode.Int16
                or TypeCode.Int32 or TypeCode.Int64 or TypeCode.UInt16 or TypeCode.UInt32
                or TypeCode.UInt64 => true,
            _ => false
        };
    }


    public override bool CanConvertTo(ITypeDescriptorContext? typeDescriptorContext, Type? destinationType)
    {
        return destinationType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext? typeDescriptorContext, CultureInfo? cultureInfo, object? source)
    {
        if (source == null)
        {
            throw this.GetConvertFromException(source);
        }

        if (source is string text)
        {
            return FromString(text, cultureInfo);
        }

        var firstAndSecond = Convert.ToDouble(source, cultureInfo);
        return new Stops(firstAndSecond, firstAndSecond);
    }

    public override object ConvertTo(ITypeDescriptorContext? typeDescriptorContext, CultureInfo? cultureInfo, object? value, Type destinationType)
    {
        if (!(value is Stops))
        {
            throw new ArgumentException("Value must be Stops", nameof(value));
        }

        var cornerStops = (Stops)value;
        if (destinationType == typeof(string))
        {
            return ToString(cornerStops, cultureInfo);
        }

        throw new ArgumentException("Could not convert type", nameof(value));
    }



    internal static string ToString(Stops stops, CultureInfo? cultureInfo)
    {
        var listSeparator = GetListSeparator(cultureInfo);
        var stringBuilder = new StringBuilder(48);
        stringBuilder.Append(stops.First.ToString(NumberFormatInfo.InvariantInfo));
        stringBuilder.Append(listSeparator);
        stringBuilder.Append(stops.Second.ToString(NumberFormatInfo.InvariantInfo));
        return stringBuilder.ToString();
    }

    private static char GetListSeparator(CultureInfo? cultureInfo)
    {
        if (cultureInfo == null)
        {
            return ',';
        }

        return cultureInfo.NumberFormat.NumberDecimalSeparator == "," ? '.' : ',';
    }

    internal static Stops FromString(string s, CultureInfo? cultureInfo)
    {
        var listSeparator = GetListSeparator(cultureInfo);

        var values = s.Split(listSeparator);

        switch (values.Length)
        {
            case 1:
                var first = Convert.ToDouble(values[0], NumberFormatInfo.InvariantInfo);
                return new Stops(first, first);
            case 2:
                return new Stops(Convert.ToDouble(values[0], NumberFormatInfo.InvariantInfo), Convert.ToDouble(values[1], NumberFormatInfo.InvariantInfo));
        }

        throw new FormatException("Invalid Stops");
    }
}