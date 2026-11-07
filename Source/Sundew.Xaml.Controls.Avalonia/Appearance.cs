// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Appearance.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using global::Avalonia;
using global::Avalonia.Input;
using global::Avalonia.Media;

/// <summary>
/// Provides attached properties for how an element appears.
/// </summary>
/// <remarks>
/// <para>
/// The visibility here is the framework's own with one state added, <see cref="Visibility.Invisible"/>, for an
/// element that is to draw nothing and still be there in full. Avalonia has the thing it is said with:
/// <see cref="Visual.OpacityMask"/> is applied after a visual has drawn itself and is no part of hit testing or of
/// automation, so a mask with no alpha in it leaves the element laid out where it was, taking the mouse, focusable,
/// and answering UI automation, with nothing of it seen.
/// </para>
/// <para>
/// Avalonia says visibility with the one <see cref="Visual.IsVisible"/> rather than with three states, and a visual
/// that is not visible is not laid out either, which is <see cref="Visibility.Collapsed"/>. There is nothing
/// for <see cref="Visibility.Hidden"/> to be, so it is said the way it would be written by hand: the element
/// stays visible, and so keeps the space it was laid out into, drawn through the same empty mask and taken out of
/// hit testing. One thing does not follow: WPF leaves a hidden element out of what UI automation reads as on
/// screen, and here it is not, so <see cref="Visibility.Invisible"/> is the state to ask for when being read
/// matters and <see cref="Visibility.Hidden"/> the one to ask for when only the layout does.
/// </para>
/// <para>
/// This takes over the properties it says itself with. While an element is invisible or hidden its
/// <see cref="Visual.OpacityMask"/>, and its <see cref="InputElement.IsHitTestVisible"/> where it is hidden, are
/// local values of this one's, and leaving that state clears them rather than putting back values of the element's
/// own, so an element with an opacity mask already set is not one to make invisible this way.
/// <see cref="Visual.IsVisible"/> is likewise set directly, which overrides a style or a binding on it: an element
/// is told how to be seen in one of the two ways, not both.
/// </para>
/// </remarks>
public static class Appearance
{
    /// <summary>
    /// Identifies the Visibility attached property, which says how an element is to be seen.
    /// </summary>
    public static readonly AttachedProperty<Visibility> VisibilityProperty =
        AvaloniaProperty.RegisterAttached<Visual, Visibility>("Visibility", typeof(Appearance));

    /// <summary>
    /// The mask an element draws nothing through. A transparent brush has no alpha anywhere, so the whole of what
    /// the element drew is masked away.
    /// </summary>
    private static readonly IBrush MaskBrush = Brushes.Transparent;

    static Appearance()
    {
        VisibilityProperty.Changed.AddClassHandler<Visual, Visibility>(OnVisibilityChanged);
    }

    /// <summary>
    /// Gets how the visual is to be seen.
    /// </summary>
    /// <param name="visual">The visual.</param>
    /// <returns>The visibility.</returns>
    public static Visibility GetVisibility(Visual visual)
    {
        return visual.GetValue(VisibilityProperty);
    }

    /// <summary>
    /// Sets how the visual is to be seen.
    /// </summary>
    /// <param name="visual">The visual.</param>
    /// <param name="elementVisibility">The visibility.</param>
    public static void SetVisibility(Visual visual, Visibility elementVisibility)
    {
        visual.SetValue(VisibilityProperty, elementVisibility);
    }

    private static void OnVisibilityChanged(Visual visual, AvaloniaPropertyChangedEventArgs<Visibility> avaloniaPropertyChangedEventArgs)
    {
        // The old value says what was taken over and so what there is to give back, which is why no state of our
        // own is kept. Clearing rather than restoring is the whole of what giving back means here, as said above.
        switch (avaloniaPropertyChangedEventArgs.OldValue.GetValueOrDefault())
        {
            case Visibility.Hidden:
                visual.ClearValue(Visual.OpacityMaskProperty);
                if (visual is InputElement wasHiddenInputElement)
                {
                    wasHiddenInputElement.ClearValue(InputElement.IsHitTestVisibleProperty);
                }

                break;
            case Visibility.Invisible:
                visual.ClearValue(Visual.OpacityMaskProperty);
                break;
        }

        switch (avaloniaPropertyChangedEventArgs.NewValue.GetValueOrDefault())
        {
            case Visibility.Hidden:
                visual.IsVisible = true;
                visual.OpacityMask = MaskBrush;

                // Hit testing is said on the input element rather than on the visual, and not every visual is one.
                if (visual is InputElement inputElement)
                {
                    inputElement.IsHitTestVisible = false;
                }

                break;
            case Visibility.Collapsed:
                visual.IsVisible = false;
                break;
            case Visibility.Invisible:
                visual.IsVisible = true;
                visual.OpacityMask = MaskBrush;
                break;
            default:
                visual.IsVisible = true;
                break;
        }
    }
}
