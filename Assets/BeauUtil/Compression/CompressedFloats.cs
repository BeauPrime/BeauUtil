/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    2 April 2021
 * 
 * File:    CompressedFloats.cs
 * Purpose: Compressed floating point values.
 */

using UnityEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace BeauUtil
{
    /// <summary>
    /// Compressed float, with a range of [0, 1].
    /// Single byte.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 1), Serializable, DebuggerDisplay("{Value}")]
    public struct Fraction8 : IEquatable<Fraction8>
    {
        private const byte MaxValue = 1 << 7;

        [SerializeField] private byte m_Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fraction8(float inValue)
        {
            m_Value = (byte) (inValue * MaxValue);
        }

        public float Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_Value / (float) MaxValue; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Value = (byte) (value * MaxValue); }
        }

        #region Overrides

        public bool Equals(Fraction8 other)
        {
            return m_Value == other.m_Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is Fraction8)
                return Equals((Fraction8) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return m_Value;
        }

        static public bool operator==(Fraction8 left, Fraction8 right)
        {
            return left.m_Value == right.m_Value;
        }

        static public bool operator!=(Fraction8 left, Fraction8 right)
        {
            return left.m_Value != right.m_Value;
        }

        static public implicit operator float(Fraction8 inFraction)
        {
            return inFraction.Value;
        }

        #endregion // Overrides

        #region Editor

        #if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(Fraction8))]
        private class Drawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                label = EditorGUI.BeginProperty(position, label, property);
                var valProp = property.FindPropertyRelative("m_Value");
                
                float currentVal = valProp.intValue / (float) MaxValue;
                EditorGUI.BeginChangeCheck();
                float nextVal = EditorGUI.Slider(position, label, currentVal, 0, 1);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                {
                    valProp.intValue = (int) (nextVal * MaxValue);
                }
                
                UnityEditor.EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        #endif // UNITY_EDITOR

        #endregion // Editor
    }

    /// <summary>
    /// Compressed float, with a range of [0, 1].
    /// Two bytes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 2), Serializable, DebuggerDisplay("{Value}")]
    public struct Fraction16 : IEquatable<Fraction16>
    {
        private const ushort MaxValue = 1 << 15;

        [SerializeField] private ushort m_Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fraction16(float inValue)
        {
            m_Value = (ushort) (inValue * MaxValue);
        }

        public float Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_Value / (float) MaxValue; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Value = (byte) (value * MaxValue); }
        }

        #region Overrides

        public bool Equals(Fraction16 other)
        {
            return m_Value == other.m_Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is Fraction16)
                return Equals((Fraction16) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return m_Value;
        }

        static public bool operator==(Fraction16 left, Fraction16 right)
        {
            return left.m_Value == right.m_Value;
        }

        static public bool operator!=(Fraction16 left, Fraction16 right)
        {
            return left.m_Value != right.m_Value;
        }

        static public implicit operator float(Fraction16 inFraction)
        {
            return inFraction.Value;
        }

        #endregion // Overrides

        #region Editor

        #if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(Fraction16))]
        private class Drawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                label = EditorGUI.BeginProperty(position, label, property);
                var valProp = property.FindPropertyRelative("m_Value");
                
                float currentVal = valProp.intValue / (float) MaxValue;
                EditorGUI.BeginChangeCheck();
                float nextVal = EditorGUI.Slider(position, label, currentVal, 0, 1);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                {
                    valProp.intValue = (int) (nextVal * MaxValue);
                }
                
                UnityEditor.EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        #endif // UNITY_EDITOR

        #endregion // Editor
    }

    /// <summary>
    /// Compressed float, with a range of [-1, 1].
    /// Single byte.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 1), Serializable, DebuggerDisplay("{Value}")]
    public struct Normal8 : IEquatable<Normal8>
    {
        private const sbyte MaxValue = 1 << 6;

        [SerializeField] private sbyte m_Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Normal8(float inValue)
        {
            m_Value = (sbyte) (inValue * MaxValue);
        }

        public float Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_Value / (float) MaxValue; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Value = (sbyte) (value * MaxValue); }
        }

        #region Overrides

        public bool Equals(Normal8 other)
        {
            return m_Value == other.m_Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is Normal8)
                return Equals((Normal8) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return m_Value.GetHashCode();
        }

        static public bool operator==(Normal8 left, Normal8 right)
        {
            return left.m_Value == right.m_Value;
        }

        static public bool operator!=(Normal8 left, Normal8 right)
        {
            return left.m_Value != right.m_Value;
        }

        static public implicit operator float(Normal8 inFraction)
        {
            return inFraction.Value;
        }

        #endregion // Overrides

        #region Editor

        #if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(Normal8))]
        private class Drawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                label = EditorGUI.BeginProperty(position, label, property);
                var valProp = property.FindPropertyRelative("m_Value");
                
                float currentVal = valProp.intValue / (float) MaxValue;
                EditorGUI.BeginChangeCheck();
                float nextVal = EditorGUI.Slider(position, label, currentVal, -1, 1);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                {
                    valProp.intValue = (int) (nextVal * MaxValue);
                }
                
                UnityEditor.EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        #endif // UNITY_EDITOR

        #endregion // Editor
    }

    /// <summary>
    /// Compressed float, with a range of [-1, 1].
    /// Two bytes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 2), Serializable, DebuggerDisplay("{Value}")]
    public struct Normal16 : IEquatable<Normal16>
    {
        private const short MaxValue = 1 << 14;

        [SerializeField] private short m_Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Normal16(float inValue)
        {
            m_Value = (sbyte) (inValue * MaxValue);
        }

        public float Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_Value / (float) MaxValue; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Value = (short) (value * MaxValue); }
        }

        #region Overrides

        public bool Equals(Normal16 other)
        {
            return m_Value == other.m_Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is Normal16)
                return Equals((Normal16) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return m_Value.GetHashCode();
        }

        static public bool operator==(Normal16 left, Normal16 right)
        {
            return left.m_Value == right.m_Value;
        }

        static public bool operator!=(Normal16 left, Normal16 right)
        {
            return left.m_Value != right.m_Value;
        }

        static public implicit operator float(Normal16 inFraction)
        {
            return inFraction.Value;
        }

        #endregion // Overrides

        #region Editor

        #if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(Normal16))]
        private class Drawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                label = EditorGUI.BeginProperty(position, label, property);
                var valProp = property.FindPropertyRelative("m_Value");
                
                float currentVal = valProp.intValue / (float) MaxValue;
                EditorGUI.BeginChangeCheck();
                float nextVal = EditorGUI.Slider(position, label, currentVal, -1, 1);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                {
                    valProp.intValue = (int) (nextVal * MaxValue);
                }
                
                UnityEditor.EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        #endif // UNITY_EDITOR

        #endregion // Editor
    }
}