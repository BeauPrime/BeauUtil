/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Apr 2021
 * 
 * File:    DMButtonUI.cs
 * Purpose: Button/toggle/submenu display for the debug menu
 */

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil.Debugger
{
    public class DMButtonUI : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private LayoutGroup m_IndentGroup = null;
        [SerializeField] private TMP_Text m_Label = null;
        [SerializeField] private Button m_Button = null;
        [SerializeField] private CanvasGroup m_FullGroup = null;
        [SerializeField] private Graphic m_ButtonBG = null;

        [Header("State")]
        [SerializeField] private Color m_ToggleOffColor = Color.white;
        [SerializeField] private Color m_ToggleOnColor = Color.yellow;
        [SerializeField] private float m_DisabledAlpha = 0.5f;

        #endregion // Inspector

        [NonSerialized] public int ElementIndex;
        [NonSerialized] private bool m_LastEnabled;
        [NonSerialized] private bool m_LastToggle;

        private Action<DMButtonUI> m_OnClick;

        #region Internal

        private void Awake()
        {
            m_Button.onClick.AddListener(OnClick);
        }

        private void SetInteractive(bool inbEnabled, bool inbForce)
        {
            if (!inbForce && m_LastEnabled == inbEnabled)
                return;

            m_LastEnabled = inbEnabled;
            m_FullGroup.interactable = m_FullGroup.blocksRaycasts = inbEnabled;
            m_FullGroup.alpha = inbEnabled ? 1 : m_DisabledAlpha;
            m_ButtonBG.raycastTarget = inbEnabled;
        }

        private void SetToggleState(bool inbState, bool inbForce)
        {
            if (!inbForce && m_LastToggle == inbState)
                return;

            m_LastToggle = inbState;
            m_ButtonBG.color = inbState ? m_ToggleOnColor : m_ToggleOffColor;
        }

        private void OnClick()
        {
            m_OnClick(this);
        }

        #endregion // Internal

        /// <summary>
        /// Initializes the button with the given element info.
        /// </summary>
        public void Initialize(int inElementIndex, DMElementInfo inInfo, Action<DMButtonUI> inOnClick, int inIndent)
        {
            ElementIndex = inElementIndex;
            m_OnClick = inOnClick;
            m_Label.SetText(inInfo.Label);

            RectOffset padding = m_IndentGroup.padding;
            padding.left = inIndent;
            m_IndentGroup.padding = padding;

            SetInteractive(DMInfo.EvaluateOptionalPredicate(inInfo.Predicate), true);

            switch(inInfo.Type)
            {
                case DMElementType.Button:
                case DMElementType.Submenu:
                    {
                        SetToggleState(false, true);
                        break;
                    }

                case DMElementType.Toggle:
                    {
                        SetToggleState(inInfo.Toggle.Getter(), true);
                        break;
                    }
            }
        }

        /// <summary>
        /// Updates the interactive state of the button.
        /// </summary>
        public void UpdateInteractive(bool inbEnabled)
        {
            SetInteractive(inbEnabled, false);
        }

        /// <summary>
        /// Returns the last toggle state.
        /// </summary>
        public bool ToggleState()
        {
            return m_LastToggle;
        }

        /// <summary>
        /// Updates the toggle state of the button.
        /// </summary>
        public void UpdateToggleState(bool inbState)
        {
            SetToggleState(inbState, false);
        }
    }
}