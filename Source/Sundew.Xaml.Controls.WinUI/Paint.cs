// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Paint.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI;

/// <summary>
/// How a region of the border is filled, reduced to something both drawing backends can carry out.
/// </summary>
/// <remarks>
/// Win2D and Skia describe brushes quite differently, and a XAML <see cref="Brush"/> describes gradients relative to
/// the bounds of whatever it is painting. Resolving all of that once, here, keeps the two backends down to the two
/// cases they really have to implement and guarantees they agree on what a brush means.
/// </remarks>
public sealed class Paint
{
    private static readonly PaintStop[] NoStops = [];

    private Paint(Color color, IReadOnlyList<PaintStop> stops, Position start, Position end, bool isRelative)
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
    public Color Color { get; }

    /// <summary>
    /// Gets the stops of the gradient, in absolute coordinates, when it is one.
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
    public static Paint Solid(Color color)
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
    public static Paint Linear(Color start, Color end, Position from, Position to)
    {
        if (start.Equals(end))
        {
            return Solid(start);
        }

        return new Paint(start, [new PaintStop(start, 0), new PaintStop(end, 1)], from, to, false);
    }

    /// <summary>
    /// Resolves a XAML brush against the area it is painting.
    /// </summary>
    /// <param name="brush">The brush.</param>
    /// <returns>The paint, or <c>null</c> when the brush paints nothing.</returns>
    public static Paint? From(Brush? brush)
    {
        switch (brush)
        {
            case SolidColorBrush solidColorBrush:
                return Solid(WithOpacity(solidColorBrush.Color, solidColorBrush.Opacity));
            case LinearGradientBrush linearGradientBrush:
                return FromLinearGradient(linearGradientBrush);
            default:
                return null;
        }
    }

    /// <summary>
    /// Resolves the ends of a relative gradient against the bounds of what is being filled.
    /// </summary>
    /// <param name="bounds">The bounds of the geometry being filled.</param>
    /// <param name="start">The resolved start.</param>
    /// <param name="end">The resolved end.</param>
    public void ResolveEnds(Rect bounds, out Position start, out Position end)
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

    private static Paint? FromLinearGradient(LinearGradientBrush brush)
    {
        var stops = new List<PaintStop>(brush.GradientStops.Count);
        foreach (var gradientStop in brush.GradientStops)
        {
            stops.Add(new PaintStop(WithOpacity(gradientStop.Color, brush.Opacity), gradientStop.Offset));
        }

        if (stops.Count == 0)
        {
            return null;
        }

        if (stops.Count == 1)
        {
            return Solid(stops[0].Color);
        }

        stops.Sort(static (first, second) => first.Offset.CompareTo(second.Offset));
        return new Paint(
            stops[0].Color,
            stops,
            new Position(brush.StartPoint.X, brush.StartPoint.Y),
            new Position(brush.EndPoint.X, brush.EndPoint.Y),
            brush.MappingMode == BrushMappingMode.RelativeToBoundingBox);
    }

    private static Color WithOpacity(Color color, double opacity)
    {
        if (opacity >= 1)
        {
            return color;
        }

        var alpha = Math.Max(0, Math.Min(255, (int)Math.Round(color.A * opacity)));
        return Color.FromArgb((byte)alpha, color.R, color.G, color.B);
    }
}
