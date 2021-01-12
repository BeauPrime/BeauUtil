/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    ColorGroup.cs
 * Purpose: Sets colors and raycast properties on renderers and Graphics.
            This will apply to all children with a ColorGroup as well.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil
{
    /// <summary>
    /// Enables, disables, and tints renderers individually or as a group.
    /// </summary>
    [DisallowMultipleComponent, ExecuteInEditMode]
    [AddComponentMenu("BeauUtil/Rendering/Color Group")]
    public sealed partial class ColorGroup : MonoBehaviour, ICanvasRaycastFilter
    {
        #region Inspector

        [SerializeField, HideInInspector]
        private Renderer m_Renderer = null;

        [SerializeField, HideInInspector]
        private Graphic m_Graphic = null;

        [SerializeField]
        private bool m_Visible = true;

        [SerializeField]
        private ColorBlock m_Colors = ColorBlock.Default;

        [SerializeField]
        private bool m_BlocksRaycasts = true;

        [SerializeField]
        private bool m_IgnoreParentGroups = false;

        [SerializeField]
        private bool m_UseMainAlphaForVisibility = true;

        [SerializeField]
        private MaterialConfig m_MaterialConfig = null;

        [SerializeField, LayerIndex]
        private int m_EnableRaycastLayer = 0;

        [SerializeField, LayerIndex]
        private int m_DisableRaycastLayer = 2;

        #endregion // Inspector

        [NonSerialized] private List<ColorGroup> m_Children;
        [NonSerialized] private ColorGroup m_Parent;

        [NonSerialized] private bool m_RendererLocated = false;
        [NonSerialized] private SpriteRenderer m_RendererAsSpriteRenderer;

        [NonSerialized] private bool m_ConcatenatedVisibility = true;
        [NonSerialized] private bool m_ConcatenatedRaycast = true;

        [NonSerialized] private ColorBlock m_ConcatenatedColorBlock = ColorBlock.Default;

        [NonSerialized] private bool m_ValidateQueued = false;

        #region Properties

        public bool Visible
        {
            get { return m_Visible; }
            set
            {
                if (m_Visible != value)
                {
                    m_Visible = value;
                    UpdateVisibility();
                }
            }
        }

        public bool BlocksRaycasts
        {
            get { return m_BlocksRaycasts; }
            set
            {
                if (m_BlocksRaycasts != value)
                {
                    m_BlocksRaycasts = value;
                    UpdateRaycast();
                }
            }
        }

        public Color Color
        {
            get { return GetColor(); }
            set { SetColor(value); }
        }

        public bool IgnoreParentGroups
        {
            get { return m_IgnoreParentGroups; }
            set
            {
                if (m_IgnoreParentGroups != value)
                {
                    m_IgnoreParentGroups = value;
                    Refresh();
                }
            }
        }

        #endregion // Properties

        #region Get/Set Interfaces

        public Color GetColor()
        {
            return GetColor(Channel.Main);
        }

        public Color GetColor(Channel inChannel)
        {
            return m_Colors[inChannel];
        }

        public void SetColor(Color inColor)
        {
            SetColor(Channel.Main, inColor);
        }

        public void SetColor(Channel inChannel, Color inColor)
        {
            if (m_Colors[inChannel] != inColor)
            {
                m_Colors[inChannel] = inColor;
                Refresh();
            }
        }

        public float GetAlpha()
        {
            return GetAlpha(Channel.Main);
        }

        public float GetAlpha(Channel inChannel)
        {
            return m_Colors[inChannel].a;
        }

        public void SetAlpha(float inAlpha)
        {
            SetAlpha(Channel.Main, inAlpha);
        }

        public void SetAlpha(Channel inChannel, float inAlpha)
        {
            Color c = m_Colors[inChannel];
            if (c.a != inAlpha)
            {
                c.a = inAlpha;
                m_Colors[inChannel] = c;
                Refresh();
            }
        }

        #endregion // Color/Alpha

        #region Concatenated Properties

        public bool VisibleInHierarchy()
        {
            return m_ConcatenatedVisibility;
        }

        public Color ColorInHierarchy()
        {
            return ColorInHierarchy(Channel.Main);
        }

        public Color ColorInHierarchy(Channel inChannel)
        {
            return m_ConcatenatedColorBlock[inChannel];
        }

        #endregion // ConcatenatedProperties

        #region Bounds

        public Bounds Bounds
        {
            get { return m_Renderer ? m_Renderer.bounds : default(Bounds); }
        }

        public Bounds GetTotalBounds()
        {
            bool bHasBounds = m_Renderer;
            Bounds bounds = Bounds;
            if (m_Children != null)
            {
                for (int i = m_Children.Count - 1; i >= 0; --i)
                {
                    Bounds childBounds = m_Children[i].GetTotalBounds();
                    if (bHasBounds)
                    {
                        bounds.Encapsulate(childBounds);
                    }
                    else
                    {
                        bounds = childBounds;
                    }
                }
            }
            return bounds;
        }

        #endregion // Bounds

        #region Internal

        private void FindRenderer(bool inbConfigure)
        {
            Initialize();

            #if UNITY_EDITOR

            if (Application.isPlaying)
            {
                if (m_RendererLocated)
                    return;

                m_RendererLocated = true;
            }

            #else

            if (m_RendererLocated)
                return;

            m_RendererLocated = true;

            #endif // UNITY_EDITOR

            if (m_Graphic)
                return;

            if (m_Renderer)
            {
                m_RendererAsSpriteRenderer = m_Renderer as SpriteRenderer;
                return;
            }

            if (m_MaterialConfig == null)
                m_MaterialConfig = new MaterialConfig();

            m_Graphic = GetComponent<Graphic>();
            if (m_Graphic)
            {
                if (inbConfigure)
                {
                    m_Colors.Main = m_Graphic.color;
                }

                return;
            }

            m_Renderer = GetComponent<Renderer>();
            if (m_Renderer)
            {
                m_RendererAsSpriteRenderer = m_Renderer as SpriteRenderer;
                if (inbConfigure)
                {
                    if (m_RendererAsSpriteRenderer)
                    {
                        m_Colors.Main = m_RendererAsSpriteRenderer.color;
                        m_MaterialConfig.MainProperty.Name = "_Color";
                        m_MaterialConfig.MainProperty.Enabled = false;
                    }
                    else if (m_Renderer.sharedMaterial)
                    {
                        m_Renderer.GetPropertyBlock(s_SharedPropertyBlock);
                        m_MaterialConfig.ConfigureForMaterial(m_Renderer.sharedMaterial);
                        m_Colors.Main = m_MaterialConfig.MainProperty.Retrieve(s_SharedPropertyBlock);
                    }
                    else
                    {
                        m_Renderer.GetPropertyBlock(s_SharedPropertyBlock);
                        m_Colors.Main = m_MaterialConfig.MainProperty.Retrieve(s_SharedPropertyBlock);
                    }

                    if (m_Colors.Main == Color.clear)
                        m_Colors.Main = Color.white;
                }
            }
        }

        private void FindParent()
        {
            if (!m_Parent)
            {
                Transform parentTransform = transform.parent;
                if (parentTransform)
                {
                    ColorGroup potentialParent = parentTransform.GetComponentInParent<ColorGroup>();
                    if (potentialParent)
                        potentialParent.UpdateChildren();
                }
            }
        }

        [ThreadStatic]
        static private HashSet<ColorGroup> s_CachedChildSet;

        private void UpdateChildren()
        {
            if (s_CachedChildSet == null)
                s_CachedChildSet = new HashSet<ColorGroup>();

            if (m_Children != null)
            {
                for (int i = m_Children.Count - 1; i >= 0; --i)
                    s_CachedChildSet.Add(m_Children[i]);

                m_Children.Clear();
            }

            SearchForChildren(transform, false);

            if (m_Children != null)
            {
                for (int i = m_Children.Count - 1; i >= 0; --i)
                {
                    ColorGroup child = m_Children[i];

                    // if this wasn't previously attached
                    if (!s_CachedChildSet.Remove(child))
                    {
                        child.Refresh();
                    }
                }
            }

            foreach (var child in s_CachedChildSet)
            {
                child.m_Parent = null;
                child.Refresh();
            }

            s_CachedChildSet.Clear();
        }

        private void SearchForChildren(Transform inTransform, bool inbOnSelf = true)
        {
            if (inbOnSelf)
            {
                ColorGroup color = inTransform.GetComponent<ColorGroup>();
                if (color != null)
                {
                    if (m_Children == null)
                        m_Children = new List<ColorGroup>();

                    m_Children.Add(color);
                    color.m_Parent = this;
                    return;
                }
            }

            for (int i = inTransform.childCount - 1; i >= 0; --i)
            {
                SearchForChildren(inTransform.GetChild(i), true);
            }
        }

        private void Refresh()
        {
            UpdateColor();
            UpdateVisibility();
            UpdateRaycast();
        }

        private void UpdateVisibility()
        {
            if (!m_IgnoreParentGroups && m_Parent)
                UpdateVisibility(m_Parent.CalculateConcatenatedVisibility());
            else
                UpdateVisibility(true);
        }

        private void UpdateVisibility(bool inbParentVisibility)
        {
            if (m_IgnoreParentGroups)
                m_ConcatenatedVisibility = m_Visible;
            else
                m_ConcatenatedVisibility = inbParentVisibility && m_Visible;

            if (m_UseMainAlphaForVisibility)
                m_ConcatenatedVisibility &= m_ConcatenatedColorBlock.Main.a > 0;

            if (m_Graphic)
            {
                m_Graphic.enabled = m_ConcatenatedVisibility;
            }

            if (m_Renderer)
            {
                m_Renderer.enabled = m_ConcatenatedVisibility;
            }

            if (m_Children != null)
            {
                for (int i = m_Children.Count - 1; i >= 0; --i)
                    m_Children[i].UpdateVisibility(m_ConcatenatedVisibility);
            }
        }

        private void UpdateColor()
        {
            ColorBlock colorBlock = ColorBlock.Default;
            if (!m_IgnoreParentGroups && m_Parent)
            {
                m_Parent.CalculateConcatenatedColor(ref colorBlock);
            }

            UpdateColor(ref colorBlock);
        }

        private void UpdateColor(ref ColorBlock inParentColor)
        {
            if (m_IgnoreParentGroups)
            {
                m_ConcatenatedColorBlock = m_Colors;
            }
            else
            {
                inParentColor.Combine(ref m_Colors, out m_ConcatenatedColorBlock);
            }

            if (m_Renderer)
            {
                if (m_RendererAsSpriteRenderer)
                {
                    m_RendererAsSpriteRenderer.color = m_ConcatenatedColorBlock.Main;
                }

                if (m_MaterialConfig.ShouldAppply())
                {
                    Initialize();
                    m_Renderer.GetPropertyBlock(s_SharedPropertyBlock);
                    m_MaterialConfig.Apply(s_SharedPropertyBlock, ref m_ConcatenatedColorBlock);
                    m_Renderer.SetPropertyBlock(s_SharedPropertyBlock);
                }
            }

            if (m_Graphic)
                m_Graphic.color = m_ConcatenatedColorBlock.Main;

            if (m_Children != null)
            {
                for (int i = m_Children.Count - 1; i >= 0; --i)
                    m_Children[i].UpdateColor(ref m_ConcatenatedColorBlock);
            }
        }

        private void UpdateRaycast()
        {
            if (!m_IgnoreParentGroups && m_Parent)
                UpdateRaycast(m_Parent.CalculateConcatenatedRaycast());
            else
                UpdateRaycast(true);
        }

        private void UpdateRaycast(bool inbParentRaycast)
        {
            if (m_IgnoreParentGroups)
                m_ConcatenatedRaycast = m_BlocksRaycasts;
            else
                m_ConcatenatedRaycast = inbParentRaycast && m_BlocksRaycasts;

            if (m_Graphic)
            {
                m_Graphic.raycastTarget = m_ConcatenatedRaycast;
            }

            if (m_Renderer)
            {
                m_Renderer.gameObject.layer = m_ConcatenatedRaycast ? m_EnableRaycastLayer : m_DisableRaycastLayer;
            }
        }

        private bool CalculateConcatenatedVisibility()
        {
            return m_Visible && (m_Parent == null || m_Parent.CalculateConcatenatedVisibility());
        }

        private void CalculateConcatenatedColor(ref ColorBlock ioColorBlock)
        {
            m_Colors.Combine(ref ioColorBlock, out ioColorBlock);
            if (m_Parent)
            {
                m_Parent.CalculateConcatenatedColor(ref ioColorBlock);
            }
        }

        private bool CalculateConcatenatedRaycast()
        {
            return m_BlocksRaycasts && (m_Parent == null || m_Parent.CalculateConcatenatedRaycast());
        }

        #endregion // Internal

        #region Unity Events

        private void OnDidApplyAnimationProperties()
        {
            Refresh();
        }

        #if UNITY_EDITOR

        private void Reset()
        {
            FindRenderer(true);
            Refresh();
        }

        private void OnValidate()
        {
            if (!m_ValidateQueued)
            {
                UnityEditor.EditorApplication.update += this.DeferredOnValidate;
                m_ValidateQueued = true;
            }
        }

        private void DeferredOnValidate()
        {
            if (!this || !m_ValidateQueued)
                return;

            Initialize();
            FindRenderer(true);

            UpdateChildren();
            Refresh();

            m_ValidateQueued = false;
        }

        #endif // UNITY_EDITOR

        private void Awake()
        {
            FindParent();
            FindRenderer(true);
            UpdateChildren();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void OnTransformChildrenChanged()
        {
            UpdateChildren();
        }

        private void OnTransformParentChanged()
        {
            if (m_Parent)
            {
                m_Parent.UpdateChildren();
            }

            FindParent();
        }

        #endregion // Unity Events

        #region ICanvasRaycastFilter

        bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return m_BlocksRaycasts;
        }

        #endregion // ICanvasRaycastFilter

        #region Static

        [NonSerialized]
        static private bool s_Initialized;
        static private MaterialPropertyBlock s_SharedPropertyBlock;

        static private void Initialize()
        {
            if (s_Initialized)
                return;

            s_SharedPropertyBlock = new MaterialPropertyBlock();
            s_Initialized = true;
        }

        #endregion // Static
    }
}