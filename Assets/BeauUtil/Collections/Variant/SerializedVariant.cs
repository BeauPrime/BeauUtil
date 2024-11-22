/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 April 2021
 * 
 * File:    SerializedVariant.cs
 * Purpose: Variant for use with the inspector.
 */

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace BeauUtil.Variants
{
    /// <summary>
    /// Serializable variant data container.
    /// </summary>
    [Serializable]
    public struct SerializedVariant
    {
        #region Inspector

        [SerializeField] private VariantType m_Type;
        [SerializeField] private uint m_RawValue;
        [SerializeField] private string m_StringHashSource;

        #endregion // Inspector

        public SerializedVariant(Variant inValue)
        {
            m_Type = inValue.Type;
            m_RawValue = inValue.RawValue;
            m_StringHashSource = null;
        }

        public SerializedVariant(StringSlice inString)
        {
            m_Type = VariantType.StringHash;
            m_StringHashSource = inString.ToString();
            m_RawValue = inString.Hash32().HashValue;
        }

        public Variant AsVariant()
        {
            return new Variant(m_Type, m_RawValue);
        }

        static public implicit operator Variant(SerializedVariant inSerializedVariant)
        {
            return inSerializedVariant.AsVariant();
        }

        #if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(SerializedVariant))]
        private class Drawer : PropertyDrawer
        {
            private const float TypeDisplayWidth = 100;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                label = EditorGUI.BeginProperty(position, label, property);

                Rect propRect = position;
                propRect.width -= TypeDisplayWidth - 4;

                Rect typeRect = new Rect(propRect.xMax + 4, propRect.y, TypeDisplayWidth, propRect.height);
                
                // type

                int lastIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                EditorGUI.BeginChangeCheck();

                var typeProp = property.FindPropertyRelative("m_Type");
                VariantType type = (VariantType) typeProp.intValue;
                type = (VariantType) EditorGUI.EnumPopup(typeRect, type);

                if (EditorGUI.EndChangeCheck())
                {
                    typeProp.intValue = (int) type;
                }

                EditorGUI.indentLevel = lastIndent;

                // value

                var rawProp = property.FindPropertyRelative("m_RawValue");
                uint rawValue = (uint) rawProp.longValue;

                switch(type)
                {
                    case VariantType.Null:
                        {
                            EditorGUI.LabelField(propRect, label, new GUIContent("Null value"));
                            break;
                        }

                    case VariantType.Int:
                        {
                            int intVal = GetInt(rawValue);
                            int newVal = EditorGUI.IntField(propRect, label, intVal);
                            if (newVal != intVal)
                            {
                                rawProp.longValue = GetRaw(newVal);
                            }
                            break;
                        }

                    case VariantType.UInt:
                        {
                            uint uintVal = rawValue;
                            uint newVal = (uint) EditorGUI.LongField(propRect, label, uintVal);
                            if (newVal != uintVal)
                            {
                                rawProp.longValue = newVal;
                            }
                            break;
                        }

                    case VariantType.Float:
                        {
                            float floatVal = GetFloat(rawValue);
                            float newVal = EditorGUI.FloatField(propRect, label, floatVal);
                            if (newVal != floatVal)
                            {
                                rawProp.longValue = GetRaw(newVal);
                            }
                            break;
                        }

                    case VariantType.Bool:
                        {
                            bool boolVal = GetBool(rawValue);
                            bool bNewVal = EditorGUI.Toggle(propRect, label, boolVal);
                            if (bNewVal != boolVal)
                            {
                                rawProp.longValue = GetRaw(bNewVal);
                            }
                            break;
                        }

                    case VariantType.StringHash:
                        {
                            var stringProp = property.FindPropertyRelative("m_StringHashSource");
                            
                            string stringVal = stringProp.stringValue;
                            string newVal = EditorGUI.TextField(propRect, label, stringVal);
                            if (stringVal != newVal)
                            {
                                stringProp.stringValue = newVal;
                                rawProp.longValue = StringHashing.Hash32(newVal, 0, newVal.Length);
                            }
                            break;
                        }

                    case VariantType.InstanceId:
                        {
                            EditorGUI.HelpBox(propRect, "Object reference not allowed", MessageType.Error);
                            break;
                        }

                }

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            #region Casting

            static private unsafe uint GetRaw(float inValue)
            {
                return *((uint*) (&inValue));
            }

            static private unsafe float GetFloat(uint inValue)
            {
                return *((float*) (&inValue));
            }

            static private unsafe uint GetRaw(int inValue)
            {
                return *((uint*) (&inValue));
            }

            static private unsafe int GetInt(uint inValue)
            {
                return *((int*) (&inValue));
            }

            static private unsafe uint GetRaw(bool inbValue)
            {
                return *((uint*) (&inbValue));
            }

            static private unsafe bool GetBool(uint inValue)
            {
                return *((bool*) (&inValue));
            }

            #endregion // Casting
            
        }

        #endif // UNITY_EDITOR
    }
}