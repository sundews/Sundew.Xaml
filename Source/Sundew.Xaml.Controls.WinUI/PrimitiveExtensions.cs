// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrimitiveExtensions.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using Microsoft.UI.Xaml;

/// <summary>
/// Translates between the platform neutral border primitives and the WinUI types.
/// </summary>
internal static class PrimitiveExtensions
{
    public static Sides<double> ToSides(this Thickness thickness)
    {
        return new Sides<double>(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
    }

    public static Corners<double> ToCorners(this CornerRadius cornerRadius)
    {
        return new Corners<double>(cornerRadius.TopLeft, cornerRadius.TopRight, cornerRadius.BottomRight, cornerRadius.BottomLeft);
    }
}
