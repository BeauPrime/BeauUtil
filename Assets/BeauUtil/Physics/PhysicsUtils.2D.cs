/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Dec 2020
 * 
 * File:    PhysicsUtils.2D.cs
 * Purpose: 2D physics utilities and extension methods.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BeauUtil.Debugger;
using UnityEngine;

namespace BeauUtil
{
    static public partial class PhysicsUtils
    {
        #region Cached Buffers

        static private readonly Collider2D[] s_TinyCollider2DArray = new Collider2D[1];
        static private readonly RaycastHit2D[] s_TinyRaycast2DArray = new RaycastHit2D[1];
        static private Collider2D[] s_CachedCollider2DArray = new Collider2D[DefaultMaxBufferSize];
        static private RaycastHit2D[] s_CachedRaycast2DArray = new RaycastHit2D[DefaultMaxBufferSize];

        #endregion // Cached Buffers

        #region Filtering

        /// <summary>
        /// Filters the given array of colliders based on whether or not the collider passes a given filter.
        /// </summary>
#if EXPANDED_REFS
        static public int Filter<T>(Collider2D[] ioColliders, int inLength, in T inFilter) where T : IObjectFilter<Collider2D>
#else
        static public int Filter<T>(Collider2D[] ioColliders, int inLength, T inFilter) where T : IObjectFilter<Collider2D>
#endif // EXPANDED_REFS
        {
            Collider2D test;
            for(int i = inLength - 1; i >= 0; --i)
            {
                test = ioColliders[i];
                if (!inFilter.Allow(test))
                    ArrayUtils.FastRemoveAt(ioColliders, ref inLength, i);
            }

            return inLength;
        }

        /// <summary>
        /// Filters the given array of colliders based on whether or not the gameobject passes a given filter.
        /// </summary>
#if EXPANDED_REFS
        static public int FilterGO<T>(Collider2D[] ioColliders, int inLength, in T inFilter) where T : IObjectFilter<GameObject>
#else
        static public int FilterGO<T>(Collider2D[] ioColliders, int inLength, T inFilter) where T : IObjectFilter<GameObject>
#endif // EXPANDED_REFS
        {
            Collider2D test;
            for(int i = inLength - 1; i >= 0; --i)
            {
                test = ioColliders[i];
                if (!inFilter.Allow(test.gameObject))
                    ArrayUtils.FastRemoveAt(ioColliders, ref inLength, i);
            }

            return inLength;
        }

        /// <summary>
        /// Filters the given array of raycasthits based on whether or not the collider passes a given filter.
        /// </summary>
#if EXPANDED_REFS
        static public int Filter<T>(RaycastHit2D[] ioRaycasts, int inLength, in T inFilter) where T : IObjectFilter<Collider2D>
#else
        static public int Filter<T>(RaycastHit2D[] ioRaycasts, int inLength, T inFilter) where T : IObjectFilter<Collider2D>
#endif // EXPANDED_REFS
        {
            Collider2D test;
            for(int i = inLength - 1; i >= 0; --i)
            {
                test = ioRaycasts[i].collider;
                if (!inFilter.Allow(test))
                    ArrayUtils.FastRemoveAt(ioRaycasts, ref inLength, i);
            }

            return inLength;
        }

        /// <summary>
        /// Filters the given array of raycast2d based on whether or not the gameobject passes a given filter.
        /// </summary>
        static public int FilterGO<T>(RaycastHit2D[] ioRaycasts, int inLength, T inFilter) where T : IObjectFilter<GameObject>
        {
            Collider2D test;
            for(int i = inLength - 1; i >= 0; --i)
            {
                test = ioRaycasts[i].collider;
                if (!inFilter.Allow(test.gameObject))
                    ArrayUtils.FastRemoveAt(ioRaycasts, ref inLength, i);
            }

            return inLength;
        }

        #endregion // Filtering

        #region Checks

        /// <summary>
        /// Returns if any colliders in the given array pass a given filter.
        /// </summary>
        static public bool Check<T>(Collider2D[] inColliders, int inLength, T inFilter, out Collider2D outCollider) where T : IObjectFilter<Collider2D>
        {
            Collider2D test;
            for(int i = inLength - 1; i >= 0; --i)
            {
                test = inColliders[i];
                if (inFilter.Allow(test))
                {
                    outCollider = test;
                    return true;
                }
            }

            outCollider = null;
            return false;
        }

