// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SideBrushesConverter.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;
using System.ComponentModel;
using System.Globalization;

/// <summary>
/// Converts to and from <see cref="SideBrushes"/>.
/// </summary>
public class SideBrushesConverter : TypeConverter
{
    /// <summary>
    /// Determines whether this instance can convert from the specified type descriptor context and source type.
    /// </summary>
    /// <param name="typeDescriptorContext">The type descriptor context.</param>
    /// <param name="sourceType">The source type.</param>
    /// <returns><c>true</c>, if the source type can be converted, otherwise <c>false</c>.</returns>
    public override bool CanConvertFrom(ITypeDescriptorContext? typeDescriptorContext, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    /// <summary>
    /// Converts the given object to the type of this converter.
    /// </summary>
    /// <param name="typeDescriptorContext">The type descriptor context.</param>
    /// <param name="cultureInfo">The culture info.</param>
    /// <param name="source">The source.</param>
    /// <returns>The converted value.</returns>
    public override object ConvertFrom(ITypeDescriptorContext? typeDescriptorContext, CultureInfo? cultureInfo, object? source)
    {
        if (source is not string text)
        {
            throw this.GetConvertFromException(source);
        }

        return SideBrushes.Parse(text);
    }
}
