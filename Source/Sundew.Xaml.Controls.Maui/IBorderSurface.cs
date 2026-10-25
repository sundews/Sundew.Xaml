// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBorderSurface.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

/// <summary>
/// The view a border draws itself on.
/// </summary>
/// <remarks>
/// MAUI does not let a control paint itself the way WPF and Avalonia do, so the border owns a view that can and
/// sits it behind its content. Only one surface exists today, a Skia canvas, which MAUI offers on every target it
/// has. The seam is kept all the same, because it is the one place a Win2D surface would be added for the Windows
/// target without the border or the renderer knowing.
/// </remarks>
internal interface IBorderSurface
{
    /// <summary>
    /// Gets the view to place in the visual tree.
    /// </summary>
    View Element { get; }

    /// <summary>
    /// Asks for the surface to be drawn again.
    /// </summary>
    void Invalidate();

    /// <summary>
    /// Releases what the surface holds.
    /// </summary>
    void Release();
}
