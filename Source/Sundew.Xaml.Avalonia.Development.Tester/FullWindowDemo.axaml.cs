// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FullWindowDemo.axaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Avalonia.Development.Tester;

using System;
using System.Globalization;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Input;
using global::Avalonia.Markup.Xaml;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using Sundew.Xaml.Controls;

/// <summary>
/// A window with no decorations, drawing its own caption and the buttons in it, so that it can be seen doing so.
/// The backdrop can be switched while it is up, which is the only way to tell what the system will actually give.
/// </summary>
public partial class FullWindowDemo : FullWindow
{
    private readonly ComboBox? backdropSelector;
    private readonly TextBlock? backdropLabel;
    private readonly TextBlock? tintLabel;
    private readonly global::Avalonia.Controls.Border? tint;

    public FullWindowDemo()
    {
        this.InitializeComponent();

        // The commands are the window's own, so the window is what the markup binds against.
        this.DataContext = this;

        // Looked up by name rather than through the generated fields, as everything in this head is: it declares
        // InitializeComponent itself, which leaves the assigning of those fields out of the build.
        this.backdropSelector = this.FindControl<ComboBox>("BackdropSelector");
        this.backdropLabel = this.FindControl<TextBlock>("BackdropLabel");

        // Said in full because this head has a Border of its own, the one being compared in the sheet next door.
        if (this.FindControl<global::Avalonia.Controls.Border>("DragArea") is { } dragArea)
        {
            dragArea.PointerPressed += this.OnDragAreaPointerPressed;
        }

        if (this.backdropSelector != null)
        {
            this.backdropSelector.ItemsSource = Enum.GetValues<WindowBackdrop>();
            this.backdropSelector.SelectedItem = this.Backdrop;
            this.backdropSelector.SelectionChanged += this.OnBackdropSelected;
        }

        this.tintLabel = this.FindControl<TextBlock>("TintLabel");
        this.tint = this.FindControl<global::Avalonia.Controls.Border>("Tint");
        if (this.FindControl<Slider>("TintSlider") is { } tintSlider)
        {
            tintSlider.PropertyChanged += this.OnTintSliderPropertyChanged;
        }

        this.UpdateTintBrush();
        this.ActualThemeVariantChanged += (_, _) => this.UpdateTintBrush();

        // What the platform gave is only known once it has been asked, which is after the window is opened, so it
        // is read when it is reported rather than straight away.
        this.PropertyChanged += this.OnWindowPropertyChanged;
        this.ShowBackdrop();
        this.ShowTint();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Moves the window, or maximizes it, as a title bar of the system's would have.
    /// </summary>
    /// <remarks>
    /// The second click of a double click arrives as a press of its own, with the count to say so, and has to be
    /// answered before the move begins: a move started on the first press takes the pointer, and the second press
    /// never reaches here at all.
    /// </remarks>
    private void OnDragAreaPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            this.ToggleMaximized();
            return;
        }

        this.BeginMoveDrag(e);
    }

    private void OnBackdropSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (this.backdropSelector?.SelectedItem is WindowBackdrop backdrop)
        {
            this.Backdrop = backdrop;
        }
    }

    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ActualBackdropProperty)
        {
            this.ShowBackdrop();
        }
    }

    private void OnTintSliderPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == RangeBase.ValueProperty && this.tint != null && e.NewValue is double value)
        {
            this.tint.Opacity = value;
            this.ShowTint();
        }
    }

    /// <summary>
    /// Keeps the tint the colour the window would have been anyway, which is the whole point of putting it over
    /// the backdrop: white where the theme is light and black where it is dark.
    /// </summary>
    private void UpdateTintBrush()
    {
        if (this.tint != null)
        {
            this.tint.Background = this.ActualThemeVariant == ThemeVariant.Dark ? Brushes.Black : Brushes.White;
        }
    }

    private void ShowBackdrop()
    {
        if (this.backdropLabel != null)
        {
            this.backdropLabel.Text = "in effect: " + this.ActualBackdrop;
        }
    }

    private void ShowTint()
    {
        if (this.tintLabel != null && this.tint != null)
        {
            this.tintLabel.Text = this.tint.Opacity.ToString("0.00", CultureInfo.InvariantCulture);
        }
    }
}
