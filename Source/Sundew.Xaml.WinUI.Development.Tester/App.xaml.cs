// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.WinUI.Development.Tester;

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
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

    /// <summary>
    /// Renders from where a window would normally open rather than off screen.
    /// </summary>
    /// <remarks>
    /// The sheet is rounded to the pixel grid of the display it is on, so two sheets are only comparable when they
    /// were taken at the same rasterization scale. Off screen this lands on the primary display; this option puts it
    /// where the Uno desktop head puts its own, which always follows the display it opens on.
    /// </remarks>
    private const string DefaultPositionOption = "--default-position";

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
            this.RenderComparisonSheet(
                commandLine[renderIndex + 1],
                Array.IndexOf(commandLine, DefaultPositionOption) < 0);
            return;
        }

        this.window = new MainWindow();
        this.window.Activate();
    }

    /// <summary>
    /// Walks the sheet and draws every border onto one Win2D surface.
    /// </summary>
    /// <remarks>
    /// A border draws onto a canvas that the compositor owns, and neither a screen grab nor WinUI's own
    /// RenderTargetBitmap can read that back. Asking each border to draw itself onto a surface of our own is the way
    /// to get the same pixels out, and it is the very same drawing code the canvas runs.
    /// </remarks>
    /// <param name="path">The file to write.</param>
    /// <param name="offScreen">Whether to render off screen.</param>
    private void RenderComparisonSheet(string path, bool offScreen)
    {
        // The sheet has to be in a live window to be laid out at all, so it is shown off screen and captured once
        // layout has settled.
        var view = new ComparisonView();
        var host = new Window { Content = view };
        if (offScreen)
        {
            host.AppWindow.Move(new Windows.Graphics.PointInt32(-4000, -4000));
        }

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
                    File.WriteAllText(path + ".info.txt", $"rasterizationScale={view.XamlRoot?.RasterizationScale}{Environment.NewLine}size={view.ActualWidth}x{view.ActualHeight}{Environment.NewLine}{origins}");
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine(exception);
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
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), width, height, 96);
        using (var session = target.CreateDrawingSession())
        {
            session.Clear(Colors.Gray);
            foreach (var border in Descendants(view))
            {
                if (border is not SundewBorder sundewBorder)
                {
                    continue;
                }

                var origin = sundewBorder.TransformToVisual(view).TransformPoint(default);
                origins.AppendLine(System.FormattableString.Invariant($"{origin.X:0.###},{origin.Y:0.###} {sundewBorder.ActualWidth:0.###}x{sundewBorder.ActualHeight:0.###}"));
                session.Transform = Matrix3x2.CreateTranslation((float)origin.X, (float)origin.Y);
                using var borderSession = new Win2DDrawingSession(session);
                sundewBorder.RenderTo(borderSession, new Size(sundewBorder.ActualWidth, sundewBorder.ActualHeight), RenderScale);
            }

            session.Transform = Matrix3x2.Identity;
        }

        target.SaveAsync(path, CanvasBitmapFileFormat.Png).AsTask().GetAwaiter().GetResult();
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
