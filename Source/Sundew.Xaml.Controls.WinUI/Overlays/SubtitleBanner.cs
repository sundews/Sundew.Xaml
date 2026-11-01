// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubtitleBanner.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Overlays;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Foundation;

/// <summary>
/// The subtitle itself: a pill of text that sizes itself to what it says.
/// </summary>
/// <remarks>
/// WPF puts this in the overlay window's own control template, which has no counterpart here: the WinUI overlay is
/// not a framework element and so has neither a style nor a template, and a WinUI <c>Window</c>, which it cannot be
/// either, is not even a dependency object. The subtitle is therefore a control of its own, which
/// <see cref="SubtitleOverlay"/> hosts, and which is just as usable placed in a page.
/// </remarks>
[TemplatePart(Name = PART_Border, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PART_Content, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PART_Subtitle, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PART_RevealHost, Type = typeof(FrameworkElement))]
public partial class SubtitleBanner : Control
{
    /// <summary>
    /// Identifies the <see cref="Subtitle"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
        nameof(Subtitle), typeof(string), typeof(SubtitleBanner), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the <see cref="IsSizeAnimationEnabled"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsSizeAnimationEnabledProperty = DependencyProperty.Register(
        nameof(IsSizeAnimationEnabled), typeof(bool), typeof(SubtitleBanner), new PropertyMetadata(false, OnIsSizeAnimationEnabledChanged));

    /// <summary>
    /// Identifies the <see cref="RevealBrush"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty RevealBrushProperty = DependencyProperty.Register(
        nameof(RevealBrush), typeof(RadialGradientBrush), typeof(SubtitleBanner), new PropertyMetadata(null, OnRevealBrushChanged));

#pragma warning disable SA1310
    private const string PART_Border = "PART_Border";
    private const string PART_Content = "PART_Content";
    private const string PART_Subtitle = "PART_Subtitle";
    private const string PART_RevealHost = "PART_RevealHost";
