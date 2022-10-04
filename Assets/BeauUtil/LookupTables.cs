/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    LookupTables.cs
 * Purpose: Contains some simple lookup tables to help avoid garbage generation.
 */

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
        private const int INTEGER_MIN = -100;
        private const int INTEGER_MAX = 100;

        static LookupTables()
        {
            s_IntegerTable = new string[INTEGER_MAX - INTEGER_MIN + 1];
            for (int i = 0; i < s_IntegerTable.Length; ++i)
                s_IntegerTable[i] = (i + INTEGER_MIN).ToString();
        }

        /// <summary>
        /// Retrieves a cached version of the string
        /// representation of this integer value.
        /// Range [-100, 100]
        /// </summary>
        static public string ToStringLookup(this int inValue)
        {
            if (inValue >= INTEGER_MIN && inValue <= INTEGER_MAX)
                return s_IntegerTable[inValue - INTEGER_MIN];
            return inValue.ToString();
        }

        /// <summary>
        /// Retrieves a cached version of the string
        /// representation of this integer value.
        /// Range [-100, 100]
        /// </summary>
        static public string ToStringLookup(this uint inValue)
        {
            if (inValue <= INTEGER_MAX)
                return s_IntegerTable[inValue - INTEGER_MIN];
            return inValue.ToString();
        }
    }
}