        /// <summary>
        /// Returns if any colliders' gameobjects in the given array pass a given filter.
        /// </summary>
        static public bool CheckGO<T>(Collider2D[] inColliders, int inLength, T inFilter, out Collider2D outCollider) where T : IObjectFilter<GameObject>
        {
            Collider2D test;
            for(int i = inLength - 1; i >= 0; --i)
            {
                test = inColliders[i];
                if (inFilter.Allow(test.gameObject))
                {
                    outCollider = test;
                    return true;
                }
            }

            outCollider = null;
            return false;
        }

        /// <summary>
        /// Returns if any colliders in the given array pass a given filter.
        /// </summary>
        static public bool Check<T>(RaycastHit2D[] inRaycasts, int inLength, T inFilter, out RaycastHit2D outRaycast) where T : IObjectFilter<Collider2D>
        {
            Collider2D test;
            for(int i = inLength - 1; i >= 0; --i)
            {
                test = inRaycasts[i].collider;
                if (inFilter.Allow(test))
                {
                    outRaycast = inRaycasts[i];
                    return true;
                }
            }

            outRaycast = default(RaycastHit2D);
            return false;
        }

        /// <summary>
        /// Returns if any colliders' gameobjects in the given array pass a given filter.
        /// </summary>
        static public bool CheckGO<T>(RaycastHit2D[] inRaycasts, int inLength, T inFilter, out RaycastHit2D outRaycast) where T : IObjectFilter<GameObject>
        {
            Collider2D test;
            for(int i = inLength - 1; i >= 0; --i)
            {
                test = inRaycasts[i].collider;
                if (inFilter.Allow(test.gameObject))
                {
                    outRaycast = inRaycasts[i];
                    return true;
                }
            }

            outRaycast = default(RaycastHit2D);
            return false;
        }

        #endregion // Filtering

        #region Scopes

        private struct Rigidbody2DOffsetScope : IDisposable
        {
            private readonly Rigidbody2D m_Rigidbody;
            private readonly Vector2 m_OriginalPos;
            private readonly bool m_OriginalTransformChanged;

            public Rigidbody2DOffsetScope(Rigidbody2D inRigidbody)
            {
                m_Rigidbody = inRigidbody;
                m_OriginalPos = m_Rigidbody.position;
                m_OriginalTransformChanged = m_Rigidbody.transform.hasChanged;
            }

            public void Dispose()
            {
                m_Rigidbody.position = m_OriginalPos;
                m_Rigidbody.transform.hasChanged = m_OriginalTransformChanged;
            }
        }

        private struct Collider2DOffsetScope : IDisposable
        {
            private readonly Collider2D m_Collider;
            private readonly Vector2 m_OriginalOffset;
            private readonly bool m_OriginalTransformChanged;

            public Collider2DOffsetScope(Collider2D inCollider)
            {
                m_Collider = inCollider;
                m_OriginalOffset = m_Collider.offset;
                m_OriginalTransformChanged = m_Collider.transform.hasChanged;
            }

            public void Dispose()
            {
                m_Collider.offset = m_OriginalOffset;
                m_Collider.transform.hasChanged = m_OriginalTransformChanged;
            }
        }

        #endregion // Scopes

        #region Colliding vs Collider

        /// <summary>
        /// Returns if two objects are overlapping .
        /// </summary>
        static public bool IsOverlapping(this Rigidbody2D inRigidbody, Collider2D inCheck)
        {
            return inRigidbody.Distance(inCheck).isOverlapped;
        }

        /// <summary>
        /// Returns if two objects are overlapping .
        /// </summary>
        static public bool IsOverlapping(this Collider2D inCollider, Collider2D inCheck)
        {
            return inCollider.Distance(inCheck).isOverlapped;
        }

        /// <summary>
        /// Returns if two objects are touching now.
        /// </summary>
        static public bool IsTouchingNow(this Rigidbody2D inRigidbody, Collider2D inCheck)
        {
            return inRigidbody.Distance(inCheck).distance <= 0;
        }

        /// <summary>
        /// Returns if two objects are touching now.
        /// </summary>
        static public bool IsTouchingNow(this Collider2D inCollider, Collider2D inCheck)
        {
            return inCollider.Distance(inCheck).distance <= 0;
        }

