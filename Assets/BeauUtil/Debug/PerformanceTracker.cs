/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    2 Sept 2020
 * 
 * File:    PerformanceTracker.cs
 * Purpose: Performance tracking object.
 */

#if UNITY_2019_1_OR_NEWER
#define USE_SRP
#endif // UNITY_2019_1_OR_NEWER

using System;
using System.Diagnostics;
using BeauUtil;
using UnityEngine;
using UnityEngine.Profiling;

#if USE_SRP
using UnityEngine.Rendering;
#endif // UNITY_2019

namespace BeauUtil.Debugger
{
    /// <summary>
    /// Performance stat tracker.
    /// </summary>
    public class PerformanceTracker : IDisposable
    {
        #region Consts

        public const int DefaultBufferSize = 64;
        public const int MinBufferSize = 30;
        public const int MaxBufferSize = 256;

        public const int BytesPerMB = 1024 * 1024;

        #endregion // Consts

        #region Frame

        /// <summary>
        /// Stats on a given frame.
        /// </summary>
        public struct Frame
        {
            public double Framerate;
            public double AvgFrameMS;
            public double AvgRenderMS;
            public double MemoryUsageMB;
        }

        #endregion // Frame

        private bool m_CameraHooksRegistered;
        private int m_CameraRenderDepth = 0;

        private Stopwatch m_RenderStopwatch;
        private Stopwatch m_FrameStopwatch;

        private RingBuffer<ulong> m_FrameTimeBuffer;
        private RingBuffer<ulong> m_RenderTimeBuffer;

        private bool m_FirstTick = true;
        private bool m_Disposed;

        public PerformanceTracker()
            : this(DefaultBufferSize)
        { }

        public PerformanceTracker(int inBufferSize)
        {
            if (inBufferSize < MinBufferSize)
                throw new ArgumentOutOfRangeException("inBufferSize", "Buffer size must be at least " + MinBufferSize);
            if (inBufferSize > MaxBufferSize)
                throw new ArgumentOutOfRangeException("inBufferSize", "Buffer size must be no more than " + MaxBufferSize);

            m_FrameTimeBuffer = new RingBuffer<ulong>(inBufferSize, RingBufferMode.Overwrite);
            m_RenderTimeBuffer = new RingBuffer<ulong>(inBufferSize, RingBufferMode.Overwrite);

            m_RenderStopwatch = new Stopwatch();
            m_FrameStopwatch = new Stopwatch();
        }

        /// <summary>
        /// Ticks the performance tracker forward.
        /// </summary>
        public void Tick()
        {
            VerifyNotDisposed();

            if (m_FirstTick)
            {
                HookCameras();
                if (Profiler.supported)
                {
                    Profiler.enabled = true;
                    Profiler.SetAreaEnabled(ProfilerArea.Memory, true);
                }
                m_FirstTick = false;
            }
            else
            {
                ulong frameTime = (ulong) m_FrameStopwatch.ElapsedTicks;
                ulong cameraTime = (ulong) m_RenderStopwatch.ElapsedTicks;
                m_FrameTimeBuffer.PushBack(frameTime);
                m_RenderTimeBuffer.PushBack(cameraTime);
            }

            m_RenderStopwatch.Reset();
            m_FrameStopwatch.Reset();

            m_FrameStopwatch.Start();
        }

        /// <summary>
        /// Stops the performance tracker and clears all recorded timings.
        /// </summary>
        public void Stop()
        {
            VerifyNotDisposed();

            m_FirstTick = true;
            m_FrameStopwatch.Stop();
            m_RenderStopwatch.Stop();
            UnhookCameras();

            m_FrameTimeBuffer.Clear();
            m_RenderTimeBuffer.Clear();
        }

        #region Stats

        /// <summary>
        /// Attempts to get stats for the current frame.
        /// </summary>
        public bool TryGetStats(out Frame outFrame)
        {
            VerifyNotDisposed();

            if (m_FirstTick || m_FrameTimeBuffer.Count < 2)
            {
                outFrame = default(Frame);
                return false;
            }

            outFrame.AvgFrameMS = AvgMillisecs(m_FrameTimeBuffer);
            outFrame.AvgRenderMS = AvgMillisecs(m_RenderTimeBuffer);
            outFrame.Framerate = 1000 / outFrame.AvgFrameMS;

            ulong memBytes;
            if (TryGetMemoryUsage(out memBytes))
            {
                outFrame.MemoryUsageMB = (double) memBytes / BytesPerMB;
            }
            else
            {
                outFrame.MemoryUsageMB = -1;
            }
            return true;
        }

