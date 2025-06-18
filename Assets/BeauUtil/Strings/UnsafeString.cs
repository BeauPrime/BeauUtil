/*
 * Copyright (C) 2025. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    28 May 2025
 * 
 * File:    UnsafeString.cs
 * Purpose: Read-only unsafe string.
 */

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

#if NETSTANDARD || NET_STANDARD
#define SUPPORTS_SPAN
#endif // NETSTANDARD || NET_STANDARD

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using BeauUtil.Debugger;
using Unity.IL2CPP.CompilerServices;

using static BeauUtil.StringSlice;

namespace BeauUtil
{
    /// <summary>
    /// An unsafe string.
    /// </summary>
    public readonly unsafe struct UnsafeString : IEnumerable<char>, IReadOnlyList<char>, IEquatable<UnsafeString>, IEquatable<StringSlice>, IEquatable<string>, IConvertible
    {
        private readonly char* m_Source;

        /// <summary>
        /// Total length of the unsafe string.
        /// </summary>
        public readonly int Length;

        public UnsafeString(char* inString, int inLength)
        {
            if (inLength < 0)
                throw new ArgumentOutOfRangeException("inLength");

            m_Source = inString;
            Length = inString != null ? inLength : 0;
        }

#if UNMANAGED_CONSTRAINT
        public UnsafeString(UnsafeSpan<char> inSpan) : this(inSpan.Ptr, inSpan.Length) { }
#endif // UNMANAGED_CONSTRAINT

        /// <summary>
        /// An empty string.
        /// </summary>
        static public readonly UnsafeString Empty = default(UnsafeString);

        /// <summary>
        /// Returns if this is an empty unsafe string.
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Length == 0; }
        }

        /// <summary>
        /// Returns if unsafe string is empty or has only whitespace characters.
        /// </summary>
        public bool IsWhitespace
        {
            get
            {
                for(int i = 0; i < Length; ++i)
                {
                    if (!char.IsWhiteSpace(m_Source[i]))
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Returns if this unsafe string has any whitespace characters.
        /// </summary>
        public bool HasWhitespace
        {
            get
            {
                for(int i = 0; i < Length; ++i)
                {
                    if (char.IsWhiteSpace(m_Source[i]))
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Returns the 32-bit hash of this unsafe string.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringHash32 Hash32()
        {
            return new StringHash32(CalculateHash32());
        }

        /// <summary>
        /// Returns the 64-bit hash of this unsafe string.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringHash64 Hash64()
        {
            return new StringHash64(CalculateHash64());
        }

        #region Search

        #region Char

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool StartsWith(char inItem)
        {
            return Length > 0 && m_Source[0] == inItem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EndsWith(char inItem)
        {
            return Length > 0 && m_Source[Length - 1] == inItem;
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

            Assert.True(inStartIdx + inCount <= Length, "Out of range");
            int idx = StringUtils.IndexOf(m_Source + inStartIdx, inCount, inItem);
            return idx < 0 ? -1 : idx + inStartIdx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfAny(char[] inItems)
        {
            return IndexOfAny(inItems, 0, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfAny(char[] inItems, int inStartIdx)
        {
            return IndexOfAny(inItems, inStartIdx, Length - inStartIdx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfAny(char[] inItems, int inStartIdx, int inCount)
        {
            if (m_Source == null)
                return -1;

            Assert.True(inStartIdx + inCount <= Length, "Out of range");
            int idx = StringUtils.IndexOfAny(m_Source + inStartIdx, inCount, inItems);
            return idx < 0 ? -1 : idx + inStartIdx;
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

            if (string.IsNullOrEmpty(inItem))
                return false;

            fixed (char* strPtr = inItem)
            {
                return StringUtils.StartsWith(m_Source, Length, strPtr, inItem.Length, inbIgnoreCase);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool StartsWith(StringSlice inItem)
        {
            return StartsWith(inItem, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool StartsWith(StringSlice inItem, bool inbIgnoreCase)
        {
            if (m_Source == null)
                return false;

            inItem.Unpack(out var str, out var off, out var len);
            if (len <= 0)
                return false;

            fixed (char* strPtr = str)
            {
                return StringUtils.StartsWith(m_Source, Length, strPtr + off, len, inbIgnoreCase);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(string inItem)
        {
            return IndexOf(inItem) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(string inItem, bool inbIgnoreCase)
        {
            return IndexOf(inItem, inbIgnoreCase) >= 0;
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

            if (string.IsNullOrEmpty(inItem))
                return false;

            fixed (char* strPtr = inItem)
            {
                return StringUtils.EndsWith(m_Source, Length, strPtr, inItem.Length, inbIgnoreCase);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EndsWith(StringSlice inItem)
        {
            return EndsWith(inItem, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EndsWith(StringSlice inItem, bool inbIgnoreCase)
        {
            if (m_Source == null)
                return false;

            inItem.Unpack(out var str, out var off, out var len);
            if (len <= 0)
                return false;

            fixed (char* strPtr = str)
            {
                return StringUtils.EndsWith(m_Source, Length, strPtr + off, len, inbIgnoreCase);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(string inItem)
        {
            return IndexOf(inItem, 0, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(string inItem, bool inbIgnoreCase)
        {
            return IndexOf(inItem, 0, Length, inbIgnoreCase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(string inItem, int inStartIdx)
        {
            return IndexOf(inItem, inStartIdx, Length - inStartIdx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(string inItem, int inStartIdx, bool inbIgnoreCase)
        {
            return IndexOf(inItem, inStartIdx, Length - inStartIdx, inbIgnoreCase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(string inItem, int inStartIdx, int inCount)
        {
            return IndexOf(inItem, inStartIdx, inCount, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(string inItem, int inStartIdx, int inCount, bool inbIgnoreCase)
        {
            if (m_Source == null || inItem == null)
                return -1;

            Assert.True(inStartIdx + inCount <= Length, "Out of range");
            int idx = StringUtils.IndexOf(m_Source + inStartIdx, inCount, inItem, inbIgnoreCase);
            return idx < 0 ? -1 : idx + inStartIdx;
        }

        #endregion // String

        #endregion // Search

        #region Trim

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeString Trim()
        {
            return Trim(TrimWhitespaceChars);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeString Trim(char[] inTrimChars)
        {
            if (inTrimChars == null || inTrimChars.Length == 0)
            {
                inTrimChars = TrimWhitespaceChars;
            }

            return Trim(m_Source, Length, inTrimChars, TrimType.Both);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeString TrimStart()
        {
            return TrimStart(TrimWhitespaceChars);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeString TrimStart(char[] inTrimChars)
        {
            if (inTrimChars == null || inTrimChars.Length == 0)
            {
                inTrimChars = TrimWhitespaceChars;
            }

            return Trim(m_Source, Length, inTrimChars, TrimType.Start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeString TrimEnd()
        {
            return TrimEnd(TrimWhitespaceChars);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeString TrimEnd(char[] inTrimChars)
        {
            if (inTrimChars == null || inTrimChars.Length == 0)
            {
                inTrimChars = TrimWhitespaceChars;
            }

            return Trim(m_Source, Length, inTrimChars, TrimType.End);
        }

        #endregion // Trim

        #region Substring

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeString Substring(int inStartIdx)
        {
            return Substring(inStartIdx, Length - inStartIdx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeString Substring(int inStartIdx, int inLength)
        {
            if (inStartIdx < 0)
                throw new ArgumentOutOfRangeException("inStartIdx");
            if (inStartIdx + inLength > Length)
                throw new ArgumentOutOfRangeException("inLength");

            return new UnsafeString(m_Source + inStartIdx, inLength);
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

            unsafe
            {
                Unsafe.CopyArray(m_Source + inStartIndex, inCount, inArray, inArrayIdx);
            }
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
                Unsafe.CopyArray(m_Source + inStartIndex, inCount, inBuffer, inCount);
            }
        }

        public char[] ToCharArray()
        {
            if (Length > 0)
            {
                char[] newArr = new char[Length];
                fixed(char* arrPtr = newArr)
                {
                    Unsafe.FastCopyArray(m_Source, Length, arrPtr);
                }
                return newArr;
            }
            return Array.Empty<char>();
        }

        /// <summary>
        /// Unpacks StringSlice parameters.
        /// </summary>
        public void Unpack(out char* outString, out int outLength)
        {
            outString = m_Source;
            outLength = Length;
        }

        #endregion // Export

        #region Spans

#if SUPPORTS_SPAN

        static public implicit operator ReadOnlySpan<char>(UnsafeString slice)
        {
            return slice.Length > 0 ? new ReadOnlySpan<char>(slice.m_Source, slice.Length) : default(ReadOnlySpan<char>);
        }

#endif // SUPPORTS_SPAN

        #endregion // Spans

        #region IReadOnlyList

        public char this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [Il2CppSetOption(Option.NullChecks, false)]
            [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
            get
            {
                if (index < 0 || index >= Length)
                    throw new IndexOutOfRangeException();
                return m_Source[index];
            }
        }

        int IReadOnlyCollection<char>.Count { get { return Length; } }

        #endregion // IReadOnlyList

        #region IEnumerable

#if UNMANAGED_CONSTRAINT
        public UnsafeEnumerator<char> GetEnumerator()
        {
            return new UnsafeEnumerator<char>(m_Source, (uint) Length);
        }

        IEnumerator<char> IEnumerable<char>.GetEnumerator()
        {
            return GetEnumerator();
        }
#else
        public IEnumerator<char> GetEnumerator()
        {
            for (int i = 0; i < Length; ++i)
                yield return this[i];
        }
#endif // UNMANAGED_CONSTRAINT

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion // IEnumerable

        #region IEquatable

        public bool Equals(StringSlice other)
        {
            return Equals(other, false);
        }

        public bool Equals(StringSlice other, bool inbIgnoreCase)
        {
            other.Unpack(out var str, out int off, out int len);
            if (len == 0)
            {
                return Length == 0;
            }

            fixed (char* strPtr = str)
            {
                return StringUtils.Equals(m_Source, Length, strPtr + off, len, false);
            }
        }

        public bool Equals(UnsafeString other)
        {
            return StringUtils.Equals(m_Source, Length, other.m_Source, other.Length, false);
        }

        public bool Equals(UnsafeString other, bool inbIgnoreCase)
        {
            return StringUtils.Equals(m_Source, Length, other.m_Source, other.Length, inbIgnoreCase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(string other)
        {
            return Equals(other, false);
        }

        public bool Equals(string other, bool inbIgnoreCase)
        {
            if (string.IsNullOrEmpty(other))
                return Length == 0;

            fixed(char* otherPtr = other)
            {
                return StringUtils.Equals(m_Source, Length, otherPtr, other.Length, inbIgnoreCase);
            }
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

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is UnsafeString)
                return Equals((UnsafeString) obj);
            if (obj is StringSlice)
                return Equals((StringSlice) obj);
            if (obj is string)
                return Equals((string) obj);

            return false;
        }

        public override int GetHashCode()
        {
            return (int) StringHashing.Hash32(m_Source, Length);
        }

        public override string ToString()
        {
            if (Length <= 0)
            {
                return string.Empty;
            }
            else
            {
                return new string(m_Source, 0, Length);
            }
        }

        static public bool operator ==(UnsafeString inA, UnsafeString inB)
        {
            return inA.Equals(inB);
        }

        static public bool operator !=(UnsafeString inA, UnsafeString inB)
        {
            return !inA.Equals(inB);
        }

        static public bool operator ==(UnsafeString inA, string inB)
        {
            return inA.Equals(inB);
        }

        static public bool operator !=(UnsafeString inA, string inB)
        {
            return !inA.Equals(inB);
        }

        static public bool operator ==(UnsafeString inA, StringSlice inB)
        {
            return inA.Equals(inB);
        }

        static public bool operator !=(UnsafeString inA, StringSlice inB)
        {
            return !inA.Equals(inB);
        }

        static public bool operator ==(string inA, UnsafeString inB)
        {
            return inB.Equals(inA);
        }

        static public bool operator !=(string inA, UnsafeString inB)
        {
            return !inB.Equals(inA);
        }

        #endregion // Overrides

        #region Internal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendTo(StringBuilder ioBuilder)
        {
            if (Length <= 0)
                return;

            ioBuilder.Reserve(Length);
            ioBuilder.Append(m_Source, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint CalculateHash32()
        {
            return StringHashing.StoreHash32(m_Source, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint CalculateHash32NoCache()
        {
            return StringHashing.Hash32(m_Source, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint CalculateHash32CaseInsensitive()
        {
            return StringHashing.StoreHash32CaseInsensitive(m_Source, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint CalculateHash32CaseInsensitiveNoCache()
        {
            return StringHashing.Hash32CaseInsensitive(m_Source, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ulong CalculateHash64()
        {
            return StringHashing.StoreHash64(m_Source, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ulong CalculateHash64NoCache()
        {
            return StringHashing.Hash64(m_Source, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ulong CalculateHash64CaseInsensitive()
        {
            return StringHashing.StoreHash64CaseInsensitive(m_Source, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ulong CalculateHash64CaseInsensitiveNoCache()
        {
            return StringHashing.Hash64CaseInsensitive(m_Source, Length);
        }

        static private UnsafeString Trim(char* inString, int inLength, char[] inTrimChars, TrimType inTrimType)
        {
            if (inString == null || inLength <= 0)
                return UnsafeString.Empty;

            if (inTrimChars == null || inTrimChars.Length == 0 || inTrimType == TrimType.None)
                return new UnsafeString(inString, inLength);

            int trimCount = inTrimChars.Length;

            int startIdx = 0;
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
                return UnsafeString.Empty;

            int newLength = endIdx - startIdx + 1;
            if (newLength == inLength)
                return new UnsafeString(inString, inLength);

            return new UnsafeString(inString + startIdx, newLength);
        }

        #endregion // Internal
    }
}