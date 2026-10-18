// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CornerWedgeTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Primitives.Tests;

using AwesomeAssertions;
using TUnit.Core;

public class CornerWedgeTests
{
    [Test]
    public void GetSlice_When_TheSliceIsTheWholeCorner_Then_ResultShouldBeTheCorner()
    {
        var wedge = Wedge();

        var result = wedge.GetSlice(0, 1);

        result.Outer.Start.Should().Be(wedge.Outer.Start);
        result.Outer.End.Should().Be(wedge.Outer.End);
        result.Inner.Start.Should().Be(wedge.Inner.Start);
        result.Inner.End.Should().Be(wedge.Inner.End);
    }

    [Test]
    [Arguments(2)]
    [Arguments(5)]
    [Arguments(20)]
    public void GetSlice_Then_SlicesShouldTileTheCorner(int count)
    {
        var wedge = Wedge();
        var previous = default(CornerWedge?);

        for (var index = 0; index < count; index++)
        {
            var slice = wedge.GetSlice((double)index / count, (double)(index + 1) / count);

            if (previous.HasValue)
            {
                // Neighbouring slices share a cut, so neither a gap nor an overlap can open between them.
                slice.Outer.Start.Should().Be(previous.Value.Outer.End);
                slice.StartCut.Should().Be(previous.Value.EndCut);
            }
            else
            {
                slice.Outer.Start.Should().Be(wedge.Outer.Start);
                slice.StartCut.Should().Be(wedge.StartCut);
            }

            previous = slice;
        }

        previous!.Value.Outer.End.Should().Be(wedge.Outer.End);
        previous!.Value.EndCut.Should().Be(wedge.EndCut);
    }

    [Test]
    public void GetSlice_Then_SliceShouldFollowTheSamePathAsTheCorner()
    {
        var wedge = Wedge();

        var result = wedge.GetSlice(0.25, 0.75);

        // Halfway along a slice from a quarter to three quarters is halfway along the corner.
        result.Outer.At(0.5).X.Should().BeApproximately(wedge.Outer.At(0.5).X, 0.0001);
        result.Outer.At(0.5).Y.Should().BeApproximately(wedge.Outer.At(0.5).Y, 0.0001);
        result.Inner.At(0.5).X.Should().BeApproximately(wedge.Inner.At(0.5).X, 0.0001);
        result.Inner.At(0.5).Y.Should().BeApproximately(wedge.Inner.At(0.5).Y, 0.0001);
    }

    [Test]
    public void GetSliceCount_When_TheCornerIsLarge_Then_ResultShouldBeCapped()
    {
        var wedge = new CornerWedge(
            new CornerArc(new Position(0, 400), new Position(0, 0), new Position(400, 0)),
            new CornerArc(new Position(400, 10), new Position(10, 10), new Position(10, 400)));

        wedge.GetSliceCount(Scale.Identity).Should().Be(20);
    }

    [Test]
    public void GetSliceCount_When_TheCornerIsTiny_Then_ResultShouldBeOne()
    {
        var wedge = new CornerWedge(
            new CornerArc(new Position(0, 1), new Position(0, 0), new Position(1, 0)),
            new CornerArc(new Position(1, 1), new Position(1, 1), new Position(1, 1)));

        wedge.GetSliceCount(Scale.Identity).Should().Be(1);
    }

    [Test]
    public void GetSliceCount_When_TheScaleIsHigher_Then_ResultShouldBeFiner()
    {
        // Small enough that twice as many slices is still under the cap.
        var wedge = new CornerWedge(
            new CornerArc(new Position(0, 8), new Position(0, 0), new Position(8, 0)),
            new CornerArc(new Position(8, 2), new Position(2, 2), new Position(2, 8)));

        var single = wedge.GetSliceCount(Scale.Identity);
        var doubled = wedge.GetSliceCount(Scale.Uniform(2));

        single.Should().Be(4);
        doubled.Should().Be(single * 2);
    }

    [Test]
    public void StartCutCentre_Then_ResultShouldBeMidwayAcrossTheCutTheCornerStartsAt()
    {
        var wedge = Wedge();

        // The corner starts where the 4 wide left side ends, so its cut runs from x 0 to x 4 at y 32.
        wedge.StartCutCentre.Should().Be(new Position(2, 32));

        // It ends where the 6 deep top side starts, so that cut runs from y 0 to y 6 at x 32.
        wedge.EndCutCentre.Should().Be(new Position(32, 3));
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
