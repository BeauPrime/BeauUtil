/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    13 Nov 2023
 * 
 * File:    MapUtils.cs
 * Purpose: Dictionary modification utilities.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Utility methods for dealing with dictionaries.
    /// </summary>
    static public class MapUtils
    {
        static private Dictionary<Type, FieldInfo> s_GetBucketFieldCache;

        /// <summary>
        /// Returns the current bucket capacity of the given dictionary.
        /// Note that this uses reflection, so do not call it in a hot loop.
        /// </summary>
        static public int GetCapacity<TKey, TValue>(this Dictionary<TKey, TValue> inDictionary)
        {
            if (s_GetBucketFieldCache == null)
            {
                s_GetBucketFieldCache = new Dictionary<Type, FieldInfo>(13);
            }

            Type dictionaryType = inDictionary.GetType();
            if (!s_GetBucketFieldCache.TryGetValue(dictionaryType, out FieldInfo field))
            {
                field = dictionaryType.GetField("_entries", BindingFlags.Instance | BindingFlags.NonPublic);
                s_GetBucketFieldCache.Add(dictionaryType, field);
            }

            Array entries = (Array) field.GetValue(inDictionary);
            return entries.Length;
        }
    }
}