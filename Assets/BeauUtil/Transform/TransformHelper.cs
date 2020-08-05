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

            int cameraCount = Camera.GetAllCameras(s_CachedCameraArray);
            
            Camera found = null;
            for(int i = 0; i < cameraCount; ++i)
            {
                Camera cam = s_CachedCameraArray[i];
                if (!inbIncludeInactive && !cam.isActiveAndEnabled)
                    continue;

                if ((cam.cullingMask & layer) == layer)
                {
                    found = cam;
                    break;
                }
            }

            Array.Clear(s_CachedCameraArray, 0, cameraCount);
            outCamera = found;
            return found;
        }

        static private readonly Camera[] s_CachedCameraArray = new Camera[64];

        #endregion // RectTransform
    }
}