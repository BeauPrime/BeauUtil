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
        static public Vector2 ScreenPosition(this Transform inTransform)
        {
            return ScreenPosition(inTransform, Camera.main);
        }

        /// <summary>
        /// Returns the screen position of this transform, offset by the given offsets, using the default camera.
        /// </summary>
        static public Vector2 ScreenPosition(this Transform inTransform, TransformOffset inOffset)
        {
            return ScreenPosition(inTransform, Camera.main, inOffset);
        }

        /// <summary>
        /// Returns the screen position of this transform using the given camera.
        /// </summary>
        static public Vector2 ScreenPosition(this Transform inTransform, Camera inCamera)
        {
            return RectTransformUtility.WorldToScreenPoint(inCamera, inTransform.position);
        }

        /// <summary>
        /// Returns the screen position of this transform, offset by the given offsets, using the given camera.
        /// </summary>
        static public Vector2 ScreenPosition(this Transform inTransform, Camera inCamera, TransformOffset inOffset)
        {
            return RectTransformUtility.WorldToScreenPoint(inCamera, inOffset.EvaluateWorld(inTransform));
        }

        #endregion // Screen Position

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
            
            for(int i = 0; i < cameraCount; ++i)
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
            while(childCount-- > 0)
            {
                child = transform.GetChild(0);
                child.SetParent(parent, true);
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
            while(childCount-- > 0)
            {
                child = transform.GetChild(0);
                child.SetParent(inParent, true);
                child.SetSiblingIndex(ioSiblingIndex++);
                FlattenHierarchyRecursive(child, inParent, ref ioSiblingIndex);
            }
        }

        #endregion // Flattening Helpers
    }
}