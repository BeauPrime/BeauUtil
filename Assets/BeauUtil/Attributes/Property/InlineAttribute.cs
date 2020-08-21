/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    InlineAttribute.cs
 * Purpose: Renders child properties inline with the parent.
 */

using System;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Displays child properties inline with the parent.
    /// </summary>
    public sealed class InlineAttribute : PropertyAttribute
    {
        public enum DisplayType
        {
            NoLabel,
            RegularLabel,
            HeaderLabel
        }

        private DisplayType m_DisplayType;

        public InlineAttribute() : this(DisplayType.NoLabel) { }
        
        public InlineAttribute(DisplayType inLabelDisplay)
        {
            m_DisplayType = inLabelDisplay;
        }

        #if UNITY_EDITOR

        [UnityEditor.CustomPropertyDrawer(typeof(InlineAttribute))]
        private class Drawer : UnityEditor.PropertyDrawer
        {
            public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
            {
                label = UnityEditor.EditorGUI.BeginProperty(position, label, property);

                if (!property.hasVisibleChildren)
                {
                    UnityEditor.EditorGUI.PropertyField(position, property, label);
                }
                else
                {
                    InlineAttribute attr = (InlineAttribute) attribute;
                    Rect currentRect = UnityEditor.EditorGUI.IndentedRect(position);
                    currentRect.height = UnityEditor.EditorGUIUtility.singleLineHeight;
                    if (attr.m_DisplayType == DisplayType.RegularLabel)
                    {
                        UnityEditor.EditorGUI.LabelField(currentRect, label);
                        currentRect.y += currentRect.height + 2;
                    }
                    else if (attr.m_DisplayType == DisplayType.HeaderLabel)
                    {
                        currentRect.y += 8;
                        UnityEditor.EditorGUI.LabelField(currentRect, label, UnityEditor.EditorStyles.boldLabel);
                        currentRect.y += currentRect.height + 2;
                    }

                    UnityEditor.SerializedProperty endProperty = property.GetEndProperty();
                    bool bEnterChildren = true;
                    while (property.NextVisible(bEnterChildren) && !UnityEditor.SerializedProperty.EqualContents(property, endProperty))
                    {
                        currentRect.height = UnityEditor.EditorGUI.GetPropertyHeight(property, true);
                        UnityEditor.EditorGUI.PropertyField(currentRect, property, true);
                        currentRect.y += currentRect.height + 2;
                        bEnterChildren = false;
                    }
                }

                UnityEditor.EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
            {
                InlineAttribute attr = (InlineAttribute) attribute;

                float height = CalculateHeight(property);
                if (!property.hasVisibleChildren || attr.m_DisplayType == DisplayType.RegularLabel)
                    return height;

                if (attr.m_DisplayType == DisplayType.HeaderLabel)
                    return height + 8;

                return height - UnityEditor.EditorGUIUtility.singleLineHeight - 2;
            }

            static private float CalculateHeight(UnityEditor.SerializedProperty property)
            {
                float height = UnityEditor.EditorGUIUtility.singleLineHeight;
                property = property.Copy();
                UnityEditor.SerializedProperty endProperty = property.GetEndProperty();

                bool bHasChildren = property.hasVisibleChildren;
                if (!bHasChildren)
                    return height;

                height += 2;

                bool bEnterChildren = true;
                while (property.NextVisible(bEnterChildren) && !UnityEditor.SerializedProperty.EqualContents(property, endProperty))
                {
                    height += UnityEditor.EditorGUI.GetPropertyHeight(property, true) + 2;
                    bEnterChildren = false;
                }

                return height;
            }
        }

        #endif // UNITY_EDITOR
    }
}