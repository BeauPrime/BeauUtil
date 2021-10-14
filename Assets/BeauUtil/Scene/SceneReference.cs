/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    27 July 2021
 * 
 * File:    SceneReference.cs
 * Purpose: Scene reference object.
 */

using System;
using UnityEngine;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace BeauUtil
{
    /// <summary>
    /// Scene reference object.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{Path}")]
    public struct SceneReference
#if UNITY_EDITOR
        : ISerializationCallbackReceiver
#endif // UNITY_EDITOR
    {
        [SerializeField] private string m_ScenePath;
        #if UNITY_EDITOR
        [SerializeField] private string m_GUID;
        #endif // UNITY_EDITOR

        public string Name
        {
            get { return System.IO.Path.GetFileNameWithoutExtension(m_ScenePath); }
        }

        public string Path
        {
            get { return m_ScenePath; }
        }

        public bool IsValid
        {
            get { return !string.IsNullOrEmpty(m_ScenePath); }
        }

        #if UNITY_EDITOR

        public void OnAfterDeserialize()
        {
        }

        public void OnBeforeSerialize()
        {
            if (!string.IsNullOrEmpty(m_GUID)) {
                string path = AssetDatabase.GUIDToAssetPath(m_GUID);
                if (m_ScenePath != path) {
                    m_ScenePath = path;
                }
            }
        }

        [CustomPropertyDrawer(typeof(SceneReference)), CanEditMultipleObjects]
        private class Inspector : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                SerializedProperty guidProp = property.FindPropertyRelative("m_GUID");
                SerializedProperty pathProp = property.FindPropertyRelative("m_ScenePath");

                SceneAsset asset = null;
                if (!property.hasMultipleDifferentValues)
                {
                    string guid = guidProp.stringValue;
                    if (!string.IsNullOrEmpty(guid))
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (!string.IsNullOrEmpty(path))
                        {
                            asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                            if (pathProp.stringValue != path)
                            {
                                pathProp.stringValue = path;
                            }
                        }
                    }
                }

                label = EditorGUI.BeginProperty(position, label, property);
                {
                    EditorGUI.BeginChangeCheck();
                    SceneAsset nextAsset = (SceneAsset) EditorGUI.ObjectField(position, label, asset, typeof(SceneAsset), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (nextAsset == null)
                        {
                            guidProp.stringValue = string.Empty;
                            pathProp.stringValue = string.Empty;
                        }
                        else
                        {
                            string path = AssetDatabase.GetAssetPath(nextAsset);
                            guidProp.stringValue = AssetDatabase.AssetPathToGUID(path);
                            pathProp.stringValue = path;
                        }
                    }
                }
                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return UnityEditor.EditorGUIUtility.singleLineHeight;
            }
        }

        #endif // UNITY_EDITOR
    }
}