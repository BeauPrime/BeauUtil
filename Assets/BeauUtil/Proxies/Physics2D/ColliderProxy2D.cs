/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    30 Sept 2019
 * 
 * File:    ColliderProxy2D.cs
 * Purpose: Base class for Collider2D listeners.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BeauUtil
{
    public abstract class ColliderProxy2D : AbstractColliderProxy<Collider2D, Collision2D>
    {
        protected override bool GetColliderEnabled(Collider2D inCollider) { return inCollider.enabled; }
        protected override void SetColliderEnabled(Collider2D inCollider, bool inbEnabled) { inCollider.enabled = inbEnabled; }
    }
}