// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubtitleOverlay.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using global::Avalonia;
using global::Avalonia.Animation;
using global::Avalonia.Animation.Easings;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Data;

/// <summary>
/// A subtitle overlay window.
/// </summary>
/// <remarks>
/// The control theme lives in this library's Themes/Generic.axaml, which the application merges into its own
/// resources. Avalonia has no implicit per assembly dictionary the way WPF has, and merging it from here in code
/// would load it by reflection, which a head compiled ahead of time cannot do.
/// </remarks>
public class SubtitleOverlay : OverlayWindow
{
    /// <summary>
    /// Identifies the <see cref="Subtitle"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<SubtitleOverlay, string?>(nameof(Subtitle));

    /// <summary>
    /// Identifies the <see cref="IsSizeAnimationEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSizeAnimationEnabledProperty =
        AvaloniaProperty.Register<SubtitleOverlay, bool>(nameof(IsSizeAnimationEnabled));

#pragma warning disable SA1310
    private const string PART_Border = "PART_Border";
    private const string PART_MeasureBorder = "PART_MeasureBorder";
#pragma warning restore SA1310

    private static readonly TimeSpan SizeAnimationDuration = TimeSpan.FromMilliseconds(200);

    private Control? subtitleBorder;
    private Control? measureBorder;
    private bool isFollowingMeasureBorder;

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
    /// Gets or sets the subtitle.
    /// </summary>
    public string? Subtitle
    {
        get => this.GetValue(SubtitleProperty);
        set => this.SetValue(SubtitleProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether size animation is enabled.
    /// </summary>
    public bool IsSizeAnimationEnabled
    {
        get => this.GetValue(IsSizeAnimationEnabledProperty);
        set => this.SetValue(IsSizeAnimationEnabledProperty, value);
    }

    /// <summary>
    /// Gets the type the control theme is looked up by, which is this one rather than <see cref="Window"/>.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(SubtitleOverlay);

    /// <summary>
    /// Called when the template has been applied.
    /// </summary>
    /// <param name="e">The event args.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        this.subtitleBorder = e.NameScope.Find<Control>(PART_Border);
        this.measureBorder = e.NameScope.Find<Control>(PART_MeasureBorder);
        this.SetAnimation(this.IsSizeAnimationEnabled);
        base.OnApplyTemplate(e);
    }

    /// <summary>
    /// Called when a property has changed.
    /// </summary>
    /// <param name="change">The change.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsSizeAnimationEnabledProperty)
        {
            this.SetAnimation(this.IsSizeAnimationEnabled);
        }
    }

    /// <summary>
    /// Gets the target for the reveal effect.
    /// </summary>
    /// <returns>The target.</returns>
    protected override Visual? GetRevealTarget()
    {
        return this.subtitleBorder;
    }

    /// <summary>
    /// Carries the size the subtitle wants over to the border that is seen, which eases into it.
    /// </summary>
    private void OnMeasureBorderPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty)
        {
            this.SizeToMeasureBorder();
        }
    }

    private void SizeToMeasureBorder()
    {
        if (this.subtitleBorder == null || this.measureBorder == null)
        {
            return;
        }

        var bounds = this.measureBorder.Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        this.subtitleBorder.Width = bounds.Width;
        this.subtitleBorder.Height = bounds.Height;
    }

    /// <summary>
    /// Turns the size animation on or off.
    /// </summary>
    /// <remarks>
    /// The measuring border is laid out at the size the subtitle wants, and the visible border is given that size
    /// through a binding carrying a transition, so that it eases into each new size rather than jumping. With the
    /// animation off the binding goes away and the border is back to sizing itself, which is instant.
    /// </remarks>
    /// <param name="isSizeAnimationEnabled">A value indicating whether size animation is enabled.</param>
    private void SetAnimation(bool isSizeAnimationEnabled)
    {
        if (this.subtitleBorder == null || this.measureBorder == null)
        {
            return;
        }

        if (this.isFollowingMeasureBorder)
        {
            this.measureBorder.PropertyChanged -= this.OnMeasureBorderPropertyChanged;
            this.isFollowingMeasureBorder = false;
        }

        if (isSizeAnimationEnabled)
        {
            this.subtitleBorder.Transitions = new Transitions
            {
                new DoubleTransition { Property = WidthProperty, Duration = SizeAnimationDuration, Easing = new QuadraticEaseOut() },
                new DoubleTransition { Property = HeightProperty, Duration = SizeAnimationDuration, Easing = new QuadraticEaseOut() },
            };

            // Followed by hand rather than bound. A binding naming its path as text is resolved by reflection,
            // which a head compiled ahead of time has no way to do, and this says the same thing without it.
            this.measureBorder.PropertyChanged += this.OnMeasureBorderPropertyChanged;
            this.isFollowingMeasureBorder = true;
            this.SizeToMeasureBorder();
        }
        else
        {
            // Cleared before the values are, so that nothing eases toward the unset size.
            this.subtitleBorder.Transitions = null;
            this.subtitleBorder.ClearValue(WidthProperty);
            this.subtitleBorder.ClearValue(HeightProperty);
        }
    }
}
