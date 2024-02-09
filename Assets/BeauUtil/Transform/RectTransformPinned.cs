/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 July 2020
 * 
 * File:    RectTransformPinned.cs
 * Purpose: Pins a RectTransform to another transform.
*/

using System;
using UnityEngine;
using UnityEngine.Events;

namespace BeauUtil
{
    [AddComponentMenu("BeauUtil/UI/RectTransform Pin Constraint")]
    [DefaultExecutionOrder(15000)]
    public class RectTransformPinned : RectTransformConstraint
    {
        #region Types

        public enum PositionBehavior
        {
            Snap,
            Custom
        }

        public enum ClampingBehavior
        {
            Point,
            Rect
        }

        public class PinBeginEvent : TinyUnityEvent<Transform, bool, Vector3> { }
        public class PinEvent : TinyUnityEvent<Transform> { }
        public class PinPositionEvent : TinyUnityEvent<Transform, Vector3> { }
        public class PinClampEvent : TinyUnityEvent<Transform, RectEdges> { }

        #endregion // Types

        #region Inspector

        [SerializeField] private Transform m_CurrentTarget;
        [SerializeField] private TransformOffset m_TargetWorldOffset;
        [Space]
        [SerializeField] private Vector2 m_LocalOffset = default(Vector2);

        [Header("Clamp")]
        [SerializeField] private bool m_ClampToParentRect = false;
        [SerializeField, ShowIfField("m_ClampToParentRect")] private Vector2 m_ParentRectClampOffset = default(Vector2);
        [SerializeField, ShowIfField("m_ClampToParentRect")] private ClampingBehavior m_ClampBehavior = default(ClampingBehavior);

        [Header("Behavior")]

        [SerializeField] private PositionBehavior m_PositionBehavior = PositionBehavior.Snap;
        [SerializeField] private bool m_UnpinWhenDisabled = false;

        #endregion // Inspector

        private readonly PinBeginEvent m_OnPinBegin = new PinBeginEvent();
        private readonly PinEvent m_OnPinAppear = new PinEvent();
        private readonly PinEvent m_OnPinDisappear = new PinEvent();
        private readonly PinPositionEvent m_OnPinUpdate = new PinPositionEvent();
        private readonly PinClampEvent m_OnClampUpdate = new PinClampEvent();
        private readonly PinEvent m_OnPinEnd = new PinEvent();

        [NonSerialized] private CameraTrackGroup m_WorldCameraGroup;
        [NonSerialized] private CameraTrackGroup m_CanvasCameraGroup;
        [NonSerialized] private TransformTrackGroup m_TargetTransformGroup;
        [NonSerialized] private CanvasSpaceTransformation m_CanvasTransformer;

        [NonSerialized] private Transform m_AppliedPinTarget;
        [NonSerialized] private Vector3 m_CurrentPinPosition;
        [NonSerialized] private bool m_PinOnScreen;
        [NonSerialized] private RectEdges m_ClampedEdges;
        #if UNITY_EDITOR
        [NonSerialized] private Vector2 m_AppliedLocalOffset;
        #endif // UNITY_EDITOR

        public PinBeginEvent OnPinBegin { get { return m_OnPinBegin; } }
        public PinEvent OnPinAppear { get { return m_OnPinAppear; } }
        public PinEvent OnPinDisappear { get { return m_OnPinDisappear; } }
        public PinPositionEvent OnPinUpdate { get { return m_OnPinUpdate; } }
        public PinClampEvent OnClampUpdate { get { return m_OnClampUpdate;}}
        public PinEvent OnPinEnd { get { return m_OnPinEnd; } }

        public RectEdges LastClampedEdges { get { return m_ClampedEdges; } }

        #region Unity Events

        protected override void Awake()
        {
            base.Awake();

            if (ShouldProcess)
            {
                TryPin(m_CurrentTarget, false);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!ShouldProcess)
                return;

            TryPin(m_CurrentTarget, !Application.isPlaying);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (!ShouldProcess)
                return;

            if (m_UnpinWhenDisabled)
            {
                TryUnpin(false);
            }
        }

