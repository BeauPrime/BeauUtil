/*
 * Copyright (C) 2024. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    17 Nov 2024
 * 
 * File:    PhysicsUtils.3D.cs
 * Purpose: 3D physics utilities and extension methods.
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

        static private readonly Collider[] s_TinyColliderArray = new Collider[1];
        static private readonly RaycastHit[] s_TinyRaycastArray = new RaycastHit[1];
        static private Collider[] s_CachedColliderArray = new Collider[DefaultMaxBufferSize];
        static private RaycastHit[] s_CachedRaycastArray = new RaycastHit[DefaultMaxBufferSize];

        #endregion // Cached Buffers

        #region Filtering

        /// <summary>
        /// Filters the given array of colliders based on whether or not the collider passes a given filter.
        /// </summary>
#if EXPANDED_REFS
        static public int Filter<T>(Collider[] ioColliders, int inLength, in T inFilter) where T : IObjectFilter<Collider>
#else
        static public int Filter<T>(Collider[] ioColliders, int inLength, T inFilter) where T : IObjectFilter<Collider>
#endif // EXPANDED_REFS
        {
            Collider test;
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
        static public int FilterGO<T>(Collider[] ioColliders, int inLength, in T inFilter) where T : IObjectFilter<GameObject>
#else
        static public int FilterGO<T>(Collider[] ioColliders, int inLength, T inFilter) where T : IObjectFilter<GameObject>
#endif // EXPANDED_REFS
        {
            Collider test;
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
        static public int Filter<T>(RaycastHit[] ioRaycasts, int inLength, in T inFilter) where T : IObjectFilter<Collider>
#else
        static public int Filter<T>(RaycastHit[] ioRaycasts, int inLength, T inFilter) where T : IObjectFilter<Collider>
#endif // EXPANDED_REFS
        {
            Collider test;
            for(int i = inLength - 1; i >= 0; --i)
            {
                test = ioRaycasts[i].collider;
                if (!inFilter.Allow(test))
                    ArrayUtils.FastRemoveAt(ioRaycasts, ref inLength, i);
            }

            return inLength;
        }

        /// <summary>
        /// Filters the given array of raycast3d based on whether or not the gameobject passes a given filter.
        /// </summary>
        static public int FilterGO<T>(RaycastHit[] ioRaycasts, int inLength, T inFilter) where T : IObjectFilter<GameObject>
        {
            Collider test;
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
        static public bool Check<T>(Collider[] inColliders, int inLength, T inFilter, out Collider outCollider) where T : IObjectFilter<Collider>
        {
            Collider test;
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
        static public bool CheckGO<T>(Collider[] inColliders, int inLength, T inFilter, out Collider outCollider) where T : IObjectFilter<GameObject>
        {
            Collider test;
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
        static public bool Check<T>(RaycastHit[] inRaycasts, int inLength, T inFilter, out RaycastHit outRaycast) where T : IObjectFilter<Collider>
        {
            Collider test;
            for(int i = inLength - 1; i >= 0; --i)
            {
                test = inRaycasts[i].collider;
                if (inFilter.Allow(test))
                {
                    outRaycast = inRaycasts[i];
                    return true;
                }
            }

            outRaycast = default(RaycastHit);
            return false;
        }

        /// <summary>
        /// Returns if any colliders' gameobjects in the given array pass a given filter.
        /// </summary>
        static public bool CheckGO<T>(RaycastHit[] inRaycasts, int inLength, T inFilter, out RaycastHit outRaycast) where T : IObjectFilter<GameObject>
        {
            Collider test;
            for(int i = inLength - 1; i >= 0; --i)
            {
                test = inRaycasts[i].collider;
                if (inFilter.Allow(test.gameObject))
                {
                    outRaycast = inRaycasts[i];
                    return true;
                }
            }

            outRaycast = default(RaycastHit);
            return false;
        }

        #endregion // Filtering

        #region Scopes

        private struct RigidbodyOffsetScope : IDisposable
        {
            private readonly Rigidbody m_Rigidbody;
            private readonly Vector3 m_OriginalPos;
            private readonly bool m_OriginalTransformChanged;

            public RigidbodyOffsetScope(Rigidbody inRigidbody)
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

        #endregion // Scopes

        #region Sizing

        /// <summary>
        /// Returns the local bounds of the given collider.
        /// </summary>
        static public Bounds GetLocalBounds(Collider inCollider)
        {
            BoxCollider box = inCollider as BoxCollider;
            if (box != null)
            {
                return new Bounds(box.center, box.size);
            }

            SphereCollider sphere = inCollider as SphereCollider;
            if (sphere != null)
            {
                float diameter = sphere.radius * 2;
                return new Bounds(sphere.center, new Vector3(diameter, diameter));
            }

            CapsuleCollider capsule = inCollider as CapsuleCollider;
            if (capsule != null)
            {
                float radius = Math.Abs(capsule.radius);
                Vector3 size = new Vector3(radius * 2, radius * 2, radius * 2);
                int dir = capsule.direction;
                size[dir] = Math.Max(size[dir], capsule.height);
                return new Bounds(capsule.center, size);
            }

            MeshCollider mesh = inCollider as MeshCollider;
            if (mesh != null)
            {
                Mesh src = mesh.sharedMesh;
                if (src)
                {
                    return src.bounds;
                }

                return default(Bounds);
            }

            Log.Warn("[PhysicsUtils] Unable to get local bounds of a collider of type '{0}'", inCollider.GetType().Name);
            return default(Bounds);
        }

        /// <summary>
        /// Returns the radius of the given 3d collider.
        /// </summary>
        static public float GetRadius(Collider inCollider)
        {
            SphereCollider sphere = inCollider as SphereCollider;
            if (sphere != null)
            {
                return sphere.radius;
            }

            BoxCollider box = inCollider as BoxCollider;
            if (box != null)
            {
                Vector2 halfSize = box.size;
                halfSize.x *= 0.5f;
                halfSize.y *= 0.5f;

                return Mathf.Sqrt(halfSize.x * halfSize.x + halfSize.y * halfSize.y);
            }

            CapsuleCollider capsule = inCollider as CapsuleCollider;
            if (capsule != null)
            {
                return Math.Max(Math.Abs(capsule.height / 2), Math.Abs(capsule.radius));
            }

            MeshCollider mesh = inCollider as MeshCollider;
            if (mesh != null)
            {
                Mesh src = mesh.sharedMesh;
                if (src)
                {
                    Bounds b = src.bounds;
                    return b.extents.magnitude;
                }

                return 0;
            }

            Log.Warn("[PhysicsUtils] Unable to get radius of a collider of type '{0}'", inCollider.GetType().Name);
            return -1;
        }

        /// <summary>
        /// Returns the radius of the given 3d collider.
        /// </summary>
        static public float GetRadius(Collider inCollider, out Vector3 outLocalCenter)
        {
            SphereCollider sphere = inCollider as SphereCollider;
            if (sphere != null)
            {
                outLocalCenter = sphere.center;
                return sphere.radius;
            }

            BoxCollider box = inCollider as BoxCollider;
            if (box != null)
            {
                outLocalCenter = box.center;

                Vector2 halfSize = box.size;
                halfSize.x *= 0.5f;
                halfSize.y *= 0.5f;

                return Mathf.Sqrt(halfSize.x * halfSize.x + halfSize.y * halfSize.y);
            }

            CapsuleCollider capsule = inCollider as CapsuleCollider;
            if (capsule != null)
            {
                outLocalCenter = capsule.center;
                return Math.Max(Math.Abs(capsule.height / 2), Math.Abs(capsule.radius));
            }

            MeshCollider mesh = inCollider as MeshCollider;
            if (mesh != null)
            {
                Mesh src = mesh.sharedMesh;
                if (src)
                {
                    Bounds b = src.bounds;
                    outLocalCenter = b.center;
                    return b.extents.magnitude;
                }

                outLocalCenter = default(Vector3);
                return 0;
            }

            Log.Warn("[PhysicsUtils] Unable to get radius of a collider of type '{0}'", inCollider.GetType().Name);
            outLocalCenter = default(Vector3);
            return -1;
        }

        #endregion // Sizing
    }
}