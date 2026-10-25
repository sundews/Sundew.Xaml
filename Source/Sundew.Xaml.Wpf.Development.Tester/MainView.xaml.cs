// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainView.xaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Wpf.Development.Tester;

using System.Windows;
using System.Windows.Controls;
using Sundew.Xaml.Development;

/// <summary>
/// The border examples, in a view of their own so that the same set can be kept in step with the Avalonia tester and
/// so that it can be rendered without bringing up the window and its overlay.
/// </summary>
public partial class MainView : UserControl
{
    public MainView()
    {
        this.InitializeComponent();
        this.RuntimeLabel.Text = RuntimeName;
        this.Loaded += this.ShowRuntime;
    }

    /// <summary>
    /// Shows how this head is running and how long it took to get here. The time is settled on the way in, the
    /// sheet being up being exactly what was being waited for.
    /// </summary>
    private void ShowRuntime(object sender, RoutedEventArgs e)
    {
        StartupClock.Loaded();
        this.RuntimeLabel.Text = RuntimeName + " · " + StartupClock.Describe();
    }

    /// <summary>
    /// Gets how this head is running. NativeAOT is the one case where the runtime cannot compile code as it goes, which
    /// is what tells it apart from a JIT run of the very same build. A browser head is neither: it is called out by
    /// name rather than reported as ahead of time, which it is not.
    /// </summary>
    private static string RuntimeName => OperatingSystem.IsBrowser()
        ? "wasm"
        : System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported ? "JIT" : "Native AOT";
}
