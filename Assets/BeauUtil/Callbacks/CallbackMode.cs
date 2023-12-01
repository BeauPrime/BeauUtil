/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Dec 2020
 * 
 * File:    CallbackMode.cs
 * Purpose: Callback mode enum definition.
 */

#if UNITY_2021_2_OR_NEWER && !BEAUUTIL_DISABLE_FUNCTION_POINTERS
#define SUPPORTS_FUNCTION_POINTERS
#endif // UNITY_2021_2_OR_NEWER

using System;

namespace BeauUtil
{
    internal enum CallbackMode : byte
    {
        Unassigned,
        NoArg,
        NativeArg,
        NativeArgRef,
        CastedArg,
#if SUPPORTS_FUNCTION_POINTERS
        NoArg_Ptr,
        NativeArg_Ptr,
        NativeArgRef_Ptr,
#endif // SUPPORTS_FUNCTION_POINTERS
    }
}