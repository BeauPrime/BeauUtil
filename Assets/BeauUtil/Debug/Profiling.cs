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
using System.Runtime.CompilerServices;
using UnityEngine.Profiling;

namespace BeauUtil.Debugger
{
    /// <summary>
    /// Profiling blocks for time and memory.
    /// </summary>
    static public class Profiling
    {
        #region Time

        static Profiling()
        {
            long hzPerSec = UnityEngine.SystemInfo.processorFrequency * 1000000L;
            if (hzPerSec == 0)
            {
                CyclesPerTick = 256;
            }
            else
            {
                CyclesPerTick = hzPerSec / (double) TimerFrequency;
            }
        }

        static private readonly long TimerFrequency = Stopwatch.Frequency;
        static private readonly double CyclesPerTick;

        /// <summary>
        /// Returns a profiling block for time.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public TimeBlock Time(string inLabel, ProfileTimeUnits inTimeUnits = ProfileTimeUnits.Milliseconds)
        {
#if ENABLE_PROFILING_BEAUUTIL
            return new TimeBlock(inLabel, inTimeUnits);
#else
            return default;
#endif // ENABLE_PROFILING_BEAUUTIL
        }

        public readonly struct TimeBlock : IDisposable
        {
#if ENABLE_PROFILING_BEAUUTIL
            private readonly string m_Label;
            private readonly long m_TickStart;
            private readonly int m_FrameStart;
            private readonly ProfileTimeUnits m_TimeUnits;

            internal TimeBlock(string inLabel, ProfileTimeUnits inUnits)
            {
                m_Label = inLabel ?? "Unknown";
                m_FrameStart = UnityHelper.IsMainThread() ? UnityEngine.Time.frameCount : 0;
                m_TimeUnits = inUnits;
                m_TickStart = Stopwatch.GetTimestamp();
            }

            public void Dispose()
            {
                if (m_TickStart > 0)
                {
                    long elapsed = Stopwatch.GetTimestamp() - m_TickStart;
                    int durationFrames = UnityHelper.IsMainThread() ? UnityEngine.Time.frameCount - m_FrameStart : 0;
                    switch (m_TimeUnits)
                    {
                        case ProfileTimeUnits.Milliseconds:
                            double durationMS = (double) elapsed / TimerFrequency * 1000;
                            UnityEngine.Debug.Log(string.Format("[Profiling] Task '{0}' took {1:0.00}ms ({2} frames)", m_Label, durationMS, durationFrames));
                            break;

                        case ProfileTimeUnits.Microseconds:
                            double durationMicroseconds = (double) elapsed / TimerFrequency * 1000000;
                            UnityEngine.Debug.Log(string.Format("[Profiling] Task '{0}' took {1:0.00}μs ({2} frames)", m_Label, durationMicroseconds, durationFrames));
                            break;

                        case ProfileTimeUnits.Ticks:
                            UnityEngine.Debug.Log(string.Format("[Profiling] Task '{0}' took {1} ticks ({2} frames)", m_Label, elapsed, durationFrames));
                            break;

                        case ProfileTimeUnits.Cycles:
                            UnityEngine.Debug.Log(string.Format("[Profiling] Task '{0}' took ~{1:0.00} est. cycles ({2} frames)", m_Label, elapsed * CyclesPerTick, durationFrames));
                            break;
                    }
                }
            }
#else
            public void Dispose() { }
#endif // ENABLE_PROFILING_BEAUUTIL
        }

        /// <summary>
        /// Returns a profiling block for computing an average time for a specific count of operations.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public AvgTimeBlock AvgTime(string inLabel, int inSampleCount, ProfileTimeUnits inTimeUnits = ProfileTimeUnits.Milliseconds)
        {
#if ENABLE_PROFILING_BEAUUTIL
            return new AvgTimeBlock(inLabel, inSampleCount, inTimeUnits);
#else
            return default;
#endif // ENABLE_PROFILING_BEAUUTIL
        }