        // Returns average millisecs from the given tick buffer
        static private double AvgMillisecs(RingBuffer<ulong> inTickBuffer)
        {
            int count = inTickBuffer.Count;

            if (count <= 1)
            {
                return -1;
            }

            ulong accum = 0;
            for(int i = count - 1; i >= 0; --i)
            {
                accum += inTickBuffer[i];
            }
            double avgTicks = (double) accum / count;
            return (avgTicks / Stopwatch.Frequency) * 1000;
        }

        /// <summary>
        /// Attempts to get the total memory used, in bytes.
        /// </summary>
        static public bool TryGetMemoryUsage(out ulong outBytes)
        {
            if (!Profiler.supported || !Profiler.enabled)
            {
                ulong gcMem = (ulong) GC.GetTotalMemory(false);

                if (gcMem > 0)
                {
                    outBytes = gcMem;
                    return true;
                }

                outBytes = 0;
                return false;
            }
            
            ulong unityBytes = (ulong) Profiler.GetTotalReservedMemoryLong() - (ulong) Profiler.GetTotalUnusedReservedMemoryLong();
            ulong monoBytes = (ulong) Profiler.GetMonoUsedSizeLong();
            outBytes = unityBytes + monoBytes;
            return true;
        }

        #endregion // Stats

        #region Rendering

        private void HookCameras()
        {
            if (m_CameraHooksRegistered)
                return;
            
            Camera.onPreRender += OnCameraPreRender;
            Camera.onPostRender += OnCameraPostRender;

            #if USE_SRP
            RenderPipelineManager.beginCameraRendering += OnRenderPipelineBeginCamera;
            RenderPipelineManager.endCameraRendering += OnRenderPipelineEndCamera;
            #endif // USE_SRP
            
            m_CameraHooksRegistered = true;
        }

        private void UnhookCameras()
        {
            if (!m_CameraHooksRegistered)
                return;
            
            Camera.onPreRender -= OnCameraPreRender;
            Camera.onPostRender -= OnCameraPostRender;

            #if USE_SRP
            RenderPipelineManager.beginCameraRendering -= OnRenderPipelineBeginCamera;
            RenderPipelineManager.endCameraRendering -= OnRenderPipelineEndCamera;
            #endif // USE_SRP

            m_CameraHooksRegistered = false;
            m_CameraRenderDepth = 0;
        }

        private void OnCameraPreRender(Camera inCamera)
        {
            RenderBegin();
        }

        private void OnCameraPostRender(Camera inCamera)
        {
            RenderEnd();
        }

        #if USE_SRP

        private void OnRenderPipelineBeginCamera(ScriptableRenderContext context, Camera camera)
        {
            RenderBegin();
        }

        private void OnRenderPipelineEndCamera(ScriptableRenderContext context, Camera camera)
        {
            RenderEnd();
        }

        #endif // USE_SRP

        /// <summary>
        /// Marks the beginning of rendering.
        /// </summary>
        public void RenderBegin()
        {
            VerifyNotDisposed();

            if (++m_CameraRenderDepth == 1)
            {
                m_RenderStopwatch.Start();
            }
        }

        /// <summary>
        /// Marks the end of rendering.
        /// </summary>
        public void RenderEnd()
        {
            VerifyNotDisposed();

            if (--m_CameraRenderDepth == 0)
            {
                m_RenderStopwatch.Stop();
            }
        }

        #endregion // Rendering

        #region IDisposable

        public void Dispose()
        {
            if (m_Disposed)
                return;
            
            Stop();

            m_FrameTimeBuffer = null;
            m_RenderTimeBuffer = null;
            m_FrameStopwatch = null;
            m_RenderStopwatch = null;

            m_Disposed = true;
        }

        private void VerifyNotDisposed()
        {
            if (m_Disposed)
                throw new InvalidOperationException("PerformanceTracker was disposed");
        }

        #endregion // IDisposable
    }
}