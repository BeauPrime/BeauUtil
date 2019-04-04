/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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
        public const int LENGTH = 32;

        /// <summary>
        /// Returns if the given uint has the given bit toggled on.
        /// </summary>
        static public bool Contains(uint inBitArray, byte inBitIndex)
        {
            return (inBitArray & (1U << inBitIndex)) > 0;
        }

        /// <summary>
        /// Returns if the given int has the given bit toggled on.
        /// </summary>
        static public bool Contains(int inBitArray, byte inBitIndex)
        {
            return (inBitArray & (1 << inBitIndex)) > 0;
        }

        /// <summary>
        /// Returns if the given uint contains the given mask.
        /// </summary>
        static public bool ContainsMask(uint inBitArray, uint inBitMask)
        {
            return (inBitArray & inBitMask) > 0;
        }

        /// <summary>
        /// Returns if the given int contains the given mask.
        /// </summary>
        static public bool ContainsMask(int inBitArray, int inBitMask)
        {
            return (inBitArray & inBitMask) > 0;
        }

        /// <summary>
        /// Toggles the given bit in the given uint to on.
        /// </summary>
        static public void Add(ref uint ioBitArray, byte inBitIndex)
        {
            ioBitArray |= (1U << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given int to on.
        /// </summary>
        static public void Add(ref int ioBitArray, byte inBitIndex)
        {
            ioBitArray |= (1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given uint to off.
        /// </summary>
        static public void Remove(ref uint ioBitArray, byte inBitIndex)
        {
            ioBitArray &= ~(1U << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given int to off.
        /// </summary>
        static public void Remove(ref int ioBitArray, byte inBitIndex)
        {
            ioBitArray &= ~(1 << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given uint to the given state.
        /// </summary>
        static public void Set(ref uint ioBitArray, byte inBitIndex, bool inbState)
        {
            if (inbState)
                Add(ref ioBitArray, inBitIndex);
            else
                Remove(ref ioBitArray, inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given int to the given state.
        /// </summary>
        static public void Set(ref int ioBitArray, byte inBitIndex, bool inbState)
        {
            if (inbState)
                Add(ref ioBitArray, inBitIndex);
            else
                Remove(ref ioBitArray, inBitIndex);
        }
    }
}
