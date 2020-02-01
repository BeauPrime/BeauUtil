/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    20 Nov 2019
 * 
 * File:    SerializedObjectUtils.cs
 * Purpose: Utility methods for dealing with serialized objects and properties.
 */

#if UNITY_2018_3_OR_NEWER
#define SUPPORTS_PREFABSTAGEUTILITY
#endif // UNITY_2018_3_OR_NEWER

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if SUPPORTS_PREFABSTAGEUTILITY
using UnityEditor.Experimental.SceneManagement;
#endif // SUPPORTS_PREFABSTAGEUTILITY

namespace BeauUtil.Editor
{
    static public class SerializedObjectUtils
    {
        /// <summary>
        /// Returns the parent property.
        /// </summary>
        static public SerializedProperty FindPropertyParent(this SerializedProperty inProperty)
        {
            StringSlice path = inProperty.propertyPath;
            int lastDot = path.LastIndexOf('.');
            if (lastDot < 0)
                return null;

            StringSlice parentPath = path.Substring(0, lastDot);
            return inProperty.serializedObject.FindProperty(parentPath.ToString());
        }

        /// <summary>
        /// Returns a sibling property.
        /// </summary>
        static public SerializedProperty FindPropertySibling(this SerializedProperty inProperty, string inSiblingPath)
        {
            SerializedProperty parentProp = FindPropertyParent(inProperty);
            if (parentProp == null)
                return inProperty.serializedObject.FindProperty(inSiblingPath);

            return parentProp.FindPropertyRelative(inSiblingPath);
        }

        /// <summary>
        /// Returns if this is editing a prefab or a prefab within the prefab editing stage.
        /// </summary>
        static public bool IsEditingPrefab(this SerializedProperty inProperty)
        {
            #if SUPPORTS_PREFABSTAGEUTILITY
            foreach (UnityEngine.Object target in inProperty.serializedObject.targetObjects)
            {
                GameObject go = target as GameObject;
                if (ReferenceEquals(go, null))
                {
                    Component component = target as Component;
                    if (!ReferenceEquals(component, null))
                        go = component.gameObject;
                }

                if (ReferenceEquals(go, null))
                    return false;

                PrefabStage stage = PrefabStageUtility.GetPrefabStage(go);
                if (stage == null)
                    return false;

                if (inProperty.isInstantiatedPrefab)
                    return false;
            }
            return true;
            #else
            throw new NotImplementedException("IsEditingPrefab not implemented for versions before 2018.3");
            #endif // SUPPORTS_PREFABSTAGEUTILITY
        }

        /// <summary>
        /// Returns if this is editing a prefab instance.
        /// </summary>
        static public bool IsEditingPrefabInstance(this SerializedProperty inProperty)
        {
            return inProperty.isInstantiatedPrefab;
        }
    }
}