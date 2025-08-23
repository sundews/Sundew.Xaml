// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThemeInfo.cs" company="Sundews">
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
/// Contains a theme and it's name.
/// </summary>
public sealed class Theme
{
    private readonly Func<SystemResourceDictionary> themeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Theme"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="themeFactory">The theme factory.</param>
    /// <param name="themeModes">The theme modes.</param>
    public Theme(string name, Func<SystemResourceDictionary> themeFactory, IReadOnlyCollection<ThemeMode> themeModes)
    {
        this.Name = name;
        this.themeFactory = themeFactory;
        this.ThemeModes = themeModes;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Theme"/> class.
    /// </summary>
    /// <param name="themeType">Type of the theme.</param>
    /// <param name="themeModes">The theme modes.</param>
    public Theme(Type themeType, ThemeMode[] themeModes)
    : this(themeType.Name, () => (SystemResourceDictionary)Activator.CreateInstance(themeType)!, themeModes)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Theme"/> class.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <param name="themeModes">The theme modes.</param>
    public Theme(string uri, ThemeMode[] themeModes)
        : this(Path.GetFileNameWithoutExtension(uri), () => (SystemResourceDictionary)Application.LoadComponent(new Uri(uri, UriKind.RelativeOrAbsolute)), themeModes)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Theme"/> class.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <param name="themeModes">The theme modes.</param>
    public Theme(Uri uri, ThemeMode[] themeModes)
        : this(Path.GetFileNameWithoutExtension(uri.OriginalString), () => (SystemResourceDictionary)Application.LoadComponent(uri), themeModes)
    {
    }

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; }

    /// <summary>
    /// Gets the theme modes.
    /// </summary>
    public IReadOnlyCollection<ThemeMode> ThemeModes { get; }

    /// <summary>
    /// Gets the theme.
    /// </summary>
    /// <value>
    /// The theme.
    /// </value>
    public SystemResourceDictionary CreateTheme() => this.themeFactory();

    /// <summary>
    /// Gets a <see cref="Theme"/> from the specified type.
    /// </summary>
    /// <typeparam name="TTheme">The type of the theme.</typeparam>
    /// <param name="themeModes">The theme modes.</param>
    /// <returns>A new theme info.</returns>
    public static Theme FromType<TTheme>(ThemeMode[] themeModes)
        where TTheme : SystemResourceDictionary
    {
        return new Theme(typeof(TTheme), themeModes);
    }

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return this.Name;
    }
}