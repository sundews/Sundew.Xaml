// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBorderDrawingSession.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;

/// <summary>
/// One pass of drawing a border, as much of a drawing API as the border needs.
/// </summary>
/// <remarks>
/// The border is built entirely from filled closed regions, so filling is all a backend has to provide. Keeping the
/// surface down to these few calls is what lets Win2D and Skia share one <see cref="Border"/>.
/// </remarks>
public interface IBorderDrawingSession : IDisposable
{
    /// <summary>
    /// Fills the straight section of one side.
    /// </summary>
    /// <param name="sideQuad">The region.</param>
    /// <param name="paint">How to fill it.</param>
    void Fill(SideQuad sideQuad, Paint paint);

    /// <summary>
    /// Fills one corner, or one slice of one.
    /// </summary>
    /// <param name="cornerWedge">The region.</param>
    /// <param name="paint">How to fill it.</param>
    void Fill(CornerWedge cornerWedge, Paint paint);

    /// <summary>
    /// Fills the area inside the border.
    /// </summary>
    /// <param name="figure">The region.</param>
    /// <param name="paint">How to fill it.</param>
    void Fill(RoundedFigure figure, Paint paint);

    /// <summary>
    /// Starts compositing everything drawn until the result is disposed as one layer at the given opacity.
    /// </summary>
    /// <remarks>
    /// A see through corner is swept as overlapping slices. Fading each slice on its own would show the overlaps as a
    /// band at every seam, so the slices are drawn opaque into a layer and the layer as a whole is faded.
    /// </remarks>
    /// <param name="opacity">The opacity, from 0 to 1.</param>
    /// <returns>A handle that ends the layer when disposed.</returns>
    IDisposable CreateOpacityLayer(double opacity);
}
