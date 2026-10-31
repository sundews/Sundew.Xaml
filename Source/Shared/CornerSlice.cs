// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CornerSlice.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

/// <summary>
/// One slice of a corner's blend, as a <see cref="CornerSweep"/> hands it out.
/// </summary>
/// <param name="Filled">The region to fill, which runs a little past <paramref name="To"/> so that the slice after it
/// paints over the antialiased edge rather than letting the background show through the seam.</param>
/// <param name="StartCutCentre">Where the slice is at the colour it starts with.</param>
/// <param name="EndCutCentre">Where the slice is at the colour it ends with.</param>
/// <param name="From">The position around the corner the slice starts at, where 0 is the side entering it.</param>
/// <param name="To">The position around the corner the slice ends at, where 1 is the side leaving it.</param>
public readonly record struct CornerSlice(
    CornerWedge Filled,
    Position StartCutCentre,
    Position EndCutCentre,
    double From,
    double To);
