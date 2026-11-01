// ----------------------------------------------------------------------------------------------------------------------
// <copyright file="SidePaints.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Benchmarks;

/// <summary>
/// What the four sides of the benchmarked border are painted with, which is what decides how much work its corners
/// come to.
/// </summary>
public enum SidePaints
{
    /// <summary>A different colour on each side, so every corner sweeps a blend between two of them.</summary>
    FourColours,

    /// <summary>One colour on all four sides, so every corner comes out flat and is filled in a single go.</summary>
    OneColour,

    /// <summary>Four colours, none of them opaque, so every corner is composited through a layer of its own.</summary>
    SeeThrough,

    /// <summary>
    /// A gradient down two opposite sides, which leaves no single colour for a corner to sweep from, so every
    /// corner falls back to one fill.
    /// </summary>
    Gradient,
}
