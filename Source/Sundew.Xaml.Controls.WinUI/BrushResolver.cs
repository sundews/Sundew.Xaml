// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrushResolver.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

/// <summary>
/// Turns the XAML brushes set on a border into the fills the renderer works in.
/// </summary>
/// <remarks>
/// This is the only part of drawing a border that knows about XAML at all. Keeping it apart from
/// <see cref="BorderRenderer"/> is what lets the renderer, and so every backend, be exercised without a XAML
/// runtime, and what lets the renderer be shared with hosts whose brushes are not these.
/// </remarks>
internal static class BrushResolver
{
    /// <summary>
    /// Resolves a XAML brush into the fill the renderer works in.
    /// </summary>
    /// <param name="brush">The brush.</param>
    /// <returns>The paint, or <c>null</c> when the brush paints nothing.</returns>
    public static Paint? ResolvePaint(Brush? brush)
    {
        switch (brush)
        {
            case SolidColorBrush solidColorBrush:
                return Paint.Solid(WithOpacity(solidColorBrush.Color, solidColorBrush.Opacity));
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
            case (SolidColorBrush firstBrush, SolidColorBrush secondBrush):
                return CornerBlend.Between(ToPaintColor(firstBrush.Color), ToPaintColor(secondBrush.Color));
            case (SolidColorBrush firstBrush, LinearGradientBrush secondBrush):
                return CornerBlend.Between(ToPaintColor(firstBrush.Color), GetEndColor(secondBrush, gradient.ReverseSecond));
            case (LinearGradientBrush firstBrush, SolidColorBrush secondBrush):
                return CornerBlend.Between(GetStartColor(firstBrush, gradient.ReverseFirst), ToPaintColor(secondBrush.Color));
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
            stops.Add(new PaintStop(WithOpacity(gradientStop.Color, brush.Opacity), gradientStop.Offset));
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
        return Paint.Gradient(
            stops,
            new Position(brush.StartPoint.X, brush.StartPoint.Y),
            new Position(brush.EndPoint.X, brush.EndPoint.Y),
            brush.MappingMode == BrushMappingMode.RelativeToBoundingBox);
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

    private static PaintColor WithOpacity(Color color, double opacity)
    {
        if (opacity >= 1)
        {
            return ToPaintColor(color);
        }

        var alpha = Math.Max(0, Math.Min(255, (int)Math.Round(color.A * opacity)));
        return PaintColor.FromArgb((byte)alpha, color.R, color.G, color.B);
    }

    private static PaintColor ToPaintColor(Color color)
    {
        return PaintColor.FromArgb(color.A, color.R, color.G, color.B);
    }
}
