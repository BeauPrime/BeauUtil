/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    15 Nov 2023
 * 
 * File:    RegularPolyGraphic.cs
 * Purpose: Regular polygon graphic.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil.UI
{
    /// <summary>
    /// Rounded rectangle graphic.
    /// </summary>
    [AddComponentMenu("BeauUtil/Rendering/Regular Polygon Graphic")]
    public class RegularPolyGraphic : ShapeGraphic
    {
        [SerializeField] private int m_Sides = 6;
        [SerializeField] private float m_RotationOffset = 0;

        [Space]
        [SerializeField] private bool m_Outline = false;
        [SerializeField, ShowIfField("m_Outline")] private float m_Thickness = 1;

        public float RotationOffset
        {
            get { return m_RotationOffset; }
            set
            {
                if (m_RotationOffset != value)
                {
                    m_RotationOffset = value;
                    SetVerticesDirty();
                }
            }
        }

        public int SideCount
        {
            get { return m_Sides; }
            set
            {
                if (m_Sides != value)
                {
                    m_Sides = value;
                    SetVerticesDirty();
                }
            }
        }

        public bool Outline
        {
            get { return m_Outline; }
            set
            {
                if (m_Outline != value)
                {
                    m_Outline = value;
                    SetVerticesDirty();
                }
            }
        }

        public float OutlineWidth
        {
            get { return m_Thickness; }
            set
            {
                if (m_Thickness != value)
                {
                    m_Thickness = value;
                    if (m_Outline)
                        SetVerticesDirty();
                }
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var r = GetPixelAdjustedRect();
            
            if (m_Outline)
                CanvasMesh.AddRegularPolygonOutline(vh, r, m_Sides, m_RotationOffset, m_Thickness, color, m_TextureRegion.UVCenter);
            else
                CanvasMesh.AddRegularPolygon(vh, r, m_Sides, m_RotationOffset, color, m_TextureRegion.UVCenter);
        }
    }
}
