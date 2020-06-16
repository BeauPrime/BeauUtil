/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    NestedCanvasSortOrderFix.cs
 * Purpose: Fixes canvas sorting order for nested canvases.
 */

using UnityEngine;

namespace BeauUtil
{
    [AddComponentMenu("BeauUtil/Patches/Nested Canvas Sorting Order Fix")]
    public sealed class NestedCanvasSortOrderFix : MonoBehaviour
    {
        [SerializeField]
        private Canvas m_Canvas;

        private void OnEnable()
        {
            if (m_Canvas)
            {
                bool bCachedOverrideSort = m_Canvas.overrideSorting;
                m_Canvas.overrideSorting = true;
                m_Canvas.overrideSorting = bCachedOverrideSort;
            }
        }

        #if UNITY_EDITOR
        private void AutoLocateCanvas()
        {
            if (!m_Canvas)
                m_Canvas = GetComponentInParent<Canvas>();
        }

        private void OnValidate()
        {
            AutoLocateCanvas();
        }

        private void Reset()
        {
            AutoLocateCanvas();
        }
        #endif
    }
}