#pragma warning restore SA1310

    private static readonly TimeSpan SizeAnimationDuration = TimeSpan.FromMilliseconds(200);

    private FrameworkElement? subtitleBorder;
    private FrameworkElement? contentBorder;
    private FrameworkElement? subtitleGroup;
    private FrameworkElement? revealHost;
    private Storyboard? sizeStoryboard;
    private DoubleAnimation? widthAnimation;
    private DoubleAnimation? heightAnimation;
    private RevealMask? revealMask;
    private Point lastCursorPosition;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubtitleBanner"/> class.
    /// </summary>
    public SubtitleBanner()
    {
        this.DefaultStyleKey = typeof(SubtitleBanner);
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
    /// Gets or sets the reveal brush.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A gradient reaching an opaque colour at its edge leaves a hole in the subtitle around the cursor. A brush
    /// stating <see cref="BrushMappingMode.Absolute"/> gives a hole of a fixed size in device independent pixels;
    /// otherwise the radius is read as a fraction of the subtitle.
    /// </para>
    /// <para>
    /// Set from the theme, as in the WPF and Avalonia overlays. Those two say it with an opacity mask on the
    /// element; WinUI has none, so the hole is built out of composition instead, and should that fail the
    /// subtitle is drawn as it was with no hole in it. Set this to null to do without.
    /// </para>
    /// </remarks>
    public RadialGradientBrush? RevealBrush
    {
        get => (RadialGradientBrush?)this.GetValue(RevealBrushProperty);
        set => this.SetValue(RevealBrushProperty, value);
    }

    /// <summary>
    /// Moves the reveal hole to where the cursor is.
    /// </summary>
    /// <remarks>
    /// Called by the overlay window, which hears about the cursor through a hook because a click through window is
    /// told nothing itself. In a page the control hears it directly and this is not needed.
    /// </remarks>
    /// <param name="positionInBanner">The cursor position, in this control's own coordinates. It is remembered,
    /// so that the hole can be put back where the cursor is once the subtitle has resized out from under it.</param>
    internal void UpdateReveal(Point positionInBanner)
    {
        if (this.revealMask == null || this.subtitleGroup == null)
        {
            return;
        }

        this.lastCursorPosition = positionInBanner;
        var origin = this.subtitleGroup.TransformToVisual(this).TransformPoint(default);
        this.revealMask.Update(new Point(positionInBanner.X - origin.X, positionInBanner.Y - origin.Y));
    }

    /// <summary>
    /// Takes hold of the template parts.
    /// </summary>
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (this.contentBorder != null)
        {
            this.contentBorder.SizeChanged -= this.OnContentSizeChanged;
        }

        this.subtitleBorder = this.GetTemplateChild(PART_Border) as FrameworkElement;
        this.contentBorder = this.GetTemplateChild(PART_Content) as FrameworkElement;
        this.subtitleGroup = this.GetTemplateChild(PART_Subtitle) as FrameworkElement;
        this.revealHost = this.GetTemplateChild(PART_RevealHost) as FrameworkElement;

        if (this.contentBorder != null)
        {
            this.contentBorder.SizeChanged += this.OnContentSizeChanged;
        }

        this.SetAnimation(this.IsSizeAnimationEnabled);
        this.UpdateRevealing();
    }

    /// <summary>
    /// Follows the cursor where the control can be pointed at, which is anywhere but a click through overlay.
    /// </summary>
    /// <param name="e">The event args.</param>
    protected override void OnPointerMoved(PointerRoutedEventArgs e)
    {
        base.OnPointerMoved(e);
        this.UpdateReveal(e.GetCurrentPoint(this).Position);
    }

    private static double AvoidNaN(double length)
    {
        return double.IsNaN(length) || double.IsInfinity(length) ? 0 : length;
    }

    private static void OnIsSizeAnimationEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is SubtitleBanner subtitleBanner)
        {
            subtitleBanner.SetAnimation(subtitleBanner.IsSizeAnimationEnabled);
        }
    }

    private static void OnRevealBrushChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is SubtitleBanner subtitleBanner)
        {
            subtitleBanner.UpdateRevealing();
        }
    }

    /// <summary>
    /// Builds, or takes down, the storyboard that eases the subtitle from one size to the next.
    /// </summary>
    /// <remarks>
    /// Width and height are layout properties, which WinUI will not animate unless asked to in so many words,
    /// hence the dependent animation. The from and to are settled each time the measuring border lands on a new
    /// size, because an animation there cannot be bound to anything.
    /// </remarks>
    private void SetAnimation(bool isSizeAnimationEnabled)
    {
        if (this.subtitleBorder == null)
        {
            return;
        }

        this.sizeStoryboard?.Stop();

        if (!isSizeAnimationEnabled)
        {
            this.sizeStoryboard = null;
            this.widthAnimation = null;
            this.heightAnimation = null;

            // The pill carries no content of its own, so without an animation to bring it to a size it has to be
            // given one outright.
            if (this.contentBorder != null)
            {
                this.subtitleBorder.Width = this.contentBorder.ActualWidth;
                this.subtitleBorder.Height = this.contentBorder.ActualHeight;
            }

            return;
        }

        var easingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
        this.widthAnimation = new DoubleAnimation { Duration = SizeAnimationDuration, EnableDependentAnimation = true, EasingFunction = easingFunction };
        this.heightAnimation = new DoubleAnimation { Duration = SizeAnimationDuration, EnableDependentAnimation = true, EasingFunction = easingFunction };
        Storyboard.SetTarget(this.widthAnimation, this.subtitleBorder);
        Storyboard.SetTargetProperty(this.widthAnimation, "Width");
        Storyboard.SetTarget(this.heightAnimation, this.subtitleBorder);
        Storyboard.SetTargetProperty(this.heightAnimation, "Height");
        this.sizeStoryboard = new Storyboard { Children = { this.widthAnimation, this.heightAnimation } };

        if (this.contentBorder != null)
        {
            this.AnimateTo(new Size(this.contentBorder.ActualWidth, this.contentBorder.ActualHeight));
        }
    }

    private void OnContentSizeChanged(object sender, SizeChangedEventArgs e)
    {
        this.SizePillTo(e.NewSize);
        this.UpdateReveal(this.lastCursorPosition);
    }

    /// <summary>
    /// Brings the pill to the size the subtitle wants, easing into it where that was asked for and arriving at
    /// once where it was not.
    /// </summary>
    /// <param name="size">The size.</param>
    private void SizePillTo(Size size)
    {
        if (this.subtitleBorder == null || size.Width <= 0 || size.Height <= 0)
        {
            return;
        }

        if (this.sizeStoryboard == null)
        {
            this.subtitleBorder.Width = size.Width;
            this.subtitleBorder.Height = size.Height;
            return;
        }

        this.AnimateTo(size);
    }

    /// <summary>
    /// Eases the visible border from the size it is at to the size the measuring border has landed on.
    /// </summary>
    /// <remarks>
    /// The from and the to are settled here rather than in the template, because WinUI will not bind an
    /// animation's endpoints to anything.
    /// </remarks>
    /// <param name="size">The size to ease to.</param>
    private void AnimateTo(Size size)
    {
        if (this.subtitleBorder == null || this.sizeStoryboard == null || this.widthAnimation == null || this.heightAnimation == null)
        {
            return;
        }

        if (size.Width <= 0 || size.Height <= 0)
        {
            return;
        }

        this.sizeStoryboard.Stop();
        this.widthAnimation.From = AvoidNaN(this.subtitleBorder.ActualWidth);
        this.widthAnimation.To = size.Width;
        this.heightAnimation.From = AvoidNaN(this.subtitleBorder.ActualHeight);
        this.heightAnimation.To = size.Height;
        this.sizeStoryboard.Begin();
    }

    private void UpdateRevealing()
    {
        this.revealMask?.Dispose();
        this.revealMask = null;

        if (this.RevealBrush is not { } revealBrush || this.subtitleGroup == null || this.revealHost == null)
        {
            return;
        }

        try
        {
            this.revealMask = new RevealMask(this.revealHost, this.subtitleGroup, revealBrush);
        }
        catch (Exception exception)
        {
            // Composition is the only way to a mask here. Losing the hole is worth far less than losing the
            // subtitle, so anything that goes wrong building it leaves the subtitle drawn as it was.
            System.Diagnostics.Debug.WriteLine($"Could not build the reveal mask: {exception.Message}");
            this.revealMask = null;
            this.subtitleGroup.Opacity = 1;
        }
    }
}
