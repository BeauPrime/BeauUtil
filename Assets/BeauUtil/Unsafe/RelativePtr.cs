/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 Nov 2023
 * 
 * File:    RelativePtr.cs
 * Purpose: Relative pointer wrapper.
 */

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#if UNITY_64 || UNITY_EDITOR_64
using PointerIntegral = System.UInt64;
using PointerDiff = System.Int64;
#else
using PointerIntegral = System.UInt32;
using PointerDiff = System.Int32;
#endif // UNITY_64 || UNITY_EDITOR_64

namespace BeauUtil
{
    #if UNMANAGED_CONSTRAINT

    /// <summary>
    /// Offset pointer wrapper. Can be used to point to an unsafe object
    /// with an address relative to the address of this struct.
    /// </summary>
    /// <remarks>
    /// Be VERY careful with this. This should either only be passed by pointer,
    /// reference, or embedded in the same struct that contains its target pointer.
    /// Without these restrictions, it is very easy to end up pointing to invalid memory.
    /// </remarks>
    [DebuggerDisplay("{Ptr}")]
    public unsafe struct RelativePtr16<T>
        where T : unmanaged
    {
        private short m_Offset;

        /// <summary>
        /// Actual pointer.
        /// </summary>
        public T* Ptr
        {
            get
            {
                fixed (short* b = &m_Offset) {
                    return m_Offset == 0 ? null : (T*) ((byte*) b + m_Offset);
                }
            }
            set
            {
                fixed (short* b = &m_Offset) {
                    checked {
                        m_Offset = value != null ? (short) ((byte*) value - (byte*) b) : (short) 0;
                    }
                }
            }
        }

        #region Overrides

        public unsafe override string ToString()
        {
            return ((PointerIntegral) Ptr).ToString(Unsafe.PointerStringFormat);
        }

        #endregion // Overrides

        #region Operators

        static public unsafe implicit operator T*(RelativePtr16<T> ptr)
        {
            return ptr.Ptr;
        }

        static public unsafe implicit operator bool(RelativePtr16<T> a)
        {
            return a.m_Offset != 0;
        }

        #endregion // Operators
    }

    /// <summary>
    /// Offset pointer wrapper. Can be used to point to an unsafe object
    /// with an address relative to the address of this struct.
    /// </summary>
    /// <remarks>
    /// Be VERY careful with this. This should either only be passed by pointer,
    /// reference, or embedded in the same struct that contains its target pointer.
    /// Without these restrictions, it is very easy to end up pointing to invalid memory.
    /// </remarks>
    [DebuggerDisplay("{Ptr}")]
    public unsafe struct RelativePtr32<T>
        where T : unmanaged
    {
        private int m_Offset;

        /// <summary>
        /// Actual pointer.
        /// </summary>
        public T* Ptr
        {
            get
            {
                fixed (int* b = &m_Offset) {
                    return m_Offset == 0 ? null : (T*) ((byte*) b + m_Offset);
                }
            }
            set
            {
                fixed (int* b = &m_Offset) {
                    checked {
                        m_Offset = value != null ? (int) ((byte*) value - (byte*) b) : 0;
                    }
                }
            }
        }

        #region Overrides

        public unsafe override string ToString()
        {
            return ((PointerIntegral) Ptr).ToString(Unsafe.PointerStringFormat);
        }

        #endregion // Overrides

        #region Operators

        static public unsafe implicit operator T*(RelativePtr32<T> ptr)
        {
            return ptr.Ptr;
        }

        static public unsafe implicit operator bool(RelativePtr32<T> a)
        {
            return a.m_Offset != 0;
        }

        #endregion // Operators
    }

    #endif // UNMANAGED_CONSTRAINT
}