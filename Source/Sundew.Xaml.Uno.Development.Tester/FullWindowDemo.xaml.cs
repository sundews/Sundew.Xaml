// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FullWindowDemo.xaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Uno.Development.Tester;

using System;
using System.Globalization;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Sundew.Xaml.Controls;

/// <summary>
/// A window with no title bar, drawing its own caption and the buttons in it, so that it can be seen doing so on
/// whichever head this is running. The backdrop can be switched while it is up, and tinted over, which is the only
/// way to tell what the system will actually give and how much of it the content lets through.
/// </summary>
public sealed partial class FullWindowDemo : FullWindow
{
    public FullWindowDemo()
    {
        this.InitializeComponent();

        // The size is left to the head. A window of this framework is sized through the window manager rather than
        // in markup, and asking for the size the other testers use comes out smaller than the content here: this
        // head measures the window in units of its own, which are not the ones it lays the content out in.
        this.SetCaption(this.DragArea);
        this.BackdropSelector.ItemsSource = Enum.GetValues<WindowBackdrop>();
        this.BackdropSelector.SelectedItem = this.Backdrop;
        this.BackdropSelector.SelectionChanged += this.OnBackdropSelected;
        this.TintSlider.ValueChanged += this.OnTintChanged;
        this.ShowBackdrop();
        this.ShowTint();
    }

    private void OnBackdropSelected(object sender, SelectionChangedEventArgs e)
    {
        if (this.BackdropSelector.SelectedItem is WindowBackdrop backdrop)
        {
            this.Backdrop = backdrop;
            this.ShowBackdrop();
        }
    }

    private void OnTintChanged(object sender, RangeBaseValueChangedEventArgs e)
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
