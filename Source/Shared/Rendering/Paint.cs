// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Paint.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System.Collections.Generic;

/// <summary>
/// How a region of the border is filled, reduced to something every drawing backend can carry out.
/// </summary>
/// <remarks>
/// Win2D, Skia and MAUI describe brushes quite differently, and a XAML gradient brush describes itself relative to
/// the bounds of whatever it is painting. Each host resolves its own brushes into this once, which keeps the
/// backends down to the two cases they really have to implement and guarantees they agree on what a brush means.
/// </remarks>
public sealed class Paint
{
    private static readonly PaintStop[] NoStops = [];

    private Paint(PaintColor color, IReadOnlyList<PaintStop> stops, Position start, Position end, bool isRelative)
    {
        this.Color = color;
        this.Stops = stops;
        this.Start = start;
        this.End = end;
        this.IsRelative = isRelative;
    }

    /// <summary>
    /// Gets a value indicating whether the fill is a gradient rather than a single colour.
    /// </summary>
    public bool IsGradient => this.Stops.Count > 0;

    /// <summary>
    /// Gets the colour the region is filled with, when it is not a gradient.
    /// </summary>
    public PaintColor Color { get; }

    /// <summary>
    /// Gets the stops of the gradient, in order, when it is one.
    /// </summary>
    public IReadOnlyList<PaintStop> Stops { get; }

    /// <summary>
    /// Gets a value indicating whether <see cref="Start"/> and <see cref="End"/> are fractions of the bounds of
    /// whatever is being filled rather than absolute coordinates.
    /// </summary>
    /// <remarks>
    /// A XAML gradient brush is relative to the bounds of the shape it paints by default, and a border paints each
    /// side and each corner as a shape of its own. Resolving the gradient against the whole border instead would
    /// stretch it across all of them, which is not what the same brush does on a WPF border.
    /// </remarks>
    public bool IsRelative { get; }

    /// <summary>
    /// Gets the point the gradient starts at, absolute or relative according to <see cref="IsRelative"/>.
    /// </summary>
    public Position Start { get; }

    /// <summary>
    /// Gets the point the gradient ends at, absolute or relative according to <see cref="IsRelative"/>.
    /// </summary>
    public Position End { get; }

    /// <summary>
    /// Creates a paint that fills with a single colour.
    /// </summary>
    /// <param name="color">The colour.</param>
    /// <returns>The paint.</returns>
    public static Paint Solid(PaintColor color)
    {
        return new Paint(color, NoStops, default, default, false);
    }

    /// <summary>
    /// Creates a paint that blends between two colours along a line.
    /// </summary>
    /// <param name="start">The colour at <paramref name="from"/>.</param>
    /// <param name="end">The colour at <paramref name="to"/>.</param>
    /// <param name="from">The point the blend starts at.</param>
    /// <param name="to">The point the blend ends at.</param>
    /// <returns>The paint.</returns>
    public static Paint Linear(PaintColor start, PaintColor end, Position from, Position to)
    {
        if (start.Equals(end))
        {
            return Solid(start);
        }

        return new Paint(start, [new PaintStop(start, 0), new PaintStop(end, 1)], from, to, false);
    }

    /// <summary>
    /// Creates a paint that runs through the specified stops, which is what a host's brush resolver produces from a
    /// gradient brush.
    /// </summary>
    /// <param name="stops">The stops, in order, of which there must be at least two.</param>
    /// <param name="start">The point the gradient starts at.</param>
    /// <param name="end">The point the gradient ends at.</param>
    /// <param name="isRelative">Whether the two points are fractions of the bounds of what is being filled.</param>
    /// <returns>The paint.</returns>
    public static Paint Gradient(IReadOnlyList<PaintStop> stops, Position start, Position end, bool isRelative)
    {
        return new Paint(stops.Count > 0 ? stops[0].Color : default, stops, start, end, isRelative);
    }

    /// <summary>
    /// Resolves the ends of a relative gradient against the bounds of what is being filled.
    /// </summary>
    /// <param name="bounds">The bounds of the geometry being filled.</param>
    /// <param name="start">The resolved start.</param>
    /// <param name="end">The resolved end.</param>
    public void ResolveEnds(Bounds bounds, out Position start, out Position end)
    {
        if (!this.IsRelative)
        {
            start = this.Start;
            end = this.End;
            return;
        }

        start = new Position(bounds.X + (this.Start.X * bounds.Width), bounds.Y + (this.Start.Y * bounds.Height));
        end = new Position(bounds.X + (this.End.X * bounds.Width), bounds.Y + (this.End.Y * bounds.Height));
    }
}
