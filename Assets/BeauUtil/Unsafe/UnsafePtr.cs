/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    5 July 2023
 * 
 * File:    UnsafePtr.cs
 * Purpose: Unsafe pointer wrapper.
 */

using System;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Pointer wrapper.
    /// Useful for passing pointers in code where unsafe pointers
    /// are not allowed (such as in enumerator methods and coroutines)
    /// </summary>
    public struct UnsafePtr<T> : IEquatable<UnsafePtr<T>>, IComparable<UnsafePtr<T>>
        where T : unmanaged
    {
        public readonly unsafe T* Ptr;

        public unsafe UnsafePtr(T* inPtr)
        {
            Ptr = inPtr;
        }

        /// <summary>
        /// Reference to the object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T Ref()
        {
            return ref *Ptr;
        }

        #region Interfaces

        public unsafe int CompareTo(UnsafePtr<T> other)
        {
            return (int) (Ptr - other.Ptr);
        }

        public unsafe bool Equals(UnsafePtr<T> other)
        {
            return Ptr == other.Ptr;
        }

        #endregion // Interfaces

        #region Overrides

        public unsafe override int GetHashCode()
        {
            return (int) Ptr;
        }

        public override bool Equals(object obj)
        {
            if (obj is UnsafePtr<T>)
                return Equals((UnsafePtr<T>) obj);
            return false;
        }

        public unsafe override string ToString()
        {
            return ((ulong) Ptr).ToString("X16");
        }

        #endregion // Overrides

        #region Operators

        static public unsafe implicit operator UnsafePtr<T>(T* ptr)
        {
            return new UnsafePtr<T>(ptr);
        }

        static public unsafe implicit operator T*(UnsafePtr<T> ptr)
        {
            return ptr.Ptr;
        }

        static public unsafe implicit operator bool(UnsafePtr<T> a)
        {
            return a.Ptr != null;
        }

        static public unsafe bool operator ==(UnsafePtr<T> a, UnsafePtr<T> b)
        {
            return a.Ptr == b.Ptr;
        }

        static public unsafe bool operator !=(UnsafePtr<T> a, UnsafePtr<T> b)
        {
            return a.Ptr != b.Ptr;
        }

        static public unsafe bool operator ==(UnsafePtr<T> a, T* b)
        {
            return a.Ptr == b;
        }

        static public unsafe bool operator !=(UnsafePtr<T> a, T* b)
        {
            return a.Ptr != b;
        }

        static public unsafe UnsafePtr<T> operator +(UnsafePtr<T> a, int inOffset)
        {
            return new UnsafePtr<T>(a.Ptr + inOffset);
        }

        static public unsafe UnsafePtr<T> operator -(UnsafePtr<T> a, int inOffset)
        {
            return new UnsafePtr<T>(a.Ptr - inOffset);
        }

        static public unsafe long operator -(UnsafePtr<T> a, UnsafePtr<T> b)
        {
            return a.Ptr - b.Ptr;
        }

        #endregion // Operators
    }
}