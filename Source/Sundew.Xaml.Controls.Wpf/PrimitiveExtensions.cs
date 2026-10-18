// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrimitiveExtensions.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System.Windows;
using System.Windows.Media;

/// <summary>
/// Translates between the platform neutral border primitives and WPF.
/// </summary>
internal static class PrimitiveExtensions
{
    public static Sides<double> ToSides(this Thickness thickness)
    {
        return new Sides<double>(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
    }

    public static Corners<double> ToCorners(this CornerRadius cornerRadius)
    {
        return new Corners<double>(cornerRadius.TopLeft, cornerRadius.TopRight, cornerRadius.BottomRight, cornerRadius.BottomLeft);
    }

    public static Scale ToScale(this DpiScale dpiScale)
    {
        return new Scale(dpiScale.DpiScaleX, dpiScale.DpiScaleY);
    }

    public static Point ToPoint(this Position position)
    {
        return new Point(position.X, position.Y);
    }

    public static Geometry ToGeometry(this SideQuad sideQuad)
    {
        return new PathGeometry(
        [
            new PathFigure(
                sideQuad.Outer.Start.ToPoint(),
                [
                    new LineSegment(sideQuad.Outer.End.ToPoint(), true),
                    new LineSegment(sideQuad.Inner.End.ToPoint(), true),
                    new LineSegment(sideQuad.Inner.Start.ToPoint(), true),
                ],
                true),
        ]);
    }

    public static Geometry ToGeometry(this CornerWedge cornerWedge)
    {
        return new PathGeometry(
        [
            new PathFigure(
                cornerWedge.Outer.Start.ToPoint(),
                [
                    new QuadraticBezierSegment(cornerWedge.Outer.Control.ToPoint(), cornerWedge.Outer.End.ToPoint(), true),
                    new LineSegment(cornerWedge.Inner.Start.ToPoint(), true),
                    new QuadraticBezierSegment(cornerWedge.Inner.Control.ToPoint(), cornerWedge.Inner.End.ToPoint(), true),
                ],
                true),
        ]);
    }

    public static PathGeometry ToGeometry(this RoundedFigure figure)
    {
        return new PathGeometry(
        [
            new PathFigure(
                figure.Start.ToPoint(),
                [
                    new LineSegment(figure.Edges.Left.End.ToPoint(), true),
                    new QuadraticBezierSegment(figure.Controls.TopLeft.ToPoint(), figure.Edges.Top.Start.ToPoint(), true),
                    new LineSegment(figure.Edges.Top.End.ToPoint(), true),
                    new QuadraticBezierSegment(figure.Controls.TopRight.ToPoint(), figure.Edges.Right.Start.ToPoint(), true),
                    new LineSegment(figure.Edges.Right.End.ToPoint(), true),
                    new QuadraticBezierSegment(figure.Controls.BottomRight.ToPoint(), figure.Edges.Bottom.Start.ToPoint(), true),
                    new LineSegment(figure.Edges.Bottom.End.ToPoint(), true),
                    new QuadraticBezierSegment(figure.Controls.BottomLeft.ToPoint(), figure.Edges.Left.Start.ToPoint(), true),
                ],
                true),
        ]);
    }

    public static GuidelineSet ToGuidelineSet(this Guidelines guidelines)
    {
        var guidelineSet = new GuidelineSet();
        foreach (var x in guidelines.X)
        {
            guidelineSet.GuidelinesX.Add(x);
        }

        foreach (var y in guidelines.Y)
        {
            guidelineSet.GuidelinesY.Add(y);
        }

        return guidelineSet;
    }
}
