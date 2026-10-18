// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrimitiveExtensions.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using global::Avalonia;
using global::Avalonia.Media;

/// <summary>
/// Translates between the platform neutral border primitives and Avalonia.
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

    public static Point ToPoint(this Position position)
    {
        return new Point(position.X, position.Y);
    }

    public static RelativePoint ToAbsolutePoint(this Position position)
    {
        return new RelativePoint(position.X, position.Y, RelativeUnit.Absolute);
    }

    public static Geometry ToGeometry(this SideQuad sideQuad)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(sideQuad.Outer.Start.ToPoint(), true);
            context.LineTo(sideQuad.Outer.End.ToPoint());
            context.LineTo(sideQuad.Inner.End.ToPoint());
            context.LineTo(sideQuad.Inner.Start.ToPoint());
            context.EndFigure(true);
        }

        return geometry;
    }

    public static Geometry ToGeometry(this CornerWedge cornerWedge)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(cornerWedge.Outer.Start.ToPoint(), true);
            context.QuadraticBezierTo(cornerWedge.Outer.Control.ToPoint(), cornerWedge.Outer.End.ToPoint());
            context.LineTo(cornerWedge.Inner.Start.ToPoint());
            context.QuadraticBezierTo(cornerWedge.Inner.Control.ToPoint(), cornerWedge.Inner.End.ToPoint());
            context.EndFigure(true);
        }

        return geometry;
    }

    public static Geometry ToGeometry(this RoundedFigure figure)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(figure.Start.ToPoint(), true);
            context.LineTo(figure.Edges.Left.End.ToPoint());
            context.QuadraticBezierTo(figure.Controls.TopLeft.ToPoint(), figure.Edges.Top.Start.ToPoint());
            context.LineTo(figure.Edges.Top.End.ToPoint());
            context.QuadraticBezierTo(figure.Controls.TopRight.ToPoint(), figure.Edges.Right.Start.ToPoint());
            context.LineTo(figure.Edges.Right.End.ToPoint());
            context.QuadraticBezierTo(figure.Controls.BottomRight.ToPoint(), figure.Edges.Bottom.Start.ToPoint());
            context.LineTo(figure.Edges.Bottom.End.ToPoint());
            context.QuadraticBezierTo(figure.Controls.BottomLeft.ToPoint(), figure.Edges.Left.Start.ToPoint());
            context.EndFigure(true);
        }

        return geometry;
    }
}
