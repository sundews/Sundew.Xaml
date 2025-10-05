// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThemeUpdateEventArgs.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Theming;

using System;

/// <summary>Event args for the theme changed event.</summary>
public sealed class ThemeUpdateEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeUpdateEventArgs"/> class.
    /// </summary>
    /// <param name="oldTheme">The old theme.</param>
    /// <param name="newTheme">The new theme.</param>
    /// <param name="oldThemeMode">The old theme mode.</param>
    /// <param name="newThemeMode">The new theme mode.</param>
    /// <param name="changeType">The change type.</param>
    public ThemeUpdateEventArgs(Theme? oldTheme, Theme newTheme, ThemeMode? oldThemeMode, ThemeMode newThemeMode, ThemeChangeType changeType)
    {
        this.OldTheme = oldTheme;
        this.NewTheme = newTheme;
        this.OldThemeMode = oldThemeMode;
        this.NewThemeMode = newThemeMode;
        this.ChangeType = changeType;
    }

    /// <summary>
    /// Gets the old theme.
    /// </summary>
    public Theme? OldTheme { get; }

    /// <summary>
    /// Gets the new theme.
    /// </summary>
    public Theme NewTheme { get; }

    /// <summary>
    /// Gets the old theme mode.
    /// </summary>
    public ThemeMode? OldThemeMode { get; }

    /// <summary>
    /// Gets the new theme mode.
    /// </summary>
    public ThemeMode NewThemeMode { get; }

    /// <summary>
    /// Gets the type of theme change that occurred.
    /// </summary>
    public ThemeChangeType ChangeType { get; }
}