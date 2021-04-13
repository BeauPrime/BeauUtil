/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    5 Oct 2020
 * 
 * File:    ScrollSensitivityFix.cs
 * Purpose: Fixes scroll speed on windows devices.
 */

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil
{
    [AddComponentMenu("BeauUtil/Patches/Scroll Sensitivity Fix")]
    public sealed class ScrollSensitivityFix : MonoBehaviour
    {
        [SerializeField, HideInInspector] private ScrollRect m_ScrollRect = null;
        [SerializeField, HideInInspector] private TMP_InputField m_TMPInput = null;

        private void Awake()
        {
            if (!IsWindows)
            {
                Destroy(this);
                return;
            }

            this.CacheComponent(ref m_ScrollRect);
            this.CacheComponent(ref m_TMPInput);

            if (m_ScrollRect)
                m_ScrollRect.scrollSensitivity *= WindowsScrollMultiplier;
            if (m_TMPInput)
                m_TMPInput.scrollSensitivity *= WindowsScrollMultiplier;
        }

        /// <summary>
        /// Sensitivity multiplier on windows machines.
        /// </summary>
        public const float WindowsScrollMultiplier = 20;

        /// <summary>
        /// Returns if running on a windows platform.
        /// </summary>
        static public readonly bool IsWindows = GetIsWindows();

        /// <summary>
        /// Returns the correct sensitivity level for the current platform,
        /// given the default sensitivity.
        /// </summary>
        static public float Correct(float inSenstivity)
        {
            return IsWindows ? inSenstivity * WindowsScrollMultiplier : inSenstivity;
        }

        static private bool GetIsWindows()
        {
            switch(Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return true;

                case RuntimePlatform.WebGLPlayer:
                default:
                    try
                    {
                        return SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows;
                    }
                    catch(Exception e)
                    {
                        Debug.LogException(e);
                        return false;
                    }
            }
        }

        #if UNITY_EDITOR

        private void Reset()
        {
            this.CacheComponent(ref m_ScrollRect);
            this.CacheComponent(ref m_TMPInput);
        }

        private void OnValidate()
        {
            this.CacheComponent(ref m_ScrollRect);
            this.CacheComponent(ref m_TMPInput);
        }

        #endif // UNITY_EDITOR
    }
}