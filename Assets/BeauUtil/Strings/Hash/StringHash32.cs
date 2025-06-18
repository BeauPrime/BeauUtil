/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    6 Sept 2020
 * 
 * File:    StringHash32.cs
 * Purpose: String hash struct.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace BeauUtil
{
    /// <summary>
    /// Four-byte string hash.
    /// </summary>
    [DebuggerDisplay("{ToDebugString()}")]
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    [DefaultEqualityComparer(typeof(StringHash32.Comparer)), DefaultSorter(typeof(StringHash32.Comparer))]
    public struct StringHash32 : IEquatable<StringHash32>, IComparable<StringHash32>, IDebugString
        #if USING_BEAUDATA
        , BeauData.ISerializedProxy<uint>
        #endif // USING_BEAUDATA
    {
        [SerializeField, FormerlySerializedAs("m_Hash"), HideInInspector] private uint m_HashValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringHash32(string inString)
        {
            m_HashValue = StringHashing.StoreHash32(inString, 0, inString == null ? 0 : inString.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringHash32(StringSlice inSlice)
        {
            m_HashValue = inSlice.CalculateHash32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringHash32(UnsafeString inString)
        {
            m_HashValue = inString.CalculateHash32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringHash32(uint inHash)
        {
            m_HashValue = inHash;
        }

        /// <summary>
        /// Returns the hash value.
        /// </summary>
        public uint HashValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_HashValue; }
        }

        /// <summary>
        /// Returns if this is an empty hash.
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_HashValue == 0; }
        }

        /// <summary>
        /// Concats a string to this hash value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringHash32 Concat(StringSlice inSlice)
        {
            return new StringHash32(inSlice.AppendHash32(m_HashValue, true));
        }

        /// <summary>
        /// Concats a string to this hash value.
        /// This will not record the concatenated string to the Reverse Lookup.
        /// In non-debug builds this is functionally identical to Concat.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringHash32 FastConcat(StringSlice inSlice)
        {
            return new StringHash32(inSlice.AppendHash32(m_HashValue, false));
        }

        /// <summary>
        /// Calculates the string hash without caching a reverse lookup.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 Fast(StringSlice inSlice)
        {
            return new StringHash32(inSlice.CalculateHash32NoCache());
        }

        /// <summary>
        /// Calculates the string hash without caching a reverse lookup.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 Fast(UnsafeString inString)
        {
            return new StringHash32(inString.CalculateHash32NoCache());
        }

        /// <summary>
        /// Calculates the string hash without caching a reverse lookup.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 Fast(StringBuilderSlice inSlice)
        {
            return new StringHash32(inSlice.CalculateHash32NoCache());
        }

        /// <summary>
        /// Calculates the string hash without caching a reverse lookup.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 Fast(StringBuilder inBuilder, int inStartIdx, int inLength)
        {
            return new StringHash32(StringHashing.Hash32(inBuilder, inStartIdx, inLength));
        }

        /// <summary>
        /// Constructs a case-insensitive hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 CaseInsensitive(string inString)
        {
            return new StringHash32(StringHashing.StoreHash32CaseInsensitive(inString, 0, inString == null ? 0 : inString.Length));
        }

        /// <summary>
        /// Constructs a case-insensitive hash without caching a reverse lookup.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 FastCaseInsensitive(string inString)
        {
            return new StringHash32(StringHashing.Hash32CaseInsensitive(inString, 0, inString == null ? 0 : inString.Length));
        }

        /// <summary>
        /// Constructs a case-insensitive hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 CaseInsensitive(StringSlice inSlice)
        {
            return new StringHash32(inSlice.CalculateHash32CaseInsensitive());
        }

        /// <summary>
        /// Constructs a case-insensitive hash without caching a reverse lookup.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 FastCaseInsensitive(StringSlice inSlice)
        {
            return new StringHash32(inSlice.CalculateHash32CaseInsensitiveNoCache());
        }

        /// <summary>
        /// Constructs a case-insensitive hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 CaseInsensitive(UnsafeString inString)
        {
            return new StringHash32(inString.CalculateHash32CaseInsensitive());
        }

        /// <summary>
        /// Constructs a case-insensitive hash without caching a reverse lookup.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 FastCaseInsensitive(UnsafeString inString)
        {
            return new StringHash32(inString.CalculateHash32CaseInsensitiveNoCache());
        }

        /// <summary>
        /// Constructs a case-insensitive hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 CaseInsensitive(StringBuilderSlice inSlice)
        {
            return new StringHash32(inSlice.CalculateHash32CaseInsensitive());
        }

        /// <summary>
        /// Constructs a case-insensitive hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 CaseInsensitive(StringBuilder inBuilder, int inStartIdx, int inLength)
        {
            return new StringHash32(StringHashing.StoreHash32CaseInsensitive(inBuilder, inStartIdx, inLength));
        }

        /// <summary>
        /// Constructs a case-insensitive hash without caching a reverse lookup.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 FastCaseInsensitive(StringBuilderSlice inSlice)
        {
            return new StringHash32(inSlice.CalculateHash32CaseInsensitiveNoCache());
        }

        /// <summary>
        /// Constructs a case-insensitive hash without caching a reverse lookup.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 FastCaseInsensitive(StringBuilder inBuilder, int inStartIdx, int inLength)
        {
            return new StringHash32(StringHashing.Hash32CaseInsensitive(inBuilder, inStartIdx, inLength));
        }

        static public readonly StringHash32 Null = new StringHash32();

        #region IEquatable

        public bool Equals(StringHash32 other)
        {
            return m_HashValue == other.m_HashValue;
        }

        #endregion // IEquatable

        #region IComparable

        public int CompareTo(StringHash32 other)
        {
            return m_HashValue < other.m_HashValue ? -1 : (m_HashValue > other.m_HashValue ? 1 : 0);
        }

        #endregion // IComparable

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is StringHash32)
                return Equals((StringHash32) obj);
            
            return false;
        }

        public override int GetHashCode()
        {
            return unchecked((int) m_HashValue);
        }

        public override string ToString()
        {
            return string.Format("@{0:X8}", m_HashValue);
        }

        public string ToDebugString()
        {
            return StringHashing.ReverseLookup32(m_HashValue);
        }

        #endregion // Overrides

        #region Operators

        static public bool operator==(StringHash32 left, StringHash32 right)
        {
            return left.m_HashValue == right.m_HashValue;
        }

        static public bool operator!=(StringHash32 left, StringHash32 right)
        {
            return left.m_HashValue != right.m_HashValue;
        }

        static public bool operator==(StringHash32 left, uint right)
        {
            return left.m_HashValue == right;
        }

        static public bool operator!=(StringHash32 left, uint right)
        {
            return left.m_HashValue != right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public implicit operator StringHash32(StringSlice inSlice)
        {
            return new StringHash32(inSlice);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public implicit operator StringHash32(string inString)
        {
            return new StringHash32(inString);
        }

        static public explicit operator bool(StringHash32 inHash)
        {
            return inHash.m_HashValue != 0;
        }

        #endregion // Operators

        #region Parse

        static public bool TryParse(StringSlice inSlice, out StringHash32 outHash)
        {
            if (inSlice.StartsWith(StringHashing.CustomHashPrefix))
            {
                ulong hexVal;
                if (StringParser.TryParseHex(inSlice.Substring(1), 8, out hexVal))
                {
                    outHash = new StringHash32((uint) hexVal);
                    return true;
                }
            }
            else if (inSlice.StartsWith("0x"))
            {
                ulong hexVal;
                if (StringParser.TryParseHex(inSlice.Substring(2), 8, out hexVal))
                {
                    outHash = new StringHash32((uint) hexVal);
                    return true;
                }

                outHash = default(StringHash32);
                return false;
            }
            else if (inSlice.StartsWith(StringHashing.StringPrefix))
            {
                outHash = inSlice.Substring(1).Hash32();
                return true;
            }
            else if (inSlice.StartsWith('"') && inSlice.EndsWith('"'))
            {
                outHash = inSlice.Substring(1, inSlice.Length - 2).Hash32();
                return true;
            }

            outHash = inSlice.Hash32();
            return true;
        }

        /// <summary>
        /// Parses the string slice into a hash.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public StringHash32 Parse(StringSlice inSlice, StringHash32 inDefault = default(StringHash32))
        {
            StringHash32 val;
            if (!TryParse(inSlice, out val))
                val = inDefault;
            return val;
        }

        static public bool TryParse(UnsafeString inString, out StringHash32 outHash)
        {
            if (inString.StartsWith(StringHashing.CustomHashPrefix))
            {
                ulong hexVal;
                if (StringParser.TryParseHex(inString.Substring(1), 8, out hexVal))
                {
                    outHash = new StringHash32((uint) hexVal);
                    return true;
                }
            }
            else if (inString.StartsWith("0x"))
            {
                ulong hexVal;
                if (StringParser.TryParseHex(inString.Substring(2), 8, out hexVal))
                {
                    outHash = new StringHash32((uint) hexVal);
                    return true;
                }

                outHash = default(StringHash32);
                return false;
            }
            else if (inString.StartsWith(StringHashing.StringPrefix))
            {
                outHash = inString.Substring(1).Hash32();
                return true;
            }
            else if (inString.StartsWith('"') && inString.EndsWith('"'))
            {
                outHash = inString.Substring(1, inString.Length - 2).Hash32();
                return true;
            }

            outHash = inString.Hash32();
            return true;
        }

        /// <summary>
        /// Parses the unsafe string into a hash.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public StringHash32 Parse(UnsafeString inString, StringHash32 inDefault = default(StringHash32))
        {
            StringHash32 val;
            if (!TryParse(inString, out val))
                val = inDefault;
            return val;
        }

        #endregion // Parse

        #region ISerializedProxy

#if USING_BEAUDATA

        public uint GetProxyValue(BeauData.ISerializerContext unused)
        {
            return m_HashValue;
        }

        public void SetProxyValue(uint inValue, BeauData.ISerializerContext unused)
        {
            m_HashValue = inValue;
        }

#endif // USING_BEAUDATA

        #endregion // ISerializedProxy

        #region Comparisons

        /// <summary>
        /// Default comparer.
        /// </summary>
        private sealed class Comparer : IEqualityComparer<StringHash32>, IComparer<StringHash32>
        {
            public int Compare(StringHash32 x, StringHash32 y)
            {
                return x.m_HashValue < y.m_HashValue ? -1 : (x.m_HashValue > y.m_HashValue ? 1 : 0);
            }

            public bool Equals(StringHash32 x, StringHash32 y)
            {
                return x.m_HashValue == y.m_HashValue;
            }

            public int GetHashCode(StringHash32 obj)
            {
                return unchecked((int) obj.m_HashValue);
            }
        }

        #endregion // Comparisons

        #region Select

        /// <summary>
        /// Returns the first hash that is not empty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 First(StringHash32 inHash0, StringHash32 inHash1)
        {
            return (!inHash0.IsEmpty ? inHash0
                : (inHash1));
        }

        /// <summary>
        /// Returns the first hash that is not empty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 First(StringHash32 inHash0, StringHash32 inHash1, StringHash32 inHash2)
        {
            return (!inHash0.IsEmpty ? inHash0
                : (!inHash1.IsEmpty ? inHash1
                : (inHash2)));
        }

        /// <summary>
        /// Returns the first hash that is not empty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 First(StringHash32 inHash0, StringHash32 inHash1, StringHash32 inHash2, StringHash32 inHash3)
        {
            return (!inHash0.IsEmpty ? inHash0
                : (!inHash1.IsEmpty ? inHash1
                : (!inHash2.IsEmpty ? inHash2
                : (inHash3))));
        }

        #endregion // Select
    }
}