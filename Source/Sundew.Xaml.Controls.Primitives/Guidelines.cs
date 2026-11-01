// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Guidelines.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;
using System.Collections.Generic;

/// <summary>
/// The coordinates a border's geometry falls on, for renderers that support pixel snapping guidelines.
/// </summary>
/// <param name="X">The distinct vertical guideline coordinates.</param>
/// <param name="Y">The distinct horizontal guideline coordinates.</param>
public readonly record struct Guidelines(IReadOnlyList<double> X, IReadOnlyList<double> Y)
{
    private const int Precision = 1;

    /// <summary>
    /// Takes the distinct coordinates of each axis of a laid out border, in the order they first appear.
    /// </summary>
    /// <remarks>
    /// Both boundaries of the ring, and every cut between one of its regions and the next, have to land on whole
    /// device pixels, otherwise the shared edges blend against each other and show as seams. Every one of those
    /// coordinates is already in the layout, so they are read back off it rather than worked out a second time.
    /// </remarks>
    /// <param name="layout">The laid out border.</param>
    /// <returns>The guidelines.</returns>
    internal static Guidelines From(in BorderLayout layout)
    {
        var sides = layout.Sides;
        return new Guidelines(
            Distinct(
            [
                sides.Left.Outer.Start.X,
                sides.Left.Inner.Start.X,
                sides.Right.Inner.Start.X,
                sides.Right.Outer.Start.X,
                sides.Top.Outer.Start.X,
                sides.Top.Outer.End.X,
                sides.Bottom.Outer.End.X,
                sides.Bottom.Outer.Start.X,
            ]),
            Distinct(
            [
                sides.Top.Outer.Start.Y,
                sides.Top.Inner.Start.Y,
                sides.Bottom.Inner.Start.Y,
                sides.Bottom.Outer.Start.Y,
                sides.Left.Outer.End.Y,
                sides.Left.Outer.Start.Y,
                sides.Right.Outer.Start.Y,
                sides.Right.Outer.End.Y,
            ]));
    }

    private static double[] Distinct(double[] values)
    {
        // Most of these repeat: the four edges of the ring account for all of them unless the corners are rounded.
        // Compacting them where they lie and copying once, at the size they came to, keeps an axis to the one
        // array it arrived in, or two where something was dropped, in place of the four a list that is grown and
        // then copied out costs.
        var count = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var rounded = Math.Round(values[index], Precision);
            if (!Contains(values, count, rounded))
            {
                // Never past index, so this only ever writes over a value already read.
                values[count] = rounded;
                count++;
            }
        }

        if (count == values.Length)
        {
            return values;
        }

        var result = new double[count];
        Array.Copy(values, result, count);
        return result;
    }

    private static bool Contains(double[] values, int count, double value)
    {
        for (var index = 0; index < count; index++)
        {
            if (values[index].Equals(value))
            {
                return true;
            }
        }

        return false;
    }
}
