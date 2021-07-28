/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 July 2020
 * 
 * File:    Unsafe.cs
 * Purpose: Unsafe utility methods.
 */

using System;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Contains unsafe utility functions.
    /// </summary>
    static public unsafe class Unsafe
    {
        static public void Prefetch(void* inData)
        {
            char c = *((char*) inData);
        }
    }
}