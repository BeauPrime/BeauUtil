/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    KeyValue.cs
 * Purpose: Serializable key-value pair entries.
*/

using System;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Interface for a key-value pair.
    /// </summary>
    public interface IKeyValuePair<K, V>
    {
        K Key { get; }
        V Value { get; }
    }

    /// <summary>
    /// Serializable map entry.
    /// Create subclass and mark serializable to use in the inspector.
    /// </summary>
    public abstract class SerializableKeyValuePair<K, V> : IKeyValuePair<K, V>
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
        /// Creates a map out from a collection of key-value pairs.
        /// </summary>
        static public Dictionary<K, V> CreateMap<K, V>(this ICollection<V> inCollection)
            where V : IKeyValuePair<K, V>
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
        /// Creates a map out from an array of key-value pairs.
        /// </summary>
        static public Dictionary<K, V> CreateMap<K, V>(this V[] inCollection)
            where V : IKeyValuePair<K, V>
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
            where T : IKeyValuePair<K, V>
        {
            var keyComparer = EqualityComparer<K>.Default;
            foreach (var entry in inCollection)
            {
                if (keyComparer.Equals(entry.Key, inKey))
                {
                    outValue = entry.Value;
                    return true;
                }
            }

            outValue = default(V);
            return false;
        }

        /// <summary>
        /// Attempts to retrieve a value from a collection of key-value pairs.
        /// </summary>
        static public bool TryGetValue<K, V>(this ICollection<V> inCollection, K inKey, out V outValue)
            where V : IKeyValuePair<K, V>
        {
            var keyComparer = EqualityComparer<K>.Default;
            foreach (var entry in inCollection)
            {
                if (keyComparer.Equals(entry.Key, inKey))
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
            where T : IKeyValuePair<K, V>
        {
            var keyComparer = EqualityComparer<K>.Default;
            foreach (var entry in inCollection)
            {
                if (keyComparer.Equals(entry.Key, inKey))
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
        static public bool TryGetValue<K, V>(this V[] inCollection, K inKey, out V outValue)
            where V : IKeyValuePair<K, V>
        {
            var keyComparer = EqualityComparer<K>.Default;
            foreach (var entry in inCollection)
            {
                if (keyComparer.Equals(entry.Key, inKey))
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
        static public bool ValidateKeys<K, V>(this ICollection<V> inCollection)
            where V : IKeyValuePair<K, V>
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
        static public bool ValidateKeys<K, V>(this V[] inCollection)
            where V : IKeyValuePair<K, V>
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
        /// Sorts the given array by key.
        /// </summary>
        static public void SortByKey<K, V, T>(this T[] inArray)
            where T : IKeyValuePair<K, V>
            where K : IComparable<K>
        {
            Array.Sort(inArray, KeySorter<K, V, T>.KeyComparer);
        }

        /// <summary>
        /// Sorts the given array by key.
        /// </summary>
        static public void SortByKey<K, V>(this V[] inArray)
            where V : IKeyValuePair<K, V>
            where K : IComparable<K>
        {
            Array.Sort(inArray, KeySorter<K, V, V>.KeyComparer);
        }

        /// <summary>
        /// Sorts the given list by key.
        /// </summary>
        static public void SortByKey<K, V, T>(this List<T> inList)
            where T : IKeyValuePair<K, V>
            where K : IComparable<K>
        {
            inList.Sort(KeySorter<K, V, T>.KeyComparer);
        }

        /// <summary>
        /// Sorts the given list by key.
        /// </summary>
        static public void SortByKey<K, V>(this List<V> inList)
            where V : IKeyValuePair<K, V>
            where K : IComparable<K>
        {
            inList.Sort(KeySorter<K, V, V>.KeyComparer);
        }

        /// <summary>
        /// Returns the index at which a sorted list of key value pairs a certain key can be found.
        /// </summary>
        static public int BinarySearch<K, V, T>(this IReadOnlyList<T> inCollection, K inKey)
            where T : IKeyValuePair<K, V>
            where K : IComparable<K>
        {
            if (inCollection.Count <= 0)
                return -1;

            int low = 0;
            int high = inCollection.Count - 1;

            Comparer<K> comparer = Comparer<K>.Default;

            while(low <= high)
            {
                int med = low + ((high - low) >> 1);
                int comp = comparer.Compare(inCollection[med].Key, inKey);
                if (comp == 0)
                    return med;
                if (comp == -1)
                    low = med + 1;
                else
                    high = med - 1;
            }

            return ~low;
        }

        /// <summary>
        /// Returns the index at which a sorted list of key value pairs a certain key can be found.
        /// </summary>
        static public int BinarySearch<K, V>(this IReadOnlyList<V> inCollection, K inKey)
            where V : IKeyValuePair<K, V>
            where K : IComparable<K>
        {
            if (inCollection.Count <= 0)
                return -1;

            int low = 0;
            int high = inCollection.Count - 1;

            Comparer<K> comparer = Comparer<K>.Default;

            while(low <= high)
            {
                int med = low + ((high - low) >> 1);
                int comp = comparer.Compare(inCollection[med].Key, inKey);
                if (comp == 0)
                    return med;
                if (comp == -1)
                    low = med + 1;
                else
                    high = med - 1;
            }

            return ~low;
        }

        /// <summary>
        /// Attempts to find the value associated with the given key, assuming the collection is sorted.
        /// </summary>
        static public bool TryBinarySearch<K, V, T>(this IReadOnlyList<T> inCollection, K inKey, out V outValue)
            where T : IKeyValuePair<K, V>
            where K : IComparable<K>
        {
            int idx = BinarySearch<K, V, T>(inCollection, inKey);
            if (idx >= 0)
            {
                outValue = inCollection[idx].Value;
                return true;
            }

            outValue = default(V);
            return false;
        }

        /// <summary>
        /// Attempts to find the value associated with the given key, assuming the collection is sorted.
        /// </summary>
        static public bool TryBinarySearch<K, V>(this IReadOnlyList<V> inCollection, K inKey, out V outValue)
            where V : IKeyValuePair<K, V>
            where K : IComparable<K>
        {
            int idx = BinarySearch<K, V, V>(inCollection, inKey);
            if (idx >= 0)
            {
                outValue = inCollection[idx].Value;
                return true;
            }

            outValue = default(V);
            return false;
        }

        static private class KeySorter<K, V, T> where T : IKeyValuePair<K, V> where K : IComparable<K>
        {
            static private Comparison<T> s_CachedComparison;

            static internal Comparison<T> KeyComparer
            {
                get
                {
                    if (s_CachedComparison == null)
                    {
                        if (typeof(T).IsClass || typeof(T).IsInterface)
                        {
                            s_CachedComparison = CompareClass;
                        }
                        else
                        {
                            s_CachedComparison = CompareStruct;
                        }
                    }

                    return s_CachedComparison;
                }
            }

            static private int CompareStruct(T x, T y)
            {
                return x.Key.CompareTo(y.Key);
            }

            static private int CompareClass(T x, T y)
            {
                if (x == null)
                {
                    if (y == null)
                        return 0;

                    return -1;
                }

                if (y == null)
                {
                    return 1;
                }

                return x.Key.CompareTo(y.Key);
            }
        }
    }
}