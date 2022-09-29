/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    28 Sept 2022
 * 
 * File:    ReferenceEqualityComparer.cs
 * Purpose: Reference equality comparer.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Equality Comparer using solely ReferenceEquals.
    /// </summary>
    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer
    {
        /// <summary>
        /// Default ReferenceEqualityComparer for this type.
        /// </summary>
        static public readonly ReferenceEqualityComparer<T> Default = new ReferenceEqualityComparer<T>();

        public bool Equals(T x, T y)
        {
            return Object.ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
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

        bool IEqualityComparer.Equals(object x, object y) {
            return object.Equals(x, y);
        }

        int IEqualityComparer.GetHashCode(object obj) {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}