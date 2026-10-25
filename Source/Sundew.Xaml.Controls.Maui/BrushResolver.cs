// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrushResolver.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

/// <summary>
/// Turns the MAUI brushes set on a border into the fills the renderer works in.
/// </summary>
/// <remarks>
/// This is the only part of drawing a border that knows about MAUI at all, and the only part of the drawing code
/// that is not shared with the WinUI and Uno library. Everything behind it, from <see cref="Paint"/> down to the
/// Skia session, is the very same source.
/// </remarks>
internal static class BrushResolver
{
    /// <summary>
    /// Resolves a MAUI brush into the fill the renderer works in.
    /// </summary>
    /// <param name="brush">The brush.</param>
    /// <returns>The paint, or <c>null</c> when the brush paints nothing.</returns>
    public static Paint? ResolvePaint(Brush? brush)
    {
        switch (brush)
        {
            case SolidColorBrush { Color: { } color }:
                return Paint.Solid(ToPaintColor(color));
            case LinearGradientBrush linearGradientBrush:
                return ResolveLinearGradient(linearGradientBrush);
            default:
                return null;
        }
    }

    /// <summary>
    /// Works out the two colours the side entering a corner is blended from and the side leaving it is blended to.
    /// </summary>
    /// <param name="first">The brush of the side entering the corner.</param>
    /// <param name="second">The brush of the side leaving the corner.</param>
    /// <param name="corner">The corner.</param>
    /// <returns>The blend, or the first brush on its own when the two cannot be blended.</returns>
    public static CornerBlend ResolveBlend(Brush? first, Brush? second, Corner corner)
    {
        var gradient = CornerGradient.For(corner);
        switch (first, second)
        {
            case (SolidColorBrush { Color: { } firstColor }, SolidColorBrush { Color: { } secondColor }):
                return CornerBlend.Between(ToPaintColor(firstColor), ToPaintColor(secondColor));
            case (SolidColorBrush { Color: { } firstColor }, LinearGradientBrush secondBrush):
                return CornerBlend.Between(ToPaintColor(firstColor), GetEndColor(secondBrush, gradient.ReverseSecond));
            case (LinearGradientBrush firstBrush, SolidColorBrush { Color: { } secondColor }):
                return CornerBlend.Between(GetStartColor(firstBrush, gradient.ReverseFirst), ToPaintColor(secondColor));
            case (LinearGradientBrush firstBrush, LinearGradientBrush secondBrush):
                return CornerBlend.Between(
                    GetStartColor(firstBrush, gradient.ReverseFirst),
                    GetEndColor(secondBrush, gradient.ReverseSecond));
            default:
                return CornerBlend.Unblended(ResolvePaint(first));
        }
    }

    private static Paint? ResolveLinearGradient(LinearGradientBrush brush)
    {
        var stops = new List<PaintStop>(brush.GradientStops.Count);
        foreach (var gradientStop in brush.GradientStops)
        {
            stops.Add(new PaintStop(ToPaintColor(gradientStop.Color), gradientStop.Offset));
        }

        if (stops.Count == 0)
        {
            return null;
        }

        if (stops.Count == 1)
        {
            return Paint.Solid(stops[0].Color);
        }

        stops.Sort(static (first, second) => first.Offset.CompareTo(second.Offset));

        // A MAUI gradient brush states its ends as fractions of what it paints and has no absolute mode, so these
        // are always relative.
        return Paint.Gradient(
            stops,
            new Position(brush.StartPoint.X, brush.StartPoint.Y),
            new Position(brush.EndPoint.X, brush.EndPoint.Y),
            true);
    }

    private static PaintColor GetStartColor(LinearGradientBrush brush, bool reverse)
    {
        if (brush.GradientStops.Count == 0)
        {
            return PaintColor.Transparent;
        }

        return ToPaintColor(reverse ? brush.GradientStops[brush.GradientStops.Count - 1].Color : brush.GradientStops[0].Color);
    }

    private static PaintColor GetEndColor(LinearGradientBrush brush, bool reverse)
    {
        if (brush.GradientStops.Count == 0)
        {
            return PaintColor.Transparent;
        }

        return ToPaintColor(reverse ? brush.GradientStops[0].Color : brush.GradientStops[brush.GradientStops.Count - 1].Color);
    }

    /// <summary>
    /// Converts a MAUI colour, whose channels run from 0 to 1, into the byte channels the renderer works in.
    /// </summary>
    /// <param name="color">The colour, which MAUI leaves null where none was set.</param>
    /// <returns>The colour.</returns>
    private static PaintColor ToPaintColor(Color? color)
    {
        if (color == null)
        {
            return PaintColor.Transparent;
        }

        return PaintColor.FromArgb(ToChannel(color.Alpha), ToChannel(color.Red), ToChannel(color.Green), ToChannel(color.Blue));
    }

    private static byte ToChannel(float value)
    {
        return (byte)Math.Max(0, Math.Min(255, (int)Math.Round(value * 255f)));
    }
}
