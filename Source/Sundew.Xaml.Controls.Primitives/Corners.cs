// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Corners.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

/// <summary>
/// A value per corner of a rectangle.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
/// <param name="TopLeft">The value for the top left corner.</param>
/// <param name="TopRight">The value for the top right corner.</param>
/// <param name="BottomRight">The value for the bottom right corner.</param>
/// <param name="BottomLeft">The value for the bottom left corner.</param>
public readonly record struct Corners<T>(T TopLeft, T TopRight, T BottomRight, T BottomLeft)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Corners{T}"/> struct with the same value on all corners.
    /// </summary>
    /// <param name="value">The value.</param>
    public Corners(T value)
        : this(value, value, value, value)
    {
    }
}
