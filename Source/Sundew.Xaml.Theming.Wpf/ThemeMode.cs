// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThemeMode.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Theming;

using System;
using System.IO;
using System.Windows;
using SystemResourceDictionary = System.Windows.ResourceDictionary;

/// <summary>
/// The theme mode info.
/// </summary>
public class ThemeMode
{
    private readonly Func<SystemResourceDictionary> themeModeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeMode"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="themeModeFactory">The theme factory.</param>
    public ThemeMode(string name, Func<SystemResourceDictionary> themeModeFactory)
    {
        this.themeModeFactory = themeModeFactory;
        this.Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeMode"/> class.
    /// </summary>
    /// <param name="themeType">The theme type.</param>
    public ThemeMode(Type themeType)
    : this(themeType.Name, () => (SystemResourceDictionary)Activator.CreateInstance(themeType)!)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeMode"/> class.
    /// </summary>
    /// <param name="uri">The URI.</param>
    public ThemeMode(string uri)
     : this(Path.GetFileNameWithoutExtension(uri), () => (SystemResourceDictionary)Application.LoadComponent(new Uri(uri, UriKind.RelativeOrAbsolute)))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeMode"/> class.
    /// </summary>
    /// <param name="uri">The URI.</param>
    public ThemeMode(Uri uri)
    : this(Path.GetFileNameWithoutExtension(uri.OriginalString), () => (SystemResourceDictionary)Application.LoadComponent(uri))
    {
    }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the theme mode.
    /// </summary>
    /// <returns>The loaded theme mode.</returns>
    public SystemResourceDictionary CreateThemeModeResourceDictionary()
    {
        return this.themeModeFactory();
    }

    /// <summary>
    /// Returns the name of this theme.
    /// </summary>
    /// <returns>The name.</returns>
    public override string ToString()
    {
        return this.Name;
    }
}