        #if UNITY_EDITOR
        
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!ShouldProcess)
                return;

            if (m_CurrentTarget != m_AppliedPinTarget || m_AppliedLocalOffset != m_LocalOffset || m_CanvasTransformer.WorldOffset != m_TargetWorldOffset)
            {
                UnityEditor.EditorApplication.delayCall += () => {
                    if (!IsDestroyed())
                    {
                        TryPin(m_CurrentTarget, false);
                    }
                };
            }
        }

        #endif // UNITY_EDITOR

        #endregion // Unity Events

        #region RectTransformConstraint

        protected override bool ShouldUpdate()
        {
            return !m_AppliedPinTarget.IsReferenceNull() || !ReferenceEquals(m_CurrentTarget, m_AppliedPinTarget);
        }

        protected override void UpdateTrackers()
        {
            base.UpdateTrackers();

            if (m_CurrentTarget != m_AppliedPinTarget)
            {
                TryPin(m_CurrentTarget, false);
            }
        }

        protected override bool CheckForChanges()
        {
            bool bChanged = base.CheckForChanges();
            bChanged |= m_WorldCameraGroup.HasChanged();
            bChanged |= m_CanvasCameraGroup.HasChanged();
            bChanged |= m_TargetTransformGroup.HasChanged();
            bChanged |= m_CanvasTransformer.WorldOffset != m_TargetWorldOffset;
            #if UNITY_EDITOR
            bChanged |= m_AppliedLocalOffset != m_LocalOffset;
            #endif // UNITY_EDITOR
            return bChanged;
        }

        protected override void ApplyConstraints()
        {
            base.ApplyConstraints();

            if (m_AppliedPinTarget.IsReferenceDestroyed())
            {
                TryUnpin(true);
            }
            else if (!m_AppliedPinTarget.IsReferenceNull())
            {
                RectEdges oldEdges = m_ClampedEdges;
                bool bNowOnScreen = RecalculatePosition();
                if (bNowOnScreen != m_PinOnScreen)
                {
                    m_PinOnScreen = bNowOnScreen;
                    if (ShouldProcess)
                    {
                        if (m_PinOnScreen)
                            m_OnPinAppear.Invoke(m_AppliedPinTarget);
                        else
                            m_OnPinDisappear.Invoke(m_AppliedPinTarget);
                    }
                }

                ApplyPosition();
                
                if (ShouldProcess)
                {
                    if (m_PinOnScreen)
                    {
                        m_OnPinUpdate.Invoke(m_AppliedPinTarget, m_CurrentPinPosition);
                    }

                    if (m_ClampedEdges != oldEdges)
                    {
                        m_OnClampUpdate.Invoke(m_AppliedPinTarget, m_ClampedEdges);
                    }
                }
            }
        }
    
        #endregion // RectTransformConstraint

        #region Pinning

        public bool Pin(Transform inTarget, TransformOffset inOffset)
        {
            m_TargetWorldOffset = inOffset;
            return TryPin(inTarget, false);
        }

        public bool Pin(Transform inTarget)
        {
            return TryPin(inTarget, false);
        }

        public bool Unpin()
        {
            return TryUnpin(false);
        }

        private bool TryPin(Transform inTarget, bool inbForce)
        {
            if (!inbForce && ReferenceEquals(m_AppliedPinTarget, inTarget))
                return false;

            TryUnpin(false);

            if (!inTarget)
                return true;

            m_CurrentTarget = inTarget;
            m_AppliedPinTarget = inTarget;

            CacheTransform();

            if (!m_CanvasTransformer.TryLoadCanvas(m_SelfRectTransform, true))
            {
                Debug.LogWarningFormat("[RectTransformPinned] Unable to locate appropriate canvas camera for '{0}'", name);
            }

            if (!m_AppliedPinTarget.TryGetCamera(false, out m_CanvasTransformer.WorldCamera))
            {
                Debug.LogWarningFormat("[RectTransformPinned] Unable to locate appropriate world camera for '{0}'", m_AppliedPinTarget.name);
            }

            m_CanvasTransformer.WorldOffset = m_TargetWorldOffset;

            m_WorldCameraGroup.Replace(m_CanvasTransformer.WorldCamera, true);
            m_CanvasCameraGroup.Replace(m_CanvasTransformer.CanvasCamera, true);
            m_TargetTransformGroup.Replace(m_AppliedPinTarget, true);

            m_PinOnScreen = RecalculatePosition();
            ApplyPosition();

            if (ShouldProcess)
                m_OnPinBegin.Invoke(m_AppliedPinTarget, m_PinOnScreen, m_CurrentPinPosition);

            return true;
        }

        private bool TryUnpin(bool inbForce)
        {
            if (inbForce || !m_AppliedPinTarget.IsReferenceNull())
            {
                Transform oldTarget = m_CurrentTarget;
                m_AppliedPinTarget = null;
                m_CurrentTarget = null;

                m_WorldCameraGroup.Clear();
                m_CanvasCameraGroup.Clear();
                m_TargetTransformGroup.Clear();

                if (ShouldProcess)
                    m_OnPinEnd.Invoke(oldTarget);

                return true;
            }

            return false;
        }

        #endregion // Pinning

        private bool RecalculatePosition()
        {
            m_CanvasTransformer.WorldOffset = m_TargetWorldOffset;
            #if UNITY_EDITOR
            m_AppliedLocalOffset = m_LocalOffset;
            #endif // UNITY_EDITOR

            bool bFound = m_CanvasTransformer.TryConvertToLocalSpace(m_CurrentTarget, out m_CurrentPinPosition);
            if (!bFound)
            {
                return false;
            }

            m_CurrentPinPosition += new Vector3(m_LocalOffset.x, m_LocalOffset.y, 0);
            RectTransform parent = m_SelfRectTransform.parent as RectTransform;
            if (!m_ClampToParentRect || parent == null)
            {
                return true;
            }

            m_ClampedEdges = 0;

            Rect rect = parent.rect;
            rect.x += m_ParentRectClampOffset.x / 2;
            rect.y += m_ParentRectClampOffset.y / 2;
            rect.width -= m_ParentRectClampOffset.x;
            rect.height -= m_ParentRectClampOffset.y;

            if (m_ClampBehavior == ClampingBehavior.Rect)
            {
                Rect myRect = m_SelfRectTransform.rect;
                rect.xMin -= myRect.xMin;
                rect.xMax -= myRect.xMax;
                rect.yMin -= myRect.yMin;
                rect.yMax -= myRect.yMax;
            }

            if (m_CurrentPinPosition.x < rect.xMin)
            {
                m_CurrentPinPosition.x = rect.xMin;
                m_ClampedEdges |= RectEdges.Left;
            }
            else if (m_CurrentPinPosition.x > rect.xMax)
            {
                m_CurrentPinPosition.x = rect.xMax;
                m_ClampedEdges |= RectEdges.Right;
            }

            if (m_CurrentPinPosition.y < rect.yMin)
            {
                m_CurrentPinPosition.y = rect.yMin;
                m_ClampedEdges |= RectEdges.Bottom;
            }
            else if (m_CurrentPinPosition.y > rect.yMax)
            {
                m_CurrentPinPosition.y = rect.yMax;
                m_ClampedEdges |= RectEdges.Top;
            }
            return true;
        }

        private void ApplyPosition()
        {
            switch(m_PositionBehavior)
            {
                case PositionBehavior.Snap:
                    {
                        m_SelfRectTransform.localPosition = m_CurrentPinPosition;
                        break;
                    }
            }
        }
    }
}