        #endregion // Colliding vs Collider

        #region Colliding vs LayerMask

        #region Check

        // HERE

        /// <summary>
        /// Returns if the given Rigidbody2D is colliding with a given layer at its current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers)
        {
            return IsOverlapping(inRigidbody, inLayers, default(ContactFilter2D));
        }

        /// <summary>
        /// Returns if the given Rigidbody2D is colliding with a given layer at its current position.
        /// </summary>
        static public bool IsOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers, ContactFilter2D inContactFilter)
        {
            inContactFilter.SetLayerMask(inLayers | inContactFilter.layerMask);

            int count = inRigidbody.OverlapCollider(inContactFilter, s_TinyCollider2DArray);
            ClearBuffer(s_TinyCollider2DArray, count);
            return count > 0;
        }

        /// <summary>
        /// Returns if the given Collider2D is colliding with a given layer at its current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsOverlapping(this Collider2D inCollider, LayerMask inLayers)
        {
            return IsOverlapping(inCollider, inLayers, default(ContactFilter2D));
        }

        /// <summary>
        /// Returns if the given Collider2D is colliding with a given layer at its current position.
        /// </summary>
        static public bool IsOverlapping(this Collider2D inCollider, LayerMask inLayers, ContactFilter2D inContactFilter)
        {
            inContactFilter.SetLayerMask(inLayers | inContactFilter.layerMask);

            int count = inCollider.OverlapCollider(inContactFilter, s_TinyCollider2DArray);
            ClearBuffer(s_TinyCollider2DArray, count);
            return count > 0;
        }

        // OFFSET

        /// <summary>
        /// Returns if a given Rigidbody2D is colliding with a given layer offset from its current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers, Vector2 inOffset)
        {
            return IsOverlapping(inRigidbody, inLayers, default(ContactFilter2D), inOffset);
        }

        /// <summary>
        /// Returns if a given Rigidbody2D is colliding with a given layer offset from its current position.
        /// </summary>
#if EXPANDED_REFS
        static public bool IsOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers, in ContactFilter2D inContactFilter, in Vector2 inOffset)
#else
        static public bool IsOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers, ContactFilter2D inContactFilter, Vector2 inOffset)
#endif // EXPANDED_REFS
        {
            using(new Rigidbody2DOffsetScope(inRigidbody))
            {
                inRigidbody.position += inOffset;
                return IsOverlapping(inRigidbody, inLayers, inContactFilter);
            }
        }

        /// <summary>
        /// Returns if a given Collider2D is colliding with a given layer offset from its current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsOverlapping(this Collider2D inCollider, LayerMask inLayers, Vector2 inOffset)
        {
            return IsOverlapping(inCollider, inLayers, default(ContactFilter2D), inOffset);
        }

        /// <summary>
        /// Returns if a given Collider2D is colliding with a given layer offset from its current position.
        /// </summary>
#if EXPANDED_REFS
        static public bool IsOverlapping(this Collider2D inCollider, LayerMask inLayers, in ContactFilter2D inContactFilter, in Vector2 inOffset)
#else
        static public bool IsOverlapping(this Collider2D inCollider, LayerMask inLayers, ContactFilter2D inContactFilter, Vector2 inOffset)
