/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    RaycastZoneEditor.cs
 * Purpose: Editor for RaycastZone.
 */

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
