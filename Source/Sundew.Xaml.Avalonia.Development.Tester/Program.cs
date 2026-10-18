// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Avalonia.Development.Tester;

using System;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Media.Imaging;
using global::Avalonia.Platform;

public static class Program
{
    private const string RenderOption = "--render";

    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Length == 2 && args[0] == RenderOption)
        {
            return Render(args[1]);
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        return 0;
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }

    /// <summary>
    /// Renders the test view to a file, so that the result can be inspected without opening a window.
    /// </summary>
    /// <param name="path">The file to write.</param>
    /// <returns>Zero when the file was written.</returns>
    private static int Render(string path)
    {
        BuildAvaloniaApp().SetupWithoutStarting();

        var size = new Size(900, 800);
        var view = new MainView
        {
            Width = size.Width,
            Height = size.Height,
        };

        view.Measure(size);
        view.Arrange(new Rect(size));
        Console.WriteLine($"view bounds {view.Bounds}");

        using var bitmap = new RenderTargetBitmap(
            new PixelSize((int)size.Width, (int)size.Height),
            new Vector(96, 96));
        bitmap.Render(view);
        bitmap.Save(path, PngBitmapEncoderOptions.Default);
        Console.WriteLine($"wrote {path}");
        return 0;
    }
}
