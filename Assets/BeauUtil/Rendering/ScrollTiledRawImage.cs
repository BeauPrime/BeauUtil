/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    31 July 2020
 * 
 * File:    ScrollTiledRawImage.cs
 * Purpose: Scrolls a tiled raw image over time.
 */

using UnityEngine;
using System;

namespace BeauUtil.UI
{
    [AddComponentMenu("BeauUtil/Rendering/Scroll TiledRawImage")]
    [RequireComponent(typeof(TiledRawImage))]
    public class ScrollTiledRawImage : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private Vector2 m_ScrollSpeed = default(Vector2);

        [SerializeField]
        private bool m_RandomizeInitialOffset = false;

        #endregion // Inspector

        [NonSerialized] private TiledRawImage m_RawImage;

        private void OnEnable()
        {
            m_RawImage = GetComponent<TiledRawImage>();
            
            if (m_RandomizeInitialOffset)
            {
                Vector2 normalizedOffset = RNG.Instance.NextVector2(Vector2.zero, Vector2.one);
                m_RawImage.NormalizedOffset = normalizedOffset;
            }
        }

        private void LateUpdate()
        {
            Scroll(Time.deltaTime);
        }

        public Vector2 ScrollSpeed
        {
            get { return m_ScrollSpeed; }
            set { m_ScrollSpeed = value; }
        }

        public bool RandomizeInitialOffset
        {
            get { return m_RandomizeInitialOffset; }
            set { m_RandomizeInitialOffset = value; }
        }

        public void Scroll(float inDeltaTime)
        {
            Vector2 offset = m_RawImage.Offset;
            offset.x = (offset.x + m_ScrollSpeed.x * inDeltaTime) % m_RawImage.UnitsPerTile.x;
            offset.y = (offset.y + m_ScrollSpeed.y * inDeltaTime) % m_RawImage.UnitsPerTile.y;
            m_RawImage.Offset = offset;
        }
    }
}