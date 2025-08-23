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
    public static readonly RoutedEvent ThemeRefreshEvent = EventManager.RegisterRoutedEvent("ThemeRefresh", RoutingStrategy.Direct, typeof(EventHandler), typeof(ThemeManager));

    private readonly bool respondToSystemTheme;
    private ThemePair? currentThemePair;
    private ThemeModePair? currentThemeModePair;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeManager" /> class.
    /// </summary>
    /// <param name="themes">The theme infos.</param>
    public ThemeManager(ObservableCollection<Theme> themes, bool respondToSystemTheme)
    {
        this.Themes = themes;
        this.respondToSystemTheme = respondToSystemTheme;
        if (respondToSystemTheme)
        {
            SystemEvents.UserPreferenceChanged += this.OnSystemEventsUserPreferenceChanged;
        }
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Occurs when the theme has changed.</summary>
    public event EventHandler<ThemeUpdatedEventArgs>? ThemeChanged;

    /// <summary>
    /// Gets the theme infos.
    /// </summary>
    /// <value>
    /// The theme infos.
    /// </value>
    public ObservableCollection<Theme> Themes { get; }

    /// <summary>
    /// Gets or sets the current theme.
    /// </summary>
    /// <value>
    /// The current theme.
    /// </value>
    public Theme? CurrentTheme
    {
        get => this.currentThemePair?.Theme;
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
        get => this.currentThemeModePair?.ThemeMode;
        set
        {
            if (value != null)
            {
                this.ChangeThemeMode(value);
            }
        }
    }

    /// <summary>
    /// Applies the specified theme.
    /// </summary>
    /// <param name="theme">The theme information.</param>
    public void ChangeTheme(Theme theme)
    {
        if (theme == this.currentThemePair?.Theme)
        {
            return;
        }

        var oldThemePair = this.currentThemePair;
        if (this.respondToSystemTheme)
        {
            this.UpdateThemeMode(theme.ThemeModes, false);
        }

        var themeResources = theme.CreateTheme();
        this.currentThemePair = new ThemePair(theme, themeResources);
        Application.Current.Resources.MergedDictionaries.Add(themeResources);
        if (oldThemePair != null)
        {
            Application.Current.Resources.MergedDictionaries.Remove(oldThemePair.ThemeResourceDictionary);
        }

        this.NotifyPropertyChanged(nameof(this.CurrentTheme));
        this.ThemeChanged?.Invoke(this, ThemeUpdatedEventArgs.ThemeChanged(oldThemePair?.Theme, theme));
        this.RaiseThemeRefresh();
    }

    /// <summary>
    /// Applies the specified theme mode.
    /// </summary>
    /// <param name="themeMode">The theme mode.</param>
    public void ChangeThemeMode(ThemeMode themeMode)
    {
        ChangeThemeMode(themeMode, true);
    }

    /// <summary>
    /// Notifies that the specified property has changed.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

    private void OnSystemEventsUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General ||
            e.Category == UserPreferenceCategory.VisualStyle)
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                this.UpdateThemeMode(this.currentThemePair?.Theme.ThemeModes ?? [], true)));
        }
    }

    private void UpdateThemeMode(IReadOnlyCollection<ThemeMode> themeModes, bool raiseThemeChange)
    {
        var newThemeMode = WindowsThemeDetector.GetCurrentTheme();
        var newThemeModeInfo = GetMatchingThemeMode(themeModes, newThemeMode);
        if (newThemeModeInfo == null)
        {
            return;
        }

        this.ChangeThemeMode(newThemeModeInfo, raiseThemeChange);
    }

    private void ChangeThemeMode(ThemeMode themeMode, bool raiseThemeChange)
    {
        if (themeMode == this.currentThemeModePair?.ThemeMode)
        {
            return;
        }

        var oldThemeModePair = this.currentThemeModePair;
        var themeModeDictionary = themeMode.CreateThemeModeResourceDictionary();
        this.currentThemeModePair = new ThemeModePair(themeMode, themeModeDictionary);
        Application.Current.Resources.MergedDictionaries.Add(themeModeDictionary);
        if (oldThemeModePair != null)
        {
            Application.Current.Resources.MergedDictionaries.Remove(oldThemeModePair.ThemeModeResourceDictionary);
        }

        this.NotifyPropertyChanged(nameof(this.CurrentThemeMode));
        if (raiseThemeChange)
        {
            this.ThemeChanged?.Invoke(this, ThemeUpdatedEventArgs.ThemeModeChanged(oldThemeModePair?.ThemeMode, themeMode));
            this.RaiseThemeRefresh();
        }
    }

    private void RaiseThemeRefresh()
    {
        foreach (Window window in Application.Current.Windows)
        {
            RefreshTheme(window);
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

    private sealed record ThemePair(Theme Theme, ResourceDictionary ThemeResourceDictionary);

    private sealed record ThemeModePair(ThemeMode ThemeMode, ResourceDictionary ThemeModeResourceDictionary);
}