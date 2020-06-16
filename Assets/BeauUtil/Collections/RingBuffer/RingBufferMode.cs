/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 June 2020
 * 
 * File:    RingBufferMode.cs
 * Purpose: Ring buffer out-of-space behavior.
 */

namespace BeauUtil
{
    /// <summary>
    /// RingBuffer behavior when out of elements.
    /// </summary>
    public enum RingBufferMode : byte
    {
        // Throw an exception
        Fixed,

        // Overwrite overlapping elements
        Overwrite,

        // Expand the buffer's capacity
        Expand
    }
}