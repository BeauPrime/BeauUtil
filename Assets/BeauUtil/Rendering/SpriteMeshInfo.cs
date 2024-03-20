/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    25 June 2023
 * 
 * File:    SpriteMeshInfo.cs
 * Purpose: Sprite vertex data.
*/

using System;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Sprite mesh vertex/index information.
    /// </summary>
    [Serializable]
    public struct SpriteMeshInfo
    {
        public Vector2[] Vertex;
        public Vector2[] UV;
        public ushort[] Index;
        public Texture2D Texture;
        public Bounds Bounds;

        public SpriteMeshInfo(Sprite inSource)
        {
            Vertex = inSource.vertices;
            UV = inSource.uv;
            Index = inSource.triangles;
            Texture = inSource.texture;
            Bounds = inSource.bounds;
        }
    }
}