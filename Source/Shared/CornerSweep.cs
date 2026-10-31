// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CornerSweep.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;

/// <summary>
/// The slices a corner's blend is swept around it in.
/// </summary>
/// <remarks>
/// A corner is cut finely enough that one linear gradient per slice cannot be told apart from a continuous sweep, but
/// the blend does not move everywhere along the sweep: outside the two <see cref="Stops"/> it holds at the colour of
/// the side, so the slices there are all the same one colour. Those are handed out as a single slice instead, which
/// is the same picture drawn in fewer fills, and a flat fill rather than a gradient one at that.
/// </remarks>
public readonly record struct CornerSweep
{
    private readonly CornerWedge wedge;
    private readonly Stops stops;
    private readonly int count;

    /// <summary>
    /// Initializes a new instance of the <see cref="CornerSweep"/> struct.
    /// </summary>
    /// <param name="wedge">The corner to sweep.</param>
    /// <param name="stops">The stops controlling how abrupt the transition is.</param>
    /// <param name="scale">The device pixel scale, which decides how fine the sweep has to be.</param>
    public CornerSweep(CornerWedge wedge, Stops stops, Scale scale)
    {
        if (stops == null)
        {
            throw new ArgumentNullException(nameof(stops));
        }

        this.wedge = wedge;
        this.stops = stops;
        this.count = wedge.GetSliceCount(scale);
    }

    /// <summary>
    /// Gets the most slices the sweep can hand out, which is what it would be without any of them being merged.
    /// </summary>
    public int MaximumCount => this.count;

    /// <summary>
    /// Returns an enumerator over the slices.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this.wedge, this.stops, this.count);
    }

    /// <summary>
    /// Walks the slices of a sweep, merging the consecutive ones the blend does not move across.
    /// </summary>
    public struct Enumerator
    {
        private readonly CornerWedge wedge;
        private readonly Stops stops;
        private readonly int count;
        private int index;

        internal Enumerator(CornerWedge wedge, Stops stops, int count)
        {
            this.wedge = wedge;
            this.stops = stops;
            this.count = count;
            this.index = 0;
            this.Current = default;
        }

        /// <summary>
        /// Gets the slice the enumerator has reached.
        /// </summary>
        public CornerSlice Current { get; private set; }

        /// <summary>
        /// Moves to the next slice.
        /// </summary>
        /// <returns><c>true</c>, if there was one, otherwise <c>false</c>.</returns>
        public bool MoveNext()
        {
            if (this.index >= this.count)
            {
                return false;
            }

            var first = this.index;
            var last = first + 1;

            // The blend holds still across this slice, so every slice after it that holds at the same colour is part
            // of the same flat stretch and can be drawn with it in one go.
            var held = this.AmountAt(first);
            if (this.AmountAt(last) == held)
            {
                while (last < this.count && this.AmountAt(last + 1) == held)
                {
                    last++;
                }
            }

            var from = (double)first / this.count;
            var to = (double)last / this.count;

            // The slice is filled a little past its own end, and the next one then paints over that overrun. One
            // slice's worth is what it takes to cover an antialiased edge, however many slices were merged.
            var filled = this.wedge.GetSlice(from, Math.Min(1, to + (CornerWedge.SliceOverlap / this.count)));
            var cuts = this.wedge.GetSlice(from, to);
            this.Current = new CornerSlice(filled, cuts.StartCutCentre, cuts.EndCutCentre, from, to);
            this.index = last;
            return true;
        }

        private double AmountAt(int boundary)
        {
            return this.stops.GetBlendAmount((double)boundary / this.count);
        }
    }
}
