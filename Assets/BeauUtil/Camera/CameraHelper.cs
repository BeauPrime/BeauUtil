/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    31 Autust 2020
 * 
 * File:    CameraHelper.cs
 * Purpose: Camera utility methods.
 */

using System;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Camera utility methods.
    /// </summary>
    static public class CameraHelper
    {
        /// <summary>
        /// Attempts to get the distance from the camera to the object plane.
        /// </summary>
        static public bool TryGetDistanceToObjectPlane(this Camera inCamera, Transform inTransform, out float outDistance)
        {
            if (!inTransform)
            {
                outDistance = 0;
                return false;
            }

            Plane p = new Plane(-inCamera.transform.forward, inTransform.position);
            Ray r = inCamera.ViewportPointToRay(s_CenterViewportPoint);

            return p.Raycast(r, out outDistance);
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
        static public bool TryCastPositionToTargetPlane(this Camera inCamera, Transform inTransform, Transform inTargetTransform, out Vector3 outNewPosition)
        {
            if (!inTransform || !inTargetTransform)
            {
                outNewPosition = default(Vector3);
                return false;
            }

            Vector3 sourcePos = inTransform.position;

            if (inCamera.orthographic)
            {
                outNewPosition = inTransform.position;
                return true;
            }

            Vector3 sourceViewport = inCamera.WorldToViewportPoint(sourcePos);

            Plane p = new Plane(-inCamera.transform.forward, inTargetTransform.position);
            Ray r = inCamera.ViewportPointToRay(sourceViewport);

            float dist;
            if (p.Raycast(r, out dist))
            {
                outNewPosition = r.GetPoint(dist);
                return true;
            }

            outNewPosition = default(Vector3);
            return false;
        }

        static private readonly Vector3 s_CenterViewportPoint = new Vector3(0.5f, 0.5f, 1);
    }
}