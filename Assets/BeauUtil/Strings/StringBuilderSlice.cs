/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 June 2022
 * 
 * File:    StringBuilderSlice.cs
 * Purpose: Read-only slice of a StringBuilder.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BeauUtil
{
    /// <summary>
    /// A section of a StringBuilder.
    /// </summary>
    public struct StringBuilderSlice : IEnumerable<char>, IReadOnlyList<char>, IEquatable<StringBuilderSlice>, IEquatable<string>
    {
        private readonly StringBuilder m_Source;
        private readonly int m_StartIndex;

        /// <summary>
        /// Total length of the slice.
        /// </summary>
        public readonly int Length;

        public StringBuilderSlice(StringBuilder inString) : this(inString, 0, inString != null ? inString.Length : 0) { }

        public StringBuilderSlice(StringBuilder inString, int inStartIdx) : this(inString, inStartIdx, inString != null ? inString.Length - inStartIdx : 0) { }

        public StringBuilderSlice(StringBuilder inString, int inStartIdx, int inLength)
        {
            if (inString == null || inString.Length == 0 || inLength <= 0)
            {
                m_Source = null;
                m_StartIndex = 0;
                Length = 0;
            }
            else
            {
                if (inStartIdx < 0)
                    throw new ArgumentOutOfRangeException("inStartIdx");
                if (inStartIdx + inLength > inString.Length)
                    throw new ArgumentOutOfRangeException("inLength");
                
                m_Source = inString;
                m_StartIndex = inStartIdx;
                Length = inLength;
            }
        }

        /// <summary>
        /// An empty slice.
        /// </summary>
        static public readonly StringBuilderSlice Empty = default(StringBuilderSlice);

        /// <summary>
        /// Returns if this is an empty slice.
        /// </summary>
        public bool IsEmpty
        {
            get { return Length == 0; }
        }

        /// <summary>
        /// Returns if slice is empty or has only whitespace characters.
        /// </summary>
        public bool IsWhitespace
        {
            get
            {
                for(int i = 0; i < Length; ++i)
                {
                    if (!char.IsWhiteSpace(m_Source[m_StartIndex + i]))
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Returns if this slice has any whitespace characters.
        /// </summary>
        public bool HasWhitespace
        {
            get
            {
                for(int i = 0; i < Length; ++i)
                {
                    if (char.IsWhiteSpace(m_Source[m_StartIndex + i]))
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Returns the 32-bit hash of this string slice.
        /// </summary>
        public StringHash32 Hash32()
        {
            return new StringHash32(StringHashing.StoreHash32(m_Source, m_StartIndex, Length));
        }

        /// <summary>
        /// Returns the 64-bit hash of this string slice.
        /// </summary>
        public StringHash64 Hash64()
        {
            return new StringHash64(StringHashing.StoreHash64(m_Source, m_StartIndex, Length));
        }

        #region Search

        #region Char

        public bool StartsWith(char inItem)
        {
            return Length > 0 && m_Source[m_StartIndex] == inItem;
        }

        public bool EndsWith(char inItem)
        {
            return Length > 0 && m_Source[m_StartIndex + Length - 1] == inItem;
        }

        public bool Contains(char inItem)
        {
            return IndexOf(inItem) >= 0;
        }

        public int IndexOf(char inItem)
        {
            return IndexOf(inItem, 0, Length);
        }

        public int IndexOf(char inItem, int inStartIdx)
        {
            return IndexOf(inItem, inStartIdx, Length - inStartIdx);
        }

        public int IndexOf(char inItem, int inStartIdx, int inCount)
        {
            if (m_Source == null)
                return -1;

            int srcIndex = m_Source.IndexOf(inItem, m_StartIndex + inStartIdx, inCount);
            return srcIndex < 0 ? srcIndex : srcIndex - m_StartIndex;
        }

        #endregion // Char

        #region String

        public bool StartsWith(string inItem)
        {
            if (m_Source == null || inItem == null)
                return false;

            return MatchStart(m_Source, m_StartIndex, Length, inItem, 0, inItem.Length, false);
        }

        public bool StartsWith(string inItem, bool inbIgnoreCase)
        {
            if (m_Source == null || inItem == null)
                return false;

            return MatchStart(m_Source, m_StartIndex, Length, inItem, 0, inItem.Length, inbIgnoreCase);
        }

        public bool EndsWith(string inItem)
        {
            if (m_Source == null || inItem == null)
                return false;

            return MatchEnd(m_Source, m_StartIndex, Length, inItem, 0, inItem.Length, false);
        }

        public bool EndsWith(string inItem, bool inbIgnoreCase)
        {
            if (m_Source == null || inItem == null)
                return false;

            return MatchEnd(m_Source, m_StartIndex, Length, inItem, 0, inItem.Length, inbIgnoreCase);
        }

        #endregion // String

        #endregion // Search

        #region Trim

        public StringBuilderSlice Trim()
        {
            return Trim(TrimWhitespaceChars);
        }

        public StringBuilderSlice Trim(char[] inTrimChars)
        {
            if (inTrimChars == null || inTrimChars.Length == 0)
            {
                inTrimChars = TrimWhitespaceChars;
            }

            return Trim(m_Source, m_StartIndex, Length, inTrimChars, TrimType.Both);
        }

        public StringBuilderSlice TrimStart()
        {
            return TrimStart(TrimWhitespaceChars);
        }

        public StringBuilderSlice TrimStart(char[] inTrimChars)
        {
            if (inTrimChars == null || inTrimChars.Length == 0)
            {
                inTrimChars = TrimWhitespaceChars;
            }

            return Trim(m_Source, m_StartIndex, Length, inTrimChars, TrimType.Start);
        }

        public StringBuilderSlice TrimEnd()
        {
            return TrimEnd(TrimWhitespaceChars);
        }

        public StringBuilderSlice TrimEnd(char[] inTrimChars)
        {
            if (inTrimChars == null || inTrimChars.Length == 0)
            {
                inTrimChars = TrimWhitespaceChars;
            }

            return Trim(m_Source, m_StartIndex, Length, inTrimChars, TrimType.End);
        }

        #endregion // Trim

        #region Substring

        public StringBuilderSlice Substring(int inStartIdx)
        {
            return Substring(inStartIdx, Length - inStartIdx);
        }

        public StringBuilderSlice Substring(int inStartIdx, int inLength)
        {
            return new StringBuilderSlice(m_Source, m_StartIndex + inStartIdx, inLength);
        }

        #endregion // Substring

        #region IReadOnlyList

        public char this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new IndexOutOfRangeException();
                return m_Source[m_StartIndex + index];
            }
        }

        int IReadOnlyCollection<char>.Count { get { return Length; } }

        #endregion // IReadOnlyList

        #region IEnumerable

        public IEnumerator<char> GetEnumerator()
        {
            for (int i = 0; i < Length; ++i)
                yield return m_Source[m_StartIndex + i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion // IEnumerable

        #region IEquatable

        public bool Equals(StringBuilderSlice other)
        {
            return Equals(m_Source, m_StartIndex, Length, other.m_Source, other.m_StartIndex, other.Length, false);
        }

        public bool Equals(StringBuilderSlice other, bool inbIgnoreCase)
        {
            return Equals(m_Source, m_StartIndex, Length, other.m_Source, other.m_StartIndex, other.Length, inbIgnoreCase);
        }

        public bool Equals(string other)
        {
            if (string.IsNullOrEmpty(other))
                return Length == 0;

            return Equals(m_Source, m_StartIndex, Length, other, 0, other.Length, false);
        }

        public bool Equals(string other, bool inbIgnoreCase)
        {
            if (string.IsNullOrEmpty(other))
                return Length == 0;

            return Equals(m_Source, m_StartIndex, Length, other, 0, other.Length, inbIgnoreCase);
        }

        #endregion // IEquatable

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is StringBuilderSlice)
                return Equals((StringBuilderSlice) obj);
            if (obj is string)
                return Equals((string) obj);

            return false;
        }

        public override int GetHashCode()
        {
            return (int) StringHashing.Hash32(m_Source, m_StartIndex, Length);
        }

        public override string ToString()
        {
            if (Length <= 0)
            {
                return string.Empty;
            }
            else if (m_StartIndex == 0 && Length == m_Source.Length)
            {
                return m_Source.ToString();
            }
            else
            {
                return m_Source.ToString(m_StartIndex, Length);
            }
        }

        static public bool operator ==(StringBuilderSlice inA, StringSlice inB)
        {
            return inA.Equals(inB);
        }

        static public bool operator !=(StringBuilderSlice inA, StringSlice inB)
        {
            return !inA.Equals(inB);
        }

        static public bool operator ==(StringBuilderSlice inA, string inB)
        {
            return inA.Equals(inB);
        }

        static public bool operator !=(StringBuilderSlice inA, string inB)
        {
            return !inA.Equals(inB);
        }

        static public bool operator ==(string inA, StringBuilderSlice inB)
        {
            return inB.Equals(inA);
        }

        static public bool operator !=(string inA, StringBuilderSlice inB)
        {
            return !inB.Equals(inA);
        }

        #endregion // Overrides

        #region Internal

        static private bool MatchStart(StringBuilder inString, int inStart, int inLength, string inMatch, int inStartMatch, int inLengthMatch, bool inbIgnoreCase)
        {
            if (inLengthMatch > inLength)
                return false;

            for (int i = 0; i < inLengthMatch; ++i)
            {
                char a = inString[inStart + i];
                char b = inMatch[inStartMatch + i];
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

        static private bool MatchEnd(StringBuilder inString, int inStart, int inLength, string inMatch, int inStartMatch, int inLengthMatch, bool inbIgnoreCase)
        {
            if (inLengthMatch > inLength)
                return false;

            int endA = inStart + inLength - 1;
            int endB = inStartMatch + inLengthMatch - 1;
            for (int i = 0; i < inLengthMatch; ++i)
            {
                char a = inString[endA - i];
                char b = inMatch[endB - i];
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

        static private bool Equals(StringBuilder inA, int inStartA, int inLengthA, StringBuilder inB, int inStartB, int inLengthB, bool inbIgnoreCase)
        {
            if (inLengthA != inLengthB)
                return false;

            for (int i = 0; i < inLengthA; ++i)
            {
                char a = inA[inStartA + i];
                char b = inB[inStartB + i];
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

        static private bool Equals(StringBuilder inA, int inStartA, int inLengthA, string inB, int inStartB, int inLengthB, bool inbIgnoreCase)
        {
            if (inLengthA != inLengthB)
                return false;

            for (int i = 0; i < inLengthA; ++i)
            {
                char a = inA[inStartA + i];
                char b = inB[inStartB + i];
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

        static private StringBuilderSlice Trim(StringBuilder inString, int inStartIdx, int inLength, char[] inTrimChars, TrimType inTrimType)
        {
            if (inString == null)
                return StringBuilderSlice.Empty;

            if (inTrimChars == null || inTrimChars.Length == 0 || inTrimType == TrimType.None)
                return new StringBuilderSlice(inString, inStartIdx, inLength);

            int trimCount = inTrimChars.Length;

            int startIdx = inStartIdx;
            int endIdx = startIdx + inLength - 1;

            if ((inTrimType & TrimType.Start) == TrimType.Start)
            {
                while (startIdx <= endIdx)
                {
                    int i = 0;
                    char c = inString[startIdx];
                    for (i = 0; i < trimCount; ++i)
                    {
                        if (c == inTrimChars[i])
                            break;
                    }
                    if (i == trimCount)
                        break;

                    ++startIdx;
                }
            }

            if ((inTrimType & TrimType.End) == TrimType.End)
            {
                while (endIdx >= startIdx)
                {
                    int i = 0;
                    char c = inString[endIdx];
                    for (i = 0; i < trimCount; ++i)
                    {
                        if (c == inTrimChars[i])
                            break;
                    }
                    if (i == trimCount)
                        break;

                    --endIdx;
                }
            }

            if (startIdx > endIdx)
                return StringBuilderSlice.Empty;

            int newLength = endIdx - startIdx + 1;
            if (newLength == inLength)
                return new StringBuilderSlice(inString, inStartIdx, inLength);

            return new StringBuilderSlice(inString, startIdx, newLength);
        }

        [Flags]
        private enum TrimType
        {
            None = 0,
            Start = 0x01,
            End = 0x02,
            Both = Start | End
        }

        // Taken from String.WhitespaceChars
        static private readonly char[] TrimWhitespaceChars = new char[]
        {
            (char) 0x9, (char) 0xA, (char) 0xB, (char) 0xC, (char) 0xD, (char) 0x20, (char) 0x85,
            (char) 0xA0, (char) 0x1680,
            (char) 0x2000, (char) 0x2001, (char) 0x2002, (char) 0x2003, (char) 0x2004, (char) 0x2005,
            (char) 0x2006, (char) 0x2007, (char) 0x2008, (char) 0x2009, (char) 0x200A, (char) 0x200B,
            (char) 0x2028, (char) 0x2029,
            (char) 0x3000, (char) 0xFEFF
        };

        #endregion // Internal
    }
}