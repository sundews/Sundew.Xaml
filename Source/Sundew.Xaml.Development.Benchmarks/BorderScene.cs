// ----------------------------------------------------------------------------------------------------------------------
// <copyright file="BorderScene.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Benchmarks;

using Sundew.Xaml.Controls;

/// <summary>
/// One border, reduced to what <see cref="BorderRenderer"/> is given.
/// </summary>
/// <remarks>
/// Each host resolves its own brushes into <see cref="Paint"/> and <see cref="CornerBlend"/> once and keeps the
/// result, so the benchmark does the same and measures the render rather than the resolving.
/// </remarks>
public sealed class BorderScene
{
    /// <summary>
    /// What every one of the border controls defaults its corner stops to. Constructing <see cref="Stops"/> with
    /// no arguments is not the same thing: that is a hard step at the start of the corner rather than a sweep
    /// across the middle of it, and it would measure a corner no control ever draws.
    /// </summary>
    private static readonly Stops DefaultStops = new Stops(0.2, 0.8);

    private static readonly PaintColor Purple = PaintColor.FromArgb(255, 147, 112, 219);
    private static readonly PaintColor Orange = PaintColor.FromArgb(255, 255, 140, 0);
    private static readonly PaintColor Blue = PaintColor.FromArgb(255, 70, 130, 180);
    private static readonly PaintColor Green = PaintColor.FromArgb(255, 46, 139, 87);

    private readonly BorderMetrics metrics;
    private readonly Paint background;
    private readonly Sides<Paint?> sides;
    private readonly Corners<CornerBlend> blends;
    private readonly Corners<Stops> stops;

    private BorderScene(
        BorderMetrics metrics,
        Paint background,
        Sides<Paint?> sides,
        Corners<CornerBlend> blends,
        Corners<Stops> stops)
    {
        this.metrics = metrics;
        this.background = background;
        this.sides = sides;
        this.blends = blends;
        this.stops = stops;
    }

    /// <summary>
    /// Creates the measurements of the benchmarked border.
    /// </summary>
    /// <param name="radius">The corner radius.</param>
    /// <returns>The measurements.</returns>
    public static BorderMetrics CreateMetrics(double radius)
    {
        return new BorderMetrics(200, 140, new Sides<double>(6), new Corners<double>(radius), Scale.Uniform(1));
    }

    /// <summary>
    /// Creates a scene.
    /// </summary>
    /// <param name="sidePaints">What the four sides are painted with.</param>
    /// <param name="radius">The corner radius.</param>
    /// <returns>The scene.</returns>
    public static BorderScene Create(SidePaints sidePaints, double radius)
    {
        var alpha = sidePaints == SidePaints.SeeThrough ? (byte)160 : byte.MaxValue;
        var left = Fade(Purple, alpha);
        var top = sidePaints == SidePaints.OneColour ? left : Fade(Orange, alpha);
        var right = sidePaints == SidePaints.OneColour ? left : Fade(Blue, alpha);
        var bottom = sidePaints == SidePaints.OneColour ? left : Fade(Green, alpha);

        var isGradient = sidePaints == SidePaints.Gradient;
        var sides = new Sides<Paint?>(
            isGradient ? Ramp(left, bottom) : Paint.Solid(left),
            Paint.Solid(top),
            isGradient ? Ramp(right, top) : Paint.Solid(right),
            Paint.Solid(bottom));

        // With a gradient down the left and the right there is no single colour for any corner to sweep from, so
        // all four of them take the unblended path instead.
        var blends = isGradient
            ? new Corners<CornerBlend>(CornerBlend.Unblended(Paint.Solid(top)))
            : new Corners<CornerBlend>(
                CornerBlend.Between(left, top),
                CornerBlend.Between(top, right),
                CornerBlend.Between(right, bottom),
                CornerBlend.Between(bottom, left));

        return new BorderScene(
            CreateMetrics(radius),
            Paint.Solid(PaintColor.FromArgb(255, 245, 245, 245)),
            sides,
            blends,
            new Corners<Stops>(DefaultStops));
    }

    /// <summary>
    /// Draws the border onto the specified session.
    /// </summary>
    /// <param name="session">The session.</param>
    public void Render(IBorderDrawingSession session)
    {
        BorderRenderer.Render(session, this.metrics, this.background, this.sides, this.blends, this.stops);
    }

    private static PaintColor Fade(PaintColor color, byte alpha)
    {
        return PaintColor.FromArgb(alpha, color.R, color.G, color.B);
    }

    private static Paint Ramp(PaintColor start, PaintColor end)
    {
        return Paint.Gradient(
            new[] { new PaintStop(start, 0), new PaintStop(end, 1) },
            new Position(0, 0),
            new Position(0, 1),
            true);
    }
}
