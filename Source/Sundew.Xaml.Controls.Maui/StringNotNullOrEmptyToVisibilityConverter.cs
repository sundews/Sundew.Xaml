// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringNotNullOrEmptyToVisibilityConverter.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System.Globalization;
using Microsoft.Maui.Controls;

/// <summary>
/// A converter that converts a string to <see cref="Visibility.Visible"/> when the string is neither null nor empty,
/// and to <see cref="NotVisibleVisibility"/> when it is.
/// </summary>
/// <remarks>
/// <para>
/// This converts to this library's <see cref="Visibility"/>, which MAUI itself has no counterpart for, so it is
/// bound to <see cref="Appearance.VisibilityProperty"/> rather than to a view's own <c>IsVisible</c>. That is what
/// gives a binding the states MAUI's single boolean cannot say: <see cref="NotVisibleVisibility"/> may be said as
/// <see cref="Visibility.Invisible"/>, and the view then keeps the space it was laid out into and is still read out
/// while its string is empty, or as <see cref="Visibility.Hidden"/> to keep the space and give up the rest.
/// <c>IsVisible</c> on its own can only say <see cref="Visibility.Collapsed"/>.
/// </para>
/// <para>
/// <see cref="NotVisibleVisibility"/> is <see cref="Visibility.Collapsed"/> unless it is said otherwise, which is
/// what a converter from a string to a visibility is usually taken to mean, and is what <c>IsVisible</c> would have
/// come to. A value that is not a string is not a string that is there, and is converted the way an empty one is.
/// </para>
/// </remarks>
public sealed class StringNotNullOrEmptyToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets how the view is to be seen while the string is null or empty.
    /// The default is <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public Visibility NotVisibleVisibility { get; set; } = Visibility.Collapsed;

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
        return value is string stringValue && !string.IsNullOrEmpty(stringValue)
            ? Visibility.Visible
            : this.NotVisibleVisibility;
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
