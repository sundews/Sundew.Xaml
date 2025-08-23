namespace Sundew.Xaml.Controls;

using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Media;

public class SideBrushesConverter : TypeConverter
{
    private readonly BrushConverter brushConverter = new BrushConverter();

    public override bool CanConvertFrom(ITypeDescriptorContext? typeDescriptorContext, Type sourceType)
    {
        var typeCode = Type.GetTypeCode(sourceType);
        return typeCode switch
        {
            TypeCode.String => true,
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
            var listSeparator = GetListSeparator(cultureInfo);

            var values = text.Split(listSeparator);

            switch (values.Length)
            {
                case 1:
                    var first = (Brush)this.brushConverter.ConvertFrom(typeDescriptorContext, cultureInfo, values[0]);
                    return new SideBrushes(first);
                case 2:
                    return new SideBrushes((Brush)this.brushConverter.ConvertFrom(typeDescriptorContext, cultureInfo, values[0]), (Brush)this.brushConverter.ConvertFrom(typeDescriptorContext, cultureInfo, values[1]));
                case 4:
                    return new SideBrushes(
                        (Brush)this.brushConverter.ConvertFrom(typeDescriptorContext, cultureInfo, values[0]),
                        (Brush)this.brushConverter.ConvertFrom(typeDescriptorContext, cultureInfo, values[1]),
                        (Brush)this.brushConverter.ConvertFrom(typeDescriptorContext, cultureInfo, values[2]),
                        (Brush)this.brushConverter.ConvertFrom(typeDescriptorContext, cultureInfo, values[3]));
            }
        }

        throw new FormatException("Invalid SideBrushes");
    }

    public override object ConvertTo(ITypeDescriptorContext? typeDescriptorContext, CultureInfo? cultureInfo, object? value, Type destinationType)
    {
        if (!(value is SideBrushes sideBrushes))
        {
            throw new ArgumentException("Value must be SideBrushes", nameof(value));
        }

        if (destinationType == typeof(string))
        {
            var listSeparator = GetListSeparator(cultureInfo);
            var stringBuilder = new StringBuilder(128);
            stringBuilder.Append(this.brushConverter.ConvertTo(typeDescriptorContext, cultureInfo, sideBrushes.Left, destinationType));
            stringBuilder.Append(listSeparator);
            stringBuilder.Append(this.brushConverter.ConvertTo(typeDescriptorContext, cultureInfo, sideBrushes.Top, destinationType));
            stringBuilder.Append(listSeparator);
            stringBuilder.Append(this.brushConverter.ConvertTo(typeDescriptorContext, cultureInfo, sideBrushes.Right, destinationType));
            stringBuilder.Append(listSeparator);
            stringBuilder.Append(this.brushConverter.ConvertTo(typeDescriptorContext, cultureInfo, sideBrushes.Bottom, destinationType));
            return stringBuilder.ToString();
        }

        throw new ArgumentException("Could not convert type", nameof(value));
    }


    private static char GetListSeparator(CultureInfo? cultureInfo)
    {
        if (cultureInfo == null)
        {
            return ',';
        }

        return cultureInfo.NumberFormat.NumberDecimalSeparator == "," ? '.' : ',';
    }
}