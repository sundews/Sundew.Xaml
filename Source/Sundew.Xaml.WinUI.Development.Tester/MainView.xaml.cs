// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainView.xaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.WinUI.Development.Tester;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Sundew.Xaml.Development;

public partial class MainView : UserControl
{
    public MainView()
    {
        this.InitializeComponent();
        this.RuntimeLabel.Text = RuntimeName;
        this.Loaded += this.ShowRuntime;
    }

    /// <summary>
    /// Gets the toggle that asks for the subtitle overlay, which the window that can host one answers.
    /// </summary>
    internal CheckBox OverlayToggle => this.SubtitleOverlayToggle;

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
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage(
        "SingleFile",
        "IL3000:Avoid accessing Assembly file path when publishing as a single file",
        Justification = "The empty location is what is being asked about, so the warning is the answer.")]
    private static string RuntimeName
    {
        get
        {
            if (OperatingSystem.IsBrowser())
            {
                return "wasm";
            }

            return typeof(object).Assembly.Location.Length == 0 ? "Native AOT" : "JIT";
        }
    }
}
