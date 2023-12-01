/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Bits.cs
 * Purpose: Helper methods for dealing with bit masks on integers and unsigned integers.
 */

#if NET_4_6 || CSHARP_7_OR_LATER
#define HAS_ENUM_CONSTRAINT
#endif // NET_4_6 || CSHARP_7_OR_LATER

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections;
using System.Runtime.CompilerServices;

#if USING_MATHEMATICS
using Unity.Mathematics;
#endif // USING_MATHEMATICS

namespace BeauUtil
{
    /// <summary>
    /// Performs bitwise operations on 32- and 64-bit integrals.
    /// </summary>
    static public class Bits
    {
        /// <summary>
        /// Total number of bits in an integer.
        /// </summary>
        public const int Length = 32;

        /// <summary>
        /// Total number of bits in a long.
        /// </summary>
        public const int Length64 = 64;

        public const int All32 = unchecked((int) AllU32);
        public const uint AllU32 = 0xFFFFFFFF;

        public const long All64 = unchecked((int) AllU64);
        public const ulong AllU64 = 0xFFFFFFFFFFFFFFFF;

        #region Contains

        /// <summary>
        /// Returns if the given uint has the given bit toggled on.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Contains(uint inBitArray, int inBitIndex)
        {
            return (inBitArray & (1U << inBitIndex)) != 0;
        }

        /// <summary>
        /// Returns if the given int has the given bit toggled on.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Contains(int inBitArray, int inBitIndex)
        {
            return (inBitArray & (1 << inBitIndex)) != 0;
        }

        /// <summary>
        /// Returns if the given ulong has the given bit toggled on.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Contains(ulong inBitArray, int inBitIndex)
        {
            return (inBitArray & ((ulong) 1 << inBitIndex)) != 0;
        }

        /// <summary>
        /// Returns if the given long has the given bit toggled on.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Contains(long inBitArray, int inBitIndex)
        {
            return (inBitArray & ((long) 1 << inBitIndex)) != 0;
        }

        /// <summary>
        /// Returns if the given enum contains any of the given mask.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Contains<T>(T inBitArray, T inBitMask)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            return ContainsAny<T>(inBitArray, inBitMask);
        }

        /// <summary>
        /// Returns if the given uint contains any of the given mask.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool ContainsAny(uint inBitArray, uint inBitMask)
        {
            return (inBitArray & inBitMask) != 0;
        }

        /// <summary>
        /// Returns if the given int contains any of the given mask.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool ContainsAny(int inBitArray, int inBitMask)
        {
            return (inBitArray & inBitMask) != 0;
        }

        /// <summary>
        /// Returns if the given ulong contains any of the given mask.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool ContainsAny(ulong inBitArray, ulong inBitMask)
        {
            return (inBitArray & inBitMask) != 0;
        }

        /// <summary>
        /// Returns if the given long contains any of the given mask.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool ContainsAny(long inBitArray, long inBitMask)
        {
            return (inBitArray & inBitMask) != 0;
        }

