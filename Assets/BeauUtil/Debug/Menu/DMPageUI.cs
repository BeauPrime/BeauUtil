/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    25 Aug 2021
 * 
 * File:    DMPageUI.cs
 * Purpose: Paging interface for debug UI.
 */

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil.Debugger
{
    public class DMPageUI : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private TMP_Text m_PageLabel = null;
        [SerializeField] private Button m_LeftButton = null;
        [SerializeField] private CanvasGroup m_LeftButtonGroup = null;
        [SerializeField] private Button m_RightButton = null;
        [SerializeField] private CanvasGroup m_RightButtonGroup = null;
        
        #endregion // Inspector

        [NonSerialized] private int m_CurrentPage;
        private Action<int> m_PageChangedCallback;

        private void Awake()
        {
            m_LeftButton.onClick.AddListener(OnLeftClicked);
            m_RightButton.onClick.AddListener(OnRightClicked);
        }

        public void UpdatePage(int inPageIndex, int inMaxPages)
        {
            m_CurrentPage = inPageIndex;

            m_PageLabel.text = string.Format("{0}/{1}", inPageIndex + 1, inMaxPages);
            bool bLeft = inPageIndex > 0;
            bool bRight = inPageIndex < inMaxPages - 1;

            m_LeftButtonGroup.alpha = bLeft ? 1 : 0.5f;
            m_LeftButtonGroup.interactable = m_LeftButtonGroup.blocksRaycasts = bLeft;

            m_RightButtonGroup.alpha = bRight ? 1 : 0.5f;
            m_RightButtonGroup.interactable = m_RightButtonGroup.blocksRaycasts = bRight;
        }

        public void SetPageCallback(Action<int> inCallback)
        {
            m_PageChangedCallback = inCallback;
        }

        private void OnLeftClicked()
        {
            m_PageChangedCallback?.Invoke(m_CurrentPage - 1);
        }

        private void OnRightClicked()
        {
            m_PageChangedCallback?.Invoke(m_CurrentPage + 1);
        }
    }
}