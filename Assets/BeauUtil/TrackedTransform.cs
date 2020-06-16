/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    TrackedTransform.cs
 * Purpose: Tracks transform updates.
 */

using System;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Tracks transform changes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrackedTransform : MonoBehaviour, IUpdateVersioned
    {
        [NonSerialized]
        private Transform m_Transform = null;

        [NonSerialized]
        private int m_UpdateSerial = 1;

        int IUpdateVersioned.GetUpdateVersion()
        {
            if (ReferenceEquals(m_Transform, null))
                m_Transform = transform;

            if (m_Transform.hasChanged)
            {
                m_Transform.hasChanged = false;
                if (m_UpdateSerial == int.MaxValue)
                    m_UpdateSerial = 1;
                else
                    ++m_UpdateSerial;
            }

            return m_UpdateSerial;
        }

        /// <summary>
        /// Suppresses any recent changes from impacting the transform serial.
        /// </summary>
        public void SuppressChanges()
        {
            if (ReferenceEquals(m_Transform, null))
                m_Transform = transform;

            if (m_Transform.hasChanged)
                m_Transform.hasChanged = false;
        }

        /// <summary>
        /// Locates the TrackedTransform for the given transform.
        /// </summary>
        static public TrackedTransform Get(Transform inTransform)
        {
            TrackedTransform tracker = inTransform.GetComponent<TrackedTransform>();
            if (!tracker)
                tracker = inTransform.gameObject.AddComponent<TrackedTransform>();
            return tracker;
        }
    }
}