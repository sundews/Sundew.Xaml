// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BorderSurfaceFactory.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Backends;

using System;
using Windows.Foundation;

/// <summary>
/// Creates the surface a border draws on. This is the Skia build of the factory, used on every Uno target other than
/// Windows.
/// </summary>
internal static class BorderSurfaceFactory
{
    public static IBorderSurface Create(Action<IBorderDrawingSession, Size> draw)
    {
        return new SkiaBorderSurface(draw);
    }
}
