/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    13 Nov 2023
 * 
 * File:    MapUtils.cs
 * Purpose: Dictionary modification utilities.
 */

#if NETSTANDARD || NET_STANDARD
    #define EXTENDED_COLLECTIONS_METHODS
#else
    #define USE_REFLECTED_METHODS
#endif // NETSTANDARD || NET_STANDARD

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Utility methods for dealing with dictionaries.
    /// </summary>
    static public class MapUtils
    {
#if USE_REFLECTED_METHODS
        static private readonly Type[] s_ResizeTypes = new Type[] { typeof(int), typeof(bool) };
#endif // USE_REFLECTED_METHODS

        static private class MethodCache<TKey, TValue>
        {
            static internal FieldInfo GetBucketField;
#if USE_REFLECTED_METHODS
            static internal MethodInfo InitializeMethod;
            static internal MethodInfo ResizeMethod;
#endif // USE_REFLECTED_METHODS

            static MethodCache()
            { 
                Type t = typeof(Dictionary<TKey, TValue>);
                GetBucketField = t.GetField("_buckets", BindingFlags.Instance | BindingFlags.NonPublic);
                if (GetBucketField == null)
                {
                    GetBucketField = t.GetField("buckets", BindingFlags.Instance | BindingFlags.NonPublic);
                }
#if USE_REFLECTED_METHODS
                InitializeMethod = t.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic);
                ResizeMethod = t.GetMethod("Resize", BindingFlags.Instance | BindingFlags.NonPublic, null, s_ResizeTypes, Array.Empty<ParameterModifier>());
#endif // USE_REFLECTED_METHODS
            }
        }

        /// <summary>
        /// Returns the current bucket capacity of the given dictionary.
        /// Note that this uses reflection, so do not call it in a hot loop.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int GetCapacity<TKey, TValue>(this Dictionary<TKey, TValue> inDictionary)
        {
            Array entries = (Array) MethodCache<TKey, TValue>.GetBucketField.GetValue(inDictionary);
            return entries != null ? entries.Length : 0;
        }

        /// <summary>
        /// Ensures the capacity of the given dictionary.
        /// In versions of .NET that don't support the built-in EnsureCapacity method.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void EnsureCapacity<TKey, TValue>(this Dictionary<TKey, TValue> inDictionary, int inCapacity)
        {
#if EXTENDED_COLLECTIONS_METHODS
            inDictionary.EnsureCapacity(inCapacity);
#elif USE_REFLECTED_METHODS
            Array entries = (Array) MethodCache<TKey, TValue>.GetBucketField.GetValue(inDictionary);
            if (entries == null)
            {
                MethodCache<TKey, TValue>.InitializeMethod.Invoke(inDictionary, new object[] { inCapacity });
            }
            else if (entries.Length < inCapacity) 
            {
                MethodCache<TKey, TValue>.ResizeMethod.Invoke(inDictionary, new object[] { InternalHashUtils.GetPrime(inCapacity), false });
            }
#endif // !EXTENDED_COLLECTIONS_METHODS
        }

        /// <summary>
        /// Ensures the capacity of the given dictionary.
        /// In versions of .NET that don't support the built-in EnsureCapacity method, this will use reflection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void EnsureCapacity<TKey, TValue>(ref Dictionary<TKey, TValue> ioDictionary, int inCapacity)
        {
            if (ioDictionary == null)
            {
                ioDictionary = Create<TKey, TValue>(inCapacity);
            }

            EnsureCapacity(ioDictionary, inCapacity);
        }

        /// <summary>
        /// Reserves space for the given number of elements.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Reserve<TKey, TValue>(this Dictionary<TKey, TValue> inDictionary, int inAdditionalCapacity)
        {
            EnsureCapacity(inDictionary, inDictionary.Count + inAdditionalCapacity);
        }

        /// <summary>
        /// Creates a new dictionary with the given capacity and default comparer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Dictionary<TKey, TValue> Create<TKey, TValue>(int inCapacity)
        {
            return new Dictionary<TKey, TValue>(inCapacity, CompareUtils.DefaultEquals<TKey>());
        }
    }
}