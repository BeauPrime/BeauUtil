/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    28 April 2021
 * 
 * File:    GradientRectGraphic.cs
 * Purpose: Gradient rectangle graphic.
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil
{
    /// <summary>
    /// Gradient rectangle graphic.
    /// </summary>
    [AddComponentMenu("BeauUtil/Rendering/Gradient Rect Graphic")]
    public class GradientRectGraphic : MaskableGraphic
    {
        private enum CornerMode
        {
            Left,
            Right,
        }

        [Header("Corners")]
        [SerializeField] private Color m_BottomLeftColor = Color.white;
        [SerializeField] private Color m_BottomRightColor = Color.white;
        [SerializeField] private Color m_TopLeftColor = Color.white;
        [SerializeField] private Color m_TopRightColor = Color.white;
        [SerializeField] private CornerMode m_TriangleGenerationMode = CornerMode.Left;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);

            Color c = color;
            vh.AddVert(new Vector3(v.x, v.y), c * m_BottomLeftColor, new Vector2(0f, 0f));
            vh.AddVert(new Vector3(v.x, v.w), c * m_TopLeftColor, new Vector2(0f, 1f));
            vh.AddVert(new Vector3(v.z, v.w), c * m_TopRightColor, new Vector2(1f, 1f));
            vh.AddVert(new Vector3(v.z, v.y), c * m_BottomRightColor, new Vector2(1f, 0f));

            int offset = (int) m_TriangleGenerationMode;

            vh.AddTriangle(0 + offset, (1 + offset) % 4, (2 + offset) % 4);
            vh.AddTriangle((2 + offset) % 4, (3 + offset) % 4, 0 + offset);
        }
    }
}
