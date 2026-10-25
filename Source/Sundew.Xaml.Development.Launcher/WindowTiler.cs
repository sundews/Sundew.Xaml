// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowTiler.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Development.Launcher;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Lays the running testers out side by side, so that the same sheet can be seen on all of them at once.
/// </summary>
/// <remarks>
/// The windows are found by the process they belong to rather than by anything the launcher holds onto, because a
/// tester is started through the dotnet CLI and so is a grandchild, and an emulator window belongs to a process of
/// the Android SDK's own.
/// </remarks>
public static class WindowTiler
{
    /// <summary>
    /// The shape a cell is aimed at, being roughly that of the 900x750 sheet every tester lays out. Cells near it
    /// show a whole sheet, which is the point of tiling in the first place.
    /// </summary>
    private const double PreferredCellAspect = 1.1;

    /// <summary>
    /// How much a grid is penalised per cell it leaves empty, so that a slightly worse cell shape is preferred to a
    /// layout that scatters the testers across a mostly empty screen.
    /// </summary>
    private const double EmptyCellPenalty = 0.05;

    /// <summary>
    /// The widest a window may be and still be taken for a panel riding alongside another. The emulator's toolbar
    /// is about 50 pixels.
    /// </summary>
    private const int WidestSidePanel = 140;

    private const uint ShowWindowRestore = 9;
    private const uint SetWindowPosNoSize = 0x0001;
    private const uint SetWindowPosNoActivate = 0x0010;
    private const uint SpiGetWorkArea = 0x0030;
    private const int GwlExStyle = -20;
    private const int WsExToolWindow = 0x00000080;
    private const uint GwOwner = 4;
    private static readonly IntPtr Top = IntPtr.Zero;

    /// <summary>
    /// The processes whose windows are tiled, in the order they are laid out. The testers are named after their own
    /// projects, and an emulator window belongs to the QEMU process the Android SDK starts.
    /// </summary>
    private static readonly string[] ProcessNames =
    [
        "Sundew.Xaml.Wpf.Development.Tester",
        "Sundew.Xaml.WinUI.Development.Tester",
        "Sundew.Xaml.Uno.Development.Tester",
        "Sundew.Xaml.Avalonia.Development.Tester",
        "Sundew.Xaml.Maui.Development.Tester",
        "qemu-system-x86_64",
        "qemu-system-aarch64",
    ];

    /// <summary>
    /// Arranges every tester window found into a grid across the work area.
    /// </summary>
    /// <param name="excludedWindow">The launcher's own window, which stays where it is.</param>
    /// <param name="extraProcessIds">Processes to include besides those known by name, such as the browser
    /// windows the launcher opened.</param>
    /// <returns>How many windows were moved.</returns>
    public static async Task<int> TileAsync(IntPtr excludedWindow, IEnumerable<int>? extraProcessIds = null)
    {
        var windows = FindWindows(excludedWindow, extraProcessIds);
        if (windows.Count == 0)
        {
            return 0;
        }

        if (!SystemParametersInfo(SpiGetWorkArea, 0, out var workArea, 0))
        {
            return 0;
        }

        var width = workArea.Right - workArea.Left;
        var height = workArea.Bottom - workArea.Top;
        var (columns, rows) = ChooseGrid(windows.Count, width, height);
        var cellWidth = width / columns;
        var cellHeight = height / rows;

        var cells = new List<(TiledWindow Window, int Left, int Top)>(windows.Count);
        for (var index = 0; index < windows.Count; index++)
        {
            var window = windows[index];
            var left = workArea.Left + ((index % columns) * cellWidth);
            var top = workArea.Top + ((index / columns) * cellHeight);
            ShowWindow(window.Handle, ShowWindowRestore);

            // The emulator's toolbar rides alongside its window rather than inside it, so the window is given that
            // much less room and the two of them together fill the cell.
            SetWindowPos(window.Handle, Top, left, top, cellWidth - window.SidePanelWidth, cellHeight, SetWindowPosNoActivate);
            cells.Add((window, left, top));
        }

        // Centring is a pass of its own, after a window has had time to answer the size it was given. The emulator
        // takes its own size rather than the one asked for, but not before the call that asked returns: measuring it
        // straight away reports the size it was told to be and leaves it at the top of its cell.
        await Task.Delay(TimeSpan.FromMilliseconds(600));
        foreach (var (window, left, top) in cells)
        {
            Centre(window, left, top, cellWidth, cellHeight);
        }

        return windows.Count;
    }

    /// <summary>
    /// Centres a window that would not take the size asked of it, as the emulator will not because it keeps the
    /// device's aspect, rather than leaving it hugging one corner of its cell.
    /// </summary>
    private static void Centre(TiledWindow window, int left, int top, int cellWidth, int cellHeight)
    {
        if (!GetWindowRect(window.Handle, out var placed))
        {
            return;
        }

        var placedWidth = placed.Right - placed.Left + window.SidePanelWidth;
        var placedHeight = placed.Bottom - placed.Top;
        if (placedWidth >= cellWidth && placedHeight >= cellHeight)
        {
            return;
        }

        SetWindowPos(
            window.Handle,
            Top,
            left + Math.Max(0, (cellWidth - placedWidth) / 2),
            top + Math.Max(0, (cellHeight - placedHeight) / 2),
            0,
            0,
            SetWindowPosNoActivate | SetWindowPosNoSize);
    }

