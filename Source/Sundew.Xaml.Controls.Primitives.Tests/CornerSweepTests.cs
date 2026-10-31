// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CornerSweepTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Primitives.Tests;

using System.Collections.Generic;
using AwesomeAssertions;
using TUnit.Core;

public class CornerSweepTests
{
    [Test]
    public void GetEnumerator_Then_SlicesShouldCoverTheWholeCorner()
    {
        var slices = Sweep(new Stops(0.2, 0.8));

        slices[0].From.Should().Be(0);
        slices[slices.Count - 1].To.Should().Be(1);
        for (var index = 1; index < slices.Count; index++)
        {
            // Neighbouring slices meet exactly, so neither a gap nor a double blend can open between them.
            slices[index].From.Should().Be(slices[index - 1].To);
        }
    }

    [Test]
    public void GetEnumerator_Then_TheStretchesTheBlendHoldsStillAcrossShouldBeOneSliceEach()
    {
        var stops = new Stops(0.2, 0.8);
        var sweep = new CornerSweep(Wedge(), stops, Scale.Identity);

        var slices = Sweep(stops);

        // Outside the two stops the blend holds at the colour of the side, so those stretches come out as one slice
        // rather than a dozen of the same colour, which is fewer slices than the sweep would otherwise be cut into.
        slices.Count.Should().BeLessThan(sweep.MaximumCount);
        slices[0].To.Should().BeGreaterThan(1d / sweep.MaximumCount);
        slices[slices.Count - 1].From.Should().BeLessThan(1 - (1d / sweep.MaximumCount));
    }

    [Test]
    public void GetEnumerator_When_ASliceSpansSeveral_Then_TheBlendShouldHoldStillAcrossIt()
    {
        var stops = new Stops(0.2, 0.8);
        var sweep = new CornerSweep(Wedge(), stops, Scale.Identity);
        var single = 1d / sweep.MaximumCount;

        foreach (var slice in sweep)
        {
            // Slices are only ever put together where doing so changes nothing, which is where the blend is not
            // moving, so a merged one has to come out the same colour at both ends.
            if (slice.To - slice.From > single + 0.0001)
            {
                stops.GetBlendAmount(slice.From).Should().Be(stops.GetBlendAmount(slice.To));
            }
        }
    }

    [Test]
    public void GetEnumerator_When_TheBlendMovesThroughout_Then_NoSlicesShouldBeMerged()
    {
        var stops = new Stops(0, 1);
        var sweep = new CornerSweep(Wedge(), stops, Scale.Identity);

        var slices = Sweep(stops);

        slices.Count.Should().Be(sweep.MaximumCount);
    }

    [Test]
    public void GetEnumerator_When_TheBlendIsAStep_Then_OnlyTheSliceAcrossTheStepShouldBeLeftWhole()
    {
        var stops = new Stops(0.5, 0.5);

        // A span of zero makes the transition abrupt, so the sweep is one colour, the slice the change falls in,
        // and then the other colour.
        var slices = Sweep(stops);

        slices.Count.Should().Be(3);
        slices[0].From.Should().Be(0);
        stops.GetBlendAmount(slices[0].To).Should().Be(0);
        stops.GetBlendAmount(slices[1].To).Should().Be(1);
        slices[2].To.Should().Be(1);
    }

    [Test]
    public void GetEnumerator_Then_SlicesShouldBeFilledPastTheirOwnEnd()
    {
        var wedge = Wedge();
        var sweep = new CornerSweep(wedge, new Stops(0.2, 0.8), Scale.Identity);

        foreach (var slice in sweep)
        {
            // Every slice but the last runs on, so that the next one paints over its antialiased edge.
            var overrun = slice.To + (CornerWedge.SliceOverlap / sweep.MaximumCount);
            var expected = wedge.GetSlice(slice.From, overrun > 1 ? 1 : overrun);
            slice.Filled.Should().Be(expected);
        }
    }

    [Test]
    public void GetEnumerator_Then_CutCentresShouldBeThoseOfTheSliceItself()
    {
        var wedge = Wedge();

        foreach (var slice in new CornerSweep(wedge, new Stops(0.2, 0.8), Scale.Identity))
        {
            var cuts = wedge.GetSlice(slice.From, slice.To);
            slice.StartCutCentre.Should().Be(cuts.StartCutCentre);
            slice.EndCutCentre.Should().Be(cuts.EndCutCentre);
        }
    }

    private static List<CornerSlice> Sweep(Stops stops)
    {
        var slices = new List<CornerSlice>();
        foreach (var slice in new CornerSweep(Wedge(), stops, Scale.Identity))
        {
            slices.Add(slice);
        }

        return slices;
    }

    /// <summary>
    /// A top left corner of a border 4 wide on the left, 6 deep on the top and rounded by 32.
    /// </summary>
    /// <returns>The corner.</returns>
    private static CornerWedge Wedge()
    {
        return new CornerWedge(
            new CornerArc(new Position(0, 32), new Position(0, 0), new Position(32, 0)),
            new CornerArc(new Position(32, 6), new Position(4, 6), new Position(4, 32)));
    }
}
