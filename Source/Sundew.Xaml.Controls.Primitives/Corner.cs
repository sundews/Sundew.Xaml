// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Corner.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

/// <summary>
/// Identifies a corner of a rectangle.
/// </summary>
public enum Corner
{
    /// <summary>
    /// The top left corner, which blends the left side into the top side.
    /// </summary>
    TopLeft,

    /// <summary>
    /// The top right corner, which blends the top side into the right side.
    /// </summary>
    TopRight,

    /// <summary>
    /// The bottom right corner, which blends the right side into the bottom side.
    /// </summary>
    BottomRight,

    /// <summary>
    /// The bottom left corner, which blends the bottom side into the left side.
    /// </summary>
    BottomLeft,
}
