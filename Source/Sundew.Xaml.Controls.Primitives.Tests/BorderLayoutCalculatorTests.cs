// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BorderLayoutCalculatorTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls.Primitives.Tests;

using AwesomeAssertions;
using TUnit.Core;

public class BorderLayoutCalculatorTests
{
    [Test]
    public void CalculateBorder_Then_SidesShouldRunBetweenTheOuterBoundaryAndTheInnerOne()
    {
        var metrics = Metrics(100, 50, thickness: 2, cornerRadius: 0);

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        result.Sides.Left.Should().Be(new SideQuad(
            new Edge(new Position(0, 48), new Position(0, 2)),
            new Edge(new Position(2, 48), new Position(2, 2))));
        result.Sides.Top.Should().Be(new SideQuad(
            new Edge(new Position(2, 0), new Position(98, 0)),
            new Edge(new Position(2, 2), new Position(98, 2))));
        result.Sides.Right.Should().Be(new SideQuad(
            new Edge(new Position(100, 2), new Position(100, 48)),
            new Edge(new Position(98, 2), new Position(98, 48))));
        result.Sides.Bottom.Should().Be(new SideQuad(
            new Edge(new Position(98, 50), new Position(2, 50)),
            new Edge(new Position(98, 48), new Position(2, 48))));
    }

    [Test]
    public void CalculateBorder_Then_CornerWedgesShouldBendAroundTheCornerOfTheBox()
    {
        var metrics = Metrics(100, 50, thickness: 2, cornerRadius: 0);

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        // The radius is no larger than the thickness, so each inner arc has collapsed onto the inner corner point.
        result.Corners.TopLeft.Should().Be(new CornerWedge(
            new CornerArc(new Position(0, 2), new Position(0, 0), new Position(2, 0)),
            new CornerArc(new Position(2, 2), new Position(2, 2), new Position(2, 2))));
        result.Corners.TopRight.Should().Be(new CornerWedge(
            new CornerArc(new Position(98, 0), new Position(100, 0), new Position(100, 2)),
            new CornerArc(new Position(98, 2), new Position(98, 2), new Position(98, 2))));
        result.Corners.BottomRight.Should().Be(new CornerWedge(
            new CornerArc(new Position(100, 48), new Position(100, 50), new Position(98, 50)),
            new CornerArc(new Position(98, 48), new Position(98, 48), new Position(98, 48))));
        result.Corners.BottomLeft.Should().Be(new CornerWedge(
            new CornerArc(new Position(2, 50), new Position(0, 50), new Position(0, 48)),
            new CornerArc(new Position(2, 48), new Position(2, 48), new Position(2, 48))));
    }

    [Test]
    [Arguments(100.0, 50.0, 2.0, 0.0)]
    [Arguments(100.0, 50.0, 8.0, 16.0)]
    [Arguments(40.0, 120.0, 4.0, 900.0)]
    [Arguments(10.0, 10.0, 1.0, 20.0)]
    [Arguments(3.0, 80.0, 6.0, 8.0)]
    public void CalculateBorder_Then_CornersShouldTileWithTheSidesEitherSideOfThem(double width, double height, double thickness, double cornerRadius)
    {
        var metrics = Metrics(width, height, thickness, cornerRadius);

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        // Every corner is traversed from the side entering it to the side leaving it, so the whole ring is
        // left, top left, top, top right, right, bottom right, bottom, bottom left and back to left.
        ShouldTile(result.Corners.TopLeft, entering: result.Sides.Left, leaving: result.Sides.Top);
        ShouldTile(result.Corners.TopRight, entering: result.Sides.Top, leaving: result.Sides.Right);
        ShouldTile(result.Corners.BottomRight, entering: result.Sides.Right, leaving: result.Sides.Bottom);
        ShouldTile(result.Corners.BottomLeft, entering: result.Sides.Bottom, leaving: result.Sides.Left);
    }

