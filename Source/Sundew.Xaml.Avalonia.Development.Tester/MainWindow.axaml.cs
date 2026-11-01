// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.axaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Avalonia.Development.Tester;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Markup.Xaml;
using global::Avalonia.Threading;
using Sundew.Xaml.Controls.Overlays;

public partial class MainWindow : Window
{
    private SubtitleOverlay? overlay;
    private DispatcherTimer? animationTimer;
    private FullWindowDemo? fullWindow;

    public MainWindow()
    {
        this.InitializeComponent();

        // One such window at a time, brought forward again rather than stacked up, since it is the window itself
        // that is being looked at.
        if (this.Content is MainView { FullWindowOpener: { } fullWindowButton })
        {
            fullWindowButton.IsVisible = true;
            fullWindowButton.Click += (_, _) => this.ShowFullWindow();
        }

        this.Closed += (_, _) => this.fullWindow?.Close();

        // The overlay is a window of its own, always on top, so it sits over whatever else is being compared.
        // That is the point of it, and a nuisance when the border is what is being looked at, so it is off until
        // asked for and goes away again when it is not.
        if (this.Content is MainView { OverlayToggle: { } subtitleOverlayToggle })
        {
            subtitleOverlayToggle.IsVisible = true;
            subtitleOverlayToggle.IsCheckedChanged += (_, _) =>
            {
                if (subtitleOverlayToggle.IsChecked == true)
                {
                    this.ShowOverlay();
                }
                else
                {
                    this.HideOverlay();
                }
            };
        }

        this.Closed += (_, _) => this.HideOverlay();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ShowFullWindow()
    {
        if (this.fullWindow != null)
        {
            this.fullWindow.Activate();
            return;
        }

        var fullWindowDemo = new FullWindowDemo();
        this.fullWindow = fullWindowDemo;
        fullWindowDemo.Closed += (_, _) => this.fullWindow = null;
        fullWindowDemo.Show();
    }

    private void ShowOverlay()
    {
        if (this.overlay != null)
        {
            return;
        }

        var subtitleDemo = new SubtitleDemo();
        var subtitleOverlay = new SubtitleOverlay(this)
        {
            DataContext = subtitleDemo,
            IsSizeAnimationEnabled = true,
            CornerRadius = new CornerRadius(64),
            FontSize = 64,
            Padding = new Thickness(24, 32, 24, 32),
        };

        this.overlay = subtitleOverlay;

        // The subtitle is carried over by hand rather than bound by a path. This head is compiled ahead of time,
        // where Avalonia asks for compiled bindings, and those are written in XAML; the overlay is built here.
        subtitleOverlay.Subtitle = subtitleDemo.CurrentText;
        subtitleDemo.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is null or nameof(SubtitleDemo.CurrentText))
            {
                subtitleOverlay.Subtitle = subtitleDemo.CurrentText;
            }
        };

        subtitleOverlay.Show();
        this.animationTimer = new DispatcherTimer(
            TimeSpan.FromSeconds(9),
            DispatcherPriority.Normal,
            (_, _) => subtitleOverlay.IsSizeAnimationEnabled = !subtitleOverlay.IsSizeAnimationEnabled);
        this.animationTimer.Start();
    }

    private void HideOverlay()
    {
        this.animationTimer?.Stop();
        this.animationTimer = null;
        this.overlay?.Close();
        this.overlay = null;
    }
}
