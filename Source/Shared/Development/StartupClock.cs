// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartupClock.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

/// <summary>
/// Times a tester from its first managed instruction to its sheet being up, which is the figure the testers show
/// beside how they are running. Comparing a head against itself compiled ahead of time is the point of it.
/// </summary>
/// <remarks>
/// The start is taken in a module initializer, which the runtime calls before anything else in the assembly and so
/// is as near the first instruction as managed code can observe. It is deliberately not measured from when the
/// process was created: that would fold in the runtime's own start, which differs so much between a framework
/// dependent head and a native one that it would swamp everything the testers are being compared on.
/// </remarks>
internal static class StartupClock
{
    /// <summary>
    /// Why the module initializer below is right here even though the analyzer warns about libraries: this file
    /// is compiled into each tester application, and the analyzer sees the shared source rather than where it
    /// ends up.
    /// </summary>
    private const string AppCode = "Compiled into each tester application, not shipped as a library.";

    private static readonly long Started = Stopwatch.GetTimestamp();
    private static long loaded;

    /// <summary>
    /// Gets a value indicating whether the sheet has reported itself up.
    /// </summary>
    internal static bool HasLoaded => Volatile.Read(ref loaded) != 0;

    /// <summary>
    /// Starts the clock. The runtime calls this before anything else in the assembly runs; the body only has to
    /// touch the field, because reading it is what runs the initializer that timestamps it.
    /// </summary>
    [ModuleInitializer]
    [SuppressMessage("Usage", "CA2255:The ModuleInitializer attribute should not be used in libraries", Justification = AppCode)]
    internal static void Begin()
    {
        _ = Started;
    }

    /// <summary>
    /// Records that the sheet is up. Only the first call counts, a view being free to load more than once.
    /// </summary>
    internal static void Loaded()
    {
        Interlocked.CompareExchange(ref loaded, Stopwatch.GetTimestamp(), 0);
    }

    /// <summary>
    /// Describes how long starting took, for showing on the sheet.
    /// </summary>
    /// <returns>The time, or an empty string before the sheet has reported itself up.</returns>
    internal static string Describe()
    {
        var at = Volatile.Read(ref loaded);
        if (at == 0)
        {
            return string.Empty;
        }

        var elapsed = Stopwatch.GetElapsedTime(Started, at);
        return elapsed.TotalMilliseconds.ToString("N0", CultureInfo.InvariantCulture) + " ms";
    }
}
