/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
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
    public abstract class AbstractColliderProxy<TCollider, TCollision, TRigidbody> : MonoBehaviour
        where TCollider : Component
        where TRigidbody : Component
    {
        private const string TrackTooltipString = "If checked, colliders that get disabled or destroyed will still dispatch their appropriate On_Exit messages" +
            "\nUncheck if you want the default behavior.";
        static protected readonly string UntaggedTag = "Untagged";

        /// <summary>
        /// Object occupying this collider proxy.
        /// </summary>
        public struct Occupant
        {
            public readonly TCollider Collider;
            public readonly TRigidbody Rigidbody;

            internal Occupant(TCollider inCollider, TRigidbody inRigidbody)
            {
                Collider = inCollider;
                Rigidbody = inRigidbody;
            }
        }

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

        [Header("Filtering")]
        [SerializeField, AutoEnum] protected ColliderProxyEventMask m_FilterEventMask = ColliderProxyEventMask.OnEnterAndExit;
        [SerializeField] protected LayerMask m_LayerMaskFilter = 0;
        [SerializeField, UnityTag] protected List<string> m_CompareTagFilters = new List<string>();
        [SerializeField] protected string m_NameFilter = null;

        #endregion // Inspector

        [NonSerialized] protected WildcardMatch m_NameFilterMatch = default(WildcardMatch);
        [NonSerialized] protected readonly List<Occupant> m_Occupants = new List<Occupant>();
        [NonSerialized] protected Type m_RequiredComponentType = null;
        [NonSerialized] protected ComponentLookupDirection m_RequiredComponentLookup;

        #region Unity Events

        protected virtual void Awake()
        {
            if (!m_Collider)
            {
                m_Collider = GetComponent<TCollider>();
                if (m_Collider)
                    SetupCollider(m_Collider);
            }

            if (!string.IsNullOrEmpty(m_NameFilter))
            {
                m_NameFilterMatch = WildcardMatch.Compile(m_NameFilter);
            }
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

        #region Filter

        /// <summary>
        /// Mask indicating which collision events will filter colliders.
        /// </summary>
        public ColliderProxyEventMask FilterEventMask
        {
            get { return m_FilterEventMask; }
            set { m_FilterEventMask = value; }
        }

        /// <summary>
        /// Filter by GameObject layer.
        /// </summary>
        public LayerMask LayerFilter
        {
            get { return m_LayerMaskFilter; }
            set { m_LayerMaskFilter = value; }
        }

        /// <summary>
        /// Filter by GameObject tag.
        /// </summary>
        public ICollection<string> TagFilter
        {
            get { return m_CompareTagFilters; }
        }

        /// <summary>
        /// Filter by GameObject name.
        /// Supports wildcards.
        /// </summary>
        public string NameFilter
        {
            get { return m_NameFilter; }
            set
            {
                if (m_NameFilter != value)
                {
                    m_NameFilter = value;
                    m_NameFilterMatch = WildcardMatch.Compile(m_NameFilter);
                }
            }
        }

        /// <summary>
        /// Filter by a component.
        /// </summary>
        public Type ComponentFilter
        {
            get { return m_RequiredComponentType; }
        }

        /// <summary>
        /// Component filter lookup direction.
        /// </summary>
        public ComponentLookupDirection ComponentFilterDirection
        {
            get { return m_RequiredComponentLookup; }
        }

        /// <summary>
        /// Sets the component filter.
        /// </summary>
        public void FilterByComponent(Type inType, ComponentLookupDirection inLookup = ComponentLookupDirection.Self)
        {
            m_RequiredComponentType = inType;
            m_RequiredComponentLookup = inLookup;
        }

        /// <summary>
        /// Sets the component filter.
        /// </summary>
        public void FilterByComponent<T>(ComponentLookupDirection inLookup = ComponentLookupDirection.Self)
        {
            FilterByComponent(typeof(T), inLookup);
        }

        /// <summary>
        /// Sets the component filter.
        /// </summary>
        public void FilterByComponentInParent<T>()
        {
            FilterByComponent(typeof(T), ComponentLookupDirection.Parent);
        }

        /// <summary>
        /// Sets the component filter.
        /// </summary>
        public void FilterByComponentInChildren<T>()
        {
            FilterByComponent(typeof(T), ComponentLookupDirection.Children);
        }

        /// <summary>
        /// Clears the component filter.
        /// </summary>
        public void ClearComponentFilter()
        {
            FilterByComponent(null);
        }

        #endregion // Filter

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
        public IReadOnlyList<Occupant> Occupants()
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

            for (int i = m_Occupants.Count - 1; i >= 0; --i)
            {
                if (m_Occupants[i].Collider.IsReferenceEquals(inCollider))
                    return false;
            }

            m_Occupants.Add(new Occupant(inCollider, GetRigidbodyForCollider(inCollider)));
            return true;
        }

        /// <summary>
        /// Removes the given collider from the tracking list.
        /// </summary>
        protected bool RemoveOccupant(TCollider inCollider)
        {
            if (!m_TrackOccupants)
                return false;

            for (int i = m_Occupants.Count - 1; i >= 0; --i)
            {
                if (m_Occupants[i].Collider.IsReferenceEquals(inCollider))
                {
                    m_Occupants.FastRemoveAt(i);
                    return true;
                }
            }
            
            return false;
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
            Occupant occupant;
            TCollider collider;
            TRigidbody rigidbody;
            for (int i = m_Occupants.Count - 1; i >= 0; --i)
            {
                occupant = m_Occupants[i];
                collider = occupant.Collider;
                rigidbody = occupant.Rigidbody;

                if (!collider || !GetColliderEnabled(collider) ||
                    GetRigidbodyForCollider(collider) != rigidbody ||
                    !CheckFiltersFast(collider, ColliderProxyEventMask.OnIdle) ||
                    (!rigidbody.IsReferenceNull() && (!rigidbody || !GetRigidbodyEnabled(rigidbody))))
                {
                    OnOccupantDiscarded(collider);
                    m_Occupants.FastRemoveAt(i);
                }
            }
        }

        protected void DiscardAllOccupants()
        {
            for (int i = 0; i < m_Occupants.Count; ++i)
            {
                OnOccupantDiscarded(m_Occupants[i].Collider);
            }

            m_Occupants.Clear();
        }

        protected bool CheckFiltersFast(TCollider inCollider, ColliderProxyEventMask inEvent)
        {
            if ((m_FilterEventMask & inEvent) == 0)
                return true;

            GameObject go = inCollider.gameObject;

            // check layer
            if (m_LayerMaskFilter != 0 && ((1 << go.layer) & m_LayerMaskFilter) == 0)
                return false;

            // check tags
            if (m_CompareTagFilters != null && m_CompareTagFilters.Count > 0)
            {
                bool bFound = false;
                for(int i = 0; i < m_CompareTagFilters.Count; ++i)
                {
                    if (go.CompareTag(m_CompareTagFilters[i]))
                    {
                        bFound = true;
                        break;
                    }
                }
                
                if (!bFound)
                    return false;
            }

            return true;
        }

        protected bool CheckFilters(TCollider inCollider, ColliderProxyEventMask inEvent)
        {
            if ((m_FilterEventMask & inEvent) == 0)
                return true;

            GameObject go = inCollider.gameObject;

            // check layer
            if (m_LayerMaskFilter != 0 && ((1 << go.layer) & m_LayerMaskFilter) == 0)
                return false;

            // check tags
            if (m_CompareTagFilters != null && m_CompareTagFilters.Count > 0)
            {
                bool bFound = false;
                for(int i = 0; i < m_CompareTagFilters.Count; ++i)
                {
                    if (go.CompareTag(m_CompareTagFilters[i]))
                    {
                        bFound = true;
                        break;
                    }
                }
                
                if (!bFound)
                    return false;
            }

            // check name
            if (!m_NameFilterMatch.IsEmpty && !m_NameFilterMatch.Match(go.name))
                return false;

            // check component type
            if (m_RequiredComponentType != null)
            {
                switch(m_RequiredComponentLookup)
                {
                    case ComponentLookupDirection.Self:
                        {
                            if (!go.GetComponent(m_RequiredComponentType))
                                return false;
                            break;
                        }

                    case ComponentLookupDirection.Parent:
                        {
                            if (!go.GetComponentInParent(m_RequiredComponentType))
                                return false;
                            break;
                        }

                    case ComponentLookupDirection.Children:
                        {
                            if (!go.GetComponentInChildren(m_RequiredComponentType))
                                return false;
                            break;
                        }
                }
            }

            return true;
        }

        protected abstract void OnOccupantDiscarded(TCollider inCollider);

        #endregion // Occupants

        #region Internal

        protected abstract bool GetColliderEnabled(TCollider inCollider);
        protected abstract void SetColliderEnabled(TCollider inCollider, bool inbEnabled);
        protected abstract TRigidbody GetRigidbodyForCollider(TCollider inCollider);

        protected abstract bool GetRigidbodyEnabled(TRigidbody inRigidbody);
        protected abstract void SetupCollider(TCollider inCollider);

        #endregion // Internal

        #if UNITY_EDITOR

        private void Reset()
        {
            m_Collider = GetComponent<TCollider>();
            if (m_Collider)
                SetupCollider(m_Collider);
        }

        private void OnValidate()
        {
            m_Collider = GetComponent<TCollider>();
            if (m_Collider)
                SetupCollider(m_Collider);
        }

        #endif // UNITY_EDITOR
    }
}