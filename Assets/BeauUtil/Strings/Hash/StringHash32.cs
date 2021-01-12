/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    6 Sept 2020
 * 
 * File:    StringHash32.cs
 * Purpose: String hash struct.
 */

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Four-byte string hash.
    /// </summary>
    [DebuggerDisplay("{ToDebugString()}")]
    [StructLayout(LayoutKind.Sequential, Size=4)]
    [Serializable]
    public struct StringHash32 : IEquatable<StringHash32>, IComparable<StringHash32>
    {
        [SerializeField, HideInInspector] private uint m_HashValue;

        public StringHash32(string inString)
        {
            m_HashValue = StringHashing.StoreHash32(inString, 0, inString == null ? 0 : inString.Length);
        }

        public StringHash32(StringSlice inSlice)
        {
            m_HashValue = inSlice.CalculateHash32();
        }

        public StringHash32(uint inHash)
        {
            m_HashValue = inHash;
        }

        /// <summary>
        /// Returns the hash value.
        /// </summary>
        public uint HashValue
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
            return (int) m_HashValue;
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

        static public implicit operator StringHash32(StringSlice inSlice)
        {
            return new StringHash32(inSlice);
        }

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

        #endregion // Parse
    }
}