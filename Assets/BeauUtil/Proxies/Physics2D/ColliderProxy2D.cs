/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
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
    public abstract class ColliderProxy2D : AbstractColliderProxy<Collider2D, Collision2D, Rigidbody2D>
    {
        protected override bool GetColliderEnabled(Collider2D inCollider) { return inCollider.enabled && inCollider.gameObject.activeInHierarchy; }
        protected override void SetColliderEnabled(Collider2D inCollider, bool inbEnabled) { inCollider.enabled = inbEnabled; }
        protected override Rigidbody2D GetRigidbodyForCollider(Collider2D inCollider) { return inCollider.attachedRigidbody; }

        protected override bool GetRigidbodyEnabled(Rigidbody2D inRigidbody)
        {
            return inRigidbody.gameObject.activeInHierarchy;
        }
    }
}