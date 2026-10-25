// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BorderSurfaceFactory.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Backends;

using System;

/// <summary>
/// Creates the surface a border draws on.
/// </summary>
internal static class BorderSurfaceFactory
{
    public static IBorderSurface Create(Action<IBorderDrawingSession, Size, double> draw)
    {
        return new SkiaBorderSurface(draw);
    }
}
