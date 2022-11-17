/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    TiledRawImage.cs
 * Purpose: Tiled raw image.
 */

using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil.UI
{
    [AddComponentMenu("BeauUtil/Rendering/Tiled RawImage")]
    public class TiledRawImage : RawImage
    {
        #region Inspector

        [SerializeField]
        protected Vector2 m_UnitsPerTile = new Vector2(100, 100);

        [SerializeField]
        protected Vector2 m_Offset = default(Vector2);

        #endregion // Inspector

        public Vector2 UnitsPerTile
        {
            get { return m_UnitsPerTile; }
            set
            {
                if (m_UnitsPerTile != value)
                {
                    m_UnitsPerTile = value;
                    RecalculateTiling();
                }
            }
        }

        public Vector2 Offset
        {
            get { return m_Offset; }
            set
            {
                if (m_Offset != value)
                {
                    m_Offset = value;
                    RecalculateTiling();
                }
            }
        }

        public Vector2 NormalizedOffset
        {
            get
            {
                Vector2 offset = m_Offset;
                if (m_UnitsPerTile.x != 0)
                    offset.x /= m_UnitsPerTile.x;
                if (m_UnitsPerTile.y != 0)
                    offset.y /= m_UnitsPerTile.y;
                return offset;
            }
            set
            {
                Vector2 offset = value;
                if (m_UnitsPerTile.x != 0)
                    offset.x *= m_UnitsPerTile.x;
                if (m_UnitsPerTile.y != 0)
                    offset.y *= m_UnitsPerTile.y;
                
                if (m_Offset != offset)
                {
                    m_Offset = offset;
                    RecalculateTiling();
                }
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            RecalculateTiling();
            base.OnRectTransformDimensionsChange();
        }

        private void RecalculateTiling()
        {
            Rect rect = this.rectTransform.rect;
            Rect uvRect = this.uvRect;

            if (m_UnitsPerTile.x == 0)
            {
                uvRect.width = 1;
                uvRect.x = m_Offset.x;
            }
            else
            {
                uvRect.width = rect.width / m_UnitsPerTile.x;
                uvRect.x = (m_Offset.x / m_UnitsPerTile.x) % 1;
            }

            if (m_UnitsPerTile.y == 0)
            {
                uvRect.height = 1;
                uvRect.y = m_Offset.y;
            }
            else
            {
                uvRect.height = rect.height / m_UnitsPerTile.y;
                uvRect.y = (m_Offset.y / m_UnitsPerTile.y) % 1;
            }

            this.uvRect = uvRect;
        }

        #if UNITY_EDITOR

        protected override void OnValidate()
        {
            RecalculateTiling();
            base.OnValidate();
        }

        [UnityEditor.CustomEditor(typeof(TiledRawImage)), UnityEditor.CanEditMultipleObjects]
        private class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
            }
        }

        #endif
    }
}