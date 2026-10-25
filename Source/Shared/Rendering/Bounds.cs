// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Bounds.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

/// <summary>
/// The rectangle a region being filled occupies, which a relative gradient is resolved against.
/// </summary>
/// <remarks>
/// Each backend knows the bounds of the geometry it has just built, and each expresses them in its own rectangle
/// type. This is the one they all hand to <see cref="Paint.ResolveEnds"/>.
/// </remarks>
/// <param name="X">The left edge.</param>
/// <param name="Y">The top edge.</param>
/// <param name="Width">The width.</param>
/// <param name="Height">The height.</param>
public readonly record struct Bounds(double X, double Y, double Width, double Height);
