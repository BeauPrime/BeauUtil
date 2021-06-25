/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 June 2021
 * 
 * File:    ColliderEditorUtils.cs
 * Purpose: Utility methods for dealing with transforms.
 */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    static public class ColliderEditorUtils
    {
        /// <summary>
        /// Ensures uniform scaling for a given transform,
        /// applying scaling to its colliders and resetting the scale of the transform itself if not uniform.
        /// </summary>
        static public void EnsureUniformScaling(Transform inTransform, bool inbForceIdentity = false)
        {
            Vector3 scale = inTransform.localScale;

            if (inbForceIdentity)
            {
                if (scale.x == 1 && scale.y == 1 && scale.z == 1)
                    return;

                Undo.SetCurrentGroupName("Scale colliders for identity scaled transform");
            }
            else
            {
                if (scale.x == scale.y && scale.y == scale.z)
                    return;

                Undo.SetCurrentGroupName("Scale colliders for uniform scaled transform");
            }

            Collider2D[] collider2ds = inTransform.GetComponents<Collider2D>();
            foreach(var collider2d in collider2ds)
            {
                Undo.RecordObject(collider2d, "Scaling collider");
                PhysicsUtils.ApplyScale(collider2d, scale);
            }

            Collider[] colliders = inTransform.GetComponents<Collider>();
            foreach(var collider in colliders)
            {
                Undo.RecordObject(collider, "Scaling collider");
                PhysicsUtils.ApplyScale(collider, scale);
            }

            Undo.RecordObject(inTransform, "Reset scale");
            inTransform.localScale = Vector3.one;
        }

        [MenuItem("GameObject/Fix Non-Uniform Scaling for Colliders", false, 2050)]
        static private void UniformScale()
        {
            foreach(var gameObject in Selection.gameObjects)
                EnsureUniformScaling(gameObject.transform, false);
        }

        [MenuItem("GameObject/Fix Non-Identity Scaling for Colliders", false, 2050)]
        static private void IdentityScale()
        {
            foreach(var gameObject in Selection.gameObjects)
                EnsureUniformScaling(gameObject.transform, true);
        }
    }
}