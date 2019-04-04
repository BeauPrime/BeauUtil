/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    PooledStringBuilder.cs
 * Purpose: Pooled StringBuilder, to help avoid garbage generation.
 */

using System;
using System.Text;

namespace BeauUtil
{
    /// <summary>
    /// Pooled version of a StringBuilder.
    /// </summary>
    public class PooledStringBuilder : IDisposable
    {
        /// <summary>
        /// The internal StringBuilder object.
        /// </summary>
        public readonly StringBuilder Builder = new StringBuilder(256);

        private void Reset()
        {
            Builder.Length = 0;
            Builder.EnsureCapacity(256);
        }

        /// <summary>
        /// Resets and recycles the builder to the pool.
        /// </summary>
        public void Dispose()
        {
            Reset();
            s_ObjectPool.Push(this);
        }

        public override string ToString()
        {
            return Builder.ToString();
        }

        static public implicit operator StringBuilder(PooledStringBuilder inPooled)
        {
            return inPooled.Builder;
        }

        #region Pool

        // Maximum number to hold in pool at a time.
        private const int POOL_SIZE = 8;

        // Object pool to hold available StringBuilders.
        static private Pool<PooledStringBuilder> s_ObjectPool = new Pool<PooledStringBuilder>.Static(POOL_SIZE, PoolUtil.Constructor<PooledStringBuilder>());

        /// <summary>
        /// Retrieves a PooledStringBuilder for use.
        /// </summary>
        static public PooledStringBuilder Create()
        {
            return s_ObjectPool.Pop();
        }

        /// <summary>
        /// Retrieves a PooledStringBuilder to use.
        /// </summary>
        static public PooledStringBuilder Create(string inSource)
        {
            PooledStringBuilder builder = s_ObjectPool.Pop();
            builder.Builder.Append(inSource);
            return builder;
        }

        #endregion
    }
}
