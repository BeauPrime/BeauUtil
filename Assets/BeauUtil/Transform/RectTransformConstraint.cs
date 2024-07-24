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
using System.Runtime.CompilerServices;
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
        private const ulong DestroyedHash = ulong.MaxValue;

        protected struct CameraTrackGroup
        {
            public Camera Camera;
            private ulong m_Hash;

            public bool IsActive()
            {
                return Camera;
            }

            public void Clear()
            {
                Camera = null;
                m_Hash = 0;
            }

            public bool Replace(Camera inCamera, bool inbSync)
            {
                if (!ReferenceEquals(inCamera, Camera))
                {
                    Camera = inCamera;
                    if (inCamera != null)
                    {
                        if (inbSync)
                            m_Hash = Camera.GetStateHash();
                        else
                            m_Hash = 0;
                    }
                    else
                    {
                        m_Hash = DestroyedHash;
                    }
                }

                return false;
            }

            public bool HasChanged()
            {
                if (Camera.IsReferenceDestroyed())
                {
                    Camera = null;
                    m_Hash = 0;
                    return true;
                }

                if (!Camera.IsReferenceNull())
                {
                    return StateHash.HasChanged(Camera.GetStateHash(), ref m_Hash);
                }

                if (m_Hash == DestroyedHash)
                {
                    m_Hash = 0;
                    return true;
                }

                return false;
            }
        
            public void Sync()
            {
                if (Camera)
                {
                    m_Hash = Camera.GetStateHash();
                }
            }
        }

        protected struct TransformTrackGroup
        {
            public Transform Transform;
            private ulong m_Hash;

            public bool IsActive()
            {
                return Transform;
            }

            public void Clear()
            {
                Transform = null;
                m_Hash = 0;
            }

            public bool Replace(Transform inTransform, bool inbSync)
            {
                if (!ReferenceEquals(inTransform, Transform))
                {
                    Transform = inTransform;
                    if (inTransform != null)
                    {
                        if (inbSync)
                            m_Hash = inTransform.GetStateHash();
                        else
                            m_Hash = 0;
                    }
                    else
                    {
                        m_Hash = DestroyedHash;
                    }
                }

                return false;
            }

            public bool HasChanged()
            {
                if (Transform.IsReferenceDestroyed())
                {
                    Transform = null;
                    m_Hash = 0;
                    return true;
                }

                if (!Transform.IsReferenceNull())
                {
                    return StateHash.HasChanged(Transform.GetStateHash(), ref m_Hash);
                }

                if (m_Hash == DestroyedHash)
                {
                    m_Hash = 0;
                    return true;
                }

                return false;
            }
        
            public void Sync()
            {
                if (Transform)
                    m_Hash = Transform.GetStateHash();
                else
                    m_Hash = 0;
            }
        }

        #region Inspector

        [SerializeField] private bool m_PreviewInEditMode = false;

        #endregion // Inspector

        [NonSerialized] protected RectTransform m_SelfRectTransform;

        protected void CacheTransform()
        {
            this.CacheComponent(ref m_SelfRectTransform);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static protected T GetTarget<T>(T inTarget, T inDefault) where T : UnityEngine.Object
        {
            if (!ReferenceEquals(inTarget, null) && inTarget)
                return inTarget;

            return inDefault;
        }
    }
}