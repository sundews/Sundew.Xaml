// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.WinUI.Development.Tester;

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Sundew.Xaml.Controls.Overlays;

public sealed partial class MainWindow : Window
{
    private SubtitleOverlay? overlay;
    private SubtitleDemo? subtitleDemo;
    private DispatcherTimer? animationTimer;

    public MainWindow()
    {
        this.InitializeComponent();
        this.Title = "Sundew.Xaml WinUI Tester";

        // The overlay is a window of its own, always on top, so it sits over whatever else is being compared.
        // That is the point of it, and a nuisance when the border is what is being looked at, so it is off until
        // asked for and goes away again when it is not.
        if (this.Content is MainView mainView)
        {
            mainView.OverlayToggle.Checked += (_, _) => this.ShowOverlay();
            mainView.OverlayToggle.Unchecked += (_, _) => this.HideOverlay();
        }

        this.Closed += (_, _) => this.HideOverlay();
    }

    private void ShowOverlay()
    {
        if (this.overlay != null)
        {
            return;
        }

        var demo = new SubtitleDemo();
        var subtitleOverlay = new SubtitleOverlay(this)
        {
            IsSizeAnimationEnabled = true,
            CornerRadius = new CornerRadius(64),
            FontSize = 64,
            Padding = new Thickness(24, 32, 24, 32),
        };

        this.overlay = subtitleOverlay;
        this.subtitleDemo = demo;

        // Bound on the overlay itself, as in WPF. It is a dependency object but not a framework element, having no
        // place in a visual tree of its own, so a binding names what it reads rather than inheriting a data
        // context. Setting one on the overlay is still worth doing where the banner is bound against it instead.
        BindingOperations.SetBinding(
            subtitleOverlay,
            SubtitleOverlay.SubtitleProperty,
            new Binding { Source = demo, Path = new PropertyPath(nameof(SubtitleDemo.CurrentText)) });
        subtitleOverlay.Activate();

        this.animationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(9) };
        this.animationTimer.Tick += (_, _) => subtitleOverlay.IsSizeAnimationEnabled = !subtitleOverlay.IsSizeAnimationEnabled;
        this.animationTimer.Start();
    }

    private void HideOverlay()
    {
        this.animationTimer?.Stop();
        this.animationTimer = null;
        this.subtitleDemo?.Stop();
        this.subtitleDemo = null;
        this.overlay?.Close();
        this.overlay = null;
    }
}
