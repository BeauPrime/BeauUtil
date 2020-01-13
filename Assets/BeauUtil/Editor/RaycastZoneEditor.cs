/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    RaycastZoneEditor.cs
 * Purpose: Editor for RaycastZone.
 */

using BeauUtil.UI;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    [CustomEditor(typeof(RaycastZone)), CanEditMultipleObjects]
    public class RaycastZoneEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            SerializedObject obj = new SerializedObject(targets);
            EditorGUILayout.PropertyField(obj.FindProperty("m_RaycastTarget"));
            EditorGUILayout.PropertyField(obj.FindProperty("m_Color"), new GUIContent("Debug Color"));
            obj.ApplyModifiedProperties();
        }
    }
}