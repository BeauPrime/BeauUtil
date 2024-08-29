/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    18 July 2022
 * 
 * File:    Enums.cs
 * Purpose: Helper methods for dealing with enums.
 */

#if NET_4_6 || CSHARP_7_OR_LATER
#define HAS_ENUM_CONSTRAINT
#endif // NET_4_6 || CSHARP_7_OR_LATER

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BeauUtil
{
    /// <summary>
    /// Fast casting and helper methods for enums.
    /// </summary>
    static public class Enums
    {
        #region To Integral

        /// <summary>
        /// Casts the enum to a byte.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IntrinsicIL("ldarg.0; conv.u1; ret")]
        static public byte ToByte<T>(T inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            #if UNMANAGED_CONSTRAINT
            unsafe
            {
                return Unsafe.Reinterpret<T, byte>(inValue);
            }
            #else
            return Convert.ToByte(inValue);
            #endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the enum to a signed byte.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IntrinsicIL("ldarg.0; conv.i1; ret")]
        static public sbyte ToSByte<T>(T inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            #if UNMANAGED_CONSTRAINT
            unsafe
            {
                return Unsafe.Reinterpret<T, sbyte>(inValue);
            }
            #else
            return Convert.ToSByte(inValue);
            #endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the enum to a short.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IntrinsicIL("ldarg.0; conv.i2; ret")]
        static public short ToShort<T>(T inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            #if UNMANAGED_CONSTRAINT
            unsafe
            {
                return Unsafe.Reinterpret<T, short>(inValue);
            }
            #else
            return Convert.ToInt16(inValue);
            #endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the enum to an unsigned short.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IntrinsicIL("ldarg.0; conv.u2; ret")] 
        static public ushort ToUShort<T>(T inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            #if UNMANAGED_CONSTRAINT
            unsafe
            {
                return Unsafe.Reinterpret<T, ushort>(inValue);
            }
            #else
            return Convert.ToUInt16(inValue);
            #endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the enum to an integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IntrinsicIL("ldarg.0; conv.i4; ret")]
        static public int ToInt<T>(T inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            #if UNMANAGED_CONSTRAINT
            unsafe
            {
                return Unsafe.Reinterpret<T, int>(inValue);
            }
            #else
            return Convert.ToInt32(inValue);
            #endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the enum to an unsigned integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IntrinsicIL("ldarg.0; conv.u4; ret")]
        static public uint ToUInt<T>(T inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            #if UNMANAGED_CONSTRAINT
            unsafe
            {
                return Unsafe.Reinterpret<T, uint>(inValue);
            }
            #else
            return Convert.ToUInt32(inValue);
            #endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the enum to a long.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IntrinsicIL("ldarg.0; conv.i8; ret")] 
        static public long ToLong<T>(T inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            #if UNMANAGED_CONSTRAINT
            unsafe
            {
                return Unsafe.Reinterpret<T, long>(inValue);
            }
            #else
            return Convert.ToInt64(inValue);
            #endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the enum to an unsigned long.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IntrinsicIL("ldarg.0; conv.u8; ret")]
        static public ulong ToULong<T>(T inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            #if UNMANAGED_CONSTRAINT
            unsafe
            {
                return Unsafe.Reinterpret<T, ulong>(inValue);
            }
            #else
            return Convert.ToUInt64(inValue);
            #endif // UNMANAGED_CONSTRAINT
        }

        #endregion // To Integral

        #region To Enum

        /// <summary>
        /// Casts the given byte to the enum type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T ToEnum<T>(byte inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            return Unsafe.Reinterpret<byte, T>(inValue);
#else
            return (T) Enum.ToObject(typeof(T), inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the given signed byte to the enum type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T ToEnum<T>(sbyte inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            return Unsafe.Reinterpret<sbyte, T>(inValue);
#else
            return (T) Enum.ToObject(typeof(T), inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the given short to the enum type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T ToEnum<T>(short inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            return Unsafe.Reinterpret<short, T>(inValue);
#else
            return (T) Enum.ToObject(typeof(T), inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the given unsigned short to the enum type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T ToEnum<T>(ushort inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            return Unsafe.Reinterpret<ushort, T>(inValue);
#else
            return (T) Enum.ToObject(typeof(T), inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the given integer to the enum type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T ToEnum<T>(int inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            return Unsafe.Reinterpret<int, T>(inValue);
#else
            return (T) Enum.ToObject(typeof(T), inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the given unsigned integer to the enum type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T ToEnum<T>(uint inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            return Unsafe.Reinterpret<uint, T>(inValue);
#else
            return (T) Enum.ToObject(typeof(T), inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the given long to the enum type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T ToEnum<T>(long inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            // can guarantee that the enum type is of the same size or shorter, and will be aligned
            return Unsafe.FastReinterpret<long, T>(inValue);
#else
            return (T) Enum.ToObject(typeof(T), inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the given unsigned long to the enum type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T ToEnum<T>(ulong inValue)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            // can guarantee that the enum type is of the same size or shorter, and will be aligned
            return Unsafe.FastReinterpret<ulong, T>(inValue);
#else
            return (T) Enum.ToObject(typeof(T), inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        #endregion // To Enum

        #region Bitwise

        /// <summary>
        /// Bitwise AND of two enums.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if ENABLE_MONO
        [IntrinsicIL("ldarg.0; ldarg.1; and; ret")]
#endif // 
        static public T And<T>(T inA, T inB)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
            return ToEnum<T>(ToULong(inA) & ToULong(inB));
        }

        /// <summary>
        /// Bitwise OR of two enums.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if ENABLE_MONO
        [IntrinsicIL("ldarg.0; ldarg.1; or; ret")]
#endif // ENABLE_MONO
        static public T Or<T>(T inA, T inB)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
            return ToEnum<T>(ToULong(inA) | ToULong(inB));
        }

        /// <summary>
        /// Bitwise XOR of two enums.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if ENABLE_MONO
        [IntrinsicIL("ldarg.0; ldarg.1; xor; ret")]
#endif // ENABLE_MONO
        static public T Xor<T>(T inA, T inB)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
            return ToEnum<T>(ToULong(inA) ^ ToULong(inB));
        }

        /// <summary>
        /// Bitwise NOT of an enum.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if ENABLE_MONO
        [IntrinsicIL("ldarg.0; not; ret")]
#endif // ENABLE_MONO
        static public T Not<T>(T inA)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
            return ToEnum<T>(~ToULong(inA));
        }

        /// <summary>
        /// Returns if the given generic enum is not zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IntrinsicIL("ldarg.0; conv.u8; ldc.i4.0; conv.u8; cgt; ret")]
        static public bool IsNotZero<T>(T inA)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
            return ToULong(inA) != 0;
        }

        /// <summary>
        /// Returns if the given generic enum is zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IntrinsicIL("ldarg.0; conv.u8; ldc.i4.0; conv.u8; ceq; ret")]
        static public bool IsZero<T>(T inA)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
            return ToULong(inA) == 0;
        }

        /// <summary>
        /// Returns if two generic enums are equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if ENABLE_MONO
        [IntrinsicIL("ldarg.0; ldarg.1; ceq; ret")]
#else
        [IntrinsicIL("ldarg.0; conv.u8; ldarg.1; conv.u8; ceq; ret")]
#endif // ENABLE_MONO
        static public bool AreEqual<T>(T inA, T inB)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
            return ToULong(inA) == ToULong(inB);
        }

#endregion // Bitwise
    }
}