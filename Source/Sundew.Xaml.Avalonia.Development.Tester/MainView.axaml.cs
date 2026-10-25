// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainView.axaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Avalonia.Development.Tester;

using global::Avalonia.Controls;
using global::Avalonia.Markup.Xaml;

public partial class MainView : UserControl
{
    public MainView()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
