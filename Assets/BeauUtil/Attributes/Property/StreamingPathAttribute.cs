/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    14 May 2021
 * 
 * File:    StreamingPathAttribute.cs
 * Purpose: Marks a string property as a path relative to streaming assets.
 */

using System;
using System.IO;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Marks a string property as a path relative to streaming assets.
    /// </summary>
    public sealed class StreamingPathAttribute : PropertyAttribute
    {
        public string Filter { get; set; }

        public StreamingPathAttribute() { }
        public StreamingPathAttribute(string inFilter)
        {
            Filter = inFilter;
        }

        #if UNITY_EDITOR

        [UnityEditor.CustomPropertyDrawer(typeof(StreamingPathAttribute))]
        private class Drawer : UnityEditor.PropertyDrawer
        {
            private readonly string[] CachedFilters = new string[2];
            private readonly GUIContent CachedContent = new GUIContent();

            private const float ButtonDisplayWidth = 32;

            public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
            {
                StreamingPathAttribute attr = (StreamingPathAttribute) attribute;
                label = UnityEditor.EditorGUI.BeginProperty(position, label, property);

                Rect propRect = position;
                propRect.width -= ButtonDisplayWidth + 4;

                Rect buttonRect = new Rect(propRect.xMax + 4, propRect.y, ButtonDisplayWidth, propRect.height);

                CachedContent.text = string.IsNullOrEmpty(property.stringValue) ? "(No Path Specified)" : property.stringValue;
                CachedContent.tooltip = property.stringValue;
                UnityEditor.EditorGUI.LabelField(propRect, label, CachedContent);

                if (GUI.Button(buttonRect, "..."))
                {
                    string streamingAssetsPath = Path.GetFullPath(Application.streamingAssetsPath).Replace("\\", "/");

                    string nextPath;
                    if (string.IsNullOrEmpty(attr.Filter))
                    {
                        nextPath = UnityEditor.EditorUtility.OpenFilePanel("Select File in StreamingAssets", streamingAssetsPath, "*");
                    }
                    else
                    {
                        CachedFilters[0] = attr.Filter;
                        CachedFilters[1] = attr.Filter;
                        nextPath = UnityEditor.EditorUtility.OpenFilePanelWithFilters("Select File in StreamingAssets", streamingAssetsPath, CachedFilters);
                    }

                    nextPath = nextPath.Replace("\\", "/");
                    
                    if (!string.IsNullOrEmpty(nextPath))
                    {
                        if (!nextPath.StartsWith(streamingAssetsPath))
                        {
                            UnityEditor.EditorUtility.DisplayDialog("Invalid Path", "Path must be in the StreamingAsset directory or a subdirectory", "Okay");
                        }
                        else
                        {
                            string strippedPath = nextPath.Substring(streamingAssetsPath.Length);
                            if (strippedPath.StartsWith("/"))
                                strippedPath = strippedPath.Substring(1);
                            property.stringValue = strippedPath;
                        }
                    }
                }

                UnityEditor.EditorGUI.EndProperty();
            }
        }

        #endif // UNITY_EDITOR
    }
}