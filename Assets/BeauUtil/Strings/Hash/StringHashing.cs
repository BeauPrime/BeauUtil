/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 Dec 2020
 * 
 * File:    StringHashing.cs
 * Purpose: Shared string hashing code.
 */

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEVELOPMENT
#define PRESERVE_DEBUG_SYMBOLS
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Text;
using System.Reflection;
using UnityEngine.Scripting;

namespace BeauUtil
{
    static public class StringHashing
    {
        static StringHashing()
        {
            #if PRESERVE_DEBUG_SYMBOLS
            foreach(var assembly in Reflect.FindAllUserAssemblies())
            {
                if (assembly.IsDefined(typeof(StringHashReverseCacheInitialCapacityAttribute)))
                {
                    s_CacheSizeAttribute = (StringHashReverseCacheInitialCapacityAttribute) assembly.GetCustomAttribute(typeof(StringHashReverseCacheInitialCapacityAttribute));
                    break;
                }
            }

            EnableReverseLookup(true);
            #endif // PRESERVE_DEBUG_SYMBOLS
        }

        internal const char CustomHashPrefix = '@';
        internal const char StringPrefix = '\'';

        private const string ReverseLookupUnavailable = "[Unavailable]";
        private const string ReverseLookupUnknownFormat = "[Unknown]:{0}";

        #region Hash Algorithms

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
                while(inLength-- > 0)
                {
                    hash = (hash ^ *inc++) * 16777619;
                }
            }
            
