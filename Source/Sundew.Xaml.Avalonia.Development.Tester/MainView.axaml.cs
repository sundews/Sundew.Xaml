// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainView.axaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Avalonia.Development.Tester;

using global::Avalonia.Controls;
using Sundew.Xaml.Development;
using global::Avalonia.Markup.Xaml;

public partial class MainView : UserControl
{
    public MainView()
    {
        this.InitializeComponent();

        // Looked up by name rather than through the generated field. This view declares InitializeComponent
        // itself, which leaves the generated one, and with it the assigning of the x:Name fields, out of the
        // build: the field is there to compile against but nothing ever sets it.
        var runtimeLabel = this.FindControl<TextBlock>("RuntimeLabel");
        if (runtimeLabel != null)
        {
            runtimeLabel.Text = RuntimeName;
            this.Loaded += (_, _) =>
            {
                StartupClock.Loaded();
                runtimeLabel.Text = RuntimeName + " · " + StartupClock.Describe();
            };
        }
    }

    /// <summary>
    /// Gets how this head is running. NativeAOT is the one case where the runtime cannot compile code as it goes, which
    /// is what tells it apart from a JIT run of the very same build. A browser head is neither: it is called out by
    /// name rather than reported as ahead of time, which it is not.
    /// </summary>
    private static string RuntimeName => OperatingSystem.IsBrowser()
        ? "wasm"
        : System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported ? "JIT" : "Native AOT";

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
