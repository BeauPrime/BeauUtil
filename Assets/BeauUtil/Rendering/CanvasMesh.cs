/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    19 Oct 2022
 * 
 * File:    CanvasMesh.cs
 * Purpose: Canvas rendering utilities.
*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil
{
    /// <summary>
    /// Canvas rendering utilities.
    /// </summary>
    static public class CanvasMesh
    {
        [Flags]
        public enum LineFlags : uint
        {
            StartCap = 0x01,
            EndCap = 0x02,
            Capped = 0x04
        }

        /// <summary>
        /// Estimates desired curve resolution for the given radius.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int EstimateCurveResolution(float inRadius)
        {
            return (int) Math.Min(40, Math.Max(3, (Math.PI * (inRadius + inRadius) / 16)));
        }

        /// <summary>
        /// Estimates desired curve resolution for the given radius and graphic.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int EstimateCurveResolution(float inRadiusX, float inRadiusY)
        {
            return EstimateCurveResolution(Math.Max(inRadiusX, inRadiusY));
        }

        /// <summary>
        /// Estimates desired curve resolution for the given radius and graphic.
        /// </summary>
        static public int EstimateCurveResolution(float inRadius, Graphic inGraphic)
        {
            RectTransform r = inGraphic.rectTransform;
            Vector3 scale = r.lossyScale;
            Canvas c = inGraphic.canvas;
            if (c)
            {
                Vector3 canvasScale = c.transform.lossyScale;
                scale.x /= canvasScale.x;
                scale.y /= canvasScale.y;
            }
            float worldRadius = inRadius * Math.Max(Math.Abs(scale.x), Math.Abs(scale.y));
            return (int) Math.Min(40, Math.Max(3, (Math.PI * (worldRadius + worldRadius) / 16)));
        }

        /// <summary>
        /// Estimates desired curve resolution for the given radius and graphic.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int EstimateCurveResolution(float inRadiusX, float inRadiusY, Graphic inGraphic)
        {
            return EstimateCurveResolution(Math.Max(inRadiusX, inRadiusY), inGraphic);
        }

        #region Quad

        /// <summary>
        /// Renders a quad.
        /// </summary>
        static public void AddQuad(this VertexHelper inVH, Vector3 inA, Vector3 inB, Vector3 inC, Vector3 inD, Color32 inColor, Vector2 inUV)
        {
            int vertCount = inVH.currentVertCount;

            inVH.AddVert(inA, inColor, inUV);
            inVH.AddVert(inB, inColor, inUV);
            inVH.AddVert(inC, inColor, inUV);
            inVH.AddVert(inD, inColor, inUV);

            inVH.AddTriangle(vertCount + 0, vertCount + 1, vertCount + 2);
            inVH.AddTriangle(vertCount + 2, vertCount + 3, vertCount + 0);
        }

        /// <summary>
        /// Renders a quad.
        /// </summary>
        static public void AddQuad(this VertexHelper inVH, Vector3 inA, Vector3 inB, Vector3 inC, Vector3 inD, Color32 inColor, Rect inUVs)
        {
            int vertCount = inVH.currentVertCount;

            inVH.AddVert(inA, inColor, new Vector2(inUVs.xMin, inUVs.yMin));
            inVH.AddVert(inB, inColor, new Vector2(inUVs.xMin, inUVs.yMax));
            inVH.AddVert(inC, inColor, new Vector2(inUVs.xMax, inUVs.yMax));
            inVH.AddVert(inD, inColor, new Vector2(inUVs.xMax, inUVs.yMin));

            inVH.AddTriangle(vertCount + 0, vertCount + 1, vertCount + 2);
            inVH.AddTriangle(vertCount + 2, vertCount + 3, vertCount + 0);
        }

        #endregion // Quad

        #region Rect

        /// <summary>
        /// Renders a rectangle.
        /// </summary>
        static public void AddRect(this VertexHelper inVH, Rect inRect, Color32 inColor, Vector2 inUV)
        {
            Vector4 outer = new Vector4(inRect.x, inRect.y, inRect.x + inRect.width, inRect.y + inRect.height);
            AddQuad(inVH,
                new Vector3(outer.x, outer.y),
                new Vector3(outer.x, outer.w),
                new Vector3(outer.z, outer.w),
                new Vector3(outer.z, outer.y),
            inColor, inUV);
        }

        /// <summary>
        /// Renders a rectangle.
        /// </summary>
        static public void AddRect(this VertexHelper inVH, Rect inRect, Color32 inColor, Rect inUVs)
        {
            Vector4 outer = new Vector4(inRect.x, inRect.y, inRect.x + inRect.width, inRect.y + inRect.height);
            AddQuad(inVH,
                new Vector3(outer.x, outer.y),
                new Vector3(outer.x, outer.w),
                new Vector3(outer.z, outer.w),
                new Vector3(outer.z, outer.y),
            inColor, inUVs);
        }

        /// <summary>
        /// Renders a rectangle outline.
        /// </summary>
        static public void AddRectOutline(this VertexHelper inVH, Rect inRect, float inOutlineWidth, Color32 inColor, Vector2 inUV)
        {
            if (inOutlineWidth <= 0)
                return;

            if (inOutlineWidth >= Math.Min(inRect.width / 2, inRect.height / 2))
            {
                AddRect(inVH, inRect, inColor, inUV);
                return;
            }

            Vector4 outer = new Vector4(inRect.x, inRect.y, inRect.x + inRect.width, inRect.y + inRect.height);
            Vector4 inner = new Vector4(outer.x + inOutlineWidth, outer.y + inOutlineWidth, outer.z - inOutlineWidth, outer.w - inOutlineWidth);

            // bottom
            AddQuad(inVH,
                new Vector3(outer.x, outer.y),
                new Vector3(outer.x, inner.y),
                new Vector3(outer.z, inner.y),
                new Vector3(outer.z, outer.y),
            inColor, inUV);


            // top
            AddQuad(inVH,
                new Vector3(outer.x, inner.w),
                new Vector3(outer.x, outer.w),
                new Vector3(outer.z, outer.w),
                new Vector3(outer.z, inner.w),
            inColor, inUV);

            // left
            AddQuad(inVH,
                new Vector3(outer.x, inner.y),
                new Vector3(outer.x, inner.w),
                new Vector3(inner.x, inner.w),
                new Vector3(inner.x, inner.y),
            inColor, inUV);

            // right
            AddQuad(inVH,
                new Vector3(inner.z, inner.y),
                new Vector3(inner.z, inner.w),
                new Vector3(outer.z, inner.w),
                new Vector3(outer.z, inner.y),
            inColor, inUV);
        }

        #endregion // Rect

        #region Rounded Rect

        /// <summary>
        /// Renders a rounded rectangle.
        /// </summary>
        static public void AddRoundedRect(this VertexHelper inVH, Rect inRect, float inRadius, int inResolution, Color32 inColor, Vector2 inUV)
        {
            if (inResolution <= 0)
                inResolution = EstimateCurveResolution(inRadius);

            if (inRadius <= 0 || inResolution <= 0)
            {
                AddRect(inVH, inRect, inColor, inUV);
                return;
            }

            inRadius = Math.Min(inRadius, Math.Min(inRect.width / 2, inRect.height / 2)); // clamp radius

            Vector4 outer = new Vector4(inRect.x, inRect.y, inRect.x + inRect.width, inRect.y + inRect.height);
            Vector4 inner = new Vector4(outer.x + inRadius, outer.y + inRadius, outer.z - inRadius, outer.w - inRadius);

            bool hasVertSpace = inner.y < inner.w;
            bool hasHorSpace = inner.x < inner.z;

            if (!hasHorSpace && !hasVertSpace)
            {
                AddEllipse(inVH, inRect, 0, 360, inResolution, inColor, inUV);
                return;
            }

            if (hasVertSpace) // has vertical space, fill horizontal first
            {
                AddQuad(inVH, // horizontal cross
                    new Vector3(outer.x, inner.y),
                    new Vector3(outer.x, inner.w),
                    new Vector3(outer.z, inner.w),
                    new Vector3(outer.z, inner.y),
                inColor, inUV);

                if (hasHorSpace)
                {
                    AddQuad(inVH, // bottom fill
                        new Vector3(inner.x, outer.y),
                        new Vector3(inner.x, inner.y),
                        new Vector3(inner.z, inner.y),
                        new Vector3(inner.z, outer.y),
                    inColor, inUV);

                    AddQuad(inVH, // top fill
                        new Vector3(inner.x, inner.w),
                        new Vector3(inner.x, outer.w),
                        new Vector3(inner.z, outer.w),
                        new Vector3(inner.z, inner.w),
                    inColor, inUV);
                }
            }
            else if (hasHorSpace) // has horizontal space, fill vertical first
            {
                AddQuad(inVH, // vertical cross
                    new Vector3(inner.x, outer.y),
                    new Vector3(inner.x, outer.w),
                    new Vector3(inner.z, outer.w),
                    new Vector3(inner.z, outer.y),
                inColor, inUV);

                if (hasVertSpace)
                {
                    AddQuad(inVH, // left fill
                        new Vector3(outer.x, inner.y),
                        new Vector3(outer.x, inner.w),
                        new Vector3(inner.x, inner.w),
                        new Vector3(inner.x, inner.y),
                    inColor, inUV);

                    AddQuad(inVH, // right fill
                        new Vector3(inner.z, inner.y),
                        new Vector3(inner.z, inner.w),
                        new Vector3(outer.z, inner.w),
                        new Vector3(outer.z, inner.y),
                    inColor, inUV);
                }
            }

            unsafe
            {
                int curveOffsetSize = inResolution + 2;
                Vector2* curveOffsets = stackalloc Vector2[curveOffsetSize];
                float increment = (float) (Math.PI / 2f / (inResolution + 1));
                
                for(int i = 0; i < inResolution; i++)
                {
                    curveOffsets[1 + i] = new Vector2(
                        Mathf.Cos(increment * (i + 1)) * inRadius,
                        Mathf.Sin(increment * (i + 1)) * inRadius
                    );
                }
                curveOffsets[0] = new Vector2(inRadius, 0);
                curveOffsets[curveOffsetSize - 1] = new Vector2(0, inRadius);

                int currentVertCount = inVH.currentVertCount;

                AddQuarterEllipse(inVH, new Vector2(inner.x, inner.y), curveOffsets, curveOffsetSize, new Vector2(-1, -1), inColor, inUV);
                AddQuarterEllipse(inVH, new Vector2(inner.x, inner.w), curveOffsets, curveOffsetSize, new Vector2(-1, 1), inColor, inUV);
                AddQuarterEllipse(inVH, new Vector2(inner.z, inner.w), curveOffsets, curveOffsetSize, new Vector2(1, 1), inColor, inUV);
                AddQuarterEllipse(inVH, new Vector2(inner.z, inner.y), curveOffsets, curveOffsetSize, new Vector2(1, -1), inColor, inUV);
            }
        }

        static private unsafe void AddQuarterEllipse(VertexHelper inVH, Vector2 inCorner, Vector2* inOffsets, int inOffsetsLength, Vector2 inOffsetScale, Color32 inColor, Vector2 inUV)
        {
            int vertCount = inVH.currentVertCount;

            inVH.AddVert(inCorner, inColor, inUV);
            for(int i = 0; i < inOffsetsLength; i++)
            {
                inVH.AddVert(inCorner + inOffsets[i] * inOffsetScale, inColor, inUV);
            }

            if (inOffsetScale.x * inOffsetScale.y < 0)
            {
                for(int i = 0; i < inOffsetsLength; i++)
                {
                    inVH.AddTriangle(vertCount, vertCount + i, vertCount + i + 1);
                }
            }
            else
            {
                for(int i = 0; i < inOffsetsLength; i++)
                {
                    inVH.AddTriangle(vertCount + i + 1, vertCount + i, vertCount);
                }
            }
        }

        /// <summary>
        /// Renders a rounded rectangle outline.
        /// </summary>
        static public void AddRoundedRectOutline(this VertexHelper inVH, Rect inRect, float inRadius, int inResolution, float inOutlineWidth, Color32 inColor, Vector2 inUV)
        {
            if (inResolution <= 0)
                inResolution = EstimateCurveResolution(inRadius);

            if (inOutlineWidth <= 0)
                return;

            if (inRadius <= 0 || inResolution <= 0)
            {
                AddRectOutline(inVH, inRect, inOutlineWidth, inColor, inUV);
                return;
            }

            float maxThickness = Math.Min(inRect.width / 2, inRect.height / 2);
            inRadius = Math.Min(inRadius, maxThickness); // clamp radius

            if (inOutlineWidth >= maxThickness)
            {
                AddRoundedRect(inVH, inRect, inRadius, inResolution, inColor, inUV);
                return;
            }

            float thickness = Math.Min(inOutlineWidth, maxThickness);
            float roundedThickness = Math.Min(thickness, inRadius);

            Vector4 outer = new Vector4(inRect.x, inRect.y, inRect.x + inRect.width, inRect.y + inRect.height);
            Vector4 outerOutline = new Vector4(outer.x + roundedThickness, outer.y + roundedThickness, outer.z - roundedThickness, outer.w - roundedThickness);
            Vector4 inner = new Vector4(outer.x + inRadius, outer.y + inRadius, outer.z - inRadius, outer.w - inRadius);

            bool hasVertSpace = inner.y < inner.w;
            bool hasHorSpace = inner.x < inner.z;

            if (!hasHorSpace && !hasVertSpace)
            {
                AddEllipseOutline(inVH, inRect, 0, 360, inResolution, inOutlineWidth, inColor, inUV);
                return;
            }

            if (hasVertSpace)
            {
                AddQuad(inVH, // left
                    new Vector3(outer.x, inner.y),
                    new Vector3(outer.x, inner.w),
                    new Vector3(outerOutline.x, inner.w),
                    new Vector3(outerOutline.x, inner.y),
                inColor, inUV);

                AddQuad(inVH, // right
                    new Vector3(outerOutline.z, inner.y),
                    new Vector3(outerOutline.z, inner.w),
                    new Vector3(outer.z, inner.w),
                    new Vector3(outer.z, inner.y),
                inColor, inUV);
            }
            
            if (hasHorSpace)
            {
                AddQuad(inVH, // bottom
                    new Vector3(inner.x, outer.y),
                    new Vector3(inner.x, outerOutline.y),
                    new Vector3(inner.z, outerOutline.y),
                    new Vector3(inner.z, outer.y),
                inColor, inUV);

                AddQuad(inVH, // top
                    new Vector3(inner.x, outerOutline.w),
                    new Vector3(inner.x, outer.w),
                    new Vector3(inner.z, outer.w),
                    new Vector3(inner.z, outerOutline.w),
                inColor, inUV);
            }

            unsafe
            {
                float innerDist = inRadius - roundedThickness;

                int curveOffsetSize = (inResolution + 2) * 2;
                Vector2* curveOffsets = stackalloc Vector2[curveOffsetSize];
                float increment = (float) (Math.PI / 2f / (inResolution + 1));
                
                for(int i = 0; i < inResolution; i++)
                {
                    curveOffsets[2 + i * 2] = new Vector2(
                        Mathf.Cos(increment * (i + 1)) * innerDist,
                        Mathf.Sin(increment * (i + 1)) * innerDist
                    );
                    curveOffsets[2 + i * 2 + 1] = new Vector2(
                        Mathf.Cos(increment * (i + 1)) * inRadius,
                        Mathf.Sin(increment * (i + 1)) * inRadius
                    );
                }
                curveOffsets[0] = new Vector2(innerDist, 0);
                curveOffsets[1] = new Vector2(inRadius, 0);
                curveOffsets[curveOffsetSize - 2] = new Vector2(0, innerDist);
                curveOffsets[curveOffsetSize - 1] = new Vector2(0, inRadius);

                int currentVertCount = inVH.currentVertCount;

                AddQuarterEllipseOutline(inVH, new Vector2(inner.x, inner.y), curveOffsets, curveOffsetSize, new Vector2(-1, -1), inColor, inUV);
                AddQuarterEllipseOutline(inVH, new Vector2(inner.x, inner.w), curveOffsets, curveOffsetSize, new Vector2(-1, 1), inColor, inUV);
                AddQuarterEllipseOutline(inVH, new Vector2(inner.z, inner.w), curveOffsets, curveOffsetSize, new Vector2(1, 1), inColor, inUV);
                AddQuarterEllipseOutline(inVH, new Vector2(inner.z, inner.y), curveOffsets, curveOffsetSize, new Vector2(1, -1), inColor, inUV);
            }

            float boxThickness = thickness - roundedThickness;
            if (boxThickness > 0)
            {
                Vector4 innerBox = new Vector4(inner.x + boxThickness, inner.y + boxThickness, inner.z - boxThickness, inner.w - boxThickness);

                AddQuad(inVH, // bottom
                    new Vector3(inner.x, inner.y),
                    new Vector3(inner.x, innerBox.y),
                    new Vector3(inner.z, innerBox.y),
                    new Vector3(inner.z, inner.y),
                inColor, inUV);

                AddQuad(inVH, // top
                    new Vector3(inner.x, innerBox.w),
                    new Vector3(inner.x, inner.w),
                    new Vector3(inner.z, inner.w),
                    new Vector3(inner.z, innerBox.w),
                inColor, inUV);

                AddQuad(inVH, // left
                    new Vector3(inner.x, innerBox.y),
                    new Vector3(inner.x, innerBox.w),
                    new Vector3(innerBox.x, innerBox.w),
                    new Vector3(innerBox.x, innerBox.y),
                inColor, inUV);

                AddQuad(inVH, // right
                    new Vector3(innerBox.z, innerBox.y),
                    new Vector3(innerBox.z, innerBox.w),
                    new Vector3(inner.z, innerBox.w),
                    new Vector3(inner.z, innerBox.y),
                inColor, inUV);
            }
        }

        static private unsafe void AddQuarterEllipseOutline(VertexHelper inVH, Vector2 inCorner, Vector2* inOffsets, int inOffsetsLength, Vector2 inOffsetScale, Color32 inColor, Vector2 inUV)
        {
            int vertCount = inVH.currentVertCount;

            for(int i = 0; i < inOffsetsLength; i++)
            {
                inVH.AddVert(inCorner + inOffsets[i] * inOffsetScale, inColor, inUV);
            }

            if (inOffsetScale.x * inOffsetScale.y < 0)
            {
                for(int i = 0; i < inOffsetsLength - 2; i += 2)
                {
                    inVH.AddTriangle(vertCount + i, vertCount + i + 1, vertCount + i + 2);
                    inVH.AddTriangle(vertCount + i + 3, vertCount + i + 2, vertCount + i + 1);
                }
            }
            else
            {
                for(int i = 0; i < inOffsetsLength - 2; i += 2)
                {
                    inVH.AddTriangle(vertCount + i, vertCount + i + 2, vertCount + i + 1);
                    inVH.AddTriangle(vertCount + i + 2, vertCount + i + 3, vertCount + i + 1);
                }
            }
        }

        #endregion // Rounded Rect

        #region Ellipse

        /// <summary>
        /// Renders an ellipse.
        /// </summary>
        static public void AddEllipse(this VertexHelper inVH, Rect inRect, float inStartDeg, float inArcDeg, int inResolution, Color32 inColor, Vector2 inUV)
        {
            if (Mathf.Approximately(inArcDeg, 0))
                return;

            if (inResolution <= 0)
                inResolution = EstimateCurveResolution(inRect.width / 2, inRect.height / 2);

            Vector2 center = inRect.center;
            float radiusX = inRect.width / 2;
            float radiusY = inRect.height / 2;

            int baseVert = inVH.currentVertCount;
            inVH.AddVert(center, inColor, inUV);
            
            int vertCount = inVH.currentVertCount;

            inArcDeg = Mathf.Clamp(inArcDeg, -360, 360);
            int edgePointCount = 2 + (int) Math.Round(inResolution * Math.Abs(inArcDeg) / 90);

            float startRad = inStartDeg * Mathf.Deg2Rad;
            float deltaRad = inArcDeg * Mathf.Deg2Rad;
            float step = deltaRad / edgePointCount;
            float current = startRad;

            for(int i = 0; i <= edgePointCount; i++)
            {
                Vector2 vert;
                vert.x = center.x + Mathf.Cos(current) * radiusX;
                vert.y = center.y + Mathf.Sin(current) * radiusY;
                inVH.AddVert(vert, inColor, inUV);
                current += step;
            }

            if (step < 0)
            {
                for(int i = 0; i < edgePointCount; i++)
                {
                    inVH.AddTriangle(vertCount + i, vertCount + i + 1, baseVert);
                }
            }
            else
            {
                for(int i = 0; i < edgePointCount; i++)
                {
                    inVH.AddTriangle(vertCount + i + 1, vertCount + i, baseVert);
                }
            }
        }

        /// <summary>
        /// Renders an ellipse outline.
        /// </summary>
        static public void AddEllipseOutline(this VertexHelper inVH, Rect inRect, float inStartDeg, float inArcDeg, int inResolution, float inOutlineWidth, Color32 inColor, Vector2 inUV)
        {
            if (inOutlineWidth <= 0)
                return;

            if (Mathf.Approximately(inArcDeg, 0))
                return;

            if (inResolution <= 0)
                inResolution = EstimateCurveResolution(inRect.width / 2, inRect.height / 2);

            inArcDeg = Mathf.Clamp(inArcDeg, -360, 360);

            if (inOutlineWidth >= Math.Min(inRect.width / 2, inRect.height / 2) && Mathf.Approximately(Mathf.Abs(inArcDeg), 360))
            {
                AddEllipse(inVH, inRect, inStartDeg, inArcDeg, inResolution, inColor, inUV);
                return;
            }

            Vector2 center = inRect.center;
            float radiusX = inRect.width / 2;
            float radiusY = inRect.height / 2;

            int vertCount = inVH.currentVertCount;

            int edgePointCount = 2 + (int) Math.Round(inResolution * Math.Abs(inArcDeg) / 90);

            float startRad = inStartDeg * Mathf.Deg2Rad;
            float deltaRad = inArcDeg * Mathf.Deg2Rad;
            float step = deltaRad / edgePointCount;
            float current = startRad;

            float dx, dy;

            for(int i = 0; i <= edgePointCount; i++)
            {
                Vector2 vert;
                dx = Mathf.Cos(current);
                dy = Mathf.Sin(current);
                vert.x = center.x + dx * radiusX;
                vert.y = center.y + dy * radiusY;
                Vector2 inner = vert;
                inner.x -= dx * inOutlineWidth;
                inner.y -= dy * inOutlineWidth;
                inVH.AddVert(inner, inColor, inUV);
                inVH.AddVert(vert, inColor, inUV);
                current += step;
            }

            if (step < 0)
            {
                for(int i = 0; i < edgePointCount; i++)
                {
                    inVH.AddTriangle(vertCount + i * 2 + 1, vertCount + i * 2 + 3, vertCount + i * 2 + 2);
                    inVH.AddTriangle(vertCount + i * 2 + 1, vertCount + i * 2, vertCount + i * 2 + 2);
                }
            }
            else
            {
                for(int i = 0; i < edgePointCount; i++)
                {
                    inVH.AddTriangle(vertCount + i * 2, vertCount + i * 2 + 3, vertCount + i * 2 + 1);
                    inVH.AddTriangle(vertCount + i * 2 + 3, vertCount + i * 2, vertCount + i * 2 + 2);
                }
            }
        }

        #endregion // Ellipse

        #region Line

        /// <summary>
        /// Renders a line.
        /// </summary>
        static public void AddLine(this VertexHelper inVH, Vector2 inA, Vector2 inB, float inLineWidth, Color32 inColor, Vector2 inUV, LineFlags inFlags = 0)
        {
            if (inLineWidth <= 0)
                return;

            float halfWidth = inLineWidth / 2;

            Vector2 offset = new Vector2(inB.x - inA.x, inB.y - inA.y);
            offset.Normalize();

            Vector2 normal = offset;
            normal.Set(-normal.y * halfWidth, normal.x * halfWidth);

            Vector2 capNormal = offset;
            capNormal.Set(normal.x * halfWidth, normal.y * halfWidth);

            if ((inFlags & LineFlags.StartCap) == LineFlags.StartCap)
            {
                inA -= capNormal;
            }

            if ((inFlags & LineFlags.EndCap) == LineFlags.EndCap)
            {
                inB += capNormal;
            }

            int vertCount = inVH.currentVertCount;

            inVH.AddVert(new Vector3(inA.x - normal.x, inA.y - normal.y), inColor, inUV);
            inVH.AddVert(new Vector3(inA.x + normal.x, inA.y + normal.y), inColor, inUV);
            inVH.AddVert(new Vector3(inB.x - normal.x, inB.y - normal.y), inColor, inUV);
            inVH.AddVert(new Vector3(inB.x + normal.x, inB.y + normal.y), inColor, inUV);

            inVH.AddTriangle(vertCount + 0, vertCount + 1, vertCount + 2);
            inVH.AddTriangle(vertCount + 2, vertCount + 3, vertCount + 0);
        }
    
        #endregion // Line
    }
}
