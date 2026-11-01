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
        this.OverlayToggle = this.FindControl<CheckBox>("SubtitleOverlayToggle");
        this.FullWindowOpener = this.FindControl<Button>("FullWindowButton");
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
    /// Gets the toggle that asks for the subtitle overlay, which only a head with windows can answer.
    /// </summary>
    internal CheckBox? OverlayToggle { get; }

    /// <summary>
    /// Gets the button that asks for the full window, which only a head with windows can answer.
    /// </summary>
    internal Button? FullWindowOpener { get; }

    /// <summary>
    /// Gets how this head is running. NativeAOT is the one case where the runtime cannot compile code as it goes, which
    /// is what tells it apart from a JIT run of the very same build. A browser head is neither: it is called out by
    /// name rather than reported as ahead of time, which it is not.
    /// </summary>
    /// <remarks>
    /// Asked by where the core library came from, because the two obvious questions both lie here. A project
    /// that can be published ahead of time has RuntimeFeature.IsDynamicCodeSupported turned off in the runtime
    /// configuration of its ordinary build as well, and IsDynamicCodeCompiled follows it, so both report a plain
    /// build as native while it runs under the just in time compiler. A head compiled ahead of time has no
    /// assembly files to have come from, so the core library has no location; one running on the runtime does.
    /// A head published as a single file would answer the same way without being native, which none of these are.
    /// </remarks>
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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
