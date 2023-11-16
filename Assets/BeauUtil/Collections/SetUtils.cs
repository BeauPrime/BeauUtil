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
#endif // NETSTANDARD || NET_STANDARD

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Utility methods for dealing with hashsets.
    /// </summary>
    static public class SetUtils
    {
#if !EXTENDED_COLLECTIONS_METHODS
        static private readonly Type[] s_ResizeTypes = new Type[] { typeof(int), typeof(bool) };
#endif // !NETSTANDARD

        static private class MethodCache<T>
        {
            static internal FieldInfo GetBucketField;
#if !EXTENDED_COLLECTIONS_METHODS
            static internal MethodInfo InitMethod;
            static internal MethodInfo ResizeMethod;
#endif // !EXTENDED_COLLECTIONS_METHODS

            static MethodCache()
            {
                Type t = typeof(HashSet<T>);
                GetBucketField = t.GetField("_buckets", BindingFlags.Instance | BindingFlags.NonPublic);
#if !EXTENDED_COLLECTIONS_METHODS
                InitMethod = t.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic);
                ResizeMethod = t.GetMethod("SetCapacity", BindingFlags.Instance | BindingFlags.NonPublic, null, s_ResizeTypes, Array.Empty<ParameterModifier>());
#endif // !EXTENDED_COLLECTIONS_METHODS
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
            return entries.Length;
        }

        /// <summary>
        /// Ensures the capacity of the given HashSet.
        /// Note that in versions of .NET that don't support the built-in EnsureCapacity method, this uses reflection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void EnsureCapacity<T>(this HashSet<T> inHashSet, int inCapacity)
        {
#if !EXTENDED_COLLECTIONS_METHODS
            if (GetCapacity(inHashSet) < inCapacity)
            {
                MethodCache<T>.ResizeMethod.Invoke(inHashSet, new object[] { InternalHashUtils.GetPrime(inCapacity), false });
            }
#else
            inHashSet.EnsureCapacity(inCapacity);
#endif // !EXTENDED_COLLECTIONS_METHODS
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

#if !EXTENDED_COLLECTIONS_METHODS
            if (GetCapacity(ioHashSet) < inCapacity)
            {
                MethodCache<T>.ResizeMethod.Invoke(ioHashSet, new object[] { InternalHashUtils.GetPrime(inCapacity), false });
            }
#else
            ioHashSet.EnsureCapacity(inCapacity);
#endif // !EXTENDED_COLLECTIONS_METHODS
        }

        /// <summary>
        /// Reserves space for the given number of elements.
        /// </summary>
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
#else
            HashSet<T> hashSet = new HashSet<T>(CompareUtils.DefaultEquals<T>());
            MethodCache<T>.InitMethod.Invoke(hashSet, new object[] { inCapacity });
            return hashSet;
#endif // EXTENDED_COLLECTIONS_METHODS
        }
    }
}