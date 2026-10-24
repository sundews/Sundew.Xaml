// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BorderRenderer.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;
using DeviceScale = Sundew.Xaml.Controls.Scale;

/// <summary>
/// Draws a border onto an <see cref="IBorderDrawingSession"/>.
/// </summary>
/// <remarks>
/// This is the whole of how a border is painted, and it is deliberately free of both XAML and of any one graphics
/// library: everything it is given has already been reduced to geometry and <see cref="Paint"/>. That is what lets
/// Win2D on Windows and Skia everywhere else produce the same border, and what makes a third backend a matter of
/// implementing <see cref="IBorderDrawingSession"/> and nothing else.
/// </remarks>
public static class BorderRenderer
{
    /// <summary>
    /// Draws a border.
    /// </summary>
    /// <param name="session">The session to draw on.</param>
    /// <param name="metrics">The measurements to lay the border out from.</param>
    /// <param name="background">How to fill the area inside the border, or <c>null</c> to leave it alone.</param>
    /// <param name="sides">How to fill each side, where <c>null</c> leaves that side undrawn.</param>
    /// <param name="blends">How the two sides meeting at each corner are blended across it.</param>
    /// <param name="stops">How abrupt the transition is at each corner.</param>
    public static void Render(
        IBorderDrawingSession session,
        in BorderMetrics metrics,
        Paint? background,
        Sides<Paint?> sides,
        Corners<CornerBlend> blends,
        Corners<Stops> stops)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        if (metrics.Width <= 0 || metrics.Height <= 0)
        {
            return;
        }

        var layout = BorderLayoutCalculator.CalculateBorder(metrics);

        if (background != null)
        {
            session.Fill(layout.Background, background);
        }

        // A see through corner is composited as a whole, so a side running under it would show through as a darker
        // band along the overlap. Those corners keep the plain shared cut instead.
        var quads = AreCornersOpaque(blends)
            ? new Sides<SideQuad>(
                layout.Sides.Left.ExtendIntoCorners(metrics.Scale),
                layout.Sides.Top.ExtendIntoCorners(metrics.Scale),
                layout.Sides.Right.ExtendIntoCorners(metrics.Scale),
                layout.Sides.Bottom.ExtendIntoCorners(metrics.Scale))
            : layout.Sides;

        FillSide(session, quads.Left, sides.Left);
        FillSide(session, quads.Top, sides.Top);
        FillSide(session, quads.Right, sides.Right);
        FillSide(session, quads.Bottom, sides.Bottom);

        FillCorner(session, layout.Corners.TopLeft, blends.TopLeft, stops.TopLeft, metrics.Scale);
        FillCorner(session, layout.Corners.TopRight, blends.TopRight, stops.TopRight, metrics.Scale);
        FillCorner(session, layout.Corners.BottomRight, blends.BottomRight, stops.BottomRight, metrics.Scale);
        FillCorner(session, layout.Corners.BottomLeft, blends.BottomLeft, stops.BottomLeft, metrics.Scale);
    }

    private static bool AreCornersOpaque(Corners<CornerBlend> blends)
    {
        return blends.TopLeft.IsOpaque && blends.TopRight.IsOpaque && blends.BottomRight.IsOpaque && blends.BottomLeft.IsOpaque;
    }

    private static void FillSide(IBorderDrawingSession session, SideQuad sideQuad, Paint? paint)
    {
        if (paint == null || !sideQuad.HasArea)
        {
            return;
        }

        session.Fill(sideQuad, paint);
    }

    /// <summary>
    /// Sweeps the blend between the two sides meeting at a corner around that corner, a slice at a time.
    /// </summary>
    /// <remarks>
    /// One linear gradient across the whole corner cannot do this without a seam: its bands of constant colour would
    /// have to run along the cut it shares with each of the two sides, and those two cuts are at right angles. Each
    /// slice is narrow enough that one gradient across it is indistinguishable from a sweep, and the slices at either
    /// end come out as the colour of the side along the whole of the cut.
    /// </remarks>
    /// <param name="session">The session to draw on.</param>
    /// <param name="cornerWedge">The corner.</param>
    /// <param name="blend">The blend between the two sides meeting at the corner.</param>
    /// <param name="stops">The stops controlling how abrupt the transition is.</param>
    /// <param name="scale">The device pixel scale, which decides how fine the sweep has to be.</param>
    private static void FillCorner(IBorderDrawingSession session, CornerWedge cornerWedge, CornerBlend blend, Stops stops, DeviceScale scale)
    {
        if (!blend.IsBlended)
        {
            if (blend.Fallback != null)
            {
                session.Fill(cornerWedge, blend.Fallback);
            }

            return;
        }

        // Where the slices overlap they would blend with each other, which a see through border would show as a band
        // at every seam. Taking the alpha off the slices and giving it to a layer of their own composites the whole
        // corner once, at which point the overlaps are opaque and leave nothing behind.
        var layer = blend.Opacity < 1 ? session.CreateOpacityLayer(blend.Opacity) : null;
        try
        {
            var count = cornerWedge.GetSliceCount(scale);
            for (var index = 0; index < count; index++)
            {
                var from = (double)index / count;
                var to = (double)(index + 1) / count;
                var slice = cornerWedge.GetSlice(from, to);

                // The slice is filled a little past its own end, and the next one then paints over that overrun.
                var filled = cornerWedge.GetSlice(from, Math.Min(1, to + (CornerWedge.SliceOverlap / count)));
                session.Fill(
                    filled,
                    Paint.Linear(
                        blend.ColorAt(stops, from),
                        blend.ColorAt(stops, to),
                        slice.StartCutCentre,
                        slice.EndCutCentre));
            }
        }
        finally
        {
            layer?.Dispose();
        }
    }
}
