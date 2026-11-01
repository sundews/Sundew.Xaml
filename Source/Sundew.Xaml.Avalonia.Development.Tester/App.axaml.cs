// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.axaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Avalonia.Development.Tester;

using global::Avalonia;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Markup.Xaml;

public partial class App : Application
{
    public override void Initialize()
    {
        this.InitializeComponent();
    }

    /// <remarks>
    /// Through a method of this name rather than calling the loader from Initialize itself. The XAML compiler
    /// rewrites the body of an InitializeComponent into a direct call to what it compiled from App.axaml;
    /// written anywhere else the call stays as it reads and looks the compiled XAML up by name at runtime, which
    /// a head compiled ahead of time has nothing left of, and the head then starts only as far as saying it has
    /// no precompiled XAML. The window and the view beside this are written the same way, for the same reason.
    /// </remarks>
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // The desktop head hands out a window; Android and the browser hand out a single view. Both show the very
        // same MainView, so the sheet of examples is identical on every head.
        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
        else if (this.ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new MainView();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
