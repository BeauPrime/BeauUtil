/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    30 Sept 2019
 * 
 * File:    CollisionListener.cs
 * Purpose: Dispatches callbacks for OnCollisionEnter and OnCollisionExit messages.
 */

using System;
using UnityEngine;
using UnityEngine.Events;

namespace BeauUtil
{
    public class CollisionListener : ColliderProxy
    {
        #region Inspector

        [SerializeField] private CollisionEvent m_OnCollisionEnter = new CollisionEvent();
        [SerializeField] private TaggedCollisionEvent m_TaggedCollisionEnter = new TaggedCollisionEvent();
        [SerializeField] private CollisionEvent m_OnCollisionExit = new CollisionEvent();
        [SerializeField] private TaggedCollisionEvent m_TaggedCollisionExit = new TaggedCollisionEvent();
        [SerializeField] private ColliderEvent m_OnCollisionCancel = new ColliderEvent();
        [SerializeField] private TaggedColliderEvent m_TaggedCollisionCancel = new TaggedColliderEvent();

        #endregion // Inspector

        public CollisionEvent onCollisionEnter { get { return m_OnCollisionEnter; } }
        public TaggedCollisionEvent onCollisionEnterTagged { get { return m_TaggedCollisionEnter; } }

        public CollisionEvent onCollisionExit { get { return m_OnCollisionExit; } }
        public TaggedCollisionEvent onCollisionExitTagged { get { return m_TaggedCollisionExit; } }

        public ColliderEvent onCollisionCancel { get { return m_OnCollisionCancel; } }
        public TaggedColliderEvent onCollisionCancelTagged { get { return m_TaggedCollisionCancel; } }

        private void OnCollisionEnter(Collision inCollision)
        {
            AddOccupant(inCollision.collider);
            m_OnCollisionEnter.Invoke(inCollision);
            m_TaggedCollisionEnter.Invoke(m_Id, inCollision);
        }

        private void OnCollisionExit(Collision inCollision)
        {
            RemoveOccupant(inCollision.collider);
            m_OnCollisionExit.Invoke(inCollision);
            m_TaggedCollisionExit.Invoke(m_Id, inCollision);
        }

        protected override void OnOccupantDiscarded(Collider inCollider)
        {
            m_OnCollisionCancel.Invoke(inCollider);
            m_TaggedCollisionCancel.Invoke(m_Id, inCollider);
        }
    }
}