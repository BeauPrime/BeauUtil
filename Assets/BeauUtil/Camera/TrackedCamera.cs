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
    public sealed class TrackedCamera : MonoBehaviour, IUpdateVersioned
    {
        [NonSerialized]
        private Camera m_Camera = null;

        [NonSerialized]
        private Transform m_Transform = null;

        [NonSerialized]
        private int m_UpdateSerial = 1;

        [NonSerialized] private float m_LastCameraAspect = 0;
        [NonSerialized] private float m_LastOrthographicSize = 0;
        [NonSerialized] private float m_LastFOV = 0;
        [NonSerialized] private Rect m_LastRect = default(Rect);
        [NonSerialized] private bool m_LastOrtho = false;
        [NonSerialized] private Vector2 m_LastScreenSize = default(Vector2);

        int IUpdateVersioned.GetUpdateVersion()
        {
            if (ReferenceEquals(m_Camera, null))
                m_Camera = GetComponent<Camera>();

            if (ReferenceEquals(m_Transform, null))
                m_Transform = transform;

            bool bCameraChanged = m_Camera.orthographic != m_LastOrtho
                || m_Camera.aspect != m_LastCameraAspect
                || m_Camera.orthographicSize != m_LastOrthographicSize
                || m_Camera.fieldOfView != m_LastFOV
                || m_Camera.rect != m_LastRect
                || Screen.width != m_LastScreenSize.x
                || Screen.height != m_LastScreenSize.y;

            if (bCameraChanged || m_Transform.hasChanged)
            {
                m_Transform.hasChanged = false;
                if (m_UpdateSerial == int.MaxValue)
                    m_UpdateSerial = 1;
                else
                    ++m_UpdateSerial;

                m_LastCameraAspect = m_Camera.aspect;
                m_LastOrthographicSize = m_Camera.orthographicSize;
                m_LastFOV = m_Camera.fieldOfView;
                m_LastRect = m_Camera.rect;
                m_LastOrtho = m_Camera.orthographic;
                m_LastScreenSize.Set(Screen.width, Screen.height);
            }

            return m_UpdateSerial;
        }

        /// <summary>
        /// Suppresses any recent changes from impacting the transform serial.
        /// </summary>
        public void SuppressChanges()
        {
            if (ReferenceEquals(m_Camera, null))
                m_Camera = GetComponent<Camera>();

            if (ReferenceEquals(m_Transform, null))
                m_Transform = transform;

            if (m_Transform.hasChanged)
                m_Transform.hasChanged = false;

            m_LastCameraAspect = m_Camera.aspect;
            m_LastOrthographicSize = m_Camera.orthographicSize;
            m_LastFOV = m_Camera.fieldOfView;
            m_LastRect = m_Camera.rect;
            m_LastOrtho = m_Camera.orthographic;
            m_LastScreenSize.Set(Screen.width, Screen.height);
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