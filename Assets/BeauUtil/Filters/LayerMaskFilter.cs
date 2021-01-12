/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Dec 2020
 * 
 * File:    LayerMaskFilter.cs
 * Purpose: Filter for a GameObject vs a LayerMask.
 */

using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Filter for a GameObject vs a LayerMask.
    /// </summary>
    public struct LayerMaskFilter : IObjectFilter<GameObject>
    {
        public LayerMask Mask;

        public bool Allow(GameObject inObject)
        {
            return Mask == 0 || ((1 << inObject.layer) & Mask) != 0;
        }
    }
}