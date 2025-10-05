// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppliedTheme.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Theming;

using System.Windows;

/// <summary>
/// Represents a theme that has been applied, including its associated resources.
/// </summary>
/// <param name="Theme">The theme that is currently applied.</param>
/// <param name="ThemeResourceDictionary">The resource dictionary containing resources for the applied theme.</param>
public sealed record AppliedTheme(Theme Theme, ResourceDictionary ThemeResourceDictionary);