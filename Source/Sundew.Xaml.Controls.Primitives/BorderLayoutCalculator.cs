// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BorderLayoutCalculator.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;

/// <summary>
/// Computes the geometry of a four sided border, independently of any UI framework.
/// </summary>
public static class BorderLayoutCalculator
{
    /// <summary>
    /// The amount the background corner control points are pushed outwards, so that the background reaches under the
    /// border instead of leaving a seam along the inside of it.
    /// </summary>
    private const double BackgroundOverdraw = 0.5;

    /// <summary>
    /// Calculates the filled regions of a border and its background.
    /// </summary>
    /// <param name="metrics">The measurements to lay out from.</param>
    /// <returns>The layout.</returns>
    public static BorderLayout CalculateBorder(in BorderMetrics metrics)
    {
        var scale = metrics.Scale;
        var width = DevicePixel.Round(metrics.Width, scale.X);
        var height = DevicePixel.Round(metrics.Height, scale.Y);

        var thickness = CalculateFittedThickness(metrics.Thickness, width, height, scale);
        var right = width - thickness.Right;
        var bottom = height - thickness.Bottom;
        var bounds = CalculateEdgeBounds(width, height, thickness, metrics.CornerRadius, scale);

        var sides = new Sides<SideQuad>(
            new SideQuad(
                new Edge(new Position(0, bounds.LeftBottom), new Position(0, bounds.LeftTop)),
                new Edge(new Position(thickness.Left, bounds.LeftBottom), new Position(thickness.Left, bounds.LeftTop))),
            new SideQuad(
                new Edge(new Position(bounds.TopLeft, 0), new Position(bounds.TopRight, 0)),
                new Edge(new Position(bounds.TopLeft, thickness.Top), new Position(bounds.TopRight, thickness.Top))),
            new SideQuad(
                new Edge(new Position(width, bounds.RightTop), new Position(width, bounds.RightBottom)),
                new Edge(new Position(right, bounds.RightTop), new Position(right, bounds.RightBottom))),
            new SideQuad(
                new Edge(new Position(bounds.BottomRight, height), new Position(bounds.BottomLeft, height)),
                new Edge(new Position(bounds.BottomRight, bottom), new Position(bounds.BottomLeft, bottom))));

        // Each corner is traversed from the side entering it to the side leaving it, and each inner arc bends around
        // the point where the inner edges of those two sides would have met, so the inside of a corner is rounded
        // wherever the two sides leave room for it. A corner given no radius turns through the outer point instead
        // of bending towards it, which leaves it square, and its inner arc collapses onto the point the two inner
        // edges cross at.
        var radius = metrics.CornerRadius;
        var corners = new Corners<CornerWedge>(
            new CornerWedge(
                new CornerArc(new Position(0, bounds.LeftTop), new Position(0, 0), new Position(bounds.TopLeft, 0), IsSquare(radius.TopLeft, scale)),
                new CornerArc(
                    new Position(bounds.TopLeft, thickness.Top),
                    new Position(thickness.Left, thickness.Top),
                    new Position(thickness.Left, bounds.LeftTop))),
            new CornerWedge(
                new CornerArc(new Position(bounds.TopRight, 0), new Position(width, 0), new Position(width, bounds.RightTop), IsSquare(radius.TopRight, scale)),
                new CornerArc(
                    new Position(right, bounds.RightTop),
                    new Position(right, thickness.Top),
                    new Position(bounds.TopRight, thickness.Top))),
            new CornerWedge(
                new CornerArc(new Position(width, bounds.RightBottom), new Position(width, height), new Position(bounds.BottomRight, height), IsSquare(radius.BottomRight, scale)),
                new CornerArc(
                    new Position(bounds.BottomRight, bottom),
                    new Position(right, bottom),
                    new Position(right, bounds.RightBottom))),
            new CornerWedge(
                new CornerArc(new Position(bounds.BottomLeft, height), new Position(0, height), new Position(0, bounds.LeftBottom), IsSquare(radius.BottomLeft, scale)),
                new CornerArc(
                    new Position(thickness.Left, bounds.LeftBottom),
                    new Position(thickness.Left, bottom),
                    new Position(bounds.BottomLeft, bottom))));

        // The background is the inside of the ring, with its corner control points pushed outwards so that it reaches
        // under the ring rather than leaving a seam along the inside of it.
        var background = new RoundedFigure(
            new Sides<Edge>(
                sides.Left.Inner,
                sides.Top.Inner,
                sides.Right.Inner,
                sides.Bottom.Inner),
            new Corners<Position>(
                new Position(thickness.Left - BackgroundOverdraw, thickness.Top - BackgroundOverdraw),
                new Position(right + BackgroundOverdraw, thickness.Top - BackgroundOverdraw),
                new Position(right + BackgroundOverdraw, bottom + BackgroundOverdraw),
                new Position(thickness.Left - BackgroundOverdraw, bottom + BackgroundOverdraw)));

        return new BorderLayout(sides, corners, background);
    }

