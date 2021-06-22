/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    19 Sept 2020
 * 
 * File:    VariantUtils.cs
 * Purpose: Variant utility methods.
 */

using System;

namespace BeauUtil.Variants
{
    /// <summary>
    /// Variant utility methods.
    /// </summary>
    static public class VariantUtils
    {
        /// <summary>
        /// Returns if the given string is a valid identifier.
        /// </summary>
        static public bool IsValidIdentifier(StringSlice inSlice)
        {
            inSlice = inSlice.Trim();
            if (inSlice.Length == 0)
                return false;

            char c = inSlice[0];
            if (char.IsDigit(c))
                return false;

            for(int i = 0; i < inSlice.Length; ++i)
            {
                c = inSlice[i];
                if (char.IsLetterOrDigit(c))
                    continue;
                if (c == '_' || c == '.' || c == '-')
                    continue;
                return false;
            }

            return true;
        }

        #region Parsing

        static internal readonly char[] SymbolsRequiringPrecedingWhitespace = new char[] { '_', '-', '.' };

        static internal bool CharRequiresWhitespace(char inChar)
        {
            return char.IsLetterOrDigit(inChar) || Array.IndexOf(SymbolsRequiringPrecedingWhitespace, inChar) >= 0;
        }

        static internal bool HasWhitespaceAt(StringSlice inSlice, int inIndex)
        {
            return inIndex < 0 || inIndex >= inSlice.Length || char.IsWhiteSpace(inSlice[inIndex]);
        }

        static internal bool TryFindOperator<T>(StringSlice inSlice, string inSearch, T inOperator, ref int ioFoundIndex, ref T ioFoundOperator, ref int ioFoundOperatorLength) where T : struct, IConvertible
        {
            if (ioFoundIndex >= 0)
                return false;

            int index = inSlice.IndexOf(inSearch);
            if (index >= 0)
            {
                if (CharRequiresWhitespace(inSearch[0]))
                {
                    if (!HasWhitespaceAt(inSlice, index - 1))
                        return false;
                    if (inSearch.Length == 1 && !HasWhitespaceAt(inSlice, index + 1))
                        return false;
                }

                if (inSearch.Length > 1 && CharRequiresWhitespace(inSearch[inSearch.Length - 1]))
                {
                    if (!HasWhitespaceAt(inSlice, index + inSearch.Length))
                        return false;
                }

                ioFoundOperator = inOperator;
                ioFoundIndex = index;
                ioFoundOperatorLength = inSearch.Length;
                return true;
            }

            return false;
        }

        static internal bool TryFindOperator<T>(StringSlice inSlice, char inSearch, T inOperator, ref int ioFoundIndex, ref T ioFoundOperator, ref int ioFoundOperatorLength) where T : struct, IConvertible
        {
            if (ioFoundIndex >= 0)
                return false;

            int index = inSlice.IndexOf(inSearch);
            if (index >= 0)
            {
                if (CharRequiresWhitespace(inSearch))
                {
                    if (!HasWhitespaceAt(inSlice, index - 1))
                        return false;
                    if (!HasWhitespaceAt(inSlice, index + 1))
                        return false;
                }

                ioFoundOperator = inOperator;
                ioFoundIndex = index;
                ioFoundOperatorLength = 1;
                return true;
            }

            return false;
        }

        #endregion // Parsing
    }
}