/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    MapEntry.cs
 * Purpose: Serializable map entries.
*/

using System;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Interface for a key-value pair.
    /// </summary>
    public interface IKeyValuePair<K, V> where K : IEquatable<K>
    {
        K Key { get; }
        V Value { get; }
    }

    /// <summary>
    /// Serializable map entry.
    /// Create subclass and mark serializable to use in the inspector.
    /// </summary>
    public abstract class SerializableKeyValuePair<K, V> : IKeyValuePair<K, V> where K : IEquatable<K>
    {
        public K Key;
        public V Value;

        K IKeyValuePair<K, V>.Key { get { return Key; } }

        V IKeyValuePair<K, V>.Value { get { return Value; } }
    }

    /// <summary>
    /// Utility functions for dealing with map entries.
    /// </summary>
    static public class KeyValueUtils
    {
        /// <summary>
        /// Creates a map out from a collection of key-value pairs.
        /// </summary>
        static public Dictionary<K, V> CreateMap<K, V, T>(this ICollection<T> inCollection)
            where K : IEquatable<K>
            where T : IKeyValuePair<K, V>
        {
            Dictionary<K, V> dict = new Dictionary<K, V>(inCollection.Count);
            foreach (var entry in inCollection)
            {
                dict.Add(entry.Key, entry.Value);
            }
            return dict;
        }

        /// <summary>
        /// Creates a map out from an array of key-value pairs.
        /// </summary>
        static public Dictionary<K, V> CreateMap<K, V, T>(this T[] inCollection)
            where K : IEquatable<K>
            where T : IKeyValuePair<K, V>
        {
            Dictionary<K, V> dict = new Dictionary<K, V>(inCollection.Length);
            foreach (var entry in inCollection)
            {
                dict.Add(entry.Key, entry.Value);
            }
            return dict;
        }

        /// <summary>
        /// Attempts to retrieve a value from a collection of key-value pairs.
        /// </summary>
        static public bool TryGetValue<K, V, T>(this ICollection<T> inCollection, K inKey, out V outValue)
            where K : IEquatable<K>
            where T : IKeyValuePair<K, V>
        {
            foreach (var entry in inCollection)
            {
                if (entry.Key.Equals(inKey))
                {
                    outValue = entry.Value;
                    return true;
                }
            }

            outValue = default(V);
            return false;
        }

        /// <summary>
        /// Attempts to retrieve a value from an array of key-value pairs.
        /// </summary>
        static public bool TryGetValue<K, V, T>(this T[] inCollection, K inKey, out V outValue)
            where K : IEquatable<K>
            where T : IKeyValuePair<K, V>
        {
            foreach (var entry in inCollection)
            {
                if (entry.Key.Equals(inKey))
                {
                    outValue = entry.Value;
                    return true;
                }
            }

            outValue = default(V);
            return false;
        }

        /// <summary>
        /// Validates that all keys in the given collection of key-value pairs are unique.
        /// </summary>
        static public bool ValidateKeys<K, V, T>(this ICollection<T> inCollection)
            where K : IEquatable<K>
            where T : IKeyValuePair<K, V>
        {
            HashSet<K> keys = new HashSet<K>();
            foreach (var entry in inCollection)
            {
                if (!keys.Add(entry.Key))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that all keys in the given collection of key-value pairs are unique.
        /// </summary>
        static public bool ValidateKeys<K, V, T>(this T[] inCollection)
            where K : IEquatable<K>
            where T : IKeyValuePair<K, V>
        {
            HashSet<K> keys = new HashSet<K>();
            foreach (var entry in inCollection)
            {
                if (!keys.Add(entry.Key))
                    return false;
            }

            return true;
        }
    }
}