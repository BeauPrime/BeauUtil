/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 July 2020
 * 
 * File:    TransformOffset.cs
 * Purpose: World and local space offsets for transform position.
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil
{
    /// <summary>
    /// Offsets from transform position.
    /// </summary>
    [Serializable]
    public struct TransformOffset : IEquatable<TransformOffset>
    {
        public Vector3 Local;
        public Vector3 World;

        public TransformOffset(Vector3 inWorld, Vector3 inLocal = default(Vector3))
        {
            World = inWorld;
            Local = inLocal;
        }

        public Vector3 EvaluateWorld(Transform inTransform)
        {
            if (Local == Vector3.zero)
                return inTransform.position + World;
            
            Vector3 localPos = inTransform.localPosition + Local;
            Transform parent = inTransform.parent;
            if (!parent)
                return localPos + World;

            return parent.TransformPoint(localPos) + World;
        }

        public Vector3 EvaluateLocal(Transform inTransform)
        {
            if (World == Vector3.zero)
                return inTransform.localPosition + Local;

            Vector3 worldPos = inTransform.position + World;
            Transform parent = inTransform.parent;
            if (!parent)
                return worldPos + Local;

            return parent.InverseTransformPoint(worldPos) + Local;
        }

        static public TransformOffset ToWorld(Vector3 inWorld)
        {
            return new TransformOffset(inWorld);
        }

        static public TransformOffset ToLocal(Vector3 inLocal)
        {
            return new TransformOffset(default(Vector3), inLocal);
        }

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is TransformOffset)
                return Equals((TransformOffset) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return Local.GetHashCode() << 5 ^ World.GetHashCode();
        }

        public bool Equals(TransformOffset inOffset)
        {
            return Local == inOffset.Local && World == inOffset.World;
        }

        static public bool operator==(TransformOffset inLeft, TransformOffset inRight)
        {
            return inLeft.Equals(inRight);
        }

        static public bool operator!=(TransformOffset inLeft, TransformOffset inRight)
        {
            return !inLeft.Equals(inRight);
        }

        #endregion // Overrides
    }
}