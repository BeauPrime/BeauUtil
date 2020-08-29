/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 August 2020
 * 
 * File:    ColliderProxyEventMask.cs
 * Purpose: Mask for collision proxy events.
 */

using System;

namespace BeauUtil
{
    [Flags]
    public enum ColliderProxyEventMask
    {
        OnEnter = 0x01,
        OnExit = 0x02,
        OnIdle = 0x04,

        [Hidden]
        OnEnterAndExit = OnEnter | OnExit,

        [Hidden]
        All = OnEnter | OnExit | OnIdle
    }
}