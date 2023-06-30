/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 June 2023
 * 
 * File:    Vectors.cs
 * Purpose: Vectored math operations.
 */

using System.Runtime.CompilerServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Batched math operations.
    /// </summary>
    static public class Vectors
    {
        // TODO: Do this with SIMD on supported platforms

        #region Transform

        /// <summary>
        /// Vector3 point transformation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void TransformPoint3(Vector3[] inPoints, OffsetLength32 inRange, ref Matrix4x4 inMatrix)
        {
            unsafe
            {
                fixed(Vector3* ptr = inPoints)
                {
                    TransformPoint3(ptr + inRange.Offset, inRange.Length, ref inMatrix);
                }
            }
        }

        /// <summary>
        /// Vector3 point transformation.
        /// </summary>
        static public unsafe void TransformPoint3(Vector3* ioVectors, int inCount, ref Matrix4x4 inMatrix)
        {
            Vector3* ptr = ioVectors;
            int count = inCount;
            while (count-- > 0)
            {
                *ptr = inMatrix.MultiplyPoint3x4(*ptr);
                ptr++;
            }
        }

        /// <summary>
        /// Vector3 point transformation, where points are embedded in the larger buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void TransformPoint3Stride<T>(T[] inPoints, OffsetLength32 inRange, int inFieldOffset, ref Matrix4x4 inMatrix)
            where T : unmanaged
        {
            unsafe
            {
                fixed (T* ptr = inPoints)
                {
                    byte* bytePtr = (byte*) (ptr + inRange.Offset);
                    TransformPoint3Stride(bytePtr, inFieldOffset, sizeof(T), inRange.Length, ref inMatrix);
                }
            }
        }

        /// <summary>
        /// Vector3 point transformation, where points are embeddeded in a larger buffer.
        /// </summary>
        static public unsafe void TransformPoint3Stride(byte* ioVectors, int inOffset, int inStride, int inCount, ref Matrix4x4 inMatrix)
        {
            byte* ptr = ioVectors + inOffset;
            int count = inCount;
            while (count-- > 0)
            {
                *(Vector3*) ptr = inMatrix.MultiplyPoint3x4(*(Vector3*) ptr);
                ptr += inStride;
            }
        }

        /// <summary>
        /// Vector3 vector transformation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void TransformVector3(Vector3[] inVectors, OffsetLength32 inRange, ref Matrix4x4 inMatrix)
        {
            unsafe
            {
                fixed (Vector3* ptr = inVectors)
                {
                    TransformVector3(ptr + inRange.Offset, inRange.Length, ref inMatrix);
                }
            }
        }

        /// <summary>
        /// Vector3 vector transformation.
        /// </summary>
        static public unsafe void TransformVector3(Vector3* ioVectors, int inCount, ref Matrix4x4 inMatrix)
        {
            Vector3* ptr = ioVectors;
            int count = inCount;
            while (count-- > 0)
            {
                *ptr = inMatrix.MultiplyPoint3x4(*ptr);
                ptr++;
            }
        }

        /// <summary>
        /// Vector3 vector transformation, where vectors are embedded in the larger buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void TransformVector3Stride<T>(T[] inVectors, OffsetLength32 inRange, int inFieldOffset, ref Matrix4x4 inMatrix)
            where T : unmanaged
        {
            unsafe
            {
                fixed (T* ptr = inVectors)
                {
                    byte* bytePtr = (byte*) (ptr + inRange.Offset);
                    TransformVector3Stride(bytePtr, inFieldOffset, sizeof(T), inRange.Length, ref inMatrix);
                }
            }
        }

        /// <summary>
        /// Vector3 vector transformation, where vectors are embeddeded in a larger buffer.
        /// </summary>
        static public unsafe void TransformVector3Stride(byte* ioVectors, int inOffset, int inStride, int inCount, ref Matrix4x4 inMatrix)
        {
            byte* ptr = ioVectors + inOffset;
            int count = inCount;
            while (count-- > 0)
            {
                *(Vector3*) ptr = inMatrix.MultiplyVector(*(Vector3*) ptr);
                ptr += inStride;
            }
        }

        #endregion // Transform
    }
}