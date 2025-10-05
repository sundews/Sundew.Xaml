// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CornerStopsConverter.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System.ComponentModel;
using System.Globalization;
using System.Text;

/// <summary>
/// Converts to and from <see cref="CornerStops"/>.
/// </summary>
public class CornerStopsConverter : TypeConverter
{
    /// <summary>
    /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
    /// </summary>
    /// <param name="typeDescriptorContext">The type descriptor context.</param>
    /// <param name="sourceType">The source type.</param>
    /// <returns><c>true</c>, if type can be converted, otherwise <c>false</c>.</returns>
    public override bool CanConvertFrom(ITypeDescriptorContext? typeDescriptorContext, Type sourceType)
    {
        var typeCode = Type.GetTypeCode(sourceType);
        return typeCode switch
        {
            TypeCode.String or TypeCode.Decimal or TypeCode.Single or TypeCode.Double or TypeCode.Int16
                or TypeCode.Int32 or TypeCode.Int64 or TypeCode.UInt16 or TypeCode.UInt32
                or TypeCode.UInt64 => true,
            _ => false,
        };
    }

    /// <summary>
    /// Determines whether this converter can convert an object to the specified destination type.
    /// </summary>
    /// <param name="typeDescriptorContext">The type descriptor context.</param>
    /// <param name="destinationType">The destination type.</param>
    /// <returns><c>true</c>, if type can be converted, otherwise <c>false</c>.</returns>
    public override bool CanConvertTo(ITypeDescriptorContext? typeDescriptorContext, Type? destinationType)
    {
        return destinationType == typeof(string);
    }

    /// <summary>
    /// Converts the specified value to a CornerStops instance, using the provided culture-specific formatting
    /// information.
    /// </summary>
    /// <param name="typeDescriptorContext">The type descriptor context.</param>
    /// <param name="cultureInfo">The culture info.</param>
    /// <param name="source">The source.</param>
    /// <returns>A CornerStops instance created from the specified value.</returns>
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

    /// <summary>
    /// Converts a CornerStops object to the specified destination type, using the provided context and culture
    /// information.
    /// </summary>
    /// <param name="typeDescriptorContext">The type descriptor context.</param>
    /// <param name="cultureInfo">The culture info.</param>
    /// <param name="value">The value.</param>
    /// <param name="destinationType">The destination type.</param>
    /// <returns>The corner stops as a string.</returns>
    /// <exception cref="ArgumentException">Thrown if value is not of type CornerStops, or if destinationType is not supported.</exception>
    public override object ConvertTo(ITypeDescriptorContext? typeDescriptorContext, CultureInfo? cultureInfo, object? value, Type destinationType)
    {
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
                    new Stops(Convert.ToDouble(values[0], NumberFormatInfo.InvariantInfo), Convert.ToDouble(values[1], NumberFormatInfo.InvariantInfo)),
                    new Stops(Convert.ToDouble(values[2], NumberFormatInfo.InvariantInfo), Convert.ToDouble(values[3], NumberFormatInfo.InvariantInfo)));
            case 8:
                return new CornerStops(
                    new Stops(Convert.ToDouble(values[0], NumberFormatInfo.InvariantInfo), Convert.ToDouble(values[1], NumberFormatInfo.InvariantInfo)),
                    new Stops(Convert.ToDouble(values[2], NumberFormatInfo.InvariantInfo), Convert.ToDouble(values[3], NumberFormatInfo.InvariantInfo)),
                    new Stops(Convert.ToDouble(values[4], NumberFormatInfo.InvariantInfo), Convert.ToDouble(values[5], NumberFormatInfo.InvariantInfo)),
                    new Stops(Convert.ToDouble(values[6], NumberFormatInfo.InvariantInfo), Convert.ToDouble(values[7], NumberFormatInfo.InvariantInfo)));
        }

        throw new FormatException("Invalid CornerStops");
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