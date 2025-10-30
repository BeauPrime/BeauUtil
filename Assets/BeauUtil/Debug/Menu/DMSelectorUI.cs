/*
 * Copyright (C) 2025. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 Oct 2025
 * 
 * File:    DMSelectorUI.cs
 * Purpose: Selector element for a debug menu.
 */

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil.Debugger
{
    [RequireComponent(typeof(DMInteractableUI))]
    public class DMSelectorUI : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private LayoutGroup m_IndentGroup = null;
        [SerializeField] private TMP_Text m_Label = null;
        [SerializeField] private TMP_Text m_Value = null;
        [SerializeField] private Button m_DecreaseButton = null;
        [SerializeField] private Button m_IncreaseButton = null;
        [SerializeField] private CanvasGroup m_DecreaseButtonGroup = null;
        [SerializeField] private CanvasGroup m_IncreaseButtonGroup = null;

        #endregion // Inspector

        [NonSerialized] public DMInteractableUI Interactable;

        [NonSerialized] public int ElementIndex;
        [NonSerialized] private int m_LastValue;
        [NonSerialized] private int m_LastIndex;
        [NonSerialized] private int m_IndexCount;
        [NonSerialized] private int m_OriginalIndent;

        private Action<DMSelectorUI, int> m_OnIndexChanged;

        private void Awake()
        {
            Interactable = GetComponent<DMInteractableUI>();
            m_DecreaseButton.onClick.AddListener(OnClickDecrease);
            m_IncreaseButton.onClick.AddListener(OnClickIncrease);
            m_OriginalIndent = m_IndentGroup.padding.left;
        }

        public void Initialize(int inElementIndex, DMElementInfo inInfo, Action<DMSelectorUI, int> inOnUpdated, int inIndent, int inInteractableIndex, DMMenuUI inMenuUI)
        {
            if (!Interactable)
            {
                Awake();
            }

            ElementIndex = inElementIndex;

            RectOffset padding = m_IndentGroup.padding;
            padding.left = m_OriginalIndent + inIndent;
            m_IndentGroup.padding = padding;

            Interactable.Initialize(inInfo, inMenuUI);
            Interactable.InteractableIndex = inInteractableIndex;

            m_Label.SetText(inInfo.Label);

            m_IndexCount = inInfo.Selector.Labels.Length;

            m_OnIndexChanged = inOnUpdated;

            UpdateValue(inInfo.Selector.Getter(), inInfo.Selector, true);
        }

        private void OnClickIncrease()
        {
            AdjustValueInTicks(1);
        }

        private void OnClickDecrease()
        {
            AdjustValueInTicks(-1);
        }

        /// <summary>
        /// Returns the last value.
        /// </summary>
        public int ValueState()
        {
            return m_LastValue;
        }

        /// <summary>
        /// Updates the currently displayed value.
        /// </summary>
        public bool UpdateValue(int inRawValue, DMSelectorInfo inInfo)
        {
            return UpdateValue(inRawValue, inInfo, false);
        }

        /// <summary>
        /// Updates the currently displayed value by the given number of ticks.
        /// </summary>
        public void AdjustValueInTicks(int inTickCount)
        {
            if (inTickCount != 0)
            {
                int nextIndex = Math.Clamp(m_LastIndex + inTickCount, 0, m_IndexCount - 1);
                m_OnIndexChanged(this, nextIndex);
            }
        }

        private bool UpdateValue(int inRawValue, DMSelectorInfo inInfo, bool inbForce)
        {
            if (!inbForce && m_LastValue == inRawValue)
                return false;

            m_LastValue = inRawValue;

            int index = DMSelectorInfo.GetIndex(inInfo, inRawValue);
            m_LastIndex = index;

            string display = index < 0 ? "(?)" : inInfo.Labels[index];
            m_Value.SetText(display);

            m_DecreaseButton.interactable = index > 0;
            m_IncreaseButton.interactable = index < inInfo.Labels.Length - 1;

            m_DecreaseButtonGroup.alpha = m_DecreaseButton.interactable ? 1 : 0.5f;
            m_IncreaseButtonGroup.alpha = m_IncreaseButton.interactable ? 1 : 0.5f;

            return true;
        }
    }
}