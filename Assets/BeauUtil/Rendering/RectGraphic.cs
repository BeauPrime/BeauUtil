/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    28 April 2021
 * 
 * File:    RectGraphic.cs
 * Purpose: Plain rectangle graphic.
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil
{
    /// <summary>
    /// Plain rectangle graphic.
    /// </summary>
    [AddComponentMenu("BeauUtil/Rendering/Rect Graphic")]
    public class RectGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);

            Color32 color32 = color;
            vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(0f, 0f));
            vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(0f, 1f));
            vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(1f, 1f));
            vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(1f, 0f));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }
}
