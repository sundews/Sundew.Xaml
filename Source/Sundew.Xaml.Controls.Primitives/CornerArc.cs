// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CornerArc.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

/// <summary>
/// A quadratic bezier segment rounding one corner of a border.
/// </summary>
/// <param name="Start">The start of the arc.</param>
/// <param name="Control">The control point, which sits where the two sides would have met.</param>
/// <param name="End">The end of the arc.</param>
public readonly record struct CornerArc(Position Start, Position Control, Position End)
{
    /// <summary>
    /// Gets the point the arc has reached at the specified position along it.
    /// </summary>
    /// <param name="position">The position, where 0 is <see cref="Start"/> and 1 is <see cref="End"/>.</param>
    /// <returns>The point.</returns>
    public Position At(double position)
    {
        var remaining = 1 - position;
        var start = remaining * remaining;
        var control = 2 * position * remaining;
        var end = position * position;
        return new Position(
            (start * this.Start.X) + (control * this.Control.X) + (end * this.End.X),
            (start * this.Start.Y) + (control * this.Control.Y) + (end * this.End.Y));
    }

    /// <summary>
    /// Gets the section of the arc between the two specified positions along it, as an arc in its own right.
    /// </summary>
    /// <param name="start">The position the section starts at.</param>
    /// <param name="end">The position the section ends at.</param>
    /// <returns>The section.</returns>
    public CornerArc Section(double start, double end)
    {
        // A quadratic section is the curve through both points whose initial direction is the curve's own there,
        // scaled by how much of the curve the section covers.
        var from = this.At(start);
        var span = end - start;
        var remaining = 1 - start;
        var control = new Position(
            from.X + (span * ((remaining * (this.Control.X - this.Start.X)) + (start * (this.End.X - this.Control.X)))),
            from.Y + (span * ((remaining * (this.Control.Y - this.Start.Y)) + (start * (this.End.Y - this.Control.Y)))));
        return new CornerArc(from, control, this.At(end));
    }
}
