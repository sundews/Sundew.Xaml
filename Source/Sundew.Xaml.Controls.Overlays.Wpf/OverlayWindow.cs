namespace Sundew.Xaml.Controls.Overlays;

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public class OverlayWindow : Window
{
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x80000;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(nint hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(nint hwnd, int index, int newStyle);

    private readonly Window? ownerWindow;

    public OverlayWindow()
    {
        this.ConfigureWindow();
    }

    public OverlayWindow(Window ownerWindow)
    {
        this.ownerWindow = ownerWindow;
        this.ownerWindow.Closed += this.OnOwnerWindowClosed;
        this.ConfigureWindow();
    }

    private void ConfigureWindow()
    {
        this.WindowState = WindowState.Maximized;
        this.WindowStyle = WindowStyle.None;
        this.ResizeMode = ResizeMode.NoResize;

        this.AllowsTransparency = true;

        this.Topmost = true;

        this.ShowInTaskbar = false;

        this.SourceInitialized += this.OnSourceInitialized;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        WindowInteropHelper helper = new WindowInteropHelper(this);
        var hwnd = helper.Handle;
        var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
    }

    private void OnOwnerWindowClosed(object? sender, EventArgs e)
    {
        this.ownerWindow?.Closed -= this.OnOwnerWindowClosed;
        this.Close();
    }
}