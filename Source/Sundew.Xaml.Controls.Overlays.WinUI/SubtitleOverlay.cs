// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubtitleOverlay.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

/// <summary>
/// A subtitle overlay window.
/// </summary>
/// <remarks>
/// <para>
/// The overlay hosts a <see cref="SubtitleBanner"/>, which is what draws the subtitle and what a style or a
/// template applies to. <see cref="Banner"/> hands it out for that purpose.
/// </para>
/// <para>
/// What the subtitle says is stated here, as dependency properties, so that it can be bound like anything else.
/// What the subtitle looks like is not: those come from the banner's own style, so asking the banner is asking the
/// one that decides, and <see cref="CornerRadius"/> and the rest are handed straight on to it.
/// </para>
/// </remarks>
public class SubtitleOverlay : OverlayWindow
{
    /// <summary>
    /// Identifies the <see cref="Subtitle"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
        nameof(Subtitle), typeof(string), typeof(SubtitleOverlay), new PropertyMetadata(null, OnSubtitleChanged));

    /// <summary>
    /// Identifies the <see cref="IsSizeAnimationEnabled"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsSizeAnimationEnabledProperty = DependencyProperty.Register(
        nameof(IsSizeAnimationEnabled), typeof(bool), typeof(SubtitleOverlay), new PropertyMetadata(false, OnIsSizeAnimationEnabledChanged));

    private readonly SubtitleBanner subtitleBanner = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SubtitleOverlay"/> class.
    /// </summary>
    public SubtitleOverlay()
    {
        this.Initialize();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubtitleOverlay"/> class.
    /// </summary>
    /// <param name="ownerWindow">The owner window.</param>
    public SubtitleOverlay(Window ownerWindow)
        : base(ownerWindow)
    {
        this.Initialize();
    }

    /// <summary>
    /// Gets the banner the subtitle is drawn by, which is what a style or a template applies to.
    /// </summary>
    public SubtitleBanner Banner => this.subtitleBanner;

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
    /// Gets or sets the corner radius.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => this.subtitleBanner.CornerRadius;
        set => this.subtitleBanner.CornerRadius = value;
    }

    /// <summary>
    /// Gets or sets the padding.
    /// </summary>
    public Thickness Padding
    {
        get => this.subtitleBanner.Padding;
        set => this.subtitleBanner.Padding = value;
    }

    /// <summary>
    /// Gets or sets the font size.
    /// </summary>
    public double FontSize
    {
        get => this.subtitleBanner.FontSize;
        set => this.subtitleBanner.FontSize = value;
    }

    /// <summary>
    /// Gets or sets the foreground.
    /// </summary>
    public Brush Foreground
    {
        get => this.subtitleBanner.Foreground;
        set => this.subtitleBanner.Foreground = value;
    }

    /// <summary>
    /// Gets or sets the reveal brush.
    /// </summary>
    public RadialGradientBrush? RevealBrush
    {
        get => this.subtitleBanner.RevealBrush;
        set => this.subtitleBanner.RevealBrush = value;
    }

    /// <summary>
    /// Gets or sets the data context the subtitle is bound against.
    /// </summary>
    public object? DataContext
    {
        get => this.subtitleBanner.DataContext;
        set => this.subtitleBanner.DataContext = value;
    }

    /// <summary>
    /// Keeps the subtitle within the work area, which is what it is allowed to grow to.
    /// </summary>
    protected override void OnMonitorBoundsChanged()
    {
        base.OnMonitorBoundsChanged();
        this.subtitleBanner.MaxWidth = this.MonitorWidth;
        this.subtitleBanner.MaxHeight = this.MonitorHeight;
    }

    /// <summary>
    /// Moves the reveal hole to where the cursor is.
    /// </summary>
    /// <param name="position">The position.</param>
    protected override void OnCursorMoved(Point position)
    {
        base.OnCursorMoved(position);
        this.subtitleBanner.UpdateReveal(position);
    }

    private static void OnSubtitleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is SubtitleOverlay subtitleOverlay)
        {
            subtitleOverlay.subtitleBanner.Subtitle = (string?)e.NewValue;
        }
    }

    private static void OnIsSizeAnimationEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is SubtitleOverlay subtitleOverlay)
        {
            subtitleOverlay.subtitleBanner.IsSizeAnimationEnabled = (bool)e.NewValue;
        }
    }

    private void Initialize()
    {
        // Subscribed before the banner is handed over, because handing it over is what loads it, and a handler
        // added afterwards would have missed it.
        this.subtitleBanner.Loaded += this.OnBannerLoaded;
        this.Content = this.subtitleBanner;
        this.subtitleBanner.MaxWidth = this.MonitorWidth;
        this.subtitleBanner.MaxHeight = this.MonitorHeight;
        if (this.subtitleBanner.IsLoaded)
        {
            this.OnBannerLoaded(this.subtitleBanner, new RoutedEventArgs());
        }
    }

    /// <summary>
    /// Settles what can only be known once the banner is loaded, which is when its style has been applied.
    /// </summary>
    /// <remarks>
    /// Whether the cursor has to be followed is one such thing: it is followed only to move the reveal, and the
    /// reveal brush comes from the style. Doing this twice is harmless, which is what lets it be called for a
    /// banner that had already loaded by the time anything could hear about it.
    /// </remarks>
    private void OnBannerLoaded(object sender, RoutedEventArgs e)
    {
        this.UpdateMonitorBounds();
        if (this.subtitleBanner.RevealBrush != null)
        {
            this.StartCursorTracking();
        }
    }
}