    [Test]
    [Arguments(100.0, 50.0, 2.0, 0.0)]
    [Arguments(100.0, 50.0, 8.0, 16.0)]
    [Arguments(40.0, 120.0, 4.0, 900.0)]
    [Arguments(10.0, 10.0, 1.0, 20.0)]
    [Arguments(3.0, 80.0, 6.0, 8.0)]
    public void CalculateBorder_Then_EachInnerArcShouldBendAroundWhereTheTwoInnerEdgesWouldHaveMet(double width, double height, double thickness, double cornerRadius)
    {
        var metrics = Metrics(width, height, thickness, cornerRadius);

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        ShouldBendAroundTheInnerCorner(result.Corners.TopLeft, result.Sides.Left, result.Sides.Top);
        ShouldBendAroundTheInnerCorner(result.Corners.TopRight, result.Sides.Right, result.Sides.Top);
        ShouldBendAroundTheInnerCorner(result.Corners.BottomRight, result.Sides.Right, result.Sides.Bottom);
        ShouldBendAroundTheInnerCorner(result.Corners.BottomLeft, result.Sides.Left, result.Sides.Bottom);
    }

    [Test]
    public void CalculateBorder_When_ThicknessIsNonUniform_Then_EachSideShouldBeInsetByItsOwnThickness()
    {
        var metrics = NonUniformMetrics();

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        // This is what stroking could not do: one pen has one width, so the four sides had to share it.
        result.Sides.Left.Outer.Start.X.Should().Be(0);
        result.Sides.Left.Inner.Start.X.Should().Be(2);
        result.Sides.Top.Outer.Start.Y.Should().Be(0);
        result.Sides.Top.Inner.Start.Y.Should().Be(6);
        result.Sides.Right.Outer.Start.X.Should().Be(100);
        result.Sides.Right.Inner.Start.X.Should().Be(90);
        result.Sides.Bottom.Outer.Start.Y.Should().Be(50);
        result.Sides.Bottom.Inner.Start.Y.Should().Be(36);
    }

    [Test]
    public void CalculateBorder_When_ThicknessIsNonUniform_Then_TheInsideOfACornerShouldBeRoundedWhereverThereIsRoom()
    {
        var metrics = NonUniformMetrics();

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        // Top left: the inner arc runs the whole way from one cut to the other, bending around the point where the
        // inner edges of the 2 thick left side and the 6 thick top side would have met.
        result.Corners.TopLeft.Outer.Should().Be(
            new CornerArc(new Position(0, 12), new Position(0, 0), new Position(12, 0)));
        result.Corners.TopLeft.Inner.Should().Be(
            new CornerArc(new Position(12, 6), new Position(2, 6), new Position(2, 12)));

        // Bottom left: the radius is raised to the 14 of the bottom side, which puts the inner corner on the cut
        // itself, so there is nothing left to round and the inside of the corner is square. Letting the radius stay
        // at the 12 asked for would not round it either, it would bend the inner boundary the other way, down into
        // the bottom side, which CalculateBorder_Then_NoInnerArcShouldLeaveTheInnerRectangle holds it to.
        result.Corners.BottomLeft.Inner.Should().Be(
            new CornerArc(new Position(2, 36), new Position(2, 36), new Position(14, 36)));
    }

    [Test]
    [Arguments(2.0, 6.0, 10.0, 14.0, 12.0)]
    [Arguments(2.0, 6.0, 10.0, 14.0, 0.0)]
    [Arguments(2.0, 6.0, 10.0, 14.0, 4.0)]
    [Arguments(14.0, 2.0, 6.0, 10.0, 8.0)]
    [Arguments(1.0, 20.0, 1.0, 20.0, 2.0)]
    [Arguments(4.0, 4.0, 4.0, 4.0, 16.0)]
    public void CalculateBorder_Then_NoInnerArcShouldLeaveTheInnerRectangle(double left, double top, double right, double bottom, double cornerRadius)
    {
        var thickness = new Sides<double>(left, top, right, bottom);
        var metrics = new BorderMetrics(100, 50, thickness, new Corners<double>(cornerRadius), Scale.Identity);

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        // An inner arc that left the rectangle the straight inner edges bound would bulge into the thicker of the two
        // sides meeting it, which reads as the corner curving the wrong way and the border thinning out there.
        // A quadratic stays within its three points, so holding those inside the rectangle holds the whole arc.
        ShouldStayInside(result.Corners.TopLeft, thickness);
        ShouldStayInside(result.Corners.TopRight, thickness);
        ShouldStayInside(result.Corners.BottomRight, thickness);
        ShouldStayInside(result.Corners.BottomLeft, thickness);

        static void ShouldStayInside(CornerWedge wedge, Sides<double> thickness)
        {
            foreach (var position in new[] { wedge.Inner.Start, wedge.Inner.Control, wedge.Inner.End })
            {
                position.X.Should().BeInRange(thickness.Left, 100 - thickness.Right);
                position.Y.Should().BeInRange(thickness.Top, 50 - thickness.Bottom);
            }
        }
    }

