// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppliedThemeMode.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Theming;

using System.Windows;

/// <summary>
/// Represents a theme mode and its associated resource dictionary for applying visual styles in an application.
/// </summary>
/// <param name="ThemeMode">The theme mode to be applied. Specifies the visual appearance, such as light or dark mode.</param>
/// <param name="ThemeModeResourceDictionary">The resource dictionary containing resources specific to the selected theme mode. Cannot be null.</param>
public sealed record AppliedThemeMode(ThemeMode ThemeMode, ResourceDictionary ThemeModeResourceDictionary);