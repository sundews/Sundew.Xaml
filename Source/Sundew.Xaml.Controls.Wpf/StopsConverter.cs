// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StopsConverter.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System.ComponentModel;
using System.Globalization;
using System.Text;

/// <summary>
/// Converts to and from <see cref="Stops"/>.
/// </summary>
public class StopsConverter : TypeConverter
{
    /// <summary>
    /// Determines whether this instance can convert from the specified source type.
    /// </summary>
    /// <param name="typeDescriptorContext">The type descriptor context.</param>
    /// <param name="sourceType">The source type.</param>
    /// <returns><c>true</c>, if the source type can be converted, otherwise <c>false</c>.</returns>
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
    /// Determines whether this converter can convert an object to the specified destination type, using the provided
    /// context.
    /// </summary>
    /// <param name="typeDescriptorContext">The type descriptor context.</param>
    /// <param name="destinationType">The destination type.</param>
    /// <returns><c>true</c>, if the destination type can be converted, otherwise <c>false</c>.</returns>
    public override bool CanConvertTo(ITypeDescriptorContext? typeDescriptorContext, Type? destinationType)
    {
        return destinationType == typeof(string);
    }

    /// <summary>
    /// Converts the given object to the type of this converter, using the specified context and culture.
    /// </summary>
    /// <param name="typeDescriptorContext">The type descriptor context.</param>
    /// <param name="cultureInfo">The culture info.</param>
    /// <param name="source">The source.</param>
    /// <returns>The converted value.</returns>
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

    /// <summary>
    /// Converts a Stops object to the specified destination type, typically a string representation.
    /// </summary>
    /// <param name="typeDescriptorContext">The type descriptor context.</param>
    /// <param name="cultureInfo">The culture info.</param>
    /// <param name="value">The value.</param>
    /// <param name="destinationType">The destination type.</param>
    /// <returns>An object representing the converted value. Returns a string if destinationType is typeof(string).</returns>
    /// <exception cref="ArgumentException">Thrown if value is not of type Stops, or if the conversion to the specified destinationType is not supported.</exception>
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

    private static char GetListSeparator(CultureInfo? cultureInfo)
    {
        if (cultureInfo == null)
        {
            return ',';
        }

        return cultureInfo.NumberFormat.NumberDecimalSeparator == "," ? '.' : ',';
    }
}