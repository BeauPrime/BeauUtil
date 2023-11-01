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

using System;
using System.Diagnostics;

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
    public readonly struct UnsafeSpan<T> : IEquatable<UnsafeSpan<T>>, IComparable<UnsafeSpan<T>>
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

        /// <summary>
        /// Returns a reference to the data at the given index.
        /// Throws an exception if the provided index is out of bounds.
        /// </summary>
        public unsafe ref T SafeGet(int inIndex) {
            if (inIndex < 0 || inIndex >= Length) {
                throw new ArgumentOutOfRangeException("inIndex");
            }
            return ref Ptr[inIndex];
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