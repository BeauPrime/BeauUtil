/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 June 2020
 * 
 * File:    ComparePredicate.cs
 * Purpose: Delegate for arbitrary comparisons.
 */

namespace BeauUtil
{
    /// <summary>
    /// Comparison predicate.
    /// </summary>
    public delegate int ComparePredicate<in T, U>(T inValue, U inCompare);
}