/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
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

            for(int i = 2; i <= 9; ++i)
            {
                for(int j = 1; j <= 9; ++j)
                {
                    s_IntegerWordTable[i * 10 + j] = s_IntegerWordTable[i * 10] + "-" + s_IntegerWordTable[j];
                }
            }
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
        /// Range: [0, 100]
        /// </summary>
        static public string ToStringEnglish(this int inValue)
        {
            if (inValue >= 0 && inValue < s_IntegerWordTable.Length)
                return s_IntegerWordTable[inValue];
            return inValue.ToString();
        }

        static private string[] s_IntegerWordTable = new string[101]
        {
            "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
            "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen",
            "twenty", null, null, null, null, null, null, null, null, null,
            "thirty", null, null, null, null, null, null, null, null, null,
            "forty", null, null, null, null, null, null, null, null, null,
            "fifty", null, null, null, null, null, null, null, null, null,
            "sixty", null, null, null, null, null, null, null, null, null,
            "seventy", null, null, null, null, null, null, null, null, null,
            "eighty", null, null, null, null, null, null, null, null, null,
            "ninety", null, null, null, null, null, null, null, null, null,
            "one-hundred"
        };
    }
}
