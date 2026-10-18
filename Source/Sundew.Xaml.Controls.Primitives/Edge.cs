// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Edge.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;

/// <summary>
/// A straight segment of a border, running between two corners.
/// </summary>
/// <param name="Start">The start of the segment.</param>
/// <param name="End">The end of the segment.</param>
public readonly record struct Edge(Position Start, Position End)
{
    private const double LengthTolerance = 0.01;

    /// <summary>
    /// Gets a value indicating whether the segment is long enough to be worth drawing.
    /// </summary>
    /// <remarks>
    /// Border edges are axis aligned, so a segment that varies on neither axis has collapsed between its corners.
    /// </remarks>
    public bool HasLength =>
        Math.Abs(this.End.X - this.Start.X) > LengthTolerance ||
        Math.Abs(this.End.Y - this.Start.Y) > LengthTolerance;

    /// <summary>
    /// Gets an edge translated by the specified amounts.
    /// </summary>
    /// <param name="x">The horizontal translation.</param>
    /// <param name="y">The vertical translation.</param>
    /// <returns>The translated edge.</returns>
    public Edge Translate(double x, double y)
    {
        return new Edge(this.Start.Translate(x, y), this.End.Translate(x, y));
    }
}
