/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Dec 2020
 * 
 * File:    ColliderFilter.cs
 * Purpose: Filter for a Collider
 */

using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Filter a Collider.
    /// </summary>
    public struct ColliderFilter : IObjectFilter<Collider>, IObjectFilter<Collider2D>
    {
        public bool UseRigidbody;
        public bool? IsTrigger;

        public bool Allow(Collider inObject)
        {
            if (!IsTrigger.HasValue)
                return true;
            return inObject.isTrigger == IsTrigger.Value;
        }

        public bool Allow(Collider2D inObject)
        {
            if (!IsTrigger.HasValue)
                return true;
            return inObject.isTrigger == IsTrigger.Value;
        }
    }
}