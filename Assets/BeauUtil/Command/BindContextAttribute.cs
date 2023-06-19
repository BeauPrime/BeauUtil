/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    27 June 2021
 * 
 * File:    BindContextAttribute.cs
 * Purpose: Attribute marking a method argument binding.
 */

using System;
using UnityEngine.Scripting;

namespace BeauUtil
{
    /// <summary>
    /// Binds the provided context to this parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class BindContextAttribute : PreserveAttribute
    {
        public virtual object Bind(object inSource)
        {
            return inSource;
        }
    }
}