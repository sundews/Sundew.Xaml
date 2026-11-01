// ----------------------------------------------------------------------------------------------------------------------
// <copyright file="BorderLayoutBenchmarks.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Benchmarks;

using BenchmarkDotNet.Attributes;
using Sundew.Xaml.Controls;

/// <summary>
/// What it costs to work out the geometry of a border, with nothing painted.
/// </summary>
/// <remarks>
/// Kept apart from <see cref="BorderRenderBenchmarks"/> so that a change to the corner sweep can be told from a
/// change to the layout underneath it.
/// </remarks>
[MemoryDiagnoser]
public class BorderLayoutBenchmarks
{
    private BorderMetrics metrics;

    /// <summary>
    /// Gets or sets the corner radius.
    /// </summary>
    [Params(0d, 4d, 20d)]
    public double Radius { get; set; }

    /// <summary>
    /// Works out the measurements once.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.metrics = BorderScene.CreateMetrics(this.Radius);
    }

    /// <summary>
    /// Lays the border out.
    /// </summary>
    /// <returns>The layout.</returns>
    [Benchmark]
    public BorderLayout Layout()
    {
        return BorderLayoutCalculator.CalculateBorder(this.metrics);
    }
}
