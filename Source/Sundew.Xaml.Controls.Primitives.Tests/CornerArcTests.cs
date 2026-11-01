// ----------------------------------------------------------------------------------------------------------------------
// <copyright file="CornerArcTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Primitives.Tests;

using AwesomeAssertions;
using TUnit.Core;

public class CornerArcTests
{
    [Test]
    public void At_When_TheArcIsSharp_Then_ResultShouldBeOnOneLegOrTheOther()
    {
        var arc = Sharp();

        // The turn of an even sided corner is halfway along it, so the first half runs down the one leg to the
        // corner of the box and the second half runs away along the other.
        arc.At(0).Should().Be(new Position(0, 4));
        arc.At(0.25).Should().Be(new Position(0, 2));
        arc.At(0.5).Should().Be(new Position(0, 0));
        arc.At(0.75).Should().Be(new Position(2, 0));
        arc.At(1).Should().Be(new Position(4, 0));
    }

    [Test]
    public void Turn_When_TheLegsAreDifferentLengths_Then_ResultShouldBeWhereTheyMeet()
    {
        // The legs are as long as the two bands are thick, so an uneven corner turns away from the middle. Taking
        // it by length is what keeps anything walking the corner moving at one rate the whole way round.
        var arc = new CornerArc(new Position(0, 2), new Position(0, 0), new Position(6, 0), true);

        arc.Turn.Should().Be(0.25);
        arc.At(0.25).Should().Be(new Position(0, 0));
        arc.At(0.625).Should().Be(new Position(3, 0));
    }

    [Test]
    public void Turn_When_TheArcCurves_Then_ResultShouldBeZero()
    {
        new CornerArc(new Position(0, 4), new Position(0, 0), new Position(4, 0)).Turn.Should().Be(0);
    }

    [Test]
    public void Section_When_TheSectionIsOnOneLeg_Then_ResultShouldBeAStraightRun()
    {
        var result = Sharp().Section(0, 0.25);

        result.IsSharp.Should().BeTrue();
        result.Start.Should().Be(new Position(0, 4));
        result.End.Should().Be(new Position(0, 2));

        // Nothing is left of the turn, so the section is the one straight run from end to end.
        result.Control.Should().Be(result.End);
        result.At(0.5).Should().Be(new Position(0, 3));
    }

    [Test]
    public void Section_When_TheSectionRunsOverTheTurn_Then_ResultShouldKeepTheTurn()
    {
        var result = Sharp().Section(0.25, 0.75);

        result.IsSharp.Should().BeTrue();
        result.Start.Should().Be(new Position(0, 2));
        result.Control.Should().Be(new Position(0, 0));
        result.End.Should().Be(new Position(2, 0));
        result.At(0.5).Should().Be(new Position(0, 0));
    }

    [Test]
    [Arguments(0d, 0.5d)]
    [Arguments(0.1d, 0.9d)]
    [Arguments(0.4d, 0.6d)]
    [Arguments(0.5d, 1d)]
    public void Section_When_TheArcIsSharp_Then_ResultShouldAgreeWithTheWholeArc(double from, double to)
    {
        var arc = Sharp();

        var result = arc.Section(from, to);

        // A section is cut again as a sweep narrows it, so where it says it is has to be where the whole arc says
        // it is. The two reach the point by different arithmetic, so they agree to within rounding.
        for (var step = 0; step <= 4; step++)
        {
            var along = step / 4d;
            var expected = arc.At(from + ((to - from) * along));

            var actual = result.At(along);

            actual.X.Should().BeApproximately(expected.X, 1e-9);
            actual.Y.Should().BeApproximately(expected.Y, 1e-9);
        }
    }

    [Test]
    public void Section_When_TheArcCurves_Then_ResultShouldStayCurved()
    {
        var arc = new CornerArc(new Position(0, 4), new Position(0, 0), new Position(4, 0));

        var result = arc.Section(0.25, 0.75);

        result.IsSharp.Should().BeFalse();
        result.Start.Should().Be(arc.At(0.25));
        result.End.Should().Be(arc.At(0.75));
    }

    [Test]
    public void Translate_When_TheArcIsSharp_Then_ResultShouldStaySharp()
    {
        var result = Sharp().Translate(10, 20);

        result.IsSharp.Should().BeTrue();
        result.Start.Should().Be(new Position(10, 24));
        result.Control.Should().Be(new Position(10, 20));
        result.End.Should().Be(new Position(14, 20));
    }

    private static CornerArc Sharp()
    {
        return new CornerArc(new Position(0, 4), new Position(0, 0), new Position(4, 0), true);
    }
}
