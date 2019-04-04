/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    PooledList.cs
 * Purpose: Pooled list, to help avoid garbage generation.
 */

using System;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Pooled version of a List.
    /// </summary>
    public class PooledList<T> : List<T>, IDisposable
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

        // Object pool to hold available PooledList.
        static private Pool<PooledList<T>> s_ObjectPool = new Pool<PooledList<T>>.Static(POOL_SIZE, PoolUtil.Constructor<PooledList<T>>());

        /// <summary>
        /// Retrieves a PooledList for use.
        /// </summary>
        static public PooledList<T> Create()
        {
            return s_ObjectPool.Pop();
        }

        /// <summary>
        /// Retrieves a PooledList for use, copying the contents
        /// of the given IEnumerable.
        /// </summary>
        static public PooledList<T> Create(IEnumerable<T> inToCopy)
        {
            PooledList<T> list = s_ObjectPool.Pop();
            list.AddRange(inToCopy);
            return list;
        }

        #endregion
    }
}
