// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowsOverlayInterop.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

/// <summary>
/// The window manager calls that make a window click through, which neither Avalonia nor WinUI expose.
/// </summary>
/// <remarks>
/// A click through window is what makes an overlay an overlay: it is drawn over everything and takes no input, so
/// whatever is underneath stays usable. Windows settles that with an extended window style, which Avalonia does not
/// reach, so the window handle is asked for and the style is set on it directly. The WinUI overlay creates its own
/// window and says it there instead. Every other platform has its own way of saying it, or no way at all, which is
/// why this is named for the one it speaks for.
/// </remarks>
[SupportedOSPlatform("windows")]
internal static class WindowsOverlayInterop
{
#pragma warning disable SA1310
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const uint LWA_ALPHA = 0x00000002;
#pragma warning restore SA1310

    /// <summary>
    /// Makes the window take no mouse input and never take the focus, so that clicks land on whatever is
    /// underneath it and the window underneath stays the one being used.
    /// </summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <remarks>
    /// <para>
    /// WS_EX_TRANSPARENT alone does not do it, however much it sounds as though it should: a window carrying only
    /// that is still hit, as a bare window carrying nothing else was measured doing. It takes WS_EX_LAYERED
    /// alongside it, and a layered window shows nothing until it is told how opaque it is, so it is told outright
    /// that it is opaque. What is see through about an overlay is what it draws, not the window.
    /// </para>
    /// <para>
    /// WS_EX_NOACTIVATE keeps the focus where it was. Without it the overlay can become the window in front,
    /// which means closing it is what Alt+F4 does, and an overlay is not a window anyone meant to close.
    /// WS_EX_TOOLWINDOW keeps it out of the alt tab list for the same reason.
    /// </para>
    /// </remarks>
    public static void MakeClickThrough(nint windowHandle)
    {
        if (windowHandle == 0)
        {
            return;
        }

        var extendedStyle = GetWindowLongAuto(windowHandle, GWL_EXSTYLE);
        SetWindowLongAuto(
            windowHandle,
            GWL_EXSTYLE,
            extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);
        SetLayeredWindowAttributes(windowHandle, 0, 255, LWA_ALPHA);
    }

    /// <summary>
    /// Gets the window that is in front, which is the one the keyboard is talking to.
    /// </summary>
    /// <returns>The window handle, or zero.</returns>
    public static nint GetWindowInFront()
    {
        return GetForegroundWindow();
    }

    /// <summary>
    /// Puts the window that was in front back in front, where the overlay took its place.
    /// </summary>
    /// <param name="previousWindow">The window that was in front before the overlay opened.</param>
    /// <param name="overlayWindow">The overlay.</param>
    /// <remarks>
    /// An overlay is asked not to be activated, and is told so again through WS_EX_NOACTIVATE, but by the time
    /// there is a window to tell it has already been shown and may already have taken the front. Taking it is
    /// what makes Alt+F4 close the overlay rather than the window being worked in, so it is handed straight back.
    /// </remarks>
    public static void RestoreWindowInFront(nint previousWindow, nint overlayWindow)
    {
        if (previousWindow == 0 || previousWindow == overlayWindow || GetForegroundWindow() != overlayWindow)
        {
            return;
        }

        SetForegroundWindow(previousWindow);
    }

    /// <summary>
    /// Gets where the cursor is, in screen pixels.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns><c>true</c>, if the position could be read, otherwise <c>false</c>.</returns>
    public static bool TryGetCursorPosition(out int x, out int y)
    {
        if (GetCursorPos(out var point))
        {
            x = point.X;
            y = point.Y;
            return true;
        }

        x = 0;
        y = 0;
        return false;
    }

    /// <summary>
    /// Reads a window long, through whichever of the two entry points this process is wide enough for.
    /// </summary>
    /// <remarks>
    /// The Ptr suffixed entry points exist only in the 64 bit window manager; a 32 bit process has to use the plain
    /// ones, which there carry the whole value.
    /// </remarks>
    private static nint GetWindowLongAuto(nint windowHandle, int index)
    {
        return nint.Size == 8 ? GetWindowLongPtr(windowHandle, index) : GetWindowLong(windowHandle, index);
    }

    private static void SetWindowLongAuto(nint windowHandle, int index, nint newStyle)
    {
        if (nint.Size == 8)
        {
            SetWindowLongPtr(windowHandle, index, newStyle);
        }
        else
        {
            SetWindowLong(windowHandle, index, (int)newStyle);
        }
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    private static extern int GetWindowLong(nint windowHandle, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern int SetWindowLong(nint windowHandle, int index, int newStyle);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern nint GetWindowLongPtr(nint windowHandle, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern nint SetWindowLongPtr(nint windowHandle, int index, nint newStyle);

    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(nint windowHandle, int colorKey, byte alpha, uint flags);

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(nint windowHandle);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT point);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
}
