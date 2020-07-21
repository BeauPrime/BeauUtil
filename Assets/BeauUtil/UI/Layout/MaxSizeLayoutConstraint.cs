/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    20 July 2020
 * 
 * File:    MaxLayoutConstraint.cs
 * Purpose: Layout element that sets a maximum size.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeauUtil.UI
{
    [AddComponentMenu("BeauUtil/UI/Max Size Layout Constraint")]
    public class MaxSizeLayoutConstraint : LayoutConstraint
    {
        #region Inspector

        [SerializeField] private float m_MaxWidth = -1;
        [SerializeField] private float m_MaxHeight = -1;

        #endregion // Inspector

        public float MaxWidth { get { return m_MaxWidth; } set { if (Ref.Replace(ref m_MaxWidth, value)) SetDirty(); } }
        public float MaxHeight { get { return m_MaxHeight; } set { if (Ref.Replace(ref m_MaxHeight, value)) SetDirty(); } }

        public override float minWidth
        {
            get 
            {
                CacheRefs(true);
                return ConstrainRange(m_LayoutSource.minWidth, 0, m_MaxWidth);
            }
        }

        public override float preferredWidth
        {
            get 
            {
                CacheRefs(true);
                return ConstrainRange(m_LayoutSource.preferredWidth, 0, m_MaxWidth);
            }
        }

        public override float minHeight
        {
            get 
            {
                CacheRefs(true);
                return ConstrainRange(m_LayoutSource.minHeight, 0, m_MaxHeight);
            }
        }

        public override float preferredHeight
        {
            get 
            {
                CacheRefs(true);
                return ConstrainRange(m_LayoutSource.preferredHeight, 0, m_MaxHeight);
            }
        }
    }
}