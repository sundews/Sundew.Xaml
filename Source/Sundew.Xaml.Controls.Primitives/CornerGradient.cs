// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CornerGradient.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

/// <summary>
/// Describes how the two sides meeting at a corner are blended across that corner.
/// </summary>
/// <remarks>
/// The blend is swept around the corner, from the side entering it at position 0 to the side leaving it at position 1,
/// so the direction of the sweep is the corner's own and needs no describing. What does differ per corner is which
/// end of an already graduated side brush is the one facing the corner, which is what the reverse flags say.
/// </remarks>
/// <param name="ReverseFirst">A value indicating whether the first side's gradient runs away from the corner.</param>
/// <param name="ReverseSecond">A value indicating whether the second side's gradient runs away from the corner.</param>
public readonly record struct CornerGradient(bool ReverseFirst, bool ReverseSecond)
{
    /// <summary>
    /// Gets the gradient description for the specified corner.
    /// </summary>
    /// <param name="corner">The corner.</param>
    /// <returns>The gradient description.</returns>
    public static CornerGradient For(Corner corner)
    {
        switch (corner)
        {
            case Corner.TopLeft:
                return new CornerGradient(false, true);
            case Corner.TopRight:
                return new CornerGradient(true, true);
            case Corner.BottomRight:
                return new CornerGradient(true, false);
            default:
                return new CornerGradient(false, false);
        }
    }
}