    /// <summary>
    /// Picks the grid whose cells come closest to the shape of a tester sheet. Squaring the count up, as the obvious
    /// reading of "a grid" would, is wrong on a wide screen: sixteen cells across 5120x2100 are 1280 by 525, so every
    /// tester shows a third of its sheet while a quarter of the screen stays empty.
    /// </summary>
    private static (int Columns, int Rows) ChooseGrid(int count, int width, int height)
    {
        var best = (Columns: count, Rows: 1);
        var bestScore = double.MaxValue;
        for (var columns = 1; columns <= count; columns++)
        {
            var rows = (int)Math.Ceiling(count / (double)columns);
            var aspect = (width / (double)columns) / (height / (double)rows);
            var score = Math.Abs(Math.Log(aspect / PreferredCellAspect)) + (EmptyCellPenalty * ((columns * rows) - count));
            if (score < bestScore)
            {
                bestScore = score;
                best = (columns, rows);
            }
        }

        return best;
    }

    private static List<TiledWindow> FindWindows(IntPtr excludedWindow, IEnumerable<int>? extraProcessIds)
    {
        var order = new Dictionary<int, int>();
        for (var index = 0; index < ProcessNames.Length; index++)
        {
            foreach (var process in Process.GetProcessesByName(ProcessNames[index]))
            {
                order[process.Id] = index;
            }
        }

        foreach (var processId in extraProcessIds ?? [])
        {
            // The browsers come last, after everything the launcher knows by name.
            order.TryAdd(processId, ProcessNames.Length);
        }

        var candidates = new List<(IntPtr Handle, int ProcessId, string Title)>();
        var sidePanels = new List<(int ProcessId, int Width)>();
        EnumWindows(
            (window, _) =>
            {
                if (window == excludedWindow || !IsWindowVisible(window))
                {
                    return true;
                }

                GetWindowThreadProcessId(window, out var processId);
                if (!order.ContainsKey(processId))
                {
                    return true;
                }

                // Only the windows that would show up in Alt-Tab are tiled: one that has an owner, or that is marked
                // a tool window, belongs to another window and follows it about. The emulator's toolbar is exactly
                // that, and tiling it as though it were a tester is what left cells empty and testers overlapping.
                if (GetWindow(window, GwOwner) != IntPtr.Zero || (GetWindowLong(window, GwlExStyle) & WsExToolWindow) != 0)
                {
                    RememberSidePanel(sidePanels, window, processId);
                    return true;
                }

                var length = GetWindowTextLength(window);
                if (length == 0)
                {
                    return true;
                }

                var text = new StringBuilder(length + 1);
                GetWindowText(window, text, text.Capacity);
                if (text.Length > 0)
                {
                    candidates.Add((window, processId, text.ToString()));
                }

                return true;
            },
            IntPtr.Zero);

        // Laid out in a fixed order rather than the z-order EnumWindows hands back, so that the same tester lands in
        // the same cell every time and two runs can be compared.
        return candidates
            .OrderBy(candidate => order[candidate.ProcessId])
            .ThenBy(candidate => candidate.Title, StringComparer.Ordinal)
            .Select(candidate => new TiledWindow(candidate.Handle, SidePanelWidthOf(sidePanels, candidate.ProcessId)))
            .ToList();
    }

    private static void RememberSidePanel(List<(int ProcessId, int Width)> sidePanels, IntPtr window, int processId)
    {
        if (!GetWindowRect(window, out var panel))
        {
            return;
        }

        // Tall and narrow, which is the emulator's toolbar and not, say, a splash screen or a tooltip.
        var panelWidth = panel.Right - panel.Left;
        if (panelWidth > 0 && panelWidth <= WidestSidePanel && panel.Bottom - panel.Top > 2 * panelWidth)
        {
            sidePanels.Add((processId, panelWidth));
        }
    }

    private static int SidePanelWidthOf(List<(int ProcessId, int Width)> sidePanels, int processId)
    {
        var width = 0;
        foreach (var panel in sidePanels)
        {
            if (panel.ProcessId == processId && panel.Width > width)
            {
                width = panel.Width;
            }
        }

        return width;
    }

    private static long GetWindowLong(IntPtr window, int index) => GetWindowLongPtr(window, index).ToInt64();

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr parameter);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr window);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr window, out int processId);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr window);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr window, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr window, uint command);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr(IntPtr window, int index);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr window, out Rectangle bounds);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr window, uint command);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

    [DllImport("user32.dll")]
    private static extern bool SystemParametersInfo(uint action, uint parameter, out Rectangle workArea, uint winIni);

    private delegate bool EnumWindowsProc(IntPtr window, IntPtr parameter);

    /// <summary>
    /// A window to tile, together with the width of any panel of its own that rides outside its frame.
    /// </summary>
    private readonly record struct TiledWindow(IntPtr Handle, int SidePanelWidth);

    [StructLayout(LayoutKind.Sequential)]
    private struct Rectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
