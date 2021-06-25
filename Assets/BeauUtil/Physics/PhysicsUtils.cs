/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Dec 2020
 * 
 * File:    PhysicsUtils.cs
 * Purpose: Physics utilities and extension methods.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;
using BeauUtil.Debugger;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Physics utilities and extension methods.
    /// </summary>
    static public partial class PhysicsUtils
    {
        #region Cached Buffers

        private const int DefaultMaxBufferSize = 32;

        static private int s_CurrentBufferSize3d = DefaultMaxBufferSize;
        static private int s_CurrentBufferSize2d = DefaultMaxBufferSize;

        static private void ClearBuffer<T>(T[] inBuffer, int inLength)
        {
            Array.Clear(inBuffer, 0, inLength);
        }

        static private void CopyBuffer(RaycastHit[] inBuffer, int inLength, List<Collider> outColliders)
        {
            ListUtils.EnsureCapacityPow2(ref outColliders, outColliders.Count + inLength);

            for(int i = 0; i < inLength; ++i)
                outColliders.Add(inBuffer[i].collider);
        }

        static private void CopyBuffer(RaycastHit2D[] inBuffer, int inLength, List<Collider2D> outColliders)
        {
            ListUtils.EnsureCapacityPow2(ref outColliders, outColliders.Count + inLength);

            if (outColliders.Capacity < outColliders.Count + inLength)
            for(int i = 0; i < inLength; ++i)
                outColliders.Add(inBuffer[i].collider);
        }

        /// <summary>
        /// Size of internal PhysicsUtils buffers for 2d collisions, used for processing collisions.
        /// Defaults to 64.
        /// </summary>
        static public int BufferSize2D
        {
            get { return s_CurrentBufferSize2d; }
            set
            {
                if (value == s_CurrentBufferSize2d)
                    return;

                if (value != 0 && (value < 16 || value > ushort.MaxValue + 1))
                    throw new ArgumentOutOfRangeException("value", "Buffer size must be between 16 and 655356");

                if (value > 0 && !Mathf.IsPowerOfTwo(value))
                    throw new ArgumentException("Buffer size must be a power of 2", "value");

                s_CurrentBufferSize2d = value;

                Array.Resize(ref s_CachedCollider2DArray, value);
                Array.Resize(ref s_CachedRaycast2DArray, value);
            }
        }

        /// <summary>
        /// Size of internal PhysicsUtils buffers for 3d collisions, used for processing collisions.
        /// Defaults to 64.
        /// </summary>
        static public int BufferSize3D
        {
            get { return s_CurrentBufferSize3d; }
            set
            {
                if (value == s_CurrentBufferSize3d)
                    return;

                if (value != 0 && (value < 16 || value > ushort.MaxValue + 1))
                    throw new ArgumentOutOfRangeException("value", "Buffer size must be between 16 and 655356");

                if (value > 0 && !Mathf.IsPowerOfTwo(value))
                    throw new ArgumentException("Buffer size must be a power of 2", "value");

                s_CurrentBufferSize3d = value;

                // TODO: Resize 3d buffers
            }
        }

        #endregion // Cached Buffers

        #region Closest

        /// <summary>
        /// Returns the closest hit from the given raycast array.
        /// </summary>
        static public RaycastHit ClosestHit(RaycastHit[] inArray, int inLength)
        {
            float minDist = float.MaxValue;
            RaycastHit closest = default(RaycastHit);
            RaycastHit checking;
            for(int i = 0; i < inLength; ++i)
            {
                checking = inArray[i];
                if (checking.distance < minDist)
                {
                    minDist = checking.distance;
                    closest = checking;
                }
            }

            return closest;
        }

        /// <summary>
        /// Returns the closest hit from the given raycast array, ignoring any with 0 or less distance.
        /// </summary>
        static public RaycastHit ClosestHitNonZero(RaycastHit[] inArray, int inLength)
        {
            float minDist = float.MaxValue;
            RaycastHit closest = default(RaycastHit);
            RaycastHit checking;
            for(int i = 0; i < inLength; ++i)
            {
                checking = inArray[i];
                if (checking.distance > 0 && checking.distance < minDist)
                {
                    minDist = checking.distance;
                    closest = checking;
                }
            }

            return closest;
        }

        /// <summary>
        /// Returns the closest hit from the given raycast list.
        /// </summary>
        static public RaycastHit ClosestHit(ListSlice<RaycastHit> inList)
        {
            float minDist = float.MaxValue;
            RaycastHit closest = default(RaycastHit);
            RaycastHit checking;
            for(int i = 0, length = inList.Length; i < length; ++i)
            {
                checking = inList[i];
                if (checking.distance < minDist)
                {
                    minDist = checking.distance;
                    closest = checking;
                }
            }

            return closest;
        }

        /// <summary>
        /// Returns the closest hit from the given raycast list, ignoring any with 0 or less distance.
        /// </summary>
        static public RaycastHit ClosestHitNonZero(ListSlice<RaycastHit> inList)
        {
            float minDist = float.MaxValue;
            RaycastHit closest = default(RaycastHit);
            RaycastHit checking;
            for(int i = 0, length = inList.Length; i < length; ++i)
            {
                checking = inList[i];
                if (checking.distance > 0 && checking.distance < minDist)
                {
                    minDist = checking.distance;
                    closest = checking;
                }
            }

            return closest;
        }

        /// <summary>
        /// Returns the closest hit from the given raycast array.
        /// </summary>
        static public RaycastHit2D ClosestHit(RaycastHit2D[] inArray, int inLength)
        {
            float minDist = float.MaxValue;
            RaycastHit2D closest = default(RaycastHit2D);
            RaycastHit2D checking;
            for(int i = 0; i < inLength; ++i)
            {
                checking = inArray[i];
                if (checking.distance < minDist)
                {
                    minDist = checking.distance;
                    closest = checking;
                }
            }

            return closest;
        }

        /// <summary>
        /// Returns the closest hit from the given raycast array, ignoring any with 0 or less distance.
        /// </summary>
        static public RaycastHit2D ClosestHitNonZero(RaycastHit2D[] inArray, int inLength)
        {
            float minDist = float.MaxValue;
            RaycastHit2D closest = default(RaycastHit2D);
            RaycastHit2D checking;
            for(int i = 0; i < inLength; ++i)
            {
                checking = inArray[i];
                if (checking.distance > 0 && checking.distance < minDist)
                {
                    minDist = checking.distance;
                    closest = checking;
                }
            }

            return closest;
        }

        /// <summary>
        /// Returns the closest hit from the given raycast list.
        /// </summary>
        static public RaycastHit2D ClosestHit(ListSlice<RaycastHit2D> inList)
        {
            float minDist = float.MaxValue;
            RaycastHit2D closest = default(RaycastHit2D);
            RaycastHit2D checking;
            for(int i = 0, length = inList.Length; i < length; ++i)
            {
                checking = inList[i];
                if (checking.distance < minDist)
                {
                    minDist = checking.distance;
                    closest = checking;
                }
            }

            return closest;
        }

        /// <summary>
        /// Returns the closest hit from the given raycast list, ignoring any with 0 or less distance.
        /// </summary>
        static public RaycastHit2D ClosestHitNonZero(ListSlice<RaycastHit2D> inList)
        {
            float minDist = float.MaxValue;
            RaycastHit2D closest = default(RaycastHit2D);
            RaycastHit2D checking;
            for(int i = 0, length = inList.Length; i < length; ++i)
            {
                checking = inList[i];
                if (checking.distance > 0 && checking.distance < minDist)
                {
                    minDist = checking.distance;
                    closest = checking;
                }
            }

            return closest;
        }

        #endregion // Closest

        #region Private Filters

        private struct SingleTagFilter : IObjectFilter<GameObject>
        {
            private readonly string m_Tag;

            internal SingleTagFilter(string inTag)
            {
                m_Tag = inTag;
            }

            public bool Allow(GameObject inObject)
            {
                return inObject.CompareTag(m_Tag);
            }
        }

        #endregion // Private Filters

        #region Uniform Scaling

        /// <summary>
        /// Ensures scaling is uniform for this transform and its colliders.
        /// </summary>
        static public bool EnsureUniformScale(Transform inTransform, bool inbForceToIdentity = false)
        {
            // if scaling is uniform, then we don't have to do anything
            Vector3 scale = inTransform.localScale;
            
            if (inbForceToIdentity)
            {
                if (scale.x == 1 && scale.y == 1 && scale.z == 1)
                    return false;
            }
            else
            {
                if (scale.x == scale.y && scale.y == scale.z)
                    return false;
            }

            Collider2D[] collider2ds = inTransform.GetComponents<Collider2D>();
            foreach(var collider2d in collider2ds)
            {
                ApplyScale(collider2d, scale);
            }

            Collider[] colliders = inTransform.GetComponents<Collider>();
            foreach(var collider in colliders)
            {
                ApplyScale(collider, scale);
            }

            inTransform.localScale = Vector3.one;
            return true;
        }

        /// <summary>
        /// Adjusts the given collider for a given scale.
        /// </summary>
        static public bool ApplyScale(Collider inCollider, Vector3 inScale)
        {
            BoxCollider box = inCollider as BoxCollider;
            if (box != null)
            {
                box.center = Vec3Mult(box.center, inScale);

                Vec3Abs(ref inScale);
                box.size = Vec3Mult(box.size, inScale);
                return true;
            }

            SphereCollider sphere = inCollider as SphereCollider;
            if (sphere != null)
            {
                sphere.center = Vec3Mult(sphere.center, inScale);

                Vec3Abs(ref inScale);
                sphere.radius *= Math.Max(Math.Max(inScale.x, inScale.y), inScale.z);
                return true;
            }

            CapsuleCollider capsule = inCollider as CapsuleCollider;
            if (capsule != null)
            {
                capsule.center = Vec3Mult(capsule.center, inScale);
                
                Vec3Abs(ref inScale);
                switch(capsule.direction)
                {
                    case 0:
                        capsule.height *= inScale.x;
                        capsule.radius *= Math.Max(inScale.y, inScale.z);
                        break;

                    case 1:
                        capsule.height *= inScale.y;
                        capsule.radius *= Math.Max(inScale.x, inScale.z);
                        break;

                    case 2:
                        capsule.height *= inScale.z;
                        capsule.radius *= Math.Max(inScale.x, inScale.y);
                        break;
                }
                return true;
            }

            Log.Warn("[PhysicsUtils] Unable to adjust scaling on a collider of type '{0}'", inCollider.GetType().Name);
            return false;
        }

        #endregion // Scaling

        #region Math
        
        static private void Vec3Abs(ref Vector3 inVec)
        {
            inVec.x = Math.Abs(inVec.x);
            inVec.y = Math.Abs(inVec.y);
            inVec.z = Math.Abs(inVec.z);
        }

        static private Vector3 Vec3Mult(Vector3 inA, Vector3 inB)
        {
            Vector3 result;
            result.x = inA.x * inB.x;
            result.y = inA.y * inB.y;
            result.z = inA.z * inB.z;
            return result;
        }

        #endregion // Math
    }
}