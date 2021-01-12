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
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Physics utilities and extension methods.
    /// </summary>
    static public partial class PhysicsUtils
    {
        #region Cached Buffers

        private const int DefaultMaxBufferSize = 64;

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
        /// Returns the closest hit from the given raycast list.
        /// </summary>
#if EXPANDED_REFS
        static public RaycastHit2D ClosestHit(in ListSlice<RaycastHit2D> inList)
#else
        static public RaycastHit2D ClosestHit(ListSlice<RaycastHit2D> inList)
#endif // EXPANDED_REFS
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
    }
}