/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    11 June 2021
 * 
 * File:    TRS.cs
 * Purpose: Translation-rotation-scale storage.
*/

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD

#if UNITY_2021_3_OR_NEWER
#define FAST_TRANSFORM_LOCAL_SET
#endif // UNITY_2021_3_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Basic 3d transform properties.
    /// </summary>
    [Serializable]
    public struct TRS
    {
        public Vector3 Position;
        public Vector3 Scale;
        public Quaternion Rotation;

        #region Constructors

        public TRS(Transform inTransform)
        {
            if (inTransform == null)
                throw new ArgumentNullException("inTransform");

#if FAST_TRANSFORM_LOCAL_SET
            inTransform.GetLocalPositionAndRotation(out Position, out Rotation);
#else
            Position = inTransform.localPosition;
            Rotation = inTransform.localRotation;
#endif // FAST_TRANSFORM_LOCAL_SET
            Scale = inTransform.localScale;
        }

        public TRS(Transform inTransform, Space inSpace)
        {
            if (inTransform == null)
                throw new ArgumentNullException("inTransform");

            if (inSpace == Space.Self)
            {
#if FAST_TRANSFORM_LOCAL_SET
                inTransform.GetLocalPositionAndRotation(out Position, out Rotation);
#else
                Position = inTransform.localPosition;
                Rotation = inTransform.localRotation;
#endif // FAST_TRANSFORM_LOCAL_SET
                Scale = inTransform.localScale;
            }
            else
            {
                inTransform.GetPositionAndRotation(out Position, out Rotation);
                Scale = inTransform.lossyScale;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TRS(Vector3 inPosition)
        {
            Position = inPosition;
            Scale = Vector3.one;
            Rotation = Quaternion.identity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TRS(Vector3 inPosition, Quaternion inRotation, Vector3 inScale)
        {
            Position = inPosition;
            Scale = inScale;
            Rotation = inRotation;
        }

        #endregion // Constructors

        public Matrix4x4 Matrix { get { return Matrix4x4.TRS(Position, Rotation, Scale); } }
        public Matrix4x4 InverseMatrix { get { return Matrix4x4.Inverse(Matrix4x4.TRS(Position, Rotation, Scale)); } }

        /// <summary>
        /// Gets the matrix that represents this transform, and its inverse. 
        /// </summary>
        public void GetMatrixAndInverse(out Matrix4x4 outMatrix, out Matrix4x4 outInverse)
        {
            outMatrix = Matrix;
            outInverse = Matrix4x4.Inverse(outMatrix.inverse);
        }

        /// <summary>
        /// Multiplies a single point around this matrix.
        /// </summary>
        public Vector3 MultiplyPoint(Vector3 inPoint)
        {
            return Matrix.MultiplyPoint3x4(inPoint);
        }

        /// <summary>
        /// Multiplies a single point into this matrix.
        /// </summary>
        public Vector3 InverseMultiplyPoint(Vector3 inPoint)
        {
            return InverseMatrix.MultiplyPoint3x4(inPoint);
        }

        /// <summary>
        /// Copies this transform to the given Unity transform.
        /// </summary>
        public void CopyTo(Transform inTransform)
        {
#if DEVELOPMENT
            if (inTransform == null)
                throw new ArgumentNullException("inTransform");
#endif // DEVELOPMENT

#if FAST_TRANSFORM_LOCAL_SET
            inTransform.SetLocalPositionAndRotation(Position, Rotation);
#else
            inTransform.localPosition = Position;
            inTransform.localRotation = Rotation;
#endif // FAST_TRANSFORM_LOCAL_SET
            inTransform.localScale = Scale;
        }

        /// <summary>
        /// Copies this transform to the given Unity transform.
        /// </summary>
        public void CopyTo(Transform inTransform, Space inSpace)
        {
#if DEVELOPMENT
            if (inTransform == null)
                throw new ArgumentNullException("inTransform");
#endif // DEVELOPMENT

            if (inSpace == Space.Self)
            {
#if FAST_TRANSFORM_LOCAL_SET
                inTransform.SetLocalPositionAndRotation(Position, Rotation);
#else
                inTransform.localPosition = Position;
                inTransform.localRotation = Rotation;
#endif // FAST_TRANSFORM_LOCAL_SET
                inTransform.localScale = Scale;
            }
            else
            {
                inTransform.SetPositionAndRotation(Position, Rotation);
                inTransform.localScale = InverseMatrix.MultiplyPoint3x4(Scale);
            }
        }

        #region Identity

        static public TRS Identity
        {
            get { return s_Identity; }
        }

        static private readonly TRS s_Identity = new TRS()
        {
            Position = default(Vector3),
            Rotation = Quaternion.identity,
            Scale = Vector3.one
        };

        #endregion // Identity

        #region Create

        static public bool TryCreateFromMatrix(Matrix4x4 inMatrix, out TRS outTRS)
        {
            outTRS.Position = new Vector3(inMatrix.m03, inMatrix.m13, inMatrix.m23);
            outTRS.Scale = inMatrix.lossyScale;
            return TryGetRotation(inMatrix, out outTRS.Rotation) && inMatrix.ValidTRS();
        }

        static public bool TryGetRotation(Matrix4x4 inMatrix, out Quaternion outRotation)
        {
            Vector3 forward = inMatrix.GetColumn(2);
            if (forward.sqrMagnitude == 0)
            {
                outRotation = Quaternion.identity;
                return false;
            }

            Vector3 up = inMatrix.GetColumn(1);
            if (up.sqrMagnitude == 0) {
                outRotation = Quaternion.identity;
                return false;
            }

            outRotation = Quaternion.LookRotation(forward, up);
            return true;
        }

        #endregion // Create
    }
}