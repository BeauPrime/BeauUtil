/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Oct 2020
 * 
 * File:    SerializedHash32.cs
 * Purpose: Serializable string hash struct.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace BeauUtil
{
    /// <summary>
    /// Serializable hashed string.
    /// </summary>
    [Serializable]
    public struct SerializedHash32
    {
        #region Inspector

        [SerializeField] private string m_Source;
        [SerializeField] private uint m_Hash;

        #endregion // Inspector

        public SerializedHash32(string inSource)
        {
            m_Source = inSource;
            m_Hash = new StringHash32(inSource).HashValue;
        }

        public SerializedHash32(StringHash32 inHash)
        {
            m_Source = null;
            m_Hash = inHash.HashValue;
        }

        public StringHash32 Hash()
        {
            if (m_Source != null && m_Source.Length > 0 && m_Hash == 0)
                m_Hash = new StringHash32(m_Source).HashValue;
            return new StringHash32(m_Hash);
        }

        static public implicit operator SerializedHash32(string inString)
        {
            return new SerializedHash32(inString);
        }

        static public implicit operator SerializedHash32(StringHash32 inHash)
        {
            return new SerializedHash32(inHash);
        }

#if EXPANDED_REFS
        static public implicit operator StringHash32(in SerializedHash32 inSerializedHash)
#else
        static public implicit operator StringHash32(SerializedHash32 inSerializedHash)
#endif // EXPANDED_REFS
        {
            return inSerializedHash.Hash();
        }

        #if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(SerializedHash32))]
        private class Drawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                label = EditorGUI.BeginProperty(position, label, property);
                EditorGUI.BeginChangeCheck();
                var stringProp = property.FindPropertyRelative("m_Source");
                var hashProp = property.FindPropertyRelative("m_Hash");
                EditorGUI.PropertyField(position, stringProp, label);
                if (UnityEditor.EditorGUI.EndChangeCheck()
                    || (!stringProp.hasMultipleDifferentValues && !string.IsNullOrEmpty(stringProp.stringValue) && hashProp.longValue == 0))
                {
                    hashProp.longValue = new StringHash32(stringProp.stringValue).HashValue;
                }
                UnityEditor.EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        #endif // UNITY_EDITOR
    }
}