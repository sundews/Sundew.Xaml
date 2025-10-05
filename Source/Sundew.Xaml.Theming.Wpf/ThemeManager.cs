// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThemeManager.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Theming;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using Sundew.Xaml.Theming.Internal;
using Application = System.Windows.Application;

/// <summary>
/// Keeps track of the current applied theme and supports changing theme at runtime.
/// </summary>
/// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
public sealed class ThemeManager : IThemeManager
{
    /// <summary>
    /// The theme refresh event field.
    /// </summary>
    public static readonly RoutedEvent ThemeRefreshEvent = EventManager.RegisterRoutedEvent("ThemeRefresh", RoutingStrategy.Direct, typeof(EventHandler), typeof(ThemeManager));

    private static ThemeManager? currentThemeManager;
    private AppliedTheme? appliedTheme;
    private AppliedThemeMode? appliedThemeMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeManager" /> class.
    /// </summary>
    /// <param name="themes">The theme infos.</param>
    /// <param name="autoApplySystemThemeMode">A value indicating whether the system theme should be applied.</param>
    public ThemeManager(ObservableCollection<Theme> themes, bool autoApplySystemThemeMode)
    {
        if (currentThemeManager != null)
        {
            throw new InvalidOperationException("Only one instance of ThemeManager is allowed.");
        }

        currentThemeManager = this;
        this.Themes = themes;
        this.AutoApplySystemThemeMode = autoApplySystemThemeMode;
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Occurs when the theme is about to change.</summary>
    public event EventHandler<ThemeUpdateEventArgs>? ThemeChanging;

    /// <summary>Occurs when the theme has changed.</summary>
    public event EventHandler<ThemeUpdateEventArgs>? ThemeChanged;

    /// <summary>
    /// Gets the current theme manager.
    /// </summary>
    public static ThemeManager? Current => currentThemeManager;

    /// <summary>
    /// Gets or sets a value indicating whether the system theme mode is automatically applied in response to operating
    /// system changes.
    /// </summary>
    public bool AutoApplySystemThemeMode
    {
        get => field;
        set
        {
            field = value;
            if (field)
            {
                SystemEvents.UserPreferenceChanged -= this.OnSystemEventsUserPreferenceChanged;
                SystemEvents.UserPreferenceChanged += this.OnSystemEventsUserPreferenceChanged;

                this.UpdateThemeMode(this.AppliedTheme?.Theme.ThemeModes ?? []);
            }
            else
            {
                SystemEvents.UserPreferenceChanged -= this.OnSystemEventsUserPreferenceChanged;
            }
        }
    }

    /// <summary>
    /// Gets the theme infos.
    /// </summary>
    /// <value>
    /// The theme infos.
    /// </value>
    public ObservableCollection<Theme> Themes { get; }

    /// <summary>
    /// Gets the applied theme.
    /// </summary>
    public AppliedTheme? AppliedTheme => this.appliedTheme;

    /// <summary>
    /// Gets the applied theme mode.
    /// </summary>
    public AppliedThemeMode? AppliedThemeMode => this.appliedThemeMode;

    /// <summary>
    /// Gets or sets the current theme.
    /// </summary>
    /// <value>
    /// The current theme.
    /// </value>
    public Theme? CurrentTheme
    {
        get => this.appliedTheme?.Theme;
        set
        {
            if (value != null)
            {
                this.ChangeTheme(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the current theme mode.
    /// </summary>
    public ThemeMode? CurrentThemeMode
    {
        get => this.appliedThemeMode?.ThemeMode;
        set
        {
            if (value != null)
            {
                this.ChangeThemeMode(value);
            }
        }
    }

    /// <summary>
    /// Adds the theme refresh handler.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="handler">The handler.</param>
    public static void AddThemeRefreshHandler(UIElement element, EventHandler handler)
    {
        element.AddHandler(ThemeRefreshEvent, handler);
    }

    /// <summary>
    /// Removes the theme refresh handler.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="handler">The handler.</param>
    public static void RemoveThemeRefreshHandler(UIElement element, EventHandler handler)
    {
        element.RemoveHandler(ThemeRefreshEvent, handler);
    }

    /// <summary>
    /// Applies the specified theme.
    /// </summary>
    /// <param name="theme">The theme information.</param>
    /// <returns>A value indicating whether the new theme was applied.</returns>
    public bool ChangeTheme(Theme theme)
    {
        if (theme == this.appliedTheme?.Theme)
        {
            return false;
        }

        if (this.AutoApplySystemThemeMode)
        {
            var matchingThemeMode = this.GetThemeModeForSystem(theme.ThemeModes);
            if (matchingThemeMode != null)
            {
                return this.ApplyTheme(theme, matchingThemeMode);
            }
        }

        var themeMode = theme.ThemeModes.FirstOrDefault();
        if (themeMode != null)
        {
            return this.ApplyTheme(theme, themeMode);
        }

        return false;
    }

    /// <summary>
    /// Applies the specified theme.
    /// </summary>
    /// <param name="theme">The theme information.</param>
    /// <param name="selectModeFunc">The select mode func.</param>
    /// <returns>A value indicating whether the new theme was applied.</returns>
    public bool ChangeTheme(Theme theme, Func<Theme, ThemeMode> selectModeFunc)
    {
        if (theme == this.appliedTheme?.Theme)
        {
            return false;
        }

        this.AutoApplySystemThemeMode = false;
        var themeMode = selectModeFunc(theme);
        return this.ApplyTheme(theme, themeMode);
    }

    /// <summary>
    /// Applies the specified theme mode.
    /// </summary>
    /// <param name="themeMode">The theme mode.</param>
    /// <returns><c>true</c>, if the theme mode was applied, otherwise <c>false</c>.</returns>
    public bool ChangeThemeMode(ThemeMode themeMode)
    {
        this.AutoApplySystemThemeMode = false;
        return this.ApplyTheme(this.appliedTheme?.Theme, themeMode);
    }

    private bool ApplyTheme(Theme? theme, ThemeMode themeMode)
    {
        var oldThemePair = this.appliedTheme;
        var oldThemeModePair = this.appliedThemeMode;
        ThemeChangeType? changeType = (oldThemePair?.Theme != theme, oldThemeModePair?.ThemeMode != themeMode) switch
        {
            (true, true) => ThemeChangeType.Theme,
            (true, false) => ThemeChangeType.Theme,
            (false, true) => ThemeChangeType.ThemeMode,
            _ => null,
        };

        if (theme == null || !changeType.HasValue)
        {
            return false;
        }

        var themeUpdateEventArgs = new ThemeUpdateEventArgs(oldThemePair?.Theme, theme, oldThemeModePair?.ThemeMode, themeMode, changeType.Value);
        this.ThemeChanging?.Invoke(this, themeUpdateEventArgs);

        var themeModeDictionary = themeMode.CreateThemeModeResourceDictionary();
        this.appliedThemeMode = new AppliedThemeMode(themeMode, themeModeDictionary);
        Application.Current.Resources.MergedDictionaries.Add(themeModeDictionary);
        if (oldThemeModePair != null)
        {
            Application.Current.Resources.MergedDictionaries.Remove(oldThemeModePair.ThemeModeResourceDictionary);
        }

        var themeResources = theme.CreateTheme();
        this.appliedTheme = new AppliedTheme(theme, themeResources);
        Application.Current.Resources.MergedDictionaries.Add(themeResources);
        if (oldThemePair != null)
        {
            Application.Current.Resources.MergedDictionaries.Remove(oldThemePair.ThemeResourceDictionary);
        }

        this.NotifyPropertyChanged(nameof(this.CurrentThemeMode));
        this.NotifyPropertyChanged(nameof(this.AppliedThemeMode));
        if (changeType == ThemeChangeType.Theme)
        {
            this.NotifyPropertyChanged(nameof(this.CurrentTheme));
            this.NotifyPropertyChanged(nameof(this.AppliedTheme));
        }

        this.ThemeChanged?.Invoke(this, themeUpdateEventArgs);
        this.RaiseThemeRefresh();
        return true;
    }

    /// <summary>
    /// Notifies that the specified property has changed.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnSystemEventsUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General ||
            e.Category == UserPreferenceCategory.VisualStyle)
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                this.UpdateThemeMode(this.appliedTheme?.Theme.ThemeModes ?? [])));
        }
    }

