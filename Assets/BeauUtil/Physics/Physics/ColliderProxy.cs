/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
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
    public abstract class ColliderProxy : AbstractColliderProxy<Collider, Collision, Rigidbody>
    {
        protected override bool GetColliderEnabled(Collider inCollider) { return inCollider.enabled && inCollider.gameObject.activeInHierarchy; }
        protected override void SetColliderEnabled(Collider inCollider, bool inbEnabled) { inCollider.enabled = inbEnabled; }
        protected override Rigidbody GetRigidbodyForCollider(Collider inCollider) { return inCollider.attachedRigidbody; }

        protected override bool GetRigidbodyEnabled(Rigidbody inRigidbody)
        {
            return inRigidbody.gameObject.activeInHierarchy;
        }
    }
}