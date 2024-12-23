/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    31 Autust 2020
 * 
 * File:    CameraHelper.cs
 * Purpose: Camera utility methods.
 */

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

#if UNITY_2019_1_OR_NEWER
#define USE_SRP
#endif // UNITY_2019_1_OR_NEWER

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

#if USE_SRP
using UnityEngine.Rendering;
#endif // USE_SRP

namespace BeauUtil
{
    /// <summary>
    /// Camera utility methods.
    /// </summary>
    static public class CameraHelper
    {
        #region Planes

        /// <summary>
        /// Attempts to get the distance from the camera to the plane.
        /// Note: This is distance from the camera transform, and not the near plane.
        /// </summary>
        static public bool TryGetDistanceToPlane(this Camera inCamera, Plane inPlane, out float outDistance)
        {
            Ray r = inCamera.ViewportPointToRay(s_CenterViewportPoint);
            bool hit = inPlane.Raycast(r, out outDistance);
            outDistance += inCamera.nearClipPlane;
            return hit;
        }

        /// <summary>
        /// Attempts to get the distance from the camera to the point plane.
        /// Note: This is distance from the camera transform, and not the near plane.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool TryGetDistanceToPointPlane(this Camera inCamera, Vector3 inPosition, out float outDistance)
        {
            Vector3 transformed = inCamera.transform.InverseTransformPoint(inPosition);
            outDistance = transformed.z;
            return outDistance >= 0;
        }

        /// <summary>
        /// Attempts to get the distance from the camera to the object plane.
        /// Note: This is distance from the camera transform, and not the near plane.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool TryGetDistanceToObjectPlane(this Camera inCamera, Transform inTransform, out float outDistance)
        {
            if (!inTransform)
            {
                outDistance = 0;
                return false;
            }

            return TryGetDistanceToPointPlane(inCamera, inTransform.position, out outDistance);
        }

        /// <summary>
        /// Attempts to get the scaling factor for a transform at one distance from the camera
        /// to be consistently sized with a transform at another distance.
        /// </summary>
        static public bool TryGetScaleForConsistentSize(this Camera inCamera, Transform inTransform, Transform inTargetTransform, out float outScale)
        {
            if (inCamera.orthographic)
            {
                outScale = 1;
                return true;
            }

            float distSrc, distTarg;
            if (!TryGetDistanceToObjectPlane(inCamera, inTransform, out distSrc)
                || !TryGetDistanceToObjectPlane(inCamera, inTargetTransform, out distTarg))
            {
                outScale = 0;
                return false;
            }

            outScale = distTarg / distSrc;
            return !float.IsInfinity(outScale) && !float.IsNaN(outScale);
        }

        /// <summary>
        /// Attempts to cast a position from one camera plane to another plane.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool TryCastPositionToTargetPlane(this Camera inCamera, Transform inTransform, Transform inTargetTransform, out Vector3 outNewPosition)
        {
            if (!inTransform)
            {
                outNewPosition = default(Vector3);
                return false;
            }

            return TryCastPositionToTargetPlane(inCamera, inTransform.position, inTargetTransform, out outNewPosition);
        }

        /// <summary>
        /// Attempts to cast a position from one camera plane to another plane.
        /// </summary>
        static public bool TryCastPositionToTargetPlane(this Camera inCamera, Vector3 inPosition, Transform inTargetTransform, out Vector3 outNewPosition)
        {
            if (!inTargetTransform)
            {
                outNewPosition = default(Vector3);
                return false;
            }

            Vector3 sourcePos = inPosition;

            Matrix4x4 toCamSpace = inCamera.transform.worldToLocalMatrix;
            Matrix4x4 toWorld = inCamera.transform.localToWorldMatrix;

            Vector3 sourceCam = toCamSpace.MultiplyPoint3x4(inPosition);
            Vector3 targetCam = toCamSpace.MultiplyPoint3x4(inTargetTransform.position);

            float zScale = targetCam.z / sourceCam.z;

            if (!inCamera.orthographic)
            {
                sourceCam.x *= zScale;
                sourceCam.y *= zScale;
            }

            sourceCam.z = targetCam.z;
            outNewPosition = toWorld.MultiplyPoint3x4(sourceCam);
            return sourceCam.z > 0;
        }

