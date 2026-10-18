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
    /// Parses the specified text into <see cref="Stops"/>.
    /// </summary>
    /// <remarks>
    /// XAML is culture invariant, so the values are separated by a comma and use a period as the decimal separator.
    /// </remarks>
    /// <param name="text">The text, either a single value used for both stops, or two comma separated values.</param>
    /// <returns>The parsed stops.</returns>
    public static Stops Parse(string text)
    {
        return StopsConverter.FromString(text, null);
    }

    /// <summary>
    /// Gets or sets the first value in the sequence.
    /// </summary>
    public double First { get; set; }

    /// <summary>
    /// Gets or sets the second value in the sequence.
    /// </summary>
    public double Second { get; set; }

    /// <summary>
    /// Gets how far the blend has come at the specified position, holding at either end outside the two stops.
    /// </summary>
    /// <param name="position">The position, from 0 to 1.</param>
    /// <returns>The amount of the second colour, from 0 to 1.</returns>
    public double GetBlendAmount(double position)
    {
        var span = this.Second - this.First;
        if (span <= 0)
        {
            return position < this.First ? 0 : 1;
        }

        var amount = (position - this.First) / span;
        return amount < 0 ? 0 : amount > 1 ? 1 : amount;
    }
}