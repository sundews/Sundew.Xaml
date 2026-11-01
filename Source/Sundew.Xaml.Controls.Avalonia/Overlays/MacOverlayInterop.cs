// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MacOverlayInterop.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

/// <summary>
/// The window server calls that make a window click through on macOS, which Avalonia does not expose.
/// </summary>
/// <remarks>
/// A window there is taken out of the way of the mouse by saying so on the window itself, which is one message to
/// the Objective C runtime. The cursor is then read by asking the window server where it is rather than by being
/// told, because a window the mouse passes through hears nothing about it.
/// </remarks>
[SupportedOSPlatform("macos")]
internal static class MacOverlayInterop
{
    private const string ObjectiveCRuntime = "/usr/lib/libobjc.dylib";
    private const string CoreGraphics = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";
    private const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    /// <summary>
    /// Makes the window take no mouse input, so that clicks land on whatever is underneath.
    /// </summary>
    /// <param name="handle">The handle Avalonia hands out, which is either the window or the view in it.</param>
    /// <returns><c>true</c>, if the window could be told, otherwise <c>false</c>.</returns>
    /// <remarks>
    /// Which of the two it is, is asked of the object rather than assumed: the answer has been both, and sending
    /// a message an object does not answer to would bring the process down rather than fail.
    /// </remarks>
    public static bool TryMakeClickThrough(nint handle)
    {
        if (handle == 0)
        {
            return false;
        }

        var ignoreMouseEvents = GetSelector("setIgnoresMouseEvents:");
        var window = handle;
        if (!RespondsToSelector(handle, ignoreMouseEvents))
        {
            var windowOfView = GetSelector("window");
            if (!RespondsToSelector(handle, windowOfView))
            {
                return false;
            }

            window = SendMessageReturningPointer(handle, windowOfView);
            if (window == 0 || !RespondsToSelector(window, ignoreMouseEvents))
            {
                return false;
            }
        }

        SendMessageTakingBoolean(window, ignoreMouseEvents, true);
        return true;
    }

    /// <summary>
    /// Asks whether the object answers to a message at all.
    /// </summary>
    private static bool RespondsToSelector(nint receiver, nint selector)
    {
        return SendMessageAskingAboutSelector(receiver, GetSelector("respondsToSelector:"), selector);
    }

    /// <summary>
    /// Gets where the cursor is, in screen pixels, with the origin at the top left as everything above this
    /// expects it. The event the location is read from is the window server's own idea of where the mouse is.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns><c>true</c>, if the position could be read, otherwise <c>false</c>.</returns>
    public static bool TryGetCursorPosition(out int x, out int y)
    {
        var mouseEvent = CGEventCreate(0);
        if (mouseEvent == 0)
        {
            x = 0;
            y = 0;
            return false;
        }

        try
        {
            var location = CGEventGetLocation(mouseEvent);
            x = (int)location.X;
            y = (int)location.Y;
            return true;
        }
        finally
        {
            CFRelease(mouseEvent);
        }
    }

    [DllImport(ObjectiveCRuntime, EntryPoint = "sel_registerName", CharSet = CharSet.Ansi)]
    private static extern nint GetSelector(string name);

    [DllImport(ObjectiveCRuntime, EntryPoint = "objc_msgSend")]
    private static extern nint SendMessageReturningPointer(nint receiver, nint selector);

    [DllImport(ObjectiveCRuntime, EntryPoint = "objc_msgSend")]
    private static extern void SendMessageTakingBoolean(nint receiver, nint selector, [MarshalAs(UnmanagedType.I1)] bool value);

    [DllImport(ObjectiveCRuntime, EntryPoint = "objc_msgSend")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool SendMessageAskingAboutSelector(nint receiver, nint selector, nint argument);

    [DllImport(CoreGraphics)]
    private static extern nint CGEventCreate(nint source);

    [DllImport(CoreGraphics)]
    private static extern CGPoint CGEventGetLocation(nint mouseEvent);

    [DllImport(CoreFoundation)]
    private static extern void CFRelease(nint handle);

#pragma warning disable SA1307
    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double X;

        public double Y;
    }
#pragma warning restore SA1307
}
