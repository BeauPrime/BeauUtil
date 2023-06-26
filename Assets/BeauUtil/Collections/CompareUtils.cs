/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 Sept 2022
 * 
 * File:    CompareUtils.cs
 * Purpose: Comparison utilities.
 */

using System;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Comparison utilities.
    /// </summary>
    static public class CompareUtils
    {
        static private readonly Type s_UnityObjectType = typeof(UnityEngine.Object);

        /// <summary>
        /// Retrieves the default comparer for the given type.
        /// For UnityEngine.Object-derived types this is more efficient than the standard comparer.
        /// </summary>
        static public IEqualityComparer<T> DefaultComparer<T>()
        {
            IEqualityComparer<T> comparer = Cache<T>.DefaultComparer;
            if (comparer == null)
            {
                Type type = typeof(T);
                if (type == typeof(string))
                {
                    comparer = (IEqualityComparer<T>) StringComparer.Ordinal;
                }
                else if (s_UnityObjectType.IsAssignableFrom(type))
                {
                    comparer = ReferenceEqualityComparer<T>.Default;
                }
                else
                {
                    comparer = EqualityComparer<T>.Default;
                }
                Cache<T>.DefaultComparer = comparer;
            }
            return comparer;
            
        }

        /// <summary>
        /// Manually specifies the default comparer for the given type.
        /// </summary>
        static public void OverwriteDefault<T>(IEqualityComparer<T> inDefault)
        {
            Cache<T>.DefaultComparer = inDefault;
        }

        static private class Cache<T>
        {
            static public IEqualityComparer<T> DefaultComparer;
        }
    }
}