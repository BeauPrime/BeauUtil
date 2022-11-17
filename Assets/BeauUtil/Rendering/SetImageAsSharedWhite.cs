/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 Nov 2022
 * 
 * File:    SetImageAsSharedWhite.cs
 * Purpose: Ensures the given image uses the shared white pixel texture
            associated with the current canvas.
 */

using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BeauUtil.UI
{
    [AddComponentMenu("BeauUtil/Rendering/Set Image as Shared White Texture")]
    [RequireComponent(typeof(Image))]
    [ExecuteAlways]
    public class SetImageAsSharedWhite : UIBehaviour, ISharedCanvasResourceListener
    {
        [NonSerialized] private Canvas m_Canvas;
        [NonSerialized] private Image m_Image;

        public void SetTextureDirty()
        {
            m_Image.sprite = SharedCanvasResources.ResolveWhiteTextureRegion(m_Canvas).Sprite;
        }

        protected override void OnEnable()
        {
            if (!m_Canvas)
                m_Canvas = GetComponentInParent<Canvas>();
            this.CacheComponent(ref m_Image);

            SetTextureDirty();
        }

        protected override void OnTransformParentChanged()
        {
            m_Canvas = GetComponentInParent<Canvas>();
            SetTextureDirty();
        }
    }
}