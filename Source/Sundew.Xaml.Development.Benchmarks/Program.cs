// ----------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Benchmarks;

using System;
using System.IO;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

public static class Program
{
    private const string CountsOption = "--counts";

    public static int Main(string[] args)
    {
        if (args != null && Array.IndexOf(args, CountsOption) >= 0)
        {
            PrintCounts();
            return 0;
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, CreateConfig());
        return 0;
    }

    /// <summary>
    /// Prints how many fills each border comes to.
    /// </summary>
    /// <remarks>
    /// These do not vary with the machine the way a timing does, so they are the numbers worth watching: a change
    /// that means well but adds a fill to every corner shows up here at once, and shows up the same anywhere.
    /// </remarks>
    private static void PrintCounts()
    {
        Console.WriteLine($"{"sides",-12}{"radius",8}{"fills",8}{"gradient",10}{"layers",8}");
        var session = new CountingDrawingSession();
        foreach (var sides in Enum.GetValues<SidePaints>())
        {
            foreach (var radius in new[] { 0d, 4d, 20d })
            {
                session.Reset();
                BorderScene.Create(sides, radius).Render(session);
                Console.WriteLine(
                    $"{sides,-12}{radius,8}{session.Fills,8}{session.GradientFills,10}{session.Layers,8}");
            }
        }
    }

    /// <summary>
    /// Builds the configuration the benchmarks run under.
    /// </summary>
    /// <remarks>
    /// BenchmarkDotNet generates a project of its own and builds it for every run. Left under the repository, that
    /// project picks up Directory.Build.props and with it this repository's analysers, its warnings as errors and
    /// its output paths, none of which generated code is written to satisfy. Somewhere else it inherits nothing,
    /// which is the whole of what it needs.
    /// </remarks>
    /// <returns>The configuration.</returns>
    private static IConfig CreateConfig()
    {
        return DefaultConfig.Instance.WithArtifactsPath(
            Path.Combine(Path.GetTempPath(), "Sundew.Xaml.Development.Benchmarks"));
    }
}
