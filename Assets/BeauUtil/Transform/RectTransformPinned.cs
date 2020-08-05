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
    public class RectTransformPinned : RectTransformConstraint
    {
        #region Types

        public enum PositionBehavior
        {
            Snap,
            Custom
        }

        public class PinBeginEvent : UnityEvent<Transform, bool, Vector3> { }
        public class PinEvent : UnityEvent<Transform> { }
        public class PinPositionEvent : UnityEvent<Transform, Vector3> { }

        #endregion // Types

        #region Inspector

        [SerializeField] private Transform m_CurrentTarget;
        [SerializeField] private TransformOffset m_TargetWorldOffset;
        [Space]
        [SerializeField] private Vector2 m_LocalOffset = default(Vector2);

        [Header("Behavior")]

        [SerializeField] private PositionBehavior m_PositionBehavior = PositionBehavior.Snap;
        [SerializeField] private bool m_UnpinWhenDisabled = false;

        [Header("Events")]

        [SerializeField] private PinBeginEvent m_OnPinBegin = new PinBeginEvent();
        [SerializeField] private PinEvent m_OnPinAppear = new PinEvent();
        [SerializeField] private PinEvent m_OnPinDisappear = new PinEvent();
        [SerializeField] private PinPositionEvent m_OnPinUpdate = new PinPositionEvent();
        [SerializeField] private PinEvent m_OnPinEnd = new PinEvent();

        #endregion // Inspector

        [NonSerialized] private CameraTrackGroup m_WorldCameraGroup;
        [NonSerialized] private CameraTrackGroup m_CanvasCameraGroup;
        [NonSerialized] private TransformTrackGroup m_TargetTransformGroup;
        [NonSerialized] private CanvasSpaceTransformation m_CanvasTransformer;

        [NonSerialized] private Transform m_AppliedPinTarget;
        [NonSerialized] private Vector3 m_CurrentPinPosition;
        [NonSerialized] private bool m_PinOnScreen;
        #if UNITY_EDITOR
        [NonSerialized] private Vector2 m_AppliedLocalOffset;
        #endif // UNITY_EDITOR

        public PinBeginEvent OnPinBegin { get { return m_OnPinBegin; } }
        public PinEvent OnPinAppear { get { return m_OnPinAppear; } }
        public PinEvent OnPinDisappear { get { return m_OnPinDisappear; } }
        public PinPositionEvent OnPinUpdate { get { return m_OnPinUpdate; } }
        public PinEvent OnPinEnd { get { return m_OnPinEnd; } }

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
            else
            {
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
                
                if (m_PinOnScreen && ShouldProcess)
                {
                    m_OnPinUpdate.Invoke(m_AppliedPinTarget, m_CurrentPinPosition);
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
            if (bFound)
                m_CurrentPinPosition += new Vector3(m_LocalOffset.x, m_LocalOffset.y, 0);
            return bFound;
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