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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BeauUtil
{
    /// <summary>
    /// Four-byte string hash.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size=4)]
    public struct StringHash : IEquatable<StringHash>, IComparable<StringHash>
    {
        static StringHash()
        {
            #if DEVELOPMENT
            EnableReverseLookup(true);
            #endif // DEVELOPMENT
        }

        private const string ReverseLookupUnavailable = "[Unavailable]";
        private const string ReverseLookupUnknownFormat = "[Unknown]:{0}";

        public readonly uint HashValue;

        public StringHash(string inString)
        {
            HashValue = StoreHash(inString, 0, inString == null ? 0 : inString.Length);
        }

        public StringHash(StringSlice inSlice)
        {
            HashValue = inSlice.CalculateHash();
        }

        public StringHash(uint inHash) 
        {
            HashValue = inHash;
        }

        /// <summary>
        /// Returns if this is an empty hash.
        /// </summary>
        public bool IsNullOrEmpty()
        {
            return HashValue == 0;
        }

        static public readonly StringHash Null = new StringHash();

        #region IEquatable

        public bool Equals(StringHash other)
        {
            return HashValue == other.HashValue;
        }

        #endregion // IEquatable

        #region IComparable

        public int CompareTo(StringHash other)
        {
            return HashValue.CompareTo(other.HashValue);
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
            return (int) HashValue;
        }

        public override string ToString()
        {
            return HashValue.ToString();
        }

        public string ToDebugString()
        {
            return ReverseLookup(HashValue);
        }

        #endregion // Overrides

        #region Operators

        static public bool operator==(StringHash left, StringHash right)
        {
            return left.HashValue == right.HashValue;
        }

        static public bool operator!=(StringHash left, StringHash right)
        {
            return left.HashValue != right.HashValue;
        }

        static public implicit operator StringHash(StringSlice inSlice)
        {
            return new StringHash(inSlice);
        }

        static public implicit operator StringHash(string inString)
        {
            return new StringHash(inString);
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
                    UnityEngine.Debug.LogFormat("[StringHash] Hash of '{0}' is {1}", current, hash);
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