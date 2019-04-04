/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    PoolUtil.cs
 * Purpose: Pool utilities.
 */

namespace BeauUtil
{
    static public class PoolUtil
    {
        /// <summary>
        /// Returns a function that calls the default empty constructor
        /// for an object.
        /// </summary>
        static public Pool<T>.Constructor Constructor<T>() where T : class, new()
        {
            return (pool) => { return new T(); };
        }
    }
}
