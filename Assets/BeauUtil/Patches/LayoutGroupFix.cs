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
        [SerializeField, HideInInspector] private ContentSizeFitter m_ContentSizer = null;

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
            {
                this.CacheComponent(ref m_LayoutGroup);
                this.CacheComponent(ref m_ContentSizer);
            }
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
            #if UNITY_EDITOR
            if (!Application.IsPlaying(this))
            {
                CanvasHelper.ForceRebuild(m_LayoutGroup, true);
                return;
            }
            #endif // UNITY_EDITOR

            m_LayoutGroup.enabled = true;
            if (m_ContentSizer != null)
                m_ContentSizer.enabled = true;
            
            CanvasHelper.ForceRebuild(m_LayoutGroup, true);
            
            m_LayoutGroup.enabled = false;
            if (m_ContentSizer != null)
                m_ContentSizer.enabled = false;
        }
        
        #if UNITY_EDITOR

        private void Reset()
        {
            this.CacheComponent(ref m_LayoutGroup);
            this.CacheComponent(ref m_ContentSizer);
        }

        private void OnValidate()
        {
            this.CacheComponent(ref m_LayoutGroup);
            this.CacheComponent(ref m_ContentSizer);
        }

        #endif // UNITY_EDITOR
    }
}