namespace Sundew.Xaml.Controls.Wpf;

using System.ComponentModel;
using System.Globalization;
using System.Text;
public class CornerStopsConverter : TypeConverter
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
        return new CornerStops(new Stops(firstAndSecond, firstAndSecond));
    }

    public override object ConvertTo(ITypeDescriptorContext? typeDescriptorContext, CultureInfo? cultureInfo, object? value, Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(destinationType);

        if (!(value is CornerStops))
        {
            throw new ArgumentException("Value must be CornerStops", nameof(value));
        }

        var cornerStops = (CornerStops)value;
        if (destinationType == typeof(string))
        {
            return ToString(cornerStops, cultureInfo);
        }

        throw new ArgumentException("Could not convert type", nameof(value));
    }



    internal static string ToString(CornerStops cornerStops, CultureInfo? cultureInfo)
    {
        var listSeparator = GetListSeparator(cultureInfo);
        var stringBuilder = new StringBuilder(128);
        stringBuilder.Append(cornerStops.TopLeft.First.ToString(NumberFormatInfo.InvariantInfo));
        stringBuilder.Append(listSeparator);
        stringBuilder.Append(cornerStops.TopLeft.Second.ToString(NumberFormatInfo.InvariantInfo));
        stringBuilder.Append(listSeparator);
        stringBuilder.Append(cornerStops.TopRight.First.ToString(NumberFormatInfo.InvariantInfo));
        stringBuilder.Append(listSeparator);
        stringBuilder.Append(cornerStops.TopRight.Second.ToString(NumberFormatInfo.InvariantInfo));
        stringBuilder.Append(listSeparator);
        stringBuilder.Append(cornerStops.BottomRight.First.ToString(NumberFormatInfo.InvariantInfo));
        stringBuilder.Append(listSeparator);
        stringBuilder.Append(cornerStops.BottomRight.Second.ToString(NumberFormatInfo.InvariantInfo));
        stringBuilder.Append(listSeparator);
        stringBuilder.Append(cornerStops.BottomLeft.First.ToString(NumberFormatInfo.InvariantInfo));
        stringBuilder.Append(listSeparator);
        stringBuilder.Append(cornerStops.BottomLeft.Second.ToString(NumberFormatInfo.InvariantInfo));
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

    internal static CornerStops FromString(string s, CultureInfo? cultureInfo)
    {
        var listSeparator = GetListSeparator(cultureInfo);

        var values = s.Split(listSeparator);

        switch (values.Length)
        {
            case 1:
                var first = Convert.ToDouble(values[0], NumberFormatInfo.InvariantInfo);
                return new CornerStops(new Stops(first, first));
            case 2:
                return new CornerStops(new Stops(Convert.ToDouble(values[0], NumberFormatInfo.InvariantInfo), Convert.ToDouble(values[1], NumberFormatInfo.InvariantInfo)));
            case 4:
                return new CornerStops(
                    new Stops(Convert.ToDouble(values[0], NumberFormatInfo.InvariantInfo),
                        Convert.ToDouble(values[1], NumberFormatInfo.InvariantInfo)),
                    new Stops(Convert.ToDouble(values[2], NumberFormatInfo.InvariantInfo),
                        Convert.ToDouble(values[3], NumberFormatInfo.InvariantInfo)));
            case 8:
                return new CornerStops(
                    new Stops(Convert.ToDouble(values[0], NumberFormatInfo.InvariantInfo), Convert.ToDouble(values[1], NumberFormatInfo.InvariantInfo)),
                    new Stops(Convert.ToDouble(values[2], NumberFormatInfo.InvariantInfo), Convert.ToDouble(values[3], NumberFormatInfo.InvariantInfo)),
                    new Stops(Convert.ToDouble(values[4], NumberFormatInfo.InvariantInfo), Convert.ToDouble(values[5], NumberFormatInfo.InvariantInfo)),
                    new Stops(Convert.ToDouble(values[6], NumberFormatInfo.InvariantInfo), Convert.ToDouble(values[7], NumberFormatInfo.InvariantInfo)));
        }

        throw new FormatException("Invalid CornerStops");
    }
}