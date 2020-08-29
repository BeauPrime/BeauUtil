/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 August 2020
 * 
 * File:    ComponentLookupDirection.cs
 * Purpose: Enum indicating a direction for looking up components.
 */

namespace BeauUtil
{
    /// <summary>
    /// Indicates which direction to use for looking up components.
    /// </summary>
    public enum ComponentLookupDirection
    {
        Self,
        Parent,
        Children
    }
}