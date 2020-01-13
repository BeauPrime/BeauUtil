/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Geom.cs
 * Purpose: Geometry utility methods.
 */

using System.Collections.Generic;
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
            float twoSqrt2 = 2 * MathUtil.SQRT_2;
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

        #endregion // Rectangle

        #region Normals

        /// <summary>
        /// Returns a vector in the given direction, with the given length.
        /// </summary>
        static public Vector2 Normalized(float inRadians, float inDistance = 1)
        {
            float x = Mathf.Cos(inRadians) * inDistance;
            float y = Mathf.Sin(inRadians) * inDistance;
            return new Vector2(x, y);
        }

        /// <summary>
        /// Returns a vector in the given direction, with the given length.
        /// </summary>
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
        static public Vector3 SwizzleYZ(Vector2 inVector)
        {
            return new Vector3(inVector.x, 0, inVector.y);
        }

        /// <summary>
        /// Swizzles the given vector's y and z coordinates.
        /// </summary>
        static public Vector3 SwizzleYZ(Vector3 inVector)
        {
            return new Vector3(inVector.x, inVector.z, inVector.y);
        }

        /// <summary>
        /// Swizzles the given vector's y and z coordinates.
        /// </summary>
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
        static public Vector2 SwizzleXY(Vector2 inVector)
        {
            return new Vector2(inVector.y, inVector.x);
        }

        /// <summary>
        /// Swizzles the given vector's x and y coordinates.
        /// </summary>
        static public Vector3 SwizzleXY(Vector3 inVector)
        {
            return new Vector3(inVector.y, inVector.x, inVector.z);
        }

        /// <summary>
        /// Swizzles the given vector's x and y coordinates.
        /// </summary>
        static public void SwizzleXY(ref Vector2 ioVector)
        {
            float y = ioVector.y;
            ioVector.y = ioVector.x;
            ioVector.x = y;
        }

        /// <summary>
        /// Swizzles the given vector's x and y coordinates.
        /// </summary>
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
        static public Vector3 SwizzleXZ(Vector3 inVector)
        {
            return new Vector3(inVector.z, inVector.y, inVector.x);
        }

        /// <summary>
        /// Swizzles the given vector's x and z coordinates.
        /// </summary>
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
        static public bool PointInPolygon(Vector2 inTest, IList<Vector2> inVertices)
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
    }
}