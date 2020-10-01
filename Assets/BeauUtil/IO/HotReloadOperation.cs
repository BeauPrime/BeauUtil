/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Sept 2020
 * 
 * File:    HotReloadOperation.cs
 * Purpose: Argument representing what kind of hot reload should occur.
 */

namespace BeauUtil.IO
{
    /// <summary>
    /// Trigger for a hot reload.
    /// </summary>
    public enum HotReloadOperation
    {
        Unaffected = 0,

        Modified,
        Deleted
    }
}