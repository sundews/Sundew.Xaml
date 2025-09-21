namespace Sundew.Xaml.Controls.Overlays;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

[TemplatePart(Name = PART_Border, Type = typeof(Border))]
public class SubtitleOverlay : OverlayWindow
{
    private const string PART_Border = "PART_Border";
    private const string AnimateSizeStoryboard = nameof(AnimateSizeStoryboard);

    public static readonly DependencyProperty CornerRadiusProperty;
    public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(SubtitleOverlay));
    public static readonly DependencyProperty IsSizeAnimationEnabledProperty = DependencyProperty.Register(nameof(IsSizeAnimationEnabled), typeof(bool), typeof(SubtitleOverlay), new FrameworkPropertyMetadata(false, IsAnimationEnabledChanged));

    private EventTrigger? sizeAnimationEventTrigger;

    static SubtitleOverlay()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SubtitleOverlay), new FrameworkPropertyMetadata(typeof(SubtitleOverlay)));
        CornerRadiusProperty = Border.CornerRadiusProperty.AddOwner(typeof(SubtitleOverlay));
    }

    public SubtitleOverlay()
    {
    }

    public SubtitleOverlay(Window ownerWindow)
        : base(ownerWindow)
    {
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)this.GetValue(CornerRadiusProperty);
        set => this.SetValue(CornerRadiusProperty, value);
    }

    public string? Subtitle
    {
        get => (string?)this.GetValue(SubtitleProperty);
        set => this.SetValue(SubtitleProperty, value);
    }

    public bool IsSizeAnimationEnabled
    {
        get => (bool)GetValue(IsSizeAnimationEnabledProperty);
        set => this.SetValue(IsSizeAnimationEnabledProperty, value);
    }

    public override void OnApplyTemplate()
    {
        this.SetAnimation(this.IsSizeAnimationEnabled);
        base.OnApplyTemplate();
    }

    private static void IsAnimationEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SubtitleOverlay subtitleOverlay)
        {
            subtitleOverlay.SetAnimation(subtitleOverlay.IsSizeAnimationEnabled);
        }
    }

    private void SetAnimation(bool isAnimationEnabled)
    {
        var border = GetTemplateChild(PART_Border);
        if (border is not Border animatedBorder)
        {
            return;
        }

        if (isAnimationEnabled)
        {
            var animateSizeStoryboard = this.TryFindStoryboard(AnimateSizeStoryboard);
            if (animateSizeStoryboard == null)
            {
                return;
            }

            this.sizeAnimationEventTrigger = new EventTrigger(FrameworkElement.SizeChangedEvent)
            {
                Actions = { new BeginStoryboard { Storyboard = animateSizeStoryboard } }
            };
            animatedBorder.Triggers.Add(this.sizeAnimationEventTrigger);
        }
        else if (this.sizeAnimationEventTrigger != null)
        {
            animatedBorder.Triggers.Remove(this.sizeAnimationEventTrigger);
            Dispatcher.BeginInvoke(new Action(() =>
            {
                animatedBorder.BeginAnimation(FrameworkElement.WidthProperty, null);
                animatedBorder.BeginAnimation(FrameworkElement.HeightProperty, null);
                animatedBorder.ClearValue(WidthProperty);
                animatedBorder.ClearValue(HeightProperty);
            }));
        }
    }

    private Storyboard? TryFindStoryboard(string storyboardName)
    {
        try
        {
            if (this.Template?.Resources[storyboardName] is Storyboard storyboard)
            {
                return storyboard;
            }

            return this.TryFindResource(storyboardName) as Storyboard;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Could not find storyboard '{storyboardName}': {ex.Message}");
            return null;
        }
    }
}