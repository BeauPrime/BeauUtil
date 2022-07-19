/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 Oct 2019
 * 
 * File:    StringUtils.cs
 * Purpose: String utility methods.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace BeauUtil
{
    /// <summary>
    /// String utility methods.
    /// </summary>
    static public class StringUtils
    {
        /// <summary>
        /// Custom escape definition.
        /// </summary>
        public interface ICustomEscapeEvaluator
        {
            /// <summary>
            /// Attempts to write an escaped sequence for the given character.
            /// Should return whether or not this evaluator was able to handle the character.
            /// </summary>
            /// <param name="inCharacter">Character to escape.</param>
            /// <param name="ioBuilder">Target for writing the escaped sequence.</param>
            bool TryEscape(char inCharacter, StringBuilder ioBuilder);

            /// <summary>
            /// Attempts to unescape the given character.
            /// Should return whether or not this evaluator was able to handle the character.
            /// </summary>
            /// <param name="inCharacter">Character to unescape.</param>
            /// <param name="inString">Source string.</param>
            /// <param name="inIndex">Current index.</param>
            /// <param name="outAdvance">Output for additional characters read.</param>
            /// <param name="ioBuilder">Target for writing the unescaped sequence.</param>
            bool TryUnescape(char inCharacter, string inString, int inIndex, out int outAdvance, StringBuilder ioBuilder);
        }

        #region Escape

        /// <summary>
        /// Escapes a string to the given string builder.
        /// </summary>
        static public void Escape(string inString, StringBuilder ioBuilder, ICustomEscapeEvaluator inCustomEscape = null)
        {
            if (inString == null)
                return;

            Escape(inString, 0, inString.Length, ioBuilder, inCustomEscape);
        }

        /// <summary>
        /// Escapes a string.
        /// </summary>
        static public string Escape(string inString, ICustomEscapeEvaluator inCustomEscape = null)
        {
            StringBuilder sb = new StringBuilder();
            Escape(inString, sb, inCustomEscape);
            return sb.ToString();
        }

        /// <summary>
        /// Escapes a substring to the given string builder.
        /// </summary>
        static public void Escape(string inString, int inStartIndex, StringBuilder ioBuilder, ICustomEscapeEvaluator inCustomEscape = null)
        {
            if (inString == null)
                return;

            Escape(inString, inStartIndex, inString.Length - inStartIndex, ioBuilder, inCustomEscape);
        }

        /// <summary>
        /// Escapes a substring.
        /// </summary>
        static public string Escape(string inString, int inStartIndex, ICustomEscapeEvaluator inCustomEscape = null)
        {
            StringBuilder sb = new StringBuilder();
            Escape(inString, inStartIndex, sb, inCustomEscape);
            return sb.ToString();
        }

        /// <summary>
        /// Escapes a substring to the given string builder.
        /// </summary>
        static public void Escape(string inString, int inStartIndex, int inLength, StringBuilder ioBuilder, ICustomEscapeEvaluator inCustomEscape = null)
        {
            if (inString == null || inLength <= 0)
                return;

            if (inStartIndex < 0 || inStartIndex >= inString.Length)
            {
                throw new ArgumentOutOfRangeException("Starting index is out of range");
            }

            int endIdx = inStartIndex + inLength;
            if (endIdx > inString.Length)
            {
                throw new ArgumentOutOfRangeException("Length extends beyond end of string");
            }

            for (int idx = inStartIndex; idx < endIdx; ++idx)
            {
                char c = inString[idx];
                if (inCustomEscape != null)
                {
                    if (inCustomEscape.TryEscape(c, ioBuilder))
                        continue;
                }

                switch (c)
                {
                    case '\\':
                        ioBuilder.Append("\\\\");
                        break;

                    case '\"':
                        ioBuilder.Append("\\\"");
                        break;

                    case '\0':
                        ioBuilder.Append("\\0");
                        break;

                    case '\a':
                        ioBuilder.Append("\\a");
                        break;

                    case '\v':
                        ioBuilder.Append("\\v");
                        break;

                    case '\t':
                        ioBuilder.Append("\\t");
                        break;

                    case '\b':
                        ioBuilder.Append("\\b");
                        break;

                    case '\f':
                        ioBuilder.Append("\\f");
                        break;

                    case '\n':
                        ioBuilder.Append("\\n");
                        break;

                    default:
                        ioBuilder.Append(c);
                        break;
                }
            }
        }

        /// <summary>
        /// Escapes a substring.
        /// </summary>
        static public string Escape(string inString, int inStartIndex, int inLength, ICustomEscapeEvaluator inCustomEscape = null)
        {
            StringBuilder sb = new StringBuilder(inLength);
            Escape(inString, inStartIndex, inLength, sb, inCustomEscape);
            return sb.ToString();
        }

        /// <summary>
        /// Escapes a string builder to another string builder.
        /// </summary>
        [MethodImpl(256)]
        static public void Escape(StringBuilder inString, StringBuilder ioBuilder)
        {
            Escape(inString, 0, inString.Length, ioBuilder);
        }

        /// <summary>
        /// Escapes a string builder to another string builder.
        /// </summary>
        static public void Escape(StringBuilder inString, int inStartIndex, int inLength, StringBuilder ioBuilder)
        {
            if (inString == null || inLength <= 0)
                return;

            if (inStartIndex < 0 || inStartIndex >= inString.Length)
            {
                throw new ArgumentOutOfRangeException("Starting index is out of range");
            }

            int endIdx = inStartIndex + inLength;
            if (endIdx > inString.Length)
            {
                throw new ArgumentOutOfRangeException("Length extends beyond end of string");
            }

            for (int idx = inStartIndex; idx < endIdx; ++idx)
            {
                char c = inString[idx];

                switch (c)
                {
                    case '\\':
                        ioBuilder.Append("\\\\");
                        break;

                    case '\"':
                        ioBuilder.Append("\\\"");
                        break;

                    case '\0':
                        ioBuilder.Append("\\0");
                        break;

                    case '\a':
                        ioBuilder.Append("\\a");
                        break;

                    case '\v':
                        ioBuilder.Append("\\v");
                        break;

                    case '\t':
                        ioBuilder.Append("\\t");
                        break;

                    case '\b':
                        ioBuilder.Append("\\b");
                        break;

                    case '\f':
                        ioBuilder.Append("\\f");
                        break;

                    case '\n':
                        ioBuilder.Append("\\n");
                        break;

                    default:
                        ioBuilder.Append(c);
                        break;
                }
            }
        }

        #endregion // Escape

        #region Unescape

        /// <summary>
        /// Unescapes a string to the given string builder.
        /// </summary>
        static public void Unescape(string inString, StringBuilder ioBuilder, ICustomEscapeEvaluator inCustomEscape = null)
        {
            if (inString == null)
                return;

            Unescape(inString, 0, inString.Length, ioBuilder, inCustomEscape);
        }

        /// <summary>
        /// Unescapes a string.
        /// </summary>
        static public string Unescape(string inString, ICustomEscapeEvaluator inCustomEscape = null)
        {
            StringBuilder sb = new StringBuilder();
            Unescape(inString, sb, inCustomEscape);
            return sb.ToString();
        }

        /// <summary>
        /// Unescapes a substring to the given string builder.
        /// </summary>
        static public void Unescape(string inString, int inStartIndex, StringBuilder ioBuilder, ICustomEscapeEvaluator inCustomEscape = null)
        {
            if (inString == null)
                return;

            Unescape(inString, inStartIndex, inString.Length - inStartIndex, ioBuilder, inCustomEscape);
        }

        /// <summary>
        /// Unescapes a substring.
        /// </summary>
        static public string Unescape(string inString, int inStartIndex, ICustomEscapeEvaluator inCustomEscape = null)
        {
            StringBuilder sb = new StringBuilder();
            Unescape(inString, inStartIndex, sb, inCustomEscape);
            return sb.ToString();
        }

        /// <summary>
        /// Unescapes a substring to the given string builder.
        /// </summary>
        static public void Unescape(string inString, int inStartIndex, int inLength, StringBuilder ioBuilder, ICustomEscapeEvaluator inCustomEscape = null)
        {
            if (inString == null || inLength <= 0)
                return;

            if (inStartIndex < 0 || inStartIndex >= inString.Length)
            {
                throw new ArgumentOutOfRangeException("Starting index is out of range");
            }

            int endIdx = inStartIndex + inLength;
            if (endIdx > inString.Length)
            {
                throw new ArgumentOutOfRangeException("Length extends beyond end of string");
            }

            for (int idx = inStartIndex; idx < endIdx; ++idx)
            {
                char c = inString[idx];

                if (inCustomEscape != null)
                {
                    int advance = 0;
                    if (inCustomEscape.TryUnescape(c, inString, idx, out advance, ioBuilder))
                    {
                        idx += advance;
                        continue;
                    }
                }

                if (c == '\\')
                {
                    ++idx;
                    c = inString[idx];
                    switch (c)
                    {
                        case '0':
                            ioBuilder.Append('\0');
                            break;

                        case 'a':
                            ioBuilder.Append('\a');
                            break;

                        case 'v':
                            ioBuilder.Append('\v');
                            break;

                        case 't':
                            ioBuilder.Append('\t');
                            break;

                        case 'r':
                            ioBuilder.Append('\r');
                            break;

                        case 'n':
                            ioBuilder.Append('\n');
                            break;

                        case 'b':
                            ioBuilder.Append('\b');
                            break;

                        case 'f':
                            ioBuilder.Append('\f');
                            break;

                        case 'u':
                            {
                                string unicode = inString.Substring(idx + 1, 4);
                                char code = (char)int.Parse(unicode, NumberStyles.AllowHexSpecifier);
                                ioBuilder.Append(code);
                                idx += 4;
                                break;
                            }

                        default:
                            ioBuilder.Append(c);
                            break;
                    }
                }
                else
                {
                    ioBuilder.Append(c);
                }
            }
        }

        /// <summary>
        /// Unescapes a substring.
        /// </summary>
        static public string Unescape(string inString, int inStartIndex, int inLength, ICustomEscapeEvaluator inCustomEscape = null)
        {
            StringBuilder sb = new StringBuilder(inLength);
            Unescape(inString, inStartIndex, inLength, sb, inCustomEscape);
            return sb.ToString();
        }

        /// <summary>
        /// Unescapes a string builder to another string builder.
        /// </summary>
        [MethodImpl(256)]
        static public void Unescape(StringBuilder inString, StringBuilder ioBuilder)
        {
            Unescape(inString, 0, inString.Length, ioBuilder);
        }

        /// <summary>
        /// Unescapes a string builder to another string builder.
        /// </summary>
        static public void Unescape(StringBuilder inString, int inStartIndex, int inLength, StringBuilder ioBuilder)
        {
            if (inString == null || inLength <= 0)
                return;

            if (inStartIndex < 0 || inStartIndex >= inString.Length)
            {
                throw new ArgumentOutOfRangeException("Starting index is out of range");
            }

            int endIdx = inStartIndex + inLength;
            if (endIdx > inString.Length)
            {
                throw new ArgumentOutOfRangeException("Length extends beyond end of string");
            }

            for (int idx = 0; idx < inLength; ++idx)
            {
                char c = inString[idx];

                if (c == '\\')
                {
                    ++idx;
                    c = inString[idx];
                    switch (c)
                    {
                        case '0':
                            ioBuilder.Append('\0');
                            break;

                        case 'a':
                            ioBuilder.Append('\a');
                            break;

                        case 'v':
                            ioBuilder.Append('\v');
                            break;

                        case 't':
                            ioBuilder.Append('\t');
                            break;

                        case 'r':
                            ioBuilder.Append('\r');
                            break;

                        case 'n':
                            ioBuilder.Append('\n');
                            break;

                        case 'b':
                            ioBuilder.Append('\b');
                            break;

                        case 'f':
                            ioBuilder.Append('\f');
                            break;

                        case 'u':
                            {
                                string unicode = inString.ToString(idx + 1, 4);
                                char code = (char)int.Parse(unicode, NumberStyles.AllowHexSpecifier);
                                ioBuilder.Append(code);
                                idx += 4;
                                break;
                            }

                        default:
                            ioBuilder.Append(c);
                            break;
                    }
                }
                else
                {
                    ioBuilder.Append(c);
                }
            }
        }

        /// <summary>
        /// Unescapes a string builder to itself.
        /// </summary>
        [MethodImpl(256)]
        static public void UnescapeInline(StringBuilder ioString)
        {
            UnescapeInline(ioString, 0, ioString.Length);
        }

        /// <summary>
        /// Unescapes a string builder to itself.
        /// </summary>
        static public void UnescapeInline(StringBuilder ioString, int inStartIndex, int inLength)
        {
            unsafe
            {
                int length = inLength;
                char* inlineCopy = stackalloc char[length];

                for(int i = 0; i < length; i++)
                    inlineCopy[i] = ioString[inStartIndex + i];

                ioString.Length -= inLength;

                for (int idx = 0; idx < length; ++idx)
                {
                    char c = inlineCopy[idx];

                    if (c == '\\')
                    {
                        ++idx;
                        if (idx >= length)
                        {
                            ioString.Append(c);
                            break;
                        }
                        
                        c = inlineCopy[idx];
                        switch (c)
                        {
                            case '0':
                                ioString.Append('\0');
                                break;

                            case 'a':
                                ioString.Append('\a');
                                break;

                            case 'v':
                                ioString.Append('\v');
                                break;

                            case 't':
                                ioString.Append('\t');
                                break;

                            case 'r':
                                ioString.Append('\r');
                                break;

                            case 'n':
                                ioString.Append('\n');
                                break;

                            case 'b':
                                ioString.Append('\b');
                                break;

                            case 'f':
                                ioString.Append('\f');
                                break;

                            case 'u':
                                {
                                    string unicode = new string(inlineCopy, idx + 1, 4);
                                    char code = (char)int.Parse(unicode, NumberStyles.AllowHexSpecifier);
                                    ioString.Append(code);
                                    idx += 4;
                                    break;
                                }

                            default:
                                ioString.Append(c);
                                break;
                        }
                    }
                    else
                    {
                        ioString.Append(c);
                    }
                }
            }
        }

        #endregion // Unescape

        #region Equals

        /// <summary>
        /// Determines if two strings are equal,
        /// treating null and string.empty as equivalent.
        /// </summary>
        static public bool NullEquivalentEquals(string inStringA, string inStringB)
        {
            inStringA = inStringA ?? string.Empty;
            inStringB = inStringB ?? string.Empty;
            return inStringA.Equals(inStringB);
        }

        /// <summary>
        /// Determines if two strings are equal,
        /// treating null and string.empty as equivalent.
        /// </summary>
        static public bool NullEquivalentEquals(string inStringA, string inStringB, StringComparison inComparison)
        {
            inStringA = inStringA ?? string.Empty;
            inStringB = inStringB ?? string.Empty;
            return inStringA.Equals(inStringB, inComparison);
        }

        #endregion // Equals

        #region Pattern Matching

        /// <summary>
        /// Determines if a string matches any of the given filters.
        /// Wildcard characters are supported at the start and end of the filter.
        /// </summary>
        /// <remarks>
        /// This does not yet support wildcards in the middle of the filter.
        /// </remarks>
        static public bool WildcardMatch(StringSlice inString, string[] inFilters, char inWildcard = '*', bool inbIgnoreCase = false)
        {
            if (inFilters.Length == 0)
                return false;

            for (int i = 0; i < inFilters.Length; ++i)
            {
                if (WildcardMatch(inString, inFilters[i], inWildcard, inbIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if a string matches any of the given filters.
        /// Wildcard characters are supported at the start and end of the filter.
        /// </summary>
        /// <remarks>
        /// This does not yet support wildcards in the middle of the filter.
        /// </remarks>
        static public bool WildcardMatch(StringSlice inString, ICollection<string> inFilters, char inWildcard = '*', bool inbIgnoreCase = false)
        {
            if (inFilters.Count == 0)
                return false;

            foreach (var filter in inFilters)
            {
                if (WildcardMatch(inString, filter, inWildcard, inbIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if a string matches the given filter.
        /// Wildcard characters are supported at the start and end of the filter.
        /// </summary>
        /// <remarks>
        /// This does not yet support wildcards in the middle of the filter.
        /// </remarks>
        static public bool WildcardMatch(StringSlice inString, string inFilter, char inWildcard = '*', bool inbIgnoreCase = false)
        {
            if (string.IsNullOrEmpty(inFilter))
            {
                return inString.IsEmpty;
            }

            int filterLength = inFilter.Length;
            if (filterLength == 1 && inFilter[0] == inWildcard)
                return true;

            if (filterLength == 2 && inFilter[0] == inWildcard && inFilter[1] == inWildcard)
                return true;

            bool bStart = inFilter[0] == inWildcard;
            bool bEnd = inFilter[filterLength - 1] == inWildcard;
            if (bStart || bEnd)
            {
                string filterStr = inFilter;
                int startIdx = 0;
                if (bStart)
                {
                    ++startIdx;
                    --filterLength;
                }
                if (bEnd)
                {
                    --filterLength;
                }

                filterStr = filterStr.Substring(startIdx, filterLength);
                if (bStart && bEnd)
                {
                    return inString.Contains(filterStr, inbIgnoreCase);
                }
                if (bStart)
                {
                    return inString.EndsWith(filterStr, inbIgnoreCase);
                }
                return inString.StartsWith(filterStr, inbIgnoreCase);
            }

            return inString.Equals(inFilter, inbIgnoreCase);
        }

        /// <summary>
        /// Returns if the given string contains the given string at the given index.
        /// </summary>
        static public bool AttemptMatch(this string inString, int inIndex, string inMatch, bool inbIgnoreCase = false)
        {
            if (string.IsNullOrEmpty(inString))
                return false;

            if (string.IsNullOrEmpty(inMatch))
                return false;

            if (inIndex < 0 || inIndex + inMatch.Length > inString.Length)
                return false;

            for (int i = 0; i < inMatch.Length; ++i)
            {
                char a = inString[inIndex + i];
                char b = inMatch[i];
                if (!inbIgnoreCase)
                {
                    if (a != b)
                        return false;
                }
                else
                {
                    if (char.ToLowerInvariant(a) != char.ToLowerInvariant(b))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns if the given StringSlice contains the given string at the given index.
        /// </summary>
        static public bool AttemptMatch(this StringSlice inString, int inIndex, string inMatch, bool inbIgnoreCase = false)
        {
            if (inString.IsEmpty)
                return false;

            if (string.IsNullOrEmpty(inMatch))
                return false;

            if (inIndex < 0 || inIndex + inMatch.Length > inString.Length)
                return false;

            for (int i = 0; i < inMatch.Length; ++i)
            {
                char a = inString[inIndex + i];
                char b = inMatch[i];
                if (!inbIgnoreCase)
                {
                    if (a != b)
                        return false;
                }
                else
                {
                    if (char.ToLowerInvariant(a) != char.ToLowerInvariant(b))
                        return false;
                }
            }

            return true;
        }

        #endregion // Pattern Matching

        #region StringBuilder

        /// <summary>
        /// Retrieves the string from the StringBuilder
        /// and clears the StringBuilder's state.
        /// </summary>
        static public string Flush(this StringBuilder ioBuilder)
        {
            if (ioBuilder == null)
                throw new ArgumentNullException("ioBuilder");

            if (ioBuilder.Length <= 0)
                return string.Empty;

            string str = ioBuilder.ToString();
            ioBuilder.Length = 0;
            return str;
        }

        /// <summary>
        /// Appends a StringSlice to the given StringBuilder.
        /// </summary>
        static public StringBuilder AppendSlice(this StringBuilder ioBuilder, StringSlice inSlice)
        {
            if (ioBuilder == null)
                throw new ArgumentNullException("ioBuilder");

            inSlice.AppendTo(ioBuilder);
            return ioBuilder;
        }

        /// <summary>
        /// Returns if the given StringBuilder contains the given string at the given index.
        /// </summary>
        static public bool AttemptMatch(this StringBuilder inBuilder, int inIndex, string inMatch, bool inbIgnoreCase = false)
        {
            if (inBuilder == null)
                throw new ArgumentNullException("inBuilder");

            if (string.IsNullOrEmpty(inMatch))
                return false;

            int matchLength = inMatch.Length;

            if (inIndex < 0 || inIndex + matchLength > inBuilder.Length)
                return false;

            if (!inbIgnoreCase)
            {
                for (int i = 0; i < matchLength; ++i)
                {
                    char a = inBuilder[inIndex + i];
                    char b = inMatch[i];
                    if (a != b)
                        return false;
                }
            }
            else
            {
                for (int i = 0; i < matchLength; ++i)
                {
                    char a = inBuilder[inIndex + i];
                    char b = inMatch[i];
                    if (char.ToLowerInvariant(a) != char.ToLowerInvariant(b))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns if the given StringBuilder contains the given string at the given index, looking backwards.
        /// </summary>
        static public bool AttemptMatchEnd(this StringBuilder inBuilder, int inIndex, string inMatch, bool inbIgnoreCase = false)
        {
            if (inBuilder == null)
                throw new ArgumentNullException("inBuilder");

            if (string.IsNullOrEmpty(inMatch))
                return false;

            int matchLength = inMatch.Length;

            if (inIndex >= inBuilder.Length || inIndex - matchLength < 0)
                return false;

            if (!inbIgnoreCase)
            {
                for (int i = 0; i < matchLength; ++i)
                {
                    char a = inBuilder[inIndex - matchLength + 1 + i];
                    char b = inMatch[i];
                    if (a != b)
                        return false;
                }
            }
            else
            {
                for (int i = 0; i < matchLength; ++i)
                {
                    char a = inBuilder[inIndex - matchLength + 1 + i];
                    char b = inMatch[i];
                    if (char.ToLowerInvariant(a) != char.ToLowerInvariant(b))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Trims characters from the end of a StringBuilder.
        /// </summary>
        static public StringBuilder TrimEnd(this StringBuilder ioBuilder, char[] inTrimChars)
        {
            if (ioBuilder == null)
                throw new ArgumentNullException("ioBuilder");
            if (inTrimChars == null)
                throw new ArgumentException("inTrimChars");

            if (inTrimChars.Length == 0)
                return ioBuilder;

            int length = ioBuilder.Length;
            char c;
            bool bFound;
            int toRemove = 0;
            while(length > 0)
            {
                c = ioBuilder[length - 1];
                bFound = false;
                for(int i = 0; !bFound && i < inTrimChars.Length; ++i)
                {
                    bFound = c == inTrimChars[i];
                }

                if (bFound)
                {
                    --length;
                    ++toRemove;
                }
                else
                {
                    break;
                }
            }

            if (toRemove > 0)
            {
                ioBuilder.Length -= toRemove;
            }

            return ioBuilder;
        }

        /// <summary>
        /// Returns the first index of the given character.
        /// </summary>
        static public int IndexOf(this StringBuilder inBuilder, char inChar, int inStartIndex, int inCount)
        {
            int end = inStartIndex + inCount;
            for(int i = inStartIndex; i < end; i++)
            {
                if (inBuilder[i] == inChar)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Returns the first index of the given character.
        /// </summary>
        [MethodImpl(256)]
        static public int IndexOf(this StringBuilder inBuilder, char inChar)
        {
            return IndexOf(inBuilder, inChar, 0, inBuilder.Length);
        }

        /// <summary>
        /// Returns the first index of the given string.
        /// </summary>
        static public int IndexOf(this StringBuilder inBuilder, string inString, int inStartIndex, int inCount)
        {
            if (string.IsNullOrEmpty(inString))
                return -1;

            char first = inString[0];
            if (inString.Length == 1)
                return IndexOf(inBuilder, first, inStartIndex, inCount);
            
            int end = inStartIndex + inCount - inString.Length;
            for(int i = inStartIndex; i < end; i++)
            {
                if (inBuilder[i] == first && AttemptMatch(inBuilder, i, inString))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Returns the first index of the given string.
        /// </summary>
        [MethodImpl(256)]
        static public int IndexOf(this StringBuilder inBuilder, string inString)
        {
            return IndexOf(inBuilder, inString, 0, inBuilder.Length);
        }

        #endregion // StringBuilder

        #region UTF8

        /// <summary>
        /// Decodes a UTF8 byte buffer into a char buffer.
        /// </summary>
        [MethodImpl(256)]
        static public unsafe int DecodeUFT8(byte* inBuffer, int inCount, char* outCharBuffer, int outCharBufferLength)
        {
            return Encoding.UTF8.GetChars(inBuffer, inCount, outCharBuffer, outCharBufferLength);
        }

        /// <summary>
        /// Returns the maximum size of a char buffer needed to decode a UTF8 byte buffer.
        /// </summary>
        [MethodImpl(256)]
        static public int DecodeSizeUTF8(int inByteCount)
        {
            return Encoding.UTF8.GetMaxCharCount(inByteCount);
        }

        /// <summary>
        /// Decodes a byte buffer into a UTF-8 char buffer.
        /// </summary>
        [MethodImpl(256)]
        static public unsafe int EncodeUFT8(char* inBuffer, int inCount, byte* outByteBuffer, int outByteBufferLength)
        {
            return Encoding.UTF8.GetBytes(inBuffer, inCount, outByteBuffer, outByteBufferLength);
        }

        /// <summary>
        /// Returns the maximum size of a UTF8 byte buffer needed to encode a char buffer.
        /// </summary>
        [MethodImpl(256)]
        static public int EncodeSizeUTF8(int inCharCount)
        {
            return Encoding.UTF8.GetMaxByteCount(inCharCount);
        }

        #endregion // UTF8

        /// <summary>
        /// CSV utils.
        /// </summary>
        static public class CSV
        {
            /// <summary>
            /// String splitter for CSV.
            /// </summary>
            public sealed class Splitter : StringSlice.ISplitter
            {
                private const int QUOTE_FLAG = 1;

                private readonly bool m_Unescape;

                public Splitter(bool inbUnescape = false)
                {
                    m_Unescape = inbUnescape;
                }

                public bool Evaluate(string inString, int inIndex, int inSliceCount, ref uint ioState, out int outAdvance)
                {
                    outAdvance = 0;
                    char c = inString[inIndex];
                    if (c == ',')
                    {
                        return ioState == 0;
                    }
                    else if (c == '"')
                    {
                        if (ioState == QUOTE_FLAG)
                        {
                            if (inIndex < inString.Length - 1 && inString[inIndex + 1] == '"')
                            {
                                outAdvance = 1;
                            }
                            else
                            {
                                ioState = 0;
                            }
                        }
                        else
                        {
                            ioState = QUOTE_FLAG;
                        }
                    }

                    return false;
                }

                public StringSlice Process(StringSlice inSlice, uint inState)
                {
                    StringSlice slice = inSlice.TrimStart().Trim(DefaultQuoteChar);

                    // if this contains escaped CSV sequences, unescape it here
                    if (m_Unescape && (slice.Contains("\\") || slice.Contains("\"\"")))
                    {
                        return slice.Unescape(Escaper.Instance);
                    }
                    return slice;
                }
            }

            /// <summary>
            /// Escape/unescape for CSV.
            /// </summary>
            public sealed class Escaper : ICustomEscapeEvaluator
            {
                static public readonly Escaper Instance = new Escaper();

                public bool TryEscape(char inCharacter, StringBuilder ioBuilder)
                {
                    if (inCharacter == '"')
                    {
                        ioBuilder.Append("\"\"");
                        return true;
                    }

                    return false;
                }

                public bool TryUnescape(char inCharacter, string inString, int inIndex, out int outAdvance, StringBuilder ioBuilder)
                {
                    if (inCharacter == '"')
                    {
                        if (inIndex < inString.Length - 1 && inString[inIndex + 1] == '"')
                        {
                            ioBuilder.Append('"');
                            outAdvance = 1;
                            return true;
                        }
                    }

                    outAdvance = 0;
                    return false;
                }
            }
        }

        /// <summary>
        /// Delimiter-separated argument list utils.
        /// </summary>
        static public class ArgsList
        {
            /// <summary>
            /// Trims quotes from the start and end of a string.
            /// </summary>
            static public StringSlice TrimQuotes(StringSlice inString)
            {
                if (inString.Length >= 2 && inString[0] == '"' && inString[inString.Length - 1] == '"')
                    return inString.Substring(1, inString.Length - 2);
                return inString;
            }

            /// <summary>
            /// Returns if the given string contains at least two delimiter-separated arguments.
            /// </summary>
            static public bool IsList(StringSlice inString, char inDelimiter = ',')
            {
                bool quote = false;
                int group = 0;

                char c;
                for(int i = 0; i < inString.Length; i++)
                {
                    c = inString[i];

                    if (c == inDelimiter)
                    {
                        if (!quote && group <= 0)
                            return true;
                    }

                    switch(c)
                    {
                        case '(':
                        case '[':
                        case '{':
                            if (!quote)
                            {
                                group++;
                            }
                            break;

                        case ')':
                        case ']':
                        case '}':
                            if (!quote)
                            {
                                --group;
                            }
                            break;

                        case '"':
                            if (quote)
                            {
                                if (i > 0 && inString[i - 1] == '\\')
                                {
                                    i++;
                                }
                                else
                                {
                                    quote = !quote;
                                }
                            }
                            else
                            {
                                quote = true;
                            }
                            break;
                    }
                }
                return false;
            }

            /// <summary>
            /// String splitter for arg lists.
            /// </summary>
            public sealed class Splitter : StringSlice.ISplitter
            {
                private const uint QUOTE_FLAG = 1u << 31;
                private const uint GROUP_MASK = (1u << 30) - 1;

                [ThreadStatic] static private Splitter s_Instance;

                static public Splitter Instance
                {
                    get { return s_Instance ?? (s_Instance = new Splitter()); }
                }

                private readonly bool m_TrimQuotes;
                private readonly bool m_Unescape;
                private readonly char m_Delimiter;

                public Splitter(bool inbUnescape = false)
                {
                    m_Delimiter = ',';
                    m_Unescape = inbUnescape;
                    m_TrimQuotes = true;
                }

                public Splitter(char inDelimiter, bool inbUnescape = false)
                {
                    m_Delimiter = inDelimiter;
                    m_Unescape = inbUnescape;
                    m_TrimQuotes = true;
                }

                public Splitter(char inDelimiter, bool inbTrimQuotes, bool inbUnescape)
                {
                    m_TrimQuotes = inbTrimQuotes;
                    m_Unescape = inbUnescape;
                    m_Delimiter = inDelimiter;
                }

                public bool Evaluate(string inString, int inIndex, int inSliceCount, ref uint ioState, out int outAdvance)
                {
                    outAdvance = 0;
                    char c = inString[inIndex];

                    bool quote;
                    uint depth;
                    DeconstructState(ioState, out quote, out depth);

                    if (c == m_Delimiter)
                        return !quote && depth <= 0;

                    switch(c)
                    {
                        case '(':
                        case '[':
                        case '{':
                            if (!quote)
                            {
                                depth++;
                                ioState = ConstructState(quote, depth);
                            }
                            break;

                        case ')':
                        case ']':
                        case '}':
                            if (!quote)
                            {
                                if (depth > 0)
                                {
                                    --depth;
                                }
                                ioState = ConstructState(quote, depth);
                            }
                            break;

                        case '"':
                            if (inIndex > 0 && inString[inIndex - 1] == '\\')
                            {
                                break;
                            }
                            else
                            {
                                quote = !quote;
                                ioState = ConstructState(quote, depth);
                            }
                            break;
                    }

                    return false;
                }

                public StringSlice Process(StringSlice inSlice, uint inState)
                {
                    StringSlice slice = inSlice.Trim();

                    if (m_TrimQuotes)
                    {
                        slice = TrimQuotes(slice);
                    }

                    // if this contains escaped CSV sequences, unescape it here
                    if (m_Unescape && (slice.Contains("\\") || slice.Contains("\\\"")))
                    {
                        return slice.Unescape(Escaper.Instance);
                    }

                    return slice;
                }
            
                static private void DeconstructState(uint inState, out bool outbQuote, out uint outDepth)
                {
                    outbQuote = (inState & QUOTE_FLAG) != 0;
                    outDepth = inState & GROUP_MASK;
                }

                static private uint ConstructState(bool inbQuote, uint inDepth)
                {
                    return (inbQuote ? QUOTE_FLAG : 0) | (inDepth & GROUP_MASK);
                }
            }

            /// <summary>
            /// Escape/unescape for arg lists.
            /// </summary>
            public sealed class Escaper : ICustomEscapeEvaluator
            {
                static public readonly Escaper Instance = new Escaper();

                public bool TryEscape(char inCharacter, StringBuilder ioBuilder)
                {
                    if (inCharacter == '"')
                    {
                        ioBuilder.Append("\\\"");
                        return true;
                    }

                    return false;
                }

                public bool TryUnescape(char inCharacter, string inString, int inIndex, out int outAdvance, StringBuilder ioBuilder)
                {
                    // This is covered by the standard unescaper
                    
                    // if (inCharacter == '\\')
                    // {
                    //     if (inIndex < inString.Length - 1 && inString[inIndex + 1] == '"')
                    //     {
                    //         ioBuilder.Append('"');
                    //         outAdvance = 1;
                    //         return true;
                    //     }
                    // }

                    outAdvance = 0;
                    return false;
                }
            }
        }

        /// <summary>
        /// Rich text utils.
        /// </summary>
        static public class RichText
        {
            static private readonly string[] s_KnownRichTags = new string[]
            {
                "align", "alpha", "color", "b", "i", "cspace", "font", "indent",
                "line-height", "line-indent", "link", "lowercase", "uppercase", "smallcaps", "allcaps",
                "margin", "margin-left", "margin-right", "mark", "mspace", "noparse", "nobr", "page", "pos",
                "size", "space", "sprite", "s", "u", "style", "sub", "sup",
                "voffset", "width", "material", "quad"
            };

            static private readonly string[] s_VisibleRichTags = new string[]
            {
                "sprite", "quad"
            };

            /// <summary>
            /// Returns if this tag generates something visible.
            /// </summary>
            static public bool GeneratesVisibleCharacter(string inRichTag)
            {
                return Array.IndexOf(s_VisibleRichTags, inRichTag) >= 0;
            }

            /// <summary>
            /// List of all recognized rich tags.
            /// This includes Unity's Rich Tags and TextMesh Pro's rich tags.
            /// </summary>
            static public IReadOnlyList<string> RecognizedRichTags { get { return s_KnownRichTags; } }
        }

        /// <summary>
        /// Default newline characters
        /// </summary>
        static public readonly char[] DefaultNewLineChars = new char[] { '\n', '\r' };

        /// <summary>
        /// Default quote character
        /// </summary>
        static public readonly char[] DefaultQuoteChar = new char[] { '"' };
    }
}