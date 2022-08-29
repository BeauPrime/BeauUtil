/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    31 Autust 2020
 * 
 * File:    CameraHelper.cs
 * Purpose: Camera utility methods.
 */

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Camera utility methods.
    /// </summary>
    static public class CameraHelper
    {
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
        [MethodImpl(256)]
        static public bool TryGetDistanceToPointPlane(this Camera inCamera, Vector3 inPosition, out float outDistance)
        {
            Plane p = new Plane(-inCamera.transform.forward, inPosition);
            return TryGetDistanceToPlane(inCamera, p, out outDistance);
        }

        /// <summary>
        /// Attempts to get the distance from the camera to the object plane.
        /// Note: This is distance from the camera transform, and not the near plane.
        /// </summary>
        [MethodImpl(256)]
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
                outNewPosition = sourcePos;
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

            if (inCamera.orthographic)
            {
                outNewPosition = inPosition;
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

        /// <summary>
        /// Returns the FOV (in degrees) required for a camera to have a frustrum height at a specific distance from the camera.
        /// </summary>
        static public float FOVForHeightAndDistance(float inHeight, float inDistance)
        {
            return 2.0f * Mathf.Atan(inHeight * 0.5f / inDistance) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Returns the distance from the camera of a specific frustrum height.
        /// </summary>
        static public float DistanceForHeightAndFOV(float inHeight, float inFOV)
        {
            return (float) (inHeight * 0.5f / Math.Tan(inFOV / 2 * Mathf.Deg2Rad));
        }

        /// <summary>
        /// Returns the distance from the camera of a specific frustrum height.
        /// </summary>
        static public float HeightForDistanceAndFOV(float inDistance, float inFOV)
        {
            return (float) (2f * Math.Tan(inFOV / 2 * Mathf.Deg2Rad) * inDistance);
        }
    }
}