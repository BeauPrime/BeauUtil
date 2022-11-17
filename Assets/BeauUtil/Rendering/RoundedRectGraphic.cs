/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    19 Oct 2022
 * 
 * File:    RoundedRectGraphic.cs
 * Purpose: Rounded rectangle graphic.
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
    [AddComponentMenu("BeauUtil/Rendering/Rounded Rect Graphic")]
    public class RoundedRectGraphic : ShapeGraphic
    {
        [SerializeField] private float m_CornerRadius = 15;
        [SerializeField, Range(0, 40)] private int m_Resolution = 0;
        
        [Space]
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

            var r = GetPixelAdjustedRect();
            int resolution = m_Resolution;
            if (resolution <= 0)
                resolution = CanvasMesh.EstimateCurveResolution(m_CornerRadius, this);
            
            if (m_Outline)
                CanvasMesh.AddRoundedRectOutline(vh, r, m_CornerRadius, resolution, m_Thickness, color, m_TextureRegion.UVCenter);
            else
                CanvasMesh.AddRoundedRect(vh, r, m_CornerRadius, resolution, color, m_TextureRegion.UVCenter);
        }
    }
}
