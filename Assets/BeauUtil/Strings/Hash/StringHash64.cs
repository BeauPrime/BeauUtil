/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    6 Sept 2020
 * 
 * File:    StringHash64.cs
 * Purpose: 64-bit string hash struct.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Eight-byte string hash.
    /// </summary>
    [DebuggerDisplay("{ToDebugString()}")]
    [StructLayout(LayoutKind.Sequential, Size=8)]
    [Serializable]
    public struct StringHash64 : IEquatable<StringHash64>, IComparable<StringHash64>, IDebugString
        #if USING_BEAUDATA
        , BeauData.ISerializedProxy<ulong>
        #endif // USING_BEAUDATA
    {
        [SerializeField, HideInInspector] private ulong m_HashValue;

        public StringHash64(string inString)
        {
            m_HashValue = StringHashing.StoreHash64(inString, 0, inString == null ? 0 : inString.Length);
        }

        public StringHash64(StringSlice inSlice)
        {
            m_HashValue = inSlice.CalculateHash64();
        }

        public StringHash64(ulong inHash)
        {
            m_HashValue = inHash;
        }

        /// <summary>
        /// Returns the hash value.
        /// </summary>
        public ulong HashValue
        {
            get { return m_HashValue; }
        }

        /// <summary>
        /// Returns if this is an empty hash.
        /// </summary>
        public bool IsEmpty
        {
            get { return m_HashValue == 0; }
        }

        /// <summary>
        /// Concats a string to this hash value.
        /// </summary>
        public StringHash64 Concat(StringSlice inSlice)
        {
            return new StringHash64(inSlice.AppendHash64(m_HashValue, true));
        }

        /// <summary>
        /// Concats a string to this hash value.
        /// This will not record the concatenated string to the Reverse Lookup.
        /// In non-debug builds this is functionally identical to Concat.
        /// </summary>
        public StringHash64 FastConcat(StringSlice inSlice)
        {
            return new StringHash64(inSlice.AppendHash64(m_HashValue, false));
        }

        /// <summary>
        /// Calculates the string hash without caching a reverse lookup.
        /// </summary>
        static public StringHash64 Fast(StringSlice inSlice)
        {
            return new StringHash64(inSlice.CalculateHash64NoCache());
        }

        /// <summary>
        /// Calculates the string hash without caching a reverse lookup.
        /// </summary>
        static public StringHash64 Fast(StringBuilderSlice inSlice)
        {
            return new StringHash64(inSlice.CalculateHash64NoCache());
        }

        /// <summary>
        /// Constructs a case-insensitive hash.
        /// </summary>
        static public StringHash64 CaseInsensitive(string inString)
        {
            return new StringHash64(StringHashing.StoreHash64CaseInsensitive(inString, 0, inString == null ? 0 : inString.Length));
        }

        /// <summary>
        /// Constructs a case-insensitive hash without caching a reverse lookup.
        /// </summary>
        static public StringHash64 FastCaseInsensitive(string inString)
        {
            return new StringHash64(StringHashing.Hash64CaseInsensitive(inString, 0, inString == null ? 0 : inString.Length));
        }

        /// <summary>
        /// Constructs a case-insensitive hash.
        /// </summary>
        static public StringHash64 CaseInsensitive(StringSlice inSlice)
        {
            return new StringHash64(inSlice.CalculateHash64CaseInsensitive());
        }

        /// <summary>
        /// Constructs a case-insensitive hash without caching a reverse lookup.
        /// </summary>
        static public StringHash64 FastCaseInsensitive(StringSlice inSlice)
        {
            return new StringHash64(inSlice.CalculateHash64CaseInsensitiveNoCache());
        }

        /// <summary>
        /// Constructs a case-insensitive hash.
        /// </summary>
        static public StringHash64 CaseInsensitive(StringBuilderSlice inSlice)
        {
            return new StringHash64(inSlice.CalculateHash64CaseInsensitive());
        }

        /// <summary>
        /// Constructs a case-insensitive hash without caching a reverse lookup.
        /// </summary>
        static public StringHash64 FastCaseInsensitive(StringBuilderSlice inSlice)
        {
            return new StringHash64(inSlice.CalculateHash64CaseInsensitiveNoCache());
        }

        static public readonly StringHash64 Null = new StringHash64();

        #region IEquatable

        public bool Equals(StringHash64 other)
        {
            return m_HashValue == other.m_HashValue;
        }

        #endregion // IEquatable

        #region IComparable

        public int CompareTo(StringHash64 other)
        {
            return m_HashValue < other.m_HashValue ? -1 : (m_HashValue > other.m_HashValue ? 1 : 0);
        }

        #endregion // IComparable

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is StringHash64)
                return Equals((StringHash64) obj);
            
            return false;
        }

        public override int GetHashCode()
        {
            return (int) m_HashValue;
        }

        public override string ToString()
        {
            return string.Format("@{0:X8}", m_HashValue);
        }

        public string ToDebugString()
        {
            return StringHashing.ReverseLookup64(m_HashValue);
        }

        #endregion // Overrides

        #region Operators

        static public bool operator==(StringHash64 left, StringHash64 right)
        {
            return left.m_HashValue == right.m_HashValue;
        }

        static public bool operator!=(StringHash64 left, StringHash64 right)
        {
            return left.m_HashValue != right.m_HashValue;
        }

        static public bool operator==(StringHash64 left, ulong right)
        {
            return left.m_HashValue == right;
        }

        static public bool operator!=(StringHash64 left, ulong right)
        {
            return left.m_HashValue != right;
        }

        static public implicit operator StringHash64(StringSlice inSlice)
        {
            return new StringHash64(inSlice);
        }

        static public implicit operator StringHash64(string inString)
        {
            return new StringHash64(inString);
        }

        static public explicit operator bool(StringHash64 inHash)
        {
            return inHash.m_HashValue != 0;
        }

        #endregion // Operators

        #region Parse

        static public bool TryParse(StringSlice inSlice, out StringHash64 outHash)
        {
            if (inSlice.StartsWith(StringHashing.CustomHashPrefix))
            {
                ulong hexVal;
                if (StringParser.TryParseHex(inSlice.Substring(1), 16, out hexVal))
                {
                    outHash = new StringHash64(hexVal);
                    return true;
                }
            }
            else if (inSlice.StartsWith("0x"))
            {
                ulong hexVal;
                if (StringParser.TryParseHex(inSlice.Substring(2), 16, out hexVal))
                {
                    outHash = new StringHash64(hexVal);
                    return true;
                }

                outHash = default(StringHash64);
                return false;
            }
            else if (inSlice.StartsWith(StringHashing.StringPrefix))
            {
                outHash = inSlice.Substring(1).Hash64();
                return true;
            }
            else if (inSlice.StartsWith('"') && inSlice.EndsWith('"'))
            {
                outHash = inSlice.Substring(1, inSlice.Length - 2).Hash64();
                return true;
            }

            outHash = inSlice.Hash64();
            return true;
        }

        /// <summary>
        /// Parses the string slice into a hash.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public StringHash64 Parse(StringSlice inSlice, StringHash64 inDefault = default(StringHash64))
        {
            StringHash64 val;
            if (!TryParse(inSlice, out val))
                val = inDefault;
            return val;
        }

        #endregion // Parse

        #region ISerializedProxy

        #if USING_BEAUDATA

        public ulong GetProxyValue(BeauData.ISerializerContext unused)
        {
            return m_HashValue;
        }

        public void SetProxyValue(ulong inValue, BeauData.ISerializerContext unused)
        {
            m_HashValue = inValue;
        }

        #endif // USING_BEAUDATA

        #endregion // ISerializedProxy
    }
}