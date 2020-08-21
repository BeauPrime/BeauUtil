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
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Sets a camera's FOV based on distance to a given target.
    /// Useful for using a perspective camera for parallax.
    /// </summary>
    [ExecuteAlways, RequireComponent(typeof(Camera))]
    [AddComponentMenu("BeauUtil/Camera FOV Plane"), DisallowMultipleComponent]
    public class CameraFOVPlane : MonoBehaviour
    {
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

        public float ZoomedHeight()
        {
            return ZoomedHeight(m_Zoom);
        }

        public float ZoomedHeight(float inZoom)
        {
            return Mathf.Clamp(m_Height / inZoom, 0.01f, 10000f);
        }

        private void OnPreCull()
        {
            float newDist;
            bool bHit = CalculateDistanceToPlane(out newDist);

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
            if (fov != m_Camera.fieldOfView)
            {
                m_Camera.fieldOfView = fov;
            }
        }

        private bool CalculateDistanceToPlane(out float outDistance)
        {
            if (!m_Target)
            {
                outDistance = 0;
                return false;
            }

            Plane p = new Plane(-m_Transform.forward, m_Target.position);
            Ray r = m_Camera.ViewportPointToRay(s_CenterViewportPoint);

            return p.Raycast(r, out outDistance);
        }

        static private readonly Vector3 s_CenterViewportPoint = new Vector3(0.5f, 0.5f, 1);
    }
}