/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    5 July 2023
 * 
 * File:    UnsafePtr.cs
 * Purpose: Unsafe pointer wrapper.
 */

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BeauUtil.Debugger;

#if UNITY_64 || UNITY_EDITOR_64 || PLATFORM_ARCH_64
using PointerIntegral = System.UInt64;
using PointerDiff = System.Int64;
#else
using PointerIntegral = System.UInt32;
using PointerDiff = System.Int32;
#endif // UNITY_64 || UNITY_EDITOR_64 || PLATFORM_ARCH_64

namespace BeauUtil
{
    #if UNMANAGED_CONSTRAINT

    /// <summary>
    /// Pointer wrapper.
    /// Useful for passing pointers in code where unsafe pointers
    /// are not allowed (such as in enumerator methods and coroutines)
    /// </summary>
    [DebuggerDisplay("{Ptr}")]
    public readonly struct UnsafePtr<T> : IEquatable<UnsafePtr<T>>, IComparable<UnsafePtr<T>>
        where T : unmanaged
    {
        public readonly unsafe T* Ptr;

        public unsafe UnsafePtr(T* inPtr)
        {
            Assert.True(Unsafe.IsAligned(inPtr), "Unaligned pointer");
            Ptr = inPtr;
        }

        /// <summary>
        /// Reference to the object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T AsRef()
        {
            return ref *Ptr;
        }

        /// <summary>
        /// Dereferences the object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T Deref()
        {
            return *Ptr;
        }

        /// <summary>
        /// Converts this to a pointer of another type.
        /// </summary>
        public unsafe UnsafePtr<U> Cast<U>() where U : unmanaged
        {
            return new UnsafePtr<U>((U*) Ptr);
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
            return ((PointerIntegral) Ptr).ToString(Unsafe.PointerStringFormat);
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

        static public unsafe UnsafePtr<T> operator +(UnsafePtr<T> a, PointerDiff inOffset)
        {
            return new UnsafePtr<T>(a.Ptr + inOffset);
        }

        static public unsafe UnsafePtr<T> operator -(UnsafePtr<T> a, PointerDiff inOffset)
        {
            return new UnsafePtr<T>(a.Ptr - inOffset);
        }

        static public unsafe PointerDiff operator -(UnsafePtr<T> a, UnsafePtr<T> b)
        {
            return (PointerDiff) (a.Ptr - b.Ptr);
        }

        #endregion // Operators
    }

    #endif // UNMANAGED_CONSTRAINT
}