// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SkiaBorderSurface.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Backends;

using System;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

/// <summary>
/// A border surface backed by a Skia canvas, which MAUI provides on every target it has.
/// </summary>
internal sealed class SkiaBorderSurface : SKCanvasView, IBorderSurface
{
    private readonly Action<IBorderDrawingSession, Size, double> draw;

    public SkiaBorderSurface(Action<IBorderDrawingSession, Size, double> draw)
    {
        this.draw = draw;
        this.PaintSurface += this.OnPaintSurface;
    }

    public View Element => this;

    public void Invalidate()
    {
        this.InvalidateSurface();
    }

    /// <summary>
    /// Releases what the surface holds. The canvas belongs to MAUI, so there is nothing of its own to let go of
    /// beyond the handler keeping this alive.
    /// </summary>
    public void Release()
    {
        this.PaintSurface -= this.OnPaintSurface;
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        // The canvas is in device pixels while the border lays itself out in device independent ones, so the canvas
        // is scaled and the border is drawn at its own size. Taking the scale from the canvas rather than from the
        // display keeps the two in step even where MAUI has rounded the view to whole pixels.
        var width = this.Width;
        var height = this.Height;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        // The border rounds its geometry to the pixel grid it is really drawn on, so it is told the scale the
        // canvas just reported rather than left to assume one.
        var scale = e.Info.Width / width;
        canvas.Scale((float)scale, (float)(e.Info.Height / height));
        using var session = new SkiaDrawingSession(canvas);
        this.draw(session, new Size(width, height), scale);
    }
}
