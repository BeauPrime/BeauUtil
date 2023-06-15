/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    20 August 2020
 * 
 * File:    CameraFOVPlane.cs
 * Purpose: Sets a camera's FOV based on distance to a given plane.
 *          Useful for using a perspective camera for parallax.
 */

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace BeauUtil
{
    /// <summary>
    /// Sets a camera's FOV based on distance to a given target.
    /// Useful for using a perspective camera for parallax.
    /// </summary>
    [ExecuteAlways, RequireComponent(typeof(Camera))]
    [AddComponentMenu("BeauUtil/Camera FOV Plane"), DisallowMultipleComponent]
    [DefaultExecutionOrder(100000000)]
    public class CameraFOVPlane : MonoBehaviour, ICameraPreCullCallback
    {
        public struct CameraSettings
        {
            public readonly float Height;
            public readonly float Zoom;

            public readonly float Distance;
            public readonly float ZoomedHeight;
            public readonly float FieldOfView;

            public CameraSettings(float inHeight, float inZoom, float inDistance, float inZoomedHeight, float inFOV)
            {
                Height = inHeight;
                Zoom = inZoom;
                Distance = inDistance;
                ZoomedHeight = inZoomedHeight;
                FieldOfView = inFOV;
            }
        }

        #region Inspector

        [SerializeField] private Transform m_Target = null;
        [SerializeField] private float m_Height = 10;
        [SerializeField, Range(0.01f, 25)] private float m_Zoom = 1;

        #endregion // Inspector

        [NonSerialized] private Transform m_Transform;
        [NonSerialized] private Camera m_Camera;
        [NonSerialized] private float m_LastDistance = 1;

        private void OnEnable()
        {
            if (!m_Transform)
                m_Transform = transform;

            if (!m_Camera)
                m_Camera = GetComponent<Camera>();

            if (m_Camera)
                m_Camera.AddOnPreCull(this);
        }

        private void OnDisable()
        {
            if (m_Camera)
                m_Camera.RemoveOnPreCull(this);
        }

        /// <summary>
        /// Current zoom level.
        /// </summary>
        public float Zoom
        {
            get { return m_Zoom; }
            set { m_Zoom = value; }
        }

        /// <summary>
        /// Desired camera height at target.
        /// </summary>
        public float Height
        {
            get { return m_Height; }
            set { m_Height = value; }
        }

        /// <summary>
        /// Camera target. Used to establish plane for FOV.
        /// </summary>
        public Transform Target
        {
            get { return m_Target; }
            set { m_Target = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ZoomedHeight()
        {
            return ZoomedHeight(m_Zoom);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ZoomedHeight(float inZoom)
        {
            return Mathf.Clamp(m_Height / inZoom, 0.01f, 10000f);
        }

        /// <summary>
        /// Sets the current target while preserving the current field of view.
        /// </summary>
        public void SetTargetPreserveFOV(Transform inTarget)
        {
            if (inTarget == null)
            {
                m_Target = null;
                return;
            }

            m_Target = inTarget;
            float distanceForFOV = CameraHelper.DistanceForHeightAndFOV(ZoomedHeight(), m_Camera.fieldOfView);
            float newDist;
            m_Camera.TryGetDistanceToObjectPlane(inTarget, out newDist);
            m_Zoom = m_Zoom * distanceForFOV / newDist;
			m_LastDistance = newDist;
        }

        /// <summary>
        /// Returns the current settings.
        /// </summary>
        public void GetSettings(out CameraSettings outSettings)
        {
            outSettings = new CameraSettings(Height, Zoom, m_LastDistance, ZoomedHeight(), m_Camera.fieldOfView);
        }

        private void Apply()
        {
            float newDist;
            bool bHit = m_Camera.TryGetDistanceToObjectPlane(m_Target, out newDist);

            if (!bHit)
            {
                newDist = m_LastDistance;
            }
            else
            {
                m_LastDistance = newDist;
            }

            float height = ZoomedHeight();
            float fov = 2.0f * Mathf.Atan(height * 0.5f / newDist) * Mathf.Rad2Deg;
            m_Camera.fieldOfView = fov;
        }

        void ICameraPreCullCallback.OnCameraPreCull(Camera inCamera, CameraCallbackSource inSource)
        {
            Apply();
        }
    }
}