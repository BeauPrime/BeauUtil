/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    5 July 2023
 * 
 * File:    UnsafeSpan.cs
 * Purpose: Unsafe range wrapper.
 */

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEVELOPMENT
#define HAS_DEBUGGER
#endif // UNITY_EDITOR

#if NETSTANDARD || NET_STANDARD
#define SUPPORTS_SPAN
#endif // NETSTANDARD || NET_STANDARD

#if UNITY_2018_1_OR_NEWER
#define NATIVE_ARRAY_EXT
#endif // UNITY_2018_1_OR_NEWER

#if UNITY_64 || UNITY_EDITOR_64 || PLATFORM_ARCH_64 || ENABLE_WASM64
#define WORD_SIZE_64
#elif !UNITY_EDITOR && UNITY_WEBGL
#define WORD_SIZE_32
#else
#define WORD_SIZE_RUNTIME
#endif // UNITY_64 || UNITY_EDITOR_64 || PLATFORM_ARCH_64 || ENABLE_WASM64

#if WORD_SIZE_64 || WORD_SIZE_RUNTIME
using PointerIntegral = System.UInt64;
using PointerDiff = System.Int64;
#else
using PointerIntegral = System.UInt32;
using PointerDiff = System.Int32;
#endif // WORD_SIZE_64 || WORD_SIZE_RUNTIME

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using BeauUtil.Debugger;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    #if UNMANAGED_CONSTRAINT

    /// <summary>
    /// Memory span wrapper.
    /// Useful for passing pointers in code where unsafe pointers
    /// are not allowed (such as in enumerator methods and coroutines)
    /// </summary>
#if HAS_DEBUGGER
    [DebuggerDisplay("Length = {Length}")]
    [DebuggerTypeProxy(typeof(UnsafeSpan<>.DebugView))]
#endif // HAS_DEBUGGER
    public readonly struct UnsafeSpan<T> : IEquatable<UnsafeSpan<T>>, IComparable<UnsafeSpan<T>>, IEnumerable<T>
        where T : unmanaged
    {
        public readonly unsafe T* Ptr;
        public readonly int Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe UnsafeSpan(T* inPtr, uint inLength)
        {
            Assert.True(inPtr != null || inLength == 0, "Null pointer");
            Assert.True(Unsafe.IsAligned(inPtr), "Unaligned pointer");
            Ptr = inPtr;
            Length = (int) inLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe UnsafeSpan(T* inPtr, int inLength)
        {
            Assert.True(inPtr != null || inLength == 0, "Null pointer");
            Assert.True(Unsafe.IsAligned(inPtr), "Unaligned pointer");
            Ptr = inPtr;
            Length = inLength;
        }

        /// <summary>
        /// Reference to data at the given index.
        /// </summary>
        public unsafe ref T this[int inIndex]
        {
            get { return ref Ptr[inIndex]; }
        }

        /// <summary>
        /// Returns a reference to the data at the given index.
        /// Throws an exception if the provided index is out of bounds.
        /// </summary>
        public unsafe ref T SafeGet(int inIndex)
        {
            if (inIndex >= 0 && inIndex < Length) {
                return ref Ptr[inIndex];
            }
            throw new ArgumentOutOfRangeException("inIndex");
        }

        /// <summary>
        /// Returns the total number of bytes in this span.
        /// Computed as Length * sizeof(T).
        /// </summary>
        public unsafe int ByteLength
        {
            get { return Length * sizeof(T); }
        }

        /// <summary>
        /// Returns if this is either a null pointer or has a length of 0.
        /// </summary>
        public unsafe bool IsNullOrEmpty
        {
            get { return Ptr == null || Length <= 0; }
        }

        #region Conversions

        /// <summary>
        /// Converts this span to a span of another type.
        /// </summary>
        public unsafe UnsafeSpan<U> Cast<U>() where U : unmanaged
        {
            return new UnsafeSpan<U>((U*) Ptr, Length * sizeof(T) / sizeof(U));
        }

        /// <summary>
        /// Returns a slice of this span.
        /// </summary>
        public unsafe UnsafeSpan<T> Slice(PointerDiff inOffset)
        {
            if (inOffset < 0 || inOffset >= Length)
                throw new ArgumentOutOfRangeException("inOffset");
            return new UnsafeSpan<T>(Ptr + inOffset, (int) (Length - inOffset));
        }

        /// <summary>
        /// Returns a slice of this span.
        /// </summary>
        public unsafe UnsafeSpan<T> Slice(PointerDiff inOffset, int inLength)
        {
            if (inOffset < 0 || inOffset >= Length)
                throw new ArgumentOutOfRangeException("inOffset");
            if (inLength < 0 || inOffset + inLength > Length)
                throw new ArgumentOutOfRangeException("inLength");
            return new UnsafeSpan<T>(Ptr + inOffset, inLength);
        }

        #endregion // Conversions

        #region Interfaces

        public unsafe int CompareTo(UnsafeSpan<T> other)
        {
            return (int) (Ptr - other.Ptr);
        }

        public unsafe bool Equals(UnsafeSpan<T> other)
        {
            return Ptr == other.Ptr && Length == other.Length;
        }

        public unsafe UnsafeEnumerator<T> GetEnumerator()
        {
            return new UnsafeEnumerator<T>(Ptr, (uint) Length);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
            return ((PointerIntegral) Ptr).ToString(Unsafe.PointerStringFormat);
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

#if SUPPORTS_SPAN

        static public unsafe implicit operator Span<T>(UnsafeSpan<T> a)
        {
            return new Span<T>(a.Ptr, a.Length);
        }

        static public unsafe implicit operator ReadOnlySpan<T>(UnsafeSpan<T> a)
        {
            return new ReadOnlySpan<T>(a.Ptr, a.Length);
        }

#endif // SUPPORTS_SPAN

        #endregion // Operators

        #region Copy

        /// <summary>
        /// Copies this span into a new array.
        /// </summary>
        public T[] ToArray()
        {
            unsafe
            {
                if (Length == 0)
                {
                    return Array.Empty<T>();
                }

                T[] newArr = new T[Length];
                fixed(T* arrPtr = newArr)
                {
                    Unsafe.FastCopyArray(Ptr, Length, arrPtr);
                }

                return newArr;
            }
        }

        #endregion // Copy

#if HAS_DEBUGGER

        private class DebugView
        {
            private readonly UnsafeSpan<T> m_Data;

            public DebugView(UnsafeSpan<T> inData)
            {
                m_Data = inData;
            }

            public unsafe T[] Items
            {
                get
                {
                    if (m_Data.Ptr == null)
                        return null;

                    T[] values = new T[m_Data.Length];
                    for (int i = 0; i < values.Length; i++)
                        values[i] = m_Data.Ptr[i];

                    return values;
                }
            }
        }

#endif // HAS_DEBUGGER

#endif // UNMANAGED_CONSTRAINT
    }
}