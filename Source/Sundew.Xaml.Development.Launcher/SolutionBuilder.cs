// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SolutionBuilder.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Launcher;

using System.Diagnostics;
using System.IO;

/// <summary>
/// Builds everything once, before any tester is started.
/// </summary>
/// <remarks>
/// Started one at a time, each tester builds the libraries they all share again, and several of them build the
/// same project at once for different heads. One build of the solution does that work once, and leaves each
/// tester with nothing to do but start.
/// </remarks>
public static class SolutionBuilder
{
    /// <summary>
    /// Builds the solution, with every head the testers have.
    /// </summary>
    /// <param name="output">Where to report progress.</param>
    /// <returns><c>true</c> when it succeeded.</returns>
    public static bool Build(Action<string> output)
    {
        var source = TesterSession.FindSourceDirectory();
        if (source == null)
        {
            output("Could not find the Source directory");
            return false;
        }

        var arguments = "build \"" + Path.Combine(source, "Sundew.Xaml.slnx") + "\""
            + " -p:SundewUnoAllHeads=true -p:SundewAvaloniaAllHeads=true -p:SundewMauiAllHeads=true"
            + " -p:SppDisable=true";
        output("dotnet " + arguments);
        using var process = Process.Start(new ProcessStartInfo("dotnet", arguments)
        {
            WorkingDirectory = source,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        });

        if (process == null)
        {
            return false;
        }

        // Only what matters is reported; a solution build says a great deal that nobody is waiting to read.
        while (process.StandardOutput.ReadLine() is { } line)
        {
            if (line.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("Build succeeded", StringComparison.Ordinal) ||
                line.Contains("Build FAILED", StringComparison.Ordinal))
            {
                output(line);
            }
        }

        process.WaitForExit();
        return process.ExitCode == 0;
    }
}
