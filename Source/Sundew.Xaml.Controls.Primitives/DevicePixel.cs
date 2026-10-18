// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DevicePixel.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System;

/// <summary>
/// Aligns layout values to whole device pixels.
/// </summary>
public static class DevicePixel
{
    /// <summary>
    /// Rounds the specified value to the nearest whole device pixel for the given scale.
    /// </summary>
    /// <param name="value">The value in device independent units.</param>
    /// <param name="scale">The scale, where 1 means one device pixel per device independent unit.</param>
    /// <returns>The rounded value in device independent units, or <paramref name="value"/> if the scale is not positive.</returns>
    public static double Round(double value, double scale)
    {
        if (scale <= 0)
        {
            return value;
        }

        return Math.Round(value * scale) / scale;
    }
}
