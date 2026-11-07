// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppearanceTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Wpf.Development.Tests;

using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AwesomeAssertions;
using Sundew.Xaml.Controls;
using Sundew.Xaml.Controls.Overlays;
using TUnit.Core;
using Border = System.Windows.Controls.Border;
using Visibility = Sundew.Xaml.Controls.Visibility;

[NotInParallel]
public class AppearanceTests
{
    private const int RenderSize = 8;

    [Test]
    public void SetVisibility_When_NothingWasSaid_Then_VisibilityShouldBeVisibleAndNothingShouldBeTakenOver()
    {
        Sta.Run(() =>
        {
            var testee = new Border();

            Appearance.GetVisibility(testee).Should().Be(Visibility.Visible);
            testee.ReadLocalValue(UIElement.VisibilityProperty).Should().BeSameAs(DependencyProperty.UnsetValue);
            testee.ReadLocalValue(UIElement.OpacityMaskProperty).Should().BeSameAs(DependencyProperty.UnsetValue);
        });
    }

    [Test]
    public void SetVisibility_When_Hidden_Then_ElementShouldBeHiddenAndUnmasked()
    {
        Sta.Run(() =>
        {
            var testee = new Border();

            Appearance.SetVisibility(testee, Visibility.Hidden);

            testee.Visibility.Should().Be(System.Windows.Visibility.Hidden);
            testee.OpacityMask.Should().BeNull();
        });
    }

    [Test]
    public void SetVisibility_When_Collapsed_Then_ElementShouldBeCollapsedAndUnmasked()
    {
        Sta.Run(() =>
        {
            var testee = new Border();

            Appearance.SetVisibility(testee, Visibility.Collapsed);

            testee.Visibility.Should().Be(System.Windows.Visibility.Collapsed);
            testee.OpacityMask.Should().BeNull();
        });
    }

    [Test]
    public void SetVisibility_When_Invisible_Then_ElementShouldBeVisibleAndNotDrawn()
    {
        Sta.Run(() =>
        {
            var testee = new Border();

            Appearance.SetVisibility(testee, Visibility.Invisible);

            testee.Visibility.Should().Be(System.Windows.Visibility.Visible);
            testee.OpacityMask.Should().BeOfType<SolidColorBrush>().Which.Color.A.Should().Be(0);
        });
    }

    [Test]
    public void SetVisibility_When_InvisibleAndThenCollapsed_Then_MaskShouldBeGivenBack()
    {
        Sta.Run(() =>
        {
            var testee = new Border();
            Appearance.SetVisibility(testee, Visibility.Invisible);

            Appearance.SetVisibility(testee, Visibility.Collapsed);

            testee.Visibility.Should().Be(System.Windows.Visibility.Collapsed);
            testee.ReadLocalValue(UIElement.OpacityMaskProperty).Should().BeSameAs(DependencyProperty.UnsetValue);
        });
    }

    [Test]
    public void SetVisibility_When_InvisibleAndThenVisible_Then_ElementShouldBeAsItWas()
    {
        Sta.Run(() =>
        {
            var testee = new Border();
            Appearance.SetVisibility(testee, Visibility.Invisible);

            Appearance.SetVisibility(testee, Visibility.Visible);

            testee.Visibility.Should().Be(System.Windows.Visibility.Visible);
            testee.ReadLocalValue(UIElement.OpacityMaskProperty).Should().BeSameAs(DependencyProperty.UnsetValue);
        });
    }

    [Test]
    public void SetVisibility_When_CollapsedAndThenInvisible_Then_ElementShouldBeVisibleAndNotDrawn()
    {
        Sta.Run(() =>
        {
            var testee = new Border();
            Appearance.SetVisibility(testee, Visibility.Collapsed);

            Appearance.SetVisibility(testee, Visibility.Invisible);

            testee.Visibility.Should().Be(System.Windows.Visibility.Visible);
            testee.OpacityMask.Should().NotBeNull();
        });
    }

    [Test]
    public void SetVisibility_When_SaidInAStyle_Then_ElementShouldNotBeDrawn()
    {
        Sta.Run(() =>
        {
            var testee = new Border
            {
                Style = new Style(typeof(Border))
                {
                    Setters = { new Setter(Appearance.VisibilityProperty, Visibility.Invisible) },
                },
            };

            testee.Measure(new Size(10, 10));

            Appearance.GetVisibility(testee).Should().Be(Visibility.Invisible);
            testee.OpacityMask.Should().NotBeNull();
        });
    }

    [Test]
    public void SetVisibility_When_Invisible_Then_ElementShouldStillBeHitTested()
    {
        Sta.Run(() =>
        {
            var root = new Grid { Background = Brushes.White };
            var testee = new Border { Background = Brushes.Black, Width = 20, Height = 20 };
            root.Children.Add(testee);
            Arrange(root, new Size(40, 40));

            Appearance.SetVisibility(testee, Visibility.Invisible);
            Arrange(root, new Size(40, 40));

            var hitTestResult = VisualTreeHelper.HitTest(root, new Point(20, 20));

            hitTestResult.Should().NotBeNull();
            hitTestResult.VisualHit.Should().BeSameAs(testee);
        });
    }

    [Test]
    public void SetVisibility_When_Visible_Then_TheElementShouldBeDrawn()
    {
        Sta.Run(() =>
        {
            var root = new Grid { Background = Brushes.White };
            var testee = new Border { Background = Brushes.Black };
            root.Children.Add(testee);

            Appearance.SetVisibility(testee, Visibility.Visible);
            Arrange(root, new Size(RenderSize, RenderSize));

            GetCenterPixel(root).Should().Be(Colors.Black);
        });
    }

