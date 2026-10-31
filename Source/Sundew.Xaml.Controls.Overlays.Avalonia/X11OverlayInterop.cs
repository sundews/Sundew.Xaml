// --------------------------------------------------------------------------------------------------------------------
// <copyright file="X11OverlayInterop.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

/// <summary>
/// The X calls that make a window click through on Linux, which Avalonia does not expose.
/// </summary>
/// <remarks>
/// <para>
/// X decides what a window can be hit on from its input shape, which by default is the whole of it. Setting that
/// shape to nothing leaves the window drawn as before and impossible to hit, so the mouse reaches whatever is
/// underneath. The cursor is then read by asking the server where it is, because a window the mouse passes through
/// hears nothing about it.
/// </para>
/// <para>
/// A connection of this library's own is opened for both, because Avalonia keeps its own to itself. X puts no
/// ownership on a window, so a second connection may shape it; the two are the same process either way.
/// </para>
/// <para>
/// This speaks X, so it is for an X session or for XWayland. A Wayland session proper decides the same thing with
/// wl_surface.set_input_region, which only the toolkit that owns the surface can say, and Avalonia does not.
/// </para>
/// </remarks>
[SupportedOSPlatform("linux")]
internal static class X11OverlayInterop
{
    private const string X11 = "libX11.so.6";
    private const string XExtension = "libXext.so.6";

#pragma warning disable SA1310
    private const int ShapeInput = 2;
    private const int ShapeSet = 0;
    private const int Unsorted = 0;
#pragma warning restore SA1310

    private static readonly object Gate = new();

    private static nint display;

    private static bool displayAttempted;

    /// <summary>
    /// Takes the window out of the way of the mouse, by leaving it no shape to be hit on.
    /// </summary>
    /// <param name="windowHandle">The window id.</param>
    /// <returns><c>true</c>, if the server could be told, otherwise <c>false</c>.</returns>
    public static bool TryMakeClickThrough(nint windowHandle)
    {
        if (windowHandle == 0 || !TryGetDisplay(out var openDisplay))
        {
            return false;
        }

        // No rectangles at all, which is an empty input shape: there is nowhere on the window to hit.
        XShapeCombineRectangles(openDisplay, windowHandle, ShapeInput, 0, 0, 0, 0, ShapeSet, Unsorted);
        XSync(openDisplay, false);
        return true;
    }

    /// <summary>
    /// Gets where the cursor is, in screen pixels.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns><c>true</c>, if the position could be read, otherwise <c>false</c>.</returns>
    public static bool TryGetCursorPosition(out int x, out int y)
    {
        x = 0;
        y = 0;
        if (!TryGetDisplay(out var openDisplay))
        {
            return false;
        }

        var root = XDefaultRootWindow(openDisplay);
        if (!XQueryPointer(openDisplay, root, out _, out _, out var rootX, out var rootY, out _, out _, out _))
        {
            return false;
        }

        x = rootX;
        y = rootY;
        return true;
    }

    /// <summary>
    /// Opens the connection, once, and remembers whether there was one to open. There is no closing it: it lasts
    /// as long as the process, like the display it speaks to.
    /// </summary>
    private static bool TryGetDisplay(out nint openDisplay)
    {
        lock (Gate)
        {
            if (!displayAttempted)
            {
                displayAttempted = true;
                try
                {
                    display = XOpenDisplay(0);
                }
                catch (DllNotFoundException)
                {
                    // No X on this session, which a Wayland one without XWayland is.
                    display = 0;
                }
                catch (EntryPointNotFoundException)
                {
                    display = 0;
                }
            }

            openDisplay = display;
            return openDisplay != 0;
        }
    }

    [DllImport(X11)]
    private static extern nint XOpenDisplay(nint name);

    [DllImport(X11)]
    private static extern nint XDefaultRootWindow(nint display);

    [DllImport(X11)]
    private static extern int XSync(nint display, bool discard);

    [DllImport(X11)]
    private static extern bool XQueryPointer(nint display, nint window, out nint root, out nint child, out int rootX, out int rootY, out int windowX, out int windowY, out uint mask);

    [DllImport(XExtension)]
    private static extern void XShapeCombineRectangles(nint display, nint window, int kind, int xOffset, int yOffset, nint rectangles, int count, int operation, int ordering);
}
