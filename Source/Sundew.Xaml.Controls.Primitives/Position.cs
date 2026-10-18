// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Position.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

/// <summary>
/// A point in a two dimensional coordinate space.
/// </summary>
/// <param name="X">The horizontal coordinate.</param>
/// <param name="Y">The vertical coordinate.</param>
public readonly record struct Position(double X, double Y)
{
    /// <summary>
    /// Gets a position translated by the specified amounts.
    /// </summary>
    /// <param name="x">The horizontal translation.</param>
    /// <param name="y">The vertical translation.</param>
    /// <returns>The translated position.</returns>
    public Position Translate(double x, double y)
    {
        return new Position(this.X + x, this.Y + y);
    }
}
