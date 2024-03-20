/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    19 June 2023
 * 
 * File:    DynamicMeshFilter.cs
 * Purpose: Dynamically generated Mesh filter.
*/

using System;
using UnityEngine;

namespace BeauUtil
{
    [AddComponentMenu("BeauUtil/Rendering/Dynamic Mesh Filter")]
    [RequireComponent(typeof(MeshFilter)), DisallowMultipleComponent, ExecuteAlways]
    public class DynamicMeshFilter : MonoBehaviour
    {
        [NonSerialized] private MeshFilter m_MeshFilter;
        [NonSerialized] private Mesh m_DynamicMesh;
        [NonSerialized] private MeshDataTarget m_MeshUploadTarget;

        ~DynamicMeshFilter()
        {
            UnityHelper.SafeDestroy(ref m_DynamicMesh);
        }

        private void Awake()
        {
            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshFilter.sharedMesh = GetMeshTarget().Mesh;
        }

        private void OnDestroy()
        {
            m_MeshFilter = null;
            UnityHelper.SafeDestroy(ref m_DynamicMesh);
        }

        private ref MeshDataTarget GetMeshTarget()
        {
            if (m_DynamicMesh == null)
            {
                m_DynamicMesh = new Mesh();
                m_DynamicMesh.name = name;
                m_DynamicMesh.hideFlags = HideFlags.DontSave;
                m_DynamicMesh.MarkDynamic();
                m_MeshUploadTarget.Mesh = m_DynamicMesh;
            }

            return ref m_MeshUploadTarget;
        }

        /// <summary>
        /// Retrieves the unique Mesh instance for this DynamicMeshFilter.
        /// </summary>
        public Mesh Mesh
        {
            get { return GetMeshTarget().Mesh; }
        }

        /// <summary>
        /// Retrieves a reference to the Mesh data target.
        /// </summary>
        public ref MeshDataTarget MeshTarget
        {
            get { return ref GetMeshTarget(); }
        }

        /// <summary>
        /// Uploads mesh data.
        /// </summary>
        public void Upload(IMeshData inMeshData, MeshDataUploadFlags inUploadFlags = 0)
        {
            inMeshData.Upload(ref GetMeshTarget(), inUploadFlags);
        }
    }
}