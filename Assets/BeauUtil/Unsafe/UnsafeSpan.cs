/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    5 July 2023
 * 
 * File:    UnsafeSpan.cs
 * Purpose: Unsafe range wrapper.
 */

using System;

namespace BeauUtil
{
    /// <summary>
    /// Memory span wrapper.
    /// Useful for passing pointers in code where unsafe pointers
    /// are not allowed (such as in enumerator methods and coroutines)
    /// </summary>
    public struct UnsafeSpan<T> : IEquatable<UnsafeSpan<T>>, IComparable<UnsafeSpan<T>>
        where T : unmanaged
    {
        public readonly unsafe T* Ptr;
        public readonly int Length;

        public unsafe UnsafeSpan(T* inPtr, uint inLength)
        {
            Ptr = inPtr;
            Length = (int) inLength;
        }

        /// <summary>
        /// Reference to data at the given index.
        /// </summary>
        public unsafe ref T this[int inIndex]
        {
            get { return ref Ptr[inIndex]; }
        }

        #region Interfaces

        public unsafe int CompareTo(UnsafeSpan<T> other)
        {
            return (int) (Ptr - other.Ptr);
        }

        public unsafe bool Equals(UnsafeSpan<T> other)
        {
            return Ptr == other.Ptr && Length == other.Length;
        }

        #endregion // Interfaces

        #region Overrides

        public unsafe override int GetHashCode()
        {
            return ((int) Ptr) << 3 ^ (Length.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (obj is UnsafeSpan<T>)
                return Equals((UnsafeSpan<T>) obj);
            return false;
        }

        public unsafe override string ToString()
        {
            return ((ulong) Ptr).ToString("X16");
        }

        #endregion // Overrides

        #region Operators

        static public unsafe implicit operator bool(UnsafeSpan<T> a)
        {
            return a.Ptr != null;
        }

        static public unsafe bool operator ==(UnsafeSpan<T> a, UnsafeSpan<T> b)
        {
            return a.Ptr == b.Ptr && a.Length == b.Length;
        }

        static public unsafe bool operator !=(UnsafeSpan<T> a, UnsafeSpan<T> b)
        {
            return a.Ptr != b.Ptr || a.Length != b.Length;
        }

        #endregion // Operators
    }
}