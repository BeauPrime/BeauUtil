/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    31 August 2020
 * 
 * File:    ITempList.cs
 * Purpose: Shared interface for temporary lists.
 */

using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Temporary list interface.
    /// </summary>
    public interface ITempList<T> : IList<T>, IReadOnlyList<T>
    {
        int Capacity { get; }
        T[] ToArray();
    }
}