/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    28 Sept 2022
 * 
 * File:    ReferenceEqualityComparer.cs
 * Purpose: Reference equality comparer.
 */

using System;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Equality Comparer using solely ReferenceEquals.
    /// </summary>
    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        static private ReferenceEqualityComparer<T> s_Instance;

        /// <summary>
        /// Default instance of ReferenceEqualityComparer.
        /// </summary>
        static public ReferenceEqualityComparer<T> Default
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new ReferenceEqualityComparer<T>();
                }
                return s_Instance;
            }
        }

        public bool Equals(T x, T y)
        {
            return Object.ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return Object.ReferenceEquals(obj, null) ? 0 : obj.GetHashCode();
        }

        public int IndexOf(T[] inArray, T inValue, int inStartIndex, int inCount)
        {
            int end = inStartIndex + inCount;
            for(int i = inStartIndex; i < end; i++)
            {
                if (Object.ReferenceEquals(inValue, inArray[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public int LastIndexOf(T[] inArray, T inValue, int inStartIndex, int inCount)
        {
            int end = inStartIndex - inCount + 1;
            for(int i = inStartIndex; i >= end; i--)
            {
                if (Object.ReferenceEquals(inValue, inArray[i]))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}