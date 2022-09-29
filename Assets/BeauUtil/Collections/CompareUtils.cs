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
        static private readonly Type s_ObjectType = typeof(UnityEngine.Object);

        static public IEqualityComparer<T> DefaultComparer<T>()
        {
            IEqualityComparer<T> comparer = Cache<T>.DefaultComparer;
            if (comparer == null)
            {
                if (s_ObjectType.IsAssignableFrom(typeof(T)))
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

        static private class Cache<T>
        {
            static public IEqualityComparer<T> DefaultComparer;
        }
    }
}