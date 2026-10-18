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

    internal static Guidelines From(double[] x, double[] y)
    {
        return new Guidelines(Distinct(x), Distinct(y));
    }

    private static double[] Distinct(double[] values)
    {
        var result = new List<double>(values.Length);
        foreach (var value in values)
        {
            var rounded = Math.Round(value, Precision);
            if (!result.Contains(rounded))
            {
                result.Add(rounded);
            }
        }

        return result.ToArray();
    }
}