#endif // EXPANDED_REFS
        {
            using(new Collider2DOffsetScope(inCollider))
            {
                inCollider.offset += (Vector2) inCollider.transform.InverseTransformVector(inOffset.x, inOffset.y, 0);
                return IsOverlapping(inCollider, inLayers, inContactFilter);
            }
        }

        // CAST

        /// <summary>
        /// Returns if a given Rigidbody2D is colliding with a given layer casted from its current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsCastOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers, Vector2 inOffset)
        {
            return IsCastOverlapping(inRigidbody, inLayers, inOffset, default(ContactFilter2D));
        }

        /// <summary>
        /// Returns if a given Rigidbody2D is colliding with a given layer casted from its current position.
        /// </summary>
        static public bool IsCastOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers, Vector2 inOffset, ContactFilter2D inContactFilter)
        {
            inContactFilter.SetLayerMask(inLayers | inContactFilter.layerMask);

            float dist = inOffset.magnitude;
            inOffset.x /= dist;
            inOffset.y /= dist;

            var buffer = s_TinyRaycast2DArray;
            int count = inRigidbody.Cast(inOffset, inContactFilter, buffer, dist);
            bool bOverlapping = buffer[0];
            ClearBuffer(buffer, count);
            return bOverlapping;
        }

        /// <summary>
        /// Returns if a given Collider2D is colliding with a given layer casted from its current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsCastOverlapping(this Collider2D inCollider, LayerMask inLayers, Vector2 inOffset)
        {
            return IsCastOverlapping(inCollider, inLayers, inOffset, default(ContactFilter2D));
        }

        /// <summary>
        /// Returns if a given Collider2D is colliding with a given layer casted from its current position.
        /// </summary>
        static public bool IsCastOverlapping(this Collider2D inCollider, LayerMask inLayers, Vector2 inOffset, ContactFilter2D inContactFilter)
        {
            inContactFilter.SetLayerMask(inLayers | inContactFilter.layerMask);

            float dist = inOffset.magnitude;
            inOffset.x /= dist;
            inOffset.y /= dist;

            var buffer = s_TinyRaycast2DArray;
            int count = inCollider.Cast(inOffset, inContactFilter, buffer, dist);
            bool bOverlapping = buffer[0];
            ClearBuffer(buffer, count);
            return bOverlapping;
        }

        #endregion // Check

        #region First

        // HERE

        /// <summary>
        /// Returns if the given Rigidbody2D is colliding with a given layer at its current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers, out Collider2D outFirst)
        {
            return IsOverlapping(inRigidbody, inLayers, default(ContactFilter2D), out outFirst);
        }

        /// <summary>
        /// Returns if the given Rigidbody2D is colliding with a given layer at its current position.
        /// </summary>
        static public bool IsOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers, ContactFilter2D inContactFilter, out Collider2D outFirst)
        {
            inContactFilter.SetLayerMask(inLayers | inContactFilter.layerMask);

            int count = inRigidbody.OverlapCollider(inContactFilter, s_TinyCollider2DArray);
            outFirst = s_TinyCollider2DArray[0];
            ClearBuffer(s_TinyCollider2DArray, count);
            return count > 0;
        }

        /// <summary>
        /// Returns if the given Collider2D is colliding with a given layer at its current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsOverlapping(this Collider2D inCollider, LayerMask inLayers, out Collider2D outFirst)
        {
            return IsOverlapping(inCollider, inLayers, default(ContactFilter2D), out outFirst);
        }

        /// <summary>
        /// Returns if the given Collider2D is colliding with a given layer at its current position.
        /// </summary>
        static public bool IsOverlapping(this Collider2D inCollider, LayerMask inLayers, ContactFilter2D inContactFilter, out Collider2D outFirst)
        {
            inContactFilter.SetLayerMask(inLayers | inContactFilter.layerMask);

            int count = inCollider.OverlapCollider(inContactFilter, s_TinyCollider2DArray);
            outFirst = s_TinyCollider2DArray[0];
            ClearBuffer(s_TinyCollider2DArray, count);
            return count > 0;
        }

        // OFFSET

        /// <summary>
        /// Returns if a given Rigidbody2D is colliding with a given layer offset from its current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers, Vector2 inOffset, out Collider2D outFIrst)
        {
            return IsOverlapping(inRigidbody, inLayers, default(ContactFilter2D), inOffset, out outFIrst);
        }

        /// <summary>
        /// Returns if a given Rigidbody2D is colliding with a given layer offset from its current position.
        /// </summary>
#if EXPANDED_REFS
        static public bool IsOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers, in ContactFilter2D inContactFilter, in Vector2 inOffset, out Collider2D outFirst)
#else
        static public bool IsOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers, ContactFilter2D inContactFilter, Vector2 inOffset, out Collider2D outFirst)
#endif // EXPANDED_REFS
        {
            using(new Rigidbody2DOffsetScope(inRigidbody))
            {
                inRigidbody.position += inOffset;
                return IsOverlapping(inRigidbody, inLayers, inContactFilter, out outFirst);
            }
        }

        /// <summary>
        /// Returns if a given Collider2D is colliding with a given layer offset from its current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsOverlapping(this Collider2D inCollider, LayerMask inLayers, Vector2 inOffset, out Collider2D outFirst)
        {
            return IsOverlapping(inCollider, inLayers, default(ContactFilter2D), inOffset, out outFirst);
        }

        /// <summary>
        /// Returns if a given Collider2D is colliding with a given layer offset from its current position.
        /// </summary>
