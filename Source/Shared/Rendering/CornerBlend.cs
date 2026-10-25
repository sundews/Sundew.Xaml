// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CornerBlend.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;

/// <summary>
/// The two colours a corner is blended between, or the fill to fall back to when the two sides meeting there cannot
/// be blended.
/// </summary>
public readonly record struct CornerBlend
{
    private readonly PaintColor start;
    private readonly PaintColor end;

    private CornerBlend(PaintColor start, PaintColor end)
    {
        this.start = start;
        this.end = end;
        this.Fallback = null;
        this.IsBlended = true;
    }

    private CornerBlend(Paint? fallback)
    {
        this.start = default;
        this.end = default;
        this.Fallback = fallback;
        this.IsBlended = false;
    }

    /// <summary>
    /// Gets the fill used for the whole corner when the two sides cannot be blended.
    /// </summary>
    public Paint? Fallback { get; }

    /// <summary>
    /// Gets a value indicating whether the corner sweeps between two colours.
    /// </summary>
    public bool IsBlended { get; }

    /// <summary>
    /// Gets a value indicating whether the corner is drawn with no transparency, in which case a side may run
    /// underneath it without the overlap showing.
    /// </summary>
    public bool IsOpaque => this.IsBlended && this.start.A == byte.MaxValue && this.end.A == byte.MaxValue;

    /// <summary>
    /// Gets the alpha the corner as a whole is drawn at, which is that of whichever of the two colours is the more
    /// opaque, so that <see cref="ColorAt"/> never has to raise one of them.
    /// </summary>
    public double Opacity => Math.Max(this.start.A, this.end.A) / 255.0;

    /// <summary>
    /// Creates a blend that sweeps from the colour of the side entering the corner to that of the side leaving it.
    /// </summary>
    /// <param name="start">The colour of the side entering the corner.</param>
    /// <param name="end">The colour of the side leaving the corner.</param>
    /// <returns>The blend.</returns>
    public static CornerBlend Between(PaintColor start, PaintColor end)
    {
        return new CornerBlend(start, end);
    }

    /// <summary>
    /// Creates a blend for a corner whose two sides cannot be blended, which is filled in one go instead.
    /// </summary>
    /// <param name="fallback">The fill, or <c>null</c> to leave the corner undrawn.</param>
    /// <returns>The blend.</returns>
    public static CornerBlend Unblended(Paint? fallback)
    {
        return new CornerBlend(fallback);
    }

    /// <summary>
    /// Gets the colour the blend has reached at the specified position around the corner.
    /// </summary>
    /// <param name="stops">The stops controlling how abrupt the transition is.</param>
    /// <param name="position">The position, where 0 is the side entering the corner and 1 the side leaving it.</param>
    /// <returns>The colour, with its alpha taken relative to the opacity the corner is composited at.</returns>
    public PaintColor ColorAt(Stops stops, double position)
    {
        if (stops == null)
        {
            throw new ArgumentNullException(nameof(stops));
        }

        var amount = stops.GetBlendAmount(position);
        var peak = Math.Max(this.start.A, this.end.A);
        return PaintColor.FromArgb(
            peak == 0 ? (byte)0 : (byte)Math.Round(Channel(this.start.A, this.end.A, amount) * 255.0 / peak),
            Channel(this.start.R, this.end.R, amount),
            Channel(this.start.G, this.end.G, amount),
            Channel(this.start.B, this.end.B, amount));
    }

    private static byte Channel(byte start, byte end, double amount)
    {
        return (byte)Math.Round(start + ((end - start) * amount));
    }
}
