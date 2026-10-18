// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SideBrushes.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System.ComponentModel;
using global::Avalonia.Media;

/// <summary>
/// Defines brushes for the sides of a rectangle.
/// </summary>
[TypeConverter(typeof(SideBrushesConverter))]
public class SideBrushes
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SideBrushes"/> class with all sides set to black.
    /// </summary>
    public SideBrushes()
        : this(Brushes.Black)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SideBrushes"/> class with all sides set to the specified brush.
    /// </summary>
    /// <param name="brush">The brush.</param>
    public SideBrushes(IBrush? brush)
        : this(brush, brush, brush, brush)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SideBrushes"/> class with the top and left sides set to the specified topLeft brush and the bottom and right sides set to the specified bottomRight brush.
    /// </summary>
    /// <param name="topLeft">The top and left.</param>
    /// <param name="bottomRight">The bottom and right.</param>
    public SideBrushes(IBrush? topLeft, IBrush? bottomRight)
        : this(topLeft, topLeft, bottomRight, bottomRight)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SideBrushes"/> class with the specified brushes.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="top">The top.</param>
    /// <param name="right">The right.</param>
    /// <param name="bottom">The bottom.</param>
    public SideBrushes(IBrush? left, IBrush? top, IBrush? right, IBrush? bottom)
    {
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
    }

    /// <summary>
    /// Gets or sets the left brush.
    /// </summary>
    public IBrush? Left { get; set; }

    /// <summary>
    /// Gets or sets the top brush.
    /// </summary>
    public IBrush? Top { get; set; }

    /// <summary>
    /// Gets or sets the right brush.
    /// </summary>
    public IBrush? Right { get; set; }

    /// <summary>
    /// Gets or sets the bottom brush.
    /// </summary>
    public IBrush? Bottom { get; set; }

    /// <summary>
    /// Parses the specified text into <see cref="SideBrushes"/>.
    /// </summary>
    /// <param name="text">The text, holding one, two or four comma separated brushes.</param>
    /// <returns>The parsed side brushes.</returns>
    public static SideBrushes Parse(string text)
    {
        return SideBrushesConverter.FromString(text, null);
    }
}
