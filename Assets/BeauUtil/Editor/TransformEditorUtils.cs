/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 March 2021
 * 
 * File:    TransformEditorUtils.cs
 * Purpose: Utility methods for dealing with transforms.
 */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    static public class TransformEditorUtils
    {
        /// <summary>
        /// Flattens the hierarchy at this transform. Children will become siblings.
        /// </summary>
        static public void FlattenChildren(Transform inTransform, bool inbRecursive = false)
        {
            if (inbRecursive)
            {
                Undo.SetCurrentGroupName("Flatten hierarchy (deep)");

                int placeIdx = inTransform.GetSiblingIndex() + 1;
                FlattenHierarchyRecursive(inTransform, inTransform.parent, ref placeIdx);
                return;
            }

            Undo.SetCurrentGroupName("Flatten hierarchy (shallow)");

            Transform transform = inTransform;
            Transform parent = transform.parent;
            Transform child;
            int childCount = transform.childCount;
            int siblingIdx = transform.GetSiblingIndex() + 1;
            while(childCount-- > 0)
            {
                child = transform.GetChild(0);
                Undo.RecordObject(child, "Flatten child");
                Undo.SetTransformParent(child, parent, "Change child parent");
                child.SetSiblingIndex(siblingIdx++);
            }
        }

        static private void FlattenHierarchyRecursive(Transform inTransform, Transform inParent, ref int ioSiblingIndex)
        {
            Transform transform = inTransform;
            Transform child;
            int childCount = transform.childCount;
            while(childCount-- > 0)
            {
                child = transform.GetChild(0);
                Undo.RecordObject(child, "Flatten child");
                Undo.SetTransformParent(child, inParent, "Change child parent");
                child.SetSiblingIndex(ioSiblingIndex++);
                FlattenHierarchyRecursive(child, inParent, ref ioSiblingIndex);
            }
        }

        [MenuItem("GameObject/Flatten Hierarchy (Shallow) %#Q")]
        static private void FlattenHierarchyNonRecursive()
        {
            foreach(var gameObject in Selection.gameObjects)
                FlattenChildren(gameObject.transform, false);
        }

        [MenuItem("GameObject/Flatten Hierarchy (Deep) %#W")]
        static private void FlattenHierarchyRecursive()
        {
            foreach(var gameObject in Selection.gameObjects)
                FlattenChildren(gameObject.transform, true);
        }
    }
}