/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    2 Sept 2020
 * 
 * File:    Profiling.cs
 * Purpose: Profiling blocks.
 */

#if ENABLE_PROFILER && ((UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEVELOPMENT)
#define ENABLE_PROFILING_BEAUUTIL
#endif // ENABLE_PROFILER && ((UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEVELOPMENT)

using System;
using System.Diagnostics;
using UnityEngine.Profiling;

namespace BeauUtil.Debugger
{
    /// <summary>
    /// Profiling blocks for time and memory.
    /// </summary>
    static public class Profiling
    {
        #region Time

        /// <summary>
        /// Returns a profiling block for time.
        /// </summary>
        static public TimeBlock Time(string inLabel)
        {
            #if ENABLE_PROFILING_BEAUUTIL
            return new TimeBlock(inLabel);
            #else
            return default;
            #endif // ENABLE_PROFILING_BEAUUTIL
        }

        public struct TimeBlock : IDisposable
        {
            #if ENABLE_PROFILING_BEAUUTIL
            private readonly string m_Label;
            private readonly long m_TickStart;
            private readonly int m_FrameStart;

            internal TimeBlock(string inLabel)
            {
                m_Label = inLabel ?? "Unknown";
                m_TickStart = Stopwatch.GetTimestamp();
                m_FrameStart = UnityEngine.Time.frameCount;
            }

            public void Dispose()
            {
                if (m_TickStart > 0)
                {
                    long elapsed = Stopwatch.GetTimestamp() - m_TickStart;
                    double durationMS = (double) elapsed / Stopwatch.Frequency * 1000;
                    int durationFrames = UnityEngine.Time.frameCount - m_FrameStart;
                    UnityEngine.Debug.Log(string.Format("[Profiling] Task '{0}' took {1:0.00}ms ({2} frames)", m_Label, durationMS, durationFrames));
                }
            }
            #else
            public void Dispose() { }
            #endif // ENABLE_PROFILING_BEAUUTIL
        }

        #endregion // Time

        #region Unity Samples

        /// <summary>
        /// Returns a profiling block for the Unity Profiler.
        /// </summary>
        static public SampleBlock Sample(string inLabel)
        {
            #if ENABLE_PROFILING_BEAUUTIL
            return new SampleBlock(inLabel);
            #else
            return default;
            #endif // ENABLE_PROFILING_BEAUUTIL
        }

        public struct SampleBlock : IDisposable
        {
            #if ENABLE_PROFILING_BEAUUTIL
            internal SampleBlock(string inLabel)
            {
                Profiler.BeginSample(inLabel);
            }

            public void Dispose()
            {
                Profiler.EndSample();
            }
            #else
            public void Dispose() { }
            #endif // ENABLE_PROFILING_BEAUUTIL
        }

        #endregion // Unity Samples
    }
}