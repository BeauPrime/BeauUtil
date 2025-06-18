/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 June 2022
 * 
 * File:    StringBuilderSlice.cs
 * Purpose: Read-only slice of a StringBuilder.
 */

#if NETSTANDARD || NET_STANDARD
#define SUPPORTS_SPAN
#endif // NETSTANDARD || NET_STANDARD

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.IL2CPP.CompilerServices;

using static BeauUtil.StringSlice;

namespace BeauUtil
{
    /// <summary>
    /// A section of a StringBuilder.
    /// </summary>
    public readonly struct StringBuilderSlice : IEnumerable<char>, IReadOnlyList<char>, IEquatable<StringBuilderSlice>, IEquatable<string>
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringHash32 Hash32()
        {
            return new StringHash32(StringHashing.StoreHash32(m_Source, m_StartIndex, Length));
        }

        /// <summary>
        /// Returns the 64-bit hash of this string slice.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringHash64 Hash64()
        {
            return new StringHash64(StringHashing.StoreHash64(m_Source, m_StartIndex, Length));
        }

        /// <summary>
        /// Unpacks StringBuilderSlice parameters.
        /// </summary>
        public void Unpack(out StringBuilder outString, out int outOffset, out int outLength)
        {
            outString = m_Source;
            outOffset = m_StartIndex;
            outLength = Length;
        }

        #region Search

