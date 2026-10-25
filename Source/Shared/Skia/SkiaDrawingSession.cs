// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SkiaDrawingSession.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Backends;

using System;
using SkiaSharp;

/// <summary>
/// Draws a border with Skia, which is the path used on every Uno target other than Windows.
/// </summary>
public sealed class SkiaDrawingSession : IBorderDrawingSession
{
    private readonly SKCanvas canvas;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkiaDrawingSession"/> class.
    /// </summary>
    /// <param name="canvas">The Skia canvas to draw on.</param>
    public SkiaDrawingSession(SKCanvas canvas)
    {
        this.canvas = canvas;
    }

    /// <inheritdoc/>
    public void Fill(SideQuad sideQuad, Paint paint)
    {
        using var path = new SKPath();
        path.MoveTo(ToPoint(sideQuad.Outer.Start));
        path.LineTo(ToPoint(sideQuad.Outer.End));
        path.LineTo(ToPoint(sideQuad.Inner.End));
        path.LineTo(ToPoint(sideQuad.Inner.Start));
        path.Close();
        this.Fill(path, paint);
    }

    /// <inheritdoc/>
    public void Fill(CornerWedge cornerWedge, Paint paint)
    {
        using var path = new SKPath();
        path.MoveTo(ToPoint(cornerWedge.Outer.Start));
        path.QuadTo(ToPoint(cornerWedge.Outer.Control), ToPoint(cornerWedge.Outer.End));
        path.LineTo(ToPoint(cornerWedge.Inner.Start));
        path.QuadTo(ToPoint(cornerWedge.Inner.Control), ToPoint(cornerWedge.Inner.End));
        path.Close();
        this.Fill(path, paint);
    }

    /// <inheritdoc/>
    public void Fill(RoundedFigure figure, Paint paint)
    {
        using var path = new SKPath();
        path.MoveTo(ToPoint(figure.Start));
        path.LineTo(ToPoint(figure.Edges.Left.End));
        path.QuadTo(ToPoint(figure.Controls.TopLeft), ToPoint(figure.Edges.Top.Start));
        path.LineTo(ToPoint(figure.Edges.Top.End));
        path.QuadTo(ToPoint(figure.Controls.TopRight), ToPoint(figure.Edges.Right.Start));
        path.LineTo(ToPoint(figure.Edges.Right.End));
        path.QuadTo(ToPoint(figure.Controls.BottomRight), ToPoint(figure.Edges.Bottom.Start));
        path.LineTo(ToPoint(figure.Edges.Bottom.End));
        path.QuadTo(ToPoint(figure.Controls.BottomLeft), ToPoint(figure.Edges.Left.Start));
        path.Close();
        this.Fill(path, paint);
    }

    /// <inheritdoc/>
    public IDisposable CreateOpacityLayer(double opacity)
    {
        var alpha = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(opacity * 255)));
        using var layerPaint = new SKPaint { Color = new SKColor(0, 0, 0, alpha) };
        var count = this.canvas.SaveLayer(layerPaint);
        return new Layer(this.canvas, count);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // The canvas belongs to the element, which hands it over for the duration of the draw only.
    }

    private static SKPoint ToPoint(Position position)
    {
        return new SKPoint((float)position.X, (float)position.Y);
    }

    private static SKColor ToColor(PaintColor color)
    {
        return new SKColor(color.R, color.G, color.B, color.A);
    }

    private void Fill(SKPath path, Paint paint)
    {
        using var skPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        if (paint.IsGradient)
        {
            var colors = new SKColor[paint.Stops.Count];
            var offsets = new float[paint.Stops.Count];
            for (var index = 0; index < colors.Length; index++)
            {
                colors[index] = ToColor(paint.Stops[index].Color);
                offsets[index] = (float)paint.Stops[index].Offset;
            }

            var bounds = path.Bounds;
            paint.ResolveEnds(
                new Bounds(bounds.Left, bounds.Top, bounds.Width, bounds.Height),
                out var start,
                out var end);
            skPaint.Shader = SKShader.CreateLinearGradient(
                ToPoint(start),
                ToPoint(end),
                colors,
                offsets,
                SKShaderTileMode.Clamp);
        }
        else
        {
            skPaint.Color = ToColor(paint.Color);
        }

        this.canvas.DrawPath(path, skPaint);
        skPaint.Shader?.Dispose();
    }

    private sealed class Layer : IDisposable
    {
        private readonly SKCanvas canvas;
        private readonly int count;

        public Layer(SKCanvas canvas, int count)
        {
            this.canvas = canvas;
            this.count = count;
        }

        public void Dispose()
        {
            this.canvas.RestoreToCount(this.count);
        }
    }
}
