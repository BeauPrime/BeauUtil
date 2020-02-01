/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    20 Jan 2020
 * 
 * File:    InstanceOnlyAttribute.cs
 * Purpose: Marks a property to be disabled when editing as a prefab.
 */

using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Disables this property if in prefab mode.
    /// </summary>
    public class InstanceOnlyAttribute : PropertyAttribute
    {
        public bool Hide { get; set; }
    }
}