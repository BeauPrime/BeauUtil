/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    9 Apr 2021
 * 
 * File:    ConsoleCamera.cs
 * Purpose: Manual camera adjustment mode.
 */

 #if UNITY_2019_1_OR_NEWER
#define USE_SRP
#endif // UNITY_2019_1_OR_NEWER

using UnityEngine;
using System;

#if USE_SRP
using UnityEngine.Rendering;
#endif // UNITY_2019

namespace BeauUtil.Debugger
{
    public class ConsoleCamera : MonoBehaviour
    {
        private struct CameraState
        {
            public Vector3 Position;
            public Vector3 Rotation;

            public bool OrthoMode;
            public float OrthoSize;
            public float FOV;
        }

        #region Inspector

        [SerializeField] private float m_MoveSpeed = 5;
        [SerializeField] private float m_RotateSpeed = 5;
        [SerializeField, EditModeOnly] private bool m_ResetCameraPositionEveryFrame = false;

        #endregion // Inspector

        [NonSerialized] private Camera m_CurrentCamera;
        [NonSerialized] private Transform m_CurrentCameraTransform;
        [NonSerialized] private bool m_CallbacksRegistered;

        private CameraState m_ResetCameraState;
        private CameraState m_DebugCameraState;

        private void OnDestroy()
        {
            DeregisterCallbacks();
        }

        #region Actions

        /// <summary>
        /// Movement speed multiplier.
        /// </summary>
        [NonSerialized] public float MoveSpeedMultiplier = 1;

        /// <summary>
        /// Rotate speed multiplier.
        /// </summary>
        [NonSerialized] public float RotateSpeedMultiplier = 1;

        /// <summary>
        /// Current camera being debugged.
        /// </summary>
        public Camera Camera() { return m_CurrentCamera; }

        /// <summary>
        /// Sets the camera being debugged.
        /// </summary>
        public bool SetCamera(Camera inCamera)
        {
            return SetCamera(inCamera, null);
        }

        /// <summary>
        /// Sets the camera being debugged.
        /// </summary>
        public bool SetCamera(Camera inCamera, Transform inCameraTransform)
        {
            if (inCameraTransform.IsReferenceNull() && !inCamera.IsReferenceNull())
                inCameraTransform = inCamera.transform;

            if (m_CurrentCamera.IsReferenceEquals(inCamera) && m_CurrentCameraTransform.IsReferenceEquals(inCameraTransform))
                return false;

            if (m_CurrentCamera)
            {
                WriteState(m_CurrentCamera, m_CurrentCameraTransform, m_ResetCameraState);
            }

            m_CurrentCamera = inCamera;
            m_CurrentCameraTransform = inCameraTransform;

            if (inCamera)
            {
                RegisterCallbacks();
                ReadState(inCamera, inCameraTransform, ref m_ResetCameraState);
                m_DebugCameraState = m_ResetCameraState;
            }
            else
            {
                DeregisterCallbacks();
            }

            return true;
        }

        /// <summary>
        /// Moves the camera relative to its look direction.
        /// </summary>
        public void MoveRelative(Vector3 inRelativeMovement)
        {
            float cameraSpeed = m_MoveSpeed * MoveSpeedMultiplier;
            Vector3 forward = Quaternion.Euler(m_DebugCameraState.Rotation) * inRelativeMovement;
            forward *= cameraSpeed;
            m_DebugCameraState.Position += forward;
        }

        /// <summary>
        /// Moves the camera in world space.
        /// </summary>
        public void MoveAbsolute(Vector3 inAbsolute)
        {
            float cameraSpeed = m_MoveSpeed * MoveSpeedMultiplier;
            m_DebugCameraState.Position += inAbsolute * cameraSpeed;
        }

        /// <summary>
        /// Rotates the camera in world space.
        /// </summary>
        public void Rotate(Vector3 inRotateDegrees)
        {
            float cameraSpeed = m_RotateSpeed * RotateSpeedMultiplier;
            m_DebugCameraState.Rotation += inRotateDegrees * cameraSpeed;
        }

        /// <summary>
        /// Resets the camera to its original position.
        /// </summary>
        public void ResetCamera()
        {
            m_DebugCameraState = m_ResetCameraState;
        }

