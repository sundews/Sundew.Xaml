// ----------------------------------------------------------------------------------------------------------------------
// <copyright file="CornerArc.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;

/// <summary>
/// A quadratic bezier segment rounding one corner of a border, or the two straight legs of a corner that has no
/// radius to round.
/// </summary>
/// <param name="Start">The start of the arc.</param>
/// <param name="Control">The control point, which sits where the two sides would have met.</param>
/// <param name="End">The end of the arc.</param>
/// <param name="IsSharp">Whether the arc turns through <paramref name="Control"/> rather than bending towards it,
/// which is what a corner with no radius does.</param>
public readonly record struct CornerArc(Position Start, Position Control, Position End, bool IsSharp = false)
{
    /// <summary>
    /// Gets how far along a sharp arc its two legs meet, as a fraction of its length, so that walking the arc
    /// covers the one leg at the same rate as the other. An arc that curves has no turn and reports zero.
    /// </summary>
    public double Turn
    {
        get
        {
            if (!this.IsSharp)
            {
                return 0;
            }

            var first = Length(this.Start, this.Control);
            var total = first + Length(this.Control, this.End);
            return total > 0 ? first / total : 0;
        }
    }

    /// <summary>
    /// Gets the point the arc has reached at the specified position along it.
    /// </summary>
    /// <param name="position">The position, where 0 is <see cref="Start"/> and 1 is <see cref="End"/>.</param>
    /// <returns>The point.</returns>
    public Position At(double position)
    {
        return this.IsSharp ? this.AtSharp(position, this.Turn) : this.AtCurved(position);
    }

    /// <summary>
    /// Gets an arc translated by the specified amounts.
    /// </summary>
    /// <param name="x">The horizontal translation.</param>
    /// <param name="y">The vertical translation.</param>
    /// <returns>The translated arc.</returns>
    public CornerArc Translate(double x, double y)
    {
        return new CornerArc(
            this.Start.Translate(x, y),
            this.Control.Translate(x, y),
            this.End.Translate(x, y),
            this.IsSharp);
    }

    /// <summary>
    /// Gets the section of the arc between the two specified positions along it, as an arc in its own right.
    /// </summary>
    /// <param name="start">The position the section starts at.</param>
    /// <param name="end">The position the section ends at.</param>
    /// <returns>The section.</returns>
    public CornerArc Section(double start, double end)
    {
        if (this.IsSharp)
        {
            // A section of two straight legs is two straight legs again, which is what lets a square corner be cut
            // up like any other. A section that does not run over the turn keeps no turn of its own, and is the
            // one straight run along the leg it lies on.
            var turn = this.Turn;
            var sharpFrom = this.AtSharp(start, turn);
            var sharpTo = this.AtSharp(end, turn);
            return new CornerArc(sharpFrom, start < turn && turn < end ? this.Control : sharpTo, sharpTo, true);
        }

        // A quadratic section is the curve through both points whose initial direction is the curve's own there,
        // scaled by how much of the curve the section covers.
        var from = this.AtCurved(start);
        var span = end - start;
        var remaining = 1 - start;
        var control = new Position(
            from.X + (span * ((remaining * (this.Control.X - this.Start.X)) + (start * (this.End.X - this.Control.X)))),
            from.Y + (span * ((remaining * (this.Control.Y - this.Start.Y)) + (start * (this.End.Y - this.Control.Y)))));
        return new CornerArc(from, control, this.AtCurved(end));
    }

    private static double Length(Position from, Position to)
    {
        var x = to.X - from.X;
        var y = to.Y - from.Y;
        return Math.Sqrt((x * x) + (y * y));
    }

    private static Position Between(Position from, Position to, double amount)
    {
        return new Position(from.X + ((to.X - from.X) * amount), from.Y + ((to.Y - from.Y) * amount));
    }

    private Position AtCurved(double position)
    {
        var remaining = 1 - position;
        var start = remaining * remaining;
        var control = 2 * position * remaining;
        var end = position * position;
        return new Position(
            (start * this.Start.X) + (control * this.Control.X) + (end * this.End.X),
            (start * this.Start.Y) + (control * this.Control.Y) + (end * this.End.Y));
    }

    private Position AtSharp(double position, double turn)
    {
        return position <= turn
            ? Between(this.Start, this.Control, turn > 0 ? position / turn : 1)
            : Between(this.Control, this.End, turn < 1 ? (position - turn) / (1 - turn) : 0);
    }
}
