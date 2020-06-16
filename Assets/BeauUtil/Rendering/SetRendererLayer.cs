/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    SetRendererLayer.cs
 * Purpose: Sets the sorting layer and order of a Renderer.
 */

using UnityEngine;

namespace BeauUtil
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Renderer))]
    [AddComponentMenu("BeauUtil/Rendering/Set Renderer Layer")]
    public sealed class SetRendererLayer : MonoBehaviour
    {
        #region Inspector

        [SerializeField, SortingLayer]
        private int m_Layer = 0;

        [SerializeField]
        private int m_Order = 0;

        #endregion

        private void Awake()
        {
            Apply();
        }

        private void Apply()
        {
            Renderer r = GetComponent<Renderer>();
            r.sortingLayerID = m_Layer;
            r.sortingOrder = m_Order;
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            Apply();
        }
        #endif
    }
}