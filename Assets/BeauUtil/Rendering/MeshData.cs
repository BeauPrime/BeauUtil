/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    18 June 2023
 * 
 * File:    MeshData.cs
 * Purpose: Mesh construction helper, using an interleaved vertex format.
*/

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace BeauUtil
{
    /// <summary>
    /// Interface for CPU-side mesh data.
    /// </summary>
    public interface IMeshData : IDisposable
    {
        void Clear();
        void Upload(Mesh ioMesh);
    }

    /// <summary>
    /// Interleaved mesh data with a 16-bit index buffer.
    /// </summary>
    public class MeshData16<TVertex> : IMeshData where TVertex : unmanaged
    {
        private const MeshUpdateFlags IgnoreAllUpdates = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers;
        private const int DefaultInitialCapacity = 32;
        private const ushort MaxIndexValue = ushort.MaxValue - 1;

        static private VertexAttributeDescriptor[] s_VertexLayout;

        private int m_VertexCount;
        private int m_IndexCount;
        private ushort[] m_IndexBuffer;
        private TVertex[] m_VertexBuffer;

        public MeshData16() : this(DefaultInitialCapacity) { }

        public MeshData16(int inInitialCapacity)
        {
            InitializeVertexLayout();

            m_VertexBuffer = new TVertex[inInitialCapacity];
            m_IndexBuffer = new ushort[inInitialCapacity];
            m_VertexCount = 0;
            m_IndexCount = 0;
        }

        public void Dispose()
        {
            Clear();
            m_VertexBuffer = null;
            m_IndexBuffer = null;
        }

        #region Buffers

        /// <summary>
        /// Preallocates buffers for the given vertex and index counts.
        /// </summary>
        public void Preallocate(int inVertexCount, int inIndexCount)
        {
            AllocateForBuffer(inVertexCount, inIndexCount);
        }

        /// <summary>
        /// Returns if adding the given vertex count would exceed the vertex cap.
        /// </summary>
        public bool NeedsFlush(int inVertexCount)
        {
            return m_VertexCount + inVertexCount > MaxIndexValue;
        }

        /// <summary>
        /// Clears the vertex and index buffers.
        /// </summary>
        public void Clear()
        {
            m_VertexCount = 0;
            m_IndexCount = 0;
        }

        /// <summary>
        /// Number of vertices in the mesh data.
        /// </summary>
        public int VertexCount
        {
            get { return m_VertexCount; }
        }

        /// <summary>
        /// Number of indices in the mesh data.
        /// </summary>
        public int IndexCount
        {
            get { return m_IndexCount; }
        }

        /// <summary>
        /// Ensures the vertex and index buffers can allow for the given new vertex and index entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AllocateForBuffer(int inVertexCount, int inIndexCount)
        {
            if (m_VertexCount + inVertexCount > MaxIndexValue)
                throw new InvalidOperationException(string.Format("Meshes can only have up to {0} vertices", MaxIndexValue));
            ArrayUtils.EnsureCapacityPow2(ref m_VertexBuffer, m_VertexCount + inVertexCount);
            ArrayUtils.EnsureCapacityPow2(ref m_IndexBuffer, m_IndexCount + inIndexCount);
        }

        /// <summary>
        /// Ensures the vertex buffer can allow for the given new vertex entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AllocateVertices(int inVertexCount)
        {
            ArrayUtils.EnsureCapacityPow2(ref m_VertexBuffer, m_VertexCount + inVertexCount);
        }

        /// <summary>
        /// Ensures the index buffer can allow for the given new index entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AllocateIndices(int inIndexCount)
        {
            ArrayUtils.EnsureCapacityPow2(ref m_IndexBuffer, m_IndexCount + inIndexCount);
        }

        #endregion // Buffers

        #region Vertices

        /// <summary>
        /// Adds a vertex.
        /// </summary>
        public MeshData16<TVertex> AddVertex(TVertex inA)
        {
            AllocateVertices(1);
            m_VertexBuffer[m_VertexCount++] = inA;
            return this;
        }

        /// <summary>
        /// Adds verticies.
        /// </summary>
        public MeshData16<TVertex> AddVertices(TVertex inA, TVertex inB)
        {
            AllocateVertices(2);
            m_VertexBuffer[m_VertexCount++] = inA;
            m_VertexBuffer[m_VertexCount++] = inB;
            return this;
        }

        /// <summary>
        /// Adds verticies.
        /// </summary>
        public MeshData16<TVertex> AddVertices(TVertex inA, TVertex inB, TVertex inC)
        {
            AllocateVertices(3);
            m_VertexBuffer[m_VertexCount++] = inA;
            m_VertexBuffer[m_VertexCount++] = inB;
            m_VertexBuffer[m_VertexCount++] = inC;
            return this;
        }

        /// <summary>
        /// Adds verticies.
        /// </summary>
        public MeshData16<TVertex> AddVertices(TVertex inA, TVertex inB, TVertex inC, TVertex inD)
        {
            AllocateVertices(4);
            m_VertexBuffer[m_VertexCount++] = inA;
            m_VertexBuffer[m_VertexCount++] = inB;
            m_VertexBuffer[m_VertexCount++] = inC;
            m_VertexBuffer[m_VertexCount++] = inD;
            return this;
        }

        #endregion // Vertices

        #region Indices

        /// <summary>
        /// Adds an index.
        /// </summary>
        public MeshData16<TVertex> AddIndex(ushort inA)
        {
            AllocateIndices(1);
            m_IndexBuffer[m_IndexCount++] = inA;
            return this;
        }

        /// <summary>
        /// Adds indicies.
        /// </summary>
        public MeshData16<TVertex> AddIndices(ushort inA, ushort inB)
        {
            AllocateIndices(2);
            m_IndexBuffer[m_IndexCount++] = inA;
            m_IndexBuffer[m_IndexCount++] = inB;
            return this;
        }

        /// <summary>
        /// Adds indicies.
        /// </summary>
        public MeshData16<TVertex> AddIndices(ushort inA, ushort inB, ushort inC)
        {
            AllocateIndices(3);
            m_IndexBuffer[m_IndexCount++] = inA;
            m_IndexBuffer[m_IndexCount++] = inB;
            m_IndexBuffer[m_IndexCount++] = inC;
            return this;
        }

        #endregion // Indices

        #region Shapes

        /// <summary>
        /// Adds a triangle to the mesh.
        /// </summary>
        public MeshData16<TVertex> AddTriangle(TVertex inA, TVertex inB, TVertex inC)
        {
            AllocateIndices(3);
            int currentVertCount = m_VertexCount;
            m_IndexBuffer[m_IndexCount++] = (ushort) currentVertCount;
            m_IndexBuffer[m_IndexCount++] = (ushort) (currentVertCount + 1);
            m_IndexBuffer[m_IndexCount++] = (ushort) (currentVertCount + 2);

            AllocateVertices(3);
            m_VertexBuffer[m_VertexCount++] = inA;
            m_VertexBuffer[m_VertexCount++] = inB;
            m_VertexBuffer[m_VertexCount++] = inC;
            return this;
        }

        /// <summary>
        /// Adds a quad to the mesh.
        /// </summary>
        public MeshData16<TVertex> AddQuad(TVertex inA, TVertex inB, TVertex inC, TVertex inD)
        {
            AllocateIndices(6);
            int currentVertCount = m_VertexCount;
            m_IndexBuffer[m_IndexCount++] = (ushort) currentVertCount;
            m_IndexBuffer[m_IndexCount++] = (ushort) (currentVertCount + 1);
            m_IndexBuffer[m_IndexCount++] = (ushort) (currentVertCount + 2);
            m_IndexBuffer[m_IndexCount++] = (ushort) (currentVertCount + 3);
            m_IndexBuffer[m_IndexCount++] = (ushort) (currentVertCount + 2);
            m_IndexBuffer[m_IndexCount++] = (ushort) (currentVertCount + 1);

            AllocateVertices(4);
            m_VertexBuffer[m_VertexCount++] = inA;
            m_VertexBuffer[m_VertexCount++] = inB;
            m_VertexBuffer[m_VertexCount++] = inC;
            m_VertexBuffer[m_VertexCount++] = inD;
            return this;
        }

        #endregion // Triangles

        #region Upload

        /// <summary>
        /// Uploads this mesh data to the given mesh.
        /// </summary>
        public void Upload(Mesh ioMesh)
        {
            if (ioMesh == null)
                throw new ArgumentNullException("ioMesh");

            ioMesh.SetVertexBufferParams(m_VertexCount, s_VertexLayout);
            ioMesh.SetVertexBufferData(m_VertexBuffer, 0, 0, m_VertexCount, 0, IgnoreAllUpdates);

            ioMesh.SetIndexBufferParams(m_IndexCount, IndexFormat.UInt16);
            ioMesh.SetIndexBufferData(m_IndexBuffer, 0, 0, m_IndexCount, IgnoreAllUpdates);

            ioMesh.subMeshCount = 1;
            ioMesh.SetSubMesh(0, new SubMeshDescriptor(0, m_IndexCount), IgnoreAllUpdates);

            ioMesh.RecalculateBounds();
            ioMesh.UploadMeshData(false);
        }

        /// <summary>
        /// Uploads this mesh data to the given mesh and clears all buffers.
        /// </summary>
        public void Flush(Mesh ioMesh)
        {
            Upload(ioMesh);
            Clear();
        }

        #endregion // Upload

        #region Vertex Layout

        static private void InitializeVertexLayout()
        {
            if (s_VertexLayout != null)
                return;

            s_VertexLayout = VertexUtility.GenerateLayout(typeof(TVertex));
        }

        /// <summary>
        /// Overrides the vertex layout for this vertex type.
        /// </summary>
        static public void OverrideLayout(VertexAttributeDescriptor[] inLayout)
        {
            if (inLayout == null)
                throw new ArgumentNullException("inDescriptors");

            s_VertexLayout = inLayout;
        }

        #endregion // Vertex Layout
    }
}