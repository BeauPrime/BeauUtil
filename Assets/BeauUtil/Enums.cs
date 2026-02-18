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

// Conversions from enum to integral in IL2CPP builds were broken by Unity's fix for UUM-91859
// This issue prevents the necessary conversion code from being generated.
// Thankfully the compiler will turn our fixes into roughly equivalent code to our hand-optimized IL.
#if !ENABLE_MONO && (UNITY_2022_3_58 || UNITY_2022_3_59 || UNITY_2022_3_60 || UNITY_2022_3_61 || UNITY_2022_3_62 || UNITY_2022_3_63 || UNITY_2022_3_64 || UNITY_2023_1_OR_NEWER)
#define BROKEN_GENERIC_ENUM_INT_CONVERSION
#endif // !ENABLE_MONO && (UNITY_2022_3_58 || UNITY_2022_3_59 || UNITY_2022_3_60 || UNITY_2022_3_61 || UNITY_2022_3_62 || UNITY_2022_3_63 || UNITY_2022_3_64 || UNITY_2023_1_OR_NEWER)

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
#if !BROKEN_GENERIC_ENUM_INT_CONVERSION
        [IntrinsicIL("ldarg.0; conv.u1; ret")]
#endif // !BROKEN_GENERIC_ENUM_INT_CONVERSION
        static public unsafe byte ToByte<T>(T inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            return *(byte*)&inValue;
#else
            return Convert.ToByte(inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the enum to a signed byte.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if !BROKEN_GENERIC_ENUM_INT_CONVERSION
        [IntrinsicIL("ldarg.0; conv.i1; ret")]
#endif // !BROKEN_GENERIC_ENUM_INT_CONVERSION
        static public unsafe sbyte ToSByte<T>(T inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            return *(sbyte*) &inValue;
#else
            return Convert.ToSByte(inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the enum to a short.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if !BROKEN_GENERIC_ENUM_INT_CONVERSION
        [IntrinsicIL("ldarg.0; conv.i2; ret")]
#endif // !BROKEN_GENERIC_ENUM_INT_CONVERSION
        static public unsafe short ToShort<T>(T inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            switch (sizeof(T))
            {
                case 1:
                    return *(sbyte*) &inValue;
                case 2:
                    return *(short*) &inValue;
                case 4:
                    return (short) *(int*) &inValue;
                default:
                    return (short) *(long*) &inValue;
            }
#else
            return Convert.ToInt16(inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the enum to an unsigned short.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if !BROKEN_GENERIC_ENUM_INT_CONVERSION
        [IntrinsicIL("ldarg.0; conv.u2; ret")] 
#endif // !BROKEN_GENERIC_ENUM_INT_CONVERSION
        static public unsafe ushort ToUShort<T>(T inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            switch (sizeof(T))
            {
                case 1:
                    return *(byte*) &inValue;
                case 2:
                    return *(ushort*) &inValue;
                case 4:
                    return (ushort) *(uint*) &inValue;
                default:
                    return (ushort) *(ulong*) &inValue;
            }
#else
            return Convert.ToUInt16(inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the enum to an integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if !BROKEN_GENERIC_ENUM_INT_CONVERSION
        [IntrinsicIL("ldarg.0; conv.i4; ret")]
#endif // !BROKEN_GENERIC_ENUM_INT_CONVERSION
        static public unsafe int ToInt<T>(T inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            switch (sizeof(T))
            {
                case 1:
                    return *(sbyte*) &inValue;
                case 2:
                    return *(short*) &inValue;
                case 4:
                    return *(int*) &inValue;
                default:
                    return (int) *(long*) &inValue;
            }
#else
            return Convert.ToInt32(inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the enum to an unsigned integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if !BROKEN_GENERIC_ENUM_INT_CONVERSION
        [IntrinsicIL("ldarg.0; conv.u4; ret")]
#endif // !BROKEN_GENERIC_ENUM_INT_CONVERSION
        static public unsafe uint ToUInt<T>(T inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            switch (sizeof(T))
            {
                case 1:
                    return *(byte*) &inValue;
                case 2:
                    return *(ushort*) &inValue;
                case 4:
                    return *(uint*) &inValue;
                default:
                    return (uint) *(ulong*) &inValue;
            }
#else
            return Convert.ToUInt32(inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the enum to a long.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if !BROKEN_GENERIC_ENUM_INT_CONVERSION
        [IntrinsicIL("ldarg.0; conv.i8; ret")]
#endif // !BROKEN_GENERIC_ENUM_INT_CONVERSION
        static public unsafe long ToLong<T>(T inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            switch (sizeof(T))
            {
                case 1:
                    return *(sbyte*) &inValue;
                case 2:
                    return *(short*) &inValue;
                case 4:
                    return *(int*) &inValue;
                default:
                    return *(long*) &inValue;
            }
#else
            return Convert.ToInt64(inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the enum to an unsigned long.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if !BROKEN_GENERIC_ENUM_INT_CONVERSION
        [IntrinsicIL("ldarg.0; conv.u8; ret")]
#endif // !BROKEN_GENERIC_ENUM_INT_CONVERSION
        static public unsafe ulong ToULong<T>(T inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            switch (sizeof(T))
            {
                case 1:
                    return *(byte*) &inValue;
                case 2:
                    return *(ushort*) &inValue;
                case 4:
                    return *(uint*) &inValue;
                default:
                    return *(ulong*) &inValue;
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
        static public unsafe T ToEnum<T>(byte inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            switch (sizeof(T))
            {
                case 1:
                    byte val1 = (byte) inValue;
                    return *(T*) &val1;
                case 2:
                    ushort val2 = (ushort) inValue;
                    return *(T*) &val2;
                case 4:
                    uint val4 = (uint) inValue;
                    return *(T*) &val4;
                default:
                    ulong val8 = (ulong) inValue;
                    return *(T*) &val8;
            }
#else
            return (T) Enum.ToObject(typeof(T), inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the given signed byte to the enum type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public unsafe T ToEnum<T>(sbyte inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            switch (sizeof(T))
            {
                case 1:
                    sbyte val1 = (sbyte) inValue;
                    return *(T*) &val1;
                case 2:
                    short val2 = (short) inValue;
                    return *(T*) &val2;
                case 4:
                    int val4 = (int) inValue;
                    return *(T*) &val4;
                default:
                    long val8 = (long) inValue;
                    return *(T*) &val8;
            }
#else
            return (T) Enum.ToObject(typeof(T), inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the given short to the enum type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public unsafe T ToEnum<T>(short inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            switch (sizeof(T))
            {
                case 1:
                    sbyte val1 = (sbyte) inValue;
                    return *(T*) &val1;
                case 2:
                    short val2 = (short) inValue;
                    return *(T*) &val2;
                case 4:
                    int val4 = (int) inValue;
                    return *(T*) &val4;
                default:
                    long val8 = (long) inValue;
                    return *(T*) &val8;
            }
#else
            return (T) Enum.ToObject(typeof(T), inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the given unsigned short to the enum type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public unsafe T ToEnum<T>(ushort inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            switch (sizeof(T))
            {
                case 1:
                    byte val1 = (byte) inValue;
                    return *(T*) &val1;
                case 2:
                    ushort val2 = (ushort) inValue;
                    return *(T*) &val2;
                case 4:
                    uint val4 = (uint) inValue;
                    return *(T*) &val4;
                default:
                    ulong val8 = (ulong) inValue;
                    return *(T*) &val8;
            }
#else
            return (T) Enum.ToObject(typeof(T), inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the given integer to the enum type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public unsafe T ToEnum<T>(int inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            switch (sizeof(T))
            {
                case 1:
                    sbyte val1 = (sbyte) inValue;
                    return *(T*) &val1;
                case 2:
                    short val2 = (short) inValue;
                    return *(T*) &val2;
                case 4:
                    int val4 = (int) inValue;
                    return *(T*) &val4;
                default:
                    long val8 = (long) inValue;
                    return *(T*) &val8;
            }
#else
            return (T) Enum.ToObject(typeof(T), inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the given unsigned integer to the enum type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public unsafe T ToEnum<T>(uint inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            switch (sizeof(T))
            {
                case 1:
                    byte val1 = (byte) inValue;
                    return *(T*) &val1;
                case 2:
                    ushort val2 = (ushort) inValue;
                    return *(T*) &val2;
                case 4:
                    uint val4 = (uint) inValue;
                    return *(T*) &val4;
                default:
                    ulong val8 = (ulong) inValue;
                    return *(T*) &val8;
            }
#else
            return (T) Enum.ToObject(typeof(T), inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the given long to the enum type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public unsafe T ToEnum<T>(long inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            switch (sizeof(T))
            {
                case 1:
                    sbyte val1 = (sbyte) inValue;
                    return *(T*) &val1;
                case 2:
                    short val2 = (short) inValue;
                    return *(T*) &val2;
                case 4:
                    int val4 = (int) inValue;
                    return *(T*) &val4;
                default:
                    long val8 = (long) inValue;
                    return *(T*) &val8;
            }
#else
            return (T) Enum.ToObject(typeof(T), inValue);
#endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Casts the given unsigned long to the enum type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public unsafe T ToEnum<T>(ulong inValue)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
#if UNMANAGED_CONSTRAINT
            switch (sizeof(T))
            {
                case 1:
                    byte val1 = (byte) inValue;
                    return *(T*) &val1;
                case 2:
                    ushort val2 = (ushort) inValue;
                    return *(T*) &val2;
                case 4:
                    uint val4 = (uint) inValue;
                    return *(T*) &val4;
                default:
                    ulong val8 = (ulong) inValue;
                    return *(T*) &val8;
            }
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
#if !BROKEN_GENERIC_ENUM_INT_CONVERSION

        [IntrinsicIL("ldarg.0; conv.u8; ldc.i4.0; conv.u8; cgt; ret")]
#endif // !BROKEN_GENERIC_ENUM_INT_CONVERSION
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
#if !BROKEN_GENERIC_ENUM_INT_CONVERSION
        [IntrinsicIL("ldarg.0; conv.u8; ldc.i4.0; conv.u8; ceq; ret")]
#endif // !BROKEN_GENERIC_ENUM_INT_CONVERSION
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
#elif !BROKEN_GENERIC_ENUM_INT_CONVERSION
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