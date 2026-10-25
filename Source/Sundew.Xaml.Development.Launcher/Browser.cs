// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Browser.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Launcher;

using System.Diagnostics;
using System.IO;

/// <summary>
/// Opens a browser head in a window of its own.
/// </summary>
/// <remarks>
/// Handing the url to whatever browser is already open would put it in a tab of a process the launcher does not
/// own, which can be neither laid out beside the other testers nor closed with them. A window of its own, in a
/// process of its own, can be both.
/// </remarks>
public static class Browser
{
    private static readonly string[] Candidates =
    [
        @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
        @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
        @"C:\Program Files\Google\Chrome\Application\chrome.exe",
        @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
    ];

    /// <summary>
    /// Opens a url in a window of its own.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <param name="name">A name for the profile the window runs under, which keeps it a process of its own.</param>
    /// <returns>The process, or null when it had to fall back to whichever browser is already open.</returns>
    public static Process? Open(string url, string name)
    {
        var browser = Candidates.FirstOrDefault(File.Exists);
        if (browser == null)
        {
            // Nothing recognised, so the url goes to whatever is registered for it. That window cannot be tiled
            // or closed by the launcher, but it is better than not opening at all.
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            return null;
        }

        // A profile of its own is what makes this a process of its own. Asked to use the usual one, a browser
        // that is already running simply opens a window and exits, leaving nothing to hold on to.
        var profile = Path.Combine(Path.GetTempPath(), "sundew-xaml-launcher", name);
        Directory.CreateDirectory(profile);
        return Process.Start(new ProcessStartInfo(browser)
        {
            // Extensions off as well: a browser installs the machine's own into even a brand new profile, and one
            // of them greeting the fresh profile with a welcome window put a window in the tiling that is not a
            // tester at all. Nothing here needs an extension.
            Arguments = "--app=" + url + " --user-data-dir=\"" + profile
                        + "\" --no-first-run --no-default-browser-check --disable-extensions",
            UseShellExecute = false,
        });
    }
}
