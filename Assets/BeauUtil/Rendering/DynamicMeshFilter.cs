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

        ~DynamicMeshFilter()
        {
            UnityHelper.SafeDestroy(ref m_DynamicMesh);
        }

        private void Awake()
        {
            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshFilter.sharedMesh = GetMesh();
        }

        private void OnDestroy()
        {
            m_MeshFilter = null;
            UnityHelper.SafeDestroy(ref m_DynamicMesh);
        }

        private Mesh GetMesh()
        {
            if (m_DynamicMesh == null)
            {
                m_DynamicMesh = new Mesh();
                m_DynamicMesh.name = name;
                m_DynamicMesh.hideFlags = HideFlags.DontSave;
                m_DynamicMesh.MarkDynamic();
            }

            return m_DynamicMesh;
        }
    
        /// <summary>
        /// Retrieves the unique Mesh instance for this DynamicMeshFilter.
        /// </summary>
        public Mesh Mesh
        {
            get { return GetMesh(); }
        }

        /// <summary>
        /// Uploads mesh data.
        /// </summary>
        public void Upload(IMeshData inMeshData)
        {
            inMeshData.Upload(GetMesh());
        }
    }
}