    [Test]
    public void CalculateBorder_When_TheRadiusIsUnderTheThickerOfTwoSides_Then_ItShouldBeRaisedToThatThickness()
    {
        var metrics = new BorderMetrics(100, 50, new Sides<double>(2, 6, 10, 14), new Corners<double>(4), Scale.Identity);

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        // The top left corner is raised from the 4 asked for to the 6 of the top side, and the bottom left to the 14
        // of the bottom side. Keeping the 4 would put the left side's cut 4 up from the bottom, inside the 14 the
        // bottom side occupies, and the inner boundary would have to bend outwards to reach it.
        result.Corners.TopLeft.Outer.End.X.Should().Be(6);
        result.Corners.BottomLeft.Outer.Start.X.Should().Be(14);
    }

    [Test]
    [Arguments(100.0, 50.0, 4.0, 0.0)]
    [Arguments(100.0, 50.0, 8.0, 16.0)]
    [Arguments(40.0, 120.0, 4.0, 24.0)]
    public void CalculateBorder_When_ThicknessIsUniform_Then_TheRingShouldSitEquallyEitherSideOfTheFormerStrokeCentreLine(double width, double height, double thickness, double cornerRadius)
    {
        var metrics = Metrics(width, height, thickness, cornerRadius);
        var offset = thickness / 2;

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        // A stroke of this thickness used to be centred half a thickness in from each side, which is where the
        // midpoint between the two boundaries of the ring now falls, so a uniform border draws as it always did.
        Midpoint(result.Sides.Left.Outer.Start.X, result.Sides.Left.Inner.Start.X).Should().Be(offset);
        Midpoint(result.Sides.Top.Outer.Start.Y, result.Sides.Top.Inner.Start.Y).Should().Be(offset);
        Midpoint(result.Sides.Right.Outer.Start.X, result.Sides.Right.Inner.Start.X).Should().Be(width - offset);
        Midpoint(result.Sides.Bottom.Outer.Start.Y, result.Sides.Bottom.Inner.Start.Y).Should().Be(height - offset);

        // The same holds around the corners, where the two arcs are the former one moved out and in by the offset.
        result.Corners.TopLeft.Outer.Control.Should().Be(new Position(0, 0));
        result.Corners.TopLeft.Inner.Control.Should().Be(new Position(thickness, thickness));
        result.Corners.BottomRight.Outer.Control.Should().Be(new Position(width, height));
        result.Corners.BottomRight.Inner.Control.Should().Be(new Position(width - thickness, height - thickness));
    }

    [Test]
    public void CalculateBorder_Then_BackgroundShouldMeetTheInsideOfTheRing()
    {
        var metrics = NonUniformMetrics();

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        // Each background corner starts and ends where the matching inner arc does, so no seam can open between them.
        result.Background.TopLeft.Start.Should().Be(result.Corners.TopLeft.Inner.End);
        result.Background.TopLeft.End.Should().Be(result.Corners.TopLeft.Inner.Start);
        result.Background.TopRight.Start.Should().Be(result.Corners.TopRight.Inner.End);
        result.Background.TopRight.End.Should().Be(result.Corners.TopRight.Inner.Start);
        result.Background.BottomRight.Start.Should().Be(result.Corners.BottomRight.Inner.End);
        result.Background.BottomRight.End.Should().Be(result.Corners.BottomRight.Inner.Start);
        result.Background.BottomLeft.Start.Should().Be(result.Corners.BottomLeft.Inner.End);
        result.Background.BottomLeft.End.Should().Be(result.Corners.BottomLeft.Inner.Start);
    }

