/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    20 Nov 2020
 * 
 * File:    ColorGroupGraphic.cs
 * Purpose: Invisible Graphic with a ColorGroup backing.
            For use with Selectable color transitions.
 */

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace BeauUtil
{
    [RequireComponent(typeof(ColorGroup))]
    public class ColorGroupGraphic : Graphic
    {
        #region Inspector

        [SerializeField, Required(ComponentLookupDirection.Self)] private ColorGroup m_ColorGroup = null;

        #endregion // Inspector

        private Coroutine m_CrossFade;
        private Action<Color> m_ColorSetter;
        private Action<float> m_AlphaSetter;

        public ColorGroupGraphic()
        {
            useLegacyMeshGeneration = false;
        }

        public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB)
        {
            if (!useAlpha && !useRGB)
                return;

            Color currentColor = m_ColorGroup.Color;
            if (currentColor == targetColor)
                return;
            
            if (!useRGB)
            {
                targetColor.r = currentColor.r;
                targetColor.g = currentColor.g;
                targetColor.b = currentColor.b;
            }
            if (!useAlpha)
            {
                targetColor.a = currentColor.a;
            }

            if (duration <= 0)
            {
                if (m_CrossFade != null)
                {
                    StopCoroutine(m_CrossFade);
                    m_CrossFade = null;
                }
                m_ColorGroup.Color = targetColor;
                return;
            }

            m_AlphaSetter = m_AlphaSetter ?? UpdateAlpha;
            m_ColorSetter = m_ColorSetter ?? UpdateColor;

            if (m_CrossFade != null)
            {
                StopCoroutine(m_CrossFade);
            }

            if (useRGB)
            {
                m_CrossFade = StartCoroutine(TweenColor(currentColor, targetColor, m_ColorSetter, duration, ignoreTimeScale));
            }
            else
            {
                m_CrossFade = StartCoroutine(TweenAlpha(currentColor.a, targetColor.a, m_AlphaSetter, duration, ignoreTimeScale));
            }
        }

        static private IEnumerator TweenColor(Color inStart, Color inEnd, Action<Color> inSetter, float inDuration, bool inbIgnoreTimeScale)
        {
            float increment = 1f / inDuration;
            float percent = 0;
            Color current;
            while(percent < 1)
            {
                percent += (inbIgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * increment;
                if (percent > 1)
                    percent = 1;
                
                current = Color.Lerp(inStart, inEnd, percent);
                inSetter(current);
                yield return null;
            }
        }

        static private IEnumerator TweenAlpha(float inStart, float inEnd, Action<float> inSetter, float inDuration, bool inbIgnoreTimeScale)
        {
            float increment = 1f / inDuration;
            float percent = 0;
            float current;
            while(percent < 1)
            {
                percent += (inbIgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * increment;
                if (percent > 1)
                    percent = 1;
                
                current = Mathf.Lerp(inStart, inEnd, percent);
                inSetter(current);
                yield return null;
            }
        }

        private void UpdateAlpha(float inAlpha)
        {
            m_ColorGroup.SetAlpha(inAlpha);
        }

        private void UpdateColor(Color inColor)
        {
            m_ColorGroup.SetColor(inColor);
        }

        public override bool raycastTarget
        {
            get
            {
                return m_ColorGroup.BlocksRaycasts;
            }
            set
            {
                m_ColorGroup.BlocksRaycasts = value;
            }
        }

        public override void SetMaterialDirty() { return; }
        public override void SetVerticesDirty() { return; }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

        protected override void UpdateGeometry()
        {
        }

        #if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.Reset();
            m_ColorGroup = this.CacheComponent(ref m_ColorGroup);
        }

        [CustomEditor(typeof(ColorGroupGraphic)), CanEditMultipleObjects]
        private class Inspector : Editor
        {
            public override void OnInspectorGUI()
            {
                UnityEditor.SerializedObject obj = new UnityEditor.SerializedObject(targets);
                obj.ApplyModifiedProperties();
            }
        }

        #endif // UNITY_EDITOR
    }
}