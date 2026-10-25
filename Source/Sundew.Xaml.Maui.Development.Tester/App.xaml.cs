// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Maui.Development.Tester;

using Microsoft.Extensions.DependencyInjection;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Without this the Windows head comes up with no title at all, which the launcher tiles windows by.
        return new Window(new MainPage()) { Title = "Sundew.Xaml MAUI Tester" };
    }
}