            return hash;
        }

        static unsafe internal uint Hash32CaseInsensitive(string inString, int inOffset, int inLength)
        {
            if (inLength <= 0)
                return 0;
            
            // fnv-1a
            uint hash = 2166136261;
            
            // unsafe method
            fixed(char* ptr = inString)
            {
                char* inc = ptr + inOffset;
                while(inLength-- > 0)
                {
                    hash = (hash ^ StringUtils.ToUpperInvariant(*inc++)) * 16777619;
                }
            }
            
            return hash;
        }

        static internal uint Hash32(StringBuilder inString, int inOffset, int inLength)
        {
            if (inLength <= 0)
                return 0;
            
            // fnv-1a
            uint hash = 2166136261;
            
            int idx = inOffset;
            while(inLength-- > 0)
            {
                hash = (hash ^ inString[idx++]) * 16777619;
            }
            
            return hash;
        }

        static internal uint Hash32CaseInsensitive(StringBuilder inString, int inOffset, int inLength)
        {
            if (inLength <= 0)
                return 0;
            
            // fnv-1a
            uint hash = 2166136261;
            
            int idx = inOffset;
            while(inLength-- > 0)
            {
                hash = (hash ^ StringUtils.ToUpperInvariant(inString[idx++])) * 16777619;
            }
            
            return hash;
        }

        static unsafe internal uint Hash32Append(uint inHash, string inString, int inOffset, int inLength)
        {
            if (inLength <= 0)
                return inHash;

            // fnv-1a
            uint hash = inHash;
            
            // unsafe method
            fixed(char* ptr = inString)
            {
                char* inc = ptr + inOffset;
                while(inLength-- > 0)
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
                while(inLength-- > 0)
                {
                    hash = (hash ^ *inc++) * 1099511628211;
                }
            }
            
            return hash;
        }

        static unsafe internal ulong Hash64CaseInsensitive(string inString, int inOffset, int inLength)
        {
            if (inLength <= 0)
                return 0;
            
            // fnv-1a
            ulong hash = 14695981039346656037;
            
            // unsafe method
            fixed(char* ptr = inString)
            {
                char* inc = ptr + inOffset;
                while(inLength-- > 0)
                {
                    hash = (hash ^ StringUtils.ToUpperInvariant(*inc++)) * 1099511628211;
                }
            }
            
            return hash;
        }

        static internal ulong Hash64(StringBuilder inString, int inOffset, int inLength)
        {
            if (inLength <= 0)
                return 0;
            
            // fnv-1a
            ulong hash = 14695981039346656037;
            
            // unsafe method
            int idx = inOffset;
            while(inLength-- > 0)
            {
                hash = (hash ^ inString[idx++]) * 1099511628211;
            }
            
            return hash;
        }

        static internal ulong Hash64CaseInsensitive(StringBuilder inString, int inOffset, int inLength)
        {
            if (inLength <= 0)
                return 0;
            
            // fnv-1a
            ulong hash = 14695981039346656037;
            
            // unsafe method
            int idx = inOffset;
            while(inLength-- > 0)
            {
                hash = (hash ^ StringUtils.ToUpperInvariant(inString[idx++])) * 1099511628211;
            }
            
            return hash;
        }

        static unsafe internal ulong Hash64Append(ulong inHash, string inString, int inOffset, int inLength)
        {
            if (inLength <= 0)
                return 0;
            
            // fnv-1a
            ulong hash = inHash;
            
            // unsafe method
            fixed(char* ptr = inString)
            {
                char* inc = ptr + inOffset;
                while(inLength-- > 0)
                {
                    hash = (hash ^ *inc++) * 1099511628211;
                }
            }
            
            return hash;
        }

        #endregion // Hash Algorithms

        #region Lookups

        public delegate void CollisionCallbackDelegate(StringSlice inStringA, StringSlice inStringB, int inHashSize, ulong inHashValue);

        #if PRESERVE_DEBUG_SYMBOLS

        static private bool s_ReverseLookupEnabled;
        static private CollisionCallbackDelegate s_OnCollision;
        static private readonly StringHashReverseCacheInitialCapacityAttribute s_CacheSizeAttribute;

        static private Dictionary<uint, string> s_ReverseLookup32;
        static private Dictionary<ulong, string> s_ReverseLookup64;

        static private uint s_ReverseLookup32ResizeCount;
        static private uint s_ReverseLookup64ResizeCount;

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
                    s_ReverseLookup32 = new Dictionary<uint, string>(s_CacheSizeAttribute != null ? s_CacheSizeAttribute.Hash32 : 512);
                    s_ReverseLookup64 = new Dictionary<ulong, string>(s_CacheSizeAttribute != null ? s_CacheSizeAttribute.Hash64 : 512);
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
            s_OnCollision = inCallback;
        }

        /// <summary>
        /// Estimates the size, in bytes, of the reverse lookup cache.
        /// Non-functional in non-development builds.
        /// </summary>
        static public void DumpReverseLookupStats()
        {
            if (!s_ReverseLookupEnabled)
            {
                UnityEngine.Debug.LogFormat("[StringHashing] Reverse Hash disabled, no stats");
                return;
            }

            long hash32size = 0, hash32count = 0, hash32capacity = 0;
            long hash64size = 0, hash64count = 0, hash64capacity = 0;
            float hash32usage = 0;
            float hash64usage = 0;

            if (s_ReverseLookup32 != null)
            {
                hash32capacity = s_ReverseLookup32.GetCapacity();

                hash32size += 24 * hash32capacity;
                foreach(var kvp in s_ReverseLookup32)
                {
                    hash32size += 12 + kvp.Value.Length * 2;
                }

                hash32count = s_ReverseLookup32.Count;
                hash32usage = hash32count * 100f / hash32capacity;
            }

            if (s_ReverseLookup64 != null)
            {
                hash64capacity = s_ReverseLookup64.GetCapacity();

                hash64size += 24 * hash64capacity;
                foreach (var kvp in s_ReverseLookup64)
                {
                    hash64size += 12 + kvp.Value.Length * 2;
                }

                hash64count = s_ReverseLookup64.Count;
                hash64usage = hash64count * 100f / hash64capacity;
            }

            UnityEngine.Debug.LogFormat("[StringHashing] Reverse Hash Table | 32 - ~{0} in {1} entries ({2}% of {3}) | 64 - ~{4} in {5} entries ({6}% of {7})",
                Unsafe.FormatBytes(hash32size), hash32count, hash32usage, hash32capacity,
                Unsafe.FormatBytes(hash64size), hash64count, hash64usage, hash64capacity);
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

        /// <summary>
        /// Estimates the size, in bytes, of the reverse lookup cache.
        /// Non-functional in non-development builds.
        /// </summary>
        static public void DumpReverseLookupStats()
        {
        }

        #endif // PRESERVE_DEBUG_SYMBOLS

        #endregion // Lookups
    
        #region Store/Retrieve

        #if PRESERVE_DEBUG_SYMBOLS

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
                        if (s_OnCollision != null)
                            s_OnCollision(existing, current, 32, hash);
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

        static internal uint StoreHash32CaseInsensitive(string inString, int inOffset, int inLength)
        {
            uint hash = Hash32CaseInsensitive(inString, inOffset, inLength);
            if (inLength > 0 && s_ReverseLookupEnabled)
            {
                StringSlice current = new StringSlice(inString, inOffset, inLength);

                string existing;
                if (s_ReverseLookup32.TryGetValue(hash, out existing))
                {
                    if (!current.Equals(existing, true))
                    {
                        if (s_OnCollision != null)
                            s_OnCollision(existing, current, 32, hash);
                        else
                            UnityEngine.Debug.LogErrorFormat("[StringHashing] 32-bit collision detected: '{0}' and '{1}' share hash {2}", existing, current, hash.ToString("X8"));
                    }
                }
                else
                {
                    s_ReverseLookup32.Add(hash, current.ToString().ToUpperInvariant());
                }
            }
            return hash;
        }

        static internal uint StoreHash32(StringBuilder inString, int inOffset, int inLength)
        {
            uint hash = Hash32(inString, inOffset, inLength);
            if (inLength > 0 && s_ReverseLookupEnabled)
            {
                StringBuilderSlice current = new StringBuilderSlice(inString, inOffset, inLength);

                string existing;
                if (s_ReverseLookup32.TryGetValue(hash, out existing))
                {
                    if (current != existing)
                    {
                        if (s_OnCollision != null)
                            s_OnCollision(existing, current.ToString(), 32, hash);
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

        static internal uint StoreHash32CaseInsensitive(StringBuilder inString, int inOffset, int inLength)
        {
            uint hash = Hash32CaseInsensitive(inString, inOffset, inLength);
            if (inLength > 0 && s_ReverseLookupEnabled)
            {
                StringBuilderSlice current = new StringBuilderSlice(inString, inOffset, inLength);

                string existing;
                if (s_ReverseLookup32.TryGetValue(hash, out existing))
                {
                    if (!current.Equals(existing, true))
                    {
                        if (s_OnCollision != null)
                            s_OnCollision(existing, current.ToString(), 32, hash);
                        else
                            UnityEngine.Debug.LogErrorFormat("[StringHashing] 32-bit collision detected: '{0}' and '{1}' share hash {2}", existing, current, hash.ToString("X8"));
                    }
                }
                else
                {
                    s_ReverseLookup32.Add(hash, current.ToString().ToUpperInvariant());
                }
            }
            return hash;
        }

        static internal uint AppendHash32(uint inHash, string inString, int inOffset, int inLength, bool inbReverseLookup)
        {
            uint hash = Hash32Append(inHash, inString, inOffset, inLength);
            if (inLength > 0 && s_ReverseLookupEnabled && inbReverseLookup)
            {
                StringSlice current = new StringSlice(inString, inOffset, inLength);
                if (inHash != 0)
                {
                    string root;
                    if (s_ReverseLookup32.TryGetValue(inHash, out root))
                    {
                        current = string.Concat(root, current.ToString());
                    }
                    else
                    {
                        UnityEngine.Debug.LogErrorFormat("[StringHashing] Unknown 32-bit hash {0}, cannot create reverse lookup for appending '{1}'", inHash.ToString("X8"), current);
                        return hash;
                    }
                }

                string existing;
                if (s_ReverseLookup32.TryGetValue(hash, out existing))
                {
                    if (current != existing)
                    {
                        if (s_OnCollision != null)
                            s_OnCollision(existing, current, 32, hash);
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
                        if (s_OnCollision != null)
                            s_OnCollision(existing, current, 64, hash);
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

        static internal ulong StoreHash64CaseInsensitive(string inString, int inOffset, int inLength)
        {
            ulong hash = Hash64CaseInsensitive(inString, inOffset, inLength);
            if (inLength > 0 && s_ReverseLookupEnabled)
            {
                StringSlice current = new StringSlice(inString, inOffset, inLength);

                string existing;
                if (s_ReverseLookup64.TryGetValue(hash, out existing))
                {
                    if (!current.Equals(existing, true))
                    {
                        if (s_OnCollision != null)
                            s_OnCollision(existing, current, 64, hash);
                        else
                            UnityEngine.Debug.LogErrorFormat("[StringHashing] 64-bit collision detected: '{0}' and '{1}' share hash {2}", existing, current, hash.ToString("X16"));
                    }
                }
                else
                {
                    s_ReverseLookup64.Add(hash, current.ToString().ToUpperInvariant());
                }
            }
            return hash;
        }

        static internal ulong StoreHash64(StringBuilder inString, int inOffset, int inLength)
        {
            ulong hash = Hash64(inString, inOffset, inLength);
            if (inLength > 0 && s_ReverseLookupEnabled)
            {
                StringBuilderSlice current = new StringBuilderSlice(inString, inOffset, inLength);

                string existing;
                if (s_ReverseLookup64.TryGetValue(hash, out existing))
                {
                    if (current != existing)
                    {
                        if (s_OnCollision != null)
                            s_OnCollision(existing, current.ToString(), 64, hash);
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

        static internal ulong StoreHash64CaseInsensitive(StringBuilder inString, int inOffset, int inLength)
        {
            ulong hash = Hash64CaseInsensitive(inString, inOffset, inLength);
            if (inLength > 0 && s_ReverseLookupEnabled)
            {
                StringBuilderSlice current = new StringBuilderSlice(inString, inOffset, inLength);

                string existing;
                if (s_ReverseLookup64.TryGetValue(hash, out existing))
                {
                    if (!current.Equals(existing, true))
                    {
                        if (s_OnCollision != null)
                            s_OnCollision(existing, current.ToString(), 64, hash);
                        else
                            UnityEngine.Debug.LogErrorFormat("[StringHashing] 64-bit collision detected: '{0}' and '{1}' share hash {2}", existing, current, hash.ToString("X16"));
                    }
                }
                else
                {
                    s_ReverseLookup64.Add(hash, current.ToString().ToUpperInvariant());
                }
            }
            return hash;
        }

        static internal ulong AppendHash64(ulong inHash, string inString, int inOffset, int inLength, bool inbReverseLookup)
        {
            ulong hash = Hash64Append(inHash, inString, inOffset, inLength);
            if (inLength > 0 && s_ReverseLookupEnabled && inbReverseLookup)
            {
                StringSlice current = new StringSlice(inString, inOffset, inLength);
                if (inHash != 0)
                {
                    string root;
                    if (s_ReverseLookup64.TryGetValue(inHash, out root))
                    {
                        current = string.Concat(root, current.ToString());
                    }
                    else
                    {
                        UnityEngine.Debug.LogErrorFormat("[StringHashing] Unknown 64-bit hash {0}, cannot create reverse lookup for appending '{1}'", inHash.ToString("X8"), current);
                        return hash;
                    }
                }

                string existing;
                if (s_ReverseLookup64.TryGetValue(hash, out existing))
                {
                    if (current != existing)
                    {
                        if (s_OnCollision != null)
                            s_OnCollision(existing, current, 64, hash);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static internal uint StoreHash32(string inString, int inOffset, int inLength)
        {
            return Hash32(inString, inOffset, inLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static internal uint StoreHash32CaseInsensitive(string inString, int inOffset, int inLength)
        {
            return Hash32CaseInsensitive(inString, inOffset, inLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static internal uint StoreHash32(StringBuilder inString, int inOffset, int inLength)
        {
            return Hash32(inString, inOffset, inLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static internal uint StoreHash32CaseInsensitive(StringBuilder inString, int inOffset, int inLength)
        {
            return Hash32CaseInsensitive(inString, inOffset, inLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static internal uint AppendHash32(uint inHash, string inString, int inOffset, int inLength, bool inbReverseLookup)
        {
            return Hash32Append(inHash, inString, inOffset, inLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static internal string ReverseLookup32(uint inHash)
        {
            return inHash == 0 ? string.Empty : ReverseLookupUnavailable;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static internal ulong StoreHash64(string inString, int inOffset, int inLength)
        {
            return Hash64(inString, inOffset, inLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static internal ulong StoreHash64CaseInsensitive(string inString, int inOffset, int inLength)
        {
            return Hash64CaseInsensitive(inString, inOffset, inLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static internal ulong StoreHash64(StringBuilder inString, int inOffset, int inLength)
        {
            return Hash64(inString, inOffset, inLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static internal ulong StoreHash64CaseInsensitive(StringBuilder inString, int inOffset, int inLength)
        {
            return Hash64CaseInsensitive(inString, inOffset, inLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static internal ulong AppendHash64(ulong inHash, string inString, int inOffset, int inLength, bool inbReverseLookup)
        {
            return Hash64Append(inHash, inString, inOffset, inLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static internal string ReverseLookup64(ulong inHash)
        {
            return inHash == 0 ? string.Empty : ReverseLookupUnavailable;
        }

        #endif // PRESERVE_DEBUG_SYMBOLS

        #endregion // Store/Retrieve
    }

    /// <summary>
    /// Declares the initial capacity for the string hash reverse lookup tables.
    /// </summary>
    /// <remarks>
    /// This MUST be declared on an assembly.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    [Preserve]
    public sealed class StringHashReverseCacheInitialCapacityAttribute : Attribute
    {
        public readonly int Hash32;
        public readonly int Hash64;

        public StringHashReverseCacheInitialCapacityAttribute(int inHash32Capacity = 256, int inHash64Capacity = 256)
        {
            Hash32 = inHash32Capacity;
            Hash64 = inHash64Capacity;
        }
    }
}