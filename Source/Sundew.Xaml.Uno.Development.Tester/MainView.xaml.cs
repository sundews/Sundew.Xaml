// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainView.xaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Uno.Development.Tester;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Sundew.Xaml.Development;

public partial class MainView : UserControl
{
    private FullWindowDemo? fullWindow;

    public MainView()
    {
        this.InitializeComponent();
        this.RuntimeLabel.Text = RuntimeName;
        this.Loaded += this.ShowRuntime;

        // Wired here rather than in a window of the tester's own, this head having none: the app hands the view
        // straight to a window of the framework's. A head with no second window to open keeps the button away.
        this.FullWindowButton.Visibility = HasWindows ? Visibility.Visible : Visibility.Collapsed;
        this.FullWindowButton.Click += (_, _) => this.ShowFullWindow();
    }

    /// <summary>
    /// Gets a value indicating whether this head hands out windows at all. A phone, a tablet and the browser give
    /// the application one surface and no more, so there is nothing for a second window to be.
    /// </summary>
    private static bool HasWindows => !OperatingSystem.IsBrowser() && !OperatingSystem.IsAndroid() && !OperatingSystem.IsIOS();

    /// <summary>
    /// Opens the full window, one at a time, brought forward again rather than stacked up, since it is the
    /// window itself that is being looked at.
    /// </summary>
    private void ShowFullWindow()
    {
        if (this.fullWindow != null)
        {
            this.fullWindow.Activate();
            return;
        }

        var fullWindowDemo = new FullWindowDemo();
        this.fullWindow = fullWindowDemo;
        fullWindowDemo.Closed += (_, _) => this.fullWindow = null;
        fullWindowDemo.Activate();
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
