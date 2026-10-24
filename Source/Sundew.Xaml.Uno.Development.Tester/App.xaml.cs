// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Uno.Development.Tester;

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using Sundew.Xaml.Controls.Backends;
using Windows.Foundation;
using SundewBorder = Sundew.Xaml.Controls.Border;

public partial class App : Application
{
    private const string RenderOption = "--render";

    /// <summary>
    /// The device pixel scale a captured sheet is rounded to, stated rather than taken from the display,
    /// so that the sheet is the same whatever the display it was captured on.
    /// </summary>
    private const double RenderScale = 1.0;

    private Window? window;

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var commandLine = Environment.GetCommandLineArgs();
        var renderIndex = Array.IndexOf(commandLine, RenderOption);
        if (renderIndex >= 0 && renderIndex + 1 < commandLine.Length)
        {
            this.RenderComparisonSheet(commandLine[renderIndex + 1]);
            return;
        }

        this.window = new Window { Content = new MainView() };
        this.window.Activate();
    }

    /// <summary>
    /// Walks the sheet and draws every border onto one Skia surface.
    /// </summary>
    /// <remarks>
    /// The same approach the WinUI tester takes, and for the same reason: a border draws onto a canvas the compositor
    /// owns, so the way to read the pixels back is to ask each border to draw itself onto a surface of our own. It is
    /// the very same drawing code the Skia canvas element runs.
    /// </remarks>
    /// <param name="path">The file to write.</param>
    private void RenderComparisonSheet(string path)
    {
        var view = new ComparisonView();
        var host = new Window { Content = view };

        // Off screen, and at the same place the WinUI tester puts its own, so that both land on the same display and
        // so report the same rasterization scale. The scale decides what the geometry is rounded to, so comparing
        // sheets taken at different scales would be comparing two different, and both correct, roundings.
        host.AppWindow.Move(new Windows.Graphics.PointInt32 { X = -4000, Y = -4000 });
        host.Activate();

        view.Loaded += (_, _) => view.DispatcherQueue.TryEnqueue(
            Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
            () =>
            {
                try
                {
                    // Laid out at exactly the size the sheet declares, so that neither the window nor the
                    // rounding of the display it opened on can shift where the borders sit.
                    view.Measure(new Size(view.Width, view.Height));
                    view.Arrange(new Rect(0, 0, view.Width, view.Height));
                    view.UpdateLayout();
                    var origins = Render(view, path);
                    File.WriteAllText(
                        path + ".info.txt",
                        $"rasterizationScale={view.XamlRoot?.RasterizationScale}{Environment.NewLine}size={view.ActualWidth}x{view.ActualHeight}{Environment.NewLine}{origins}");
                }
                catch (Exception exception)
                {
                    File.WriteAllText(path + ".error.txt", exception.ToString());
                }

                this.Exit();
            });
    }

    /// <summary>
    /// Draws every border on the sheet onto one surface and writes it out.
    /// </summary>
    /// <param name="view">The sheet.</param>
    /// <param name="path">The file to write.</param>
    /// <returns>Where each border was found, which says whether two sheets are comparable at all.</returns>
    private static string Render(FrameworkElement view, string path)
    {
        var origins = new System.Text.StringBuilder();
        var width = (int)Math.Ceiling(view.ActualWidth);
        var height = (int)Math.Ceiling(view.ActualHeight);
        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(new SKColor(0x80, 0x80, 0x80));

        foreach (var child in Descendants(view))
        {
            if (child is not SundewBorder border)
            {
                continue;
            }

            var origin = border.TransformToVisual(view).TransformPoint(default);
            origins.AppendLine(System.FormattableString.Invariant($"{origin.X:0.###},{origin.Y:0.###} {border.ActualWidth:0.###}x{border.ActualHeight:0.###}"));
            var saved = canvas.Save();
            canvas.Translate((float)origin.X, (float)origin.Y);
            using (var session = new SkiaDrawingSession(canvas))
            {
                border.RenderTo(session, new Size(border.ActualWidth, border.ActualHeight), RenderScale);
            }

            canvas.RestoreToCount(saved);
        }

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(path);
        data.SaveTo(stream);
        return origins.ToString();
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            yield return child;
            foreach (var descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }
}
