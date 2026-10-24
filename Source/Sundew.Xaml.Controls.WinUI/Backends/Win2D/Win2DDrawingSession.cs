// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Win2DDrawingSession.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Backends;

using System;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;

/// <summary>
/// Draws a border with Win2D, which is the Direct2D path used on Windows.
/// </summary>
public sealed class Win2DDrawingSession : IBorderDrawingSession
{
    private readonly CanvasDrawingSession session;

    /// <summary>
    /// Initializes a new instance of the <see cref="Win2DDrawingSession"/> class.
    /// </summary>
    /// <param name="session">The Win2D drawing session to draw on.</param>
    public Win2DDrawingSession(CanvasDrawingSession session)
    {
        this.session = session;
        this.session.Antialiasing = CanvasAntialiasing.Antialiased;
    }

    /// <inheritdoc/>
    public void Fill(SideQuad sideQuad, Paint paint)
    {
        using var geometry = CreateGeometry(this.session, sideQuad);
        this.Fill(geometry, paint);
    }

    /// <inheritdoc/>
    public void Fill(CornerWedge cornerWedge, Paint paint)
    {
        using var geometry = CreateGeometry(this.session, cornerWedge);
        this.Fill(geometry, paint);
    }

    /// <inheritdoc/>
    public void Fill(RoundedFigure figure, Paint paint)
    {
        using var geometry = CreateGeometry(this.session, figure);
        this.Fill(geometry, paint);
    }

    /// <inheritdoc/>
    public IDisposable CreateOpacityLayer(double opacity)
    {
        return this.session.CreateLayer((float)opacity);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // The session belongs to the canvas control, which disposes it when the draw is over.
    }

    private static CanvasGeometry CreateGeometry(CanvasDrawingSession session, SideQuad sideQuad)
    {
        using var builder = new CanvasPathBuilder(session);
        builder.BeginFigure(ToVector(sideQuad.Outer.Start));
        builder.AddLine(ToVector(sideQuad.Outer.End));
        builder.AddLine(ToVector(sideQuad.Inner.End));
        builder.AddLine(ToVector(sideQuad.Inner.Start));
        builder.EndFigure(CanvasFigureLoop.Closed);
        return CanvasGeometry.CreatePath(builder);
    }

    private static CanvasGeometry CreateGeometry(CanvasDrawingSession session, CornerWedge cornerWedge)
    {
        using var builder = new CanvasPathBuilder(session);
        builder.BeginFigure(ToVector(cornerWedge.Outer.Start));
        builder.AddQuadraticBezier(ToVector(cornerWedge.Outer.Control), ToVector(cornerWedge.Outer.End));
        builder.AddLine(ToVector(cornerWedge.Inner.Start));
        builder.AddQuadraticBezier(ToVector(cornerWedge.Inner.Control), ToVector(cornerWedge.Inner.End));
        builder.EndFigure(CanvasFigureLoop.Closed);
        return CanvasGeometry.CreatePath(builder);
    }

    private static CanvasGeometry CreateGeometry(CanvasDrawingSession session, RoundedFigure figure)
    {
        using var builder = new CanvasPathBuilder(session);
        builder.BeginFigure(ToVector(figure.Start));
        builder.AddLine(ToVector(figure.Edges.Left.End));
        builder.AddQuadraticBezier(ToVector(figure.Controls.TopLeft), ToVector(figure.Edges.Top.Start));
        builder.AddLine(ToVector(figure.Edges.Top.End));
        builder.AddQuadraticBezier(ToVector(figure.Controls.TopRight), ToVector(figure.Edges.Right.Start));
        builder.AddLine(ToVector(figure.Edges.Right.End));
        builder.AddQuadraticBezier(ToVector(figure.Controls.BottomRight), ToVector(figure.Edges.Bottom.Start));
        builder.AddLine(ToVector(figure.Edges.Bottom.End));
        builder.AddQuadraticBezier(ToVector(figure.Controls.BottomLeft), ToVector(figure.Edges.Left.Start));
        builder.EndFigure(CanvasFigureLoop.Closed);
        return CanvasGeometry.CreatePath(builder);
    }

    private static Vector2 ToVector(Position position)
    {
        return new Vector2((float)position.X, (float)position.Y);
    }

    private void Fill(CanvasGeometry geometry, Paint paint)
    {
        if (!paint.IsGradient)
        {
            this.session.FillGeometry(geometry, paint.Color);
            return;
        }

        var stops = new CanvasGradientStop[paint.Stops.Count];
        for (var index = 0; index < stops.Length; index++)
        {
            var stop = paint.Stops[index];
            stops[index] = new CanvasGradientStop { Position = (float)stop.Offset, Color = stop.Color };
        }

        paint.ResolveEnds(geometry.ComputeBounds(), out var start, out var end);
        using var brush = new CanvasLinearGradientBrush(this.session, stops)
        {
            StartPoint = ToVector(start),
            EndPoint = ToVector(end),
        };
        this.session.FillGeometry(geometry, brush);
    }
}
