/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    27 Sept 2019
 * 
 * File:    LabelAttribute.cs
 * Purpose: Specifies a label for an element.
 */

using System;

namespace BeauUtil
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public sealed class LabelAttribute : Attribute
    {
        public string Name { get; private set; }
        
        public LabelAttribute(string inName)
        {
            Name = inName;
        }
    }
}