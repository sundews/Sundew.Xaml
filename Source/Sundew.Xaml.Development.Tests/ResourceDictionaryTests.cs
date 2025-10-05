// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceDictionaryTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Wpf.Development.Tests;

using System;
using System.Linq;
using AwesomeAssertions;
using Xunit;

public class ResourceDictionaryTests : IDisposable
{
    static ResourceDictionaryTests()
    {
        WpfApplication.Initialize();
        WpfApplication.Current.ToString();
    }

    [Fact]
    public void Source_Then_SourceShouldBeSetAndItemsLoaded()
    {
        var testee = new Tests.ResourceDictionary();

        testee.Source = GetTesteeUri();

        testee.Source.Should().Be(GetTesteeUri());
        testee.MergedDictionaries.Should().NotBeEmpty().And.Subject.First().Count.Should().NotBe(0);
    }

    [Fact]
    public void Source_When_UriAlreadyLoaded_Then_MergedDictionariesShouldContainExpectedResourceDictionary()
    {
        var expectedResourceDictionary = new Tests.ResourceDictionary { Source = GetTesteeUri() };
        var testee = new Tests.ResourceDictionary();

        testee.Source = GetTesteeUri();

        testee.MergedDictionaries.Should().Contain(expectedResourceDictionary.MergedDictionaries.FirstOrDefault());
        ResourceDictionary.CachedDictionaries.Count.Should().Be(1);
    }

    [Fact]
    public void Source_When_AlreadySet_Then_MergedDictionariesShouldBeChangedToNewSource()
    {
        var expectedResourceDictionary = new Tests.ResourceDictionary { Source = GetTesteeUri() };
        var testee = new Tests.ResourceDictionary { Source = GetTesteeUri() };
        var firstMergedDictionary = testee.MergedDictionaries.FirstOrDefault();

        testee.Source = GetSecondUri();

        firstMergedDictionary.Should().NotBeNull();
        testee.MergedDictionaries.Should().NotContain(firstMergedDictionary);
        testee.MergedDictionaries.Should().NotContain(expectedResourceDictionary.MergedDictionaries.FirstOrDefault());
        testee.MergedDictionaries.Should().NotBeEmpty().And.Subject.Should().NotBeEmpty();
        ResourceDictionary.CachedDictionaries.Count.Should().Be(2);
    }

    [Fact]
    public void Source_When_AlreadySetAndIsFirstReference_Then_MergedDictionariesShouldBeChangedToNewSource()
    {
        var testee = new Tests.ResourceDictionary { Source = GetTesteeUri() };
        var expectedResourceDictionary = new Tests.ResourceDictionary { Source = GetTesteeUri() };
        var firstMergedDictionary = testee.MergedDictionaries.FirstOrDefault();

        testee.Source = GetSecondUri();

        firstMergedDictionary.Should().NotBeNull();
        testee.MergedDictionaries.Should().NotContain(firstMergedDictionary);
        testee.MergedDictionaries.Should().NotContain(expectedResourceDictionary.MergedDictionaries.FirstOrDefault());
        testee.MergedDictionaries.Should().NotBeEmpty().And.Subject.Should().NotBeEmpty();
        ResourceDictionary.CachedDictionaries.Count.Should().Be(2);
    }

    [Fact]
    public void Source_When_AlreadySet_Then_MergedDictionariesShouldNotContainOldDictionary()
    {
        var testee = new Tests.ResourceDictionary { Source = GetTesteeUri() };
        var oldMergedDictionary = testee.MergedDictionaries.FirstOrDefault();

        testee.Source = GetSecondUri();

        testee.MergedDictionaries.Should().NotContain(oldMergedDictionary);
        testee.MergedDictionaries.Should().NotBeEmpty();
        ResourceDictionary.CachedDictionaries.Count.Should().Be(1);
    }

    [Fact]
    public void TryRemoveFromCache_Then_CachedResourceDictionaryShouldNotLongerExistInCache()
    {
        var testee = new Tests.ResourceDictionary { Source = GetTesteeUri() };
        testee = null;
        GC.Collect(2, GCCollectionMode.Forced);
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();

        ResourceDictionary.TryRemoveFromCache(GetTesteeUri());

        ResourceDictionary.CachedDictionaries.Count.Should().Be(0);
    }

    [Fact]
    public void Indexer_Then_ResultShouldNotBeNull()
    {
        var testee = new System.Windows.ResourceDictionary { Source = GetTesteeUri() };

        var result = testee["Brush"];

        result.Should().NotBeNull();
    }

    public void Dispose()
    {
        ResourceDictionary.CachedDictionaries.Clear();
        GC.Collect(2, GCCollectionMode.Forced);
        GC.WaitForPendingFinalizers();
    }

    private static Uri GetTesteeUri()
    {
        return new Uri(
            "/Sundew.Xaml.Wpf.Development.Tests;component/SampleResourceDictionary.xaml",
            UriKind.Relative);
    }

    private static Uri GetSecondUri()
    {
        return new Uri(
            "/Sundew.Xaml.Wpf.Development.Tests;component/SampleResourceDictionary2.xaml",
            UriKind.Relative);
    }
}