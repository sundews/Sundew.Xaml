// ----------------------------------------------------------------------------------------------------------------------
// <copyright file="PaintTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Primitives.Tests;

using AwesomeAssertions;
using TUnit.Core;

public class PaintTests
{
    private static readonly PaintColor Orange = PaintColor.FromArgb(255, 255, 140, 0);
    private static readonly PaintColor Blue = PaintColor.FromArgb(255, 70, 130, 180);
    private static readonly PaintColor Green = PaintColor.FromArgb(255, 46, 139, 87);

    [Test]
    public void Solid_Then_ResultShouldNotBeAGradient()
    {
        var result = Paint.Solid(Orange);

        result.IsGradient.Should().BeFalse();
        result.Color.Should().Be(Orange);
        result.Stops.Count.Should().Be(0);
    }

    [Test]
    public void Linear_Then_ResultShouldHaveAStopAtEitherEnd()
    {
        var result = Paint.Linear(Orange, Blue, new Position(1, 2), new Position(3, 4));

        result.IsGradient.Should().BeTrue();
        result.Stops.Count.Should().Be(2);
        result.Stops[0].Should().Be(new PaintStop(Orange, 0));
        result.Stops[1].Should().Be(new PaintStop(Blue, 1));
        result.Start.Should().Be(new Position(1, 2));
        result.End.Should().Be(new Position(3, 4));
    }

    [Test]
    public void Linear_When_BothEndsAreTheSameColor_Then_ResultShouldBeSolid()
    {
        // The corner sweep relies on this: a slice the blend holds still across costs a flat fill and not a
        // gradient, which is most of what merging those slices is worth.
        var result = Paint.Linear(Orange, Orange, new Position(1, 2), new Position(3, 4));

        result.IsGradient.Should().BeFalse();
        result.Stops.Count.Should().Be(0);
        result.Color.Should().Be(Orange);
    }

    [Test]
    public void Gradient_Then_ResultShouldKeepEveryStop()
    {
        var stops = new[] { new PaintStop(Orange, 0), new PaintStop(Blue, 0.25), new PaintStop(Green, 1) };

        var result = Paint.Gradient(stops, new Position(0, 0), new Position(0, 1), true);

        result.IsGradient.Should().BeTrue();
        result.IsRelative.Should().BeTrue();
        result.Stops.Count.Should().Be(3);
        result.Stops[0].Should().Be(stops[0]);
        result.Stops[1].Should().Be(stops[1]);
        result.Stops[2].Should().Be(stops[2]);
    }

    [Test]
    public void Equals_When_TheBlendsEndOnDifferentColors_Then_ResultShouldBeFalse()
    {
        // The two colours of a blend are kept in the paint rather than in a list of its own, so equality has to
        // read them rather than compare the list that is no longer there.
        var first = Paint.Linear(Orange, Blue, default, default);
        var second = Paint.Linear(Orange, Green, default, default);

        first.Equals(second).Should().BeFalse();
        first.Equals(Paint.Linear(Orange, Blue, default, default)).Should().BeTrue();
    }

    [Test]
    public void Equals_When_OneIsSolidAndTheOtherBlendsFromTheSameColor_Then_ResultShouldBeFalse()
    {
        var solid = Paint.Solid(Orange);
        var blend = Paint.Linear(Orange, Blue, default, default);

        solid.Equals(blend).Should().BeFalse();
    }

    [Test]
    public void ResolveEnds_When_TheGradientIsRelative_Then_ResultShouldBeAgainstTheBounds()
    {
        var paint = Paint.Gradient(
            new[] { new PaintStop(Orange, 0), new PaintStop(Blue, 1) },
            new Position(0, 0),
            new Position(1, 1),
            true);

        paint.ResolveEnds(new Bounds(10, 20, 100, 200), out var start, out var end);

        start.Should().Be(new Position(10, 20));
        end.Should().Be(new Position(110, 220));
    }
}
