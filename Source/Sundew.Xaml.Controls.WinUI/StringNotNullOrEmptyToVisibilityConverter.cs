// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringNotNullOrEmptyToVisibilityConverter.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using Microsoft.UI.Xaml.Data;

/// <summary>
/// A converter that converts a string to <see cref="Visibility.Visible"/> when the string is neither null nor empty,
/// and to <see cref="NotVisibleVisibility"/> when it is.
/// </summary>
/// <remarks>
/// <para>
/// This converts to this library's <see cref="Visibility"/> and not the framework's, so it is bound to
/// <see cref="Appearance.VisibilityProperty"/> rather than to the element's own visibility. That is what gives a
/// binding the fourth state: <see cref="NotVisibleVisibility"/> may be said as <see cref="Visibility.Invisible"/>,
/// and the element then keeps the space it was laid out into and stays in the automation tree while its string is
/// empty, which is what neither <see cref="Visibility.Collapsed"/> nor <see cref="Visibility.Hidden"/> does.
/// </para>
/// <para>
/// <see cref="NotVisibleVisibility"/> is <see cref="Visibility.Collapsed"/> unless it is said otherwise, which is
/// what a converter from a string to a visibility is usually taken to mean. A value that is not a string is not a
/// string that is there, and is converted the way an empty one is.
/// </para>
/// </remarks>
public sealed partial class StringNotNullOrEmptyToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets how the element is to be seen while the string is null or empty.
    /// The default is <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public Visibility NotVisibleVisibility { get; set; } = Visibility.Collapsed;

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
    /// <param name="language">The language.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="NotSupportedException">The method in not supported.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
