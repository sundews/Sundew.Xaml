// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PaintColor.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

/// <summary>
/// A colour, in a type of our own rather than any one UI framework's.
/// </summary>
/// <remarks>
/// The renderer and the drawing backends are shared between frameworks that each have their own colour type, and
/// carrying colours in one of ours is what keeps them from depending on any of those. Each host converts once, at
/// its edge, where it turns its own brushes into <see cref="Paint"/>.
/// </remarks>
/// <param name="A">The alpha channel, where 0 is fully transparent.</param>
/// <param name="R">The red channel.</param>
/// <param name="G">The green channel.</param>
/// <param name="B">The blue channel.</param>
public readonly record struct PaintColor(byte A, byte R, byte G, byte B)
{
    /// <summary>
    /// Gets a colour that paints nothing.
    /// </summary>
    public static PaintColor Transparent => default;

    /// <summary>
    /// Creates a colour from its channels.
    /// </summary>
    /// <param name="a">The alpha channel.</param>
    /// <param name="r">The red channel.</param>
    /// <param name="g">The green channel.</param>
    /// <param name="b">The blue channel.</param>
    /// <returns>The colour.</returns>
    public static PaintColor FromArgb(byte a, byte r, byte g, byte b)
    {
        return new PaintColor(a, r, g, b);
    }
}
