/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    LookupTables.cs
 * Purpose: Contains some simple lookup tables to help avoid garbage generation.
 */

using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Contains lookup tables, primarily for string values.
    /// This avoids unnecessary allocations.
    /// For example, int.ToString() creates a new string each time,
    /// but ToStringLookup() shares the same string between all
    /// instances of a given integer value (within a certain range).
    /// </summary>
    static public class LookupTables
    {
        static private string[] s_IntegerTable;
        private const int INTEGER_MIN = -128;
        private const int INTEGER_MAX = 255;

        static LookupTables()
        {
            s_IntegerTable = new string[INTEGER_MAX - INTEGER_MIN + 1];
            for (int i = 0; i < s_IntegerTable.Length; ++i)
                s_IntegerTable[i] = (i + INTEGER_MIN).ToString();
        }

#pragma warning disable CS0652

        /// <summary>
        /// Retrieves a cached version of the string
        /// representation of this integer value.
        /// Range [-128, 255]
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string ToStringLookup(this sbyte inValue)
        {
            return s_IntegerTable[inValue - INTEGER_MIN];
        }

        /// <summary>
        /// Retrieves a cached version of the string
        /// representation of this integer value.
        /// Range [-128, 255]
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string ToStringLookup(this short inValue)
        {
            if (inValue >= INTEGER_MIN && inValue <= INTEGER_MAX)
                return s_IntegerTable[inValue - INTEGER_MIN];
            return inValue.ToString();
        }

        /// <summary>
        /// Retrieves a cached version of the string
        /// representation of this integer value.
        /// Range [-128, 255]
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string ToStringLookup(this int inValue)
        {
            if (inValue >= INTEGER_MIN && inValue <= INTEGER_MAX)
                return s_IntegerTable[inValue - INTEGER_MIN];
            return inValue.ToString();
        }

        /// <summary>
        /// Retrieves a cached version of the string
        /// representation of this integer value.
        /// Range [-128, 255]
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string ToStringLookup(this long inValue)
        {
            if (inValue >= INTEGER_MIN && inValue <= INTEGER_MAX)
                return s_IntegerTable[inValue - INTEGER_MIN];
            return inValue.ToString();
        }

        /// <summary>
        /// Retrieves a cached version of the string
        /// representation of this integer value.
        /// Range [0, 255]
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string ToStringLookup(this byte inValue)
        {
            return s_IntegerTable[inValue - INTEGER_MIN];
        }

        /// <summary>
        /// Retrieves a cached version of the string
        /// representation of this integer value.
        /// Range [0, 255]
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string ToStringLookup(this ushort inValue)
        {
            if (inValue <= INTEGER_MAX)
                return s_IntegerTable[inValue - INTEGER_MIN];
            return inValue.ToString();
        }

        /// <summary>
        /// Retrieves a cached version of the string
        /// representation of this integer value.
        /// Range [0, 255]
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string ToStringLookup(this uint inValue)
        {
            if (inValue <= INTEGER_MAX)
                return s_IntegerTable[inValue - INTEGER_MIN];
            return inValue.ToString();
        }

        /// <summary>
        /// Retrieves a cached version of the string
        /// representation of this integer value.
        /// Range [0, 255]
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string ToStringLookup(this ulong inValue)
        {
            if (inValue <= INTEGER_MAX)
                return s_IntegerTable[(int) inValue - INTEGER_MIN];
            return inValue.ToString();
        }
    }

#pragma warning restore CS0652
}
