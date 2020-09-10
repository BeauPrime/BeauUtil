/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    3 Sept 2020
 * 
 * File:    ConsoleBuildInfo.cs
 * Purpose: Displays build information.
 */

using System;
using UnityEngine;
using BeauUtil;
using TMPro;

namespace BeauUtil.Debugger
{
    public class ConsoleBuildInfo : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private TMP_Text m_BuildIdText = null;
        [SerializeField] private TMP_Text m_BuildDateText = null;
        [SerializeField] private TMP_Text m_BuildVersionText = null;
        [SerializeField] private TMP_Text m_BuildTagText = null;

        #endregion // Inspector

        [NonSerialized] private bool m_Populated;

        private void Awake()
        {
            BuildInfo.Load(Populate);
        }

        private void OnEnable()
        {
            if (!m_Populated && BuildInfo.IsAvailable())
                Populate();
        }

        private void Populate()
        {
            if (m_Populated)
                return;

            if (m_BuildIdText)
            {
                m_BuildIdText.SetText(BuildInfo.Id());
            }

            if (m_BuildDateText)
            {
                m_BuildDateText.SetText(BuildInfo.Date());
            }

            if (m_BuildVersionText)
            {
                m_BuildVersionText.SetText(BuildInfo.BundleVersion());
            }

            if (m_BuildTagText)
            {
                m_BuildTagText.SetText(BuildInfo.Tag());
            }

            m_Populated = true;
        }
    }
}