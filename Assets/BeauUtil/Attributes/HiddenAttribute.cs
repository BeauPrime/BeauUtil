/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    1 Oct 2019
 * 
 * File:    HiddenAttribute.cs
 * Purpose: Specifies that an element should be hidden.
 */

using System;

namespace BeauUtil
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public sealed class HiddenAttribute : Attribute
    {
    }
}