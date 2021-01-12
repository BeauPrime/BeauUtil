/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Dec 2020
 * 
 * File:    IGOFilterSet.cs
 * Purpose: Interface for a set of gameobject filters.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Interface for a set of GameObject, Collider, and Collider2D Filters
    /// </summary>
    public interface IGOFilterSet : IFilterSet<GameObject>, IFilterSet<Collider>, IFilterSet<Collider2D>
    {
        LayerMask ConcatenatedLayerMask();
        LayerMask LayerMask(StringHash32 inId);
    }
}