// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SkiaBorderSurface.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Backends;

using System;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

/// <summary>
/// A border surface backed by the Skia canvas Uno renders its own visuals into.
/// </summary>
internal sealed class SkiaBorderSurface : SKCanvasElement, IBorderSurface
{
    private readonly Action<IBorderDrawingSession, Size> draw;

    public SkiaBorderSurface(Action<IBorderDrawingSession, Size> draw)
    {
        this.draw = draw;
    }

    public FrameworkElement Element => this;

    /// <summary>
    /// Releases what the surface holds. Skia draws straight into the canvas Uno already owns, so there is nothing of
    /// its own to let go of.
    /// </summary>
    public void Release()
    {
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        using var session = new SkiaDrawingSession(canvas);
        this.draw(session, area);
    }
}
