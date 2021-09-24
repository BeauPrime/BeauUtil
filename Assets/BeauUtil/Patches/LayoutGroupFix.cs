/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 Sept 2021
 * 
 * File:    LayoutGroupFix.cs
 * Purpose: Rebuilds layout groups on enable.
 */

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil
{
    [AddComponentMenu("BeauUtil/Patches/Layout Group Fix")]
    [ExecuteAlways]
    [RequireComponent(typeof(LayoutGroup))]
    public sealed class LayoutGroupFix : MonoBehaviour
    {
        [SerializeField, HideInInspector] private LayoutGroup m_LayoutGroup = null;

        [NonSerialized] private readonly Canvas.WillRenderCanvases m_Callback;
        [NonSerialized] private bool m_RebuildQueued;

        private LayoutGroupFix()
        {
            m_Callback = RebuildFromEnabled;
        }

        private void OnEnable()
        {
            #if UNITY_EDITOR
            if (!Application.IsPlaying(this))
                this.CacheComponent(ref m_LayoutGroup);
            #endif // UNITY_EDITOR

            m_RebuildQueued = true;
            Canvas.preWillRenderCanvases += m_Callback;
        }

        private void OnDisable()
        {
            Canvas.preWillRenderCanvases -= m_Callback;
        }

        private void RebuildFromEnabled()
        {
            if (m_RebuildQueued)
            {
                Rebuild();
                m_RebuildQueued = false;
            }
        }

        public void Rebuild()
        {
            CanvasHelper.ForceRebuild(m_LayoutGroup, true);
        }
        
        #if UNITY_EDITOR

        private void Reset()
        {
            this.CacheComponent(ref m_LayoutGroup);
        }

        private void OnValidate()
        {
            this.CacheComponent(ref m_LayoutGroup);
        }

        #endif // UNITY_EDITOR
    }
}