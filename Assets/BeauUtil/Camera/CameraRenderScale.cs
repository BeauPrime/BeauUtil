/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    12 May 2022
 * 
 * File:    CameraRenderScale.cs
 * Purpose: Sets a camera's rendering scale.
 */

#if UNITY_2019_1_OR_NEWER
#define USE_SRP
#endif // UNITY_2019_1_OR_NEWER

using System;
using UnityEngine;
using UnityEngine.Events;

#if USE_SRP
using UnityEngine.Rendering;
#endif // USE_SRP

#if USING_URP
using UnityEngine.Rendering.Universal;
#endif // USING_URP

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace BeauUtil
{
    /// <summary>
    /// Sets a camera's rendering scale.
    /// </summary>
    [ExecuteAlways, RequireComponent(typeof(Camera))]
    [AddComponentMenu("BeauUtil/Camera Render Scale"), DisallowMultipleComponent]
    public class CameraRenderScale : MonoBehaviour, ICameraPreRenderCallback, ICameraPostRenderCallback
    {
        public enum ScaleMode
        {
            Scale,
            PixelHeight
        }

        #region Inspector

        [SerializeField, HideInInspector] private Camera m_Camera = null;
        [SerializeField, Tooltip("Indicates whether to specify scale directly, or as a factor of a desired resolution height")] private ScaleMode m_Mode = ScaleMode.Scale;
        [SerializeField, Tooltip("Render texture scale.")] private float m_Scale = 1;
        [SerializeField, Tooltip("Desired resolution height, in pixels.")] private int m_PixelHeight = 360;
        [SerializeField, Tooltip("Filter mode use when upscaling")] private FilterMode m_FilterMode = FilterMode.Bilinear;

        #endregion // Inspector

        [NonSerialized] private Rect m_OriginalCameraRect;
        [NonSerialized] private CameraCallbackSource m_AppliedScale = CameraCallbackSource.None;

        /// <summary>
        /// Scaling mode.
        /// </summary>
        public ScaleMode Mode
        {
            get { return m_Mode; }
            set { m_Mode = value; }
        }

        /// <summary>
        /// Camera resolution scale.
        /// </summary>
        public float Scale
        {
            get { return m_Scale; }
            set { m_Scale = Mathf.Clamp(value, 0.1f, 2f); }
        }

        /// <summary>
        /// Desired pixel height.
        /// </summary>
        public int PixelHeight
        {
            get { return m_PixelHeight; }
            set { m_PixelHeight = Mathf.Clamp(value, 1, SystemInfo.maxTextureSize); }
        }

        #region Unity Events

        private void Awake()
        {
            m_Camera = GetComponent<Camera>();
            m_OriginalCameraRect = m_Camera.rect;
        }

        private void OnEnable()
        {
            #if UNITY_EDITOR
            if (!Application.IsPlaying(this)) {
                if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) {
                    return;
                }
            }
            #endif // UNITY_EDITOR

            m_Camera.AddOnPreRender(this);
            m_Camera.AddOnPostRender(this);
        }

        private void OnDisable()
        {
            m_Camera.RemoveOnPreRender(this);
            m_Camera.RemoveOnPostRender(this);
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            switch(m_AppliedScale)
            {
                case CameraCallbackSource.None:
                {
                    Graphics.Blit(src, dst);
                    break;
                }

                case CameraCallbackSource.Default:
                {
                    PostApplyScale(CameraCallbackSource.Default);
                    if (m_FilterMode == FilterMode.Point)
                    {
                        UnityHelper.BlitPixelPerfect(src, dst, m_Camera);
                    }
                    else
                    {
                        src.filterMode = m_FilterMode;
                        Graphics.Blit(src, dst);
                    }
                    break;
                }
            }
        }

        private float CalculateDesiredScale()
        {
            int fullHeight = (int) (Screen.height * m_Camera.rect.height);
            int pixelHeight;
            switch(m_Mode)
            {
                case ScaleMode.Scale:
                default:
                    pixelHeight = (int) (fullHeight * m_Scale) & ~(1);
                    break;
                case ScaleMode.PixelHeight:
                    pixelHeight = m_PixelHeight & (~1);
                    break;
            }

            float scale = (float) Mathf.Clamp(pixelHeight + 0.9f, 1, SystemInfo.maxTextureSize) / fullHeight;
            if (IsUsingSRP())
            {
                scale = Mathf.Clamp(scale, 0.1f, 2f);
            }
            return scale;
        }

        #if UNITY_EDITOR

        private void Reset()
        {
            m_Camera = GetComponent<Camera>();
            m_PixelHeight = m_Camera.pixelHeight;
        }

        private void OnValidate()
        {
            m_Camera = GetComponent<Camera>();
        }

        #endif // UNITY_EDITOR

        #endregion // Unity Events

        #region Callbacks

        private void ApplyScale(CameraCallbackSource inMode, float inScale)
        {
            if (m_AppliedScale != 0)
                return;

            m_AppliedScale = inMode;
            if (inMode == CameraCallbackSource.SRP)
            {
                #if USE_SRP
                ApplySRPScale(GraphicsSettings.renderPipelineAsset, inScale);
                #endif // USE_SRP
            }
            else
            {
                m_OriginalCameraRect = m_Camera.rect;
                Rect scaled = m_OriginalCameraRect;
                inScale = Mathf.Clamp01(inScale);
                scaled.Set(scaled.x, scaled.y, scaled.width * inScale, scaled.height * inScale);
                m_Camera.rect = scaled;
            }
        }

        private void PostApplyScale(CameraCallbackSource inMode)
        {
            if (m_AppliedScale != inMode)
                return;

            if (inMode == CameraCallbackSource.SRP)
            {
                #if USE_SRP
                ApplySRPScale(GraphicsSettings.renderPipelineAsset, 1);
                #endif // USE_SRP
            }
            else
            {
                m_Camera.rect = m_OriginalCameraRect;
            }
            m_AppliedScale = CameraCallbackSource.None;
        }

        void ICameraPreRenderCallback.OnCameraPreRender(Camera inCamera, CameraCallbackSource inSource)
        {
            float desiredScale = CalculateDesiredScale();
            if (desiredScale != 1)
            {
                ApplyScale(inSource, desiredScale);
            }
        }

        void ICameraPostRenderCallback.OnCameraPostRender(Camera inCamera, CameraCallbackSource inSource)
        {
            if (inSource == CameraCallbackSource.SRP)
            {
                if (m_AppliedScale == inSource)
                {
                    PostApplyScale(inSource);
                }
            }
        }

        #endregion // Callbacks
    
        #if USE_SRP
        
        static private void ApplySRPScale(RenderPipelineAsset inPipelineAsset, float inScale)
        {
            #if USING_URP
            var urpAsset = inPipelineAsset as UniversalRenderPipelineAsset;
            if (urpAsset)
            {
                urpAsset.renderScale = inScale;
                return;
            }
            #endif // USING_URP
        }

        #endif // USE_SRP

        static private bool IsUsingSRP()
        {
            #if USE_SRP
            return GraphicsSettings.renderPipelineAsset != null;
            #else
            return false;
            #endif // USE_SRP
        }

        #region Editor

        #if UNITY_EDITOR

        [CustomEditor(typeof(CameraRenderScale)), CanEditMultipleObjects]
        private class Editor : UnityEditor.Editor
        {
            private SerializedProperty m_ModeProperty;
            private SerializedProperty m_ScaleProperty;
            private SerializedProperty m_PixelHeightProperty;
            private SerializedProperty m_FilterModeProperty;

            private void OnEnable()
            {
                m_ModeProperty = serializedObject.FindProperty("m_Mode");
                m_ScaleProperty = serializedObject.FindProperty("m_Scale");
                m_PixelHeightProperty = serializedObject.FindProperty("m_PixelHeight");
                m_FilterModeProperty = serializedObject.FindProperty("m_FilterMode");
            }

            public override void OnInspectorGUI()
            {
                serializedObject.UpdateIfRequiredOrScript();

                EditorGUILayout.PropertyField(m_ModeProperty);
                if (m_ModeProperty.hasMultipleDifferentValues)
                {
                    EditorGUILayout.HelpBox("Cannot edit multiple cameras with different scale modes.", MessageType.Warning);
                }
                else
                {
                    bool hasSRP = IsUsingSRP();
                    ScaleMode mode = (ScaleMode) m_ModeProperty.intValue;
                    switch(mode)
                    {
                        case ScaleMode.Scale:
                            {
                                EditorGUILayout.Slider(m_ScaleProperty, 0.1f, hasSRP ? 2 : 1);
                                break;
                            }
                        case ScaleMode.PixelHeight:
                            {
                                EditorGUILayout.PropertyField(m_PixelHeightProperty);
                                if (!m_PixelHeightProperty.hasMultipleDifferentValues)
                                {
                                    m_PixelHeightProperty.intValue = Mathf.Clamp(m_PixelHeightProperty.intValue, 1, SystemInfo.maxTextureSize);
                                }
                                break;
                            }
                    }

                    if (!hasSRP)
                    {
                        EditorGUILayout.PropertyField(m_FilterModeProperty);
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }
        }

        #endif

        #endregion // Editor
    }
}