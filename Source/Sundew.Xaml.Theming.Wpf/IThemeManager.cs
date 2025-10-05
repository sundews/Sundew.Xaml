// ----------------------------------------------------------------------------------------
// <copyright file="IThemeManager.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------------------------

namespace Sundew.Xaml.Theming;

using System.Collections.ObjectModel;
using System.ComponentModel;

/// <summary>
/// Interface for implementing a theme manager.
/// </summary>
/// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
public interface IThemeManager : INotifyPropertyChanged
{
    /// <summary>Occurs when the theme is about to change.</summary>
    event EventHandler<ThemeUpdateEventArgs>? ThemeChanging;

    /// <summary>Occurs when the theme has changed.</summary>
    event EventHandler<ThemeUpdateEventArgs>? ThemeChanged;

    /// <summary>
    /// Gets or sets the current theme.
    /// </summary>
    /// <value>
    /// The current theme.
    /// </value>
    Theme? CurrentTheme { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the theme mode should be automatically updated when the system theme mode changes.
    /// </summary>
    bool AutoApplySystemThemeMode { get; set; }

    /// <summary>
    /// Gets the theme infos.
    /// </summary>
    /// <value>
    /// The theme infos.
    /// </value>
    ObservableCollection<Theme> Themes { get; }

    /// <summary>
    /// Gets the applied theme.
    /// </summary>
    AppliedTheme? AppliedTheme { get; }

    /// <summary>
    /// Gets the applied theme mode.
    /// </summary>
    AppliedThemeMode? AppliedThemeMode { get; }

    /// <summary>
    /// Gets or sets the current theme mode.
    /// </summary>
    ThemeMode? CurrentThemeMode { get; set; }

    /// <summary>
    /// Applies the specified theme.
    /// </summary>
    /// <param name="theme">The theme information.</param>
    /// <returns>A value indicating whether the new theme was applied.</returns>
    bool ChangeTheme(Theme theme);

    /// <summary>
    /// Applies the specified theme.
    /// </summary>
    /// <param name="theme">The theme information.</param>
    /// <param name="selectModeFunc">The select mode func.</param>
    /// <returns>A value indicating whether the new theme was applied.</returns>
    bool ChangeTheme(Theme theme, Func<Theme, ThemeMode> selectModeFunc);

    /// <summary>
    /// Applies the specified theme mode.
    /// </summary>
    /// <param name="themeMode">The theme mode.</param>
    bool ChangeThemeMode(ThemeMode themeMode);
}