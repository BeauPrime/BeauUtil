/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Dec 2020
 * 
 * File:    IObjectFilter.cs
 * Purpose: Filter for a specific object.
 */

namespace BeauUtil
{
    /// <summary>
    /// Filtering interface.
    /// </summary>
    public interface IObjectFilter<in T> where T : class
    {
        bool Allow(T inObject);
    }
}