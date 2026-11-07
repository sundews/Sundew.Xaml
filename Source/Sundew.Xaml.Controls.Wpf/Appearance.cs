// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Appearance.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System.Windows;
using System.Windows.Media;

/// <summary>
/// Provides attached properties for how an element appears.
/// </summary>
/// <remarks>
/// <para>
/// The visibility here is the framework's own with one state added, <see cref="Visibility.Invisible"/>, for an
/// element that is to draw nothing and still be there in full. WPF has the thing it is said with:
/// <see cref="UIElement.OpacityMask"/> is applied after an element has drawn itself and is no part of hit testing
/// or of automation, so a mask with no alpha in it leaves the element laid out where it was, taking the mouse,
/// focusable, and answering UI automation, with nothing of it seen. That is what tells
/// <see cref="Visibility.Invisible"/> apart from <see cref="Visibility.Hidden"/>, which keeps the space
/// too but is neither drawn nor interactive.
/// </para>
/// <para>
/// One thing of an invisible element is still drawn, and it is not the mask failing. WPF puts
/// <see cref="FrameworkElement.FocusVisualStyle"/> in the adorner layer, which is not the element's own visual and
/// so is not under its opacity mask, so an invisible element that takes keyboard focus draws its focus rectangle
/// around nothing. Read it as the framework pointing out that something reachable is being kept unseen: an element
/// made invisible until it is reached for wants its trigger to name keyboard focus beside the mouse, and then the
/// rectangle arrives with the rest of the element rather than on its own.
/// </para>
/// <para>
/// <see cref="Visibility.Visible"/>, <see cref="Visibility.Hidden"/> and
/// <see cref="Visibility.Collapsed"/> are the matching <see cref="System.Windows.Visibility"/> and nothing more, so an
/// element asked for one of those behaves exactly as it would had its own property been set.
/// </para>
/// <para>
/// This takes over the two properties it says itself with. While an element is invisible its
/// <see cref="UIElement.OpacityMask"/> is a local value of this one's, and leaving that state clears it rather than
/// putting back a mask of the element's own, so an element with an opacity mask already set is not one to make
/// invisible this way. <see cref="UIElement.Visibility"/> is likewise set directly, which overrides a style or a
/// binding on it: an element is told how to be seen in one of the two ways, not both.
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
        new FrameworkPropertyMetadata(Visibility.Visible, OnVisibilityChanged));

    /// <summary>
    /// The mask an element draws nothing through. A transparent brush has no alpha anywhere, so the whole of what
    /// the element drew is masked away.
    /// </summary>
    private static readonly Brush MaskBrush = Brushes.Transparent;

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
        if (dependencyPropertyChangedEventArgs.OldValue is Visibility.Invisible)
        {
            element.ClearValue(UIElement.OpacityMaskProperty);
        }

        switch (dependencyPropertyChangedEventArgs.NewValue)
        {
            case Visibility.Hidden:
                element.Visibility = System.Windows.Visibility.Hidden;
                break;
            case Visibility.Collapsed:
                element.Visibility = System.Windows.Visibility.Collapsed;
                break;
            case Visibility.Invisible:
                element.Visibility = System.Windows.Visibility.Visible;
                element.OpacityMask = MaskBrush;
                break;
            default:
                element.Visibility = System.Windows.Visibility.Visible;
                break;
        }
    }
}
