// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowsThemeModeDetector.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Theming.Internal;

using System;
using Microsoft.Win32;

/// <summary>
/// Provides functionality to detect the current Windows theme mode preference for applications.
/// </summary>
/// <remarks>This class reads the user's theme preference from the Windows registry to determine whether the system is configured to use a light or dark theme for applications.
/// It is intended for use on Windows platforms that support theme personalization. The class does not support other operating systems.</remarks>
internal class WindowsThemeModeDetector
{
    private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string AppUseLightTheme = "AppsUseLightTheme";
    private const string SystemUsesLightTheme = "SystemUsesLightTheme";

    public static ThemeModeVariant GetCurrentThemeMode()
    {
        try
        {
            using var personalizeKey = Registry.CurrentUser.OpenSubKey(PersonalizeKey);
            if (personalizeKey == null)
            {
                return ThemeModeVariant.Light;
            }

            // Check app theme setting
            var appsValue = personalizeKey.GetValue(AppUseLightTheme);
            if (appsValue is int themeValue)
            {
                return themeValue == 0 ? ThemeModeVariant.Dark : ThemeModeVariant.Light;
            }

            // Fallback to system theme
            var systemValue = personalizeKey.GetValue(SystemUsesLightTheme);
            if (systemValue is int systemThemeValue)
            {
                return systemThemeValue == 0 ? ThemeModeVariant.Dark : ThemeModeVariant.Light;
            }
        }
        catch (Exception)
        {
        }

        return ThemeModeVariant.Light;
    }
}