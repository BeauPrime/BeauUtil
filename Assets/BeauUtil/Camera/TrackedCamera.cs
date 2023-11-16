/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 July 2020
 * 
 * File:    TrackedCamera.cs
 * Purpose: Tracks camera updates.
 */

using System;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Tracks camera changes.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public sealed class TrackedCamera : MonoBehaviour, IUpdateVersioned, IStateHash
    {
        [NonSerialized]
        private Camera m_Camera = null;

        [NonSerialized]
        private int m_UpdateSerial = 1;

        [NonSerialized] private ulong m_LastHash;

        int IUpdateVersioned.GetUpdateVersion()
        {
            if (ReferenceEquals(m_Camera, null))
                m_Camera = GetComponent<Camera>();

            ulong hash = m_Camera.GetStateHash();

            if (m_LastHash != hash)
            {
                if (m_UpdateSerial == int.MaxValue)
                    m_UpdateSerial = 1;
                else
                    ++m_UpdateSerial;

                m_LastHash = hash;
            }

            return m_UpdateSerial;
        }

        ulong IStateHash.GetStateHash()
        {
            if (ReferenceEquals(m_Camera, null))
                m_Camera = GetComponent<Camera>();

            return m_Camera.GetStateHash();
        }

        /// <summary>
        /// Suppresses any recent changes from impacting the transform serial.
        /// </summary>
        public void SuppressChanges()
        {
            if (ReferenceEquals(m_Camera, null))
                m_Camera = GetComponent<Camera>();

            m_LastHash = m_Camera.GetStateHash();
        }

        /// <summary>
        /// Locates the TrackedCamera for the given camera.
        /// </summary>
        static public TrackedCamera Get(Camera inCamera)
        {
            TrackedCamera tracker = inCamera.GetComponent<TrackedCamera>();
            if (!tracker)
            {
                tracker = inCamera.gameObject.AddComponent<TrackedCamera>();
                tracker.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
            }
            return tracker;
        }
    }
}