    private bool UpdateThemeMode(IReadOnlyCollection<ThemeMode> themeModes)
    {
        var newThemeMode = this.GetThemeModeForSystem(themeModes);
        if (newThemeMode == null)
        {
            return false;
        }

        return this.ApplyTheme(this.appliedTheme?.Theme, newThemeMode);
    }

    private ThemeMode? GetThemeModeForSystem(IReadOnlyCollection<ThemeMode> themeModes)
    {
        var newThemeMode = WindowsThemeModeDetector.GetCurrentThemeMode();
        var newThemeModeInfo = this.GetMatchingThemeMode(themeModes, newThemeMode);
        return newThemeModeInfo;
    }

    private void RaiseThemeRefresh()
    {
        foreach (Window window in Application.Current.Windows)
        {
            this.RefreshTheme(window);
        }
    }

    private void RefreshTheme(UIElement uiElement)
    {
        var args = new RoutedEventArgs(ThemeRefreshEvent, uiElement);
        uiElement.RaiseEvent(args);
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(uiElement); i++)
        {
            var child = VisualTreeHelper.GetChild(uiElement, i);
            if (child is UIElement childElement)
            {
                this.RefreshTheme(childElement);
            }
        }
    }

    private ThemeMode? GetMatchingThemeMode(IReadOnlyCollection<ThemeMode> themeModes, ThemeModeVariant themeModeVariant)
    {
        var themeModeInfo = themeModes.FirstOrDefault(x => x.Name == themeModeVariant.ToString());
        if (themeModeInfo != null)
        {
            return themeModeInfo;
        }

        return themeModes.FirstOrDefault();
    }
}