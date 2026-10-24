// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SideBrushes.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;
using System.ComponentModel;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation.Metadata;

/// <summary>
/// Defines brushes for the sides of a rectangle.
/// </summary>
/// <remarks>
/// Neither WinUI nor Uno honours a type converter, so CreateFromString is what lets XAML write these as a
/// string. The type converter is declared as well, for anything that does honour one.
/// </remarks>
[TypeConverter(typeof(SideBrushesConverter))]
[CreateFromString(MethodName = "Sundew.Xaml.Controls.SideBrushes.Parse")]
public class SideBrushes
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SideBrushes"/> class with all sides unset.
    /// </summary>
    public SideBrushes()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SideBrushes"/> class with all sides set to the specified brush.
    /// </summary>
    /// <param name="brush">The brush.</param>
    public SideBrushes(Brush? brush)
        : this(brush, brush, brush, brush)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SideBrushes"/> class with the top and left sides set to the specified topLeft brush and the bottom and right sides set to the specified bottomRight brush.
    /// </summary>
    /// <param name="topLeft">The top and left.</param>
    /// <param name="bottomRight">The bottom and right.</param>
    public SideBrushes(Brush? topLeft, Brush? bottomRight)
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
    public SideBrushes(Brush? left, Brush? top, Brush? right, Brush? bottom)
    {
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
    }

    /// <summary>
    /// Gets or sets the left brush.
    /// </summary>
    public Brush? Left { get; set; }

    /// <summary>
    /// Gets or sets the top brush.
    /// </summary>
    public Brush? Top { get; set; }

    /// <summary>
    /// Gets or sets the right brush.
    /// </summary>
    public Brush? Right { get; set; }

    /// <summary>
    /// Gets or sets the bottom brush.
    /// </summary>
    public Brush? Bottom { get; set; }

    /// <summary>
    /// Parses the specified text into <see cref="SideBrushes"/>.
    /// </summary>
    /// <param name="text">The text, holding one, two or four comma separated brushes.</param>
    /// <returns>The parsed side brushes.</returns>
    public static SideBrushes Parse(string text)
    {
        var values = text.Split(',');
        switch (values.Length)
        {
            case 1:
                return new SideBrushes(ParseBrush(values[0]));
            case 2:
                return new SideBrushes(ParseBrush(values[0]), ParseBrush(values[1]));
            case 4:
                return new SideBrushes(
                    ParseBrush(values[0]),
                    ParseBrush(values[1]),
                    ParseBrush(values[2]),
                    ParseBrush(values[3]));
            default:
                throw new FormatException("Invalid SideBrushes");
        }
    }

    private static Brush? ParseBrush(string text)
    {
        return XamlBindingHelper.ConvertValue(typeof(Brush), text.Trim()) as Brush;
    }
}
