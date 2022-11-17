/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    15 Oct 2022
 * 
 * File:    NoParseAttribute.cs
 * Purpose: Attribute marking a method parameter should not be parsed.
            This is only valid on the first parameter not marked with a BindContextAttribute.
 */

using System;

namespace BeauUtil
{
    /// <summary>
    /// Marks that a method parameter should not be parsed.
    /// This is only valid for the first parameter not marked with a BindContextAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class NoParseAttribute : Attribute
    {
    }
}