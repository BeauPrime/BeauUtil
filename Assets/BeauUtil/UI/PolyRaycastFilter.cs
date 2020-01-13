/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    PolyRaycastFilter.cs
 * Purpose: Polygonal raycast filter.
*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeauUtil.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class PolyRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
    {
        [NonSerialized]
        private RectTransform m_RectTransform;

        protected RectTransform Rect
        {
            get
            {
                if (!m_RectTransform)
                    m_RectTransform = (RectTransform) transform;
                return m_RectTransform;
            }
        }

        static private readonly List<Vector2> s_CornerList = new List<Vector2>();

        bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect, sp, eventCamera, out localPos);

            int shapeCount = GetShapeCount();
            for (int shapeIdx = 0; shapeIdx < shapeCount; ++shapeIdx)
            {
                s_CornerList.Clear();
                GetCorners(shapeIdx, s_CornerList);
                if (Geom.PointInPolygon(localPos, s_CornerList))
                    return true;
            }

            return false;
        }

        protected Vector2 WorldToLocal(Vector3 inWorld)
        {
            return Rect.InverseTransformPoint(inWorld);
        }

        protected abstract int GetShapeCount();
        protected abstract void GetCorners(int inShapeIdx, List<Vector2> outCorners);

        #region Editor

        #if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            int shapeCount = GetShapeCount();
            for (int shapeIdx = 0; shapeIdx < shapeCount; ++shapeIdx)
            {
                s_CornerList.Clear();
                GetCorners(shapeIdx, s_CornerList);

                Gizmos.color = Color.red;

                int i, j;
                for (i = 0; i < s_CornerList.Count; ++i)
                {
                    j = (i + 1) % s_CornerList.Count;
                    DrawLine(s_CornerList[i], s_CornerList[j]);
                }
            }
        }

        private void DrawLine(Vector2 posA, Vector2 posB)
        {
            Gizmos.DrawLine(Rect.TransformPoint(posA), Rect.TransformPoint(posB));
        }

        #endif // UNITY_EDITOR

        #endregion // Editor
    }
}