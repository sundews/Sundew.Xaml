// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SideQuad.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;

/// <summary>
/// The filled region of the straight section of one side of a border: the rectangle between the outer boundary and the
/// inner one.
/// </summary>
/// <remarks>
/// The region is traversed <see cref="Outer"/>.Start, <see cref="Outer"/>.End, <see cref="Inner"/>.End,
/// <see cref="Inner"/>.Start and is then closed, which runs clockwise for every side.
/// </remarks>
/// <param name="Outer">The run along the outside of the border.</param>
/// <param name="Inner">The same run along the inside of the border, inset by this side's own thickness.</param>
public readonly record struct SideQuad(Edge Outer, Edge Inner)
{
    private const double ThicknessTolerance = 0.01;

    /// <summary>
    /// How far a side is drawn past each of its own ends, in device pixels, so that the corner drawn over it has
    /// something opaque to be antialiased against rather than the page behind the border.
    /// </summary>
    private const double SeamOverlap = 0.75;

    /// <summary>
    /// Gets a value indicating whether the region covers any area, which it does not when the side has no thickness or
    /// when the corners either side of it have taken all the room.
    /// </summary>
    public bool HasArea =>
        this.Outer.HasLength &&
        (Math.Abs(this.Inner.Start.X - this.Outer.Start.X) > ThicknessTolerance ||
         Math.Abs(this.Inner.Start.Y - this.Outer.Start.Y) > ThicknessTolerance);

    /// <summary>
    /// Gets the side drawn a little past both of its ends, so that it runs under the two corners instead of stopping
    /// exactly where they begin.
    /// </summary>
    /// <remarks>
    /// A side and the corner next to it share a cut. Drawn as two shapes that merely meet along it, each is
    /// antialiased on its own and each covers only part of the pixels the cut runs through, so the page behind shows
    /// between them wherever the cut does not land exactly on a pixel boundary. Running the side on underneath the
    /// corner, which is drawn after it, leaves the corner's edge blending against the side's own colour instead.
    /// The two are the same colour along the cut, so the overlap itself cannot be seen.
    /// The overrun is along the side's own axis only, so it stays within the corner's arc rather than poking out of it.
    /// </remarks>
    /// <param name="scale">The device pixel scale.</param>
    /// <returns>The extended side.</returns>
    public SideQuad ExtendIntoCorners(Scale scale)
    {
        var alongX = Math.Sign(this.Outer.End.X - this.Outer.Start.X) * (SeamOverlap / Positive(scale.X));
        var alongY = Math.Sign(this.Outer.End.Y - this.Outer.Start.Y) * (SeamOverlap / Positive(scale.Y));
        return new SideQuad(
            new Edge(this.Outer.Start.Translate(-alongX, -alongY), this.Outer.End.Translate(alongX, alongY)),
            new Edge(this.Inner.Start.Translate(-alongX, -alongY), this.Inner.End.Translate(alongX, alongY)));

        static double Positive(double value)
        {
            return value > 0 ? value : 1;
        }
    }
}
