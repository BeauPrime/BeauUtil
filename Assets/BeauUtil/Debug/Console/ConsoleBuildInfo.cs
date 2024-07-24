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
using TMPro;
using System.Text;

namespace BeauUtil.Debugger
{
    public class ConsoleBuildInfo : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private TMP_Text m_BuildIdText = null;
        [SerializeField] private TMP_Text m_BuildDateText = null;
        [SerializeField] private TMP_Text m_BuildVersionText = null;
        [SerializeField] private TMP_Text m_BuildTagText = null;
        [SerializeField] private TMP_Text m_BuildBranchText = null;

        [Header("Condensed")]
        [SerializeField] private TMP_Text m_BuildInfoCondensedText = null;
        [SerializeField] private bool m_CondensedUseBundleVersion = true;

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

            if (m_BuildBranchText)
            {
                m_BuildBranchText.SetText(BuildInfo.Branch());
            }

            if (m_BuildInfoCondensedText)
            {
                StringBuilder sb = new StringBuilder(512);
                sb.Append(BuildInfo.Id())
                    .Append(' ');
                
                string branch = BuildInfo.Branch();
                if (!string.IsNullOrEmpty(branch))
                {
                    sb.Append(branch)
                        .Append(' ');
                }

                if (m_CondensedUseBundleVersion)
                {
                    string ver = BuildInfo.BundleVersion();
                    if (!string.IsNullOrEmpty(ver))
                    {
                        sb.Append(ver)
                            .Append(' ');
                    }
                }

                string tag = BuildInfo.Tag();
                if (!string.IsNullOrEmpty(tag))
                {
                    sb.Append(tag)
                        .Append(' ');
                }

                sb.Append('@')
                    .Append(BuildInfo.Date());
                
                m_BuildInfoCondensedText.SetText(sb);
                sb.Clear();
            }

            m_Populated = true;
        }
    }
}