        #region Char

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool StartsWith(char inItem)
        {
            return Length > 0 && m_Source[m_StartIndex] == inItem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EndsWith(char inItem)
        {
            return Length > 0 && m_Source[m_StartIndex + Length - 1] == inItem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(char inItem)
        {
            return IndexOf(inItem) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char inItem)
        {
            return IndexOf(inItem, 0, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char inItem, int inStartIdx)
        {
            return IndexOf(inItem, inStartIdx, Length - inStartIdx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char inItem, int inStartIdx, int inCount)
        {
            if (m_Source == null)
                return -1;

            int srcIndex = m_Source.IndexOf(inItem, m_StartIndex + inStartIdx, inCount);
            return srcIndex < 0 ? srcIndex : srcIndex - m_StartIndex;
        }

        #endregion // Char

        #region String

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool StartsWith(string inItem)
        {
            return StartsWith(inItem, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool StartsWith(string inItem, bool inbIgnoreCase)
        {
            if (m_Source == null || inItem == null)
                return false;

            return StringUtils.StartsWith(m_Source, m_StartIndex, Length, inItem, 0, inItem.Length, inbIgnoreCase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EndsWith(string inItem)
        {
            return EndsWith(inItem, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EndsWith(string inItem, bool inbIgnoreCase)
        {
            if (m_Source == null || inItem == null)
                return false;

            return StringUtils.EndsWith(m_Source, m_StartIndex, Length, inItem, 0, inItem.Length, inbIgnoreCase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(string inItem)
        {
            return Contains(inItem, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(string inItem, bool inbIgnoreCase)
        {
            return Length >= inItem.Length && StringUtils.IndexOf(m_Source, inItem, m_StartIndex, Length, inbIgnoreCase) >= 0;
        }

        #endregion // String

        #endregion // Search

        #region Trim

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilderSlice Trim()
        {
            return Trim(TrimWhitespaceChars);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilderSlice Trim(char[] inTrimChars)
        {
            if (inTrimChars == null || inTrimChars.Length == 0)
            {
                inTrimChars = TrimWhitespaceChars;
            }

            return Trim(m_Source, m_StartIndex, Length, inTrimChars, TrimType.Both);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilderSlice TrimStart()
        {
            return TrimStart(TrimWhitespaceChars);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilderSlice TrimStart(char[] inTrimChars)
        {
            if (inTrimChars == null || inTrimChars.Length == 0)
            {
                inTrimChars = TrimWhitespaceChars;
            }

            return Trim(m_Source, m_StartIndex, Length, inTrimChars, TrimType.Start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilderSlice TrimEnd()
        {
            return TrimEnd(TrimWhitespaceChars);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilderSlice Substring(int inStartIdx)
        {
            return Substring(inStartIdx, Length - inStartIdx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilderSlice Substring(int inStartIdx, int inLength)
        {
            return new StringBuilderSlice(m_Source, m_StartIndex + inStartIdx, inLength);
        }

        #endregion // Substring

        #region Export

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(char[] inArray, int inArrayIdx)
        {
            CopyTo(0, inArray, inArrayIdx, Length);
        }

        public void CopyTo(int inStartIndex, char[] inArray, int inArrayIdx, int inCount)
        {
            if (m_Source == null || inCount <= 0)
                return;
            if (inArray.Length < inCount)
                throw new ArgumentException("Not enough room to copy " + inCount + " items to destination");
            if (inStartIndex + inCount > Length)
                throw new ArgumentException("Attempting to copy data outside StringSlice range");

            m_Source.CopyTo(m_StartIndex + inStartIndex, inArray, inArrayIdx, inCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(char[] inArray)
        {
            CopyTo(0, inArray, 0, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void CopyTo(char* inBuffer, int inBufferLength)
        {
            CopyTo(0, inBuffer, inBufferLength, Length);
        }

        public unsafe void CopyTo(int inStartIndex, char* inBuffer, int inArrayLength, int inCount)
        {
            if (m_Source == null || inCount <= 0)
                return;
            if (inArrayLength < inCount)
                throw new ArgumentException("Not enough room to copy " + inCount + " items to destination");
            if (inStartIndex + inCount > Length)
                throw new ArgumentException("Attempting to copy data outside StringSlice range");

            unsafe
            {
#if SUPPORTS_SPAN
                Span<char> data = new Span<char>(inBuffer, inArrayLength);
                m_Source.CopyTo(m_StartIndex + inStartIndex, data, inCount);
#else
                for(int i = 0; i < inCount; i++)
                {
                    inBuffer[i] = m_Source[m_StartIndex + inStartIndex + i];
                }
#endif // SUPPORTS_SPAN
            }
        }

        public char[] ToCharArray()
        {
            if (Length > 0)
            {
                char[] data = new char[Length];
                m_Source.CopyTo(m_StartIndex, data, 0, Length);
                return data;
            }
            return Array.Empty<char>();
        }

#endregion // Export

        #region IReadOnlyList

        public char this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [Il2CppSetOption(Option.NullChecks, false)]
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
            return StringUtils.Equals(m_Source, m_StartIndex, Length, other.m_Source, other.m_StartIndex, other.Length, false);
        }

        public bool Equals(StringBuilderSlice other, bool inbIgnoreCase)
        {
            return StringUtils.Equals(m_Source, m_StartIndex, Length, other.m_Source, other.m_StartIndex, other.Length, inbIgnoreCase);
        }

        public bool Equals(string other)
        {
            if (string.IsNullOrEmpty(other))
                return Length == 0;

            return StringUtils.Equals(m_Source, m_StartIndex, Length, other, 0, other.Length, false);
        }

        public bool Equals(string other, bool inbIgnoreCase)
        {
            if (string.IsNullOrEmpty(other))
                return Length == 0;

            return StringUtils.Equals(m_Source, m_StartIndex, Length, other, 0, other.Length, inbIgnoreCase);
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

        public void AppendTo(StringBuilder ioBuilder)
        {
            if (Length <= 0)
                return;

            unsafe
            {
                ioBuilder.Reserve(Length);
                char* copy = stackalloc char[Length];
                for(int i = 0; i < Length; i++)
                {
                    copy[i] = m_Source[m_StartIndex + i];
                }
                ioBuilder.Append(copy, Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint CalculateHash32()
        {
            return StringHashing.StoreHash32(m_Source, m_StartIndex, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint CalculateHash32NoCache()
        {
            return StringHashing.Hash32(m_Source, m_StartIndex, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint CalculateHash32CaseInsensitive()
        {
            return StringHashing.StoreHash32CaseInsensitive(m_Source, m_StartIndex, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint CalculateHash32CaseInsensitiveNoCache()
        {
            return StringHashing.Hash32CaseInsensitive(m_Source, m_StartIndex, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ulong CalculateHash64()
        {
            return StringHashing.StoreHash64(m_Source, m_StartIndex, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ulong CalculateHash64NoCache()
        {
            return StringHashing.Hash64(m_Source, m_StartIndex, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ulong CalculateHash64CaseInsensitive()
        {
            return StringHashing.StoreHash64CaseInsensitive(m_Source, m_StartIndex, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ulong CalculateHash64CaseInsensitiveNoCache()
        {
            return StringHashing.Hash64CaseInsensitive(m_Source, m_StartIndex, Length);
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

        #endregion // Internal
    }
}