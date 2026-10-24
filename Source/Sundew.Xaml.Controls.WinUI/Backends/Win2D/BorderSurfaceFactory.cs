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
/// Creates the surface a border draws on. This is the Win2D build of the factory, used by WinUI on Windows and by the
/// Windows head of an Uno app.
/// </summary>
internal static class BorderSurfaceFactory
{
    public static IBorderSurface Create(Action<IBorderDrawingSession, Size> draw)
    {
        return new Win2DBorderSurface(draw);
    }
}
