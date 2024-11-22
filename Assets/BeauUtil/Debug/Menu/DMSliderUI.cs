/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 Apr 2023
 * 
 * File:    DMSliderUI.cs
 * Purpose: Slider element for a debug menu.
 */

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil.Debugger
{
    [RequireComponent(typeof(DMInteractableUI))]
    public class DMSliderUI : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private LayoutGroup m_IndentGroup = null;
        [SerializeField] private TMP_Text m_Label = null;
        [SerializeField] private TMP_Text m_Value = null;
        [SerializeField] private Slider m_Slider = null;
        
        #endregion // Inspector

        [NonSerialized] public DMInteractableUI Interactable;

        [NonSerialized] public int ElementIndex;
        [NonSerialized] private float m_LastValue;
        [NonSerialized] private int m_OriginalIndent;
        [NonSerialized] private float m_RemappedIncrement;

        private Action<DMSliderUI, float> m_OnValueChanged;

        private void Awake()
        {
            Interactable = GetComponent<DMInteractableUI>();
            m_Slider.onValueChanged.AddListener(OnChanged);
            m_OriginalIndent = m_IndentGroup.padding.left;
        }

        public void Initialize(int inElementIndex, DMElementInfo inInfo, Action<DMSliderUI, float> inOnUpdated, int inIndent, int inInteractableIndex, DMMenuUI inMenuUI)
        {
            if (!Interactable) {
                Awake();
            }

            ElementIndex = inElementIndex;

            RectOffset padding = m_IndentGroup.padding;
            padding.left = m_OriginalIndent + inIndent;
            m_IndentGroup.padding = padding;

            Interactable.Initialize(inInfo, inMenuUI);
            Interactable.InteractableIndex = inInteractableIndex;

            m_Label.SetText(inInfo.Label);

            m_OnValueChanged = inOnUpdated;

            Configure(inInfo.Slider);
            UpdateValue(inInfo.Slider.Getter(), inInfo.Slider, true);
        }

        private void Configure(DMSliderInfo inInfo)
        {
            m_Slider.SetValueWithoutNotify(0);
            int incrementCount = DMSliderRange.GetTotalIncrements(inInfo.Range);
            if (incrementCount <= 0)
            {
                m_Slider.wholeNumbers = false;
                m_Slider.maxValue = 1;
                m_RemappedIncrement = 0.1f;
            }
            else
            {
                m_Slider.maxValue = incrementCount;
                m_Slider.wholeNumbers = true;
                m_RemappedIncrement = 1;
            }
        }

        private void OnChanged(float inValue)
        {
            m_OnValueChanged(this, inValue / m_Slider.maxValue);
        }

        /// <summary>
        /// Returns the last value.
        /// </summary>
        public float ValueState()
        {
            return m_LastValue;
        }

        /// <summary>
        /// Updates the currently displayed value.
        /// </summary>
        public bool UpdateValue(float inRawValue, DMSliderInfo inInfo)
        {
            return UpdateValue(inRawValue, inInfo, false);
        }

        /// <summary>
        /// Updates the currently displayed value by the given number of ticks.
        /// </summary>
        public void AdjustValueInTicks(int inTickCount)
        {
            float currentValue = m_Slider.value;
            float nextValue = currentValue + m_RemappedIncrement * inTickCount;
            nextValue = Mathf.Clamp(nextValue, 0, m_Slider.maxValue);
            m_Slider.value = nextValue;
        }

        private bool UpdateValue(float inRawValue, DMSliderInfo inInfo, bool inbForce)
        {
            if (!inbForce && m_LastValue == inRawValue)
                return false;

            m_LastValue = inRawValue;
            
            float percentage = DMSliderRange.GetPercentage(inInfo.Range, inRawValue);
            float val = percentage * m_Slider.maxValue;
            if (m_Slider.wholeNumbers)
            {
                val = (float) Math.Round(val);
            }
            
            m_Slider.SetValueWithoutNotify(val);

            string display = inInfo.Label?.Invoke(inRawValue);
            inRawValue.ToString();
            m_Value.SetText(display);

            return true;
        }
    }
}