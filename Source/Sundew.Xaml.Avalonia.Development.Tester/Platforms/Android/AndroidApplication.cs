// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AndroidApplication.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Avalonia.Development.Tester.Droid;

using global::Android.App;
using global::Android.Runtime;
using global::Avalonia;
using global::Avalonia.Android;

/// <summary>
/// The Android entry point, which hands Avalonia the very same <see cref="App"/> the desktop head runs.
/// </summary>
[Application]
public class AndroidApplication : AvaloniaAndroidApplication<App>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AndroidApplication"/> class.
    /// </summary>
    /// <param name="javaReference">The java reference.</param>
    /// <param name="transfer">The handle ownership.</param>
    protected AndroidApplication(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    /// <inheritdoc/>
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder);
    }
}