        public readonly struct AvgTimeBlock : IDisposable
        {
#if ENABLE_PROFILING_BEAUUTIL
            private readonly string m_Label;
            private readonly long m_TickStart;
            private readonly int m_SampleCount;
            private readonly ProfileTimeUnits m_TimeUnits;

            internal AvgTimeBlock(string inLabel, int inSampleCount, ProfileTimeUnits inUnits)
            {
                m_Label = inLabel ?? "Unknown";
                m_TimeUnits = inUnits;
                m_SampleCount = inSampleCount;
                m_TickStart = Stopwatch.GetTimestamp();
            }

            public void Dispose()
            {
                if (m_TickStart > 0)
                {
                    long elapsed = Stopwatch.GetTimestamp() - m_TickStart;
                    switch (m_TimeUnits)
                    {
                        case ProfileTimeUnits.Milliseconds:
                            double durationMS = (double) elapsed / TimerFrequency * 1000;
                            UnityEngine.Debug.Log(string.Format("[Profiling] Task '{0}' took {1:0.00}ms ({2} samples, {3:0.00} avg)", m_Label, durationMS, m_SampleCount, durationMS / m_SampleCount));
                            break;

                        case ProfileTimeUnits.Microseconds:
                            double durationMicroseconds = (double) elapsed / TimerFrequency * 1000000;
                            UnityEngine.Debug.Log(string.Format("[Profiling] Task '{0}' took {1:0.00}μs ({2} samples, {3:0.00} avg)", m_Label, durationMicroseconds, m_SampleCount, durationMicroseconds / m_SampleCount));
                            break;

                        case ProfileTimeUnits.Ticks:
                            UnityEngine.Debug.Log(string.Format("[Profiling] Task '{0}' took {1} ticks ({2} samples, {3:0.00} avg)", m_Label, elapsed, m_SampleCount, elapsed / (double) m_SampleCount));
                            break;

                        case ProfileTimeUnits.Cycles:
                            UnityEngine.Debug.Log(string.Format("[Profiling] Task '{0}' took ~{1:0.00} est. cycles ({2} samples, {3} avg)", m_Label, elapsed * CyclesPerTick, m_SampleCount, elapsed * CyclesPerTick / m_SampleCount));
                            break;
                    }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public SampleBlock Sample(string inLabel)
        {
#if ENABLE_PROFILING_BEAUUTIL
            return new SampleBlock(inLabel);
#else
            return default;
#endif // ENABLE_PROFILING_BEAUUTIL
        }

        public readonly struct SampleBlock : IDisposable
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

        #region Timing

        /// <summary>
        /// Returns a timestamp, in ticks.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public long NowTicks()
        {
            return Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// Calculates converts from ticks to milliseconds. 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public double TicksToMillisecs(long inTicks)
        {
            return (double) inTicks / TimerFrequency * 1000;
        }

        /// <summary>
        /// Calculates converts from ticks to milliseconds. 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public double TicksToMillisecs(double inTicks)
        {
            return (double) inTicks / TimerFrequency * 1000;
        }

        /// <summary>
        /// Calculates converts from ticks to microseconds. 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public double TicksToMicrosecs(long inTicks)
        {
            return (double) inTicks / TimerFrequency * 1000000;
        }

        /// <summary>
        /// Calculates converts from ticks to microseconds. 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public double TicksToMicrosecs(double inTicks)
        {
            return (double) inTicks / TimerFrequency * 1000000;
        }

        /// <summary>
        /// Calculates converts from ticks to cycles. 
        /// If the estimated CPU cycle rate is not available, cycles are assumed to be 256 per tick.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public double TicksToEstCycles(long inTicks)
        {
            return inTicks * CyclesPerTick;
        }

        /// <summary>
        /// Calculates converts from ticks to cycles.
        /// If the estimated CPU cycle rate is not available, cycles are assumed to be 256 per tick.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public double TicksToEstCycles(double inTicks)
        {
            return inTicks * CyclesPerTick;
        }

        #endregion // Timing
    }

    /// <summary>
    /// Time units used for reporting from Profiling.Time.
    /// </summary>
    public enum ProfileTimeUnits
    {
        /// <summary>
        /// Reports in milliseconds.
        /// </summary>
        Milliseconds,

        /// <summary>
        /// Reports in microseconds.
        /// </summary>
        Microseconds,

        /// <summary>
        /// Reports in system ticks.
        /// </summary>
        Ticks,

        /// <summary>
        /// Report in estimated cycles, if available.
        /// If the estimated CPU cycle rate is not available, cycles are assumed to be 256 per tick.
        /// </summary>
        Cycles
    }
}