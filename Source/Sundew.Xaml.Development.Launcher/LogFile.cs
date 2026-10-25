// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogFile.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Launcher;

using System.IO;

/// <summary>
/// Keeps what the launcher and the testers write, so that a failure can be read after the fact.
/// </summary>
/// <remarks>
/// Every tester writes from a thread of its own, so the writes are serialised here. Appending from each of them
/// as they came was asking the file to be opened while it was already open, which it is not.
/// </remarks>
public static class LogFile
{
    private static readonly object Gate = new();

    /// <summary>
    /// Gets where the log is written.
    /// </summary>
    public static string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sundew-xaml-launcher.log");

    /// <summary>
    /// Writes a line.
    /// </summary>
    /// <param name="line">The line.</param>
    public static void Write(string line)
    {
        lock (Gate)
        {
            try
            {
                File.AppendAllText(Path, line + Environment.NewLine);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                // A log that cannot be written is not worth bringing the launcher down over.
            }
        }
    }
}
