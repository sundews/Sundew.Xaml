namespace Sundew.Xaml.Wpf.Tester;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Sundew.Xaml.Controls.Overlays;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly DispatcherTimer dispatcherTimer;

    public MainWindow()
    {
        this.InitializeComponent();
        var overlayDemo = new SubtitleOverlay(this) { DataContext = new SubtitleDemo(), IsSizeAnimationEnabled = true, CornerRadius = new CornerRadius(64), FontSize = 64 };
        var binding = new Binding(nameof(SubtitleDemo.CurrentText));
        overlayDemo.SetBinding(SubtitleOverlay.SubtitleProperty, binding);
        overlayDemo.Show();
        this.dispatcherTimer = new DispatcherTimer(TimeSpan.FromSeconds(9), DispatcherPriority.DataBind,
            (sender, args) => overlayDemo.IsSizeAnimationEnabled = !overlayDemo.IsSizeAnimationEnabled, Dispatcher.CurrentDispatcher);
        this.Closed += (s, e) => this.dispatcherTimer.Stop();
    }
}

public class SubtitleDemo : INotifyPropertyChanged
{
    private readonly DispatcherTimer timer;
    private int messageIndex = 0;

    public SubtitleDemo()
    {
        this.CurrentText = string.Empty;
        var messages = new[]
        {
            @"This is a subtitle demo
 It supports multiple lines
 and aligns across different resolutions",
            "It is intended to show how to use the SubtitleOverlay",
            "The SubtitleOverlay is a window that is always on top",
            "And it is transparent to mouse events",
            "This makes it possible to create overlays that do not interfere with the underlying application",
            "This can be used for subtitles, notifications, or other overlays",
            @"The SubtitleOverlay is implemented using P/Invoke to set the WS_EX_TRANSPARENT style
This style makes the window transparent to mouse events",
            "You can click through the window to interact with the underlying application",
            "This demo changes the subtitle every 3000 milliseconds",
            "You can customize the appearance of the subtitles using styles and templates",
            "In the demo every 9 seconds it toggles animating the window size on and off",
            "Feel free to modify this demo to suit your needs",
        };

        this.timer = new DispatcherTimer(
        TimeSpan.FromMilliseconds(3000),
        DispatcherPriority.DataBind,
        (sender, args) =>
        {
            if (this.messageIndex >= messages.Length)
            {
                this.messageIndex = 0;
            }

            this.CurrentText = messages[this.messageIndex++];
        },
        Dispatcher.CurrentDispatcher);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string CurrentText
    {
        get => field;
        set => this.SetField(ref field, value);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}