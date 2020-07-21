/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    20 July 2020
 * 
 * File:    LayoutConstraint.cs
 * Purpose: Base class for a layout element that constrains another layout element's parameters.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeauUtil.UI
{
    [RequireComponent(typeof(RectTransform), typeof(ILayoutElement))]
    public abstract class LayoutConstraint : UIBehaviour, ILayoutElement
    {
        protected const int ConstraintPriorityStart = 500;

        #region Inspector

        [SerializeField, HideInInspector] private Component m_SerializedLayoutComponent;

        #endregion // Inspector

        [NonSerialized] protected ILayoutElement m_LayoutSource;

        #region ILayoutElement

        public virtual float minWidth { get { return -1; } }

        public virtual float preferredWidth { get { return -1; } }

        public virtual float flexibleWidth { get { return -1; } }

        public virtual float minHeight { get { return -1; } }

        public virtual float preferredHeight { get { return -1; } }

        public virtual float flexibleHeight { get { return -1; } }

        public virtual int layoutPriority { get { return ConstraintPriorityStart; } }

        public virtual void CalculateLayoutInputHorizontal() { }

        public virtual void CalculateLayoutInputVertical() { }

        #endregion // ILayoutElement

        #region Unity Events

        protected override void Awake()
        {
            base.Awake();

            CacheRefs(false);
        }

        #endregion // Unity Events

        #region Helpers

        static protected float ConstrainRange(float inInput, float inMin, float inMax)
        {
            float val = inInput;
            if (val < 0)
                return val;

            if (inMin >= 0 && val < inMin)
                val = inMin;
            if (inMax >= 0 && val > inMax)
                val = inMax;

            return val;
        }

        #endregion // Helpers

        #region Virtuals

        static private readonly List<ILayoutElement> s_CachedLayoutList = new List<ILayoutElement>();

        protected virtual void CacheRefs(bool inbThrowErrors)
        {
            if (m_LayoutSource == null)
            {
                if (m_SerializedLayoutComponent && m_SerializedLayoutComponent.gameObject == gameObject)
                {
                    m_LayoutSource = (ILayoutElement) m_SerializedLayoutComponent;
                }
                else
                {
                    GetComponents<ILayoutElement>(s_CachedLayoutList);
                    s_CachedLayoutList.Remove(this);
                    if (s_CachedLayoutList.Count > 0)
                    {
                        m_LayoutSource = s_CachedLayoutList[0];
                        m_SerializedLayoutComponent = (Component) m_LayoutSource;
                    }
                    s_CachedLayoutList.Clear();
                }
            }

            if (inbThrowErrors && m_LayoutSource == null)
            {
                throw new MissingComponentException("Could not locate another ILayoutElement component on GameObject " + gameObject.name);
            }
        }

        protected virtual void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }

        #endregion // Virtuals

        #if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();

            if (!IsActive())
                return;

            CacheRefs(false);
            SetDirty();
        }

        protected override void Reset()
        {
            base.Reset();

            CacheRefs(false);
            SetDirty();
        }

        #endif // UNITY_EDITOR
    }
}