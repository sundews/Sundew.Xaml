// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringNotNullOrEmptyToBooleanConverter.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using System.Globalization;
using global::Avalonia.Data.Converters;

/// <summary>
/// A converter that converts a string to <c>true</c> if the string is not null or empty, otherwise to <c>false</c>.
/// </summary>
/// <remarks>
/// Avalonia has no visibility enumeration; a visual is shown or not by a boolean, which is what this converts to.
/// </remarks>
public sealed class StringNotNullOrEmptyToBooleanConverter : IValueConverter
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
        return value is string stringValue && !string.IsNullOrEmpty(stringValue);
    }

    /// <summary>
    /// Converts a value back to its original type for use in two-way data binding scenarios.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="NotSupportedException">The method in not supported.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
