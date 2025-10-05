// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WpfApplication.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Wpf.Development.Tests;

using System.Windows;

public static class WpfApplication
{
    public static readonly Application Current = new Application();

    public static void Initialize()
    {
    }
}