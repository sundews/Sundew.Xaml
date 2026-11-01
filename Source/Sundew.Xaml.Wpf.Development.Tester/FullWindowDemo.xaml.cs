// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FullWindowDemo.xaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Wpf.Development.Tester;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Sundew.Xaml.Controls;

/// <summary>
/// A window with no non client area, drawing its own caption and the buttons in it, so that it can be seen doing so.
/// The backdrop can be switched while it is up, which is the only way to tell what the system will actually give.
/// </summary>
public partial class FullWindowDemo : FullWindow
{
    public FullWindowDemo()
    {
        this.InitializeComponent();
        this.BackdropSelector.ItemsSource = Enum.GetValues<WindowBackdrop>();
        this.BackdropSelector.SelectedItem = this.Backdrop;
        this.BackdropSelector.SelectionChanged += this.OnBackdropSelected;
        this.TintSlider.ValueChanged += this.OnTintChanged;
        this.ShowTint();

        // The backdrop is asked for when the window is first shown, so what came of it is only worth reading once
        // there is a window to have asked about.
        this.Loaded += (_, _) => this.ShowBackdrop();
    }

    private void OnBackdropSelected(object sender, SelectionChangedEventArgs e)
    {
        if (this.BackdropSelector.SelectedItem is WindowBackdrop backdrop)
        {
            this.Backdrop = backdrop;
            this.ShowBackdrop();
        }
    }

    private void OnTintChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        this.Tint.Opacity = e.NewValue;
        this.ShowTint();
    }

    private void ShowBackdrop()
    {
        this.BackdropLabel.Text = "in effect: " + this.ActualBackdrop;
    }

    private void ShowTint()
    {
        this.TintLabel.Text = this.Tint.Opacity.ToString("0.00", CultureInfo.InvariantCulture);
    }
}