    /// <summary>
    /// Calculates the area inside the border, for clipping content to the corner radius.
    /// </summary>
    /// <param name="metrics">The measurements to lay out from.</param>
    /// <param name="padding">The padding between the border and the content, which the figure is offset by so that it
    /// is expressed in the coordinate space of the content.</param>
    /// <param name="figure">The figure, when there is room for content.</param>
    /// <returns><c>true</c>, if there is room for content, otherwise <c>false</c>.</returns>
    public static bool TryCalculateContentClip(in BorderMetrics metrics, Sides<double> padding, out RoundedFigure figure)
    {
        var scale = metrics.Scale;
        var width = DevicePixel.Round(metrics.Width, scale.X);
        var height = DevicePixel.Round(metrics.Height, scale.Y);
        var thickness = CalculateFittedThickness(metrics.Thickness, width, height, scale);

        var right = width - thickness.Right;
        var bottom = height - thickness.Bottom;
        if (right <= thickness.Left || bottom <= thickness.Top)
        {
            figure = default;
            return false;
        }

        // The clip is the inside of the ring, so that it follows exactly the boundary the border is drawn down to.
        var bounds = CalculateEdgeBounds(width, height, thickness, metrics.CornerRadius, scale);

        figure = new RoundedFigure(
            new Sides<Edge>(
                new Edge(
                    new Position(thickness.Left, bounds.LeftBottom),
                    new Position(thickness.Left, bounds.LeftTop)),
                new Edge(
                    new Position(bounds.TopLeft, thickness.Top),
                    new Position(bounds.TopRight, thickness.Top)),
                new Edge(
                    new Position(right, bounds.RightTop),
                    new Position(right, bounds.RightBottom)),
                new Edge(
                    new Position(bounds.BottomRight, bottom),
                    new Position(bounds.BottomLeft, bottom))),
            new Corners<Position>(
                new Position(thickness.Left, thickness.Top),
                new Position(right, thickness.Top),
                new Position(right, bottom),
                new Position(thickness.Left, bottom)))
            .Translate(-(thickness.Left + padding.Left), -(thickness.Top + padding.Top));

        return true;
    }

    /// <summary>
    /// Calculates the thickness of each side, aligned to device pixels along the axis of that side.
    /// </summary>
    /// <param name="thickness">The border thickness.</param>
    /// <param name="scale">The device pixel scale.</param>
    /// <returns>The thickness per side.</returns>
    public static Sides<double> CalculateEdgeThickness(Sides<double> thickness, Scale scale)
    {
        return new Sides<double>(
            DevicePixel.Round(thickness.Left, scale.X),
            DevicePixel.Round(thickness.Top, scale.Y),
            DevicePixel.Round(thickness.Right, scale.X),
            DevicePixel.Round(thickness.Bottom, scale.Y));
    }

    /// <summary>
    /// Calculates the thickness of each side, aligned to device pixels and scaled down where the two sides facing each
    /// other are together thicker than the border is wide or tall.
    /// </summary>
    /// <remarks>
    /// Without this the inner boundary would turn itself inside out, which a filled region, unlike a stroke, shows.
    /// </remarks>
    /// <param name="thickness">The border thickness.</param>
    /// <param name="width">The rendered width.</param>
    /// <param name="height">The rendered height.</param>
    /// <param name="scale">The device pixel scale.</param>
    /// <returns>The thickness per side.</returns>
    public static Sides<double> CalculateFittedThickness(Sides<double> thickness, double width, double height, Scale scale)
    {
        var rounded = CalculateEdgeThickness(thickness, scale);
        var (left, right) = Fit(rounded.Left, rounded.Right, width, scale.X);
        var (top, bottom) = Fit(rounded.Top, rounded.Bottom, height, scale.Y);
        return new Sides<double>(left, top, right, bottom);

        static (double First, double Second) Fit(double first, double second, double extent, double axisScale)
        {
            var total = first + second;
            if (total <= extent || total <= 0)
            {
                return (first, second);
            }

            var fitted = DevicePixel.Round(first * extent / total, axisScale);
            return (fitted, extent - fitted);
        }
    }

    /// <summary>
    /// Says whether a corner is square, which it is when it is given no radius at all.
    /// </summary>
    /// <param name="radius">The corner radius.</param>
    /// <param name="scale">The device pixel scale.</param>
    /// <returns><c>true</c>, if the corner is square.</returns>
    private static bool IsSquare(double radius, Scale scale)
    {
        return DevicePixel.Round(radius, scale.Minimum) <= 0;
    }

