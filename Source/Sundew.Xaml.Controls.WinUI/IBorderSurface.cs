// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBorderSurface.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using Microsoft.UI.Xaml;

/// <summary>
/// The element a border draws itself on.
/// </summary>
/// <remarks>
/// Neither WinUI nor Uno lets a control paint itself the way WPF and Avalonia do, so the border owns an element that
/// can, and sits it behind its content. Which element that is, is the one thing that differs between the backends:
/// a Win2D canvas on Windows, a Skia canvas everywhere else.
/// </remarks>
public interface IBorderSurface
{
    /// <summary>
    /// Gets the element to place in the visual tree.
    /// </summary>
    FrameworkElement Element { get; }

    /// <summary>
    /// Asks for the surface to be drawn again.
    /// </summary>
    void Invalidate();

    /// <summary>
    /// Releases what the surface holds, which Win2D requires before the border leaves the visual tree.
    /// </summary>
    void Release();
}
