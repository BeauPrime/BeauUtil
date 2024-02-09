/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Sept 2019
 * 
 * File:    CollisionListener2D.cs
 * Purpose: Dispatches callbacks for OnCollisionEnter2D and OnCollisionExit2D messages.
 */

using System;
using UnityEngine;
using UnityEngine.Events;

namespace BeauUtil
{
    public class CollisionListener2D : ColliderProxy2D
    {
        #region Events

        private readonly CollisionEvent m_OnCollisionEnter = new CollisionEvent();
        private readonly TaggedCollisionEvent m_TaggedCollisionEnter = new TaggedCollisionEvent();
        private readonly CollisionEvent m_OnCollisionExit = new CollisionEvent();
        private readonly TaggedCollisionEvent m_TaggedCollisionExit = new TaggedCollisionEvent();
        private readonly ColliderEvent m_OnCollisionCancel = new ColliderEvent();
        private readonly TaggedColliderEvent m_TaggedCollisionCancel = new TaggedColliderEvent();

        #endregion // Events

        public CollisionEvent onCollisionEnter { get { return m_OnCollisionEnter; } }
        public TaggedCollisionEvent onCollisionEnterTagged { get { return m_TaggedCollisionEnter; } }

        public CollisionEvent onCollisionExit { get { return m_OnCollisionExit; } }
        public TaggedCollisionEvent onCollisionExitTagged { get { return m_TaggedCollisionExit; } }

        public ColliderEvent onCollisionCancel { get { return m_OnCollisionCancel; } }
        public TaggedColliderEvent onCollisionCancelTagged { get { return m_TaggedCollisionCancel; } }

        private void OnCollisionEnter2D(Collision2D inCollision)
        {
            if (!CheckFilters(inCollision.collider, ColliderProxyEventMask.OnEnter))
                return;

            AddOccupant(inCollision.collider);
            m_OnCollisionEnter.Invoke(inCollision);
            m_TaggedCollisionEnter.Invoke(m_Id, inCollision);
        }

        private void OnCollisionExit2D(Collision2D inCollision)
        {
            if (!CheckFilters(inCollision.collider, ColliderProxyEventMask.OnExit))
                return;

            RemoveOccupant(inCollision.collider);
            m_OnCollisionExit.Invoke(inCollision);
            m_TaggedCollisionExit.Invoke(m_Id, inCollision);
        }

        protected override void OnOccupantDiscarded(Collider2D inCollider)
        {
            m_OnCollisionCancel.Invoke(inCollider);
            m_TaggedCollisionCancel.Invoke(m_Id, inCollider);
        }

        protected override void SetupCollider(Collider2D inCollider)
        {
            inCollider.isTrigger = false;
        }
    }
}