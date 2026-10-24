// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Win2DBorderSurface.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Backends;

using System;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using Windows.Foundation;

/// <summary>
/// A border surface backed by a Win2D canvas.
/// </summary>
internal sealed class Win2DBorderSurface : IBorderSurface
{
    private readonly Action<IBorderDrawingSession, Size> draw;
    private CanvasControl? canvasControl;

    public Win2DBorderSurface(Action<IBorderDrawingSession, Size> draw)
    {
        this.draw = draw;
        this.canvasControl = new CanvasControl();
        this.canvasControl.Draw += this.OnDraw;
    }

    public FrameworkElement Element => this.canvasControl ?? throw new ObjectDisposedException(nameof(Win2DBorderSurface));

    public void Invalidate()
    {
        this.canvasControl?.Invalidate();
    }

    /// <summary>
    /// Releases the canvas, which Win2D requires before it leaves the visual tree so that its device resources go
    /// with it rather than being held until a garbage collection that may never come.
    /// </summary>
    public void Release()
    {
        if (this.canvasControl == null)
        {
            return;
        }

        this.canvasControl.Draw -= this.OnDraw;
        this.canvasControl.RemoveFromVisualTree();
        this.canvasControl = null;
    }

    private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        using var session = new Win2DDrawingSession(args.DrawingSession);
        this.draw(session, new Size(sender.ActualWidth, sender.ActualHeight));
    }
}
