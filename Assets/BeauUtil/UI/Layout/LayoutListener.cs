/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    19 December 2022
 * 
 * File:    LayoutListener.cs
 * Purpose: Layout event listener.
*/

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeauUtil.UI
{
    /// <summary>
    /// Layout modifier.
    /// </summary>
    [AddComponentMenu("BeauUtil/UI/Layout Listener")]
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class LayoutListener : MonoBehaviour, ILayoutElement, ILayoutSelfController
    {
        /// <summary>
        /// Invoked before layout has been determined.
        /// </summary>
        public readonly CastableEvent<LayoutListener> OnPreLayout;

        /// <summary>
        /// Invoked after layout has been determined.
        /// </summary>
        public readonly CastableEvent<LayoutListener> OnPostLayout;

        [NonSerialized] private bool m_Rebuilding;

        protected LayoutListener()
        {
            OnPreLayout = new CastableEvent<LayoutListener>(2);
            OnPostLayout = new CastableEvent<LayoutListener>(2);
        }

        #region Unity Events

        private void OnEnable()
        {
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform) transform);
        }

        #endregion // Unity Events

        #region ILayout interfaces

        float ILayoutElement.minWidth  { get { return -1; } }

        float ILayoutElement.preferredWidth { get { return -1; } }

        float ILayoutElement.flexibleWidth { get { return -1; } }

        float ILayoutElement.minHeight { get { return -1; } }

        float ILayoutElement.preferredHeight { get { return -1; } }

        float ILayoutElement.flexibleHeight { get { return -1; } }

        int ILayoutElement.layoutPriority { get { return -1; } }

        void ILayoutElement.CalculateLayoutInputHorizontal()
        {
            if (!m_Rebuilding) {
                m_Rebuilding = true;
                OnPreLayout.Invoke(this);
            }
        }

        void ILayoutElement.CalculateLayoutInputVertical() { }

        void ILayoutController.SetLayoutHorizontal() { }

        void ILayoutController.SetLayoutVertical()
        {
            if (m_Rebuilding) {
                m_Rebuilding = false;
                OnPostLayout.Invoke(this);
            }
        }

        #endregion // ILayout interfaces
    }
}