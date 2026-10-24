// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrushResolver.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using Microsoft.UI.Xaml.Media;
using Windows.UI;

/// <summary>
/// Turns the XAML brushes set on a border into the fills the renderer works in.
/// </summary>
/// <remarks>
/// This is the only part of drawing a border that knows about XAML at all. Keeping it apart from
/// <see cref="BorderRenderer"/> is what lets the renderer, and so every backend, be exercised without a XAML runtime.
/// </remarks>
internal static class BrushResolver
{
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
                return CornerBlend.Between(firstBrush.Color, secondBrush.Color);
            case (SolidColorBrush firstBrush, LinearGradientBrush secondBrush):
                return CornerBlend.Between(firstBrush.Color, GetEndColor(secondBrush, gradient.ReverseSecond));
            case (LinearGradientBrush firstBrush, SolidColorBrush secondBrush):
                return CornerBlend.Between(GetStartColor(firstBrush, gradient.ReverseFirst), secondBrush.Color);
            case (LinearGradientBrush firstBrush, LinearGradientBrush secondBrush):
                return CornerBlend.Between(
                    GetStartColor(firstBrush, gradient.ReverseFirst),
                    GetEndColor(secondBrush, gradient.ReverseSecond));
            default:
                return CornerBlend.Unblended(Paint.From(first));
        }
    }

    private static Color GetStartColor(LinearGradientBrush brush, bool reverse)
    {
        if (brush.GradientStops.Count == 0)
        {
            return Microsoft.UI.Colors.Transparent;
        }

        return reverse ? brush.GradientStops[brush.GradientStops.Count - 1].Color : brush.GradientStops[0].Color;
    }

    private static Color GetEndColor(LinearGradientBrush brush, bool reverse)
    {
        if (brush.GradientStops.Count == 0)
        {
            return Microsoft.UI.Colors.Transparent;
        }

        return reverse ? brush.GradientStops[0].Color : brush.GradientStops[brush.GradientStops.Count - 1].Color;
    }
}
