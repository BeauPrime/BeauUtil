/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    20 Jan 2020
 * 
 * File:    PrefabModeOnlyAttribute.cs
 * Purpose: Marks a property to be disabled when not in prefab mode.
 */

using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Disables this property if not in prefab mode.
    /// </summary>
    public class PrefabModeOnlyAttribute : PropertyAttribute
    {
        public bool Hide { get; set; }
    }
}