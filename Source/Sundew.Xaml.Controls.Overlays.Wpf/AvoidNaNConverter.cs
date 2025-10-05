// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AvoidNaNConverter.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using System.Globalization;
using System.Windows.Data;

/// <summary>
/// A converter that converts NaN and Infinity to 0.
/// </summary>
public sealed class AvoidNaNConverter : IValueConverter
{
    /// <summary>
    /// Converts the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The converted value.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Convert(value);
    }

    /// <summary>
    /// Converts a value back to its original type for use in two-way data binding scenarios.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The converted value.</returns>
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