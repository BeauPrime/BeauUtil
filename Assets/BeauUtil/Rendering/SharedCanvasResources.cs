/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Oct 2022
 * 
 * File:    SharedCanvasResources.cs
 * Purpose: Shared canvas resources.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil.UI
{
    /// <summary>
    /// Shared canvas resources.
    /// </summary>
    [AddComponentMenu("BeauUtil/Rendering/Shared Canvas Resources")]
    [RequireComponent(typeof(Canvas))]
    public class SharedCanvasResources : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private Sprite m_WhiteSprite = null;

        #endregion // Inspector

        [NonSerialized] private TextureRegion m_WhiteRegion;

        #region Sprite

        /// <summary>
        /// Texture region to use when rendering blank/shape sprites.
        /// </summary>
        public TextureRegion WhiteTextureRegion
        {
            get
            {
                if (m_WhiteSprite == null)
                    return DefaultWhiteTextureRegion;

                if (m_WhiteRegion.Texture.IsReferenceNull())
                    m_WhiteRegion = new TextureRegion(m_WhiteSprite);
                
                return m_WhiteRegion;
            }
        }

        #endregion // Sprite

        #region Global

        static private Sprite s_DefaultWhiteSprite = null;
        static private TextureRegion s_DefaultRegion;

        /// <summary>
        /// Default sprite to use when rendering blank/shape sprites.
        /// </summary>
        static public Sprite DefaultWhiteSprite
        {
            get
            {
                return s_DefaultWhiteSprite;
            }
            set
            {
                if (s_DefaultWhiteSprite != value)
                {
                    s_DefaultWhiteSprite = value;
                    s_DefaultRegion = value == null ? new TextureRegion(Texture2D.whiteTexture) : new TextureRegion(value);
                }
            }
        }

        /// <summary>
        /// Default texture region to use when rendering blank/shape sprites
        /// </summary>
        static public TextureRegion DefaultWhiteTextureRegion
        {
            get
            {
                ReinitializeGlobalWhite();
                return s_DefaultRegion;
            }
        }

        static private void ReinitializeGlobalWhite()
        {
            if (s_DefaultRegion.Texture.IsReferenceNull())
            {
                s_DefaultRegion = new TextureRegion(Texture2D.whiteTexture);
            }
        }

        #endregion // Global
    
        #region Resolve

        static public TextureRegion ResolveWhiteTextureRegion(Canvas inCanvas)
        {
            SharedCanvasResources shared;
            if (inCanvas == null || (shared = inCanvas.GetComponent<SharedCanvasResources>()) == null)
                return DefaultWhiteTextureRegion;
            return shared.WhiteTextureRegion;
        }

        #endregion // Resolve

        #region Editor

        #if UNITY_EDITOR

        static private List<ISharedCanvasResourceListener> s_ListenerWorkList;

        private void OnValidate()
        {
            bool bUpdate = false;

            if (m_WhiteSprite != null && m_WhiteRegion.Texture != m_WhiteSprite.texture)
            {
                m_WhiteRegion = new TextureRegion(m_WhiteSprite);
                bUpdate = true;
            }
            else if (m_WhiteSprite == null && m_WhiteRegion.Texture != null)
            {
                m_WhiteRegion = default(TextureRegion);
                bUpdate = true;
            }

            if (bUpdate)
            {
                List<ISharedCanvasResourceListener> workList = s_ListenerWorkList ?? (s_ListenerWorkList = new List<ISharedCanvasResourceListener>(8));
                GetComponentsInChildren<ISharedCanvasResourceListener>(false, workList);
                foreach(var shape in workList)
                {
                    shape.SetTextureDirty();
                }
                workList.Clear();
            }
        }

        #endif // UNITY_EDITOR

        #endregion // Editor
    }

    public interface ISharedCanvasResourceListener
    {
        void SetTextureDirty();
    }
}
