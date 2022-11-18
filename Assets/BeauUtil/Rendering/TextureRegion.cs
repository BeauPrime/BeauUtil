/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Oct 2022
 * 
 * File:    TextureRegion.cs
 * Purpose: Subsection of a texture.
*/

using System;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Texture subsection.
    /// </summary>
    [Serializable]
    public struct TextureRegion : IEquatable<TextureRegion>
    {
        public Texture2D Texture;
        public Sprite Sprite;
        public Rect UVRect;
        public Vector2 UVCenter;

        public TextureRegion(Texture2D inTexture)
        {
            Texture = inTexture;
            Sprite = null;
            UVRect = new Rect(0, 0, 1, 1);
            UVCenter = new Vector2(0.5f, 0.5f);
        }

        public TextureRegion(Texture2D inTexture, Rect inRect)
        {
            Texture = inTexture;
            Sprite = null;
            UVRect = inRect;
            UVCenter = inRect.center;
        }

        public TextureRegion(Sprite inSprite)
        {
            Texture = inSprite.texture;
            Sprite = inSprite;
            if (inSprite.packed && inSprite.packingMode == SpritePackingMode.Tight)
            {
                throw new ArgumentException(string.Format("TextureRegion does not support tightly packed sprites (sprite provided: {0})", inSprite.name), "inSprite");
            }
            else
            {
                Rect r = inSprite.textureRect;
                Vector2 texelSize = Texture.texelSize;
                r.x *= texelSize.x;
                r.y *= texelSize.y;
                r.width *= texelSize.x;
                r.height *= texelSize.y;
                UVRect = r;
            }
            UVCenter = UVRect.center;
        }

        public bool Equals(TextureRegion other)
        {
            return object.ReferenceEquals(Texture, other.Texture)
                && UVRect == other.UVRect
                && UVCenter == other.UVCenter;
        }
    }
}
