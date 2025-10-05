// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Stops.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System.ComponentModel;

/// <summary>
/// Represents a pair of numeric stop values, typically used to define a range or interval.
/// </summary>
[TypeConverter(typeof(StopsConverter))]
public class Stops
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Stops"/> class.
    /// </summary>
    public Stops()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Stops"/> class with specified stop values.
    /// </summary>
    /// <param name="first">The first stop.</param>
    /// <param name="second">The second stop.</param>
    public Stops(double first, double second)
    {
        this.First = first;
        this.Second = second;
    }

    /// <summary>
    /// Gets or sets the first value in the sequence.
    /// </summary>
    public double First { get; set; }

    /// <summary>
    /// Gets or sets the second value in the sequence.
    /// </summary>
    public double Second { get; set; }
}