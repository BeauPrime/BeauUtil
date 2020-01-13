/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    TiledRawImage.cs
 * Purpose: Tiled raw image.
 */

using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil
{
    public class TiledRawImage : RawImage
    {
        [SerializeField]
        protected Vector2 m_UnitsPerTile = default(Vector2);

        protected override void OnRectTransformDimensionsChange()
        {
            RecalculateTiling();
            base.OnRectTransformDimensionsChange();
        }

        private void RecalculateTiling()
        {
            Rect rect = this.rectTransform.rect;
            Rect uvRect = this.uvRect;

            uvRect.width = m_UnitsPerTile.x == 0 ? 1 : rect.width / m_UnitsPerTile.x;
            uvRect.height = m_UnitsPerTile.y == 0 ? 1 : rect.height / m_UnitsPerTile.y;

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