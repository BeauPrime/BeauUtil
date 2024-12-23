/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Sept 2019
 * 
 * File:    TriggerListener2D.cs
 * Purpose: Dispatches callbacks for OnTriggerEnter2D and OnTriggerExit2D messages.
 */

using UnityEngine;

namespace BeauUtil
{
    public class TriggerListener2D : ColliderProxy2D
    {
        #region Events

#if BEAUUTIL_USE_LEGACY_UNITYEVENTS
        [Header("Events")]
        [SerializeField] private ColliderEvent m_OnTriggerEnter = new ColliderEvent();
        [SerializeField] private TaggedColliderEvent m_TaggedTriggerEnter = new TaggedColliderEvent();
        [SerializeField] private ColliderEvent m_OnTriggerExit = new ColliderEvent();
        [SerializeField] private TaggedColliderEvent m_TaggedTriggerExit = new TaggedColliderEvent();
#else
        private ColliderEvent m_OnTriggerEnter = new ColliderEvent();
        private TaggedColliderEvent m_TaggedTriggerEnter = new TaggedColliderEvent();
        private ColliderEvent m_OnTriggerExit = new ColliderEvent();
        private TaggedColliderEvent m_TaggedTriggerExit = new TaggedColliderEvent();
#endif // BEAUUTIL_USE_LEGACY_UNITYEVENTS

        #endregion // Events

        public ColliderEvent onTriggerEnter { get { return m_OnTriggerEnter; } }
        public TaggedColliderEvent onTriggerEnterTagged { get { return m_TaggedTriggerEnter; } }

        public ColliderEvent onTriggerExit { get { return m_OnTriggerExit; } }
        public TaggedColliderEvent onTriggerExitTagged { get { return m_TaggedTriggerExit; } }

        private void OnTriggerEnter2D(Collider2D inCollider)
        {
            if (!CheckFilters(inCollider, ColliderProxyEventMask.OnEnter))
                return;
            
            AddOccupant(inCollider);
            m_OnTriggerEnter.Invoke(inCollider);
            m_TaggedTriggerEnter.Invoke(m_Id, inCollider);
        }

        private void OnTriggerExit2D(Collider2D inCollider)
        {
            if (!CheckFilters(inCollider, ColliderProxyEventMask.OnExit))
                return;

            RemoveOccupant(inCollider);
            m_OnTriggerExit.Invoke(inCollider);
            m_TaggedTriggerExit.Invoke(m_Id, inCollider);
        }

        protected override void OnOccupantDiscarded(Collider2D inCollider)
        {
            m_OnTriggerExit.Invoke(inCollider);
            m_TaggedTriggerEnter.Invoke(m_Id, inCollider);
        }

        protected override void SetupCollider(Collider2D inCollider)
        {
            inCollider.isTrigger = true;
        }
    }
}