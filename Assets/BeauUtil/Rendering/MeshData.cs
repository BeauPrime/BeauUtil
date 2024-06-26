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

// This controls whether or not buffer uploads are
// done with NativeArray or just plain Array
// NativeArray skips some of the "IsBlittable" validation
// but since we're already specifying an unmanaged vertex type
// that is unnecessary and can be safely skipped
#define UNSAFE_BUFFER_COPY

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BeauUtil.Debugger;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace BeauUtil
{
    /// <summary>
    /// Interface for CPU-side mesh data.
    /// </summary>
    public interface IMeshData : IDisposable
    {
        int VertexCount { get; }
        int IndexCount { get; }
        MeshTopology Topology { get; }

        void Preallocate(int inVertexCount, int inIndexCount);
        bool NeedsFlush(int inVertexCount, int inIndexCount);
        void Clear();
        void Upload(ref MeshDataTarget ioTarget, MeshDataUploadFlags inUploadFlags = 0);
        void Release();
    }

    [Flags]
    public enum MeshDataUploadFlags : uint
    {
        Default = 0,
        MarkNoLongerReadable = 0x01,
        DontRecalculateBounds = 0x02,
        SkipIndexBufferUpload = 0x04
    }

    /// <summary>
    /// MeshData upload target.
    /// Provides several checks to avoid uploading duplicate data.
    /// </summary>
    public struct MeshDataTarget
    {
        public Mesh Mesh;
        public ulong VertexFormatHash;
        public IndexFormat IndexFormat;
        public MeshTopology Topology;

        static public MeshDataTarget CreateFromMesh(Mesh inMesh)
        {
            MeshDataTarget t;
            t.VertexFormatHash = 0;
            t.IndexFormat = (IndexFormat) (-1);
            t.Topology = (MeshTopology) (-1);
            t.Mesh = inMesh;
            return t;
        }
    }

    /// <summary>
    /// Interleaved mesh data with a 16-bit index buffer.
    /// </summary>
    public class MeshData16<TVertex> : IMeshData where TVertex : unmanaged
    {
        private const MeshUpdateFlags IgnoreAllUpdates = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers;
        private const int DefaultInitialCapacity = 32;

        static private unsafe readonly long MaxAllowedVertices = Math.Min(VertexUtility.MaxMeshVertexStreamSize / sizeof(TVertex), ushort.MaxValue);
        static private VertexLayout s_VertexLayout;

        private int m_VertexCount;
        private int m_IndexCount;
        private ushort[] m_IndexBuffer;
        private TVertex[] m_VertexBuffer;
        private MeshTopology m_Topology;
        private bool m_Flexible;

        public MeshData16() : this(DefaultInitialCapacity, MeshTopology.Triangles, true) { }

        public MeshData16(int inInitialCapacity, MeshTopology inTopology = MeshTopology.Triangles, bool inFlexible = true)
            : this(inInitialCapacity, inInitialCapacity, inTopology, inFlexible) { }

        public MeshData16(int inInitialVertexCapacity, int inInitialIndexCapacity, MeshTopology inTopology = MeshTopology.Triangles, bool inFlexible = true)
        {
            InitializeVertexLayout();

            m_VertexBuffer = new TVertex[inInitialVertexCapacity];
            m_IndexBuffer = new ushort[inInitialIndexCapacity];
            m_VertexCount = 0;
            m_IndexCount = 0;
            m_Topology = inTopology;
            m_Flexible = inFlexible;
        }

        public void Dispose()
        {
            Clear();
            m_VertexBuffer = null;
            m_IndexBuffer = null;
        }

        /// <summary>
        /// Topology of the mesh.
        /// Defines the faces described by the index buffer.
        /// </summary>
        public MeshTopology Topology
        {
            get { return m_Topology; }
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
        public bool NeedsFlush(int inVertexCount, int inIndexCount)
        {
            return m_VertexCount + inVertexCount > MaxAllowedVertices
                || m_IndexCount + inIndexCount > VertexUtility.MaxMeshIndexStreamSize16;
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
        /// Releases buffers.
        /// </summary>
        public void Release()
        {
            Clear();
            m_VertexBuffer = Array.Empty<TVertex>();
            m_IndexBuffer = Array.Empty<ushort>();
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
            AllocateVertices(inVertexCount);
            AllocateIndices(inIndexCount);
        }

        /// <summary>
        /// Ensures the vertex buffer can allow for the given new vertex entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        private void AllocateVertices(int inVertexCount)
        {
            int required = m_VertexCount + inVertexCount;
            if (required > m_VertexBuffer.Length)
            {
                if (!m_Flexible)
                    throw new InvalidOperationException(string.Format("MeshData16 was marked inflexible, and can only have {0} vertices", m_VertexBuffer.Length));
                if (required > MaxAllowedVertices)
                    throw new InvalidOperationException(string.Format("MeshData16<{0}> can only have up to {1} vertices", typeof(TVertex).Name, MaxAllowedVertices));
                ArrayUtils.EnsureCapacity(ref m_VertexBuffer, Mathf.NextPowerOfTwo(required));
            }
        }

        /// <summary>
        /// Ensures the index buffer can allow for the given new index entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        private void AllocateIndices(int inIndexCount)
        {
            int required = m_IndexCount + inIndexCount;
            if (required > m_IndexBuffer.Length)
            {
                if (!m_Flexible)
                    throw new InvalidOperationException(string.Format("MeshData16 was marked inflexible, and can only have {0} indices", m_IndexBuffer.Length));
                if (required > VertexUtility.MaxMeshIndexStreamSize16)
                    throw new InvalidOperationException(string.Format("MeshData16 can only have up to {0} indices", VertexUtility.MaxMeshIndexStreamSize16));
                ArrayUtils.EnsureCapacity(ref m_IndexBuffer, Mathf.NextPowerOfTwo(required));
            }
        }

        #endregion // Buffers

        #region Vertices

        /// <summary>
        /// Adds a vertex.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public MeshData16<TVertex> AddVertex(in TVertex inA)
        {
            AllocateVertices(1);
            m_VertexBuffer[m_VertexCount++] = inA;
            return this;
        }

        /// <summary>
        /// Adds verticies.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public MeshData16<TVertex> AddVertices(in TVertex inA, in TVertex inB)
        {
            AllocateVertices(2);
            m_VertexBuffer[m_VertexCount++] = inA;
            m_VertexBuffer[m_VertexCount++] = inB;
            return this;
        }

        /// <summary>
        /// Adds verticies.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public MeshData16<TVertex> AddVertices(in TVertex inA, in TVertex inB, in TVertex inC)
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
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public MeshData16<TVertex> AddVertices(in TVertex inA, in TVertex inB, in TVertex inC, in TVertex inD)
        {
            AllocateVertices(4);
            m_VertexBuffer[m_VertexCount++] = inA;
            m_VertexBuffer[m_VertexCount++] = inB;
            m_VertexBuffer[m_VertexCount++] = inC;
            m_VertexBuffer[m_VertexCount++] = inD;
            return this;
        }

        /// <summary>
        /// Returns a reference to a vertex in the vertex buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public ref TVertex Vertex(int inIndex)
        {
            if (inIndex < 0 || inIndex >= m_VertexCount)
                throw new ArgumentOutOfRangeException("inIndex");
            return ref m_VertexBuffer[inIndex];
        }

        #endregion // Vertices

        #region Indices

        /// <summary>
        /// Adds an index.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public MeshData16<TVertex> AddIndex(ushort inA)
        {
            AllocateIndices(1);
            m_IndexBuffer[m_IndexCount++] = inA;
            return this;
        }

        /// <summary>
        /// Adds indicies.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
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
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public MeshData16<TVertex> AddIndices(ushort inA, ushort inB, ushort inC)
        {
            AllocateIndices(3);
            m_IndexBuffer[m_IndexCount++] = inA;
            m_IndexBuffer[m_IndexCount++] = inB;
            m_IndexBuffer[m_IndexCount++] = inC;
            return this;
        }

        /// <summary>
        /// Returns a reference to a index in the index buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public ref ushort Index(int inIndex)
        {
            if (inIndex < 0 || inIndex >= m_IndexCount)
                throw new ArgumentOutOfRangeException("inIndex");
            return ref m_IndexBuffer[inIndex];
        }

        #endregion // Indices

        #region Shapes

        /// <summary>
        /// Adds a triangle to the mesh.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public OffsetLengthU16 AddTriangle(in TVertex inA, in TVertex inB, in TVertex inC)
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
            return new OffsetLengthU16((ushort) currentVertCount, 3);
        }

        /// <summary>
        /// Adds a triangle to the mesh.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public OffsetLengthU16 AddTriangle(in Vertex3<TVertex> inVertices)
    {
            AllocateIndices(3);
            int currentVertCount = m_VertexCount;
            m_IndexBuffer[m_IndexCount++] = (ushort) currentVertCount;
            m_IndexBuffer[m_IndexCount++] = (ushort) (currentVertCount + 1);
            m_IndexBuffer[m_IndexCount++] = (ushort) (currentVertCount + 2);

            AllocateVertices(3);
            m_VertexBuffer[m_VertexCount++] = inVertices.A;
            m_VertexBuffer[m_VertexCount++] = inVertices.B;
            m_VertexBuffer[m_VertexCount++] = inVertices.C;
            return new OffsetLengthU16((ushort) currentVertCount, 3);
        }

        /// <summary>
        /// Adds a quad to the mesh.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public OffsetLengthU16 AddQuad(in TVertex inA, in TVertex inB, in TVertex inC, in TVertex inD)
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
            return new OffsetLengthU16((ushort) currentVertCount, 4);
        }

        /// <summary>
        /// Adds a quad to the mesh.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public OffsetLengthU16 AddQuad(in Vertex4<TVertex> inVertices)
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
            m_VertexBuffer[m_VertexCount++] = inVertices.A;
            m_VertexBuffer[m_VertexCount++] = inVertices.B;
            m_VertexBuffer[m_VertexCount++] = inVertices.C;
            m_VertexBuffer[m_VertexCount++] = inVertices.D;
            return new OffsetLengthU16((ushort) currentVertCount, 4);
        }

        /// <summary>
        /// Adds arbitrary vertices and indices to the mesh.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public OffsetLengthU16 AddFromBuffers(TVertex[] inVertices, int inVertexCount, ushort[] inIndices, int inIndexCount)
        {
            if (inVertexCount > inVertices.Length || inIndexCount > inIndices.Length)
                throw new ArgumentOutOfRangeException();
            
            AllocateIndices(inIndexCount);
            int currentVertCount = m_VertexCount;
            for(int i = 0, end = i + inIndexCount; i < end; i++)
            {
                m_IndexBuffer[m_IndexCount++] = (ushort) (currentVertCount + inIndices[i]);
            }

            AllocateVertices(inVertexCount);
            for(int i = 0, end = i + inVertexCount; i < end; i++)
            {
                m_VertexBuffer[m_VertexCount++] = inVertices[i];
            }
            return new OffsetLengthU16((ushort) currentVertCount, (ushort) inVertexCount);
        }

        /// <summary>
        /// Adds arbitrary vertices and indices to the mesh.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public OffsetLengthU16 AddFromBuffers(List<TVertex> inVertices, int inVertexCount, List<ushort> inIndices, int inIndexCount)
        {
            if (inVertexCount > inVertices.Count || inIndexCount > inIndices.Count)
                throw new ArgumentOutOfRangeException();

            AllocateIndices(inIndexCount);
            int currentVertCount = m_VertexCount;
            for (int i = 0, end = i + inIndexCount; i < end; i++)
            {
                m_IndexBuffer[m_IndexCount++] = (ushort) (currentVertCount + inIndices[i]);
            }

            AllocateVertices(inVertexCount);
            for (int i = 0, end = i + inVertexCount; i < end; i++)
            {
                m_VertexBuffer[m_VertexCount++] = inVertices[i];
            }
            return new OffsetLengthU16((ushort) currentVertCount, (ushort) inVertexCount);
        }

        /// <summary>
        /// Adds arbitrary vertices and indices to the mesh.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public unsafe OffsetLengthU16 AddFromBuffers(TVertex* inVertices, int inVertexCount, ushort* inIndices, int inIndexCount)
        {
            AllocateIndices(inIndexCount);
            int currentVertCount = m_VertexCount;
            for (int i = 0, end = i + inIndexCount; i < end; i++)
            {
                m_IndexBuffer[m_IndexCount++] = (ushort) (currentVertCount + inIndices[i]);
            }

            AllocateVertices(inVertexCount);
            fixed(TVertex* dst = &m_VertexBuffer[m_VertexCount])
            {
                Unsafe.FastCopyArray<TVertex>(inVertices, inVertexCount, dst);
                m_VertexCount += inVertexCount;
            }
            return new OffsetLengthU16((ushort) currentVertCount, (ushort) inVertexCount);
        }

        #endregion // Shapes

        #region Transforms

        /// <summary>
        /// Transforms the given vertex range by the given matrix.
        /// </summary>
        public void Transform(OffsetLengthU16 inRange, Matrix4x4 inMatrix)
        {
            Assert.True(inRange.Offset >= 0 && inRange.End <= m_IndexCount, "Provided vertex range out of range");
            unsafe
            {
                fixed (TVertex* verts = m_VertexBuffer)
                {
                    byte* stream = (byte*) (verts + inRange.Offset);
                    if (s_VertexLayout.Has(VertexAttribute.Position))
                    {
                        Vectors.TransformPoint3Stride(stream, s_VertexLayout.Offset(VertexAttribute.Position), s_VertexLayout.Stride, (int) inRange.Length, ref inMatrix);
                    }
                    if (s_VertexLayout.Has(VertexAttribute.Normal))
                    {
                        Vectors.TransformVector3Stride(stream, s_VertexLayout.Offset(VertexAttribute.Normal), s_VertexLayout.Stride, (int) inRange.Length, ref inMatrix);
                    }
                    if (s_VertexLayout.Has(VertexAttribute.Tangent))
                    {
                        Vectors.TransformVector3Stride(stream, s_VertexLayout.Offset(VertexAttribute.Tangent), s_VertexLayout.Stride, (int) inRange.Length, ref inMatrix);
                    }
                }
            }
        }

        #endregion // Transforms

        #region Upload

        /// <summary>
        /// Uploads this mesh data to the given mesh.
        /// </summary>
        public void Upload(ref MeshDataTarget ioMesh, MeshDataUploadFlags inUploadFlags = 0)
        {
            Mesh m = ioMesh.Mesh;
            if (m == null)
                throw new ArgumentNullException("ioMesh.Mesh");

            bool uploadIndices = (inUploadFlags & MeshDataUploadFlags.SkipIndexBufferUpload) == 0 || ioMesh.IndexFormat != IndexFormat.UInt16
                || m.subMeshCount == 0 || m.GetIndexCount(0) != m_IndexCount;

#if UNSAFE_BUFFER_COPY
            unsafe
            {
                fixed (TVertex* verts = m_VertexBuffer)
                {
                    if (Ref.Replace(ref ioMesh.VertexFormatHash, s_VertexLayout.Hash) || m.vertexCount != m_VertexCount)
                    {
                        m.SetVertexBufferParams(m_VertexCount, s_VertexLayout.Descriptors);
                    }
                    m.SetVertexBufferData(Unsafe.NativeArray(verts, m_VertexCount), 0, 0, m_VertexCount, 0, IgnoreAllUpdates);
                }

                if (uploadIndices)
                {
                    ioMesh.IndexFormat = IndexFormat.UInt16;
                    m.SetIndexBufferParams(m_IndexCount, IndexFormat.UInt16);
                    fixed (ushort* inds = m_IndexBuffer)
                    {
                        m.SetIndexBufferData(Unsafe.NativeArray(inds, m_IndexCount), 0, 0, m_IndexCount, IgnoreAllUpdates);
                    }
                }
            }
#else
            if (ioMesh.VertexFormatHash != s_VertexLayout.Hash)
            {
                ioMesh.VertexFormatHash = s_VertexLayout.Hash;
                m.SetVertexBufferParams(m_VertexCount, s_VertexLayout.Descriptors);
            }
            m.SetVertexBufferData(m_VertexBuffer, 0, 0, m_VertexCount, 0, IgnoreAllUpdates);

            if (uploadIndices)
            {
                ioMesh.IndexFormat = IndexFormat.UInt16;
                m.SetIndexBufferParams(m_IndexCount, IndexFormat.UInt16);
                m.SetIndexBufferData(m_IndexBuffer, 0, 0, m_IndexCount, IgnoreAllUpdates);
            }
#endif // UNSAFE_BUFFER_COPY

            if (uploadIndices || ioMesh.Topology != m_Topology || m.subMeshCount != 1)
            {
                ioMesh.Topology = m_Topology;
                m.subMeshCount = 1;
                m.SetSubMesh(0, new SubMeshDescriptor(0, m_IndexCount, m_Topology), IgnoreAllUpdates);
            }

            if ((inUploadFlags & MeshDataUploadFlags.DontRecalculateBounds) == 0)
            {
                m.RecalculateBounds();
            }
            m.UploadMeshData((inUploadFlags & MeshDataUploadFlags.MarkNoLongerReadable) != 0);
        }

        #endregion // Upload

        #region Vertex Layout

        static private void InitializeVertexLayout()
        {
            if (s_VertexLayout)
                return;

            s_VertexLayout = VertexUtility.GenerateLayout(typeof(TVertex));
        }

        /// <summary>
        /// Overrides the vertex layout for this vertex type.
        /// </summary>
        static public void OverrideLayout(in VertexLayout inLayout)
        {
            if (!inLayout)
                throw new ArgumentNullException("inLayout");

            s_VertexLayout = inLayout;
        }

        #endregion // Vertex Layout
    }

    /// <summary>
    /// Interleaved mesh data with a 32-bit index buffer.
    /// </summary>
    public class MeshData32<TVertex> : IMeshData where TVertex : unmanaged
    {
        private const MeshUpdateFlags IgnoreAllUpdates = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers;
        private const int DefaultInitialCapacity = 32;
        static private unsafe readonly long MaxAllowedVertices = Math.Min(VertexUtility.MaxMeshVertexStreamSize / sizeof(TVertex), int.MaxValue);

        static private VertexLayout s_VertexLayout;

        private int m_VertexCount;
        private int m_IndexCount;
        private uint[] m_IndexBuffer;
        private TVertex[] m_VertexBuffer;
        private MeshTopology m_Topology;
        private bool m_Flexible;

        public MeshData32() : this(DefaultInitialCapacity, MeshTopology.Triangles, true) { }

        public MeshData32(int inInitialCapacity, MeshTopology inTopology = MeshTopology.Triangles, bool inFlexible = true)
            : this(inInitialCapacity, inInitialCapacity, inTopology, inFlexible) { }

        public MeshData32(int inInitialVertexCapacity, int inInitialIndexCapacity, MeshTopology inTopology = MeshTopology.Triangles, bool inFlexible = true)
        {
            InitializeVertexLayout();

            m_VertexBuffer = new TVertex[inInitialVertexCapacity];
            m_IndexBuffer = new uint[inInitialIndexCapacity];
            m_VertexCount = 0;
            m_IndexCount = 0;
            m_Topology = inTopology;
            m_Flexible = inFlexible;
        }

        public void Dispose()
        {
            Clear();
            m_VertexBuffer = null;
            m_IndexBuffer = null;
        }

        /// <summary>
        /// Topology of the mesh.
        /// Defines the faces described by the index buffer.
        /// </summary>
        public MeshTopology Topology
        {
            get { return m_Topology; }
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
        public bool NeedsFlush(int inVertexCount, int inIndexCount)
        {
            return m_VertexCount + inVertexCount > MaxAllowedVertices
                || m_IndexCount + inIndexCount > VertexUtility.MaxMeshIndexStreamSize32;
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
        /// Releases buffers.
        /// </summary>
        public void Release() {
            Clear();
            m_VertexBuffer = Array.Empty<TVertex>();
            m_IndexBuffer = Array.Empty<uint>();
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
            AllocateVertices(inVertexCount);
            AllocateIndices(inIndexCount);
        }

        /// <summary>
        /// Ensures the vertex buffer can allow for the given new vertex entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        private void AllocateVertices(int inVertexCount)
        {
            int required = m_VertexCount + inVertexCount;
            if (required > m_VertexBuffer.Length)
            {
                if (!m_Flexible)
                    throw new InvalidOperationException(string.Format("MeshData32 was marked inflexible, and can only have {0} vertices", m_VertexBuffer.Length));
                if (required > MaxAllowedVertices)
                    throw new InvalidOperationException(string.Format("MeshData32<{0}> can only have up to {1} vertices", typeof(TVertex).Name, MaxAllowedVertices));
                ArrayUtils.EnsureCapacity(ref m_VertexBuffer, Mathf.NextPowerOfTwo(required));
            }
        }

        /// <summary>
        /// Ensures the index buffer can allow for the given new index entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        private void AllocateIndices(int inIndexCount)
        {
            int required = m_IndexCount + inIndexCount;
            if (required > m_IndexBuffer.Length)
            {
                if (!m_Flexible)
                    throw new InvalidOperationException(string.Format("MeshData32 was marked inflexible, and can only have {0} indices", m_IndexBuffer.Length));
                if (required > VertexUtility.MaxMeshIndexStreamSize32)
                    throw new InvalidOperationException(string.Format("MeshData32 can only have up to {0} indices", VertexUtility.MaxMeshIndexStreamSize32));
                ArrayUtils.EnsureCapacity(ref m_IndexBuffer, Mathf.NextPowerOfTwo(required));
            }
        }

        #endregion // Buffers

        #region Vertices

        /// <summary>
        /// Adds a vertex.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public MeshData32<TVertex> AddVertex(in TVertex inA)
        {
            AllocateVertices(1);
            m_VertexBuffer[m_VertexCount++] = inA;
            return this;
        }

        /// <summary>
        /// Adds verticies.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public MeshData32<TVertex> AddVertices(in TVertex inA, in TVertex inB)
        {
            AllocateVertices(2);
            m_VertexBuffer[m_VertexCount++] = inA;
            m_VertexBuffer[m_VertexCount++] = inB;
            return this;
        }

        /// <summary>
        /// Adds verticies.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public MeshData32<TVertex> AddVertices(in TVertex inA, in TVertex inB, in TVertex inC)
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
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public MeshData32<TVertex> AddVertices(in TVertex inA, in TVertex inB, in TVertex inC, in TVertex inD)
        {
            AllocateVertices(4);
            m_VertexBuffer[m_VertexCount++] = inA;
            m_VertexBuffer[m_VertexCount++] = inB;
            m_VertexBuffer[m_VertexCount++] = inC;
            m_VertexBuffer[m_VertexCount++] = inD;
            return this;
        }

        /// <summary>
        /// Returns a reference to a vertex in the vertex buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public ref TVertex Vertex(int inIndex)
        {
            if (inIndex < 0 || inIndex >= m_VertexCount)
                throw new ArgumentOutOfRangeException("inIndex");
            return ref m_VertexBuffer[inIndex];
        }

        #endregion // Vertices

        #region Indices

        /// <summary>
        /// Adds an index.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public MeshData32<TVertex> AddIndex(uint inA)
        {
            AllocateIndices(1);
            m_IndexBuffer[m_IndexCount++] = inA;
            return this;
        }

        /// <summary>
        /// Adds indicies.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public MeshData32<TVertex> AddIndices(uint inA, uint inB)
        {
            AllocateIndices(2);
            m_IndexBuffer[m_IndexCount++] = inA;
            m_IndexBuffer[m_IndexCount++] = inB;
            return this;
        }

        /// <summary>
        /// Adds indicies.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public MeshData32<TVertex> AddIndices(uint inA, uint inB, uint inC)
        {
            AllocateIndices(3);
            m_IndexBuffer[m_IndexCount++] = inA;
            m_IndexBuffer[m_IndexCount++] = inB;
            m_IndexBuffer[m_IndexCount++] = inC;
            return this;
        }

        /// <summary>
        /// Returns a reference to a index in the index buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public ref uint Index(int inIndex)
        {
            if (inIndex < 0 || inIndex >= m_IndexCount)
                throw new ArgumentOutOfRangeException("inIndex");
            return ref m_IndexBuffer[inIndex];
        }

        #endregion // Indices

        #region Shapes

        /// <summary>
        /// Adds a triangle to the mesh.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public OffsetLengthU32 AddTriangle(in TVertex inA, in TVertex inB, in TVertex inC)
        {
            AllocateIndices(3);
            int currentVertCount = m_VertexCount;
            m_IndexBuffer[m_IndexCount++] = (uint) currentVertCount;
            m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + 1);
            m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + 2);

            AllocateVertices(3);
            m_VertexBuffer[m_VertexCount++] = inA;
            m_VertexBuffer[m_VertexCount++] = inB;
            m_VertexBuffer[m_VertexCount++] = inC;
            return new OffsetLengthU32((uint) currentVertCount, 3);
        }

        /// <summary>
        /// Adds a triangle to the mesh.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public OffsetLengthU32 AddTriangle(in Vertex3<TVertex> inVertices)
        {
            AllocateIndices(3);
            int currentVertCount = m_VertexCount;
            m_IndexBuffer[m_IndexCount++] = (uint) currentVertCount;
            m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + 1);
            m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + 2);

            AllocateVertices(3);
            m_VertexBuffer[m_VertexCount++] = inVertices.A;
            m_VertexBuffer[m_VertexCount++] = inVertices.B;
            m_VertexBuffer[m_VertexCount++] = inVertices.C;
            return new OffsetLengthU32((uint) currentVertCount, 3);
        }

        /// <summary>
        /// Adds a quad to the mesh.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public OffsetLengthU32 AddQuad(in TVertex inA, in TVertex inB, in TVertex inC, in TVertex inD)
        {
            AllocateIndices(6);
            int currentVertCount = m_VertexCount;
            m_IndexBuffer[m_IndexCount++] = (uint) currentVertCount;
            m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + 1);
            m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + 2);
            m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + 3);
            m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + 2);
            m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + 1);

            AllocateVertices(4);
            m_VertexBuffer[m_VertexCount++] = inA;
            m_VertexBuffer[m_VertexCount++] = inB;
            m_VertexBuffer[m_VertexCount++] = inC;
            m_VertexBuffer[m_VertexCount++] = inD;
            return new OffsetLengthU32((uint) currentVertCount, 4);
        }

        /// <summary>
        /// Adds a quad to the mesh.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public OffsetLengthU32 AddQuad(in Vertex4<TVertex> inVertices)
        {
            AllocateIndices(6);
            int currentVertCount = m_VertexCount;
            m_IndexBuffer[m_IndexCount++] = (uint) currentVertCount;
            m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + 1);
            m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + 2);
            m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + 3);
            m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + 2);
            m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + 1);

            AllocateVertices(4);
            m_VertexBuffer[m_VertexCount++] = inVertices.A;
            m_VertexBuffer[m_VertexCount++] = inVertices.B;
            m_VertexBuffer[m_VertexCount++] = inVertices.C;
            m_VertexBuffer[m_VertexCount++] = inVertices.D;
            return new OffsetLengthU32((uint) currentVertCount, 4);
        }

        /// <summary>
        /// Adds arbitrary vertices and indices to the mesh.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public OffsetLengthU32 AddFromBuffers(TVertex[] inVertices, int inVertexCount, uint[] inIndices, int inIndexCount)
        {
            if (inVertexCount > inVertices.Length || inIndexCount > inIndices.Length)
                throw new ArgumentOutOfRangeException();

            AllocateIndices(inIndexCount);
            int currentVertCount = m_VertexCount;
            for (int i = 0, end = i + inIndexCount; i < end; i++)
            {
                m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + inIndices[i]);
            }

            AllocateVertices(inVertexCount);
            for (int i = 0, end = i + inVertexCount; i < end; i++)
            {
                m_VertexBuffer[m_VertexCount++] = inVertices[i];
            }
            return new OffsetLengthU32((uint) currentVertCount, (uint) inVertexCount);
        }

        /// <summary>
        /// Adds arbitrary vertices and indices to the mesh.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public OffsetLengthU32 AddFromBuffers(List<TVertex> inVertices, int inVertexCount, List<uint> inIndices, int inIndexCount)
        {
            if (inVertexCount > inVertices.Count || inIndexCount > inIndices.Count)
                throw new ArgumentOutOfRangeException();

            AllocateIndices(inIndexCount);
            int currentVertCount = m_VertexCount;
            for (int i = 0, end = i + inIndexCount; i < end; i++)
            {
                m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + inIndices[i]);
            }

            AllocateVertices(inVertexCount);
            for (int i = 0, end = i + inVertexCount; i < end; i++)
            {
                m_VertexBuffer[m_VertexCount++] = inVertices[i];
            }
            return new OffsetLengthU32((uint) currentVertCount, (uint) inVertexCount);
        }

        /// <summary>
        /// Adds arbitrary vertices and indices to the mesh.
        /// </summary>
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public unsafe OffsetLengthU32 AddFromBuffers(TVertex* inVertices, int inVertexCount, uint* inIndices, int inIndexCount)
        {
            AllocateIndices(inIndexCount);
            int currentVertCount = m_VertexCount;
            for (int i = 0, end = i + inIndexCount; i < end; i++)
            {
                m_IndexBuffer[m_IndexCount++] = (uint) (currentVertCount + inIndices[i]);
            }

            AllocateVertices(inVertexCount);
            fixed (TVertex* dst = &m_VertexBuffer[m_VertexCount])
            {
                Unsafe.FastCopyArray<TVertex>(inVertices, inVertexCount, dst);
                m_VertexCount += inVertexCount;
            }
            return new OffsetLengthU32((uint) currentVertCount, (uint) inVertexCount);
        }

        #endregion // Shapes

        #region Transforms

        /// <summary>
        /// Transforms the given vertex range by the given matrix.
        /// </summary>
        public void Transform(OffsetLengthU32 inRange, Matrix4x4 inMatrix)
        {
            Assert.True(inRange.Offset >= 0 && inRange.End <= m_IndexCount, "Provided vertex range out of range");
            unsafe
            {
                fixed (TVertex* verts = m_VertexBuffer)
                {
                    byte* stream = (byte*) (verts + inRange.Offset);
                    if (s_VertexLayout.Has(VertexAttribute.Position))
                    {
                        Vectors.TransformPoint3Stride(stream, s_VertexLayout.Offset(VertexAttribute.Position), s_VertexLayout.Stride, (int) inRange.Length, ref inMatrix);
                    }
                    if (s_VertexLayout.Has(VertexAttribute.Normal))
                    {
                        Vectors.TransformVector3Stride(stream, s_VertexLayout.Offset(VertexAttribute.Normal), s_VertexLayout.Stride, (int) inRange.Length, ref inMatrix);
                    }
                    if (s_VertexLayout.Has(VertexAttribute.Tangent))
                    {
                        Vectors.TransformVector3Stride(stream, s_VertexLayout.Offset(VertexAttribute.Tangent), s_VertexLayout.Stride, (int) inRange.Length, ref inMatrix);
                    }
                }
            }
        }

        #endregion // Transforms

        #region Upload

        /// <summary>
        /// Uploads this mesh data to the given mesh.
        /// </summary>
        public void Upload(ref MeshDataTarget ioMesh, MeshDataUploadFlags inUploadFlags = 0)
        {
            Mesh m = ioMesh.Mesh;

            if (m == null)
                throw new ArgumentNullException("ioMesh.Mesh");

            bool uploadIndices = (inUploadFlags & MeshDataUploadFlags.SkipIndexBufferUpload) == 0 || ioMesh.IndexFormat != IndexFormat.UInt32
                || m.subMeshCount == 0 || m.GetIndexCount(0) != m_IndexCount;

#if UNSAFE_BUFFER_COPY
            unsafe
            {
                fixed (TVertex* verts = m_VertexBuffer)
                {
                    if (Ref.Replace(ref ioMesh.VertexFormatHash, s_VertexLayout.Hash) || m.vertexCount != m_VertexCount)
                    {
                        m.SetVertexBufferParams(m_VertexCount, s_VertexLayout.Descriptors);
                    }
                    m.SetVertexBufferData(Unsafe.NativeArray(verts, m_VertexCount), 0, 0, m_VertexCount, 0, IgnoreAllUpdates);
                }

                if (uploadIndices)
                {
                    ioMesh.IndexFormat = IndexFormat.UInt32;
                    m.SetIndexBufferParams(m_IndexCount, IndexFormat.UInt32);
                    fixed (uint* inds = m_IndexBuffer)
                    {
                        m.SetIndexBufferData(Unsafe.NativeArray(inds, m_IndexCount), 0, 0, m_IndexCount, IgnoreAllUpdates);
                    }
                }
            }
#else
            if (ioMesh.VertexFormatHash != s_VertexLayout.Hash)
            {
                ioMesh.VertexFormatHash = s_VertexLayout.Hash;
                m.SetVertexBufferParams(m_VertexCount, s_VertexLayout.Descriptors);
            }
            m.SetVertexBufferData(m_VertexBuffer, 0, 0, m_VertexCount, 0, IgnoreAllUpdates);

            if (uploadIndices)
            {
                ioMesh.IndexFormat = IndexFormat.UInt32;
                m.SetIndexBufferParams(m_IndexCount, IndexFormat.UInt32);
                m.SetIndexBufferData(m_IndexBuffer, 0, 0, m_IndexCount, IgnoreAllUpdates);
            }
#endif // UNSAFE_BUFFER_COPY

            if (uploadIndices || ioMesh.Topology != m_Topology || m.subMeshCount != 1)
            {
                ioMesh.Topology = m_Topology;
                m.subMeshCount = 1;
                m.SetSubMesh(0, new SubMeshDescriptor(0, m_IndexCount, m_Topology), IgnoreAllUpdates);
            }

            if ((inUploadFlags & MeshDataUploadFlags.DontRecalculateBounds) == 0)
            {
                m.RecalculateBounds();
            }
            
            m.UploadMeshData((inUploadFlags & MeshDataUploadFlags.MarkNoLongerReadable) != 0);
        }

        #endregion // Upload

        #region Vertex Layout

        static private void InitializeVertexLayout()
        {
            if (s_VertexLayout)
                return;

            s_VertexLayout = VertexUtility.GenerateLayout(typeof(TVertex));
        }

        /// <summary>
        /// Overrides the vertex layout for this vertex type.
        /// </summary>
        static public void OverrideLayout(in VertexLayout inLayout)
        {
            if (!inLayout)
                throw new ArgumentNullException("inLayout");

            s_VertexLayout = inLayout;
        }

        #endregion // Vertex Layout
    }
}