/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Sept 2020
 * 
 * File:    EditorValidation.cs
 * Purpose: Basic validation methods.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    static public class EditorValidation
    {
        // [MenuItem("Assets/BeauUtil/Check for StringHash Collisions", false, 1001)]
        // static public void CheckForStringHashCollisions()
        // {
        //     bool bPrevEnabled = StringHash.IsReverseLookupEnabled();
        //     StringHash.EnableReverseLookup(true);
        //     StringHash.ClearReverseLookup();

        //     try
        //     {
        //         EditorUtility.DisplayProgressBar("Searching for all public static StringHash fields...", string.Empty, 0);

        //         foreach(var type in Reflect.FindDerivedTypes(typeof(object), Reflect.FindAllAssemblies(0, Reflect.AssemblyType.DefaultNonUserMask)))
        //         {
        //             foreach(var field in type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
        //             {
        //                 if (field.FieldType == typeof(StringHash))
        //                 {
        //                     field.GetValue(null);
        //                 }
        //             }
        //         }
        //     }
        //     finally
        //     {
        //         StringHash.EnableReverseLookup(bPrevEnabled);
        //         EditorUtility.ClearProgressBar();
        //         Debug.LogFormat("[EditorValidation] StringHash validation completed. Check above logs for any collisions.");
        //     }
        // }
    }
}