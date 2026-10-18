// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BorderLayout.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

/// <summary>
/// The geometry of a four sided border, with each side and corner kept separate so that each can be filled with its
/// own brush.
/// </summary>
/// <remarks>
/// The border is a ring between an outer rounded rectangle and an inner one, cut into eight closed regions that tile
/// it without gaps or overlaps: a quad for the straight section of each side and a wedge for each corner. Because the
/// regions are filled rather than stroked, each side is as thick as that side alone, so a non uniform
/// thickness draws just as faithfully as a uniform one.
/// The background sits inside the ring and is a closed figure.
/// </remarks>
/// <param name="Sides">The filled region of each side's straight section.</param>
/// <param name="Corners">The filled region of each corner.</param>
/// <param name="Background">The closed figure covering the area inside the ring.</param>
/// <param name="Guidelines">The horizontal and vertical pixel snapping guidelines the geometry falls on.</param>
public readonly record struct BorderLayout(
    Sides<SideQuad> Sides,
    Corners<CornerWedge> Corners,
    RoundedFigure Background,
    Guidelines Guidelines);
