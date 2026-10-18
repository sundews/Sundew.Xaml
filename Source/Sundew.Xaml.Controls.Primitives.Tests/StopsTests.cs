// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StopsTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Primitives.Tests;

using AwesomeAssertions;
using TUnit.Core;

public class StopsTests
{
    [Test]
    [Arguments(0.0, 0.0)]
    [Arguments(0.2, 0.0)]
    [Arguments(0.5, 0.5)]
    [Arguments(0.8, 1.0)]
    [Arguments(1.0, 1.0)]
    public void GetBlendAmount_Then_ResultShouldHoldOutsideTheStopsAndRunStraightBetweenThem(double position, double expected)
    {
        var stops = new Stops(0.2, 0.8);

        stops.GetBlendAmount(position).Should().BeApproximately(expected, 0.0001);
    }

    [Test]
    public void GetBlendAmount_When_TheStopsMeet_Then_ResultShouldStepFromOneColourToTheOther()
    {
        var stops = new Stops(0.5, 0.5);

        stops.GetBlendAmount(0.49).Should().Be(0);
        stops.GetBlendAmount(0.5).Should().Be(1);
    }
}
