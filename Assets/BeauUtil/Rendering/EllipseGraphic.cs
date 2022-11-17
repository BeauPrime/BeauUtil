/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 Nov 2022
 * 
 * File:    EllipseGraphic.cs
 * Purpose: Ellipse graphic.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil.UI
{
    /// <summary>
    /// Ellipse graphic
    /// </summary>
    [AddComponentMenu("BeauUtil/Rendering/Ellipse Graphic")]
    public class EllipseGraphic : ShapeGraphic
    {
        [SerializeField, Range(0, 40)] private int m_Resolution = 0;

        [Space]
        [SerializeField, Range(0, 360)] private float m_StartDegrees = 0;
        [SerializeField, Range(-360, 360)] private float m_ArcDegrees = 360;
        [SerializeField, Range(0, 1)] private float m_ArcFill = 1;

        [Space]
        [SerializeField] private bool m_Outline = false;
        [SerializeField, ShowIfField("m_Outline")] private float m_Thickness = 1;

        public float StartDegrees
        {
            get { return m_StartDegrees; }
            set
            {
                if (m_StartDegrees != value)
                {
                    m_StartDegrees = Mathf.Clamp(value, 0, 360);
                    SetVerticesDirty();
                }
            }
        }

        public float ArcDegrees
        {
            get { return m_ArcDegrees; }
            set
            {
                if (m_ArcDegrees != value)
                {
                    m_ArcDegrees = Mathf.Clamp(value, 0, 360);
                    SetVerticesDirty();
                }
            }
        }

        public float ArcFill
        {
            get { return m_ArcFill; }
            set
            {
                if (m_ArcFill != value)
                {
                    m_ArcFill = Mathf.Clamp01(value);
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
            int resolution = m_Resolution;
            if (resolution <= 0)
                resolution = CanvasMesh.EstimateCurveResolution(r.width / 2, r.height / 2, this);
            
            if (m_Outline)
                CanvasMesh.AddEllipseOutline(vh, r, m_StartDegrees, m_ArcDegrees * m_ArcFill, resolution, m_Thickness, color, m_TextureRegion.UVCenter);
            else
                CanvasMesh.AddEllipse(vh, r, m_StartDegrees, m_ArcDegrees * m_ArcFill, resolution, color, m_TextureRegion.UVCenter);
        }
    }
}
