/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    6 Sept 2020
 * 
 * File:    StringHash.cs
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
    public struct StringHash : IEquatable<StringHash>, IComparable<StringHash>
    {
        internal const char Prefix = '@';

        static StringHash()
        {
            #if DEVELOPMENT
            EnableReverseLookup(true);
            #endif // DEVELOPMENT
        }

        private const string ReverseLookupUnavailable = "[Unavailable]";
        private const string ReverseLookupUnknownFormat = "[Unknown]:{0}";

        [SerializeField, HideInInspector] private uint m_HashValue;

        public StringHash(string inString)
        {
            m_HashValue = StoreHash(inString, 0, inString == null ? 0 : inString.Length);
        }

        public StringHash(StringSlice inSlice)
        {
            m_HashValue = inSlice.CalculateHash();
        }

        public StringHash(uint inHash) 
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

        static public readonly StringHash Null = new StringHash();

        #region IEquatable

        public bool Equals(StringHash other)
        {
            return m_HashValue == other.m_HashValue;
        }

        #endregion // IEquatable

        #region IComparable

        public int CompareTo(StringHash other)
        {
            return m_HashValue.CompareTo(other.m_HashValue);
        }

        #endregion // IComparable

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is StringHash)
                return Equals((StringHash) obj);
            
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
            return ReverseLookup(m_HashValue);
        }

        #endregion // Overrides

        #region Operators

        static public bool operator==(StringHash left, StringHash right)
        {
            return left.m_HashValue == right.m_HashValue;
        }

        static public bool operator!=(StringHash left, StringHash right)
        {
            return left.m_HashValue != right.m_HashValue;
        }

        static public implicit operator StringHash(StringSlice inSlice)
        {
            return new StringHash(inSlice);
        }

        static public implicit operator StringHash(string inString)
        {
            return new StringHash(inString);
        }

        static public implicit operator bool(StringHash inHash)
        {
            return inHash.m_HashValue != 0;
        }

        #endregion // Operators

        static internal uint Hash(string inString, int inOffset, int inLength)
        {
            if (inLength <= 0)
                return 0;
            
            uint hash = 2166136261;
            for (int i = 0; i < inLength; ++i)
                hash = (hash ^ inString[inOffset + i]) * 16777619;
            return hash;
        }

        #region Caching

        #if DEVELOPMENT

        static private bool s_ReverseLookupEnabled;
        static private Dictionary<uint, string> s_ReverseLookup;

        /// <summary>
        /// Enabled/disables reverse hash lookup.
        /// Reverse lookup cannot be enabled in non-development builds.
        /// </summary>
        static public void EnableReverseLookup(bool inbEnabled)
        {
            if (s_ReverseLookupEnabled != inbEnabled)
            {
                s_ReverseLookupEnabled = inbEnabled;
                if (inbEnabled)
                {
                    s_ReverseLookup = new Dictionary<uint, string>(256);
                }
                else
                {
                    s_ReverseLookup.Clear();
                    s_ReverseLookup = null;
                }
            }
        }

        /// <summary>
        /// Returns if reverse hash lookup is enabled.
        /// </summary>
        static public bool IsReverseLookupEnabled()
        {
            return s_ReverseLookupEnabled;
        }

        /// <summary>
        /// Clears the reverse hash lookup cache.
        /// Non-functional in non-development builds.
        /// </summary>
        static public void ClearReverseLookup()
        {
            if (s_ReverseLookupEnabled)
            {
                s_ReverseLookup.Clear();
            }
        }

        static internal uint StoreHash(string inString, int inOffset, int inLength)
        {
            uint hash = Hash(inString, inOffset, inLength);
            if (inLength > 0 && s_ReverseLookupEnabled)
            {
                StringSlice current = new StringSlice(inString, inOffset, inLength);

                string existing;
                if (s_ReverseLookup.TryGetValue(hash, out existing))
                {
                    if (current != existing)
                    {
                        UnityEngine.Debug.LogErrorFormat("[StringHash] Collision detected: '{0}' and '{1}' share hash {2}", existing, current, hash);
                    }
                }
                else
                {
                    s_ReverseLookup.Add(hash, current.ToString());
                }
            }
            return hash;
        }

        static private string ReverseLookup(uint inHash)
        {
            if (inHash == 0)
                return string.Empty;

            if (!s_ReverseLookupEnabled)
                return ReverseLookupUnavailable;

            string str;
            if (!s_ReverseLookup.TryGetValue(inHash, out str))
                str = string.Format(ReverseLookupUnknownFormat, inHash);

            return str;
        }

        #else

        /// <summary>
        /// Enabled/disables reverse hash lookup.
        /// Reverse lookup cannot be enabled in non-development builds.
        /// </summary>
        static public void EnableReverseLookup(bool inbEnabled)
        {
            if (inbEnabled)
                throw new InvalidOperationException("Reverse lookup cannot be enabled in non-development builds");
        }

        /// <summary>
        /// Returns if reverse hash lookup is enabled.
        /// </summary>
        static public bool IsReverseLookupEnabled()
        {
            return false;
        }

        /// <summary>
        /// Clears the reverse hash lookup cache.
        /// Non-functional in non-development builds.
        /// </summary>
        static public void ClearReverseLookup()
        {
        }

        [MethodImpl(256)]
        static internal uint StoreHash(string inString, int inOffset, int inLength)
        {
            return Hash(inString, inOffset, inLength);
        }

        static private string ReverseLookup(uint inHash)
        {
            return inHash == 0 ? string.Empty : ReverseLookupUnavailable;
        }

#endif // DEVELOPMENT

        #endregion // Caching
    }
}