        static private readonly Vector3 s_CenterViewportPoint = new Vector3(0.5f, 0.5f, 1);

        #endregion // Planes

        #region FOV calculation

        /// <summary>
        /// Returns the FOV (in degrees) required for a camera to have a frustrum height at a specific distance from the camera.
        /// </summary>
        static public float FOVForHeightAndDistance(float inHeight, float inDistance)
        {
            return 2.0f * Mathf.Atan(inHeight * 0.5f / inDistance) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Returns the distance from the camera of a given frustrum height and FOV.
        /// </summary>
        static public float DistanceForHeightAndFOV(float inHeight, float inFOV)
        {
            return (float) (inHeight * 0.5f / Math.Tan(inFOV / 2 * Mathf.Deg2Rad));
        }

        /// <summary>
        /// Returns the frustum height of a camera at the given distance and FOV.
        /// </summary>
        static public float HeightForDistanceAndFOV(float inDistance, float inFOV)
        {
            return (float) (2f * Math.Tan(inFOV / 2 * Mathf.Deg2Rad)) * inDistance;
        }

        /// <summary>
        /// Returns the unit frustum height of a camera with the given FOV.
        /// </summary>
        static public float UnitHeightForFOV(float inFOV)
        {
            return (float) (2f * Math.Tan(inFOV / 2 * Mathf.Deg2Rad));
        }
    
        #endregion // FOV calculation

        #region Callbacks

        private struct CameraCallbackEntry<T> : IComparable<CameraCallbackEntry<T>>
        {
            public readonly int CameraId;
            public readonly T Callback;
            public readonly int Order;

            public CameraCallbackEntry(Camera inCamera, T inCallback, int inOrder)
            {
                CameraId = inCamera ? inCamera.GetInstanceID() : 0;
                Callback = inCallback;
                Order = inOrder;
            }

            public int CompareTo(CameraCallbackEntry<T> other)
            {
                int sort = Order - other.Order;
                if (sort == 0)
                {
                    sort = CameraId - other.CameraId;
                }
                return sort;
            }
        }

        private delegate void CallbackInvoker<T>(T inHandler, Camera inCamera, CameraCallbackSource inSource);

        static private bool s_RegisteredDefaultCallbacks;
        static private List<CameraCallbackEntry<ICameraPreCullCallback>> s_PreCullCallbacks;
        static private List<CameraCallbackEntry<ICameraPreRenderCallback>> s_PreRenderCallbacks;
        static private List<CameraCallbackEntry<ICameraPostRenderCallback>> s_PostRenderCallbacks;

        static private bool s_PreCullDirty;
        static private bool s_PreRenderDirty;
        static private bool s_PostRenderDirty;

        static private void ForEachCallback<T>(List<CameraCallbackEntry<T>> inList, Camera inTargetCamera, CameraCallbackSource inSource, CallbackInvoker<T> inExecute, ref bool ioDirtyFlag)
        {
            if (inList == null)
                return;

            if (ioDirtyFlag)
            {
                inList.Sort();
                ioDirtyFlag = false;
            }

            int targetId = inTargetCamera ? inTargetCamera.GetInstanceID() : 0;
            
            for(int i = 0; i < inList.Count; i++)
            {
                var entry = inList[i];
                if (entry.CameraId == 0 || entry.CameraId == targetId)
                {
                    inExecute(entry.Callback, inTargetCamera, inSource);
                }
            }
        }

        static private bool RemoveCallback<T>(List<CameraCallbackEntry<T>> inList, Camera inTargetCamera, T inCallback)
        {
            if (inList == null)
                return false;

            int targetId = inTargetCamera ? inTargetCamera.GetInstanceID() : 0;

            for(int i = 0, len = inList.Count; i < len; i++)
            {
                var entry = inList[i];
                if (entry.CameraId == 0 || entry.CameraId == targetId)
                {
                    inList.FastRemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        static private bool HasCameraCallbacks()
        {
            return (s_PreCullCallbacks != null && s_PreCullCallbacks.Count > 0)
                || (s_PreRenderCallbacks != null && s_PreRenderCallbacks.Count > 0)
                || (s_PostRenderCallbacks != null && s_PostRenderCallbacks.Count > 0);
        }

        #region Registration

        /// <summary>
        /// Adds a handler, invoked before culling occurs for this camera.
        /// </summary>
        static public void AddOnPreCull(this Camera inCamera, ICameraPreCullCallback inPreCull, int inOrder = 0)
        {
            if (s_PreCullCallbacks == null)
            {
                s_PreCullCallbacks = new List<CameraCallbackEntry<ICameraPreCullCallback>>(2);
            }
            s_PreCullCallbacks.Add(new CameraCallbackEntry<ICameraPreCullCallback>(inCamera, inPreCull, inOrder));
            s_PreCullDirty = true;
            RegisterCallbacks();
        }

        /// <summary>
        /// Adds a global handler, invoked before culling occurs for any camera.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void AddOnPreCull(ICameraPreCullCallback inPreCull, int inOrder = 0)
        {
            AddOnPreCull(null, inPreCull, inOrder);
        }

        /// <summary>
        /// Removes a pre-cull handler.
        /// </summary>
        static public void RemoveOnPreCull(this Camera inCamera, ICameraPreCullCallback inPreCull)
        {
            if (RemoveCallback(s_PreCullCallbacks, inCamera, inPreCull))
            {
                s_PreCullDirty = true;
                if (!HasCameraCallbacks())
                {
                    DeregisterCallbacks();
                }
            }
        }

        /// <summary>
        /// Removes a global pre-cull handler.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void RemoveOnPreCull(ICameraPreCullCallback inPreCull)
        {
            RemoveOnPreCull(null, inPreCull);
        }

        /// <summary>
        /// Adds a handler, invoked before rendering occurs for this camera.
        /// </summary>
        static public void AddOnPreRender(this Camera inCamera, ICameraPreRenderCallback inPreRender, int inOrder = 0)
        {
            if (s_PreRenderCallbacks == null)
            {
                s_PreRenderCallbacks = new List<CameraCallbackEntry<ICameraPreRenderCallback>>(2);
            }
            s_PreRenderCallbacks.Add(new CameraCallbackEntry<ICameraPreRenderCallback>(inCamera, inPreRender, inOrder));
            s_PreRenderDirty = true;
            RegisterCallbacks();
        }

        /// <summary>
        /// Adds a global handler, invoked before rendering occurs for any camera.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void AddOnPreRender(ICameraPreRenderCallback inPreRender, int inOrder = 0)
        {
            AddOnPreRender(null, inPreRender, inOrder);
        }

        /// <summary>
        /// Removes a pre-render handler.
        /// </summary>
        static public void RemoveOnPreRender(this Camera inCamera, ICameraPreRenderCallback inPreRender)
        {
            if (RemoveCallback(s_PreRenderCallbacks, inCamera, inPreRender))
            {
                s_PreRenderDirty = true;
                if (!HasCameraCallbacks())
                {
                    DeregisterCallbacks();
                }
            }
        }

        /// <summary>
        /// Removes a global pre-render handler.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void RemoveOnPreRender(ICameraPreRenderCallback inPreRender)
        {
            RemoveOnPreRender(null, inPreRender);
        }

        /// <summary>
        /// Adds a handler, invoked after rendering occurs for this camera.
        /// </summary>
        static public void AddOnPostRender(this Camera inCamera, ICameraPostRenderCallback inPostRender, int inOrder = 0)
        {
            if (s_PostRenderCallbacks == null)
            {
                s_PostRenderCallbacks = new List<CameraCallbackEntry<ICameraPostRenderCallback>>(2);
            }
            s_PostRenderCallbacks.Add(new CameraCallbackEntry<ICameraPostRenderCallback>(inCamera, inPostRender, inOrder));
            s_PostRenderDirty = true;
            RegisterCallbacks();
        }

        /// <summary>
        /// Adds a global handler, invoked after rendering occurs for any camera.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void AddOnPostRender(ICameraPostRenderCallback inPostRender, int inOrder = 0)
        {
            AddOnPostRender(null, inPostRender, inOrder);
        }

        /// <summary>
        /// Removes a post-render handler.
        /// </summary>
        static public void RemoveOnPostRender(this Camera inCamera, ICameraPostRenderCallback inPostRender)
        {
            if (RemoveCallback(s_PostRenderCallbacks, inCamera, inPostRender))
            {
                s_PostRenderDirty = true;
                if (!HasCameraCallbacks())
                {
                    DeregisterCallbacks();
                }
            }
        }

        /// <summary>
        /// Removes a global post-render handler.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void RemoveOnPostRender(ICameraPostRenderCallback inPostRender)
        {
            RemoveOnPostRender(null, inPostRender);
        }

        #endregion // Registration

        #region Default

        static private void RegisterCallbacks()
        {
            if (!s_RegisteredDefaultCallbacks)
            {
                Camera.onPreCull += OnCameraPreCull;
                Camera.onPreRender += OnCameraPreRender;
                Camera.onPostRender += OnCameraPostRender;
                s_RegisteredDefaultCallbacks = true;
            }

            #if USE_SRP
            RegisterSRPCallbacks();
            #endif // USE_SRP
        }

        static private void DeregisterCallbacks()
        {
            if (s_RegisteredDefaultCallbacks)
            {
                Camera.onPreCull -= OnCameraPreCull;
                Camera.onPreRender -= OnCameraPreRender;
                Camera.onPostRender -= OnCameraPostRender;
                s_RegisteredDefaultCallbacks = false;
            }

            #if USE_SRP
            DeregisterSRPCallbacks();
            #endif // USE_SRP
        }

        static private void OnCameraPreCull(Camera inCamera)
        {
            ForEachCallback(s_PreCullCallbacks, inCamera, CameraCallbackSource.Default, (p, c, s) => p.OnCameraPreCull(c, s), ref s_PreCullDirty);
        }

        static private void OnCameraPreRender(Camera inCamera)
        {
            ForEachCallback(s_PreRenderCallbacks, inCamera, CameraCallbackSource.Default, (p, c, s) => p.OnCameraPreRender(c, s), ref s_PreRenderDirty);
        }

        static private void OnCameraPostRender(Camera inCamera)
        {
            ForEachCallback(s_PostRenderCallbacks, inCamera, CameraCallbackSource.Default, (p, c, s) => p.OnCameraPostRender(c, s), ref s_PostRenderDirty);
        }

        #endregion // Default

        #region SRP

        #if USE_SRP

        static private bool s_RegisteredSRPCallbacks;

        static private void RegisterSRPCallbacks()
        {
            if (!s_RegisteredSRPCallbacks)
            {
                RenderPipelineManager.beginCameraRendering += OnSRPPreRender;
                RenderPipelineManager.endCameraRendering += OnSRPPostRender;
                s_RegisteredSRPCallbacks = true;
            }
        }

        static private void DeregisterSRPCallbacks()
        {
            if (s_RegisteredSRPCallbacks)
            {
                RenderPipelineManager.beginCameraRendering -= OnSRPPreRender;
                RenderPipelineManager.endCameraRendering -= OnSRPPostRender;
                s_RegisteredSRPCallbacks = false;
            }
        }

        static private void OnSRPPreRender(ScriptableRenderContext inContext, Camera inCamera)
        {
            ForEachCallback(s_PreCullCallbacks, inCamera, CameraCallbackSource.SRP, (p, c, s) => p.OnCameraPreCull(c, s), ref s_PreCullDirty);
            ForEachCallback(s_PreRenderCallbacks, inCamera, CameraCallbackSource.SRP, (p, c, s) => p.OnCameraPreRender(c, s), ref s_PreRenderDirty);
        }

        static private void OnSRPPostRender(ScriptableRenderContext inContext, Camera inCamera)
        {
            ForEachCallback(s_PostRenderCallbacks, inCamera, CameraCallbackSource.SRP, (p, c, s) => p.OnCameraPostRender(c, s), ref s_PostRenderDirty);
        }

#endif // USE_SRP

        #endregion // SRP

        #endregion // Callbacks

        #region State Hash

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct CameraHashState
        {
            public Matrix4x4 MatrixA;
            public Rect Rect;
            public Vector2 ScreenSize;
            public float OrthoSize;
            public float FOV;
            public Matrix4x4 MatrixB;
        }

        /// <summary>
        /// Calculates a 64-bit hash of the camera's state.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        static public unsafe ulong GetStateHash(this Camera inCamera)
        {
            CameraHashState state;
            state.MatrixA = inCamera.projectionMatrix;
            state.MatrixB = inCamera.cameraToWorldMatrix;
            state.Rect = inCamera.rect;
            state.ScreenSize = new Vector2(Screen.width, Screen.height);
            state.OrthoSize = inCamera.orthographic ? inCamera.orthographicSize : float.NaN;
            state.FOV = inCamera.orthographic ? float.NaN : inCamera.fieldOfView;
            return Unsafe.Hash64(&state, sizeof(CameraHashState));
        }

        #endregion // State Hash

        #region Letterboxing

        /// <summary>
        /// Renders letterboxing in normalized viewport coordinates.
        /// </summary>
        static public void RenderLetterboxing(Rect inInnerRect, Color inOuterColor, bool inbClearDepth = true)
        {
            RenderLetterboxing(inInnerRect, new Rect(0, 0, 1, 1), inOuterColor, inbClearDepth);
        }

        /// <summary>
        /// Renders letterboxing in normalized viewport coordinates.
        /// </summary>
        static public void RenderLetterboxing(Rect inInnerRect, Rect inOuterRect, Color inOuterColor, bool inbClearDepth)
        {
            float left = inInnerRect.x - inOuterRect.x, right = inOuterRect.xMax - inInnerRect.xMax,
                bottom = inInnerRect.y - inOuterRect.y, top = inOuterRect.yMax - inInnerRect.yMax;

            if (left > 0 || right > 0 || bottom > 0 || top > 0)
            {
                float scrW = Screen.width, scrH = Screen.height;
                GL.PushMatrix();
                GL.LoadOrtho();

                Rect r = new Rect(inOuterRect.x * scrW, inOuterRect.y * scrH, 0, inOuterRect.height * scrH);
                if (left > 0)
                {
                    r.width = scrW * left;
                    GL.Viewport(r);
                    GL.Clear(inbClearDepth, true, inOuterColor);
                }
                if (right > 0)
                {
                    r.width = scrW * right;
                    r.x = scrW * inInnerRect.xMax;
                    GL.Viewport(r);
                    GL.Clear(inbClearDepth, true, inOuterColor);
                }

                r.x = scrW * inInnerRect.x;
                r.width = inInnerRect.width * scrW;
                if (bottom > 0)
                {
                    //r.y = inOuterRect.y * scrH;
                    r.height = scrH * bottom;
                    GL.Viewport(r);
                    GL.Clear(inbClearDepth, true, inOuterColor);
                }
                if (top > 0)
                {
                    r.y = inInnerRect.yMax * scrH;
                    r.height = scrH * top;
                    GL.Viewport(r);
                    GL.Clear(inbClearDepth, true, inOuterColor);
                }

                GL.PopMatrix();
            }
        }

        #endregion // Letterboxing
    }
}