        /// <summary>
        /// Returns if the given enum contains any of the given mask.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool ContainsAny<T>(T inBitArray, T inBitMask)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            return (Enums.ToULong(inBitArray) & Enums.ToULong(inBitMask)) != 0;
        }

        /// <summary>
        /// Returns if the given uint contains all of the given mask.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool ContainsAll(uint inBitArray, uint inBitMask)
        {
            return (inBitArray & inBitMask) == inBitMask;
        }

        /// <summary>
        /// Returns if the given int contains all of the given mask.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool ContainsAll(int inBitArray, int inBitMask)
        {
            return (inBitArray & inBitMask) == inBitMask;
        }

        /// <summary>
        /// Returns if the given ulong contains all of the given mask.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool ContainsAll(ulong inBitArray, ulong inBitMask)
        {
            return (inBitArray & inBitMask) == inBitMask;
        }

        /// <summary>
        /// Returns if the given long contains all of the given mask.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool ContainsAll(long inBitArray, long inBitMask)
        {
            return (inBitArray & inBitMask) == inBitMask;
        }

        /// <summary>
        /// Returns if the given enum contains all of the given mask.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool ContainsAll<T>(T inBitArray, T inBitMask)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            ulong mask = Enums.ToULong(inBitMask);
            return (Enums.ToULong(inBitArray) & mask) == mask;
        }

        #endregion // Contains

        #region Add/Remove

        /// <summary>
        /// Toggles the given bit in the given uint to on.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Add(ref uint ioBitArray, int inBitIndex)
        {
            ioBitArray |= (1U << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given uint to on.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint Add(uint inBitArray, int inBitIndex)
        {
            return inBitArray | (1U << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given int to on.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Add(ref int ioBitArray, int inBitIndex)
        {
            ioBitArray |= (1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given int to on.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int Add(int inBitArray, int inBitIndex)
        {
            return inBitArray | (1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given ulong to on.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Add(ref ulong ioBitArray, int inBitIndex)
        {
            ioBitArray |= ((ulong) 1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given ulong to on.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong Add(ulong inBitArray, int inBitIndex)
        {
            return inBitArray | ((ulong) 1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given long to on.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Add(ref long ioBitArray, int inBitIndex)
        {
            ioBitArray |= ((long) 1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given long to on.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public long Add(long inBitArray, int inBitIndex)
        {
            return inBitArray | ((long) 1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given mask in the given enum to on.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Add<T>(ref T ioBitArray, T inMask)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            ioBitArray = Enums.ToEnum<T>(Enums.ToULong(ioBitArray) | Enums.ToULong(inMask));
        }

        /// <summary>
        /// Toggles the given mask in the given enum to on.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T Add<T>(T inBitArray, T inMask)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
            return Enums.ToEnum<T>(Enums.ToULong(inBitArray) | Enums.ToULong(inMask));
        }

        /// <summary>
        /// Toggles the given bit in the given uint to off.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Remove(ref uint ioBitArray, int inBitIndex)
        {
            ioBitArray &= ~(1U << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given uint to off.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint Remove(uint inBitArray, int inBitIndex)
        {
            return inBitArray & ~(1U << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given int to off.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Remove(ref int ioBitArray, int inBitIndex)
        {
            ioBitArray &= ~(1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given int to off.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int Remove(int inBitArray, int inBitIndex)
        {
            return inBitArray & ~(1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given ulong to off.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Remove(ref ulong ioBitArray, int inBitIndex)
        {
            ioBitArray &= ~((ulong) 1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given ulong to off.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong Remove(ulong inBitArray, int inBitIndex)
        {
            return inBitArray & ~((ulong) 1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given long to off.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Remove(ref long ioBitArray, int inBitIndex)
        {
            ioBitArray &= ~((long) 1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given long to off.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public long Remove(long inBitArray, int inBitIndex)
        {
            return inBitArray & ~((long) 1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given mask in the given enum to off.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Remove<T>(ref T ioBitArray, T inMask)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            ioBitArray = Enums.ToEnum<T>(Enums.ToULong(ioBitArray) & ~Enums.ToULong(inMask));
        }

        /// <summary>
        /// Toggles the given mask in the given enum to off.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T Remove<T>(T inBitArray, T inMask)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
            return Enums.ToEnum<T>(Enums.ToULong(inBitArray) & ~Enums.ToULong(inMask));
        }

        /// <summary>
        /// Toggles the given bit in the given uint to the given state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Set(ref uint ioBitArray, int inBitIndex, bool inbState)
        {
            if (inbState)
                Add(ref ioBitArray, inBitIndex);
            else
                Remove(ref ioBitArray, inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given uint to the given state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint Set(uint inBitArray, int inBitIndex, bool inbState)
        {
            if (inbState)
                return Add(inBitArray, inBitIndex);
            else
                return Remove(inBitArray, inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given int to the given state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Set(ref int ioBitArray, int inBitIndex, bool inbState)
        {
            if (inbState)
                Add(ref ioBitArray, inBitIndex);
            else
                Remove(ref ioBitArray, inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given int to the given state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int Set(int inBitArray, int inBitIndex, bool inbState)
        {
            if (inbState)
                return Add(inBitArray, inBitIndex);
            else
                return Remove(inBitArray, inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given ulong to the given state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Set(ref ulong ioBitArray, int inBitIndex, bool inbState)
        {
            if (inbState)
                Add(ref ioBitArray, inBitIndex);
            else
                Remove(ref ioBitArray, inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given ulong to the given state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong Set(ulong inBitArray, int inBitIndex, bool inbState)
        {
            if (inbState)
                return Add(inBitArray, inBitIndex);
            else
                return Remove(inBitArray, inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given long to the given state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Set(ref long ioBitArray, int inBitIndex, bool inbState)
        {
            if (inbState)
                Add(ref ioBitArray, inBitIndex);
            else
                Remove(ref ioBitArray, inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given long to the given state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public long Set(long inBitArray, int inBitIndex, bool inbState)
        {
            if (inbState)
                return Add(inBitArray, inBitIndex);
            else
                return Remove(inBitArray, inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given int to the given state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Set<T>(ref T ioBitArray, T inMask, bool inbState)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            if (inbState)
                Add(ref ioBitArray, inMask);
            else
                Remove(ref ioBitArray, inMask);
        }

        /// <summary>
        /// Toggles the given bit in the given int to the given state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T Set<T>(T inBitArray, T inMask, bool inbState)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
            if (inbState)
                return Add(inBitArray, inMask);
            else
                return Remove(inBitArray, inMask);
        }

        #endregion // Add/Remove

        #region IndexOf

        /// <summary>
        /// Returns the bit index of the given bit array, if it contains a single set bit.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int IndexOf(int inBitArray)
        {
            unsafe
            {
                return IndexOf(*(uint*)(&inBitArray));
            }
        }

        /// <summary>
        /// Returns the bit index of the given bit array, if it contains a single set bit.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int IndexOf(uint inBitArray)
        {
            if (inBitArray == 0)
                return -1;
            if ((inBitArray & (inBitArray - 1)) != 0)
                return -1;

#if USING_MATHEMATICS
            return math.tzcnt(inBitArray);
#else
            int shiftCount = 0;
            while (inBitArray != 1)
            {
                inBitArray >>= 1;
                ++shiftCount;
            }
            return shiftCount;
#endif // USING_MATHEMATICS
        }

        /// <summary>
        /// Returns the bit index of the given bit array, if it contains a single set bit.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int IndexOf(long inBitArray)
        {
            unsafe
            {
                return IndexOf(*(ulong*) (&inBitArray));
            }
        }

        /// <summary>
        /// Returns the bit index of the given bit array, if it contains a single set bit.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int IndexOf(ulong inBitArray)
        {
            if (inBitArray == 0)
                return -1;
            if ((inBitArray & (inBitArray - 1)) != 0)
                return -1;

#if USING_MATHEMATICS
            return math.tzcnt(inBitArray);
#else
            int shiftCount = 0;
            while (inBitArray != 1)
            {
                inBitArray >>= 1;
                ++shiftCount;
            }
            return shiftCount;
#endif // USING_MATHEMATICS
        }

        /// <summary>
        /// Returns the bit index of the given bit array, if it contains a single set bit.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int IndexOf<T>(T inBitArray)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
            return IndexOf(Enums.ToULong(inBitArray));
        }

        #endregion // Indexof

        #region Count

        /// <summary>
        /// Returns the number of set bits in the given bit array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int Count(int inBitArray)
        {
            unsafe
            {
                return Count(*(uint*)(&inBitArray));
            }
        }

        /// <summary>
        /// Returns the number of set bits in the given bit array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int Count(uint inBitArray)
        {
#if USING_MATHEMATICS
            return math.countbits(inBitArray);
#else
            int count = 0;
            while (inBitArray != 0)
            {
                inBitArray &= (inBitArray - 1);
                count++;
            }
            return count;
#endif // USING_MATHEMATICS
        }

        /// <summary>
        /// Returns the number of set bits in the given bit array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int Count(long inBitArray)
        {
            unsafe
            {
                return Count(*(ulong*)(&inBitArray));
            }
        }

        /// <summary>
        /// Returns the number of set bits in the given bit array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int Count(ulong inBitArray)
        {
#if USING_MATHEMATICS
            return math.countbits(inBitArray);
#else
            int count = 0;
            while (inBitArray != 0)
            {
                inBitArray &= (inBitArray - 1);
                count++;
            }
            return count;
#endif // USING_MATHEMATICS
        }

        /// <summary>
        /// Returns the number of set bits in the given enum.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int Count<T>(T inBitArray)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
            return Count(Enums.ToULong(inBitArray));
        }

        #endregion // Count

        #region Rotate

        /// <summary>
        /// Rotates the given bits to the left.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int RotateLeft(int inBitArray, int inAmount)
        {
            unsafe
            {
                uint rot = RotateLeft(*(uint*) (&inBitArray), inAmount);
                return *(int*) &rot;
            }
        }

        /// <summary>
        /// Rotates the given bits to the left.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void RotateLeft(ref int ioBitArray, int inAmount)
        {
            ioBitArray = RotateLeft(ioBitArray, inAmount);
        }

        /// <summary>
        /// Rotates the given bits to the left.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint RotateLeft(uint inBitArray, int inAmount)
        {
            return (inBitArray << inAmount) | (inBitArray >> (32 - inAmount));
        }

        /// <summary>
        /// Rotates the given bits to the left.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void RotateLeft(ref uint ioBitArray, int inAmount)
        {
            ioBitArray = (ioBitArray << inAmount) | (ioBitArray >> (32 - inAmount));
        }

        /// <summary>
        /// Rotates the given bits to the left.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public long RotateLeft(long inBitArray, int inAmount)
        {
            unsafe
            {
                ulong rot = RotateLeft(*(ulong*) (&inBitArray), inAmount);
                return *(long*) &rot;
            }
        }

        /// <summary>
        /// Rotates the given bits to the left.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void RotateLeft(ref long ioBitArray, int inAmount)
        {
            ioBitArray = RotateLeft(ioBitArray, inAmount);
        }

        /// <summary>
        /// Rotates the given bits to the left.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong RotateLeft(ulong inBitARray, int inAmount)
        {
            return (inBitARray << inAmount) | (inBitARray >> (64 - inAmount));
        }

        /// <summary>
        /// Rotates the given bits to the left.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void RotateLeft(ref ulong ioBitArray, int inAmount)
        {
            ioBitArray = (ioBitArray << inAmount) | (ioBitArray >> (64 - inAmount));
        }

        /// <summary>
        /// Rotates the given bits to the right.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int RotateRight(int inBitArray, int inAmount)
        {
            unsafe
            {
                uint rot = RotateRight(*(uint*) (&inBitArray), inAmount);
                return *(int*) &rot;
            }
        }

        /// <summary>
        /// Rotates the given bits to the right.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void RotateRight(ref int ioBitArray, int inAmount)
        {
            ioBitArray = RotateRight(ioBitArray, inAmount);
        }

        /// <summary>
        /// Rotates the given bits to the right.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint RotateRight(uint inBitArray, int inAmount)
        {
            return (inBitArray >> inAmount) | (inBitArray << (32 - inAmount));
        }

        /// <summary>
        /// Rotates the given bits to the right.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void RotateRight(ref uint ioBitArray, int inAmount)
        {
            ioBitArray = (ioBitArray >> inAmount) | (ioBitArray << (32 - inAmount));
        }

        /// <summary>
        /// Rotates the given bits to the right.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public long RotateRight(long inBitArray, int inAmount)
        {
            unsafe
            {
                ulong rot = RotateRight(*(ulong*) (&inBitArray), inAmount);
                return *(long*) &rot;
            }
        }

        /// <summary>
        /// Rotates the given bits to the right.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void RotateRight(ref long ioBitArray, int inAmount)
        {
            ioBitArray = RotateRight(ioBitArray, inAmount);
        }

        /// <summary>
        /// Rotates the given bits to the right.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong RotateRight(ulong inBitARray, int inAmount)
        {
            return (inBitARray >> inAmount) | (inBitARray << (64 - inAmount));
        }

        /// <summary>
        /// Rotates the given bits to the right.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void RotateRight(ref ulong ioBitArray, int inAmount)
        {
            ioBitArray = (ioBitArray >> inAmount) | (ioBitArray << (64 - inAmount));
        }

        #endregion // Rotate
    }
}