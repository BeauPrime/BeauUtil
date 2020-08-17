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
    public struct StringSlice : IEnumerable<char>, IReadOnlyList<char>, IEquatable<StringSlice>, IEquatable<string>
    {
        private readonly string m_Source;
        private readonly int m_StartIndex;

        /// <summary>
        /// Total length of the slice.
        /// </summary>
        public readonly int Length;

        public StringSlice(string inString) : this(inString, 0, inString != null ? inString.Length : 0) { }

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
            /// Resets the splitter's state.
            /// </summary>
            void Reset();

            /// <summary>
            /// Evaluates whether or not to split on a given character.
            /// </summary>
            /// <param name="inString">String to evaluate.</param>
            /// <param name="inIndex">Current evaluation index.</param>
            /// <param name="outAdvance">Output for any additional characters to advance.</param>
            bool Evaluate(string inString, int inIndex, out int outAdvance);

            /// <summary>
            /// Post-process for a given slice. Useful for trimming and unescaping.
            /// </summary>
            StringSlice Process(StringSlice inSlice);
        }

        #region Slice

        public StringSlice[] Split(char[] inSeparator, StringSplitOptions inSplitOptions)
        {
            return Split(m_Source, m_StartIndex, Length, inSeparator, inSplitOptions);
        }

        public StringSlice[] Split(ISplitter inSplitter, StringSplitOptions inSplitOptions)
        {
            return Split(m_Source, m_StartIndex, Length, inSplitter, inSplitOptions);
        }

        public int Split(char[] inSeparator, StringSplitOptions inSplitOptions, IList<StringSlice> outSlices)
        {
            return Split(m_Source, m_StartIndex, Length, inSeparator, inSplitOptions, outSlices);
        }

        public int Split(ISplitter inSplitter, StringSplitOptions inSplitOptions, IList<StringSlice> outSlices)
        {
            return Split(m_Source, m_StartIndex, Length, inSplitter, inSplitOptions, outSlices);
        }

        public IEnumerable<StringSlice> EnumeratedSplit(char[] inSeparator, StringSplitOptions inSplitOptions)
        {
            return EnumerableSplit(m_Source, m_StartIndex, Length, inSeparator, inSplitOptions);
        }

        public IEnumerable<StringSlice> EnumeratedSplit(ISplitter inSplitter, StringSplitOptions inSplitOptions)
        {
            return EnumerableSplit(m_Source, m_StartIndex, Length, inSplitter, inSplitOptions);
        }

        #endregion // Slice

        #region Static

        static public StringSlice[] Split(string inString, char[] inSeparator, StringSplitOptions inSplitOptions)
        {
            return Split(inString, 0, inString.Length, inSeparator, inSplitOptions);
        }

        static public StringSlice[] Split(string inString, ISplitter inSplitter, StringSplitOptions inSplitOptions)
        {
            return Split(inString, 0, inString.Length, inSplitter, inSplitOptions);
        }

        static public int Split(string inString, char[] inSeparator, StringSplitOptions inSplitOptions, IList<StringSlice> outSlices)
        {
            return Split(inString, 0, inString.Length, inSeparator, inSplitOptions, outSlices);
        }

        static public int Split(string inString, ISplitter inSplitter, StringSplitOptions inSplitOptions, IList<StringSlice> outSlices)
        {
            return Split(inString, 0, inString.Length, inSplitter, inSplitOptions, outSlices);
        }

        static public IEnumerable<StringSlice> EnumeratedSplit(string inString, char[] inSeparator, StringSplitOptions inSplitOptions)
        {
            return EnumerableSplit(inString, 0, inString.Length, inSeparator, inSplitOptions);
        }

        static public IEnumerable<StringSlice> EnumeratedSplit(string inString, ISplitter inSplitter, StringSplitOptions inSplitOptions)
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
            if (inArray.Length < inCount)
                throw new ArgumentException("Not enough room to copy " + inCount + " items to destination");

            for (int i = 0; i < inCount; ++i)
            {
                inArray[inArrayIdx + i] = m_Source[m_StartIndex + inStartIndex + i];
            }
        }

        public void CopyTo(char[] inArray)
        {
            CopyTo(0, inArray, 0, Length);
        }

        public char[] ToCharArray()
        {
            char[] arr = new char[Length];
            for (int i = 0; i < Length; ++i)
            {
                arr[i] = m_Source[m_StartIndex + i];
            }
            return arr;
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
            uint hash = 2166136261;
            for (int i = 0; i < Length; ++i)
            {
                char c = m_Source[m_StartIndex + i];
                byte b = (byte) (c % 256);
                hash = (hash ^ b) * 16777619;
            }
            return (int) hash;
        }

        public override string ToString()
        {
            if (Length <= 0)
                return string.Empty;
            else if (m_StartIndex == 0 && Length == m_Source.Length)
                return m_Source;
            else
                return m_Source.Substring(m_StartIndex, Length);
        }

        static public bool operator ==(StringSlice inA, StringSlice inB)
        {
            return inA.Equals(inB);
        }

        static public bool operator !=(StringSlice inA, StringSlice inB)
        {
            return inA.Equals(inB);
        }

        static public bool operator ==(StringSlice inA, string inB)
        {
            return inA.Equals(inB);
        }

        static public bool operator !=(StringSlice inA, string inB)
        {
            return inA.Equals(inB);
        }

        static public bool operator ==(string inA, StringSlice inB)
        {
            return inB.Equals(inA);
        }

        static public bool operator !=(string inA, StringSlice inB)
        {
            return inB.Equals(inA);
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

        static private bool MatchStart(string inString, int inStart, int inLength, string inMatch, int inStartMatch, int inLengthMatch, bool inbIgnoreCase)
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

        static private bool MatchEnd(string inString, int inStart, int inLength, string inMatch, int inStartMatch, int inLengthMatch, bool inbIgnoreCase)
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
                    if (char.ToLowerInvariant(a) != char.ToLowerInvariant(b))
                        return false;
                }
            }

            return true;
        }

        static private StringSlice[] Split(string inString, int inStartIdx, int inLength, char[] inSeparator, StringSplitOptions inSplitOptions)
        {
            List<StringSlice> slices = new List<StringSlice>();
            Split(inString, inStartIdx, inLength, inSeparator, inSplitOptions, slices);
            return slices.ToArray();
        }

        static private StringSlice[] Split(string inString, int inStartIdx, int inLength, ISplitter inSplitter, StringSplitOptions inSplitOptions)
        {
            List<StringSlice> slices = new List<StringSlice>();
            Split(inString, inStartIdx, inLength, inSplitter, inSplitOptions, slices);
            return slices.ToArray();
        }

        static private int Split(string inString, int inStartIdx, int inLength, char[] inSeparators, StringSplitOptions inSplitOptions, IList<StringSlice> outSlices)
        {
            if (inString == null)
                return 0;

            bool bRemoveEmpty = (inSplitOptions & StringSplitOptions.RemoveEmptyEntries) != 0;

            if (inSeparators == null || inSeparators.Length == 0)
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

        static private int Split(string inString, int inStartIdx, int inLength, ISplitter inSplitter, StringSplitOptions inSplitOptions, IList<StringSlice> outSlices)
        {
            if (inString == null)
                return 0;

            bool bRemoveEmpty = (inSplitOptions & StringSplitOptions.RemoveEmptyEntries) != 0;

            if (inSplitter == null)
            {
                if (!bRemoveEmpty || inLength > 0)
                {
                    outSlices.Add(new StringSlice(inString, inStartIdx, inLength));
                    return 1;
                }

                return 0;
            }

            inSplitter.Reset();

            int startIdx = inStartIdx;
            int currentLength = 0;
            int slices = 0;

            for (int charIdx = 0; charIdx < inLength; ++charIdx)
            {
                int realIdx = inStartIdx + charIdx;

                int evalAdvance;
                bool bSplit = inSplitter.Evaluate(inString, realIdx, out evalAdvance);

                charIdx += evalAdvance;
                currentLength += evalAdvance;

                if (bSplit)
                {
                    if (!bRemoveEmpty || currentLength > 0)
                    {
                        StringSlice slice = new StringSlice(inString, startIdx, currentLength);
                        slice = inSplitter.Process(slice);
                        if (!bRemoveEmpty || slice.Length > 0)
                        {
                            outSlices.Add(slice);
                            ++slices;
                        }
                    }

                    startIdx = realIdx + 1 + evalAdvance;
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
                slice = inSplitter.Process(slice);
                if (!bRemoveEmpty || slice.Length > 0)
                {
                    outSlices.Add(slice);
                    ++slices;
                }
            }

            return slices;
        }

        static private IEnumerable<StringSlice> EnumerableSplit(string inString, int inStartIdx, int inLength, char[] inSeparators, StringSplitOptions inSplitOptions)
        {
            if (inString == null)
                yield break;

            bool bRemoveEmpty = (inSplitOptions & StringSplitOptions.RemoveEmptyEntries) != 0;

            if (inSeparators == null || inSeparators.Length == 0)
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
                    }

                    startIdx = realIdx + 1;
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

        static private IEnumerable<StringSlice> EnumerableSplit(string inString, int inStartIdx, int inLength, ISplitter inSplitter, StringSplitOptions inSplitOptions)
        {
            if (inString == null)
                yield break;

            bool bRemoveEmpty = (inSplitOptions & StringSplitOptions.RemoveEmptyEntries) != 0;

            if (inSplitter == null)
            {
                if (!bRemoveEmpty || inLength > 0)
                {
                    yield return new StringSlice(inString, inStartIdx, inLength);
                }
                yield break;
            }

            inSplitter.Reset();

            int startIdx = inStartIdx;
            int currentLength = 0;

            for (int charIdx = 0; charIdx < inLength; ++charIdx)
            {
                int realIdx = inStartIdx + charIdx;

                int evalAdvance;
                bool bSplit = inSplitter.Evaluate(inString, realIdx, out evalAdvance);

                charIdx += evalAdvance;
                currentLength += evalAdvance;

                if (bSplit)
                {
                    if (!bRemoveEmpty || currentLength > 0)
                    {
                        StringSlice slice = new StringSlice(inString, startIdx, currentLength);
                        slice = inSplitter.Process(slice);
                        if (!bRemoveEmpty || slice.Length > 0)
                        {
                            yield return slice;
                        }
                    }

                    startIdx = realIdx + 1 + evalAdvance;
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
                slice = inSplitter.Process(slice);
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