    [Test]
    public void SetVisibility_When_Invisible_Then_NothingOfTheElementShouldBeDrawn()
    {
        Sta.Run(() =>
        {
            var root = new Grid { Background = Brushes.White };
            var testee = new Border { Background = Brushes.Black };
            root.Children.Add(testee);

            Appearance.SetVisibility(testee, Visibility.Invisible);
            Arrange(root, new Size(RenderSize, RenderSize));

            GetCenterPixel(root).Should().Be(Colors.White);
        });
    }

    [Test]
    public void Convert_When_StringIsNotEmpty_Then_ResultShouldBeVisible()
    {
        new StringNotNullOrEmptyToVisibilityConverter()
            .Convert("a subtitle", typeof(Visibility), null, CultureInfo.InvariantCulture)
            .Should().Be(Visibility.Visible);
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    public void Convert_When_StringIsNullOrEmpty_Then_ResultShouldBeCollapsed(string? value)
    {
        new StringNotNullOrEmptyToVisibilityConverter()
            .Convert(value, typeof(Visibility), null, CultureInfo.InvariantCulture)
            .Should().Be(Visibility.Collapsed);
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    public void Convert_When_NullOrEmptyAndNotVisibleVisibilityWasSaid_Then_ResultShouldBeThatVisibility(string? value)
    {
        new StringNotNullOrEmptyToVisibilityConverter { NotVisibleVisibility = Visibility.Invisible }
            .Convert(value, typeof(Visibility), null, CultureInfo.InvariantCulture)
            .Should().Be(Visibility.Invisible);
    }

    [Test]
    public void Convert_When_NotEmptyAndNotVisibleVisibilityWasSaid_Then_ResultShouldStillBeVisible()
    {
        new StringNotNullOrEmptyToVisibilityConverter { NotVisibleVisibility = Visibility.Invisible }
            .Convert("a subtitle", typeof(Visibility), null, CultureInfo.InvariantCulture)
            .Should().Be(Visibility.Visible);
    }

    [Test]
    public void Convert_When_ValueIsNotAString_Then_ResultShouldBeTheNotVisibleVisibility()
    {
        new StringNotNullOrEmptyToVisibilityConverter { NotVisibleVisibility = Visibility.Hidden }
            .Convert(42, typeof(Visibility), null, CultureInfo.InvariantCulture)
            .Should().Be(Visibility.Hidden);
    }

    [Test]
    public void NotVisibleVisibility_When_NothingWasSaid_Then_ItShouldBeCollapsed()
    {
        new StringNotNullOrEmptyToVisibilityConverter().NotVisibleVisibility.Should().Be(Visibility.Collapsed);
    }

    // The property is said in XAML by name, which is how it is meant to be reached, so that it parses at all is
    // part of what it is rather than something to take on trust.
    [Test]
    public void NotVisibleVisibility_When_SaidInXaml_Then_ItShouldBeThatVisibility()
    {
        Sta.Run(() =>
        {
            const string xaml = """
                <sx:StringNotNullOrEmptyToVisibilityConverter
                    xmlns:sx="clr-namespace:Sundew.Xaml.Controls;assembly=Sundew.Xaml.Controls.Wpf"
                    NotVisibleVisibility="Invisible"/>
                """;

            var testee = (StringNotNullOrEmptyToVisibilityConverter)XamlReader.Parse(xaml);

            testee.NotVisibleVisibility.Should().Be(Visibility.Invisible);
        });
    }

    [Test]
    public void Convert_Then_ResultShouldBeOurVisibilityAndNotTheFrameworks()
    {
        new StringNotNullOrEmptyToVisibilityConverter()
            .Convert("a subtitle", typeof(Visibility), null, CultureInfo.InvariantCulture)
            .Should().BeOfType<Visibility>();
    }

    [Test]
    public void SubtitleOverlayTemplate_Then_TheRootShouldSayItsVisibilityThroughAppearance()
    {
        Sta.Run(() =>
        {
            var template = GetSubtitleOverlayTemplate();

            var root = (DependencyObject)template.LoadContent();

            root.Should().BeOfType<Grid>();
            root.ReadLocalValue(Appearance.VisibilityProperty).Should().NotBeSameAs(DependencyProperty.UnsetValue);
            root.ReadLocalValue(UIElement.VisibilityProperty).Should().BeSameAs(DependencyProperty.UnsetValue);
        });
    }

    private static void Arrange(UIElement element, Size size)
    {
        element.Measure(size);
        element.Arrange(new Rect(size));
        element.UpdateLayout();
    }

    private static Color GetCenterPixel(Visual visual)
    {
        var renderTargetBitmap = new RenderTargetBitmap(RenderSize, RenderSize, 96, 96, PixelFormats.Pbgra32);
        renderTargetBitmap.Render(visual);
        var pixel = new byte[4];
        renderTargetBitmap.CopyPixels(new Int32Rect(RenderSize / 2, RenderSize / 2, 1, 1), pixel, 4, 0);
        return Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]);
    }

    // Walks down from the overlay to the one element the template says the visibility of, which is how the
    // template binding is checked without showing a window.
    private static UIElement? GetTemplatedRoot(DependencyObject root)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is UIElement element
                && element.ReadLocalValue(Appearance.VisibilityProperty) != DependencyProperty.UnsetValue)
            {
                return element;
            }

            var found = GetTemplatedRoot(child);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static ControlTemplate GetSubtitleOverlayTemplate()
    {
        var themes = new System.Windows.ResourceDictionary
        {
            Source = new Uri(
                "pack://application:,,,/Sundew.Xaml.Controls.Wpf;component/Themes/Generic.xaml",
                UriKind.Absolute),
        };
        var style = (Style)themes[typeof(SubtitleOverlay)];
        return (ControlTemplate)style.Setters.OfType<Setter>()
            .Single(setter => setter.Property == Control.TemplateProperty).Value;
    }
}
