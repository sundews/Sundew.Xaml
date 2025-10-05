// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThemeResourceDictionary.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Theming;

/// <summary>
/// A ResourceDictionary that ensures that a source is only loaded once and otherwise retrieved from a cache.
/// </summary>
public sealed class ThemeResourceDictionary : ResourceDictionaryBase<ThemeResourceDictionary>
{
    static ThemeResourceDictionary()
    {
        var themeManager = ThemeManager.Current;
        if (themeManager != null)
        {
            themeManager.ThemeChanging += (s, e) =>
            {
                ResourceDictionaries.Clear();
            };
        }
    }
}