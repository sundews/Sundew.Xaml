// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainView.axaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Avalonia.Development.Tester;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Markup.Xaml;
using global::Avalonia.Media;

public partial class MainView : UserControl
{
    public MainView()
    {
        this.InitializeComponent();
        this.ConnectZoom();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Scales the examples by the zoom slider.
    /// </summary>
    /// <remarks>
    /// The WPF tester binds its <c>ScaleTransform</c> straight from the slider, but a transform here is not in the
    /// name scope the binding would have to resolve through, so the two are connected in code instead.
    /// </remarks>
    private void ConnectZoom()
    {
        var zoom = this.FindControl<Slider>("ZoomSlider");
        var scaled = this.FindControl<LayoutTransformControl>("ScaledExamples");
        if (zoom == null || scaled == null)
        {
            return;
        }

        scaled.LayoutTransform = new ScaleTransform(zoom.Value, zoom.Value);
        zoom.PropertyChanged += (_, e) =>
        {
            if (e.Property == RangeBase.ValueProperty && e.NewValue is double value)
            {
                scaled.LayoutTransform = new ScaleTransform(value, value);
            }
        };
    }
}
