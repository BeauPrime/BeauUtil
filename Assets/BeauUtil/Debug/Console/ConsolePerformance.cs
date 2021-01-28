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
using System.Collections;
using System.Text;

namespace BeauUtil.Debugger
{
    public class ConsolePerformance : MonoBehaviour
    {
        #region Inspector

        [Header("Tracker")]
        [SerializeField, Range(PerformanceTracker.MinBufferSize, PerformanceTracker.MaxBufferSize)]
        private int m_BufferSize = PerformanceTracker.DefaultBufferSize;
        [SerializeField, Range(10, 600)] private int m_UpdatePeriodFrames = 30;

        [Header("Display")]
        [SerializeField] private TMP_Text m_FramerateText = null;
        [SerializeField] private TMP_Text m_FrameMSText = null;
        [SerializeField] private TMP_Text m_CameraMSText = null;
        [SerializeField] private TMP_Text m_MemoryText = null;
        [SerializeField] private string m_UnavaialbleTextString = "---";

        #endregion // Inspector

        [NonSerialized] private PerformanceTracker m_Tracker;
        [NonSerialized] private Coroutine m_EOFCoroutine;
        [NonSerialized] private WaitForEndOfFrame m_EOF;
        [NonSerialized] private int m_FrameCounter;
        [NonSerialized] private StringBuilder m_StatsBuilder;

        private void Awake()
        {
            m_Tracker = new PerformanceTracker(m_BufferSize);
            m_EOF = new WaitForEndOfFrame();
            m_StatsBuilder = new StringBuilder(16);
        }

        private void OnEnable()
        {
            m_FrameCounter = 0;
            m_EOFCoroutine = StartCoroutine(UpdateLoop());
            Refresh();
        }

        private void OnDisable()
        {
            m_Tracker.Stop();
            StopCoroutine(m_EOFCoroutine);
            m_EOFCoroutine = null;
        }

        private void OnDestroy()
        {
            m_Tracker.Dispose();
            m_Tracker = null;
        }

        private IEnumerator UpdateLoop()
        {
            while(true)
            {
                yield return m_EOF;
                Tick();
            }
        }

        private void Tick()
        {
            m_Tracker.Tick();
            if (++m_FrameCounter >= m_UpdatePeriodFrames)
            {
                m_FrameCounter = 0;
                Refresh();
            }
        }

        private void Refresh()
        {
            PerformanceTracker.Frame frame;
            bool bAvailable = m_Tracker.TryGetStats(out frame);
            if (m_FramerateText)
            {
                if (!bAvailable || frame.Framerate < 0)
                {
                    m_FramerateText.SetText(m_UnavaialbleTextString);
                }
                else 
                {
                    m_StatsBuilder.Append(frame.Framerate.ToString("0.00")).Append(" fps");
                    m_FramerateText.SetText(m_StatsBuilder.Flush());
                }
            }
            if (m_FrameMSText)
            {
                if (!bAvailable || frame.AvgFrameMS < 0)
                {
                    m_FrameMSText.SetText(m_UnavaialbleTextString);
                }
                else
                {
                    m_StatsBuilder.Append(frame.AvgFrameMS.ToString("0.00")).Append(" ms");
                    m_FrameMSText.SetText(m_StatsBuilder.Flush());
                }
            }
            if (m_CameraMSText)
            {
                if (!bAvailable || frame.AvgRenderMS < 0)
                {
                    m_CameraMSText.SetText(m_UnavaialbleTextString);
                }
                else
                {
                    m_StatsBuilder.Append(frame.AvgRenderMS.ToString("0.00")).Append(" ms");
                    m_CameraMSText.SetText(m_StatsBuilder.Flush());
                }
            }
            if (m_MemoryText)
            {
                if (!bAvailable || frame.MemoryUsageMB < 0)
                {
                    m_MemoryText.SetText(m_UnavaialbleTextString);
                }
                else
                {
                    m_StatsBuilder.Append(frame.MemoryUsageMB.ToString("0.00")).Append(" MB");
                    m_MemoryText.SetText(m_StatsBuilder.Flush());
                }
            }
        }
    }
}