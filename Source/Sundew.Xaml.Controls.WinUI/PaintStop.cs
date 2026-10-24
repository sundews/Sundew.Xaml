// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PaintStop.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using Windows.UI;

/// <summary>
/// One stop of a gradient fill.
/// </summary>
/// <param name="Color">The colour at this stop.</param>
/// <param name="Offset">How far along the gradient the stop sits, from 0 to 1.</param>
public readonly record struct PaintStop(Color Color, double Offset);
