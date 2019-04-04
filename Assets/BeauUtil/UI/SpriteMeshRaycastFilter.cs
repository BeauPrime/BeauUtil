/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    SpriteMeshRaycastFilter.cs
 * Purpose: Raycast filter mapping to a sprite's physics shape.
            Note that this will not work if the sprite's physics shape
            has a hole in it.
*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeauUtil.UI
{
    [AddComponentMenu("BeauUtil/UI/Sprite Mesh Raycast Filter")]
    public class SpriteMeshRaycastFilter : PolyRaycastFilter
    {
        #region Inspector

        [SerializeField]
        private Sprite m_Sprite = null;

        [SerializeField]
        private float m_Scale = 1;

        #endregion // Inspector

        public Sprite Sprite
        {
            get
            {
                return m_Sprite;
            }
            set
            {
                if (m_Sprite != value)
                {
                    m_Sprite = value;
                    m_Dirty = true;
                }
            }
        }

        public float Scale
        {
            get { return m_Scale; }
            set { m_Scale = value; }
        }

        private bool m_Dirty = false;
        private int m_CachedShapeCount;
        private Vector2[][] m_CachedShapes;

        protected override int GetShapeCount()
        {
            RefreshShapes();
            return m_CachedShapeCount;
        }

        protected override void GetCorners(int inShapeIdx, List<Vector2> outCorners)
        {
            if (!m_Sprite)
                return;

            RefreshShapes();

            Rect r = Rect.rect;
            float scaleX = m_Scale * r.width;
            float scaleY = m_Scale * r.height;

            Vector2[] shape = m_CachedShapes[inShapeIdx];
            for (int i = 0; i < shape.Length; ++i)
            {
                Vector2 point = shape[i];
                point.x *= scaleX;
                point.y *= scaleY;
                outCorners.Add(point);
            }
        }

        #if UNITY_EDITOR

        private void OnValidate()
        {
            m_Dirty = true;
        }

        #endif // UNITY_EDITOR

        [ThreadStatic]
        static private List<Vector2> s_PooledList;

        private void RefreshShapes()
        {
            if (m_CachedShapes == null || m_Dirty)
            {
                m_CachedShapeCount = m_Sprite != null ? m_Sprite.GetPhysicsShapeCount() : 0;
                Array.Resize(ref m_CachedShapes, m_CachedShapeCount);

                if (m_CachedShapeCount > 0)
                {
                    if (s_PooledList == null)
                        s_PooledList = new List<Vector2>(64);

                    for (int shapeIdx = 0; shapeIdx < m_CachedShapeCount; ++shapeIdx)
                    {
                        s_PooledList.Clear();
                        int pointCount = m_Sprite.GetPhysicsShape(shapeIdx, s_PooledList);

                        m_CachedShapes[shapeIdx] = s_PooledList.ToArray();
                    }
                }

                m_Dirty = false;
            }
        }
    }
}