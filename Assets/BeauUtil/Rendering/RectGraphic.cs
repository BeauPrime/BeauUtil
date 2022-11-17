/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    28 April 2021
 * 
 * File:    RectGraphic.cs
 * Purpose: Plain rectangle graphic.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil.UI
{
    /// <summary>
    /// Plain rectangle graphic.
    /// </summary>
    [AddComponentMenu("BeauUtil/Rendering/Rect Graphic")]
    public class RectGraphic : ShapeGraphic
    {
        [SerializeField] private bool m_Outline = false;
        [SerializeField, ShowIfField("m_Outline")] private float m_Thickness = 1;

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

            Rect rect = GetPixelAdjustedRect();
            
            if (m_Outline)
                CanvasMesh.AddRectOutline(vh, rect, m_Thickness, color, m_TextureRegion.UVCenter);
            else
                CanvasMesh.AddRect(vh, rect, color, m_TextureRegion.UVCenter);
        }
    }
}
