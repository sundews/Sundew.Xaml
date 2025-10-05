// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Wpf.Development.Tester;

using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Sundew.Xaml.Controls.Overlays;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer dispatcherTimer;

    public MainWindow()
    {
        this.InitializeComponent();
        var overlayDemo = new SubtitleOverlay(this) { DataContext = new SubtitleDemo(), IsSizeAnimationEnabled = true, CornerRadius = new CornerRadius(64), FontSize = 64, Padding = new Thickness(24, 32, 24, 32) };
        var binding = new Binding(nameof(SubtitleDemo.CurrentText));
        overlayDemo.SetBinding(SubtitleOverlay.SubtitleProperty, binding);
        overlayDemo.Show();
        this.dispatcherTimer = new DispatcherTimer(
            TimeSpan.FromSeconds(9),
            DispatcherPriority.DataBind,
            (sender, args) => overlayDemo.IsSizeAnimationEnabled = !overlayDemo.IsSizeAnimationEnabled,
            Dispatcher.CurrentDispatcher);
        this.Closed += (s, e) => this.dispatcherTimer.Stop();
    }
}