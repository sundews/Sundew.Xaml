// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Appearance.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using Microsoft.UI.Xaml;

/// <summary>
/// Provides attached properties for how an element appears.
/// </summary>
/// <remarks>
/// <para>
/// The visibility here is the framework's own with one state added, <see cref="Visibility.Invisible"/>, for an
/// element that is to draw nothing and still be there in full. Neither WinUI nor Uno has an opacity mask on an
/// element to say it with, the way WPF and Avalonia do, so it is said with <see cref="UIElement.Opacity"/> instead,
/// which for a mask with no alpha anywhere in it comes to the same picture and costs nothing. Opacity is no part of
/// hit testing and no part of what the automation tree holds, so the element is laid out where it was, takes the
/// pointer, is focusable, and answers UI automation, with nothing of it seen. A mask that is not empty is another
/// matter, and the one this library draws, <c>Overlays.RevealMask</c>, is said in composition.
/// </para>
/// <para>
/// WinUI says visibility with two states rather than three, and a collapsed element is not laid out, so
/// <see cref="Visibility.Visible"/> and <see cref="Visibility.Collapsed"/> are the matching
/// <see cref="Microsoft.UI.Xaml.Visibility"/> and nothing more. There is nothing for
/// <see cref="Visibility.Hidden"/> to be, so it is said the way it would be written by hand: the element stays
/// visible, and so keeps the space it was laid
/// out into, drawn at no opacity and taken out of hit testing. One thing does not follow: WPF leaves a hidden
/// element out of what UI automation reads as on screen, and here it is not, so
/// <see cref="Visibility.Invisible"/> is the state to ask for when being read matters and
/// <see cref="Visibility.Hidden"/> the one to ask for when only the layout does.
/// </para>
/// <para>
/// This takes over the properties it says itself with. While an element is invisible or hidden its
/// <see cref="UIElement.Opacity"/>, and its <see cref="UIElement.IsHitTestVisible"/> where it is hidden, are local
/// values of this one's, and leaving that state clears them rather than putting back values of the element's own,
/// so an element whose opacity is set or animated elsewhere is not one to make invisible this way.
/// <see cref="UIElement.Visibility"/> is likewise set directly, which overrides a style or a binding on it: an
/// element is told how to be seen in one of the two ways, not both.
/// </para>
/// </remarks>
public static class Appearance
{
    /// <summary>
    /// Identifies the Visibility attached property, which says how an element is to be seen.
    /// </summary>
    public static readonly DependencyProperty VisibilityProperty = DependencyProperty.RegisterAttached(
        "Visibility",
        typeof(Visibility),
        typeof(Appearance),
        new PropertyMetadata(Visibility.Visible, OnVisibilityChanged));

    /// <summary>
    /// The opacity an element draws nothing at, which is what an empty opacity mask would have come to.
    /// </summary>
    private const double InvisibleOpacity = 0d;

    /// <summary>
    /// Gets how the element is to be seen.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>The visibility.</returns>
    public static Visibility GetVisibility(UIElement element)
    {
        return (Visibility)element.GetValue(VisibilityProperty);
    }

    /// <summary>
    /// Sets how the element is to be seen.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="elementVisibility">The visibility.</param>
    public static void SetVisibility(UIElement element, Visibility elementVisibility)
    {
        element.SetValue(VisibilityProperty, elementVisibility);
    }

    private static void OnVisibilityChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
        if (dependencyObject is not UIElement element)
        {
            return;
        }

        // The old value says what was taken over and so what there is to give back, which is why no state of our
        // own is kept. Clearing rather than restoring is the whole of what giving back means here, as said above.
        switch (dependencyPropertyChangedEventArgs.OldValue)
        {
            case Visibility.Hidden:
                element.ClearValue(UIElement.OpacityProperty);
                element.ClearValue(UIElement.IsHitTestVisibleProperty);
                break;
            case Visibility.Invisible:
                element.ClearValue(UIElement.OpacityProperty);
                break;
        }

        switch (dependencyPropertyChangedEventArgs.NewValue)
        {
            case Visibility.Hidden:
                element.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                element.Opacity = InvisibleOpacity;
                element.IsHitTestVisible = false;
                break;
            case Visibility.Collapsed:
                element.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                break;
            case Visibility.Invisible:
                element.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                element.Opacity = InvisibleOpacity;
                break;
            default:
                element.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                break;
        }
    }
}
