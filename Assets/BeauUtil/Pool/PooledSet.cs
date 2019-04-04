/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    PooledSet.cs
 * Purpose: Pooled set, to help avoid garbage generation.
 */

using System;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Pooled version of a Set.
    /// </summary>
    public class PooledSet<T> : HashSet<T>, IDisposable
    {
        private void Reset()
        {
            Clear();
        }

        /// <summary>
        /// Resets and recycles the PooledList to the pool.
        /// </summary>
        public void Dispose()
        {
            Reset();
            s_ObjectPool.Push(this);
        }

        #region Pool

        // Maximum number to hold in pool at a time.
        private const int POOL_SIZE = 8;

        // Object pool to hold available PooledSet.
        static private Pool<PooledSet<T>> s_ObjectPool = new Pool<PooledSet<T>>.Static(POOL_SIZE, PoolUtil.Constructor<PooledSet<T>>());

        /// <summary>
        /// Retrieves a PooledList for use.
        /// </summary>
        static public PooledSet<T> Create()
        {
            return s_ObjectPool.Pop();
        }

        /// <summary>
        /// Retrieves a PooledSet for use, copying the contents
        /// of the given IEnumerable.
        /// </summary>
        static public PooledSet<T> Create(IEnumerable<T> inToCopy)
        {
            PooledSet<T> set = s_ObjectPool.Pop();
            foreach (var obj in inToCopy)
                set.Add(obj);
            return set;
        }

        #endregion
    }
}
