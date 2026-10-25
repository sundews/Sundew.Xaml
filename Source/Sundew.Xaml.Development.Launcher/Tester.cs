// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tester.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Launcher;

/// <summary>
/// Where a tester runs, which decides how it is started and how it is stopped again.
/// </summary>
public enum TesterKind
{
    /// <summary>A window on this machine.</summary>
    Windows,

    /// <summary>An app deployed to an attached Android device.</summary>
    Android,

    /// <summary>A page served to a browser.</summary>
    Browser,
}

/// <summary>
/// One tester the launcher can start.
/// </summary>
/// <param name="Name">What to call it in the list.</param>
/// <param name="Kind">Where it runs.</param>
/// <param name="ProjectName">The project folder, under Source.</param>
/// <param name="TargetFramework">The head to build.</param>
/// <param name="ApplicationId">The Android package, which is how an Android tester is stopped again.</param>
/// <param name="ProcessName">What the running tester shows up as, for a tester that runs here.</param>
/// <param name="ExtraArguments">Anything else the build needs.</param>
/// <param name="SupportsNativeAot">Whether this head can be compiled ahead of time, which the WPF and MAUI
/// Windows heads cannot and the Android and browser heads cannot either. Where it can, the launcher runs the
/// published native binary in preference to the CLI, which would otherwise start the very same build under the
/// JIT and the tester would say so.</param>
public sealed record Tester(
    string Name,
    TesterKind Kind,
    string ProjectName,
    string TargetFramework,
    string? ApplicationId = null,
    string? ProcessName = null,
    string? ExtraArguments = null,
    bool SupportsNativeAot = false);
