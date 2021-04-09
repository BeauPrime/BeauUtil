/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Apr 2021
 * 
 * File:    DMHeaderUI.cs
 * Purpose: Debug menu header display
 */

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BeauUtil.Debugger
{
    public class DMHeaderUI : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private TMP_Text m_HeaderText = null;
        [SerializeField] private Button m_BackButton = null;

        #endregion // Inspector

        public void Init(DMHeaderInfo inHeaderInfo, bool inbHasBack)
        {
            m_HeaderText.SetText(inHeaderInfo.Label);
            m_BackButton.gameObject.SetActive(inbHasBack);
        }

        public void SetBackCallback(UnityAction inCallback)
        {
            m_BackButton.onClick.AddListener(inCallback);
        }
    }
}