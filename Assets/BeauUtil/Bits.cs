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
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Performs bitwise operations on integers and unsigned integers.
    /// </summary>
    static public class Bits
    {
        /// <summary>
        /// Total number of bits in an integer.
        /// </summary>
        public const int Length = 32;

        public const int All32 = unchecked((int) AllU32);
        public const uint AllU32 = 0xFFFFFFFF;

        /// <summary>
        /// Returns if the given uint has the given bit toggled on.
        /// </summary>
        [MethodImpl(256)]
        static public bool Contains(uint inBitArray, int inBitIndex)
        {
            return (inBitArray & (1U << inBitIndex)) != 0;
        }

        /// <summary>
        /// Returns if the given int has the given bit toggled on.
        /// </summary>
        [MethodImpl(256)]
        static public bool Contains(int inBitArray, int inBitIndex)
        {
            return (inBitArray & (1 << inBitIndex)) != 0;
        }

        /// <summary>
        /// Returns if the given uint contains any of the given mask.
        /// </summary>
        [MethodImpl(256)]
        static public bool ContainsAny(uint inBitArray, uint inBitMask)
        {
            return (inBitArray & inBitMask) != 0;
        }

        /// <summary>
        /// Returns if the given int contains any of the given mask.
        /// </summary>
        [MethodImpl(256)]
        static public bool ContainsAny(int inBitArray, int inBitMask)
        {
            return (inBitArray & inBitMask) != 0;
        }

        /// <summary>
        /// Returns if the given enum contains any of the given mask.
        /// </summary>
        [MethodImpl(256)]
        static public bool ContainsAny<T>(T inBitArray, T inBitMask)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            return (Enums.ToUInt(inBitArray) & Enums.ToUInt(inBitMask)) != 0;
        }

        /// <summary>
        /// Returns if the given uint contains all of the given mask.
        /// </summary>
        [MethodImpl(256)]
        static public bool ContainsAll(uint inBitArray, uint inBitMask)
        {
            return (inBitArray & inBitMask) == inBitMask;
        }

        /// <summary>
        /// Returns if the given int contains all of the given mask.
        /// </summary>
        [MethodImpl(256)]
        static public bool ContainsAll(int inBitArray, int inBitMask)
        {
            return (inBitArray & inBitMask) == inBitMask;
        }

        /// <summary>
        /// Returns if the given enum contains any of the given mask.
        /// </summary>
        [MethodImpl(256)]
        static public bool ContainsAll<T>(T inBitArray, T inBitMask)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            uint mask = Enums.ToUInt(inBitMask);
            return (Enums.ToUInt(inBitArray) & mask) == mask;
        }

        /// <summary>
        /// Toggles the given bit in the given uint to on.
        /// </summary>
        [MethodImpl(256)]
        static public void Add(ref uint ioBitArray, int inBitIndex)
        {
            ioBitArray |= (1U << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given int to on.
        /// </summary>
        [MethodImpl(256)]
        static public void Add(ref int ioBitArray, int inBitIndex)
        {
            ioBitArray |= (1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given mask in the given enum to on.
        /// </summary>
        [MethodImpl(256)]
        static public void Add<T>(ref T ioBitArray, T inMask)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            ioBitArray = Enums.ToEnum<T>(Enums.ToUInt(ioBitArray) | Enums.ToUInt(inMask));
        }

        /// <summary>
        /// Toggles the given bit in the given uint to off.
        /// </summary>
        [MethodImpl(256)]
        static public void Remove(ref uint ioBitArray, int inBitIndex)
        {
            ioBitArray &= ~(1U << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given int to off.
        /// </summary>
        [MethodImpl(256)]
        static public void Remove(ref int ioBitArray, int inBitIndex)
        {
            ioBitArray &= ~(1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given mask in the given enum to off.
        /// </summary>
        [MethodImpl(256)]
        static public void Remove<T>(ref T ioBitArray, T inMask)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            ioBitArray = Enums.ToEnum<T>(Enums.ToUInt(ioBitArray) & ~Enums.ToUInt(inMask));
        }

        /// <summary>
        /// Toggles the given bit in the given uint to the given state.
        /// </summary>
        [MethodImpl(256)]
        static public void Set(ref uint ioBitArray, int inBitIndex, bool inbState)
        {
            if (inbState)
                Add(ref ioBitArray, inBitIndex);
            else
                Remove(ref ioBitArray, inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given int to the given state.
        /// </summary>
        [MethodImpl(256)]
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
        [MethodImpl(256)]
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
        /// Returns the bit index of the given bit array, if it contains a single set bit.
        /// </summary>
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
        static public int IndexOf(uint inBitArray)
        {
            if (inBitArray == 0)
                return -1;
            if ((inBitArray & (inBitArray - 1)) != 0)
                return -1;

            int shiftCount = 0;
            while (inBitArray != 1)
            {
                inBitArray >>= 1;
                ++shiftCount;
            }
            return shiftCount;
        }

        /// <summary>
        /// Returns the bit index of the given bit array, if it contains a single set bit.
        /// </summary>
        [MethodImpl(256)]
        static public int IndexOf<T>(T inBitArray)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            return IndexOf(Enums.ToUInt(inBitArray));
        }

        /// <summary>
        /// Returns the number of set bits in the given bit array.
        /// </summary>
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
        static public int Count(uint inBitArray)
        {
            int count = 0;
            while(inBitArray != 0)
            {
                count += (int) (inBitArray & 1);
                inBitArray >>= 1;
            }
            return count;
        }

        /// <summary>
        /// Returns the number of set bits in the given enum.
        /// </summary>
        [MethodImpl(256)]
        static public int Count<T>(T inBitArray)
        #if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum
        #elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
        #else
            where T : struct, IConvertible
        #endif // HAS_ENUM_CONSTRAINT
        {
            return Count(Enums.ToUInt(inBitArray));
        }
    }
}