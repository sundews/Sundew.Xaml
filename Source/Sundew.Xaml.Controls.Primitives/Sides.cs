// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Sides.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

/// <summary>
/// A value per side of a rectangle.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
/// <param name="Left">The value for the left side.</param>
/// <param name="Top">The value for the top side.</param>
/// <param name="Right">The value for the right side.</param>
/// <param name="Bottom">The value for the bottom side.</param>
public readonly record struct Sides<T>(T Left, T Top, T Right, T Bottom)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Sides{T}"/> struct with the same value on all sides.
    /// </summary>
    /// <param name="value">The value.</param>
    public Sides(T value)
        : this(value, value, value, value)
    {
    }
}
