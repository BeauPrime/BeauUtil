/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    28 April 2021
 * 
 * File:    MeshBuilder.cs
 * Purpose: Mesh construction helper.
*/

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeauUtil
{
    public class MeshBuilder : IDisposable
    {
        #region Vertex Types

        public struct IndexedVertex
        {
            public ushort Index;
            public VertexData Attributes;
        }

        public struct VertexData
        {
            public Vector3 Position;
            public Color32 Color;
            public Vector2 UV;

            public VertexData(Vector3 inPosition, Color32 inColor, Vector2 inUV)
            {
                Position = inPosition;
                Color = inColor;
                UV = inUV;
            }

            static public void Transform(ref VertexData ioAttributes, ref Matrix4x4 inMatrix)
            {
                ioAttributes.Position = inMatrix.MultiplyPoint3x4(ioAttributes.Position);
            }
        }

        #endregion // Vertex Types

        #region Shapes

        public struct Triangle
        {
            public ushort VertIndexStart;
            public VertexData A;
            public VertexData B;
            public VertexData C;
        }

        public struct Quad
        {
            public ushort VertIndexStart;
            public VertexData A;
            public VertexData B;
            public VertexData C;
            public VertexData D;

            public void UpdateUVs(Rect inUVRect)
            {
                Vector2 uv = inUVRect.min;
                A.UV = uv;
                
                uv.x = inUVRect.xMax;
                B.UV = uv;
                
                uv.y = inUVRect.yMax;
                D.UV = uv;

                uv.x = inUVRect.xMin;
                C.UV = uv;
            }
        }

        public struct Sprite
        {
            public Quad Quad;
            public TRS Transform;
            public Color32 Blend;
            public Rect Rect;
            public bool FlipX;
            public bool FlipY;
        }

        #endregion // Shapes

        static private readonly Color32 White = Color.white;
        static private readonly Vector2 LowerLeft = new Vector2(0, 0);
        static private readonly Vector2 UpperLeft = new Vector2(0, 1);
        static private readonly Vector2 UpperRight = new Vector2(1, 1);
        static private readonly Vector2 LowerRight = new Vector2(1, 0);
        static private readonly Vector3 Up = Vector3.up;

        private const int DefaultVertexBufferSize = 128;
        private const int DefaultIndexBufferSize = DefaultVertexBufferSize * 2;

        private Mesh m_Mesh;
        private readonly List<Vector3> m_Positions = new List<Vector3>(DefaultVertexBufferSize);
        private readonly List<Color32> m_Colors = new List<Color32>(DefaultVertexBufferSize);
        private readonly List<Vector2> m_UVs = new List<Vector2>(DefaultVertexBufferSize);
        private readonly List<ushort> m_Indices = new List<ushort>(DefaultIndexBufferSize);

        private ushort m_VertexCount;
        private uint m_IndexCount;

        private bool m_Disposed = false;
        private bool m_Dirty = false;

        public MeshBuilder()
        {
            m_Mesh = new Mesh();
            m_Mesh.MarkDynamic();
        }

        public MeshBuilder(bool inbMarkDynamic)
        {
            m_Mesh = new Mesh();
            if (inbMarkDynamic)
                m_Mesh.MarkDynamic();
        }

        public MeshBuilder(Mesh inSource, bool inbMarkDynamic = true)
        {
            m_Mesh = UnityEngine.Mesh.Instantiate(inSource);
            if (inbMarkDynamic)
            {
                m_Mesh.MarkDynamic();
            }

            CopyFrom(m_Mesh);
            m_Dirty = false;
        }

        /// <summary>
        /// Returns the up-to-date mesh representation of the mesh builder.
        /// </summary>
        public Mesh Mesh()
        {
            RecalculateMesh();
            return m_Mesh;
        }

        #region Lifecycle

        ~MeshBuilder()
        {
            Dispose();
        }

        /// <summary>
        /// Disposes of the mesh builder.
        /// </summary>
        public void Dispose()
        {
            if (m_Disposed)
                return;

            m_Positions.Clear();
            m_Colors.Clear();
            m_UVs.Clear();
            m_Indices.Clear();

            m_Dirty = false;
            m_VertexCount = 0;
            m_IndexCount = 0;

            m_Disposed = true;

            if (!m_Mesh.IsReferenceNull())
            {
                m_Mesh.Clear(false);

                #if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEngine.Mesh.DestroyImmediate(m_Mesh);
                else
                    UnityEngine.Mesh.Destroy(m_Mesh);
                #else
                    UnityEngine.Mesh.Destroy(m_Mesh);
                #endif // UNITY_EDITOR
                
                m_Mesh = null;
            }
        }

        private void CheckDisposed()
        {
            if (m_Disposed)
                throw new InvalidOperationException("MeshBuilder has already been disposed");
        }

        #endregion // Lifecycle

        #region Upload

        /// <summary>
        /// Uploads mesh changes to the internal mesh.
        /// </summary>
        public bool RecalculateMesh()
        {
            CheckDisposed();

            if (!m_Dirty)
                return false;

            UpdateMesh(m_Mesh);
            m_Dirty = false;
            return true;
        }

        /// <summary>
        /// Updates the given mesh mesh to match this mesh.
        /// </summary>
        public void UpdateMesh(Mesh ioMesh)
        {
            CheckDisposed();

            if (ioMesh == null)
                throw new ArgumentNullException("ioMesh");

            ioMesh.SetColors(m_Colors);
            ioMesh.SetUVs(0, m_UVs);
            ioMesh.SetVertices(m_Positions);
            ioMesh.SetTriangles(m_Indices, 0, false);

            ioMesh.RecalculateNormals();
            ioMesh.RecalculateTangents();
            ioMesh.RecalculateBounds();
            ioMesh.UploadMeshData(false);
        }

        #endregion // Upload

        #region Operations

        /// <summary>
        /// Clears all vertices and indices.
        /// </summary>
        public void Clear()
        {
            CheckDisposed();

            m_Positions.Clear();
            m_Colors.Clear();
            m_UVs.Clear();
            m_Indices.Clear();
            
            m_Mesh.Clear(true);

            m_Dirty = false;
            m_VertexCount = 0;
            m_IndexCount = 0;
        }

        /// <summary>
        /// Copies all vertices and indices from the given mesh.
        /// </summary>
        public void CopyFrom(Mesh inMesh)
        {
            CheckDisposed();

            if (inMesh == null)
                throw new ArgumentNullException("inMesh");

            inMesh.GetColors(m_Colors);
            inMesh.GetTriangles(m_Indices, 0);
            inMesh.GetVertices(m_Positions);
            inMesh.GetUVs(0, m_UVs);

            m_Dirty = true;
            m_VertexCount = (ushort) inMesh.vertexCount;
            m_IndexCount = m_Mesh.GetIndexCount(0);
        }

        #region Triangles

        /// <summary>
        /// Adds a triangle to the mesh.
        /// </summary>
        public void AddTriangle(Vector3 inA, Vector3 inB, Vector3 inC)
        {
            CheckDisposed();

            m_Positions.Add(inA);
            m_Positions.Add(inB);
            m_Positions.Add(inC);

            m_Colors.Add(White);
            m_Colors.Add(White);
            m_Colors.Add(White);

            m_UVs.Add(default(Vector2));
            m_UVs.Add(default(Vector2));
            m_UVs.Add(default(Vector2));

            m_Indices.Add(m_VertexCount);
            m_Indices.Add((ushort) (m_VertexCount + 1));
            m_Indices.Add((ushort) (m_VertexCount + 2));

            m_VertexCount += 3;
            m_IndexCount += 3;
            m_Dirty = true;
        }

        /// <summary>
        /// Adds a triangle to the mesh.
        /// </summary>
        public void AddTriangle(Vector3 inA, Vector3 inB, Vector3 inC, out Triangle outTriangle)
        {
            CheckDisposed();

            m_Positions.Add(inA);
            m_Positions.Add(inB);
            m_Positions.Add(inC);

            m_Colors.Add(White);
            m_Colors.Add(White);
            m_Colors.Add(White);

            m_UVs.Add(default(Vector2));
            m_UVs.Add(default(Vector2));
            m_UVs.Add(default(Vector2));

            m_Indices.Add(m_VertexCount);
            m_Indices.Add((ushort) (m_VertexCount + 1));
            m_Indices.Add((ushort) (m_VertexCount + 2));

            outTriangle.VertIndexStart = m_VertexCount;
            outTriangle.A = new VertexData(inA, White, default(Vector2));
            outTriangle.B = new VertexData(inB, White, default(Vector2));
            outTriangle.C = new VertexData(inC, White, default(Vector2));

            m_VertexCount += 3;
            m_IndexCount += 3;
            m_Dirty = true;
        }

        /// <summary>
        /// Adds a triangle to the mesh.
        /// </summary>
        public void AddTriangle(Vector3 inA, Vector3 inB, Vector3 inC, Color32 inColor)
        {
            CheckDisposed();

            m_Positions.Add(inA);
            m_Positions.Add(inB);
            m_Positions.Add(inC);

            m_Colors.Add(inColor);
            m_Colors.Add(inColor);
            m_Colors.Add(inColor);

            m_UVs.Add(default(Vector2));
            m_UVs.Add(default(Vector2));
            m_UVs.Add(default(Vector2));

            m_Indices.Add(m_VertexCount);
            m_Indices.Add((ushort) (m_VertexCount + 1));
            m_Indices.Add((ushort) (m_VertexCount + 2));

            m_VertexCount += 3;
            m_IndexCount += 3;
            m_Dirty = true;
        }

        /// <summary>
        /// Adds a triangle to the mesh.
        /// </summary>
        public void AddTriangle(Vector3 inA, Vector3 inB, Vector3 inC, Color32 inColor, out Triangle outTriangle)
        {
            CheckDisposed();

            m_Positions.Add(inA);
            m_Positions.Add(inB);
            m_Positions.Add(inC);

            m_Colors.Add(inColor);
            m_Colors.Add(inColor);
            m_Colors.Add(inColor);

            m_UVs.Add(default(Vector2));
            m_UVs.Add(default(Vector2));
            m_UVs.Add(default(Vector2));

            m_Indices.Add(m_VertexCount);
            m_Indices.Add((ushort) (m_VertexCount + 1));
            m_Indices.Add((ushort) (m_VertexCount + 2));

            outTriangle.VertIndexStart = m_VertexCount;
            outTriangle.A = new VertexData(inA, inColor, default(Vector2));
            outTriangle.B = new VertexData(inB, inColor, default(Vector2));
            outTriangle.C = new VertexData(inC, inColor, default(Vector2));

            m_VertexCount += 3;
            m_IndexCount += 3;
            m_Dirty = true;
        }

        /// <summary>
        /// Adds a triangle to the mesh.
        /// </summary>
        public void AddTriangle(VertexData inA, VertexData inB, VertexData inC)
        {
            CheckDisposed();

            m_Positions.Add(inA.Position);
            m_Positions.Add(inB.Position);
            m_Positions.Add(inC.Position);

            m_Colors.Add(inA.Color);
            m_Colors.Add(inB.Color);
            m_Colors.Add(inC.Color);

            m_UVs.Add(inA.UV);
            m_UVs.Add(inB.UV);
            m_UVs.Add(inC.UV);

            m_Indices.Add(m_VertexCount);
            m_Indices.Add((ushort) (m_VertexCount + 1));
            m_Indices.Add((ushort) (m_VertexCount + 2));

            m_VertexCount += 3;
            m_IndexCount += 3;
            m_Dirty = true;
        }

        /// <summary>
        /// Adds a triangle to the mesh.
        /// </summary>
        public void AddTriangle(VertexData inA, VertexData inB, VertexData inC, out Triangle outTriangle)
        {
            CheckDisposed();

            m_Positions.Add(inA.Position);
            m_Positions.Add(inB.Position);
            m_Positions.Add(inC.Position);

            m_Colors.Add(inA.Color);
            m_Colors.Add(inB.Color);
            m_Colors.Add(inC.Color);

            m_UVs.Add(inA.UV);
            m_UVs.Add(inB.UV);
            m_UVs.Add(inC.UV);

            m_Indices.Add(m_VertexCount);
            m_Indices.Add((ushort) (m_VertexCount + 1));
            m_Indices.Add((ushort) (m_VertexCount + 2));

            outTriangle.VertIndexStart = m_VertexCount;
            outTriangle.A = inA;
            outTriangle.B = inB;
            outTriangle.C = inC;

            m_VertexCount += 3;
            m_IndexCount += 3;
            m_Dirty = true;
        }

        /// <summary>
        /// Updates a triangle instance.
        /// </summary>
#if EXPANDED_REFS
        public void UpdateTriangle(in Triangle inTriangle)
#else
        public void UpdateTriangle(Triangle inTriangle)
#endif // EXPANDED_REFS
        {
            CheckDisposed();

            if (inTriangle.VertIndexStart + 3 > m_VertexCount)
                throw new ArgumentNullException("inTriangle.VertIndexStart");

            UnsafeSetVertex(inTriangle.VertIndexStart, inTriangle.A);
            UnsafeSetVertex((ushort) (inTriangle.VertIndexStart + 1), inTriangle.B);
            UnsafeSetVertex((ushort) (inTriangle.VertIndexStart + 2), inTriangle.C);

            m_Dirty = true;
        }

        #endregion // Triangles

        #region Quads

        /// <summary>
        /// Adds a quad to the mesh.
        /// </summary>
        public void AddQuad(Vector3 inA, Vector3 inB, Vector3 inC, Vector3 inD)
        {
            m_Positions.Add(inA);
            m_Positions.Add(inB);
            m_Positions.Add(inC);
            m_Positions.Add(inD);

            m_Colors.Add(White);
            m_Colors.Add(White);
            m_Colors.Add(White);
            m_Colors.Add(White);

            m_UVs.Add(LowerLeft);
            m_UVs.Add(LowerRight);
            m_UVs.Add(UpperLeft);
            m_UVs.Add(UpperRight);

            m_Indices.Add(m_VertexCount);
            m_Indices.Add((ushort) (m_VertexCount + 1));
            m_Indices.Add((ushort) (m_VertexCount + 2));
            m_Indices.Add((ushort) (m_VertexCount + 3));
            m_Indices.Add((ushort) (m_VertexCount + 2));
            m_Indices.Add((ushort) (m_VertexCount + 1));

            m_VertexCount += 4;
            m_IndexCount += 6;
            m_Dirty = true;
        }
        
        /// <summary>
        /// Adds a quad to the mesh.
        /// </summary>
        public void AddQuad(Vector3 inA, Vector3 inB, Vector3 inC, Vector3 inD, out Quad outQuad)
        {
            m_Positions.Add(inA);
            m_Positions.Add(inB);
            m_Positions.Add(inC);
            m_Positions.Add(inD);

            m_Colors.Add(White);
            m_Colors.Add(White);
            m_Colors.Add(White);
            m_Colors.Add(White);

            m_UVs.Add(LowerLeft);
            m_UVs.Add(LowerRight);
            m_UVs.Add(UpperLeft);
            m_UVs.Add(UpperRight);

            m_Indices.Add(m_VertexCount);
            m_Indices.Add((ushort) (m_VertexCount + 1));
            m_Indices.Add((ushort) (m_VertexCount + 2));
            m_Indices.Add((ushort) (m_VertexCount + 3));
            m_Indices.Add((ushort) (m_VertexCount + 2));
            m_Indices.Add((ushort) (m_VertexCount + 1));

            outQuad.VertIndexStart = m_VertexCount;
            outQuad.A = new VertexData(inA, White, LowerLeft);
            outQuad.B = new VertexData(inB, White, LowerRight);
            outQuad.C = new VertexData(inC, White, UpperLeft);
            outQuad.D = new VertexData(inD, White, UpperRight);

            m_VertexCount += 4;
            m_IndexCount += 6;
            m_Dirty = true;
        }

        /// <summary>
        /// Adds a quad to the mesh.
        /// </summary>
        public void AddQuad(Vector3 inA, Vector3 inB, Vector3 inC, Vector3 inD, Color32 inColor)
        {
            m_Positions.Add(inA);
            m_Positions.Add(inB);
            m_Positions.Add(inC);
            m_Positions.Add(inD);

            m_Colors.Add(inColor);
            m_Colors.Add(inColor);
            m_Colors.Add(inColor);
            m_Colors.Add(inColor);

            m_UVs.Add(LowerLeft);
            m_UVs.Add(LowerRight);
            m_UVs.Add(UpperLeft);
            m_UVs.Add(UpperRight);

            m_Indices.Add(m_VertexCount);
            m_Indices.Add((ushort) (m_VertexCount + 1));
            m_Indices.Add((ushort) (m_VertexCount + 2));
            m_Indices.Add((ushort) (m_VertexCount + 3));
            m_Indices.Add((ushort) (m_VertexCount + 2));
            m_Indices.Add((ushort) (m_VertexCount + 1));

            m_VertexCount += 4;
            m_IndexCount += 6;
            m_Dirty = true;
        }
        
        /// <summary>
        /// Adds a quad to the mesh.
        /// </summary>
        public void AddQuad(Vector3 inA, Vector3 inB, Vector3 inC, Vector3 inD, Color32 inColor, out Quad outQuad)
        {
            m_Positions.Add(inA);
            m_Positions.Add(inB);
            m_Positions.Add(inC);
            m_Positions.Add(inD);

            m_Colors.Add(inColor);
            m_Colors.Add(inColor);
            m_Colors.Add(inColor);
            m_Colors.Add(inColor);

            m_UVs.Add(LowerLeft);
            m_UVs.Add(LowerRight);
            m_UVs.Add(UpperLeft);
            m_UVs.Add(UpperRight);

            m_Indices.Add(m_VertexCount);
            m_Indices.Add((ushort) (m_VertexCount + 1));
            m_Indices.Add((ushort) (m_VertexCount + 2));
            m_Indices.Add((ushort) (m_VertexCount + 3));
            m_Indices.Add((ushort) (m_VertexCount + 2));
            m_Indices.Add((ushort) (m_VertexCount + 1));

            outQuad.VertIndexStart = m_VertexCount;
            outQuad.A = new VertexData(inA, inColor, LowerLeft);
            outQuad.B = new VertexData(inB, inColor, LowerRight);
            outQuad.C = new VertexData(inC, inColor, UpperLeft);
            outQuad.D = new VertexData(inD, inColor, UpperRight);

            m_VertexCount += 4;
            m_IndexCount += 6;
            m_Dirty = true;
        }

        /// <summary>
        /// Adds a quad to the mesh.
        /// </summary>
        public void AddQuad(VertexData inA, VertexData inB, VertexData inC, VertexData inD)
        {
            m_Positions.Add(inA.Position);
            m_Positions.Add(inB.Position);
            m_Positions.Add(inC.Position);
            m_Positions.Add(inD.Position);

            m_Colors.Add(inA.Color);
            m_Colors.Add(inB.Color);
            m_Colors.Add(inC.Color);
            m_Colors.Add(inD.Color);

            m_UVs.Add(inA.UV);
            m_UVs.Add(inB.UV);
            m_UVs.Add(inC.UV);
            m_UVs.Add(inD.UV);

            m_Indices.Add(m_VertexCount);
            m_Indices.Add((ushort) (m_VertexCount + 1));
            m_Indices.Add((ushort) (m_VertexCount + 2));
            m_Indices.Add((ushort) (m_VertexCount + 3));
            m_Indices.Add((ushort) (m_VertexCount + 2));
            m_Indices.Add((ushort) (m_VertexCount + 1));

            m_VertexCount += 4;
            m_IndexCount += 6;
            m_Dirty = true;
        }

        /// <summary>
        /// Adds a quad to the mesh.
        /// </summary>
        public void AddQuad(VertexData inA, VertexData inB, VertexData inC, VertexData inD, out Quad outQuad)
        {
            m_Positions.Add(inA.Position);
            m_Positions.Add(inB.Position);
            m_Positions.Add(inC.Position);
            m_Positions.Add(inD.Position);

            m_Colors.Add(inA.Color);
            m_Colors.Add(inB.Color);
            m_Colors.Add(inC.Color);
            m_Colors.Add(inD.Color);

            m_UVs.Add(inA.UV);
            m_UVs.Add(inB.UV);
            m_UVs.Add(inC.UV);
            m_UVs.Add(inD.UV);

            m_Indices.Add(m_VertexCount);
            m_Indices.Add((ushort) (m_VertexCount + 1));
            m_Indices.Add((ushort) (m_VertexCount + 2));
            m_Indices.Add((ushort) (m_VertexCount + 3));
            m_Indices.Add((ushort) (m_VertexCount + 2));
            m_Indices.Add((ushort) (m_VertexCount + 1));

            outQuad.VertIndexStart = m_VertexCount;
            outQuad.A = inA;
            outQuad.B = inB;
            outQuad.C = inC;
            outQuad.D = inD;

            m_VertexCount += 4;
            m_IndexCount += 6;
            m_Dirty = true;
        }

        /// <summary>
        /// Updates a quad instance.
        /// </summary>
#if EXPANDED_REFS
        public void UpdateQuad(in Quad inQuad)
#else
        public void UpdateQuad(Quad inQuad)
#endif // EXPANDED_REFS
        {
            CheckDisposed();

            if (inQuad.VertIndexStart + 4 > m_VertexCount)
                throw new ArgumentNullException("inQuad.VertIndexStart");

            UnsafeSetVertex(inQuad.VertIndexStart, inQuad.A);
            UnsafeSetVertex((ushort) (inQuad.VertIndexStart + 1), inQuad.B);
            UnsafeSetVertex((ushort) (inQuad.VertIndexStart + 2), inQuad.C);
            UnsafeSetVertex((ushort) (inQuad.VertIndexStart + 3), inQuad.D);

            m_Dirty = true;
        }

        #endregion // Quads

        #region Line

        /// <summary>
        /// Adds a line with the given start, end, and thickness.
        /// </summary>
        public void AddLine(Vector3 inStart, Vector3 inEnd, float inWidth)
        {
            Vector3 delta = (inEnd - inStart);
            delta.Normalize();
            Vector3 crossed = Vector3.Cross(Up, delta);
            crossed *= inWidth;
            AddLine(inStart, inEnd, crossed);
        }

        /// <summary>
        /// Adds a line with the given start, end, and thickness.
        /// </summary>
        public void AddLine(Vector3 inStart, Vector3 inEnd, float inWidth, Color32 inColor)
        {
            Vector3 delta = (inEnd - inStart);
            delta.Normalize();
            Vector3 crossed = Vector3.Cross(Up, delta);
            crossed *= inWidth;
            AddLine(inStart, inEnd, crossed, inColor);
        }

        /// <summary>
        /// Adds a line with the given start, end, and thickness.
        /// </summary>
        public void AddLine(Vector3 inStart, Vector3 inEnd, Vector3 inWidth)
        {
            Vector3 halfWidth = inWidth * 0.5f;
            AddQuad(inStart - halfWidth, inStart + halfWidth, inEnd - halfWidth, inEnd + halfWidth);
        }

        /// <summary>
        /// Adds a line with the given start, end, and thickness.
        /// </summary>
        public void AddLine(Vector3 inStart, Vector3 inEnd, Vector3 inWidth, Color32 inColor)
        {
            Vector3 halfWidth = inWidth * 0.5f;
            AddQuad(inStart - halfWidth, inStart + halfWidth, inEnd - halfWidth, inEnd + halfWidth, inColor);
        }

        #endregion // Line

        #region Sprites

//         /// <summary>
//         /// Adds a transformable quad to the mesh.
//         /// </summary>
// #if EXPANDED_REFS
//         public void AddSprite(Vector2 inSize, in TRS inTransform)
// #else
//         public void AddSprite(Vector2 inSize, TRS inTransform)
// #endif // EXPANDED_REFS
//         {
//             Sprite _;
//             AddSprite(inSize, inTransform, out _);
//         }

//         /// <summary>
//         /// Adds a transformable quad to the mesh.
//         /// </summary>
// #if EXPANDED_REFS
//         public void AddSprite(Vector2 inSize, in TRS inTransform, out Sprite outParticle)
// #else
//         public void AddSprite(Vector2 inSize, TRS inTransform, out Particle outParticle)
// #endif // EXPANDED_REFS
//         {
//             Matrix4x4 matrix = inTransform.Matrix();
//             m_Positions.Add(matrix.MultiplyPoint3x4(inA.Position));
//             m_Positions.Add(matrix.MultiplyPoint3x4(inB.Position));
//             m_Positions.Add(matrix.MultiplyPoint3x4(inC.Position));
//             m_Positions.Add(matrix.MultiplyPoint3x4(inD.Position));

//             m_Colors.Add(inA.Color);
//             m_Colors.Add(inB.Color);
//             m_Colors.Add(inC.Color);
//             m_Colors.Add(inD.Color);

//             m_UVs.Add(inA.UV);
//             m_UVs.Add(inB.UV);
//             m_UVs.Add(inC.UV);
//             m_UVs.Add(inD.UV);

//             m_Indices.Add(m_VertexCount);
//             m_Indices.Add((ushort) (m_VertexCount + 1));
//             m_Indices.Add((ushort) (m_VertexCount + 2));
//             m_Indices.Add((ushort) (m_VertexCount + 3));
//             m_Indices.Add((ushort) (m_VertexCount + 2));
//             m_Indices.Add((ushort) (m_VertexCount + 1));

//             outParticle.Quad.VertIndexStart = m_VertexCount;
//             outParticle.Quad.A = inA;
//             outParticle.Quad.B = inB;
//             outParticle.Quad.C = inC;
//             outParticle.Quad.D = inD;
//             outParticle.Transform = inTransform;

//             m_VertexCount += 4;
//             m_IndexCount += 6;
//             m_Dirty = true;
//         }

//         /// <summary>
//         /// Adds a transformable quad to the mesh.
//         /// </summary>
//         public void AddSprite(VertexData inA, VertexData inB, VertexData inC, VertexData inD, TRS inTransform)
//         {
//             Sprite _;
//             AddSprite(inA, inB, inC, inD, inTransform, out _);
//         }

//         /// <summary>
//         /// Adds a transformable quad to the mesh.
//         /// </summary>
//         public void AddSprite(VertexData inA, VertexData inB, VertexData inC, VertexData inD, TRS inTransform, out Sprite outParticle)
//         {
//             Matrix4x4 matrix = inTransform.Matrix;
//             m_Positions.Add(matrix.MultiplyPoint3x4(inA.Position));
//             m_Positions.Add(matrix.MultiplyPoint3x4(inB.Position));
//             m_Positions.Add(matrix.MultiplyPoint3x4(inC.Position));
//             m_Positions.Add(matrix.MultiplyPoint3x4(inD.Position));

//             m_Colors.Add(inA.Color);
//             m_Colors.Add(inB.Color);
//             m_Colors.Add(inC.Color);
//             m_Colors.Add(inD.Color);

//             m_UVs.Add(inA.UV);
//             m_UVs.Add(inB.UV);
//             m_UVs.Add(inC.UV);
//             m_UVs.Add(inD.UV);

//             m_Indices.Add(m_VertexCount);
//             m_Indices.Add((ushort) (m_VertexCount + 1));
//             m_Indices.Add((ushort) (m_VertexCount + 2));
//             m_Indices.Add((ushort) (m_VertexCount + 3));
//             m_Indices.Add((ushort) (m_VertexCount + 2));
//             m_Indices.Add((ushort) (m_VertexCount + 1));

//             outParticle.Quad.VertIndexStart = m_VertexCount;
//             outParticle.Quad.A = inA;
//             outParticle.Quad.B = inB;
//             outParticle.Quad.C = inC;
//             outParticle.Quad.D = inD;
//             outParticle.Transform = inTransform;
//             outParticle.Blend = White;

//             m_VertexCount += 4;
//             m_IndexCount += 6;
//             m_Dirty = true;
//         }

//         /// <summary>
//         /// Updates a sprite instance.
//         /// </summary>
// #if EXPANDED_REFS
//         public void UpdateSprite(in Sprite inParticle)
// #else
//         public void UpdateSprite(Particle inParticle)
// #endif // EXPANDED_REFS
//         {
//             CheckDisposed();

//             if (inParticle.Quad.VertIndexStart + 4 > m_VertexCount)
//                 throw new ArgumentNullException("inParticle.Quad.VertIndexStart");

//             bool bBlendColor = inParticle.Blend.IsNotWhite();

//             Matrix4x4 matrix = inParticle.Transform.Matrix;

//             VertexData vert = inParticle.Quad.A;
//             VertexData.Transform(ref vert, ref matrix);
//             if (bBlendColor)
//                 vert.Color = Colors.Multiply(vert.Color, inParticle.Blend);
//             UnsafeSetVertex(inParticle.Quad.VertIndexStart, vert);

//             vert = inParticle.Quad.B;
//             VertexData.Transform(ref vert, ref matrix);
//             if (bBlendColor)
//                 vert.Color = Colors.Multiply(vert.Color, inParticle.Blend);
//             UnsafeSetVertex((ushort) (inParticle.Quad.VertIndexStart + 1), vert);

//             vert = inParticle.Quad.C;
//             VertexData.Transform(ref vert, ref matrix);
//             if (bBlendColor)
//                 vert.Color = Colors.Multiply(vert.Color, inParticle.Blend);
//             UnsafeSetVertex((ushort) (inParticle.Quad.VertIndexStart + 2), vert);

//             vert = inParticle.Quad.D;
//             VertexData.Transform(ref vert, ref matrix);
//             if (bBlendColor)
//                 vert.Color = Colors.Multiply(vert.Color, inParticle.Blend);
//             UnsafeSetVertex((ushort) (inParticle.Quad.VertIndexStart + 3), vert);

//             m_Dirty = true;
//         }

        #endregion // Sprites

        #endregion // Operations

        #region Vertex Operations

        /// <summary>
        /// Returns the vertex at the given vertex index.
        /// </summary>
        public IndexedVertex GetVertex(ushort inIndex)
        {
            CheckDisposed();

            if (inIndex >= m_VertexCount)
                throw new ArgumentOutOfRangeException("inIndex");

            return new IndexedVertex()
            {
                Index = inIndex,
                Attributes = new VertexData()
                {
                    Position = m_Positions[inIndex],
                    Color = m_Colors[inIndex],
                    UV = m_UVs[inIndex]
                }
            };
        }

        /// <summary>
        /// Sets the vertex at the given vertex index.
        /// </summary>
#if EXPANDED_REFS
        public void SetVertex(ushort inIndex, in VertexData inAttributes)
#else
        public void SetVertex(ushort inIndex, VertexAttributes inAttributes)
#endif // EXPANDED_REFS
        {
            CheckDisposed();

            if (inIndex >= m_VertexCount)
                throw new ArgumentOutOfRangeException("inIndex");

            UnsafeSetVertex(inIndex, inAttributes);
            m_Dirty = true;
        }

        /// <summary>
        /// Sets the vertex at the given vertex index.
        /// </summary>
#if EXPANDED_REFS
        public void SetVertex(in IndexedVertex inVertex)
#else
        public void SetVertex(IndexedVertex inVertex)
#endif // EXPANDED_REFS
        {
            CheckDisposed();

            ushort index = inVertex.Index;
            if (index >= m_VertexCount)
                throw new ArgumentOutOfRangeException("inVertex.Index");

            UnsafeSetVertex(index, inVertex.Attributes);
            m_Dirty = true;
        }

#if EXPANDED_REFS
        private void UnsafeSetVertex(ushort inIndex, in VertexData inAttributes)
#else
        private void UnsafeSetVertex(ushort inIndex, VertexAttributes inAttributes)
#endif // EXPANDED_REFS
        {
            m_Positions[inIndex] = inAttributes.Position;
            m_Colors[inIndex] = inAttributes.Color;
            m_UVs[inIndex] = inAttributes.UV;
        }

        #endregion // Vertex Operations
    }
}