    [Test]
    public void CalculateBorder_Then_BackgroundShouldRunAlongTheInsideOfTheBorder()
    {
        var metrics = Metrics(100, 50, thickness: 2, cornerRadius: 0);

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        result.Background.Edges.Left.Should().Be(new Edge(new Position(2, 48), new Position(2, 2)));
        result.Background.Edges.Top.Should().Be(new Edge(new Position(2, 2), new Position(98, 2)));
        result.Background.Edges.Right.Should().Be(new Edge(new Position(98, 2), new Position(98, 48)));
        result.Background.Edges.Bottom.Should().Be(new Edge(new Position(98, 48), new Position(2, 48)));
    }

    [Test]
    public void CalculateBorder_Then_BackgroundControlPointsShouldOverdrawUnderTheBorder()
    {
        var metrics = Metrics(100, 50, thickness: 2, cornerRadius: 0);

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        result.Background.Controls.Should().Be(new Corners<Position>(
            new Position(1.5, 1.5),
            new Position(98.5, 1.5),
            new Position(98.5, 48.5),
            new Position(1.5, 48.5)));
    }

    [Test]
    [Arguments(100.0, 50.0, 2.0, 0.0)]
    [Arguments(100.0, 50.0, 8.0, 16.0)]
    [Arguments(40.0, 120.0, 4.0, 900.0)]
    [Arguments(10.0, 10.0, 1.0, 20.0)]
    [Arguments(3.0, 80.0, 6.0, 8.0)]
    public void CalculateBorder_Then_NoSideShouldRunBackwards(double width, double height, double thickness, double cornerRadius)
    {
        var metrics = Metrics(width, height, thickness, cornerRadius);

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        result.Sides.Left.Outer.Start.Y.Should().BeGreaterThanOrEqualTo(result.Sides.Left.Outer.End.Y);
        result.Sides.Top.Outer.End.X.Should().BeGreaterThanOrEqualTo(result.Sides.Top.Outer.Start.X);
        result.Sides.Right.Outer.End.Y.Should().BeGreaterThanOrEqualTo(result.Sides.Right.Outer.Start.Y);
        result.Sides.Bottom.Outer.Start.X.Should().BeGreaterThanOrEqualTo(result.Sides.Bottom.Outer.End.X);
    }

    [Test]
    public void ExtendIntoCorners_Then_TheSideShouldRunPastBothOfItsEndsWithoutMovingSideways()
    {
        var metrics = Metrics(100, 50, thickness: 2, cornerRadius: 0);
        var layout = BorderLayoutCalculator.CalculateBorder(metrics);

        var result = layout.Sides.Left.ExtendIntoCorners(Scale.Identity);

        // The left side runs bottom to top, so it grows downwards at its start and upwards at its end, and stays on
        // x 0 and x 2. Running under the corners is what stops the page showing through the cut between them.
        result.Outer.Should().Be(new Edge(new Position(0, 48.75), new Position(0, 1.25)));
        result.Inner.Should().Be(new Edge(new Position(2, 48.75), new Position(2, 1.25)));
    }

    [Test]
    public void ExtendIntoCorners_When_TheScaleIsHigher_Then_TheOverrunShouldStayTheSameNumberOfDevicePixels()
    {
        var metrics = Metrics(100, 50, thickness: 2, cornerRadius: 0);
        var layout = BorderLayoutCalculator.CalculateBorder(metrics);

        var result = layout.Sides.Top.ExtendIntoCorners(Scale.Uniform(3));

        // Three device pixels to the unit, so the same overrun is a third of the distance.
        result.Outer.Start.X.Should().BeApproximately(2 - (0.75 / 3), 0.0001);
        result.Outer.End.X.Should().BeApproximately(98 + (0.75 / 3), 0.0001);
    }

