/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Dec 2020
 * 
 * File:    CallbackMode.cs
 * Purpose: Callback mode enum definition.
 */

using System;

namespace BeauUtil
{
    internal enum CallbackMode : byte
    {
        Unassigned,
        NoArg,
        NativeArg,
        CastedArg,
    }
}