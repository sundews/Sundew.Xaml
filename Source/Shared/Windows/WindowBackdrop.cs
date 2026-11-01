// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowBackdrop.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

/// <summary>
/// What the window manager draws behind a window, where it draws anything there at all.
/// </summary>
/// <remarks>
/// A backdrop is the window manager's to draw, not the application's, which is why asking for one is asking rather
/// than setting: a window asking for something the system it is running on does not have is given what that system
/// does instead, and says so through the backdrop it ends up with. Windows 11 has all of these; Windows 10 has
/// none of them, and neither has any other system Avalonia runs on today.
/// </remarks>
public enum WindowBackdrop
{
    /// <summary>
    /// Nothing is asked for, which leaves the window however the framework and the system would have it. This is
    /// the default.
    /// </summary>
    Default,

    /// <summary>
    /// No backdrop: the window's own background is all there is behind the content.
    /// </summary>
    None,

    /// <summary>
    /// Mica, the desktop wallpaper blurred and tinted far enough to be a surface of its own. It is meant for the
    /// window a session is spent in.
    /// </summary>
    Mica,

    /// <summary>
    /// Mica, tinted more strongly, for a window whose caption is a tab strip and wants the strip to read as the
    /// background rather than the page.
    /// </summary>
    MicaAlt,

    /// <summary>
    /// Acrylic, which blurs whatever happens to be behind the window rather than the wallpaper. It is meant for
    /// something transient on top of other windows.
    /// </summary>
    Acrylic,
}
