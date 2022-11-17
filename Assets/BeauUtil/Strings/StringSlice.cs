/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 Oct 2019
 * 
 * File:    StringSlice.cs
 * Purpose: Read-only slice of a string.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BeauUtil
{
    /// <summary>
    /// A section of a string.
    /// </summary>
    public struct StringSlice : IEnumerable<char>, IReadOnlyList<char>, IEquatable<StringSlice>, IEquatable<string>, IConvertible, IComparable<StringSlice>
    {
        private readonly string m_Source;
        private readonly int m_StartIndex;

        /// <summary>
        /// Total length of the slice.
        /// </summary>
        public readonly int Length;

        public StringSlice(string inString)
        {
            m_Source = inString;
            m_StartIndex = 0;
            Length = inString != null ? inString.Length : 0; 
        }

        public StringSlice(string inString, int inStartIdx) : this(inString, inStartIdx, inString != null ? inString.Length - inStartIdx : 0) { }

        public StringSlice(string inString, int inStartIdx, int inLength)
        {
            if (string.IsNullOrEmpty(inString) || inLength <= 0)
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
        static public readonly StringSlice Empty = default(StringSlice);

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
            return new StringHash32(CalculateHash32());
        }

        /// <summary>
        /// Returns the 64-bit hash of this string slice.
        /// </summary>
        public StringHash64 Hash64()
        {
            return new StringHash64(CalculateHash64());
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

        public int IndexOfAny(char[] inItems)
        {
            return IndexOfAny(inItems, 0, Length);
        }

        public int IndexOfAny(char[] inItems, int inStartIdx)
        {
            return IndexOfAny(inItems, inStartIdx, Length - inStartIdx);
        }

        public int IndexOfAny(char[] inItems, int inStartIdx, int inCount)
        {
            if (m_Source == null)
                return -1;

            int srcIndex = m_Source.IndexOfAny(inItems, m_StartIndex + inStartIdx, inCount);
            return srcIndex < 0 ? srcIndex : srcIndex - m_StartIndex;
        }

        public int LastIndexOf(char inItem)
        {
            return LastIndexOf(inItem, Length - 1, Length);
        }

        public int LastIndexOf(char inItem, int inStartIdx)
        {
            return LastIndexOf(inItem, inStartIdx, inStartIdx + 1);
        }

        public int LastIndexOf(char inItem, int inStartIdx, int inCount)
        {
            if (m_Source == null)
                return -1;

            int srcIndex = m_Source.LastIndexOf(inItem, m_StartIndex + inStartIdx, inCount);
            return srcIndex < 0 ? srcIndex : srcIndex - m_StartIndex;
        }

        public int LastIndexOfAny(char[] inItems)
        {
            return LastIndexOfAny(inItems, Length - 1, Length);
        }

        public int LastIndexOfAny(char[] inItems, int inStartIdx)
        {
            return LastIndexOfAny(inItems, inStartIdx, inStartIdx + 1);
        }

        public int LastIndexOfAny(char[] inItems, int inStartIdx, int inCount)
        {
            if (m_Source == null)
                return -1;

            int srcIndex = m_Source.LastIndexOfAny(inItems, m_StartIndex + inStartIdx, inCount);
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

        public bool StartsWith(StringSlice inItem)
        {
            if (m_Source == null || inItem.m_Source == null)
                return false;

            return MatchStart(m_Source, m_StartIndex, Length, inItem.m_Source, inItem.m_StartIndex, inItem.Length, false);
        }

        public bool StartsWith(StringSlice inItem, bool inbIgnoreCase)
        {
            if (m_Source == null || inItem.m_Source == null)
                return false;

            return MatchStart(m_Source, m_StartIndex, Length, inItem.m_Source, inItem.m_StartIndex, inItem.Length, inbIgnoreCase);
        }

        public bool Contains(string inItem)
        {
            return IndexOf(inItem) >= 0;
        }

        public bool Contains(string inItem, bool inbIgnoreCase)
        {
            return IndexOf(inItem, inbIgnoreCase) >= 0;
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

        public bool EndsWith(StringSlice inItem)
        {
            if (m_Source == null || inItem.m_Source == null)
                return false;

            return MatchEnd(m_Source, m_StartIndex, Length, inItem.m_Source, inItem.m_StartIndex, inItem.Length, false);
        }

        public bool EndsWith(StringSlice inItem, bool inbIgnoreCase)
        {
            if (m_Source == null || inItem.m_Source == null)
                return false;

            return MatchEnd(m_Source, m_StartIndex, Length, inItem.m_Source, inItem.m_StartIndex, inItem.Length, inbIgnoreCase);
        }

        public int IndexOf(string inItem)
        {
            return IndexOf(inItem, 0, Length);
        }

        public int IndexOf(string inItem, bool inbIgnoreCase)
        {
            return IndexOf(inItem, 0, Length, inbIgnoreCase);
        }

        public int IndexOf(string inItem, int inStartIdx)
        {
            return IndexOf(inItem, inStartIdx, Length - inStartIdx);
        }

        public int IndexOf(string inItem, int inStartIdx, bool inbIgnoreCase)
        {
            return IndexOf(inItem, inStartIdx, Length - inStartIdx, inbIgnoreCase);
        }

        public int IndexOf(string inItem, int inStartIdx, int inCount)
        {
            if (m_Source == null || inItem == null)
                return -1;

            int srcIndex = m_Source.IndexOf(inItem, m_StartIndex + inStartIdx, inCount, StringComparison.Ordinal);
            return srcIndex < 0 ? srcIndex : srcIndex - m_StartIndex;
        }

        public int IndexOf(string inItem, int inStartIdx, int inCount, bool inbIgnoreCase)
        {
            if (m_Source == null || inItem == null)
                return -1;

            int srcIndex = m_Source.IndexOf(inItem, m_StartIndex + inStartIdx, inCount, inbIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            return srcIndex < 0 ? srcIndex : srcIndex - m_StartIndex;
        }

        public int LastIndexOf(string inItem)
        {
            return LastIndexOf(inItem, Length - 1, Length);
        }

        public int LastIndexOf(string inItem, bool inbIgnoreCase)
        {
            return LastIndexOf(inItem, Length - 1, Length, inbIgnoreCase);
        }

        public int LastIndexOf(string inItem, int inStartIdx)
        {
            return LastIndexOf(inItem, inStartIdx, inStartIdx + 1);
        }

        public int LastIndexOf(string inItem, int inStartIdx, bool inbIgnoreCase)
        {
            return LastIndexOf(inItem, inStartIdx, inStartIdx + 1, inbIgnoreCase);
        }

        public int LastIndexOf(string inItem, int inStartIdx, int inCount)
        {
            if (m_Source == null || inItem == null)
                return -1;

            int srcIndex = m_Source.LastIndexOf(inItem, m_StartIndex + inStartIdx, inCount, StringComparison.Ordinal);
            return srcIndex < 0 ? srcIndex : srcIndex - m_StartIndex;
        }

        public int LastIndexOf(string inItem, int inStartIdx, int inCount, bool inbIgnoreCase)
        {
            if (m_Source == null || inItem == null)
                return -1;

            int srcIndex = m_Source.LastIndexOf(inItem, m_StartIndex + inStartIdx, inCount, inbIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            return srcIndex < 0 ? srcIndex : srcIndex - m_StartIndex;
        }

        #endregion // String

        #endregion // Search

        #region Trim

        public StringSlice Trim()
        {
            return Trim(TrimWhitespaceChars);
        }

        public StringSlice Trim(char[] inTrimChars)
        {
            if (inTrimChars == null || inTrimChars.Length == 0)
            {
                inTrimChars = TrimWhitespaceChars;
            }

            return Trim(m_Source, m_StartIndex, Length, inTrimChars, TrimType.Both);
        }

        public StringSlice TrimStart()
        {
            return TrimStart(TrimWhitespaceChars);
        }

        public StringSlice TrimStart(char[] inTrimChars)
        {
            if (inTrimChars == null || inTrimChars.Length == 0)
            {
                inTrimChars = TrimWhitespaceChars;
            }

            return Trim(m_Source, m_StartIndex, Length, inTrimChars, TrimType.Start);
        }

        public StringSlice TrimEnd()
        {
            return TrimEnd(TrimWhitespaceChars);
        }

        public StringSlice TrimEnd(char[] inTrimChars)
        {
            if (inTrimChars == null || inTrimChars.Length == 0)
            {
                inTrimChars = TrimWhitespaceChars;
            }

            return Trim(m_Source, m_StartIndex, Length, inTrimChars, TrimType.End);
        }

        #endregion // Trim

        #region Substring

        public StringSlice Substring(int inStartIdx)
        {
            return Substring(inStartIdx, Length - inStartIdx);
        }

        public StringSlice Substring(int inStartIdx, int inLength)
        {
            return new StringSlice(m_Source, m_StartIndex + inStartIdx, inLength);
        }

        #endregion // Substring

        #region Split

        /// <summary>
        /// Interface for custom splitting logic.
        /// </summary>
        public interface ISplitter
        {
            /// <summary>
            /// Evaluates whether or not to split on a given character.
            /// </summary>
            /// <param name="inString">String to evaluate.</param>
            /// <param name="inIndex">Current evaluation index.</param>
            /// <param name="inSliceCount">Number of slices currently found.</param>
            /// <param name="outAdvance">Output for any additional characters to advance.</param>
            bool Evaluate(string inString, int inIndex, int inSliceCount, ref uint ioState, out int outAdvance);

            /// <summary>
            /// Post-process for a given slice. Useful for trimming and unescaping.
            /// </summary>
            StringSlice Process(StringSlice inSlice, uint ioState);
        }

        #region Slice

        public StringSlice[] Split(char[] inSeparator, StringSliceOptions inSplitOptions)
        {
            return Split(m_Source, m_StartIndex, Length, inSeparator, inSplitOptions);
        }

        public StringSlice[] Split(ISplitter inSplitter, StringSliceOptions inSplitOptions)
        {
            return Split(m_Source, m_StartIndex, Length, inSplitter, inSplitOptions);
        }

        public int Split(char[] inSeparator, StringSliceOptions inSplitOptions, IList<StringSlice> outSlices)
        {
            return Split(m_Source, m_StartIndex, Length, inSeparator, inSplitOptions, outSlices);
        }

        public int Split(ISplitter inSplitter, StringSliceOptions inSplitOptions, IList<StringSlice> outSlices)
        {
            return Split(m_Source, m_StartIndex, Length, inSplitter, inSplitOptions, outSlices);
        }

        public int Split<T>(char[] inSeparator, StringSliceOptions inSplitOptions, ref T outSlices) where T : ITempList<StringSlice>
        {
            return Split(m_Source, m_StartIndex, Length, inSeparator, inSplitOptions, ref outSlices);
        }

        public int Split<T>(ISplitter inSplitter, StringSliceOptions inSplitOptions, ref T outSlices) where T : ITempList<StringSlice>
        {
            return Split(m_Source, m_StartIndex, Length, inSplitter, inSplitOptions, ref outSlices);
        }

        public IEnumerable<StringSlice> EnumeratedSplit(char[] inSeparator, StringSliceOptions inSplitOptions)
        {
            return EnumerableSplit(m_Source, m_StartIndex, Length, inSeparator, inSplitOptions);
        }

        public IEnumerable<StringSlice> EnumeratedSplit(ISplitter inSplitter, StringSliceOptions inSplitOptions)
        {
            return EnumerableSplit(m_Source, m_StartIndex, Length, inSplitter, inSplitOptions);
        }

        #endregion // Slice

        #region Static

        static public StringSlice[] Split(string inString, char[] inSeparator, StringSliceOptions inSplitOptions)
        {
            return Split(inString, 0, inString.Length, inSeparator, inSplitOptions);
        }

        static public StringSlice[] Split(string inString, ISplitter inSplitter, StringSliceOptions inSplitOptions)
        {
            return Split(inString, 0, inString.Length, inSplitter, inSplitOptions);
        }

        static public int Split(string inString, char[] inSeparator, StringSliceOptions inSplitOptions, IList<StringSlice> outSlices)
        {
            return Split(inString, 0, inString.Length, inSeparator, inSplitOptions, outSlices);
        }

        static public int Split(string inString, ISplitter inSplitter, StringSliceOptions inSplitOptions, IList<StringSlice> outSlices)
        {
            return Split(inString, 0, inString.Length, inSplitter, inSplitOptions, outSlices);
        }

        static public int Split<T>(string inString, char[] inSeparator, StringSliceOptions inSplitOptions, ref T outSlices) where T : ITempList<StringSlice>
        {
            return Split(inString, 0, inString.Length, inSeparator, inSplitOptions, ref outSlices);
        }

        static public int Split<T>(string inString, ISplitter inSplitter, StringSliceOptions inSplitOptions, ref T outSlices) where T : ITempList<StringSlice>
        {
            return Split(inString, 0, inString.Length, inSplitter, inSplitOptions, ref outSlices);
        }

        static public IEnumerable<StringSlice> EnumeratedSplit(string inString, char[] inSeparator, StringSliceOptions inSplitOptions)
        {
            return EnumerableSplit(inString, 0, inString.Length, inSeparator, inSplitOptions);
        }

        static public IEnumerable<StringSlice> EnumeratedSplit(string inString, ISplitter inSplitter, StringSliceOptions inSplitOptions)
        {
            return EnumerableSplit(inString, 0, inString.Length, inSplitter, inSplitOptions);
        }

        #endregion // Static

        #endregion // Split

        #region Export

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

            unsafe
            {
                fixed(char* src = m_Source)
                {
                    Unsafe.CopyArray(src + m_StartIndex + inStartIndex, inCount, inArray, inArrayIdx);
                }
            }
        }

        public void CopyTo(char[] inArray)
        {
            CopyTo(0, inArray, 0, Length);
        }

        public char[] ToCharArray()
        {
            if (Length > 0)
            {
                return m_Source.ToCharArray(m_StartIndex, Length);
            }
            return Array.Empty<char>();
        }

        public string Escape()
        {
            return StringUtils.Escape(m_Source, m_StartIndex, Length);
        }

        public string Escape(StringUtils.ICustomEscapeEvaluator inCustomEscape)
        {
            return StringUtils.Escape(m_Source, m_StartIndex, Length, inCustomEscape);
        }

        public void Escape(StringBuilder ioBuilder)
        {
            StringUtils.Escape(m_Source, m_StartIndex, Length, ioBuilder);
        }

        public void Escape(StringBuilder ioBuilder, StringUtils.ICustomEscapeEvaluator inCustomEscape)
        {
            StringUtils.Escape(m_Source, m_StartIndex, Length, ioBuilder, inCustomEscape);
        }

        public string Unescape()
        {
            return StringUtils.Unescape(m_Source, m_StartIndex, Length);
        }

        public string Unescape(StringUtils.ICustomEscapeEvaluator inCustomEscape)
        {
            return StringUtils.Unescape(m_Source, m_StartIndex, Length, inCustomEscape);
        }

        public void Unescape(StringBuilder ioBuilder)
        {
            StringUtils.Unescape(m_Source, m_StartIndex, Length, ioBuilder);
        }

        public void Unescape(StringBuilder ioBuilder, StringUtils.ICustomEscapeEvaluator inCustomUnescape)
        {
            StringUtils.Unescape(m_Source, m_StartIndex, Length, ioBuilder, inCustomUnescape);
        }

        /// <summary>
        /// Unpacks StringSlice parameters.
        /// </summary>
        public void Unpack(out string outString, out int outOffset, out int outLength)
        {
            outString = m_Source;
            outOffset = m_StartIndex;
            outLength = Length;
        }

        #endregion // Export

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

        public bool Equals(StringSlice other)
        {
            return Equals(m_Source, m_StartIndex, Length, other.m_Source, other.m_StartIndex, other.Length, false);
        }

        public bool Equals(StringSlice other, bool inbIgnoreCase)
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

        #region IConvertible

        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Object;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            bool b;
            if (!StringParser.TryParseBool(this, out b))
                throw new FormatException();
            return b;
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            byte b;
            if (!StringParser.TryParseByte(this, out b))
                throw new FormatException();
            return b;
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            if (Length == 0)
                throw new FormatException();
            return this[0];
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(ToString());
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            double d;
            if (!StringParser.TryParseDouble(this, out d))
                throw new FormatException();
            return (decimal) d;
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            double d;
            if (!StringParser.TryParseDouble(this, out d))
                throw new FormatException();
            return d;
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            short s;
            if (!StringParser.TryParseShort(this, out s))
                throw new FormatException();
            return s;
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            int i;
            if (!StringParser.TryParseInt(this, out i))
                throw new FormatException();
            return i;
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            long l;
            if (!StringParser.TryParseLong(this, out l))
                throw new FormatException();
            return l;
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            sbyte s;
            if (!StringParser.TryParseSByte(this, out s))
                throw new FormatException();
            return s;
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            float f;
            if (!StringParser.TryParseFloat(this, out f))
                throw new FormatException();
            return f;
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return ToString();
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            object o;
            if (!StringParser.TryConvertTo(this, conversionType, out o))
                throw new FormatException();
            return o;
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            ushort u;
            if (!StringParser.TryParseUShort(this, out u))
                throw new FormatException();
            return u;
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            uint u;
            if (!StringParser.TryParseUInt(this, out u))
                throw new FormatException();
            return u;
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            ulong u;
            if (!StringParser.TryParseULong(this, out u))
                throw new FormatException();
            return u;
        }

        #endregion // IConvertible

        #region IComparable

        public int CompareTo(StringSlice other)
        {
            if (Length == 0)
            {
                if (other.Length == 0)
                {
                    return 0;
                }

                return -1;
            }
            if (other.Length == 0)
            {
                return 1;
            }

            int minLength = Math.Min(Length, other.Length);
            int baseCompare = string.CompareOrdinal(m_Source, m_StartIndex, other.m_Source, other.m_StartIndex, minLength);
            if (baseCompare != 0)
            {
                return baseCompare;
            }

            int lengthCompare = Length - other.Length;

            if (lengthCompare < 0)
                return -1;
            if (lengthCompare > 0)
                return 1;
            
            return 0;
        }

        #endregion // IComparable

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is StringSlice)
                return Equals((StringSlice) obj);
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
                return m_Source;
            }
            else
            {
                return m_Source.Substring(m_StartIndex, Length);
            }
        }

        static public bool operator ==(StringSlice inA, StringSlice inB)
        {
            return inA.Equals(inB);
        }

        static public bool operator !=(StringSlice inA, StringSlice inB)
        {
            return !inA.Equals(inB);
        }

        static public bool operator ==(StringSlice inA, string inB)
        {
            return inA.Equals(inB);
        }

        static public bool operator !=(StringSlice inA, string inB)
        {
            return !inA.Equals(inB);
        }

        static public bool operator ==(string inA, StringSlice inB)
        {
            return inB.Equals(inA);
        }

        static public bool operator !=(string inA, StringSlice inB)
        {
            return !inB.Equals(inA);
        }

        static public implicit operator StringSlice(string inString)
        {
            return new StringSlice(inString);
        }

        #endregion // Overrides

        #region Internal

        public void AppendTo(StringBuilder ioBuilder)
        {
            if (Length <= 0)
                return;
            
            ioBuilder.Append(m_Source, m_StartIndex, Length);
        }

        internal uint CalculateHash32()
        {
            return StringHashing.StoreHash32(m_Source, m_StartIndex, Length);
        }

        internal uint CalculateHash32NoCache()
        {
            return StringHashing.Hash32(m_Source, m_StartIndex, Length);
        }

        internal uint CalculateHash32CaseInsensitive()
        {
            return StringHashing.StoreHash32CaseInsensitive(m_Source, m_StartIndex, Length);
        }

        internal uint CalculateHash32CaseInsensitiveNoCache()
        {
            return StringHashing.Hash32CaseInsensitive(m_Source, m_StartIndex, Length);
        }

        internal uint AppendHash32(uint inHash, bool inbReverseLookup)
        {
            return StringHashing.AppendHash32(inHash, m_Source, m_StartIndex, Length, inbReverseLookup);
        }

        internal ulong CalculateHash64()
        {
            return StringHashing.StoreHash64(m_Source, m_StartIndex, Length);
        }

        internal ulong CalculateHash64NoCache()
        {
            return StringHashing.Hash64(m_Source, m_StartIndex, Length);
        }

        internal ulong CalculateHash64CaseInsensitive()
        {
            return StringHashing.StoreHash64CaseInsensitive(m_Source, m_StartIndex, Length);
        }

        internal ulong CalculateHash64CaseInsensitiveNoCache()
        {
            return StringHashing.Hash64CaseInsensitive(m_Source, m_StartIndex, Length);
        }

        internal ulong AppendHash64(ulong inHash, bool inbReverseLookup)
        {
            return StringHashing.AppendHash64(inHash, m_Source, m_StartIndex, Length, inbReverseLookup);
        }

        static unsafe private bool MatchStart(string inString, int inStart, int inLength, string inMatch, int inStartMatch, int inLengthMatch, bool inbIgnoreCase)
        {
            if (inLengthMatch > inLength || inStart + inLength > inString.Length || inStartMatch + inLengthMatch > inMatch.Length)
                return false;

            fixed(char* str = inString)
            fixed(char* match = inMatch)
            {
                char* a = str + inStart;
                char* b = match + inStartMatch;
                char* end = b + inLengthMatch;

                if (inbIgnoreCase)
                {
                    while(b != end)
                    {
                        if (StringUtils.ToUpperInvariant(*a++) != StringUtils.ToUpperInvariant(*b++))
                            return false;
                    }
                }
                else
                {
                    while(b != end)
                    {
                        if (*a++ != *b++)
                            return false;
                    }
                }
            }

            return true;
        }

        static private unsafe bool MatchEnd(string inString, int inStart, int inLength, string inMatch, int inStartMatch, int inLengthMatch, bool inbIgnoreCase)
        {
            if (inLengthMatch > inLength || inStart + inLength > inString.Length || inStartMatch + inLengthMatch > inMatch.Length)
                return false;

            fixed(char* str = inString)
            fixed(char* match = inMatch)
            {
                char* a = str + inStart + inLength - inLengthMatch;
                char* b = match + inStartMatch;
                char* end = b + inLengthMatch;

                if (inbIgnoreCase)
                {
                    while(b != end)
                    {
                        if (StringUtils.ToUpperInvariant(*a++) != StringUtils.ToUpperInvariant(*b++))
                            return false;
                    }
                }
                else
                {
                    while(b != end)
                    {
                        if (*a++ != *b++)
                            return false;
                    }
                }
            }

            return true;
        }

        static private bool Equals(string inA, int inStartA, int inLengthA, string inB, int inStartB, int inLengthB, bool inbIgnoreCase)
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
                    if (StringUtils.ToUpperInvariant(a) != StringUtils.ToUpperInvariant(b))
                        return false;
                }
            }

            return true;
        }

        static private StringSlice[] Split(string inString, int inStartIdx, int inLength, char[] inSeparator, StringSliceOptions inOptions)
        {
            List<StringSlice> slices = new List<StringSlice>();
            Split(inString, inStartIdx, inLength, inSeparator, inOptions, slices);
            return slices.ToArray();
        }

        static private StringSlice[] Split(string inString, int inStartIdx, int inLength, ISplitter inSplitter, StringSliceOptions inOptions)
        {
            List<StringSlice> slices = new List<StringSlice>();
            Split(inString, inStartIdx, inLength, inSplitter, inOptions, slices);
            return slices.ToArray();
        }

        static private int Split(string inString, int inStartIdx, int inLength, char[] inSeparators, StringSliceOptions inOptions, IList<StringSlice> outSlices)
        {
            if (inString == null || inOptions.MaxSlices <= 0)
                return 0;

            bool bRemoveEmpty = (inOptions.Split & StringSplitOptions.RemoveEmptyEntries) != 0;

            if (inSeparators == null || inSeparators.Length == 0 || inOptions.MaxSlices == 1)
            {
                if (!bRemoveEmpty || inLength > 0)
                {
                    outSlices.Add(new StringSlice(inString, inStartIdx, inLength));
                    return 1;
                }

                return 0;
            }

            int sepCount = inSeparators.Length;

            int startIdx = inStartIdx;
            int currentLength = 0;
            int slices = 0;

            for (int charIdx = 0; charIdx < inLength; ++charIdx)
            {
                int realIdx = inStartIdx + charIdx;
                char c = inString[realIdx];
                bool bSplit = false;
                for (int sepIdx = 0; !bSplit && sepIdx < sepCount; ++sepIdx)
                {
                    bSplit = c == inSeparators[sepIdx];
                }

                if (bSplit)
                {
                    if (!bRemoveEmpty || currentLength > 0)
                    {
                        StringSlice slice = new StringSlice(inString, startIdx, currentLength);
                        outSlices.Add(slice);
                        ++slices;
                    }

                    startIdx = realIdx + 1;

                    if (slices >= inOptions.MaxSlices - 1)
                    {
                        currentLength = inLength - charIdx - 1;
                        break;
                    }

                    currentLength = 0;
                }
                else
                {
                    ++currentLength;
                }
            }

            if (currentLength > 0)
            {
                StringSlice slice = new StringSlice(inString, startIdx, currentLength);
                outSlices.Add(slice);
                ++slices;
            }

            return slices;
        }

        static private int Split(string inString, int inStartIdx, int inLength, ISplitter inSplitter, StringSliceOptions inOptions, IList<StringSlice> outSlices)
        {
            if (inString == null || inOptions.MaxSlices <= 0)
                return 0;

            bool bRemoveEmpty = (inOptions.Split & StringSplitOptions.RemoveEmptyEntries) != 0;

            if (inSplitter == null)
            {
                if (!bRemoveEmpty || inLength > 0)
                {
                    outSlices.Add(new StringSlice(inString, inStartIdx, inLength));
                    return 1;
                }

                return 0;
            }

            if (inOptions.MaxSlices == 1)
            {
                StringSlice slice = inSplitter.Process(new StringSlice(inString, inStartIdx, inLength), 0);
                if (!bRemoveEmpty || inLength > 0)
                {
                    outSlices.Add(slice);
                    return 1;
                }

                return 0;
            }

            uint splitterState = 0;

            int startIdx = inStartIdx;
            int currentLength = 0;
            int slices = 0;

            for (int charIdx = 0; charIdx < inLength; ++charIdx)
            {
                int realIdx = inStartIdx + charIdx;

                int evalAdvance;
                bool bSplit = inSplitter.Evaluate(inString, realIdx, slices, ref splitterState, out evalAdvance);

                charIdx += evalAdvance;
                currentLength += evalAdvance;

                if (bSplit)
                {
                    if (!bRemoveEmpty || currentLength > 0)
                    {
                        StringSlice slice = new StringSlice(inString, startIdx, currentLength);
                        slice = inSplitter.Process(slice, splitterState);
                        if (!bRemoveEmpty || slice.Length > 0)
                        {
                            outSlices.Add(slice);
                            ++slices;
                        }
                    }

                    startIdx = realIdx + 1 + evalAdvance;

                    if (slices >= inOptions.MaxSlices - 1)
                    {
                        currentLength = inLength - charIdx - 1;
                        break;
                    }

                    currentLength = 0;
                }
                else
                {
                    ++currentLength;
                }
            }

            if (currentLength > 0)
            {
                StringSlice slice = new StringSlice(inString, startIdx, currentLength);
                slice = inSplitter.Process(slice, splitterState);
                if (!bRemoveEmpty || slice.Length > 0)
                {
                    outSlices.Add(slice);
                    ++slices;
                }
            }

            return slices;
        }

        static private int Split<T>(string inString, int inStartIdx, int inLength, char[] inSeparators, StringSliceOptions inOptions, ref T outSlices) where T : ITempList<StringSlice>
        {
            if (inString == null || inOptions.MaxSlices <= 0)
                return 0;

            bool bRemoveEmpty = (inOptions.Split & StringSplitOptions.RemoveEmptyEntries) != 0;

            if (inSeparators == null || inSeparators.Length == 0 || inOptions.MaxSlices == 1)
            {
                if (!bRemoveEmpty || inLength > 0)
                {
                    outSlices.Add(new StringSlice(inString, inStartIdx, inLength));
                    return 1;
                }

                return 0;
            }

            int sepCount = inSeparators.Length;

            int startIdx = inStartIdx;
            int currentLength = 0;
            int slices = 0;

            for (int charIdx = 0; charIdx < inLength && slices < outSlices.Capacity; ++charIdx)
            {
                int realIdx = inStartIdx + charIdx;
                char c = inString[realIdx];
                bool bSplit = false;
                for (int sepIdx = 0; !bSplit && sepIdx < sepCount; ++sepIdx)
                {
                    bSplit = c == inSeparators[sepIdx];
                }

                if (bSplit)
                {
                    if (!bRemoveEmpty || currentLength > 0)
                    {
                        StringSlice slice = new StringSlice(inString, startIdx, currentLength);
                        outSlices.Add(slice);
                        ++slices;
                    }

                    startIdx = realIdx + 1;
                    
                    if (slices >= inOptions.MaxSlices - 1)
                    {
                        currentLength = inLength - charIdx - 1;
                        break;
                    }

                    currentLength = 0;
                }
                else
                {
                    ++currentLength;
                }
            }

            if (currentLength > 0 && slices < outSlices.Capacity)
            {
                StringSlice slice = new StringSlice(inString, startIdx, currentLength);
                outSlices.Add(slice);
                ++slices;
            }

            return slices;
        }

        static private int Split<T>(string inString, int inStartIdx, int inLength, ISplitter inSplitter, StringSliceOptions inOptions, ref T outSlices) where T : ITempList<StringSlice>
        {
            if (inString == null || inOptions.MaxSlices <= 0)
                return 0;

            bool bRemoveEmpty = (inOptions.Split & StringSplitOptions.RemoveEmptyEntries) != 0;

            if (inSplitter == null || inOptions.MaxSlices == 1)
            {
                if (!bRemoveEmpty || inLength > 0)
                {
                    outSlices.Add(new StringSlice(inString, inStartIdx, inLength));
                    return 1;
                }

                return 0;
            }

            if (inOptions.MaxSlices == 1)
            {
                StringSlice slice = inSplitter.Process(new StringSlice(inString, inStartIdx, inLength), 0);
                if (!bRemoveEmpty || inLength > 0)
                {
                    outSlices.Add(slice);
                    return 1;
                }

                return 0;
            }

            uint splitterState = 0;

            int startIdx = inStartIdx;
            int currentLength = 0;
            int slices = 0;

            for (int charIdx = 0; charIdx < inLength && slices < outSlices.Capacity; ++charIdx)
            {
                int realIdx = inStartIdx + charIdx;

                int evalAdvance;
                bool bSplit = inSplitter.Evaluate(inString, realIdx, slices, ref splitterState, out evalAdvance);

                charIdx += evalAdvance;
                currentLength += evalAdvance;

                if (bSplit)
                {
                    if (!bRemoveEmpty || currentLength > 0)
                    {
                        StringSlice slice = new StringSlice(inString, startIdx, currentLength);
                        slice = inSplitter.Process(slice, splitterState);
                        if (!bRemoveEmpty || slice.Length > 0)
                        {
                            outSlices.Add(slice);
                            ++slices;
                        }
                    }

                    startIdx = realIdx + 1 + evalAdvance;
                    
                    if (slices >= inOptions.MaxSlices - 1)
                    {
                        currentLength = inLength - charIdx - 1;
                        break;
                    }

                    currentLength = 0;
                }
                else
                {
                    ++currentLength;
                }
            }

            if (currentLength > 0 && slices < outSlices.Capacity)
            {
                StringSlice slice = new StringSlice(inString, startIdx, currentLength);
                slice = inSplitter.Process(slice, splitterState);
                if (!bRemoveEmpty || slice.Length > 0)
                {
                    outSlices.Add(slice);
                    ++slices;
                }
            }

            return slices;
        }

        static private IEnumerable<StringSlice> EnumerableSplit(string inString, int inStartIdx, int inLength, char[] inSeparators, StringSliceOptions inOptions)
        {
            if (inString == null || inOptions.MaxSlices <= 0)
                yield break;

            bool bRemoveEmpty = (inOptions.Split & StringSplitOptions.RemoveEmptyEntries) != 0;

            if (inSeparators == null || inSeparators.Length == 0 || inOptions.MaxSlices == 1)
            {
                if (!bRemoveEmpty || inLength > 0)
                {
                    yield return new StringSlice(inString, inStartIdx, inLength);
                }
                yield break;
            }

            int sepCount = inSeparators.Length;

            int startIdx = inStartIdx;
            int currentLength = 0;
            int slices = 0;

            for (int charIdx = 0; charIdx < inLength; ++charIdx)
            {
                int realIdx = inStartIdx + charIdx;
                char c = inString[realIdx];
                bool bSplit = false;
                for (int sepIdx = 0; !bSplit && sepIdx < sepCount; ++sepIdx)
                {
                    bSplit = c == inSeparators[sepIdx];
                }

                if (bSplit)
                {
                    if (!bRemoveEmpty || currentLength > 0)
                    {
                        StringSlice slice = new StringSlice(inString, startIdx, currentLength);
                        yield return slice;
                        slices++;
                    }

                    startIdx = realIdx + 1;
                    
                    if (slices >= inOptions.MaxSlices - 1)
                    {
                        currentLength = inLength - charIdx - 1;
                        break;
                    }

                    currentLength = 0;
                }
                else
                {
                    ++currentLength;
                }
            }

            if (currentLength > 0)
            {
                StringSlice slice = new StringSlice(inString, startIdx, currentLength);
                yield return slice;
            }
        }

        static private IEnumerable<StringSlice> EnumerableSplit(string inString, int inStartIdx, int inLength, ISplitter inSplitter, StringSliceOptions inOptions)
        {
            if (inString == null || inOptions.MaxSlices <= 0)
                yield break;

            bool bRemoveEmpty = (inOptions.Split & StringSplitOptions.RemoveEmptyEntries) != 0;

            if (inSplitter == null)
            {
                if (!bRemoveEmpty || inLength > 0)
                {
                    yield return new StringSlice(inString, inStartIdx, inLength);
                }
                yield break;
            }

            if (inOptions.MaxSlices == 1)
            {
                StringSlice slice = inSplitter.Process(new StringSlice(inString, inStartIdx, inLength), 0);
                if (!bRemoveEmpty || inLength > 0)
                {
                    yield return slice;
                }
                yield break;
            }

            uint splitterState = 0;

            int startIdx = inStartIdx;
            int currentLength = 0;
            int slices = 0;

            for (int charIdx = 0; charIdx < inLength; ++charIdx)
            {
                int realIdx = inStartIdx + charIdx;

                int evalAdvance;
                bool bSplit = inSplitter.Evaluate(inString, realIdx, slices, ref splitterState, out evalAdvance);

                charIdx += evalAdvance;
                currentLength += evalAdvance;

                if (bSplit)
                {
                    if (!bRemoveEmpty || currentLength > 0)
                    {
                        StringSlice slice = new StringSlice(inString, startIdx, currentLength);
                        slice = inSplitter.Process(slice, splitterState);
                        if (!bRemoveEmpty || slice.Length > 0)
                        {
                            yield return slice;
                            slices++;
                        }
                    }

                    startIdx = realIdx + 1 + evalAdvance;
                    
                    if (slices >= inOptions.MaxSlices - 1)
                    {
                        currentLength = inLength - charIdx - 1;
                        break;
                    }

                    currentLength = 0;
                }
                else
                {
                    ++currentLength;
                }
            }

            if (currentLength > 0)
            {
                StringSlice slice = new StringSlice(inString, startIdx, currentLength);
                slice = inSplitter.Process(slice, splitterState);
                if (!bRemoveEmpty || slice.Length > 0)
                {
                    yield return slice;
                }
            }
        }

        static private StringSlice Trim(string inString, int inStartIdx, int inLength, char[] inTrimChars, TrimType inTrimType)
        {
            if (inString == null)
                return StringSlice.Empty;

            if (inTrimChars == null || inTrimChars.Length == 0 || inTrimType == TrimType.None)
                return new StringSlice(inString, inStartIdx, inLength);

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
                return StringSlice.Empty;

            int newLength = endIdx - startIdx + 1;
            if (newLength == inLength)
                return new StringSlice(inString, inStartIdx, inLength);

            return new StringSlice(inString, startIdx, newLength);
        }

        [Flags]
        internal enum TrimType
        {
            None = 0,
            Start = 0x01,
            End = 0x02,
            Both = Start | End
        }

        // Taken from String.WhitespaceChars
        static internal readonly char[] TrimWhitespaceChars = new char[]
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

    /// <summary>
    /// String slicing options.
    /// </summary>
    public struct StringSliceOptions
    {
        public readonly StringSplitOptions Split;
        public readonly int MaxSlices;

        static public readonly StringSliceOptions Default = new StringSliceOptions(StringSplitOptions.None, int.MaxValue);

        public StringSliceOptions(StringSplitOptions inOptions, int inMaxSlices = int.MaxValue)
        {
            Split = inOptions;
            MaxSlices = inMaxSlices;
        }

        static public implicit operator StringSliceOptions(StringSplitOptions inSplit)
        {
            return new StringSliceOptions(inSplit);
        }
    }
}