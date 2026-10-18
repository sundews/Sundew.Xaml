// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BorderMetrics.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

/// <summary>
/// The measurements a border is laid out from.
/// </summary>
/// <param name="Width">The rendered width.</param>
/// <param name="Height">The rendered height.</param>
/// <param name="Thickness">The border thickness per side.</param>
/// <param name="CornerRadius">The corner radius per corner.</param>
/// <param name="Scale">The device pixel scale to align to.</param>
public readonly record struct BorderMetrics(
    double Width,
    double Height,
    Sides<double> Thickness,
    Corners<double> CornerRadius,
    Scale Scale);
