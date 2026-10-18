// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoundedFigure.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

/// <summary>
/// A closed rounded rectangle, described as four straight edges joined by four quadratic corner arcs.
/// </summary>
/// <remarks>
/// The figure is traversed clockwise starting at <see cref="Edges"/>.Left.Start, which is the lower end of the left
/// edge. Each edge is followed by the arc that joins it to the next edge, so the whole figure is
/// left edge, top left arc, top edge, top right arc, right edge, bottom right arc, bottom edge, bottom left arc.
/// </remarks>
/// <param name="Edges">The four straight edges.</param>
/// <param name="Controls">The control point of the arc at each corner.</param>
public readonly record struct RoundedFigure(Sides<Edge> Edges, Corners<Position> Controls)
{
    /// <summary>
    /// Gets the position the figure starts at.
    /// </summary>
    public Position Start => this.Edges.Left.Start;

    /// <summary>
    /// Gets the arc joining the left edge to the top edge.
    /// </summary>
    public CornerArc TopLeft => new CornerArc(this.Edges.Left.End, this.Controls.TopLeft, this.Edges.Top.Start);

    /// <summary>
    /// Gets the arc joining the top edge to the right edge.
    /// </summary>
    public CornerArc TopRight => new CornerArc(this.Edges.Top.End, this.Controls.TopRight, this.Edges.Right.Start);

    /// <summary>
    /// Gets the arc joining the right edge to the bottom edge.
    /// </summary>
    public CornerArc BottomRight => new CornerArc(this.Edges.Right.End, this.Controls.BottomRight, this.Edges.Bottom.Start);

    /// <summary>
    /// Gets the arc joining the bottom edge back to the left edge, closing the figure.
    /// </summary>
    public CornerArc BottomLeft => new CornerArc(this.Edges.Bottom.End, this.Controls.BottomLeft, this.Edges.Left.Start);

    /// <summary>
    /// Gets a figure translated by the specified amounts.
    /// </summary>
    /// <param name="x">The horizontal translation.</param>
    /// <param name="y">The vertical translation.</param>
    /// <returns>The translated figure.</returns>
    public RoundedFigure Translate(double x, double y)
    {
        return new RoundedFigure(
            new Sides<Edge>(
                this.Edges.Left.Translate(x, y),
                this.Edges.Top.Translate(x, y),
                this.Edges.Right.Translate(x, y),
                this.Edges.Bottom.Translate(x, y)),
            new Corners<Position>(
                this.Controls.TopLeft.Translate(x, y),
                this.Controls.TopRight.Translate(x, y),
                this.Controls.BottomRight.Translate(x, y),
                this.Controls.BottomLeft.Translate(x, y)));
    }
}
