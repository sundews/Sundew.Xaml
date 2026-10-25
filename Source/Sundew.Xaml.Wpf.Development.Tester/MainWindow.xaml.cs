// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Wpf.Development.Tester;

using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Sundew.Xaml.Controls.Overlays;

public partial class MainWindow : Window
{
    private SubtitleOverlay? overlay;
    private DispatcherTimer? animationTimer;

    public MainWindow()
    {
        this.InitializeComponent();

        // The overlay is a window of its own, always on top, so it sits over whatever else is being compared.
        // That is the point of it, and a nuisance when the border is what is being looked at, so it is off until
        // asked for and goes away again when it is not.
        this.View.SubtitleOverlayToggle.Checked += (_, _) => this.ShowOverlay();
        this.View.SubtitleOverlayToggle.Unchecked += (_, _) => this.HideOverlay();
        this.Closed += (_, _) => this.HideOverlay();
    }

    private void ShowOverlay()
    {
        if (this.overlay != null)
        {
            return;
        }

        this.overlay = new SubtitleOverlay(this)
        {
            DataContext = new SubtitleDemo(),
            IsSizeAnimationEnabled = true,
            CornerRadius = new CornerRadius(64),
            FontSize = 64,
            Padding = new Thickness(24, 32, 24, 32),
        };

        this.overlay.SetBinding(SubtitleOverlay.SubtitleProperty, new Binding(nameof(SubtitleDemo.CurrentText)));
        this.overlay.Show();
        this.animationTimer = new DispatcherTimer(
            TimeSpan.FromSeconds(9),
            DispatcherPriority.DataBind,
            (_, _) => this.overlay.IsSizeAnimationEnabled = !this.overlay.IsSizeAnimationEnabled,
            Dispatcher.CurrentDispatcher);
    }

    private void HideOverlay()
    {
        this.animationTimer?.Stop();
        this.animationTimer = null;
        this.overlay?.Close();
        this.overlay = null;
    }
}
