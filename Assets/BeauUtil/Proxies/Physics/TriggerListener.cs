/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Sept 2019
 * 
 * File:    TriggerListener.cs
 * Purpose: Dispatches callbacks for OnTriggerEnter and OnTriggerExit messages.
 */

using UnityEngine;

namespace BeauUtil
{
    public class TriggerListener : ColliderProxy
    {
        #region Inspector

        [Header("Events")]
        [SerializeField] private ColliderEvent m_OnTriggerEnter = new ColliderEvent();
        [SerializeField] private TaggedColliderEvent m_TaggedTriggerEnter = new TaggedColliderEvent();
        [SerializeField] private ColliderEvent m_OnTriggerExit = new ColliderEvent();
        [SerializeField] private TaggedColliderEvent m_TaggedTriggerExit = new TaggedColliderEvent();

        #endregion // Inspector

        public ColliderEvent onTriggerEnter { get { return m_OnTriggerEnter; } }
        public TaggedColliderEvent onTriggerEnterTagged { get { return m_TaggedTriggerEnter; } }

        public ColliderEvent onTriggerExit { get { return m_OnTriggerExit; } }
        public TaggedColliderEvent onTriggerExitTagged { get { return m_TaggedTriggerExit; } }

        private void OnTriggerEnter(Collider inCollider)
        {
            if (!CheckFilters(inCollider, ColliderProxyEventMask.OnEnter))
                return;

            AddOccupant(inCollider);
            m_OnTriggerEnter.Invoke(inCollider);
            m_TaggedTriggerEnter.Invoke(m_Id, inCollider);
        }

        private void OnTriggerExit(Collider inCollider)
        {
            if (!CheckFilters(inCollider, ColliderProxyEventMask.OnExit))
                return;

            RemoveOccupant(inCollider);
            m_OnTriggerExit.Invoke(inCollider);
            m_TaggedTriggerExit.Invoke(m_Id, inCollider);
        }

        protected override void OnOccupantDiscarded(Collider inCollider)
        {
            m_OnTriggerExit.Invoke(inCollider);
            m_TaggedTriggerExit.Invoke(m_Id, inCollider);
        }

        protected override void SetupCollider(Collider inCollider)
        {
            inCollider.isTrigger = true;
        }
    }
}