// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubtitleOverlay.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

/// <summary>
/// A subtitle overlay window.
/// </summary>
[TemplatePart(Name = PART_Border, Type = typeof(Border))]
public class SubtitleOverlay : OverlayWindow
{
    /// <summary>
    /// Identifies the <see cref="CornerRadius"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty;

    /// <summary>
    /// Identifies the <see cref="Subtitle"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(SubtitleOverlay));

    /// <summary>
    /// Identifies the <see cref="IsSizeAnimationEnabled"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsSizeAnimationEnabledProperty = DependencyProperty.Register(nameof(IsSizeAnimationEnabled), typeof(bool), typeof(SubtitleOverlay), new FrameworkPropertyMetadata(false, IsAnimationEnabledChanged));

#pragma warning disable SA1310
    private const string PART_Border = "PART_Border";
#pragma warning restore SA1310
    private const string AnimateSizeStoryboard = nameof(AnimateSizeStoryboard);

    private EventTrigger? sizeAnimationEventTrigger;

    static SubtitleOverlay()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SubtitleOverlay), new FrameworkPropertyMetadata(typeof(SubtitleOverlay)));
        CornerRadiusProperty = Border.CornerRadiusProperty.AddOwner(typeof(SubtitleOverlay));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubtitleOverlay"/> class.
    /// </summary>
    public SubtitleOverlay()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubtitleOverlay"/> class.
    /// </summary>
    /// <param name="ownerWindow">The owner window.</param>
    public SubtitleOverlay(Window ownerWindow)
        : base(ownerWindow)
    {
    }

    /// <summary>
    /// Gets or sets the corner radius.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)this.GetValue(CornerRadiusProperty);
        set => this.SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the subtitle.
    /// </summary>
    public string? Subtitle
    {
        get => (string?)this.GetValue(SubtitleProperty);
        set => this.SetValue(SubtitleProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether size animation is enabled.
    /// </summary>
    public bool IsSizeAnimationEnabled
    {
        get => (bool)this.GetValue(IsSizeAnimationEnabledProperty);
        set => this.SetValue(IsSizeAnimationEnabledProperty, value);
    }

    /// <summary>
    /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
    /// </summary>
    public override void OnApplyTemplate()
    {
        this.SetAnimation(this.IsSizeAnimationEnabled);
        base.OnApplyTemplate();
    }

    /// <summary>
    /// Gets the target for the reveal effect.
    /// </summary>
    /// <returns>The target.</returns>
    protected override FrameworkElement? GetRevealTarget()
    {
        var border = this.GetTemplateChild(PART_Border);
        return border as FrameworkElement;
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
        var border = this.GetTemplateChild(PART_Border);
        if (border is not FrameworkElement targetFrameworkElement)
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
                Actions = { new BeginStoryboard { Storyboard = animateSizeStoryboard } },
            };
            targetFrameworkElement.Triggers.Add(this.sizeAnimationEventTrigger);
        }
        else if (this.sizeAnimationEventTrigger != null)
        {
            targetFrameworkElement.Triggers.Remove(this.sizeAnimationEventTrigger);
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                targetFrameworkElement.BeginAnimation(FrameworkElement.WidthProperty, null);
                targetFrameworkElement.BeginAnimation(FrameworkElement.HeightProperty, null);
                targetFrameworkElement.ClearValue(WidthProperty);
                targetFrameworkElement.ClearValue(HeightProperty);
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