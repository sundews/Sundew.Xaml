// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceDictionaryBase.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Theming;

using System;
using System.Collections;
using System.Collections.Concurrent;
using SystemResourceDictionary = System.Windows.ResourceDictionary;

/// <summary>
/// A ResourceDictionary that ensures that a source is only loaded once and otherwise retrieved from a cache.
/// </summary>
/// <seealso cref="SystemResourceDictionary" />
/// <typeparam name="TResourceDictionary">The resource dictionary type.</typeparam>
public abstract class ResourceDictionaryBase<TResourceDictionary> : SystemResourceDictionary
{
    private SystemResourceDictionary? resourceDictionary;

    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    /// <value>
    /// The source.
    /// </value>
    public new Uri? Source
    {
        get => this.resourceDictionary?.Source;

        set
        {
            var oldSource = this.Source;
            if (oldSource == value)
            {
                return;
            }

            if (this.resourceDictionary is { } oldResourceDictionary)
            {
                this.MergedDictionaries.Remove(oldResourceDictionary);

                var wasUpdated = false;
                Entry lastEntry = default;
                while (!wasUpdated)
                {
                    if (!ResourceDictionaries.TryGetValue(oldResourceDictionary.Source, out lastEntry))
                    {
                        break;
                    }

                    var newEntry = lastEntry.ReferenceCount > 1 ? lastEntry with { ReferenceCount = lastEntry.ReferenceCount - 1 } : default;
                    wasUpdated = ResourceDictionaries.TryUpdate(oldResourceDictionary.Source, newEntry, lastEntry);
                    lastEntry = newEntry;
                }

                if (wasUpdated && lastEntry.SourceResourceDictionary == null)
                {
                    ResourceDictionaries.TryRemove(oldResourceDictionary.Source, out _);
                }
            }

            if (value != null)
            {
                var lazy = new Lazy<SystemResourceDictionary>(() => this.resourceDictionary = new SystemResourceDictionary { Source = value });
                var newEntry = ResourceDictionaries.AddOrUpdate(
                    value,
                    uri =>
                    {
                        var newEntry = new Entry(lazy.Value, 1);
                        return newEntry;
                    },
                    (uri, existingEntry) => existingEntry with
                    {
                        ReferenceCount = existingEntry.ReferenceCount + 1,
                    });
                this.resourceDictionary = newEntry.SourceResourceDictionary;
                this.MergedDictionaries.Add(newEntry.SourceResourceDictionary);
            }
        }
    }

    /// <summary>
    /// Gets the collection of cached resource dictionaries used by the application.
    /// </summary>
    internal static IDictionary CachedDictionaries => ResourceDictionaries;

    /// <summary>
    /// Gets a thread-safe collection of resource dictionary entries, indexed by their URI.
    /// </summary>
    /// <remarks>This collection is shared across all instances and can be used to retrieve or manage resource
    /// dictionaries by their unique identifiers. Access to this property is intended for derived classes that need to
    /// interact with the global resource dictionary cache.</remarks>
    protected static ConcurrentDictionary<Uri, Entry> ResourceDictionaries { get; } = new();

    /// <summary>
    /// Tries to remove a resources by <see cref="Uri"/> from the cache, if there are no references.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns><c>true</c>, if the item could be removed, otherwise <c>false</c>.</returns>
    public static bool TryRemoveFromCache(Uri source)
    {
        var newEntry = ResourceDictionaries.AddOrUpdate(source, uri => default, (uri, oldEntry) =>
        {
            return oldEntry.ReferenceCount switch
            {
                0 or 1 => default,
                _ => oldEntry,
            };
        });

        return newEntry.SourceResourceDictionary == null && ResourceDictionaries.TryRemove(source, out _);
    }

    /// <summary>
    /// Represents an entry that associates a system resource dictionary with its reference count.
    /// </summary>
    /// <param name="SourceResourceDictionary">The system resource dictionary associated with this entry. Can be null if no dictionary is assigned.</param>
    /// <param name="ReferenceCount">The number of references to the associated resource dictionary. Must be zero or greater.</param>
    protected readonly record struct Entry(SystemResourceDictionary? SourceResourceDictionary, int ReferenceCount);
}