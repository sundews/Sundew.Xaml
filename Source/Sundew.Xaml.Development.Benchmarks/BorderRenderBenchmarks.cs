// ----------------------------------------------------------------------------------------------------------------------
// <copyright file="BorderRenderBenchmarks.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Benchmarks;

using BenchmarkDotNet.Attributes;

/// <summary>
/// What it costs to turn one border into the fills a backend carries out.
/// </summary>
/// <remarks>
/// This is the part of drawing a border that WinUI and MAUI share and that neither of them can make faster on its
/// own. What it leaves out is the rasterising afterwards, which depends on the machine and the graphics driver and
/// so is not something a benchmark can hold still.
/// </remarks>
[MemoryDiagnoser]
public class BorderRenderBenchmarks
{
    private readonly CountingDrawingSession session = new CountingDrawingSession();
    private BorderScene scene = null!;

    /// <summary>
    /// Gets or sets what the four sides are painted with.
    /// </summary>
    [Params(SidePaints.FourColours, SidePaints.OneColour, SidePaints.SeeThrough, SidePaints.Gradient)]
    public SidePaints Sides { get; set; }

    /// <summary>
    /// Gets or sets the corner radius, which decides how many slices each corner is swept in.
    /// </summary>
    [Params(0d, 4d, 20d)]
    public double Radius { get; set; }

    /// <summary>
    /// Resolves the border once, the way a host resolves its brushes once.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.scene = BorderScene.Create(this.Sides, this.Radius);
    }

    /// <summary>
    /// Draws the border.
    /// </summary>
    /// <returns>How many regions were filled.</returns>
    [Benchmark]
    public int Render()
    {
        this.session.Reset();
        this.scene.Render(this.session);
        return this.session.Fills;
    }
}
