/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Apr 2021
 * 
 * File:    DMButtonUI.cs
 * Purpose: Button/toggle/submenu display for the debug menu
 */

using System;
using BeauUtil.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeauUtil.Debugger
{
    public sealed class DMInteractableUI : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private CanvasGroup m_FullGroup = null;
        [SerializeField] private float m_DisabledAlpha = 0.5f;
        [SerializeField] private Graphic m_MainRaycaster = null;
        [SerializeField] private PointerListener m_PointerListener;

        [SerializeField] public Selectable Selectable = null;

        #endregion // Inspector

        [NonSerialized] private DMMenuUI m_MenuUI;
        [NonSerialized] public int InteractableIndex;
        [NonSerialized] private bool m_LastEnabled;

        #region Internal

        private void Awake()
        {
            if (m_PointerListener != null)
            {
                m_PointerListener.onPointerEnter.Register(OnPointerEnter);
            }
        }

        internal void SetInteractive(bool inbEnabled, bool inbForce)
        {
            if (!inbForce && m_LastEnabled == inbEnabled)
                return;

            m_LastEnabled = inbEnabled;
            m_FullGroup.interactable = m_FullGroup.blocksRaycasts = inbEnabled;
            m_FullGroup.alpha = inbEnabled ? 1 : m_DisabledAlpha;
            if (m_MainRaycaster)
            {
                m_MainRaycaster.raycastTarget = inbEnabled;
            }
        }

        #endregion // Internal

        /// <summary>
        /// Initializes the button with the given element info.
        /// </summary>
        public void Initialize(DMElementInfo inInfo, DMMenuUI inMenu)
        {
            SetInteractive(DMInfo.EvaluateOptionalPredicate(inInfo.Predicate), true);
            m_MenuUI = inMenu;
        }

        /// <summary>
        /// Updates the interactive state of the button.
        /// </summary>
        public void UpdateInteractive(bool inbEnabled)
        {
            SetInteractive(inbEnabled, false);
        }

        private void OnPointerEnter(PointerEventData eventData)
        {
            m_MenuUI.OnInteractableHover(this);
        }
    }
}