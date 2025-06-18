/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 July 2020
 * 
 * File:    TransformHelper.cs
 * Purpose: Transform helper functions.
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using Unity.IL2CPP.CompilerServices;


#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace BeauUtil
{
    /// <summary>
    /// Contains transform-specific utility functions.
    /// </summary>
    static public class TransformHelper
    {
        #region Screen Position

        /// <summary>
        /// Returns the screen position of this transform using the default camera.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector2 ScreenPosition(this Transform inTransform)
        {
            return ScreenPosition(inTransform, Camera.main);
        }

        /// <summary>
        /// Returns the screen position of this transform, offset by the given offsets, using the default camera.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector2 ScreenPosition(this Transform inTransform, TransformOffset inOffset)
        {
            return ScreenPosition(inTransform, Camera.main, inOffset);
        }

        /// <summary>
        /// Returns the screen position of this transform using the given camera.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector2 ScreenPosition(this Transform inTransform, Camera inCamera)
        {
            return RectTransformUtility.WorldToScreenPoint(inCamera, inTransform.position);
        }

        /// <summary>
        /// Returns the screen position of this transform, offset by the given offsets, using the given camera.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector2 ScreenPosition(this Transform inTransform, Camera inCamera, TransformOffset inOffset)
        {
            return RectTransformUtility.WorldToScreenPoint(inCamera, inOffset.EvaluateWorld(inTransform));
        }

        #endregion // Screen Position

        #region Viewport  Position

        /// <summary>
        /// Returns the screen position of this transform using the default camera.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector3 ViewportPosition(this Transform inTransform)
        {
            return ViewportPosition(inTransform, Camera.main);
        }

        /// <summary>
        /// Returns the screen position of this transform, offset by the given offsets, using the default camera.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector3 ViewportPosition(this Transform inTransform, TransformOffset inOffset)
        {
            return ViewportPosition(inTransform, Camera.main, inOffset);
        }

        /// <summary>
        /// Returns the screen position of this transform using the given camera.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector3 ViewportPosition(this Transform inTransform, Camera inCamera)
        {
            return RectTransformUtility.WorldToScreenPoint(inCamera, inTransform.position);
        }

        /// <summary>
        /// Returns the screen position of this transform, offset by the given offsets, using the given camera.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector3 ViewportPosition(this Transform inTransform, Camera inCamera, TransformOffset inOffset)
        {
            return RectTransformUtility.WorldToScreenPoint(inCamera, inOffset.EvaluateWorld(inTransform));
        }

        #endregion // Viewport Position

        #region RectTransform

        /// <summary>
        /// Returns the canvas for the given Transform.
        /// </summary>
        static public Canvas GetCanvas(this Transform inTransform)
        {
            Graphic graphic = inTransform.GetComponent<Graphic>();
            if (graphic)
                return graphic.canvas;

            return inTransform.GetComponentInParent<Canvas>();
        }

        #endregion // RectTransform

        #region Camera

        /// <summary>
        /// Attempts to get the default camera used to render this Transform.
        /// This will skip cameras that are currently inactive.
        /// </summary>
        static public bool TryGetCamera(this Transform inTransform, out Camera outCamera)
        {
            return TryGetCamera(inTransform, false, out outCamera);
        }

        /// <summary>
        /// Attempts to get the default camera used to render this Transform.
        /// </summary>
        static public bool TryGetCamera(this Transform inTransform, bool inbIncludeInactive, out Camera outCamera)
        {
            // if this is a RectTransform, try to use RectTransform rendering first
            if (inTransform is RectTransform)
            {
                Canvas canvas = GetCanvas(inTransform);
                if (canvas)
                {
                    bool bFound = canvas.TryGetCamera(out outCamera);
                    if (bFound && !inbIncludeInactive && !outCamera.isActiveAndEnabled)
                    {
                        outCamera = null;
                        bFound = false;
                    }
                    return bFound;
                }
            }

            return TryGetCameraFromLayer(inTransform, inbIncludeInactive, out outCamera);
        }

        /// <summary>
        /// Attempts to get the default camera used to render this Transform.
        /// This will search based on GameObject layer.
        /// This will skip cameras that are currently inactive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool TryGetCameraFromLayer(this Transform inTransform, out Camera outCamera)
        {
            return TryGetCameraFromLayer(inTransform, false, out outCamera);
        }

        /// <summary>
        /// Attempts to get the default camera used to render this Transform.
        /// This will search based on GameObject layer.
        /// </summary>
        static public bool TryGetCameraFromLayer(this Transform inTransform, bool inbIncludeInactive, out Camera outCamera)
        {
            int layer = inTransform.gameObject.layer;
            if (layer == -1)
            {
                outCamera = null;
                return false;
            }

            layer = 1 << layer;

            Camera[] cameraArray = s_CachedCameraArray;
            int cameraCount = Camera.GetAllCameras(cameraArray);

            // find the camera with the most specific rendering mask that includes this layer
            Camera found = null;
            int mostSpecificBitCount = int.MaxValue;

            for (int i = 0; i < cameraCount; ++i)
            {
                Camera cam = cameraArray[i];
                if (!inbIncludeInactive && !cam.isActiveAndEnabled)
                    continue;

                int camCullingMask = cam.cullingMask;

                if ((camCullingMask & layer) == layer)
                {
                    int bitCount = Bits.Count(camCullingMask);
                    if (bitCount < mostSpecificBitCount)
                    {
                        found = cam;
                        mostSpecificBitCount = bitCount;
                    }
                }
            }

            Array.Clear(cameraArray, 0, cameraCount);
            outCamera = found;
            return found;
        }

        static private readonly Camera[] s_CachedCameraArray = new Camera[64];

        #endregion // Camera

        #region Polyfills

        /// <summary>
        /// Sets a parent and preserves world position, rotation, and scale.
        /// </summary>
        static public void SetParentPreserveWorld(this Transform inTransform, Transform parent)
        {
            inTransform.GetPositionAndRotation(out Vector3 p, out Quaternion q);
            inTransform.SetParent(parent, true);
            inTransform.SetPositionAndRotation(p, q);
        }

#if !UNITY_2021_3_OR_NEWER
        /// <summary>
        /// Polyfill for Transform.GetPositionAndRotation.
        /// </summary>
        static public void GetPositionAndRotation(this Transform inTransform, out Vector3 position, out Quaternion rotation)
        {
            position = inTransform.position;
            rotation = inTransform.rotation;
        }

        /// <summary>
        /// Polyfill for Transform.GetLocalPositionAndRotation.
        /// </summary>
        static public void GetLocalPositionAndRotation(this Transform inTransform, out Vector3 position, out Quaternion rotation)
        {
            position = inTransform.localPosition;
            rotation = inTransform.localRotation;
        }

        /// <summary>
        /// Polyfill for Transform.SetLocalPositionAndRotation.
        /// </summary>
        static public void SetLocalPositionAndRotation(this Transform inTransform, Vector3 position, Quaternion rotation)
        {
            inTransform.localPosition = position;
            inTransform.localRotation = rotation;
        }
#endif // UNITY_2021_3_OR_NEWER

        #endregion // Polyfills

        #region Flattening Helpers

        /// <summary>
        /// Flattens the hierarchy at this transform. Children will become siblings.
        /// </summary>
        static public void FlattenHierarchy(this Transform inTransform, bool inbRecursive = false)
        {
            if (inbRecursive)
            {
                int placeIdx = inTransform.GetSiblingIndex() + 1;
                FlattenHierarchyRecursive(inTransform, inTransform.parent, ref placeIdx);
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(inTransform);
                if (root != null)
                    PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
#endif // UNITY_EDITOR

            Transform transform = inTransform;
            Transform parent = transform.parent;
            Transform child;
            int childCount = transform.childCount;
            int siblingIdx = transform.GetSiblingIndex() + 1;
            while (childCount-- > 0)
            {
                child = transform.GetChild(0);
                child.SetParentPreserveWorld(parent);
                child.SetSiblingIndex(siblingIdx++);
            }
        }

        static private void FlattenHierarchyRecursive(Transform inTransform, Transform inParent, ref int ioSiblingIndex)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(inTransform);
                if (root != null)
                    PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
#endif // UNITY_EDITOR

            Transform transform = inTransform;
            Transform child;
            int childCount = transform.childCount;
            while (childCount-- > 0)
            {
                child = transform.GetChild(0);
                child.SetParentPreserveWorld(inParent);
                child.SetSiblingIndex(ioSiblingIndex++);
                FlattenHierarchyRecursive(child, inParent, ref ioSiblingIndex);
            }
        }

        #endregion // Flattening Helpers

        #region State Hash

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct TransformHashState
        {
            public Matrix4x4 Matrix;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct RectTransformHashState
        {
            public Matrix4x4 Matrix;
            public Rect Rect;
        }

        /// <summary>
        /// Calculates a 64-bit hash for the current state of the transform.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        static public unsafe ulong GetStateHash(this Transform inTransform)
        {
            RectTransform r = inTransform as RectTransform;
            if (!ReferenceEquals(r, null))
            {
                RectTransformHashState state;
                state.Matrix = inTransform.localToWorldMatrix;
                state.Rect = r.rect;
                return Unsafe.Hash64(&state, sizeof(RectTransformHashState));
            }
            else
            {
                TransformHashState state;
                state.Matrix = inTransform.localToWorldMatrix;
                return Unsafe.Hash64(&state, sizeof(TransformHashState));
            }
        }

        #endregion // State Hash
    }
}