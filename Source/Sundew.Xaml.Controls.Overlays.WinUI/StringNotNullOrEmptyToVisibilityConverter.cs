// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringNotNullOrEmptyToVisibilityConverter.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

/// <summary>
/// A converter that converts a string to <see cref="Visibility.Visible"/> if the string is not null or empty, otherwise to <see cref="Visibility.Collapsed"/>.
/// </summary>
public sealed partial class StringNotNullOrEmptyToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="language">The language.</param>
    /// <returns>The converted value.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string stringValue)
        {
            return !string.IsNullOrEmpty(stringValue) ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    /// <summary>
    /// Converts a value back to its original type for use in two-way data binding scenarios.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="language">The language.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="NotSupportedException">The method in not supported.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
