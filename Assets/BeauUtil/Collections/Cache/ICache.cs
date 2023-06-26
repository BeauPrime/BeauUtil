/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 June 2023
 * 
 * File:    ICache.cs
 * Purpose: Cache interface and associated callbacks.
*/

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Cache interface.
    /// </summary>
    public interface ICache<TKey, TValue>
        where TKey : struct
    {
        /// <summary>
        /// Invoked when an operation executes for a key already in the cache.
        /// </summary>
        event CacheReportDelegate<TKey> OnHit;

        /// <summary>
        /// Invoked when an operation executes for key not in the cache.
        /// </summary>
        event CacheReportDelegate<TKey> OnMiss;

        /// <summary>
        /// Invoked when a key is evicted from the cache.
        /// </summary>
        event CacheReportDelegate<TKey> OnEvict;

        /// <summary>
        /// Tracked cache stats.
        /// </summary>
        CacheStats<TKey> Stats { get; }

        /// <summary>
        /// Number of elements currently in the cache.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Maximum number of elements in the cache.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Returns if a value for the given key is in the cache.
        /// </summary>
        bool Contains(TKey inKey);

        /// <summary>
        /// Reads the value for the given key from the cache,
        /// or fetches it if not already present.
        /// </summary>
        TValue Read(TKey inKey);

        /// <summary>
        /// Attempts to read the value for the given key from the cache.
        /// This will not fetch a new value on a cache miss.
        /// </summary>
        bool TryRead(TKey inKey, out TValue outValue);

        /// <summary>
        /// Ensures a value for the given key is loaded in the cache.
        /// </summary>
        void Poke(TKey inKey);

        /// <summary>
        /// Writes a value into the cache for the given key.
        /// Returns the current value, in case an existing value was overwritten.
        /// </summary>
        TValue Write(TKey inKey, TValue inValue);

        /// <summary>
        /// Writes a value into the cache for the given key.
        /// </summary>
        void Write(TKey inKey, ref TValue ioValue);

        /// <summary>
        /// Evicts the value for the given key from the cache.
        /// </summary>
        void Invalidate(TKey inKey);

        /// <summary>
        /// Evicts all values from the cache.
        /// </summary>
        void InvalidateAll();

        /// <summary>
        /// Returns if the cache is locked.
        /// Locked caches do not evict or fetch new values.
        /// </summary>
        bool IsLocked();

        /// <summary>
        /// Locks the cache.
        /// Locked caches do not evict or fetch new values.
        /// </summary>
        void Lock();

        /// <summary>
        /// Unlocks the cache.
        /// Locked caches do not evict or fetch new values.
        /// </summary>
        void Unlock();
    }

    /// <summary>
    /// Type of cache operation.
    /// </summary>
    public enum CacheOperation
    {
        Read,
        Write,
        Evict
    }

    /// <summary>
    /// Block of essential callbacks for cache operation.
    /// </summary
    public struct CacheCallbacks<TKey, TValue>
    {
        /// <summary>
        /// Fetches the value for the given key.
        /// </summary>
        public CacheFetchDelegate<TKey, TValue> Fetch;

        /// <summary>
        /// Overwrites the value.
        /// </summary>
        public CacheOverwriteDelegate<TValue> Overwrite;

        /// <summary>
        /// Evicts the given value.
        /// </summary>
        public CacheEvictCallback<TValue> Evict;

        static internal void FillDefaults(ref CacheCallbacks<TKey, TValue> ioConfig)
        {
            ioConfig.Fetch = ioConfig.Fetch ?? (typeof(TValue).IsValueType ? s_FetchNoOp : s_FetchCreateInstance);
            ioConfig.Overwrite = ioConfig.Overwrite ?? (typeof(ICopyCloneable<TValue>).IsAssignableFrom(typeof(TValue)) ? s_OverwriteCopyClone : s_OverwriteNoOp);
            ioConfig.Evict = ioConfig.Evict ?? s_EvictNoOp;
        }

        static private readonly CacheFetchDelegate<TKey, TValue> s_FetchNoOp = (k) => default(TValue);
        static private readonly CacheFetchDelegate<TKey, TValue> s_FetchCreateInstance = (k) => (TValue) Activator.CreateInstance(typeof(TValue));

        static private readonly CacheOverwriteDelegate<TValue> s_OverwriteNoOp = (ref TValue dest, TValue src) => dest = src;
        static private readonly CacheOverwriteDelegate<TValue> s_OverwriteCopyClone = (ref TValue dest, TValue src) => ((ICopyCloneable<TValue>) dest).CopyFrom(src);

        static private readonly CacheEvictCallback<TValue> s_EvictNoOp = (ref TValue v) => { };
    }

    /// <summary>
    /// Callback used to fetch the most up-to-date value
    /// associated with the given key.
    /// </summary>
    public delegate TValue CacheFetchDelegate<TKey, TValue>(TKey inKey);

    /// <summary>
    /// Callback used when a value is evicted from the cache.
    /// </summary>
    public delegate void CacheEvictCallback<TValue>(ref TValue inValue);

    /// <summary>
    /// Callback used when a value in the cache is directly overwritten.
    /// </summary>
    public delegate void CacheOverwriteDelegate<TValue>(ref TValue inDest, TValue inSource);

    /// <summary>
    /// Callback used when the cache is accessed.
    /// </summary>
    public delegate void CacheReportDelegate<TKey>(TKey inKey, CacheOperation inOperation);

    /// <summary>
    /// Cache hit/miss stat tracking.
    /// </summary>
    public struct CacheStats<TKey> where TKey : struct
    {
        internal struct RW
        {
            public ulong Read;
            public ulong Write;
        }

        internal struct HME
        {
            public ulong Hit;
            public ulong Miss;
            public ulong Evict;
        }

        internal HME m_ReadStats;
        internal HME m_WriteStats;

        /// <summary>
        /// Returns how often an operation missed the cache.
        /// </summary>
        public double MissRatio
        {
            get
            {
                ulong miss = MissCount;
                return (double) miss / (double) (miss + HitCount);
            }
        }

        /// <summary>
        /// Total number of read/write hits.
        /// </summary>
        public ulong HitCount
        {
            get { return m_ReadStats.Hit + m_WriteStats.Hit; }
        }

        /// <summary>
        /// Total number of read/write misses
        /// </summary>
        public ulong MissCount
        {
            get { return m_ReadStats.Miss + m_WriteStats.Miss; }
        }
    }

    /// <summary>
    /// ICache utility and extension methods.
    /// </summary>
    static public class CacheUtility
    {
        /// <summary>
        /// Ensures values for all given keys are present in the cache.
        /// </summary>
        static public void Poke<TKey, TValue>(this ICache<TKey, TValue> inThis, IEnumerable<TKey> inKeys)
            where TKey : unmanaged
        {
            foreach (var key in inKeys)
                inThis.Poke(key);
        }

        /// <summary>
        /// Ensures values for all given keys are present in the cache.
        /// </summary>
        static public void Poke<TKey, TValue>(this ICache<TKey, TValue> inThis, List<TKey> inKeys)
            where TKey : unmanaged
        {
            foreach (var key in inKeys)
                inThis.Poke(key);
        }

        /// <summary>
        /// Ensures values for all given keys are present in the cache.
        /// </summary>
        static public void Poke<TKey, TValue>(this ICache<TKey, TValue> inThis, TKey[] inKeys)
            where TKey : unmanaged
        {
            for (int i = 0, len = inKeys.Length; i < len; i++)
                inThis.Poke(inKeys[i]);
        }

        /// <summary>
        /// Ensures values for all given keys are present in the cache.
        /// </summary>
        static public unsafe void Poke<TKey, TValue>(this ICache<TKey, TValue> inThis, TKey* inKeyBuffer, int inBufferLength)
            where TKey : unmanaged
        {
            for (int i = 0; i < inBufferLength; i++)
                inThis.Poke(inKeyBuffer[i]);
        }

        /// <summary>
        /// Invalidates the entries for the given keys.
        /// </summary>
        static public void Invalidate<TKey, TValue>(this ICache<TKey, TValue> inThis, IEnumerable<TKey> inKeys)
            where TKey : unmanaged
        {
            foreach (var key in inKeys)
                inThis.Poke(key);
        }

        /// <summary>
        /// Invalidates the entries for the given keys.
        /// </summary>
        static public void Invalidate<TKey, TValue>(this ICache<TKey, TValue> inThis, List<TKey> inKeys)
            where TKey : unmanaged
        {
            foreach (var key in inKeys)
                inThis.Poke(key);
        }

        /// <summary>
        /// Invalidates the entries for the given keys.
        /// </summary>
        static public void Invalidate<TKey, TValue>(this ICache<TKey, TValue> inThis, TKey[] inKeys)
            where TKey : unmanaged
        {
            for (int i = 0, len = inKeys.Length; i < len; i++)
                inThis.Poke(inKeys[i]);
        }

        /// <summary>
        /// Invalidates the entries for the given keys.
        /// </summary>
        static public unsafe void Invalidate<TKey, TValue>(this ICache<TKey, TValue> inThis, TKey* inKeyBuffer, int inBufferLength)
            where TKey : unmanaged
        {
            if (inThis.IsLocked())
                return;

            for (int i = 0; i < inBufferLength; i++)
                inThis.Poke(inKeyBuffer[i]);
        }
    }
}