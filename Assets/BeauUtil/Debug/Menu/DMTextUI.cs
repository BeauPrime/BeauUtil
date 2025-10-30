/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Apr 2021
 * 
 * File:    DMTextUI.cs
 * Purpose: Text element for a debug menu.
 */

using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil.Debugger
{
    public class DMTextUI : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private LayoutGroup m_IndentGroup = null;
        [SerializeField] private TMP_Text m_Label = null;
        [SerializeField] private TMP_Text m_Value = null;
        
        #endregion // Inspector

        [NonSerialized] public int ElementIndex;

        public void Initialize(int inElementIndex, DMElementInfo inInfo, int inIndent)
        {
            ElementIndex = inElementIndex;

            RectOffset padding = m_IndentGroup.padding;
            padding.left = inIndent;
            m_IndentGroup.padding = padding;

            m_Label.SetText(inInfo.Label);

            DMTextInfo textInfo = inInfo.Text;

            StringBuilder sb = DMInfo.SharedTextBuilder;
            sb.Clear();

            if (textInfo.Text != null)
            {
                sb.Append(textInfo.Text);
            }
            else if (textInfo.Getter != null)
            {
                sb.Append(textInfo.Getter());
            }
            else if (textInfo.GetterSB != null)
            {
                textInfo.GetterSB(sb);
            }

            if (sb.Length > 0)
            {
                m_Value.gameObject.SetActive(true);
                m_Value.SetText(sb);
                sb.Clear();
            }
            else
            {
                m_Value.gameObject.SetActive(false);
            }
        }

        public void UpdateValue(string inValue)
        {
            m_Value.SetText(inValue);
        }

        public void UpdateValue(StringBuilder inValue)
        {
            m_Value.SetText(inValue);
        }
    }
}