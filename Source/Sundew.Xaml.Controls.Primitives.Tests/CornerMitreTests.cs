// ----------------------------------------------------------------------------------------------------------------------
// <copyright file="CornerMitreTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Primitives.Tests;

using System.Collections.Generic;
using AwesomeAssertions;
using TUnit.Core;

public class CornerMitreTests
{
    [Test]
    public void GetEnumerator_When_TheCornerIsSquare_Then_ThereShouldBeOneSliceOfEachSide()
    {
        var slices = Mitre(Square());

        // No arc means nothing for a blend to follow, so the two sides meet along the mitre instead of sweeping.
        slices.Count.Should().Be(2);
    }

    [Test]
    public void GetEnumerator_When_TheCornerIsSquare_Then_EachSideShouldTakeOneFlatColour()
    {
        var slices = Mitre(Square());

        // Both ends of a slice take their colour from the same place, which is what makes each side flat and the
        // join between them a hard line rather than a fade.
        slices[0].From.Should().Be(0);
        slices[0].To.Should().Be(0);
        slices[1].From.Should().Be(1);
        slices[1].To.Should().Be(1);
    }

    [Test]
    public void GetEnumerator_When_TheCornerIsSquare_Then_TheStopsShouldMakeNoDifference()
    {
        var square = Square();

        var gradual = Mitre(square, new Stops(0.2, 0.8));
        var abrupt = Mitre(square, new Stops(0.5, 0.5));

        // There is no distance across a square corner for a transition to be gradual over, so the stops have
        // nothing to say about it.
        gradual.Should().BeEquivalentTo(abrupt);
    }

    [Test]
    public void GetEnumerator_When_TheCornerIsSquare_Then_TheTwoSidesShouldMeetAlongTheMitre()
    {
        var square = Square();

        var slices = Mitre(square);

        // The first side runs from the cut it shares with the side entering the corner to the corner of the box,
        // the second from there to the cut it shares with the side leaving, and both close on the inside of the
        // corner. The line between them is the mitre.
        slices[1].Filled.Outer.Start.Should().Be(square.Anchor);
        slices[1].Filled.Outer.End.Should().Be(square.Outer.End);
        slices[0].Filled.Outer.Start.Should().Be(square.Outer.Start);
        slices[0].Filled.Inner.Start.Should().Be(square.Inner.Start);
    }

    [Test]
    public void GetEnumerator_When_TheCornerIsSquare_Then_TheFirstSideShouldRunPastTheMitre()
    {
        var square = Square();

        var slices = Mitre(square);

        // The second side paints over the overrun, so the background cannot show through the hard line.
        slices[0].Filled.Outer.End.X.Should().BeGreaterThan(square.Anchor.X);
        slices[0].Filled.Outer.Control.Should().Be(square.Anchor);
    }

    [Test]
    public void GetEnumerator_When_TheCornerIsRounded_Then_ItShouldStillBeSwept()
    {
        var rounded = BorderLayoutCalculator
            .CalculateBorder(new BorderMetrics(100, 50, new Sides<double>(6), new Corners<double>(20), Scale.Identity))
            .Corners.TopLeft;

        var slices = Mitre(rounded);

        rounded.IsSquare.Should().BeFalse();
        slices.Count.Should().BeGreaterThan(2);
    }

    private static CornerWedge Square()
    {
        return BorderLayoutCalculator
            .CalculateBorder(new BorderMetrics(100, 50, new Sides<double>(6, 2, 4, 8), new Corners<double>(0), Scale.Identity))
            .Corners.TopLeft;
    }

    private static IReadOnlyList<CornerSlice> Mitre(CornerWedge wedge, Stops? stops = null)
    {
        var slices = new List<CornerSlice>();
        foreach (var slice in new CornerSweep(wedge, stops ?? new Stops(0.2, 0.8), Scale.Identity))
        {
            slices.Add(slice);
        }

        return slices;
    }
}
