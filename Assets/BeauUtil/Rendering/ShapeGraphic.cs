/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Oct 2022
 * 
 * File:    ShapeGraphic.cs
 * Purpose: Base for shape graphics.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil.UI
{
    /// <summary>
    /// Graphic that renders a shape.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public abstract class ShapeGraphic : MaskableGraphic, ISharedCanvasResourceListener
    {
        [NonSerialized] protected TextureRegion m_TextureRegion = default;
        [NonSerialized] private bool m_TextureDirty = false;

        protected ShapeGraphic()
        {
            useLegacyMeshGeneration = false;
        }

        public override Texture mainTexture
        {
            get { return m_TextureRegion.Texture; }
        }

        public override void Rebuild(CanvasUpdate update)
        {
            if (update == CanvasUpdate.PreRender)
            {
                if (m_TextureDirty)
                {
                    UpdateTextureRegion();
                    m_TextureDirty = false;
                }
            }

            base.Rebuild(update);
        }

        public override void SetAllDirty()
        {
            m_TextureDirty = true;
            base.SetAllDirty();
        }

        public void SetTextureDirty()
        {
            SetAllDirty();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            m_TextureDirty = true;
        }

        private void UpdateTextureRegion()
        {
            TextureRegion region = SharedCanvasResources.ResolveWhiteTextureRegion(canvas);
            if (!region.Equals(m_TextureRegion))
            {
                m_TextureRegion = region;
            }
        }
    }
}
