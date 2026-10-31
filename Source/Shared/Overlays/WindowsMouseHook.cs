// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowsMouseHook.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

/// <summary>
/// Reports where the cursor is while it is over a window that cannot be told.
/// </summary>
/// <remarks>
/// A click through overlay is given no mouse messages at all, which is the whole point of it, and so it cannot follow
/// the cursor the ordinary way. A low level hook sees the moves before any window does and is not affected by the
/// extended style, so it is what is left. It is installed only while something actually tracks the cursor, because a
/// hook of this kind is called for every move in the session.
/// </remarks>
[SupportedOSPlatform("windows")]
internal sealed class WindowsMouseHook : IDisposable
{
#pragma warning disable SA1310
    private const int WH_MOUSE_LL = 14;
    private const int WM_MOUSEMOVE = 0x0200;
#pragma warning restore SA1310

    private readonly Action<int, int> onMouseMove;

    /// <summary>
    /// Kept alive for as long as the hook is installed. The window manager holds the unmanaged function pointer
    /// only, so letting the delegate be collected would leave it calling into nothing.
    /// </summary>
    private readonly LowLevelMouseProc mouseProcedure;

    private nint hookId;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsMouseHook"/> class and installs the hook.
    /// </summary>
    /// <param name="onMouseMove">Called with the cursor position in screen pixels, on the thread the move arrived on.</param>
    public WindowsMouseHook(Action<int, int> onMouseMove)
    {
        this.onMouseMove = onMouseMove;
        this.mouseProcedure = this.OnMouseProcedure;
        this.hookId = Install(this.mouseProcedure);
    }

    /// <summary>
    /// Gets a value indicating whether the hook is installed.
    /// </summary>
    public bool IsInstalled => this.hookId != 0;

    /// <summary>
    /// Removes the hook.
    /// </summary>
    public void Dispose()
    {
        if (this.hookId != 0)
        {
            UnhookWindowsHookEx(this.hookId);
            this.hookId = 0;
        }
    }

    private static nint Install(LowLevelMouseProc lowLevelMouseProc)
    {
        using var currentProcess = Process.GetCurrentProcess();
        using var mainModule = currentProcess.MainModule;
        if (mainModule?.ModuleName == null)
        {
            return 0;
        }

        return SetWindowsHookEx(WH_MOUSE_LL, lowLevelMouseProc, GetModuleHandle(mainModule.ModuleName), 0);
    }

    private nint OnMouseProcedure(int code, nint wParam, nint lParam)
    {
        if (code >= 0 && wParam == WM_MOUSEMOVE)
        {
            var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            this.onMouseMove(hookStruct.pt.X, hookStruct.pt.Y);
        }

        return CallNextHookEx(this.hookId, code, wParam, lParam);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll")]
    private static extern nint CallNextHookEx(nint hhk, int code, nint wParam, nint lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint GetModuleHandle(string moduleName);

    private delegate nint LowLevelMouseProc(int code, nint wParam, nint lParam);

#pragma warning disable SA1307
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public nint dwExtraInfo;
    }
#pragma warning restore SA1307
}
