/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    14 Nov 2019
 * 
 * File:    GUIScopes.cs
 * Purpose: Additional scopes for GUI.
 */

using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    static public class GUIScopes
    {
        /// <summary>
        /// Sets editor label width.
        /// </summary>
        public class LabelWidthScope : GUI.Scope
        {
            private readonly float m_LastLabelWidth;

            public LabelWidthScope(float inNewWidth)
            {
                m_LastLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = inNewWidth;
            }

            protected override void CloseScope()
            {
                EditorGUIUtility.labelWidth = m_LastLabelWidth;
            }
        }

        /// <summary>
        /// Changes the indent level.
        /// </summary>
        public class IndentLevelScope : GUI.Scope
        {
            private readonly int m_LastIndent;

            public IndentLevelScope(int inIndentChange)
            {
                m_LastIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel += inIndentChange;
            }

            protected override void CloseScope()
            {
                EditorGUI.indentLevel = m_LastIndent;
            }

            static public IndentLevelScope SetIndent(int inNewIndent)
            {
                return new IndentLevelScope(inNewIndent - EditorGUI.indentLevel);
            }
        }

        /// <summary>
        /// Changes the indent level using GUILayout.
        /// </summary>
        public class IndentLayoutScope : GUI.Scope
        {
            public IndentLayoutScope(float inIndentAmount)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(inIndentAmount);
                EditorGUILayout.BeginVertical();
            }

            protected override void CloseScope()
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Changes the GUI color.
        /// </summary>
        public class ColorScope : GUI.Scope
        {
            private readonly Color m_LastColor;

            public ColorScope(Color inColor)
            {
                m_LastColor = GUI.color;
                GUI.color = inColor;
            }

            protected override void CloseScope()
            {
                GUI.color = m_LastColor;
            }
        }
    }
}