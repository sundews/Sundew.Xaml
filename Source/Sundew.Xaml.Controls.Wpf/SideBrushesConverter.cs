// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SideBrushesConverter.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Media;

/// <summary>
/// Converts to and from <see cref="SideBrushes"/>.
/// </summary>
public class SideBrushesConverter : TypeConverter
{
    private readonly BrushConverter brushConverter = new BrushConverter();

    /// <summary>
    /// Determines whether this instance can convert from the specified type descriptor context and source type.
    /// </summary>
    /// <param name="typeDescriptorContext">The type descriptor context.</param>
    /// <param name="sourceType">The source type.</param>
    /// <returns><c>true</c>, if the source type can be converted, otherwise <c>false</c>.</returns>
    public override bool CanConvertFrom(ITypeDescriptorContext? typeDescriptorContext, Type sourceType)
    {
        var typeCode = Type.GetTypeCode(sourceType);
        return typeCode switch
        {
            TypeCode.String => true,
            _ => false,
        };
    }

    /// <summary>
    /// Determines whether this converter can convert an object to the specified destination type, using the provided context.
    /// </summary>
    /// <param name="typeDescriptorContext">The type descriptor context.</param>
    /// <param name="destinationType">The destination type.</param>
    /// <returns><c>true</c>, if the destination type is string; otherwise, <c>false</c>.</returns>
    public override bool CanConvertTo(ITypeDescriptorContext? typeDescriptorContext, Type? destinationType)
    {
        return destinationType == typeof(string);
    }

    /// <summary>
    /// Converts the given object to the type of this converter, using the specified context and culture information.
    /// </summary>
    /// <param name="typeDescriptorContext">The type descriptor context.</param>
    /// <param name="cultureInfo">The culture info.</param>
    /// <param name="source">The source.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="FormatException">Thrown if the source cannot be converted.</exception>
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

    /// <summary>
    /// Converts a SideBrushes object to the specified destination type, using the provided context and culture information.
    /// </summary>
    /// <param name="typeDescriptorContext">The type descriptor context.</param>
    /// <param name="cultureInfo">The culture info.</param>
    /// <param name="value">The value.</param>
    /// <param name="destinationType">The destination type.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="ArgumentException">Thrown if value is not a SideBrushes instance, or if the conversion to the specified destinationType is not supported.</exception>
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