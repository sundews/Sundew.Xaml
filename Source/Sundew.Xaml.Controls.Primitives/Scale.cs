// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Scale.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;

/// <summary>
/// The number of device pixels per device independent unit, per axis.
/// </summary>
/// <param name="X">The horizontal scale.</param>
/// <param name="Y">The vertical scale.</param>
public readonly record struct Scale(double X, double Y)
{
    /// <summary>
    /// Gets a scale of one device pixel per device independent unit on both axes.
    /// </summary>
    public static Scale Identity { get; } = new Scale(1, 1);

    /// <summary>
    /// Gets the smaller of the two axis scales, used where a value cannot be aligned per axis.
    /// </summary>
    public double Minimum => Math.Min(this.X, this.Y);

    /// <summary>
    /// Creates a scale that is uniform on both axes.
    /// </summary>
    /// <param name="scale">The scale.</param>
    /// <returns>The uniform scale.</returns>
    public static Scale Uniform(double scale)
    {
        return new Scale(scale, scale);
    }
}
