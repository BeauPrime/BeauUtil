/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    11 June 2021
 * 
 * File:    TRS.cs
 * Purpose: Translation-rotation-scale storage.
*/

using System;
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

            Position = inTransform.localPosition;
            Scale = inTransform.localScale;
            Rotation = inTransform.localRotation;
        }

        public TRS(Transform inTransform, Space inSpace)
        {
            if (inTransform == null)
                throw new ArgumentNullException("inTransform");

            if (inSpace == Space.Self)
            {
                Position = inTransform.localPosition;
                Scale = inTransform.localScale;
                Rotation = inTransform.localRotation;
            }
            else
            {
                Position = inTransform.position;
                Scale = inTransform.lossyScale;
                Rotation = inTransform.rotation;
            }
        }

        public TRS(Vector3 inPosition)
        {
            Position = inPosition;
            Scale = s_Identity.Scale;
            Rotation = Quaternion.identity;
        }

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
            if (inTransform == null)
                throw new ArgumentNullException("inTransform");

            inTransform.localPosition = Position;
            inTransform.localRotation = Rotation;
            inTransform.localScale = Scale;
        }

        /// <summary>
        /// Copies this transform to the given Unity transform.
        /// </summary>
        public void CopyTo(Transform inTransform, Space inSpace)
        {
            if (inTransform == null)
                throw new ArgumentNullException("inTransform");

            if (inSpace == Space.Self)
            {
                inTransform.localPosition = Position;
                inTransform.localRotation = Rotation;
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
    }
}