/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    31 August 2020
 * 
 * File:    TempList16.cs
 * Purpose: Temporary list with up to 16 elements.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Temporary list, going up to 16 elements.
    /// </summary>
    public interface ITempList<T> : IList<T>, IReadOnlyList<T>
    {
        int Capacity { get; }
        T[] ToArray();
    }
}