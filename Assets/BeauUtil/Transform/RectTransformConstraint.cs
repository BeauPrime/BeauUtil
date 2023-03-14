/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 July 2020
 * 
 * File:    RectTransformConstraint.cs
 * Purpose: Base class for a UI element that constrains itself.
*/

#if UNITY_2018_3_OR_NEWER
#define USE_ALWAYS
#endif // UNITY_2018_3_OR_NEWER

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeauUtil
{
    [RequireComponent(typeof(RectTransform))]
    #if USE_ALWAYS
    [ExecuteAlways]
    #else
    [ExecuteInEditMode]
    #endif // USE_ALWAYS
    public abstract class RectTransformConstraint : UIBehaviour
    {
        protected struct CameraTrackGroup
        {
            public Camera Camera;
            private TrackedCamera m_Tracker;
            private int m_Version;

            public bool IsActive()
            {
                return m_Tracker;
            }

            public void Clear()
            {
                Camera = null;
                m_Tracker = null;
                m_Version = -1;
            }

            public bool Replace(Camera inCamera, bool inbSync)
            {
                if (!ReferenceEquals(inCamera, Camera))
                {
                    Camera = inCamera;
                    if (inCamera != null)
                    {
                        m_Tracker = TrackedCamera.Get(Camera);
                        if (inbSync)
                            UpdateVersion.Sync(m_Tracker, ref m_Version);
                        else
                            UpdateVersion.Reset(ref m_Version);
                    }
                    else
                    {
                        m_Tracker = null;
                        m_Version = -2;
                    }
                }

                return false;
            }

            public bool HasChanged()
            {
                if (Camera.IsReferenceDestroyed() || m_Tracker.IsReferenceDestroyed())
                {
                    Camera = null;
                    m_Tracker = null;
                    m_Version = -1;
                    return true;
                }

                if (!m_Tracker.IsReferenceNull())
                {
                    return m_Tracker.HasChanged(ref m_Version);
                }

                if (m_Version == -2)
                {
                    m_Version = -1;
                    return true;
                }

                return false;
            }
        
            public void Sync()
            {
                if (m_Tracker)
                    m_Tracker.Sync(ref m_Version);
                else
                    m_Version = -1;
            }
        }

        protected struct TransformTrackGroup
        {
            public Transform Transform;
            private TrackedTransform m_Tracker;
            private int m_Version;

            public bool IsActive()
            {
                return m_Tracker;
            }

            public void Clear()
            {
                Transform = null;
                m_Tracker = null;
                m_Version = -1;
            }

            public bool Replace(Transform inTransform, bool inbSync)
            {
                if (!ReferenceEquals(inTransform, Transform))
                {
                    Transform = inTransform;
                    if (inTransform != null)
                    {
                        m_Tracker = TrackedTransform.Get(Transform);
                        if (inbSync)
                            UpdateVersion.Sync(m_Tracker, ref m_Version);
                        else
                            UpdateVersion.Reset(ref m_Version);

                    }
                    else
                    {
                        m_Tracker = null;
                        m_Version = -2;
                    }
                }

                return false;
            }

            public bool HasChanged()
            {
                if (Transform.IsReferenceDestroyed() || m_Tracker.IsReferenceDestroyed())
                {
                    Transform = null;
                    m_Tracker = null;
                    m_Version = -1;
                    return true;
                }

                if (!m_Tracker.IsReferenceNull())
                {
                    return m_Tracker.HasChanged(ref m_Version);
                }

                if (m_Version == -2)
                {
                    m_Version = -1;
                    return true;
                }

                return false;
            }
        
            public void Sync()
            {
                if (m_Tracker)
                    m_Tracker.Sync(ref m_Version);
                else
                    m_Version = -1;
            }
        }

        #region Inspector

        [SerializeField] private bool m_PreviewInEditMode = false;

        #endregion // Inspector

        [NonSerialized] protected RectTransform m_SelfRectTransform;

        protected void CacheTransform()
        {
            if (ReferenceEquals(m_SelfRectTransform, null))
                m_SelfRectTransform = (RectTransform) transform;
        }

        protected override void Awake()
        {
            base.Awake();

            CacheTransform();

            if (!ShouldProcess)
                return;

            UpdateConstraints();
        }

        #if UNITY_EDITOR
        private void Update()
        {
            if (!m_PreviewInEditMode || Application.IsPlaying(gameObject))
                return;

            UpdateConstraints();
        }
        #endif // UNITY_EDITOR

        private void LateUpdate()
        {
            #if UNITY_EDITOR
            if (!Application.IsPlaying(gameObject))
                return;
            #endif // UNITY_EDITOR

            UpdateConstraints();
        }

        private void UpdateConstraints()
        {
            CacheTransform();

            if (!ShouldUpdate())
                return;

            UpdateTrackers();
            if (CheckForChanges())
            {
                ApplyConstraints();
            }
        }

        protected virtual void UpdateTrackers() { }
        protected virtual bool ShouldUpdate() { return true; }
        protected virtual bool CheckForChanges() { return false; }
        protected virtual void ApplyConstraints() { }

        #if UNITY_EDITOR
        protected bool ShouldProcess
        {
            get { return (m_PreviewInEditMode && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) || Application.IsPlaying(gameObject); }
        }
        #else
        protected const bool ShouldProcess = true;
        #endif // UNITY_EDITOR

        static protected T GetTarget<T>(T inTarget, T inDefault) where T : UnityEngine.Object
        {
            if (!ReferenceEquals(inTarget, null) && inTarget)
                return inTarget;

            return inDefault;
        }
    }
}