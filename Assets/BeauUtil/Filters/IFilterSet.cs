/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Dec 2020
 * 
 * File:    IFilterSet.cs
 * Purpose: Interface for a set of filters.
 */

namespace BeauUtil
{
    /// <summary>
    /// Interface for a set of filters.
    /// </summary>
    public interface IFilterSet<in T> where T : class
    {
        StringHash32 Check(T inObject);
        bool Test(T inObject, StringHash32 inId);
        IObjectFilter<T> GetFilter(StringHash32 inId);
    }
}