// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Appearance.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

/// <summary>
/// Provides attached properties for how an element appears.
/// </summary>
/// <remarks>
/// <para>
/// The visibility here is the framework's own with one state added, <see cref="Visibility.Invisible"/>, for an
/// element that is to draw nothing and still be there in full. MAUI has no opacity mask on an element to say it
/// with, the way WPF and Avalonia do, and opacity is the wrong thing to reach for in its place: a view at no
/// opacity is taken off the Apple platforms' accessibility tree and out of their hit testing altogether, which is
/// the one thing this state is for. <see cref="VisualElement.Clip"/> is the thing that does hold: it is a mask the
/// platform applies to what the view drew, by a layer mask on the Apple platforms, a clip path on Android and a
/// composition clip on Windows, none of which the platform asks about when it hit tests or when it walks its
/// accessibility tree. Clipped to nothing, the element is laid out where it was, takes the touch, and is still read
/// out, with nothing of it seen.
/// </para>
/// <para>
/// MAUI says visibility with the one <see cref="VisualElement.IsVisible"/> rather than with three states, and a
/// view that is not visible is not laid out either, which is <see cref="Visibility.Collapsed"/>. There is
/// nothing for <see cref="Visibility.Hidden"/> to be, so it is said the way it would be written by hand: the
/// element stays visible, and so keeps the space it was laid out into, drawn at no opacity and taken out of input.
/// That it also stops being read out on the Apple platforms is what <see cref="Visibility.Hidden"/> is
/// about, and the reason the two states are said with two different properties here.
/// </para>
/// <para>
/// This takes over the properties it says itself with. While an element is invisible its
/// <see cref="VisualElement.Clip"/> is a value of this one's, and while it is hidden its
/// <see cref="VisualElement.Opacity"/> and <see cref="VisualElement.InputTransparent"/> are, and leaving either
/// state clears them rather than putting back values of the element's own, so an element that is clipped already is
/// not one to make invisible this way. <see cref="VisualElement.IsVisible"/> is likewise set directly, which
/// overrides a style or a binding on it: an element is told how to be seen in one of the two ways, not both.
/// </para>
/// </remarks>
public static class Appearance
{
    /// <summary>
    /// Identifies the Visibility attached property, which says how an element is to be seen.
    /// </summary>
    public static readonly BindableProperty VisibilityProperty = BindableProperty.CreateAttached(
        "Visibility",
        typeof(Visibility),
        typeof(Appearance),
        Visibility.Visible,
        propertyChanged: OnVisibilityChanged);

    /// <summary>
    /// The opacity an element draws nothing at.
    /// </summary>
    private const double HiddenOpacity = 0d;

    /// <summary>
    /// Gets how the element is to be seen.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>The visibility.</returns>
    public static Visibility GetVisibility(VisualElement element)
    {
        return (Visibility)element.GetValue(VisibilityProperty);
    }

    /// <summary>
    /// Sets how the element is to be seen.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="elementVisibility">The visibility.</param>
    public static void SetVisibility(VisualElement element, Visibility elementVisibility)
    {
        element.SetValue(VisibilityProperty, elementVisibility);
    }

    private static void OnVisibilityChanged(BindableObject bindableObject, object oldValue, object newValue)
    {
        if (bindableObject is not VisualElement element)
        {
            return;
        }

        // The old value says what was taken over and so what there is to give back, which is why no state of our
        // own is kept. Clearing rather than restoring is the whole of what giving back means here, as said above.
        switch (oldValue)
        {
            case Visibility.Hidden:
                element.ClearValue(VisualElement.OpacityProperty);
                element.ClearValue(VisualElement.InputTransparentProperty);
                break;
            case Visibility.Invisible:
                element.ClearValue(VisualElement.ClipProperty);
                break;
        }

        switch (newValue)
        {
            case Visibility.Hidden:
                element.IsVisible = true;
                element.Opacity = HiddenOpacity;
                element.InputTransparent = true;
                break;
            case Visibility.Collapsed:
                element.IsVisible = false;
                break;
            case Visibility.Invisible:
                element.IsVisible = true;

                // A clip is an element of the tree in its own right and is given the element it clips as its
                // parent, so each invisible element is handed one of its own rather than all of them one shared.
                element.Clip = new RectangleGeometry(Rect.Zero);
                break;
            default:
                element.IsVisible = true;
                break;
        }
    }
}
