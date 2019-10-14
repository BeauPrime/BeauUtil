/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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