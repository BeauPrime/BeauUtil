/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    2 Sept 2020
 * 
 * File:    Profiling.cs
 * Purpose: Profiling blocks.
 */

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using System;
using System.Diagnostics;

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
        static public IDisposable Time(string inLabel)
        {
            #if DEVELOPMENT
            return new TimeBlock(inLabel);
            #else
            return null;
            #endif // DEVELOPMENT
        }

        private class TimeBlock : IDisposable
        {
            private readonly string m_Label;
            private readonly Stopwatch m_Stopwatch;
            private readonly int m_FrameStart;

            internal TimeBlock(string inLabel)
            {
                m_Label = inLabel ?? "Unknown";
                m_Stopwatch = Stopwatch.StartNew();
                m_FrameStart = UnityEngine.Time.frameCount;
            }

            public void Dispose()
            {
                m_Stopwatch.Stop();
                double durationMS = (double) m_Stopwatch.ElapsedTicks / Stopwatch.Frequency * 1000;
                int durationFrames = UnityEngine.Time.frameCount - m_FrameStart;
                UnityEngine.Debug.Log(string.Format("[Profiling] Task '{0}' took {1:0.00}ms ({2} frames)", m_Label, durationMS, durationFrames));
            }
        }

        #endregion // TIme

        #region Memory

        /// <summary>
        /// Returns a profiling block for memory (experimental).
        /// </summary>
        static public IDisposable Memory(string inLabel)
        {
            #if DEVELOPMENT
            return new MemoryBlock(inLabel);
            #else
            return null;
            #endif // DEVELOPMENT
        }

        private class MemoryBlock : IDisposable
        {
            private readonly string m_Label;
            private readonly long m_BytesStart;

            internal MemoryBlock(string inLabel)
            {
                m_Label = inLabel ?? "Unknown";
                m_BytesStart = GC.GetTotalMemory(false);
            }

            public void Dispose()
            {
                long memDiff = GC.GetTotalMemory(false) - m_BytesStart;
                // TODO: Log
            }
        }

        #endregion // Memory
    }
}