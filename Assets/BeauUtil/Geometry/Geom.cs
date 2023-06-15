/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Geom.cs
 * Purpose: Geometry utility methods.
 */

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Geometry utility functions.
    /// </summary>
    static public class Geom
    {
        #region Rectangle

        /// <summary>
        /// Maps a coordinate from circle space to square space.
        /// </summary>
        static public Vector2 MapCircleToSquare(Vector2 inCircleCoords)
        {
            float u = inCircleCoords.x;
            float v = inCircleCoords.y;
            float u2 = u * u;
            float v2 = v * v;
            float twoSqrt2 = 2 * MathUtils.SQRT_2;
            float uSubTerm = 2 + u2 - v2;
            float vSubTerm = 2 - u2 + v2;
            float uTerm1 = uSubTerm + u * twoSqrt2;
            float uTerm2 = uSubTerm - u * twoSqrt2;
            float vTerm1 = vSubTerm + v * twoSqrt2;
            float vTerm2 = vSubTerm - v * twoSqrt2;

            if (Mathf.Abs(uTerm1) < 0.00001f)
                uTerm1 = 0;
            if (Mathf.Abs(vTerm1) < 0.00001f)
                vTerm1 = 0;

            if (Mathf.Abs(uTerm2) < 0.00001f)
                uTerm2 = 0;
            if (Mathf.Abs(vTerm2) < 0.00001f)
                vTerm2 = 0;

            float x = 0.5f * Mathf.Sqrt(uTerm1) - 0.5f * Mathf.Sqrt(uTerm2);
            float y = 0.5f * Mathf.Sqrt(vTerm1) - 0.5f * Mathf.Sqrt(vTerm2);

            return new Vector2(x, y);
        }

        /// <summary>
        /// Returns the minimum bounding box size for a rectangle with the given size,
        /// rotated around its center by the given radians
        /// </summary>
        static public Vector2 MinRotatedSize(Vector2 inOriginalSize, float inRadians)
        {
            float sin = Mathf.Sin(inRadians);
            float cos = Mathf.Cos(inRadians);
            float tx = inOriginalSize.x;
            float ty = inOriginalSize.y;
            return new Vector2(
                Mathf.Abs(tx * cos) + Mathf.Abs(ty * sin),
                Mathf.Abs(tx * sin) + Mathf.Abs(ty * cos)
            );
        }

        /// <summary>
        /// Constrains a rectangle to be contained with another rectangle's edges.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Rect Constrain(Rect inRect, Rect inRegion, RectEdges inEdges = RectEdges.All)
        {
            Constrain(ref inRect, inRegion, inEdges);
            return inRect;
        }

        /// <summary>
        /// Constrains a rectangle to be contained with another rectangle's edges.
        /// </summary>
        static public void Constrain(ref Rect ioRect, Rect inRegion, RectEdges inEdges = RectEdges.All)
        {
            if (inEdges == 0)
                return;

            float leftDist = Mathf.Max(inRegion.xMin - ioRect.xMin, 0);
            float rightDist = Mathf.Max(ioRect.xMax - inRegion.xMax, 0);
            float topDist = Mathf.Max(ioRect.yMax - inRegion.yMax, 0);
            float bottomDist = Mathf.Max(inRegion.yMin - ioRect.yMin, 0);

            if ((inEdges & RectEdges.Left) == 0)
                leftDist = 0;
            if ((inEdges & RectEdges.Right) == 0)
                rightDist = 0;
            if ((inEdges & RectEdges.Top) == 0)
                topDist = 0;
            if ((inEdges & RectEdges.Bottom) == 0)
                bottomDist = 0;

            ioRect.x += leftDist - rightDist;
            ioRect.y += bottomDist - topDist;
        }

        /// <summary>
        /// Constrains a position to be contained with another rectangle's edges.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector2 Constrain(Vector2 inCenter, Vector2 inSize, Rect inRegion, RectEdges inEdges = RectEdges.All)
        {
            Constrain(ref inCenter, inSize, inRegion, inEdges);
            return inCenter;
        }

        /// <summary>
        /// Constrains a position to be contained with another rectangle's edges.
        /// </summary>
        static public void Constrain(ref Vector2 ioCenter, Vector2 inSize, Rect inRegion, RectEdges inEdges = RectEdges.All)
        {
            if (inEdges == 0)
                return;

            float leftDist = Mathf.Max(inRegion.xMin - (ioCenter.x - inSize.x / 2), 0);
            float rightDist = Mathf.Max((ioCenter.x + inSize.x / 2) - inRegion.xMax, 0);
            float topDist = Mathf.Max((ioCenter.y + inSize.y / 2) - inRegion.yMax, 0);
            float bottomDist = Mathf.Max(inRegion.yMin - (ioCenter.y - inSize.y / 2), 0);

            if ((inEdges & RectEdges.Left) == 0)
                leftDist = 0;
            if ((inEdges & RectEdges.Right) == 0)
                rightDist = 0;
            if ((inEdges & RectEdges.Top) == 0)
                topDist = 0;
            if ((inEdges & RectEdges.Bottom) == 0)
                bottomDist = 0;

            ioCenter.x += leftDist - rightDist;
            ioCenter.y += bottomDist - topDist;
        }

        /// <summary>
        /// Expands the given rectangle to encompass the given point.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Rect Encapsulate(Rect inRect, Vector2 inPosition)
        {
            Rect r = inRect;
            Encapsulate(ref r, inPosition);
            return r;
        }

        /// <summary>
        /// Expands the given rectangle to encompass the given point.
        /// </summary>
        static public void Encapsulate(ref Rect ioRect, Vector2 inPosition)
        {
            float dx = inPosition.x - ioRect.x;
            float dy = inPosition.y - ioRect.y;

            if (dx > ioRect.width)
                ioRect.width = dx;
            else if (dx < 0)
                ioRect.xMin += dx;

            if (dy > ioRect.height)
                ioRect.height = dy;
            else if (dy < 0)
                ioRect.yMin += dy;
        }

        /// <summary>
        /// Expands the given rectangle to encompass the given rectangle.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Rect Encapsulate(Rect inRect, Rect inTarget)
        {
            Rect r = inRect;
            Encapsulate(ref r, inTarget);
            return r;
        }

        /// <summary>
        /// Expands the given rectangle to encompass the given rectangle.
        /// </summary>
        static public void Encapsulate(ref Rect ioRect, Rect inTarget)
        {
            float dx = inTarget.x - ioRect.x;
            float dy = inTarget.y - ioRect.y;

            if (dx < 0)
            {
                ioRect.xMin += dx;
                dx = 0;
            }
            if (dy < 0)
            {
                ioRect.yMin += dy;
                dy = 0;
            }

            dx += inTarget.width;
            dy += inTarget.height;

            if (dx > ioRect.width)
                ioRect.width = dx;
            if (dy > ioRect.height)
                ioRect.height = dy;
        }

        /// <summary>
        /// Remaps a vector from one rectangle space into another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector2 Remap(Vector2 inVector, Rect inRange1, Rect inRange2)
        {
            Vector2 v = inVector;
            Remap(ref v, inRange1, inRange2);
            return v;
        }

        /// <summary>
        /// Remaps a vector from one rectangle space into another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Remap(ref Vector2 ioVector, Rect inRange1, Rect inRange2)
        {
            ioVector.x = MathUtils.Remap(ioVector.x, inRange1.xMin, inRange1.xMax, inRange2.xMin, inRange2.xMax);
            ioVector.y = MathUtils.Remap(ioVector.y, inRange1.yMin, inRange1.yMax, inRange2.yMin, inRange2.yMax);
        }

        #endregion // Rectangle

        #region Normals

        /// <summary>
        /// Returns a vector in the given direction, with the given length.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector2 Normalized(float inRadians, float inDistance = 1)
        {
            float x = Mathf.Cos(inRadians) * inDistance;
            float y = Mathf.Sin(inRadians) * inDistance;
            return new Vector2(x, y);
        }

        /// <summary>
        /// Returns a vector in the given direction, with the given length.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector3 Normalized(Quaternion inRotation, float inDistance = 1)
        {
            return (inRotation * Vector3.forward) * inDistance;
        }

        #endregion // Normals

        #region Swizzle

        #region YZ

        /// <summary>
        /// Swizzles the given vector's y and z coordinates.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector3 SwizzleYZ(Vector2 inVector)
        {
            return new Vector3(inVector.x, 0, inVector.y);
        }

        /// <summary>
        /// Swizzles the given vector's y and z coordinates.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector3 SwizzleYZ(Vector3 inVector)
        {
            return new Vector3(inVector.x, inVector.z, inVector.y);
        }

        /// <summary>
        /// Swizzles the given vector's y and z coordinates.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SwizzleYZ(ref Vector3 ioVector)
        {
            float z = ioVector.z;
            ioVector.z = ioVector.y;
            ioVector.y = z;
        }

        #endregion // YZ

        #region XY

        /// <summary>
        /// Swizzles the given vector's x and y coordinates.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector2 SwizzleXY(Vector2 inVector)
        {
            return new Vector2(inVector.y, inVector.x);
        }

        /// <summary>
        /// Swizzles the given vector's x and y coordinates.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector3 SwizzleXY(Vector3 inVector)
        {
            return new Vector3(inVector.y, inVector.x, inVector.z);
        }

        /// <summary>
        /// Swizzles the given vector's x and y coordinates.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SwizzleXY(ref Vector2 ioVector)
        {
            float y = ioVector.y;
            ioVector.y = ioVector.x;
            ioVector.x = y;
        }

        /// <summary>
        /// Swizzles the given vector's x and y coordinates.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SwizzleXY(ref Vector3 ioVector)
        {
            float y = ioVector.y;
            ioVector.y = ioVector.x;
            ioVector.x = y;
        }

        #endregion // XY

        #region XZ

        /// <summary>
        /// Swizzles the given vector's x and z coordinates.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector3 SwizzleXZ(Vector3 inVector)
        {
            return new Vector3(inVector.z, inVector.y, inVector.x);
        }

        /// <summary>
        /// Swizzles the given vector's x and z coordinates.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SwizzleXZ(ref Vector3 ioVector)
        {
            float z = ioVector.z;
            ioVector.z = ioVector.x;
            ioVector.x = z;
        }

        #endregion // XZ

        #endregion // Swizzle

        #region Rotation

        /// <summary>
        /// Returns the given vector, rotated by the given radians.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector2 Rotate(Vector2 inVector, float inRadians)
        {
            float sin = Mathf.Sin(inRadians);
            float cos = Mathf.Cos(inRadians);
            float tx = inVector.x;
            float ty = inVector.y;
            return new Vector2((cos * tx) - (sin * ty), (sin * tx) + (cos * ty));
        }

        /// <summary>
        /// Rotates the given vector by the given radians.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Rotate(ref Vector2 ioVector, float inRadians)
        {
            float sin = Mathf.Sin(inRadians);
            float cos = Mathf.Cos(inRadians);
            float tx = ioVector.x;
            float ty = ioVector.y;
            ioVector.x = (cos * tx) - (sin * ty);
            ioVector.y = (sin * tx) + (cos * ty);
        }

        #endregion // Rotation

        #region Polygon Test

        /// <summary>
        /// Determines if a point is within the given polygon, assuming no holes.
        /// </summary>
        static public bool PointInPolygon(Vector2 inTest, Vector2[] inVertices)
        {
            int i, j;
            bool check = false;

            int numVerts = inVertices.Length;
            float testX = inTest.x;
            float testY = inTest.y;

            float iX, iY, jX, jY;

            for (i = 0, j = numVerts - 1; i < numVerts; j = i++)
            {
                iX = inVertices[i].x;
                iY = inVertices[i].y;
                jX = inVertices[j].x;
                jY = inVertices[j].y;

                if ((iY > testY) != (jY > testY) &&
                    (testX < (jX - iX) * (testY - iY) / (jY - iY) + iX))
                    check = !check;
            }

            return check;
        }

        /// <summary>
        /// Determines if a point is within the given polygon, assuming no holes.
        /// </summary>
        static public bool PointInPolygon(Vector2 inTest, IReadOnlyList<Vector2> inVertices)
        {
            int i, j;
            bool check = false;

            int numVerts = inVertices.Count;
            float testX = inTest.x;
            float testY = inTest.y;

            float iX, iY, jX, jY;

            for (i = 0, j = numVerts - 1; i < numVerts; j = i++)
            {
                iX = inVertices[i].x;
                iY = inVertices[i].y;
                jX = inVertices[j].x;
                jY = inVertices[j].y;

                if ((iY > testY) != (jY > testY) &&
                    (testX < (jX - iX) * (testY - iY) / (jY - iY) + iX))
                    check = !check;
            }

            return check;
        }

        #endregion // Polygon Test
    
        #region Bounds

        /// <summary>
        /// Creates a Rect from the given bounds.
        /// </summary>
        static public Rect BoundsToRect(Bounds inBounds)
        {
            Vector3 min = inBounds.min, max = inBounds.max;
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        /// <summary>
        /// Generates the minimum AABB for the given two points.
        /// </summary>
        static public Bounds AABB(Vector3 inPointA, Vector3 inPointB) {
            Bounds b = new Bounds();
            b.SetMinMax(inPointA, inPointB);
            return b;
        }

        /// <summary>
        /// Generates the minimum AABB for the given two points.
        /// </summary>
        static public Bounds AABB(Vector2 inPointA, Vector2 inPointB) {
            Bounds b = new Bounds();
            b.SetMinMax(inPointA, inPointB);
            return b;
        }

        /// <summary>
        /// Generates the minimum AABB for the given series of points.
        /// </summary>
        static public Bounds MinAABB(Vector3[] inPoints)
        {
            if (inPoints.Length == 0)
                return default(Bounds);

            Vector3 min = inPoints[0], max = min, point;
            for(int i = 1, len = inPoints.Length; i < len; i++)
            {
                point = inPoints[i];
                min.x = Mathf.Min(min.x, point.x);
                min.y = Mathf.Min(min.y, point.y);
                min.z = Mathf.Min(min.z, point.z);
                max.x = Mathf.Max(max.x, point.x);
                max.y = Mathf.Max(max.y, point.y);
                max.z = Mathf.Max(max.z, point.z);
            }

            Bounds b = new Bounds();
            b.SetMinMax(min, max);
            return b;
        }

        /// <summary>
        /// Generates the minimum AABB for the given series of points.
        /// </summary>
        static public Bounds MinAABB(Vector2[] inPoints)
        {
            if (inPoints.Length == 0)
                return default(Bounds);
                
            Vector3 min = inPoints[0], max = min, point;
            for(int i = 1, len = inPoints.Length; i < len; i++)
            {
                point = inPoints[i];
                min.x = Mathf.Min(min.x, point.x);
                min.y = Mathf.Min(min.y, point.y);
                max.x = Mathf.Max(max.x, point.x);
                max.y = Mathf.Max(max.y, point.y);
            }

            Bounds b = new Bounds();
            b.SetMinMax(min, max);
            return b;
        }

        /// <summary>
        /// Generates the minimum AABB for the given series of points.
        /// </summary>
        static public Bounds MinAABB(IReadOnlyList<Vector3> inPoints)
        {
            if (inPoints.Count == 0)
                return default(Bounds);
                
            Vector3 min = inPoints[0], max = min, point;
            for(int i = 1, len = inPoints.Count; i < len; i++)
            {
                point = inPoints[i];
                min.x = Mathf.Min(min.x, point.x);
                min.y = Mathf.Min(min.y, point.y);
                min.z = Mathf.Min(min.z, point.z);
                max.x = Mathf.Max(max.x, point.x);
                max.y = Mathf.Max(max.y, point.y);
                max.z = Mathf.Max(max.z, point.z);
            }

            Bounds b = new Bounds();
            b.SetMinMax(min, max);
            return b;
        }

        /// <summary>
        /// Generates the minimum AABB for the given series of points.
        /// </summary>
        static public Bounds MinAABB(IReadOnlyList<Vector2> inPoints)
        {
            if (inPoints.Count == 0)
                return default(Bounds);
                
            Vector3 min = inPoints[0], max = min, point;
            for(int i = 1, len = inPoints.Count; i < len; i++)
            {
                point = inPoints[i];
                min.x = Mathf.Min(min.x, point.x);
                min.y = Mathf.Min(min.y, point.y);
                max.x = Mathf.Max(max.x, point.x);
                max.y = Mathf.Max(max.y, point.y);
            }

            Bounds b = new Bounds();
            b.SetMinMax(min, max);
            return b;
        }

        /// <summary>
        /// Remaps a vector from one bounds space into another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vector3 Remap(Vector3 inVector, Bounds inRange1, Bounds inRange2)
        {
            Vector3 v = inVector;
            Remap(ref v, inRange1, inRange2);
            return v;
        }

        /// <summary>
        /// Remaps a vector from one bounds space into another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Remap(ref Vector3 ioVector, Bounds inRange1, Bounds inRange2)
        {
            Vector3 min1 = inRange1.min, min2 = inRange2.min,
                    max1 = inRange1.max, max2 = inRange2.max;
            ioVector.x = MathUtils.Remap(ioVector.x, min1.x, max1.x, min2.x, max2.x);
            ioVector.y = MathUtils.Remap(ioVector.y, min1.y, max1.y, min2.y, max2.y);
            ioVector.z = MathUtils.Remap(ioVector.z, min1.z, max1.z, min2.z, max2.z);
        }

        #endregion // Bounds
    }
}