    [Test]
    public void CalculateBorder_When_CornersConsumeAllTheRoom_Then_NoSideShouldHaveArea()
    {
        var metrics = Metrics(10, 10, thickness: 1, cornerRadius: 20);

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        result.Sides.Left.HasArea.Should().BeFalse();
        result.Sides.Top.HasArea.Should().BeFalse();
        result.Sides.Right.HasArea.Should().BeFalse();
        result.Sides.Bottom.HasArea.Should().BeFalse();
    }

    [Test]
    public void CalculateBorder_When_ASideHasNoThickness_Then_ThatSideShouldHaveNoArea()
    {
        var metrics = new BorderMetrics(100, 50, new Sides<double>(0, 4, 4, 4), new Corners<double>(8), Scale.Identity);

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        result.Sides.Left.HasArea.Should().BeFalse();
        result.Sides.Top.HasArea.Should().BeTrue();
    }

    [Test]
    public void CalculateBorder_When_CornerRadiusIsZero_Then_CornersShouldStillBeAsLargeAsTheSidesAreThick()
    {
        var metrics = Metrics(100, 50, thickness: 8, cornerRadius: 0);

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        // The straight top side starts where the top left corner ends, so the corner has taken the thickness as radius.
        result.Sides.Top.Outer.Start.X.Should().Be(8);
        result.Sides.Left.Outer.End.Y.Should().Be(8);
    }

    [Test]
    public void CalculateBorder_Then_GuidelinesShouldBeDistinct()
    {
        var metrics = Metrics(100, 50, thickness: 2, cornerRadius: 0);

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        // Both boundaries of the ring, rather than the former stroke centre lines, are what has to snap now.
        result.Guidelines.X.Should().Equal(0d, 2d, 98d, 100d);
        result.Guidelines.Y.Should().Equal(0d, 2d, 48d, 50d);
    }

    [Test]
    public void TryCalculateContentClip_Then_FigureShouldBeInsetByThicknessAndPaddingAndExpressedInContentSpace()
    {
        var metrics = Metrics(100, 50, thickness: 4, cornerRadius: 10);

        var result = BorderLayoutCalculator.TryCalculateContentClip(metrics, new Sides<double>(2), out var figure);

        result.Should().BeTrue();
        figure.Edges.Left.Should().Be(new Edge(new Position(-2, 34), new Position(-2, 4)));
        figure.Edges.Top.Should().Be(new Edge(new Position(4, -2), new Position(84, -2)));
        figure.Edges.Right.Should().Be(new Edge(new Position(90, 4), new Position(90, 34)));
        figure.Edges.Bottom.Should().Be(new Edge(new Position(84, 40), new Position(4, 40)));
        figure.Controls.Should().Be(new Corners<Position>(
            new Position(-2, -2),
            new Position(90, -2),
            new Position(90, 40),
            new Position(-2, 40)));
    }

    [Test]
    public void TryCalculateContentClip_Then_FigureShouldFollowTheInsideOfTheRing()
    {
        var metrics = NonUniformMetrics();
        var layout = BorderLayoutCalculator.CalculateBorder(metrics);

        BorderLayoutCalculator.TryCalculateContentClip(metrics, default, out var figure);

        // The clip and the border are drawn from the same boundary, so content cannot creep out under the border.
        var offset = figure.Translate(2, 6);
        offset.Edges.Left.Should().Be(layout.Sides.Left.Inner);
        offset.Edges.Top.Should().Be(layout.Sides.Top.Inner);
        offset.Edges.Right.Should().Be(layout.Sides.Right.Inner);
        offset.Edges.Bottom.Should().Be(layout.Sides.Bottom.Inner);
        offset.TopLeft.Control.Should().Be(layout.Corners.TopLeft.Inner.Control);
    }

    [Test]
    public void TryCalculateContentClip_When_ThereIsNoRoomForContent_Then_ResultShouldBeFalse()
    {
        var metrics = Metrics(100, 50, thickness: 60, cornerRadius: 0);

        var result = BorderLayoutCalculator.TryCalculateContentClip(metrics, default, out var figure);

        result.Should().BeFalse();
        figure.Should().Be(default(RoundedFigure));
    }

