// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TesterCatalog.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Launcher;

/// <summary>
/// Every tester the launcher knows how to start.
/// </summary>
public static class TesterCatalog
{
    /// <summary>
    /// Gets the testers, in the order they are listed.
    /// </summary>
    /// <returns>The testers.</returns>
    public static IReadOnlyList<Tester> All { get; } = new Tester[]
    {
        new Tester("WPF", TesterKind.Windows, "Sundew.Xaml.Wpf.Development.Tester", "net10.0-windows", ProcessName: "Sundew.Xaml.Wpf.Development.Tester"),
        new Tester("WinUI", TesterKind.Windows, "Sundew.Xaml.WinUI.Development.Tester", "net10.0-windows10.0.19041.0", ProcessName: "Sundew.Xaml.WinUI.Development.Tester", ExtraArguments: "-p:Platform=x64"),
        new Tester("Uno desktop", TesterKind.Windows, "Sundew.Xaml.Uno.Development.Tester", "net10.0-desktop", ProcessName: "Sundew.Xaml.Uno.Development.Tester", SupportsNativeAot: true),
        new Tester("Avalonia desktop", TesterKind.Windows, "Sundew.Xaml.Avalonia.Development.Tester", "net10.0", ProcessName: "Sundew.Xaml.Avalonia.Development.Tester", SupportsNativeAot: true),
        new Tester("MAUI Windows", TesterKind.Windows, "Sundew.Xaml.Maui.Development.Tester", "net10.0-windows10.0.19041.0", ProcessName: "Sundew.Xaml.Maui.Development.Tester"),

        new Tester("Uno Android", TesterKind.Android, "Sundew.Xaml.Uno.Development.Tester", "net10.0-android", "dev.sundew.xaml.unotester", ExtraArguments: "-p:SundewUnoAndroidOnly=true", SupportsNativeAot: true),
        new Tester("Avalonia Android", TesterKind.Android, "Sundew.Xaml.Avalonia.Development.Tester", "net10.0-android", "dev.sundew.xaml.avaloniatester", ExtraArguments: "-p:SundewAvaloniaAndroidOnly=true", SupportsNativeAot: true),

        // MAUI stays on the fast deployment. Publishing it ahead of time fails inside the Android SDK, whose
        // FindJavaObjectsStep writes a sidecar next to each assembly it reads and so tries to write into the
        // runtime pack under Program Files, which is not writable.
        new Tester("MAUI Android", TesterKind.Android, "Sundew.Xaml.Maui.Development.Tester", "net10.0-android", "dev.sundew.xaml.mauitester"),

        new Tester("Uno browser", TesterKind.Browser, "Sundew.Xaml.Uno.Development.Tester", "net10.0-browserwasm"),
        new Tester("Avalonia browser", TesterKind.Browser, "Sundew.Xaml.Avalonia.Development.Tester", "net10.0-browser"),
    };
}
