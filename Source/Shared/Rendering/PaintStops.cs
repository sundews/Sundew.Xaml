// ----------------------------------------------------------------------------------------------------------------------
// <copyright file="PaintStops.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;
using System.Collections.Generic;

/// <summary>
/// The stops of a gradient, read where they are rather than copied somewhere to be read.
/// </summary>
/// <remarks>
/// Nearly every gradient a border draws is the blend between two colours across one slice of one corner, and a
/// frame is hundreds of those. Keeping that pair in the <see cref="Paint"/> itself and handing it out through this
/// is what stops each one allocating a two element array that is read once and dropped. A gradient brush resolved
/// by a host may have any number of stops and does keep them in a list, which this reads the same way, so a
/// backend has one way of reading stops and need not know which kind it has.
/// </remarks>
public readonly struct PaintStops
{
    private readonly IReadOnlyList<PaintStop>? stops;
    private readonly PaintColor start;
    private readonly PaintColor end;
    private readonly int count;

    internal PaintStops(IReadOnlyList<PaintStop>? stops, PaintColor start, PaintColor end, bool isPair)
    {
        this.stops = stops;
        this.start = start;
        this.end = end;
        this.count = stops?.Count ?? (isPair ? 2 : 0);
    }

    /// <summary>
    /// Gets how many stops the gradient has, which is zero when the paint is a single colour.
    /// </summary>
    public int Count => this.count;

    /// <summary>
    /// Gets the stop at the specified index.
    /// </summary>
    /// <param name="index">The index, from zero to one less than <see cref="Count"/>.</param>
    /// <returns>The stop.</returns>
    public PaintStop this[int index]
    {
        get
        {
            if (this.stops != null)
            {
                return this.stops[index];
            }

            return index switch
            {
                0 => new PaintStop(this.start, 0),
                1 => new PaintStop(this.end, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }
    }
}