    [Test]
    public void TryCalculateContentClip_Then_CornerRadiusShouldBeReducedByTheThickness()
    {
        var metrics = Metrics(100, 50, thickness: 4, cornerRadius: 10);

        BorderLayoutCalculator.TryCalculateContentClip(metrics, default, out var figure);

        // The inner radius is 10 - 4, so the top edge starts six units in from the inner left edge at x = 4.
        figure.Edges.Top.Start.X.Should().Be(6);
    }

    [Test]
    public void CalculateEdgeThickness_Then_EachSideShouldBeAlignedToItsOwnAxis()
    {
        var thickness = new Sides<double>(1, 2, 3, 4);

        var result = BorderLayoutCalculator.CalculateEdgeThickness(thickness, new Scale(2, 1));

        result.Should().Be(new Sides<double>(1, 2, 3, 4));
    }

    [Test]
    public void CalculateFittedThickness_When_TheSidesFit_Then_ResultShouldBeTheThicknessAsGiven()
    {
        var thickness = new Sides<double>(1, 2, 3, 4);

        var result = BorderLayoutCalculator.CalculateFittedThickness(thickness, 100, 50, Scale.Identity);

        result.Should().Be(thickness);
    }

    [Test]
    public void CalculateFittedThickness_When_TwoFacingSidesAreTogetherTooThick_Then_TheyShouldBeScaledDownToFit()
    {
        var thickness = new Sides<double>(30, 10, 10, 10);

        var result = BorderLayoutCalculator.CalculateFittedThickness(thickness, 20, 50, Scale.Identity);

        // Three quarters of the twenty available go to the left side, which is three quarters of the thickness asked for.
        result.Should().Be(new Sides<double>(15, 10, 5, 10));
    }

    [Test]
    public void CalculateBorder_When_TheSidesAreTogetherTooThick_Then_TheInnerBoundaryShouldNotTurnInsideOut()
    {
        var metrics = Metrics(30, 30, thickness: 20, cornerRadius: 4);

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        result.Sides.Left.Inner.Start.X.Should().BeLessThanOrEqualTo(result.Sides.Right.Inner.Start.X);
        result.Sides.Top.Inner.Start.Y.Should().BeLessThanOrEqualTo(result.Sides.Bottom.Inner.Start.Y);
    }

    [Test]
    public void CalculateBorder_When_ScaleIsFractional_Then_GeometryShouldLandOnWholeDevicePixels()
    {
        var metrics = new BorderMetrics(100.4, 50.7, new Sides<double>(1), new Corners<double>(0), new Scale(1.5, 1.5));

        var result = BorderLayoutCalculator.CalculateBorder(metrics);

        (result.Sides.Top.Inner.Start.Y * 1.5).Should().BeApproximately(2, 0.0001);
        (result.Sides.Left.Inner.Start.X * 1.5).Should().BeApproximately(2, 0.0001);
    }

    private static void ShouldTile(CornerWedge wedge, SideQuad entering, SideQuad leaving)
    {
        wedge.Outer.Start.Should().Be(entering.Outer.End);
        wedge.StartCut.Should().Be(entering.Inner.End);
        wedge.Outer.End.Should().Be(leaving.Outer.Start);
        wedge.EndCut.Should().Be(leaving.Inner.Start);
    }

    private static void ShouldBendAroundTheInnerCorner(CornerWedge wedge, SideQuad horizontal, SideQuad vertical)
    {
        wedge.Inner.Control.Should().Be(new Position(horizontal.Inner.Start.X, vertical.Inner.Start.Y));
    }

    private static double Midpoint(double first, double second)
    {
        return (first + second) / 2;
    }

    private static BorderMetrics Metrics(double width, double height, double thickness, double cornerRadius)
    {
        return new BorderMetrics(
            width,
            height,
            new Sides<double>(thickness),
            new Corners<double>(cornerRadius),
            Scale.Identity);
    }

    private static BorderMetrics NonUniformMetrics()
    {
        return new BorderMetrics(
            100,
            50,
            new Sides<double>(2, 6, 10, 14),
            new Corners<double>(12),
            Scale.Identity);
    }
}
