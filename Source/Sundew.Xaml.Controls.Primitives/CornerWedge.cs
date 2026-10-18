// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CornerWedge.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;

/// <summary>
/// The filled region of one corner of a border: the area between the arc rounding the outside of the corner and the
/// arc rounding the inside of it, closed off at either end by the cut it shares with the side next to it.
/// </summary>
/// <remarks>
/// The region is traversed <see cref="Outer"/>.Start, the outer arc to <see cref="Outer"/>.End, a straight cut to
/// <see cref="Inner"/>.Start, the inner arc to <see cref="Inner"/>.End, and then closed back to
/// <see cref="Outer"/>.Start. The inner arc therefore runs the opposite way round the corner to the outer one, and
/// both of its ends are the inner end of a cut. Every corner is traversed from the side entering it to the side
/// leaving it, and both cuts are axis aligned and shared, point for point, with the <see cref="SideQuad"/> on that
/// side, so that the four sides and the four corners tile the whole border without gaps or overlaps.
/// </remarks>
/// <param name="Outer">The arc rounding the outside of the corner.</param>
/// <param name="Inner">The arc rounding the inside of the corner, which bends around the point where the inner edges
/// of the two sides meeting here would have met.</param>
public readonly record struct CornerWedge(CornerArc Outer, CornerArc Inner)
{
    /// <summary>
    /// How far past its own end a slice should be drawn, as a fraction of a slice, so that the slice after it paints
    /// over its antialiased edge instead of letting the background show through the seam between them.
    /// </summary>
    /// <remarks>
    /// Half a slice is well over the pixel it takes to cover an antialiased edge. The overrun is drawn in the colour
    /// the slice ends on, which is the colour the next slice starts with, so painting over it changes nothing.
    /// </remarks>
    public const double SliceOverlap = 0.5;

    /// <summary>
    /// How much of a corner one slice of its blend covers, in device pixels. Small enough that the step from one
    /// slice to the next cannot be told apart from a continuous sweep.
    /// </summary>
    private const double DevicePixelsPerSlice = 4;

    /// <summary>
    /// The most slices a corner is ever cut into, however large it is.
    /// </summary>
    private const int MaximumSliceCount = 20;

    /// <summary>
    /// Gets the point on the inner boundary that the cut from the start of the outer arc leaves.
    /// </summary>
    public Position StartCut => this.Inner.End;

    /// <summary>
    /// Gets the point on the inner boundary that the cut from the end of the outer arc meets.
    /// </summary>
    public Position EndCut => this.Inner.Start;

    /// <summary>
    /// Gets the middle of the cut the corner starts at, which is where a blend swept around the corner is at the
    /// colour of the side entering it.
    /// </summary>
    public Position StartCutCentre => Centre(this.Outer.Start, this.Inner.End);

    /// <summary>
    /// Gets the middle of the cut the corner ends at, which is where a blend swept around the corner is at the colour
    /// of the side leaving it.
    /// </summary>
    public Position EndCutCentre => Centre(this.Outer.End, this.Inner.Start);

    /// <summary>
    /// Gets the number of slices the corner should be cut into to blend the two sides meeting it smoothly.
    /// </summary>
    /// <remarks>
    /// A single linear gradient cannot blend a corner without a seam, because its bands of constant colour would have
    /// to run along both cuts at once and the two cuts are at right angles. Sweeping the blend around the corner
    /// instead, a slice at a time, puts the side's own colour along the whole of each cut.
    /// </remarks>
    /// <param name="scale">The device pixel scale.</param>
    /// <returns>The number of slices, at least one.</returns>
    public int GetSliceCount(Scale scale)
    {
        var span =
            (Math.Abs(this.Outer.End.X - this.Outer.Start.X) * Math.Max(scale.X, 0)) +
            (Math.Abs(this.Outer.End.Y - this.Outer.Start.Y) * Math.Max(scale.Y, 0));
        var slices = (int)Math.Ceiling(span / DevicePixelsPerSlice);
        return Math.Max(1, Math.Min(slices, MaximumSliceCount));
    }

    /// <summary>
    /// Gets the slice of the corner between the two specified positions around it, as a wedge in its own right.
    /// </summary>
    /// <remarks>
    /// Neighbouring slices share a cut point for point, so the slices of a corner tile it the same way the corners
    /// and sides tile the border.
    /// </remarks>
    /// <param name="start">The position the slice starts at, where 0 is the side entering the corner.</param>
    /// <param name="end">The position the slice ends at, where 1 is the side leaving the corner.</param>
    /// <returns>The slice.</returns>
    public CornerWedge GetSlice(double start, double end)
    {
        // The inner arc runs the other way round the corner, so it is sectioned from the other end.
        return new CornerWedge(this.Outer.Section(start, end), this.Inner.Section(1 - end, 1 - start));
    }

    private static Position Centre(Position first, Position second)
    {
        return new Position((first.X + second.X) / 2, (first.Y + second.Y) / 2);
    }
}
