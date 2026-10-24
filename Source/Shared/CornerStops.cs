// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CornerStops.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System.ComponentModel;

/// <summary>
/// Represents a set of four corner-specific stop values, typically used to define gradient or transition points for each corner of a rectangular region.
/// </summary>
[TypeConverter(typeof(CornerStopsConverter))]
#if SUNDEW_CREATE_FROM_STRING
[Windows.Foundation.Metadata.CreateFromString(MethodName = "Sundew.Xaml.Controls.CornerStops.Parse")]
#endif
public class CornerStops
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CornerStops"/> class with default stop values of 0.4 and 0.6 for all corners.
    /// </summary>
    public CornerStops()
        : this(new Stops(0.4, 0.6))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CornerStops"/> class with the same stop values for all corners.
    /// </summary>
    /// <param name="stops">The stops.</param>
    public CornerStops(Stops stops)
        : this(stops, stops, stops, stops)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CornerStops"/> class with specified stop values for the top-left and bottom-right corners, while the top-right and bottom-left corners inherit the same values as their adjacent corners.
    /// </summary>
    /// <param name="topLeft">The top left.</param>
    /// <param name="bottomRight">The bottom right.</param>
    public CornerStops(Stops topLeft, Stops bottomRight)
        : this(topLeft, topLeft, bottomRight, bottomRight)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CornerStops"/> class with the specified stops for each corner.
    /// </summary>
    /// <param name="topLeft">The stops to use for the top-left corner.</param>
    /// <param name="topRight">The stops to use for the top-right corner.</param>
    /// <param name="bottomRight">The stops to use for the bottom-right corner.</param>
    /// <param name="bottomLeft">The stops to use for the bottom-left corner.</param>
    public CornerStops(Stops topLeft, Stops topRight, Stops bottomRight, Stops bottomLeft)
    {
        this.TopLeft = topLeft;
        this.TopRight = topRight;
        this.BottomRight = bottomRight;
        this.BottomLeft = bottomLeft;
    }

    /// <summary>
    /// Parses the specified text into <see cref="CornerStops"/>.
    /// </summary>
    /// <remarks>
    /// XAML is culture invariant, so the values are separated by a comma and use a period as the decimal separator.
    /// </remarks>
    /// <param name="text">The text, holding one, two, four or eight comma separated values.</param>
    /// <returns>The parsed corner stops.</returns>
    public static CornerStops Parse(string text)
    {
        return CornerStopsConverter.FromString(text, null);
    }

    /// <summary>
    /// Gets or sets the stops for the top-left corner.
    /// </summary>
    public Stops TopLeft { get; set; }

    /// <summary>
    /// Gets or sets the stops for the top-right corner.
    /// </summary>
    public Stops TopRight { get; set; }

    /// <summary>
    /// Gets or sets the stops for the bottom-right corner.
    /// </summary>
    public Stops BottomRight { get; set; }

    /// <summary>
    /// Gets or sets the stops for the bottom-left corner.
    /// </summary>
    public Stops BottomLeft { get; set; }
}