        /// <summary>
        /// Whether or not the camera is orthographic.
        /// </summary>
        public bool OrthoMode
        {
            get { return m_DebugCameraState.OrthoMode; }
            set { m_DebugCameraState.OrthoMode = value; }
        }

        /// <summary>
        /// Field of view.
        /// </summary>
        public float FOV
        {
            get { return m_DebugCameraState.FOV; }
            set { m_DebugCameraState.FOV = value; }
        }

        /// <summary>
        /// Orthographic size.
        /// </summary>
        public float OrthoSize
        {
            get { return m_DebugCameraState.OrthoSize; }
            set { m_DebugCameraState.OrthoSize = value; }
        }

        #endregion // Actions

        #region Camera Callbacks

        private void RegisterCallbacks()
        {
            if (m_CallbacksRegistered)
                return;

            UnityEngine.Camera.onPreRender += OnCameraPreRender;
            if (m_ResetCameraPositionEveryFrame)
                UnityEngine.Camera.onPostRender += OnCameraPostRender;

            #if USE_SRP
            RenderPipelineManager.beginCameraRendering += OnRenderPipelineBeginCamera;
            if (m_ResetCameraPositionEveryFrame)
                RenderPipelineManager.endCameraRendering += OnRenderPipelineEndCamera;
            #endif // USE_SRP

            m_CallbacksRegistered = true;
        }

        private void DeregisterCallbacks()
        {
            if (!m_CallbacksRegistered)
                return;

            UnityEngine.Camera.onPreRender -= OnCameraPreRender;
            if (m_ResetCameraPositionEveryFrame)
                UnityEngine.Camera.onPostRender -= OnCameraPostRender;

            #if USE_SRP
            RenderPipelineManager.beginCameraRendering -= OnRenderPipelineBeginCamera;
            if (m_ResetCameraPositionEveryFrame)
                RenderPipelineManager.endCameraRendering -= OnRenderPipelineEndCamera;
            #endif // USE_SRP

            m_CallbacksRegistered = false;
        }

        private void OnCameraPreRender(Camera inCamera)
        {
            if (inCamera == m_CurrentCamera)
            {
                if (m_ResetCameraPositionEveryFrame)
                    ReadState(m_CurrentCamera, m_CurrentCameraTransform, ref m_ResetCameraState);
                
                WriteState(m_CurrentCamera, m_CurrentCameraTransform, m_DebugCameraState);
            }
        }

        private void OnCameraPostRender(Camera inCamera)
        {
            if (inCamera == m_CurrentCamera)
            {
                WriteState(m_CurrentCamera, m_CurrentCameraTransform, m_ResetCameraState);
            }
        }

        #if USE_SRP

        private void OnRenderPipelineBeginCamera(ScriptableRenderContext context, Camera camera)
        {
            if (camera == m_CurrentCamera)
            {
                if (m_ResetCameraPositionEveryFrame)
                    ReadState(m_CurrentCamera, m_CurrentCameraTransform, ref m_ResetCameraState);

                WriteState(m_CurrentCamera, m_CurrentCameraTransform, m_DebugCameraState);
            }
        }

        private void OnRenderPipelineEndCamera(ScriptableRenderContext context, Camera camera)
        {
            if (camera == m_CurrentCamera)
            {
                WriteState(m_CurrentCamera, m_CurrentCameraTransform, m_ResetCameraState);
            }
        }

        #endif // USE_SRP

        #endregion // Camera Callbacks

        #region Camera State

        static private void WriteState(Camera ioCamera, Transform ioTransform, CameraState inState)
        {
            bool bTransformChangeState = ioTransform.hasChanged;
            ioTransform.SetPositionAndRotation(inState.Position, Quaternion.Euler(inState.Rotation));
            ioTransform.hasChanged = bTransformChangeState;

            ioCamera.orthographic = inState.OrthoMode;
            ioCamera.orthographicSize = inState.OrthoSize;
            ioCamera.fieldOfView = inState.FOV;
        }

        static private void ReadState(Camera inCamera, Transform inTransform, ref CameraState outState)
        {
            outState.Position = inTransform.position;
            outState.Rotation = inTransform.eulerAngles;

            outState.OrthoMode = inCamera.orthographic;
            outState.OrthoSize = inCamera.orthographicSize;
            outState.FOV = inCamera.fieldOfView;
        }
    
        #endregion // Camera State
    }
}