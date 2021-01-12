/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Bits.cs
 * Purpose: Helper methods for dealing with bit masks on integers and unsigned integers.
 */

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
        static public bool Contains(uint inBitArray, int inBitIndex)
        {
            return (inBitArray & (1U << inBitIndex)) != 0;
        }

        /// <summary>
        /// Returns if the given int has the given bit toggled on.
        /// </summary>
        static public bool Contains(int inBitArray, int inBitIndex)
        {
            return (inBitArray & (1 << inBitIndex)) != 0;
        }

        /// <summary>
        /// Returns if the given uint contains the given mask.
        /// </summary>
        static public bool ContainsMask(uint inBitArray, uint inBitMask)
        {
            return (inBitArray & inBitMask) != 0;
        }

        /// <summary>
        /// Returns if the given int contains the given mask.
        /// </summary>
        static public bool ContainsMask(int inBitArray, int inBitMask)
        {
            return (inBitArray & inBitMask) != 0;
        }

        /// <summary>
        /// Toggles the given bit in the given uint to on.
        /// </summary>
        static public void Add(ref uint ioBitArray, int inBitIndex)
        {
            ioBitArray |= (1U << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given int to on.
        /// </summary>
        static public void Add(ref int ioBitArray, int inBitIndex)
        {
            ioBitArray |= (1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given uint to off.
        /// </summary>
        static public void Remove(ref uint ioBitArray, int inBitIndex)
        {
            ioBitArray &= ~(1U << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given int to off.
        /// </summary>
        static public void Remove(ref int ioBitArray, int inBitIndex)
        {
            ioBitArray &= ~(1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given uint to the given state.
        /// </summary>
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
        static public void Set(ref int ioBitArray, int inBitIndex, bool inbState)
        {
            if (inbState)
                Add(ref ioBitArray, inBitIndex);
            else
                Remove(ref ioBitArray, inBitIndex);
        }

        /// <summary>
        /// Returns the bit index of the given bit array, if it contains a single set bit.
        /// </summary>
        static public int IndexOf(int inBitArray)
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
        /// Returns the number of set bits in the given bit array.
        /// </summary>
        static public int Count(int inBitArray)
        {
            int count = 0;
            int mask = 1;
            for(int i = 0; i < 32; ++i)
            {
                if ((inBitArray & mask) != 0)
                    ++count;
                
                mask <<= 1;
            }
            return count;
        }

        /// <summary>
        /// Returns the number of set bits in the given bit array.
        /// </summary>
        static public int Count(uint inBitArray)
        {
            int count = 0;
            uint mask = 1;
            for(int i = 0; i < 32; ++i)
            {
                if ((inBitArray & mask) != 0)
                    ++count;
                
                mask <<= 1;
            }
            return count;
        }
    }
}