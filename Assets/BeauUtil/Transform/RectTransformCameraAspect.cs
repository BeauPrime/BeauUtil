/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 July 2020
 * 
 * File:    RectTransformCameraAspect.cs
 * Purpose: Ensures the aspect ratio for a RectTransform lines up with the camera's aspect ratio.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeauUtil
{
    [AddComponentMenu("BeauUtil/UI/RectTransform Camera Aspect Constraint")]
    public class RectTransformCameraAspect : RectTransformConstraint
    {
        #region Inspector

        [SerializeField] private Camera m_Camera = null;
        
        [SerializeField, Tooltip("If set, the size of this RectTransform will be set to the size of the camera directly.")]
        private bool m_MatchCameraOrthoSize = false;

        #endregion // Inspector

        [NonSerialized] private CameraTrackGroup m_TargetCameraGroup;

        protected override void UpdateTrackers()
        {
            base.UpdateTrackers();

            m_TargetCameraGroup.Replace(GetTarget(m_Camera, () => Camera.main), false);
        }

        protected override bool CheckForChanges()
        {
            bool bChanged = base.CheckForChanges();
            bChanged |= m_TargetCameraGroup.HasChanged();
            return bChanged;
        }

        protected override void ApplyConstraints()
        {
            base.ApplyConstraints();

            Camera cam = m_TargetCameraGroup.Camera;
            if (cam)
            {
                Vector2 sizeDelta = m_SelfRectTransform.sizeDelta;
                if (m_MatchCameraOrthoSize && cam.orthographic)
                {
                    sizeDelta.y = cam.orthographicSize * 2;
                }
                sizeDelta.x = sizeDelta.y * cam.aspect;
                m_SelfRectTransform.sizeDelta = sizeDelta;
            }
        }

        #if UNITY_EDITOR

        protected override void Reset()
        {
            base.Reset();

            m_SelfRectTransform = ((RectTransform) transform);
            m_SelfRectTransform.TryGetCamera(true, out m_Camera);
        }

        #endif // UNITY_EDITOR
    }
}