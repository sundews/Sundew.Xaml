// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Visibility.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

/// <summary>
/// Says how an element is to be seen.
/// </summary>
/// <remarks>
/// The first three states are what a framework's own visibility says, and the fourth is the one it has no word for:
/// an element that draws nothing yet is still there in full, laid out where it was, taking the mouse and answering
/// UI automation. This is the type of the <see cref="Appearance"/> visibility, which is told to each framework in
/// whichever of its own terms comes closest. <see cref="Appearance"/> says per platform exactly how.
/// </remarks>
public enum Visibility
{
    /// <summary>
    /// The element is laid out and drawn.
    /// </summary>
    Visible,

    /// <summary>
    /// The element keeps the space it was laid out into but is neither drawn nor interactive.
    /// </summary>
    Hidden,

    /// <summary>
    /// The element is neither laid out nor drawn, and gives up the space it would have taken.
    /// </summary>
    Collapsed,

    /// <summary>
    /// The element keeps the space it was laid out into and nothing of it is drawn, and it is otherwise untouched:
    /// it still takes the mouse, is still focusable, and is still found by UI automation. Being found is what tells
    /// this apart from <see cref="Hidden"/>, which is not drawn either but is out of reach.
    /// </summary>
    Invisible,
}
