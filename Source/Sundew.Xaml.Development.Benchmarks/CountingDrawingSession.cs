// ----------------------------------------------------------------------------------------------------------------------
// <copyright file="CountingDrawingSession.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Benchmarks;

using System;
using Sundew.Xaml.Controls;

/// <summary>
/// A drawing session that draws nothing and counts what it was asked to draw.
/// </summary>
/// <remarks>
/// This is what makes the shared renderer measurable on its own. <see cref="BorderRenderer"/> reaches a backend
/// only through <see cref="IBorderDrawingSession"/>, so standing in for the backend leaves everything the backends
/// have in common and nothing that differs between them. It allocates nothing itself, so what a run reports
/// allocating is the renderer's and no one else's.
/// </remarks>
public sealed class CountingDrawingSession : IBorderDrawingSession
{
    private readonly LayerHandle handle;

    /// <summary>
    /// Initializes a new instance of the <see cref="CountingDrawingSession"/> class.
    /// </summary>
    public CountingDrawingSession()
    {
        this.handle = new LayerHandle();
    }

    /// <summary>
    /// Gets how many regions were filled, which is the work the rasteriser is handed.
    /// </summary>
    public int Fills { get; private set; }

    /// <summary>
    /// Gets how many of those fills were gradients, which are the expensive ones.
    /// </summary>
    public int GradientFills { get; private set; }

    /// <summary>
    /// Gets how many corners were composited through a layer of their own.
    /// </summary>
    public int Layers { get; private set; }

    /// <summary>
    /// Forgets everything counted so far.
    /// </summary>
    public void Reset()
    {
        this.Fills = 0;
        this.GradientFills = 0;
        this.Layers = 0;
    }

    /// <inheritdoc/>
    public void Fill(SideQuad sideQuad, Paint paint)
    {
        this.Count(paint);
    }

    /// <inheritdoc/>
    public void Fill(CornerWedge cornerWedge, Paint paint)
    {
        this.Count(paint);
    }

    /// <inheritdoc/>
    public void Fill(RoundedFigure figure, Paint paint)
    {
        this.Count(paint);
    }

    /// <inheritdoc/>
    public IDisposable CreateOpacityLayer(double opacity)
    {
        this.Layers++;
        return this.handle;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    private void Count(Paint paint)
    {
        this.Fills++;
        if (paint.IsGradient)
        {
            this.GradientFills++;
        }
    }

    /// <summary>
    /// Ends a layer. One is made and handed out again every time, so that counting a render allocates nothing.
    /// </summary>
    private sealed class LayerHandle : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