    /// <summary>
    /// Works out how far a corner reaches along each of the two sides meeting it.
    /// </summary>
    /// <param name="radius">The corner radius.</param>
    /// <param name="verticalThickness">The thickness of the horizontal side meeting the corner.</param>
    /// <param name="horizontalThickness">The thickness of the vertical side meeting the corner.</param>
    /// <param name="limit">Half the smaller of the two dimensions, which no corner may exceed.</param>
    /// <param name="scale">The device pixel scale.</param>
    /// <returns>How far the corner reaches horizontally and vertically.</returns>
    private static (double Horizontal, double Vertical) LimitRadius(
        double radius,
        double verticalThickness,
        double horizontalThickness,
        double limit,
        Scale scale)
    {
        // A square corner reaches exactly as far as each of the two bands meeting it is thick, so that its cuts
        // land on their inner edges and the inside of it is the single point where those edges cross. That is the
        // one case where the two reaches differ, and it is why they are worked out per axis at all.
        if (IsSquare(radius, scale))
        {
            return (Limit(horizontalThickness), Limit(verticalThickness));
        }

        // A rounded corner can be no tighter than the thicker of the two sides, not the thinner. Its cuts sit a
        // radius away from it along each side, so a radius under the thickness of either side puts that side's cut
        // inside the band of the other one, and the inner boundary then runs outside the inner rectangle and bulges
        // into the thicker side. The cost is that a radius between nothing and the thicker side is drawn at that
        // thickness.
        var rounded = DevicePixel.Round(radius, scale.Minimum);
        var atLeastThickness = Limit(Math.Max(rounded, Math.Max(verticalThickness, horizontalThickness)));
        return (atLeastThickness, atLeastThickness);

        double Limit(double value)
        {
            return DevicePixel.Round(Math.Min(value, limit), scale.Minimum);
        }
    }

    private static EdgeBounds CalculateEdgeBounds(double width, double height, Sides<double> thickness, Corners<double> cornerRadius, Scale scale)
    {
        // No corner may be larger than half the smaller dimension, whatever it was asked for.
        var limit = Math.Min(width, height) / 2;
        var topLeft = LimitRadius(cornerRadius.TopLeft, thickness.Top, thickness.Left, limit, scale);
        var topRight = LimitRadius(cornerRadius.TopRight, thickness.Top, thickness.Right, limit, scale);
        var bottomRight = LimitRadius(cornerRadius.BottomRight, thickness.Bottom, thickness.Right, limit, scale);
        var bottomLeft = LimitRadius(cornerRadius.BottomLeft, thickness.Bottom, thickness.Left, limit, scale);

        return CalculateEdgeBounds(
            width,
            height,
            new Corners<double>(topLeft.Horizontal, topRight.Horizontal, bottomRight.Horizontal, bottomLeft.Horizontal),
            new Corners<double>(topLeft.Vertical, topRight.Vertical, bottomRight.Vertical, bottomLeft.Vertical),
            scale);
    }

    /// <summary>
    /// Turns how far each corner reaches into the coordinates the straight section of each side runs between.
    /// </summary>
    /// <param name="width">The rendered width.</param>
    /// <param name="height">The rendered height.</param>
    /// <param name="across">How far each corner reaches along the horizontal side meeting it.</param>
    /// <param name="down">How far each corner reaches along the vertical side meeting it.</param>
    /// <param name="scale">The device pixel scale.</param>
    /// <returns>The bounds.</returns>
    private static EdgeBounds CalculateEdgeBounds(
        double width,
        double height,
        Corners<double> across,
        Corners<double> down,
        Scale scale)
    {
        var leftTop = DevicePixel.Round(down.TopLeft, scale.Y);
        var leftBottom = DevicePixel.Round(height - down.BottomLeft, scale.Y);
        var topLeft = DevicePixel.Round(across.TopLeft, scale.X);
        var topRight = DevicePixel.Round(width - across.TopRight, scale.X);
        var rightTop = DevicePixel.Round(down.TopRight, scale.Y);
        var rightBottom = DevicePixel.Round(height - down.BottomRight, scale.Y);
        var bottomLeft = DevicePixel.Round(across.BottomLeft, scale.X);
        var bottomRight = DevicePixel.Round(width - across.BottomRight, scale.X);

        // When the corners take up more room than there is, collapse the straight section between them rather than
        // letting it run backwards.
        if (leftTop > leftBottom)
        {
            leftTop = leftBottom;
        }

        if (rightTop > rightBottom)
        {
            rightTop = rightBottom;
        }

        if (topLeft > topRight)
        {
            topLeft = topRight;
        }

        if (bottomLeft > bottomRight)
        {
            bottomLeft = bottomRight;
        }

        return new EdgeBounds(leftTop, leftBottom, topLeft, topRight, rightTop, rightBottom, bottomLeft, bottomRight);
    }

    private readonly record struct EdgeBounds(
        double LeftTop,
        double LeftBottom,
        double TopLeft,
        double TopRight,
        double RightTop,
        double RightBottom,
        double BottomLeft,
        double BottomRight);
}
