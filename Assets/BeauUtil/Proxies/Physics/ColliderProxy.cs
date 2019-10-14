/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    30 Sept 2019
 * 
 * File:    ColliderProxy.cs
 * Purpose: Base class for collider listeners.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BeauUtil
{
    public abstract class ColliderProxy : AbstractColliderProxy<Collider, Collision>
    {
        protected override bool GetColliderEnabled(Collider inCollider) { return inCollider.enabled; }
        protected override void SetColliderEnabled(Collider inCollider, bool inbEnabled) { inCollider.enabled = inbEnabled; }
    }
}