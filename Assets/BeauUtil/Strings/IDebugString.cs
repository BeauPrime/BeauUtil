/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    20 May 2021
 * 
 * File:    IDebugString.cs
 * Purpose: Interface for objects with a specific debug string.
 */

using System;

namespace BeauUtil
{
    /// <summary>
    /// Interface for objects with a debug string.
    /// </summary>
    public interface IDebugString
    {
        string ToDebugString();
    }
}