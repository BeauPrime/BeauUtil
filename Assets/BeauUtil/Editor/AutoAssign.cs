/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    8 Oct 2019
 * 
 * File:    AutoAssign.cs
 * Purpose: Utility methods for automatically assigning serializable references.
 */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    static public class AutoAssign
    {
        static public void Apply(object inObject, UnityEngine.Object inHost = null)
        {
            if (inObject == null)
                return;

            Type objType = inObject.GetType();

            bool bChanged = false;

            foreach (var field in objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!field.IsPublic && !field.IsDefined(typeof(SerializeField)))
                    continue;

                DefaultAssetAttribute defaultAssetAttr = Reflect.GetAttribute<DefaultAssetAttribute>(field);
                if (defaultAssetAttr != null)
                {
                    Type type = field.FieldType;
                    string name = defaultAssetAttr.AssetName;
                    UnityEngine.Object asset = (UnityEngine.Object) field.GetValue(inObject);
                    if (asset == null)
                    {
                        asset = AssetDBUtils.FindAsset(type, name);
                        if (asset != null)
                        {
                            field.SetValue(inObject, asset);
                            bChanged = true;
                        }
                    }
                }
            }

            if (bChanged && inHost)
            {
                EditorUtility.SetDirty(inHost);
            }
        }
    }
}