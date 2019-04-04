/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    TrapezoidRaycastFilter.cs
 * Purpose: Trapezoidal filter, created by offsetting the top and bottom vertices.
*/

using System.Collections.Generic;
using UnityEngine;

namespace BeauUtil.UI
{
    [AddComponentMenu("BeauUtil/UI/Trapezoid Raycast Filter")]
    public class TrapezoidRaycastFilter : PolyRaycastFilter
    {
        #region Inspector

        [SerializeField]
        private Vector2 m_TopOffset = default(Vector2);

        [SerializeField]
        private Vector2 m_BottomOffset = default(Vector2);

        #endregion // Inspector

        protected override int GetShapeCount() { return 1; }

        protected override void GetCorners(int inShapeIdx, List<Vector2> outCorners)
        {
            Rect myRect = Rect.rect;

            Vector2 topLeft = new Vector2(myRect.xMin - m_TopOffset.x, myRect.yMax + m_TopOffset.y);
            Vector2 topRight = new Vector2(myRect.xMax + m_TopOffset.x, myRect.yMax + m_TopOffset.y);
            Vector2 bottomLeft = new Vector2(myRect.xMin - m_BottomOffset.x, myRect.yMin - m_BottomOffset.y);
            Vector2 bottomRight = new Vector2(myRect.xMax + m_BottomOffset.x, myRect.yMin - m_BottomOffset.y);

            outCorners.Add(topLeft);
            outCorners.Add(topRight);
            outCorners.Add(bottomRight);
            outCorners.Add(bottomLeft);
        }
    }
}