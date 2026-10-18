// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainView.xaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Wpf.Development.Tester;

using System.Windows.Controls;

/// <summary>
/// The border examples, in a view of their own so that the same set can be kept in step with the Avalonia tester and
/// so that it can be rendered without bringing up the window and its overlay.
/// </summary>
public partial class MainView : UserControl
{
    public MainView()
    {
        this.InitializeComponent();
    }
}
