// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThemeChangedEventArgs.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Theming;

using System;
using Sundew.DiscriminatedUnions;

/// <summary>Event args for the theme changed event.</summary>
[DiscriminatedUnion]
public abstract partial class ThemeUpdatedEventArgs : EventArgs
{
}