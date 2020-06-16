/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    27 Sept 2019
 * 
 * File:    LabeledEnumAttribute.cs
 * Purpose: Specifies that an enum should be presented in a sorted manner.
 */

using System;

namespace BeauUtil
{
    [AttributeUsage(AttributeTargets.Enum, Inherited = true, AllowMultiple = false)]
    public sealed class LabeledEnum : Attribute
    {
        public bool Sorted { get; private set; }

        public LabeledEnum(bool inbSorted = true)
        {
            Sorted = inbSorted;
        }
    }
}