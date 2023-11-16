/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Oct 2020
 * 
 * File:    SerializedHash32.cs
 * Purpose: Serializable string hash struct.
 */

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEVELOPMENT
#define PRESERVE_DEBUG_SYMBOLS
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace BeauUtil
{
    /// <summary>
    /// Serializable hashed string.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{ToDebugString()}")]
    [DefaultEqualityComparer(typeof(SerializedHash32.Comparer)), DefaultSorter(typeof(SerializedHash32.Comparer))]
    public struct SerializedHash32 : IEquatable<SerializedHash32>, IDebugString
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif // UNITY_EDITOR
    {
        #region Inspector

        [SerializeField] private string m_Source;
        [SerializeField, FormerlySerializedAs("m_Hash")] private uint m_HashValue;

        #endregion // Inspector

        public SerializedHash32(string inSource)
        {
            m_Source = inSource;
            m_HashValue = new StringHash32(inSource).HashValue;
        }

        public SerializedHash32(StringHash32 inHash)
        {
            m_Source = null;
            m_HashValue = inHash.HashValue;
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_HashValue == 0; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Source()
        {
            return m_Source;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringHash32 Hash()
        {
            return new StringHash32(m_HashValue);
        }

        public string ToDebugString()
        {
            return Hash().ToDebugString();
        }

        public override string ToString()
        {
            return Hash().ToString();
        }

        public bool Equals(SerializedHash32 other)
        {
            return m_HashValue == other.m_HashValue;
        }

        public override bool Equals(object obj)
        {
            if (obj is SerializedHash32)
                return Equals((SerializedHash32) obj);
            if (obj is StringHash32)
                return m_HashValue == ((StringHash32) obj).HashValue;

            return false;
        }

        public override int GetHashCode()
        {
            return m_HashValue.GetHashCode();
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

        public void OnBeforeSerialize()
		{
            if (!string.IsNullOrEmpty(m_Source))
            {
                uint hash = new StringHash32(m_Source).HashValue;
                if (m_HashValue != hash)
                {
                    UnityEngine.Debug.LogWarningFormat("[SerializedHash32] Inconsistent hash for '{0}': expected {1}, had {2}", m_Source, hash, m_HashValue);
                    m_HashValue = hash;
                }
            }
		}

        public void OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(m_Source))
            {
                uint hash = new StringHash32(m_Source).HashValue;
                if (m_HashValue != hash)
                {
                    UnityEngine.Debug.LogWarningFormat("[SerializedHash32] Inconsistent hash for '{0}': expected {1}, had {2}", m_Source, hash, m_HashValue);
                    m_HashValue = hash;
                }
            }
        }

        [CustomPropertyDrawer(typeof(SerializedHash32))]
        private class Drawer : PropertyDrawer
        {
            private const float HashDisplayWidth = 90;

            private GUIStyle m_HashStyle;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                if (m_HashStyle == null)
                {
                    m_HashStyle = new GUIStyle(EditorStyles.label);
                    m_HashStyle.normal.textColor = Color.gray;
                }

                label = EditorGUI.BeginProperty(position, label, property);
                Rect propRect = position;
                propRect.width -= HashDisplayWidth - 4;
                Rect labelRect = new Rect(propRect.xMax + 4, propRect.y, HashDisplayWidth, propRect.height);
                
                EditorGUI.BeginChangeCheck();
                var stringProp = property.FindPropertyRelative("m_Source");
                var hashProp = property.FindPropertyRelative("m_HashValue");
                EditorGUI.showMixedValue = stringProp.hasMultipleDifferentValues;
                string newString = EditorGUI.TextField(propRect, label, stringProp.stringValue);
                EditorGUI.showMixedValue = false;
                if (UnityEditor.EditorGUI.EndChangeCheck())
                {
                    stringProp.stringValue = newString;
                    hashProp.longValue = new StringHash32(newString).HashValue;
                }
                
                int lastIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                EditorGUI.LabelField(labelRect, "0x" + hashProp.longValue.ToString("X8"), m_HashStyle);

                EditorGUI.indentLevel = lastIndent;

                UnityEditor.EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        #endif // UNITY_EDITOR

        #region Comparisons

        /// <summary>
        /// Default comparer.
        /// </summary>
        private sealed class Comparer : IEqualityComparer<SerializedHash32>, IComparer<SerializedHash32>
        {
            public int Compare(SerializedHash32 x, SerializedHash32 y)
            {
                return x.m_HashValue < y.m_HashValue ? -1 : (x.m_HashValue > y.m_HashValue ? 1 : 0);
            }

            public bool Equals(SerializedHash32 x, SerializedHash32 y)
            {
                return x.m_HashValue == y.m_HashValue;
            }

            public int GetHashCode(SerializedHash32 obj)
            {
                return unchecked((int) obj.m_HashValue);
            }
        }

        #endregion // Comparisons
    }
}