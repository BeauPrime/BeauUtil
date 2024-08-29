/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    15 Nov 2023
 * 
 * File:    SetUtils.cs
 * Purpose: HashSet modification utilities.
 */

#if NETSTANDARD || NET_STANDARD
    #define EXTENDED_COLLECTIONS_METHODS
#else
    #define USE_REFLECTED_METHODS
#endif // NETSTANDARD || NET_STANDARD

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Utility methods for dealing with hashsets.
    /// </summary>
    static public class SetUtils
    {
#if USE_REFLECTED_METHODS
        static private readonly Type[] s_ResizeTypes = new Type[] { typeof(int) };
#endif // USE_REFLECTED_METHODS

        static private class MethodCache<T>
        {
            static internal FieldInfo GetBucketField;
#if USE_REFLECTED_METHODS
            static internal MethodInfo InitializeMethod;
            static internal MethodInfo ResizeMethod;
#endif // USE_REFLECTED_METHODS

            static MethodCache()
            {
                Type t = typeof(HashSet<T>);
                GetBucketField = t.GetField("_buckets", BindingFlags.Instance | BindingFlags.NonPublic);
                if (GetBucketField == null)
                {
                    GetBucketField = t.GetField("buckets", BindingFlags.Instance | BindingFlags.NonPublic);
                }
#if USE_REFLECTED_METHODS
                InitializeMethod = t.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic);
                ResizeMethod = t.GetMethod("SetCapacity", BindingFlags.Instance | BindingFlags.NonPublic, null, s_ResizeTypes, Array.Empty<ParameterModifier>());
#endif // USE_REFLECTED_METHODS
            }
        }

        /// <summary>
        /// Returns the current bucket capacity of the given HashSet.
        /// Note that this uses reflection, so do not call it in a hot loop.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int GetCapacity<T>(this HashSet<T> inHashSet)
        {
            Array entries = (Array) MethodCache<T>.GetBucketField.GetValue(inHashSet);
            return entries != null ? entries.Length : 0;
        }

        /// <summary>
        /// Ensures the capacity of the given HashSet.
        /// Note that in versions of .NET that don't support the built-in EnsureCapacity method, this uses reflection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void EnsureCapacity<T>(this HashSet<T> inHashSet, int inCapacity)
        {
#if USE_REFLECTED_METHODS
            Array entries = (Array) MethodCache<T>.GetBucketField.GetValue(inHashSet);
            if (entries == null)
            {
                MethodCache<T>.InitializeMethod.Invoke(inHashSet, new object[] { inCapacity });
            }
            else if (entries.Length < inCapacity)
            {
                MethodCache<T>.ResizeMethod.Invoke(inHashSet, new object[] { InternalHashUtils.GetPrime(inCapacity) });
            }
#else
            inHashSet.EnsureCapacity(inCapacity);
#endif // USE_REFLECTED_METHODS
        }

        /// <summary>
        /// Ensures the capacity of the given HashSet.
        /// Note that in versions of .NET that don't support the built-in EnsureCapacity method, this uses reflection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void EnsureCapacity<T>(ref HashSet<T> ioHashSet, int inCapacity)
        {
            if (ioHashSet == null)
            {
                ioHashSet = Create<T>(inCapacity);
                return;
            }

            EnsureCapacity(ioHashSet, inCapacity);
        }

        /// <summary>
        /// Reserves space for the given number of elements.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Reserve<T>(this HashSet<T> inHashSet, int inAdditionalCapacity)
        {
            EnsureCapacity(inHashSet, inHashSet.Count + inAdditionalCapacity);
        }

        /// <summary>
        /// Creates a new HashSet with the given capacity and default comparer.
        /// Note that in versions of .NET that don't support the built-in capacity constructor, this uses reflection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public HashSet<T> Create<T>(int inCapacity)
        {
#if EXTENDED_COLLECTIONS_METHODS
            return new HashSet<T>(inCapacity, CompareUtils.DefaultEquals<T>());
#elif USE_REFLECTED_METHODS
            HashSet<T> hashSet = new HashSet<T>(CompareUtils.DefaultEquals<T>());
            MethodCache<T>.InitializeMethod.Invoke(hashSet, new object[] { inCapacity });
            return hashSet;
#endif // EXTENDED_COLLECTIONS_METHODS
        }
    }
}