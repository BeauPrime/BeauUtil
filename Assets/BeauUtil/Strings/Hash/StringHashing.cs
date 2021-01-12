/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 Dec 2020
 * 
 * File:    StringHashing.cs
 * Purpose: Shared string hashing code.
 */

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    static public class StringHashing
    {
        static StringHashing()
        {
            #if DEVELOPMENT
            EnableReverseLookup(true);
            #endif // DEVELOPMENT
        }

        internal const char CustomHashPrefix = '@';
        internal const char StringPrefix = '\'';

        private const string ReverseLookupUnavailable = "[Unavailable]";
        private const string ReverseLookupUnknownFormat = "[Unknown]:{0}";

        #region Hash Algorithms

        static unsafe internal ushort Hash16(string inString, int inOffset, int inLength)
        {
            if (inLength <= 0)
                return 0;
            
            // fnv-1a
            uint hash = 2166136261;
            
            // unsafe method
            fixed(char* ptr = inString)
            {
                char* inc = ptr + inOffset;
                while(--inLength >= 0)
                {
                    hash = (hash ^ *inc++) * 16777619;
                }
            }
            
            return (ushort) ((hash >> 16) ^ (hash & 0xFFFF));
        }

        static unsafe internal uint Hash32(string inString, int inOffset, int inLength)
        {
            if (inLength <= 0)
                return 0;
            
            // fnv-1a
            uint hash = 2166136261;
            
            // unsafe method
            fixed(char* ptr = inString)
            {
                char* inc = ptr + inOffset;
                while(--inLength >= 0)
                {
                    hash = (hash ^ *inc++) * 16777619;
                }
            }
            
            return hash;
        }

        static unsafe internal ulong Hash64(string inString, int inOffset, int inLength)
        {
            if (inLength <= 0)
                return 0;
            
            // fnv-1a
            ulong hash = 14695981039346656037;
            
            // unsafe method
            fixed(char* ptr = inString)
            {
                char* inc = ptr + inOffset;
                while(--inLength >= 0)
                {
                    hash = (hash ^ *inc++) * 1099511628211;
                }
            }
            
            return hash;
        }

        #endregion // Hash Algorithms

        #region Lookups

        public delegate void CollisionCallbackDelegate(StringSlice inStringA, StringSlice inStringB, int inHashSize, ulong inHashValue);

        #if DEVELOPMENT

        static private bool s_ReverseLookupEnabled;
        static private CollisionCallbackDelegate s_OnCollison;

        static private Dictionary<uint, string> s_ReverseLookup32;
        static private Dictionary<ulong, string> s_ReverseLookup64;

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
                    s_ReverseLookup32 = new Dictionary<uint, string>(256);
                    s_ReverseLookup64 = new Dictionary<ulong, string>(256);
                }
                else
                {
                    s_ReverseLookup32.Clear();
                    s_ReverseLookup64.Clear();

                    s_ReverseLookup32 = null;
                    s_ReverseLookup64 = null;
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
                s_ReverseLookup32.Clear();
                s_ReverseLookup64.Clear();
            }
        }
    
        /// <summary>
        /// Sets the callback for when a hash collision occurs.
        /// Non-functional in non-development builds.
        /// </summary>
        static public void SetOnCollision(CollisionCallbackDelegate inCallback)
        {
            s_OnCollison = inCallback;
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

        /// <summary>
        /// Sets the callback for when a hash collision occurs.
        /// Non-functional in non-development builds.
        /// </summary>
        static public void SetOnCollision(CollisionCallbackDelegate inCallback)
        {
        }

        #endif // DEVELOPMENT

        #endregion // Lookups
    
        #region Store/Retrieve

        #if DEVELOPMENT

        static internal uint StoreHash32(string inString, int inOffset, int inLength)
        {
            uint hash = Hash32(inString, inOffset, inLength);
            if (inLength > 0 && s_ReverseLookupEnabled)
            {
                StringSlice current = new StringSlice(inString, inOffset, inLength);

                string existing;
                if (s_ReverseLookup32.TryGetValue(hash, out existing))
                {
                    if (current != existing)
                    {
                        if (s_OnCollison != null)
                            s_OnCollison(existing, current, 32, hash);
                        else
                            UnityEngine.Debug.LogErrorFormat("[StringHashing] 32-bit collision detected: '{0}' and '{1}' share hash {2}", existing, current, hash.ToString("X8"));
                    }
                }
                else
                {
                    s_ReverseLookup32.Add(hash, current.ToString());
                }
            }
            return hash;
        }

        static internal string ReverseLookup32(uint inHash)
        {
            if (inHash == 0)
                return string.Empty;

            if (!s_ReverseLookupEnabled)
                return ReverseLookupUnavailable;

            string str;
            if (!s_ReverseLookup32.TryGetValue(inHash, out str))
                str = string.Format(ReverseLookupUnknownFormat, inHash);

            return str;
        }

        static internal ulong StoreHash64(string inString, int inOffset, int inLength)
        {
            ulong hash = Hash64(inString, inOffset, inLength);
            if (inLength > 0 && s_ReverseLookupEnabled)
            {
                StringSlice current = new StringSlice(inString, inOffset, inLength);

                string existing;
                if (s_ReverseLookup64.TryGetValue(hash, out existing))
                {
                    if (current != existing)
                    {
                        if (s_OnCollison != null)
                            s_OnCollison(existing, current, 64, hash);
                        else
                            UnityEngine.Debug.LogErrorFormat("[StringHashing] 64-bit collision detected: '{0}' and '{1}' share hash {2}", existing, current, hash.ToString("X16"));
                    }
                }
                else
                {
                    s_ReverseLookup64.Add(hash, current.ToString());
                }
            }
            return hash;
        }

        static internal string ReverseLookup64(ulong inHash)
        {
            if (inHash == 0)
                return string.Empty;

            if (!s_ReverseLookupEnabled)
                return ReverseLookupUnavailable;

            string str;
            if (!s_ReverseLookup64.TryGetValue(inHash, out str))
                str = string.Format(ReverseLookupUnknownFormat, inHash);

            return str;
        }

        #else

        [MethodImpl(256)]
        static internal uint StoreHash32(string inString, int inOffset, int inLength)
        {
            return Hash32(inString, inOffset, inLength);
        }

        [MethodImpl(256)]
        static internal string ReverseLookup32(uint inHash)
        {
            return inHash == 0 ? string.Empty : ReverseLookupUnavailable;
        }

        [MethodImpl(256)]
        static internal ulong StoreHash64(string inString, int inOffset, int inLength)
        {
            return Hash64(inString, inOffset, inLength);
        }

        [MethodImpl(256)]
        static internal string ReverseLookup64(ulong inHash)
        {
            return inHash == 0 ? string.Empty : ReverseLookupUnavailable;
        }

        #endif // DEVELOPMENT

        #endregion // Store/Retrieve
    }
}