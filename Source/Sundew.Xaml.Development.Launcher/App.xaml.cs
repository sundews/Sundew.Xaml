// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Launcher;

using Microsoft.UI.Xaml;

public partial class App : Application
{
    private Window? window;

    public App()
    {
        this.InitializeComponent();
        this.UnhandledException += (_, e) =>
        {
            LogFile.Write(DateTime.Now + " " + e.Exception);
        };
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        this.window = new MainWindow();
        this.window.Activate();
    }
}
