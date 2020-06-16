/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    ColorGroup.Editor.cs
 * Purpose: Inspector for the ColorGroup component
 */

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace BeauUtil
{
    public sealed partial class ColorGroup
    {
        [CustomEditor(typeof(ColorGroup)), CanEditMultipleObjects]
        private class Editor : UnityEditor.Editor
        {
            private SerializedProperty m_VisibilityProperty;
            private SerializedProperty m_ColorBlockProperty;
            private SerializedProperty m_MainColorProperty;
            private SerializedProperty m_BlocksRaycastsProperty;
            private SerializedProperty m_IgnoreParentGroupsProperty;
            private SerializedProperty m_UseAlphaForVisibilityProperty;

            private SerializedProperty m_MaterialConfigProperty;
            private SerializedProperty m_EnableRaycastLayerProperty;
            private SerializedProperty m_DisableRaycastLayerProperty;

            static private readonly GUIContent s_ColorBlockLabel = new GUIContent("Colors (Expanded)");
            static private readonly GUIContent s_UseAlphaLabel = new GUIContent("Use Main Alpha for Visibility", "If set, the renderer will be disabled if the alpha on the Main channel is 0");

            public void OnEnable()
            {
                m_VisibilityProperty = serializedObject.FindProperty("m_Visible");
                m_ColorBlockProperty = serializedObject.FindProperty("m_Colors");
                m_MainColorProperty = m_ColorBlockProperty.FindPropertyRelative("Main");
                m_BlocksRaycastsProperty = serializedObject.FindProperty("m_BlocksRaycasts");
                m_IgnoreParentGroupsProperty = serializedObject.FindProperty("m_IgnoreParentGroups");
                m_UseAlphaForVisibilityProperty = serializedObject.FindProperty("m_UseMainAlphaForVisibility");

                m_MaterialConfigProperty = serializedObject.FindProperty("m_MaterialConfig");
                m_EnableRaycastLayerProperty = serializedObject.FindProperty("m_EnableRaycastLayer");
                m_DisableRaycastLayerProperty = serializedObject.FindProperty("m_DisableRaycastLayer");
            }

            public override void OnInspectorGUI()
            {
                serializedObject.UpdateIfRequiredOrScript();

                bool bShouldPresentRendererSettings = true;
                foreach (var obj in targets)
                {
                    ColorGroup rColor = (ColorGroup) obj;
                    if (!rColor.m_Renderer)
                    {
                        bShouldPresentRendererSettings = false;
                        break;
                    }
                }

                EditorGUILayout.PropertyField(m_IgnoreParentGroupsProperty);
                EditorGUILayout.PropertyField(m_VisibilityProperty);
                EditorGUILayout.PropertyField(m_BlocksRaycastsProperty);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(m_MainColorProperty);
                EditorGUILayout.PropertyField(m_ColorBlockProperty, s_ColorBlockLabel, true);
                EditorGUILayout.PropertyField(m_UseAlphaForVisibilityProperty, s_UseAlphaLabel);

                if (bShouldPresentRendererSettings)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Renderer Settings", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(m_MaterialConfigProperty, true);
                    EditorGUILayout.PropertyField(m_EnableRaycastLayerProperty);
                    EditorGUILayout.PropertyField(m_DisableRaycastLayerProperty);
                }

                if (serializedObject.hasModifiedProperties)
                    serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

#endif // UNITY_EDITOR