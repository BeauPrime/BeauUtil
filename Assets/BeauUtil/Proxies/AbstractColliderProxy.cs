/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    7 Oct 2019
 * 
 * File:    AbstractColliderProxy.cs
 * Purpose: Generic base class for collider listeners.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BeauUtil
{
    public abstract class AbstractColliderProxy<TCollider, TCollision> : MonoBehaviour where TCollider : Component
    {
        private const string TrackTooltipString = "If checked, colliders that get disabled or destroyed will still dispatch their appropriate On_Exit messages" +
            "\nUncheck if you want the default behavior.";

        #region Types

        [Serializable]
        public class CollisionEvent : UnityEvent<TCollision> { }

        [Serializable]
        public class TaggedCollisionEvent : UnityEvent<int, TCollision> { }

        [Serializable]
        public class ColliderEvent : UnityEvent<TCollider> { }

        [Serializable]
        public class TaggedColliderEvent : UnityEvent<int, TCollider> { }

        #endregion // Types

        #region Inspector

        [SerializeField] protected int m_Id = 0;
        [SerializeField] protected TCollider m_Collider = null;
        [SerializeField, Tooltip(TrackTooltipString)] protected bool m_TrackOccupants = true;

        #endregion // Inspector

        [NonSerialized] protected List<TCollider> m_Occupants = new List<TCollider>();

        #region Unity Events

        protected virtual void Awake()
        {
            if (!m_Collider)
                m_Collider = GetComponent<TCollider>();
        }

        protected virtual void OnEnable()
        {
            if (m_Collider)
                SetColliderEnabled(m_Collider, true);
        }

        protected virtual void OnDisable()
        {
            if (m_Collider)
                SetColliderEnabled(m_Collider, false);

            DiscardAllOccupants();
        }

        #endregion // Unity Events

        /// <summary>
        /// Identifier to send with tagged collision events.
        /// </summary>
        public int Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        #region Occupants

        /// <summary>
        /// Enables/disables occupant tracking.
        /// </summary>
        public void SetOccupantTracking(bool inbActive)
        {
            if (m_TrackOccupants != inbActive)
            {
                m_TrackOccupants = inbActive;
                if (!inbActive)
                    ClearOccupants();
            }
        }

        /// <summary>
        /// Ensures a certain capacity for occupant tracking.
        /// </summary>
        public void EnsureOccupantCapacity(int inCapacity)
        {
            if (m_Occupants.Capacity < inCapacity)
            {
                m_Occupants.Capacity = inCapacity;
            }
        }

        /// <summary>
        /// Returns the list of occupants.
        /// </summary>
        public IReadOnlyList<TCollider> Occupants()
        {
            return m_Occupants;
        }

        /// <summary>
        /// Starts tracking the given collider.
        /// </summary>
        protected bool AddOccupant(TCollider inCollider)
        {
            if (!m_TrackOccupants)
                return false;

            if (m_Occupants.Contains(inCollider))
                return false;

            m_Occupants.Add(inCollider);
            return true;
        }

        /// <summary>
        /// Removes the given collider from the tracking list.
        /// </summary>
        protected bool RemoveOccupant(TCollider inCollider)
        {
            if (!m_TrackOccupants)
                return false;

            return m_Occupants.Remove(inCollider);
        }

        /// <summary>
        /// Discards all deactivated occupants.
        /// </summary>
        public void ProcessOccupants()
        {
            DiscardDeactivatedOccupants();
        }

        /// <summary>
        /// Clears all occupants.
        /// </summary>
        public void ClearOccupants()
        {
            DiscardAllOccupants();
        }

        protected void DiscardDeactivatedOccupants()
        {
            for (int i = m_Occupants.Count - 1; i >= 0; --i)
            {
                TCollider occupant = m_Occupants[i];
                if (!occupant || !GetColliderEnabled(occupant) || !occupant.gameObject.activeInHierarchy)
                {
                    OnOccupantDiscarded(occupant);
                    m_Occupants.RemoveAt(i);
                }
            }
        }

        protected void DiscardAllOccupants()
        {
            for (int i = 0; i < m_Occupants.Count; ++i)
            {
                OnOccupantDiscarded(m_Occupants[i]);
            }

            m_Occupants.Clear();
        }

        protected abstract void OnOccupantDiscarded(TCollider inCollider);

        #endregion // Occupants

        #region Internal

        protected abstract bool GetColliderEnabled(TCollider inCollider);
        protected abstract void SetColliderEnabled(TCollider inCollider, bool inbEnabled);

        #endregion // Internal

        #if UNITY_EDITOR

        private void Reset()
        {
            m_Collider = GetComponent<TCollider>();
        }

        private void OnValidate()
        {
            m_Collider = GetComponent<TCollider>();
        }

        #endif // UNITY_EDITOR
    }
}