#if EXPANDED_REFS
        static public bool IsOverlapping(this Collider2D inCollider, LayerMask inLayers, in ContactFilter2D inContactFilter, in Vector2 inOffset, out Collider2D outFirst)
#else
        static public bool IsOverlapping(this Collider2D inCollider, LayerMask inLayers, ContactFilter2D inContactFilter, Vector2 inOffset, out Collider2D outFirst)
#endif // EXPANDED_REFS
        {
            using(new Collider2DOffsetScope(inCollider))
            {
                inCollider.offset += (Vector2) inCollider.transform.InverseTransformVector(inOffset.x, inOffset.y, 0);
                return IsOverlapping(inCollider, inLayers, inContactFilter, out outFirst);
            }
        }

        // CAST

        /// <summary>
        /// Returns if a given Rigidbody2D is colliding with a given layer casted from its current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsCastOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers, Vector2 inOffset, out Collider2D outFirst)
        {
            return IsCastOverlapping(inRigidbody, inLayers, inOffset, default(ContactFilter2D), out outFirst);
        }

        /// <summary>
        /// Returns if a given Rigidbody2D is colliding with a given layer casted from its current position.
        /// </summary>
        static public bool IsCastOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers, Vector2 inOffset, ContactFilter2D inContactFilter, out Collider2D outFirst)
        {
            inContactFilter.SetLayerMask(inLayers | inContactFilter.layerMask);

            float dist = inOffset.magnitude;
            inOffset.x /= dist;
            inOffset.y /= dist;

            var buffer = s_CachedRaycast2DArray;
            int count = inRigidbody.Cast(inOffset, inContactFilter, buffer, dist);
            outFirst = count > 0 ? PhysicsUtils.ClosestHit(buffer, count).collider : null;
            ClearBuffer(buffer, count);
            return !ReferenceEquals(outFirst, null);
        }

        /// <summary>
        /// Returns if a given Rigidbody2D is colliding with a given layer casted from its current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsCastOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers, Vector2 inOffset, out RaycastHit2D outFirst)
        {
            return IsCastOverlapping(inRigidbody, inLayers, inOffset, default(ContactFilter2D), out outFirst);
        }

        /// <summary>
        /// Returns if a given Rigidbody2D is colliding with a given layer casted from its current position.
        /// </summary>
        static public bool IsCastOverlapping(this Rigidbody2D inRigidbody, LayerMask inLayers, Vector2 inOffset, ContactFilter2D inContactFilter, out RaycastHit2D outFirst)
        {
            inContactFilter.SetLayerMask(inLayers | inContactFilter.layerMask);

            float dist = inOffset.magnitude;
            inOffset.x /= dist;
            inOffset.y /= dist;

            var buffer = s_CachedRaycast2DArray;
            int count = inRigidbody.Cast(inOffset, inContactFilter, buffer, dist);
            outFirst = count > 0 ? PhysicsUtils.ClosestHit(buffer, count) : default(RaycastHit2D);
            ClearBuffer(buffer, count);
            return outFirst;
        }

        /// <summary>
        /// Returns if a given Collider2D is colliding with a given layer casted from its current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsCastOverlapping(this Collider2D inCollider, LayerMask inLayers, Vector2 inOffset, out Collider2D outFirst)
        {
            return IsCastOverlapping(inCollider, inLayers, inOffset, default(ContactFilter2D), out outFirst);
        }

        /// <summary>
        /// Returns if a given Collider2D is colliding with a given layer casted from its current position.
        /// </summary>
        static public bool IsCastOverlapping(this Collider2D inCollider, LayerMask inLayers, Vector2 inOffset, ContactFilter2D inContactFilter, out Collider2D outFirst)
        {
            inContactFilter.SetLayerMask(inLayers | inContactFilter.layerMask);

            float dist = inOffset.magnitude;
            inOffset.x /= dist;
            inOffset.y /= dist;

            var buffer = s_CachedRaycast2DArray;
            int count = inCollider.Cast(inOffset, inContactFilter, buffer, dist);
            outFirst = count > 0 ? ClosestHit(buffer, count).collider : null;
            ClearBuffer(buffer, count);
            return !ReferenceEquals(outFirst, null);
        }

        /// <summary>
        /// Returns if a given Collider2D is colliding with a given layer casted from its current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsCastOverlapping(this Collider2D inCollider, LayerMask inLayers, Vector2 inOffset, out RaycastHit2D outFirst)
        {
            return IsCastOverlapping(inCollider, inLayers, inOffset, default(ContactFilter2D), out outFirst);
        }

        /// <summary>
        /// Returns if a given Collider2D is colliding with a given layer casted from its current position.
        /// </summary>
        static public bool IsCastOverlapping(this Collider2D inCollider, LayerMask inLayers, Vector2 inOffset, ContactFilter2D inContactFilter, out RaycastHit2D outFirst)
        {
            inContactFilter.SetLayerMask(inLayers | inContactFilter.layerMask);

            float dist = inOffset.magnitude;
            inOffset.x /= dist;
            inOffset.y /= dist;

            var buffer = s_CachedRaycast2DArray;
            int count = inCollider.Cast(inOffset, inContactFilter, buffer, dist);
            outFirst = count > 0 ? ClosestHit(buffer, count) : default(RaycastHit2D);
            ClearBuffer(buffer, count);
            return outFirst;
        }

        #endregion // First

        #endregion // Colliding vs LayerMask

        #region Sizing

        /// <summary>
        /// Returns the local bounds of the given collider.
        /// </summary>
        static public Bounds GetLocalBounds(Collider2D inCollider)
        {
            Vector2 center = inCollider.offset;

            BoxCollider2D box = inCollider as BoxCollider2D;
            if (box != null)
            {
                return new Bounds(center, box.size);
            }

            CircleCollider2D circle = inCollider as CircleCollider2D;
            if (circle != null)
            {
                float diameter = circle.radius * 2;
                return new Bounds(center, new Vector3(diameter, diameter));
            }

            EdgeCollider2D edge = inCollider as EdgeCollider2D;
            if (edge != null)
            {
                Vector2[] points = edge.points;
                Bounds b = Geom.MinAABB(points);
                b.center += (Vector3) center;
                return b;
            }

            CapsuleCollider2D capsule = inCollider as CapsuleCollider2D;
            if (capsule != null)
            {
                Vector2 size = capsule.size;
                switch(capsule.direction)
                {
                    case CapsuleDirection2D.Horizontal:
                        size.x *= 2;
                        break;

                    case CapsuleDirection2D.Vertical:
                        size.y *= 2;
                        break;
                }
                return new Bounds(center, size);
            }

            PolygonCollider2D poly = inCollider as PolygonCollider2D;
            if (poly != null)
            {
                Vector2[] points = edge.points;
                Bounds b = Geom.MinAABB(points);
                b.center += (Vector3) center;
                return b;
            }

            Log.Warn("[PhysicsUtils] Unable to get local bounds of a collider of type '{0}'", inCollider.GetType().Name);
            return default(Bounds);
        }

        /// <summary>
        /// Returns the radius of the given 2d collider.
        /// </summary>
        static public float GetRadius(Collider2D inCollider)
        {
            CircleCollider2D circle = inCollider as CircleCollider2D;
            if (circle != null)
            {
                return circle.radius;
            }

            BoxCollider2D box = inCollider as BoxCollider2D;
            if (box != null)
            {
                Vector2 halfSize = box.size;
                halfSize.x *= 0.5f;
                halfSize.y *= 0.5f;

                return Mathf.Sqrt(halfSize.x * halfSize.x + halfSize.y * halfSize.y) + box.edgeRadius;
            }

            EdgeCollider2D edge = inCollider as EdgeCollider2D;
            if (edge != null)
            {
                Vector2[] points = edge.points;
                Bounds b = Geom.MinAABB(points);
                Vector2 halfSize = b.extents;
                return Mathf.Sqrt(halfSize.x * halfSize.x + halfSize.y * halfSize.y) + edge.edgeRadius;
            }

            CapsuleCollider2D capsule = inCollider as CapsuleCollider2D;
            if (capsule != null)
            {
                Vector2 size = capsule.size;
                switch(capsule.direction)
                {
                    case CapsuleDirection2D.Horizontal:
                        return size.x;

                    case CapsuleDirection2D.Vertical:
                        return size.y;
                }
            }

            PolygonCollider2D poly = inCollider as PolygonCollider2D;
            if (poly != null)
            {
                Vector2[] points = edge.points;
                Bounds b = Geom.MinAABB(points);
                Vector2 halfSize = b.extents;
                return Mathf.Sqrt(halfSize.x * halfSize.x + halfSize.y * halfSize.y) + edge.edgeRadius;
            }

            Log.Warn("[PhysicsUtils] Unable to get radius of a collider of type '{0}'", inCollider.GetType().Name);
            return -1;
        }

        /// <summary>
        /// Returns the radius of the given 2d collider.
        /// </summary>
        static public float GetRadius(Collider2D inCollider, out Vector2 outLocalCenter)
        {
            outLocalCenter = inCollider.offset;

            CircleCollider2D circle = inCollider as CircleCollider2D;
            if (circle != null)
            {
                return circle.radius;
            }

            BoxCollider2D box = inCollider as BoxCollider2D;
            if (box != null)
            {
                Vector2 halfSize = box.size;
                halfSize.x *= 0.5f;
                halfSize.y *= 0.5f;

                return Mathf.Sqrt(halfSize.x * halfSize.x + halfSize.y * halfSize.y) + box.edgeRadius;
            }

            EdgeCollider2D edge = inCollider as EdgeCollider2D;
            if (edge != null)
            {
                Vector2[] points = edge.points;
                Bounds b = Geom.MinAABB(points);
                Vector2 halfSize = b.extents;
                outLocalCenter += (Vector2) b.center;
                return Mathf.Sqrt(halfSize.x * halfSize.x + halfSize.y * halfSize.y) + edge.edgeRadius;
            }

            CapsuleCollider2D capsule = inCollider as CapsuleCollider2D;
            if (capsule != null)
            {
                Vector2 size = capsule.size;
                switch(capsule.direction)
                {
                    case CapsuleDirection2D.Horizontal:
                        return size.x;

                    case CapsuleDirection2D.Vertical:
                        return size.y;
                }
            }

            PolygonCollider2D poly = inCollider as PolygonCollider2D;
            if (poly != null)
            {
                Vector2[] points = edge.points;
                Bounds b = Geom.MinAABB(points);
                Vector2 halfSize = b.extents;
                outLocalCenter += (Vector2) b.center;
                return Mathf.Sqrt(halfSize.x * halfSize.x + halfSize.y * halfSize.y) + edge.edgeRadius;
            }

            Log.Warn("[PhysicsUtils] Unable to get radius of a collider of type '{0}'", inCollider.GetType().Name);
            return -1;
        }

        #endregion // Sizing
    
        #region Uniform Scaling

        /// <summary>
        /// Adjusts the given collider for a given scale.
        /// </summary>
        static public bool ApplyScale(Collider2D inCollider, Vector3 inScale)
        {
            inCollider.offset *= inScale;
            
            BoxCollider2D box = inCollider as BoxCollider2D;
            if (box != null)
            {
                Vec3Abs(ref inScale);
                box.size *= inScale;
                return true;
            }

            CircleCollider2D circle = inCollider as CircleCollider2D;
            if (circle != null)
            {
                Vec3Abs(ref inScale);
                circle.radius *= Math.Max(inScale.x, inScale.y);
                return true;
            }

            EdgeCollider2D edge = inCollider as EdgeCollider2D;
            if (edge != null)
            {
                Vector2[] points = edge.points;
                for(int i = 0, count = points.Length; i < count; i++)
                    points[i] *= inScale;
                edge.points = points;
                return true;
            }

            CapsuleCollider2D capsule = inCollider as CapsuleCollider2D;
            if (capsule != null)
            {
                Vec3Abs(ref inScale);
                capsule.size *= inScale;
                return true;
            }

            PolygonCollider2D poly = inCollider as PolygonCollider2D;
            if (poly != null)
            {
                Vector2[] points = poly.points;
                for(int i = 0, count = points.Length; i < count; i++)
                    points[i] *= inScale;
                poly.points = points;
                return false;
            }

            Log.Warn("[PhysicsUtils] Unable to adjust scaling on a collider of type '{0}'", inCollider.GetType().Name);
            return false;
        }

        #endregion // Uniform Scaling
    }
}