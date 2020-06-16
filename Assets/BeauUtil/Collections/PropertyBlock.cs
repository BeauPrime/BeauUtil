/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    2 May 2019
 * 
 * File:    PropertyBlock.cs
 * Purpose: Serializable key-value block.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif // UNITY_EDITOR

namespace BeauUtil
{
    /// <summary>
    /// Interface for a property block with arbitrary values associated with keys.
    /// </summary>
    public interface IReadOnlyPropertyBlock<TKey> where TKey : IEquatable<TKey>
    {
        TValue Get<TValue>(TKey inKey);
        TValue Get<TValue>(TKey inKey, TValue inDefault);

        IReadOnlyPropertyBlock<TKey> Prototype { get; }

        bool Has(TKey inKey, bool inbIncludePrototype = true);
        int Count(bool inbIncludePrototype = true);
        IEnumerable<TKey> Keys(bool inbIncludePrototype = true);
    }

    /// <summary>
    /// Modifiable property block.
    /// </summary>
    public interface IPropertyBlock<TKey> : IReadOnlyPropertyBlock<TKey> where TKey : IEquatable<TKey>
    {
        new IReadOnlyPropertyBlock<TKey> Prototype { get; set; }

        void Set(TKey inKey, object inValue);
        bool Remove(TKey inKey);
        void Clear();
    }

    /// <summary>
    /// Serializable property block.
    /// </summary>
    [Serializable]
    public sealed class PropertyBlock : IPropertyBlock<PropertyName>
    {
        #region Types

        private enum FieldType
        {
            Int64, // (int) IntegerValue
            Bool, // IntegerValue > 0
            Color, // (Color) VectorValue.xyzw
            HDRColor, // (Color) VectorValue.xyzw
            Double, // NumberValue
            Vector, // VectorValue
            String, // StringValue
            UnityObject, // UnityObjectValue,
            Raw // RawObjectValue
        }

        [Serializable]
        private sealed class Field
        {
            #region Inspector

            [SerializeField]
            public PropertyName Key;

            [SerializeField]
            public FieldType Type;

            [SerializeField]
            private long m_IntValue;

            [SerializeField]
            private double m_NumberValue;

            [SerializeField]
            private string m_StringValue;

            [SerializeField]
            private Vector4 m_VectorValue;

            [SerializeField]
            private UnityEngine.Object m_UnityObjectValue;

            [NonSerialized]
            private object m_RawObjectValue;

            #endregion // Inspector

            #region Get/Set

            public object Get(Type inExpectedType)
            {
                if (inExpectedType == typeof(object))
                    return GetObject();

                if (inExpectedType.IsEnum)
                    return GetEnum(inExpectedType);

                TypeCode valueTypeCode = System.Type.GetTypeCode(inExpectedType);
                switch (valueTypeCode)
                {
                    case TypeCode.Boolean:
                        return BoolValue;
                    case TypeCode.Byte:
                        return (byte) LongValue;
                    case TypeCode.Int16:
                        return (Int16) LongValue;
                    case TypeCode.Int32:
                        return (Int32) LongValue;
                    case TypeCode.Int64:
                        return (Int64) LongValue;
                    case TypeCode.Double:
                        return DoubleValue;
                    case TypeCode.SByte:
                        return (sbyte) LongValue;
                    case TypeCode.Single:
                        return (float) DoubleValue;
                    case TypeCode.String:
                        return StringValue;
                    case TypeCode.UInt16:
                        return (UInt16) LongValue;
                    case TypeCode.UInt32:
                        return (UInt32) LongValue;
                    case TypeCode.Object:
                        {
                            if (inExpectedType == typeof(Color))
                                return ColorValue;
                            if (inExpectedType == typeof(Vector4))
                                return VectorValue;
                            if (inExpectedType == typeof(Vector3))
                                return (Vector3) VectorValue;
                            if (inExpectedType == typeof(Vector2))
                                return (Vector2) VectorValue;
                            if (inExpectedType == typeof(Quaternion))
                            {
                                Vector4 val = VectorValue;
                                return new Quaternion(val.x, val.y, val.z, val.w);
                            }
                            if (typeof(UnityEngine.Object).IsAssignableFrom(inExpectedType))
                                return UnityObjectValue;

                            break;
                        }

                    case TypeCode.Empty:
                        return null;
                }

                return RawObjectValue;
            }

            public void Set(object inValue)
            {
                if (inValue == null)
                {
                    SetType(FieldType.Raw);
                    m_RawObjectValue = null;
                    return;
                }

                Type valueType = inValue.GetType();
                if (valueType.IsEnum)
                {
                    SetEnum((Enum) inValue);
                    return;
                }

                TypeCode valueTypeCode = System.Type.GetTypeCode(valueType);
                switch (valueTypeCode)
                {
                    case TypeCode.Boolean:
                        BoolValue = Convert.ToBoolean(inValue);
                        return;
                    case TypeCode.Byte:
                        LongValue = Convert.ToByte(inValue);
                        return;
                    case TypeCode.Int16:
                        LongValue = Convert.ToInt16(inValue);
                        return;
                    case TypeCode.Int32:
                        LongValue = Convert.ToInt32(inValue);
                        return;
                    case TypeCode.Int64:
                        LongValue = Convert.ToInt64(inValue);
                        return;
                    case TypeCode.Double:
                        DoubleValue = Convert.ToDouble(inValue);
                        return;
                    case TypeCode.SByte:
                        LongValue = Convert.ToSByte(inValue);
                        return;
                    case TypeCode.Single:
                        DoubleValue = Convert.ToSingle(inValue);
                        return;
                    case TypeCode.String:
                        StringValue = Convert.ToString(inValue);
                        return;
                    case TypeCode.UInt16:
                        LongValue = Convert.ToUInt16(inValue);
                        return;
                    case TypeCode.UInt32:
                        LongValue = Convert.ToUInt32(inValue);
                        return;
                    case TypeCode.Object:
                        {
                            if (valueType == typeof(Color))
                            {
                                ColorValue = (Color) inValue;
                                return;
                            }
                            if (valueType == typeof(Vector4))
                            {
                                VectorValue = (Vector4) inValue;
                                return;
                            }
                            if (valueType == typeof(Vector3))
                            {
                                VectorValue = (Vector3) inValue;
                                return;
                            }
                            if (valueType == typeof(Vector2))
                            {
                                VectorValue = (Vector2) inValue;
                                return;
                            }
                            if (valueType == typeof(Quaternion))
                            {
                                Quaternion quat = (Quaternion) inValue;
                                VectorValue = new Vector4(quat.x, quat.y, quat.z, quat.w);
                                return;
                            }
                            if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
                            {
                                UnityObjectValue = (UnityEngine.Object) inValue;
                                return;
                            }

                            break;
                        }

                    case TypeCode.Empty:
                        UnityObjectValue = null;
                        return;
                }

                RawObjectValue = inValue;
            }

            #endregion // Get/Set

            #region Value Types

            private long LongValue
            {
                get
                {
                    switch (Type)
                    {
                        case FieldType.Int64:
                        case FieldType.Bool:
                            return m_IntValue;

                        case FieldType.Double:
                            return (long) m_NumberValue;

                        default:
                            return 0;
                    }
                }
                set
                {
                    SetType(FieldType.Int64);
                    m_IntValue = value;
                }
            }

            private double DoubleValue
            {
                get
                {
                    switch (Type)
                    {
                        case FieldType.Int64:
                        case FieldType.Bool:
                            return m_IntValue;

                        case FieldType.Double:
                            return m_NumberValue;

                        default:
                            return 0;
                    }
                }
                set
                {
                    SetType(FieldType.Double);
                    m_NumberValue = value;
                }
            }

            private bool BoolValue
            {
                get
                {
                    switch (Type)
                    {
                        case FieldType.Int64:
                        case FieldType.Bool:
                            return m_IntValue > 0;

                        case FieldType.Double:
                            return m_VectorValue.x > 0;

                        case FieldType.UnityObject:
                            return m_UnityObjectValue != null;

                        default:
                            return false;
                    }
                }
                set
                {
                    SetType(FieldType.Bool);
                    m_IntValue = value ? 1 : 0;
                }
            }

            private Color ColorValue
            {
                get
                {
                    switch (Type)
                    {
                        case FieldType.Color:
                        case FieldType.HDRColor:
                        case FieldType.Vector:
                            return (Color) m_VectorValue;

                        default:
                            return default(Color);
                    }
                }
                set
                {
                    SetType(FieldType.Color);
                    m_VectorValue = value;
                }
            }

            private string StringValue
            {
                get
                {
                    switch (Type)
                    {
                        case FieldType.String:
                            return m_StringValue;
                        case FieldType.Color:
                        case FieldType.HDRColor:
                            return ColorValue.ToString();
                        case FieldType.Int64:
                            return LongValue.ToString();
                        case FieldType.Double:
                            return DoubleValue.ToString();
                        case FieldType.UnityObject:
                            return m_UnityObjectValue.ToString();
                        case FieldType.Bool:
                            return BoolValue.ToString();

                        default:
                            return null;
                    }
                }
                set
                {
                    SetType(FieldType.String);
                    m_StringValue = value;
                    m_RawObjectValue = null;
                }
            }

            private Vector4 VectorValue
            {
                get
                {
                    switch (Type)
                    {
                        case FieldType.Color:
                        case FieldType.HDRColor:
                        case FieldType.Vector:
                            return m_VectorValue;

                        default:
                            return default(Vector4);
                    }
                }
                set
                {
                    SetType(FieldType.Vector);
                    m_VectorValue = value;
                }
            }

            private UnityEngine.Object UnityObjectValue
            {
                get
                {
                    switch (Type)
                    {
                        case FieldType.UnityObject:
                            return m_UnityObjectValue;

                        default:
                            return null;
                    }
                }
                set
                {
                    SetType(FieldType.UnityObject);
                    m_UnityObjectValue = value;
                }
            }

            private System.Object RawObjectValue
            {
                get { return GetObject(); }
                set
                {
                    SetType(FieldType.Raw);
                    m_RawObjectValue = value;
                }
            }

            private System.Object GetObject()
            {
                switch (Type)
                {
                    case FieldType.Int64:
                        return LongValue;
                    case FieldType.Bool:
                        return BoolValue;
                    case FieldType.Double:
                        return DoubleValue;
                    case FieldType.Color:
                    case FieldType.HDRColor:
                        return ColorValue;
                    case FieldType.String:
                        return StringValue;
                    case FieldType.Vector:
                        return VectorValue;
                    case FieldType.UnityObject:
                        return m_UnityObjectValue;
                    case FieldType.Raw:
                        return m_RawObjectValue;
                    default:
                        return null;
                }
            }

            private object GetEnum(Type inEnumType)
            {
                switch (Type)
                {
                    case FieldType.Int64:
                        return Enum.ToObject(inEnumType, LongValue);
                    case FieldType.String:
                        {
                            if (m_RawObjectValue == null)
                            {
                                Enum val = (Enum) Enum.Parse(inEnumType, m_StringValue, true);
                                m_RawObjectValue = val;
                            }
                            return m_RawObjectValue;
                        }

                    default:
                        return Enum.ToObject(inEnumType, 0);
                }
            }

            private void SetEnum(Enum inEnumValue)
            {
                SetType(FieldType.Int64);
                m_IntValue = Convert.ToInt64(inEnumValue);
            }

            private void SetType(FieldType inType)
            {
                if (Type == inType)
                    return;

                Type = inType;

                m_IntValue = 0;
                m_NumberValue = 0;
                m_UnityObjectValue = null;
                m_VectorValue = default(Vector4);
                m_StringValue = null;
                m_RawObjectValue = null;
            }

            #endregion // Value Types

            public Field Clone()
            {
                Field field = new Field();
                field.Key = Key;
                field.Type = Type;
                field.m_IntValue = m_IntValue;
                field.m_NumberValue = m_NumberValue;
                field.m_UnityObjectValue = m_UnityObjectValue;
                field.m_RawObjectValue = m_RawObjectValue;
                field.m_StringValue = m_StringValue;
                field.m_VectorValue = m_VectorValue;
                return field;
            }
        }

        #endregion // Types

        #region Inspector

        [SerializeField]
        private List<Field> m_Fields = null;

        #endregion // Inspector

        private Dictionary<PropertyName, Field> m_FieldMap;
        [NonSerialized] private bool m_Locked = false;
        [NonSerialized] private IReadOnlyPropertyBlock<PropertyName> m_Prototype = null;

        private void Initialize()
        {
            if (m_FieldMap != null)
                return;

            if (m_Fields == null)
            {
                m_Fields = new List<Field>();
                m_FieldMap = new Dictionary<PropertyName, Field>();
            }
            else
            {
                m_FieldMap = new Dictionary<PropertyName, Field>();
                foreach (var field in m_Fields)
                    m_FieldMap.Add(field.Key, field);
            }
        }

        /// <summary>
        /// Returns a clone of this PropertyBlock.
        /// </summary>
        public PropertyBlock Clone()
        {
            PropertyBlock block = new PropertyBlock();
            if (m_Fields != null)
            {
                List<Field> duplicatedList = new List<Field>(m_Fields.Count);
                foreach (var field in m_Fields)
                    duplicatedList.Add(field.Clone());
                block.m_Fields = duplicatedList;
            }
            block.m_Prototype = m_Prototype;
            return block;
        }

        /// <summary>
        /// Locks this PropertyBlock from edits.
        /// </summary>
        public bool Lock()
        {
            if (!m_Locked)
            {
                m_Locked = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unlocks this PropertyBlock, allowing edits.
        /// </summary>
        public bool Unlock()
        {
            if (m_Locked)
            {
                m_Locked = false;
                return true;
            }

            return false;
        }

        #region IPropertyBlock

        /// <summary>
        /// Prototype that defines parent fields.
        /// </summary>
        public IReadOnlyPropertyBlock<PropertyName> Prototype
        {
            get { return m_Prototype; }
            set
            {
                if (m_Locked)
                    throw new InvalidOperationException("Cannot modify prototype on a readonly PropertyBlock.");

                m_Prototype = value;
            }
        }

        /// <summary>
        /// Retrieves the value with the given key as the given type.
        /// Throws an exception if the key does not exist.
        /// </summary>
        public TValue Get<TValue>(PropertyName inKey)
        {
            Initialize();

            Field field;
            if (!m_FieldMap.TryGetValue(inKey, out field))
            {
                if (m_Prototype != null)
                    return m_Prototype.Get<TValue>(inKey);
                throw new Exception("Missing field " + inKey.ToString());
            }

            return (TValue) field.Get(typeof(TValue));
        }

        /// <summary>
        /// Retrieves the value with the given key as the given type.
        /// Will return a default value if the key does not exist.
        /// </summary>
        public TValue Get<TValue>(PropertyName inKey, TValue inDefault)
        {
            Initialize();

            Field field;
            if (!m_FieldMap.TryGetValue(inKey, out field))
            {
                if (m_Prototype != null)
                    return m_Prototype.Get<TValue>(inKey, inDefault);
                return inDefault;
            }

            return (TValue) field.Get(typeof(TValue));
        }

        /// <summary>
        /// Sets the value of the given key.
        /// </summary>
        public void Set(PropertyName inKey, object inValue)
        {
            if (m_Locked)
                throw new InvalidOperationException("Cannot modify values on a readonly PropertyBlock.");

            Initialize();

            Field field;
            if (!m_FieldMap.TryGetValue(inKey, out field))
            {
                field = new Field();
                field.Key = inKey;
                m_Fields.Add(field);
                m_FieldMap.Add(inKey, field);
            }

            field.Set(inValue);
        }

        /// <summary>
        /// Returns if the PropertyBlock has the given key.
        /// </summary>
        public bool Has(PropertyName inKey, bool inbIncludePrototype = true)
        {
            Initialize();

            if (m_FieldMap.ContainsKey(inKey))
                return true;
            if (inbIncludePrototype && m_Prototype != null)
                return m_Prototype.Has(inKey);
            return false;
        }

        /// <summary>
        /// Removes the given key.
        /// Returns if the key was present.
        /// </summary>
        public bool Remove(PropertyName inKey)
        {
            if (m_Locked)
                throw new InvalidOperationException("Cannot modify values on a readonly PropertyBlock.");

            Initialize();

            Field field;
            if (m_FieldMap.TryGetValue(inKey, out field))
            {
                m_FieldMap.Remove(inKey);
                m_Fields.Remove(field);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears all keys from the PropertyBlock.
        /// </summary>
        public void Clear()
        {
            if (m_Locked)
                throw new InvalidOperationException("Cannot modify values on a readonly PropertyBlock.");

            if (m_Fields != null)
                m_Fields.Clear();
            if (m_FieldMap != null)
                m_FieldMap.Clear();
        }

        /// <summary>
        /// The number of keys in the PropertyBlock.
        /// </summary>
        public int Count(bool inbIncludePrototype = true)
        {
            if (m_Prototype == null || !inbIncludePrototype)
            {
                if (m_Fields != null)
                    return m_Fields.Count;
                return 0;
            }

            HashSet<PropertyName> collection = new HashSet<PropertyName>();
            PropertyBlockExtensions.GatherKeys(this, collection);
            int count = collection.Count;
            collection.Clear();
            return count;
        }

        /// <summary>
        /// Collection of all keys in the PropertyBlock.
        /// </summary>
        public IEnumerable<PropertyName> Keys(bool inbIncludePrototype = true)
        {
            Initialize();

            if (m_Prototype == null || !inbIncludePrototype)
            {
                return m_FieldMap.Keys;
            }

            HashSet<PropertyName> collection = new HashSet<PropertyName>();
            PropertyBlockExtensions.GatherKeys(this, collection);
            return collection;
        }

        #endregion // IPropertyBlock

        #region Editor

        #if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(PropertyBlock))]
        private class Drawer : PropertyDrawer
        {
            [NonSerialized]
            private ReorderableList m_List;

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                float height = base.GetPropertyHeight(property, label);
                if (!property.hasMultipleDifferentValues && property.isExpanded)
                {
                    SerializedProperty fieldArr = property.FindPropertyRelative("m_Fields");
                    int allElements = Mathf.Max(fieldArr.arraySize, 1) + 1;
                    height += (allElements - 1) * EditorGUIUtility.standardVerticalSpacing + allElements * (EditorGUIUtility.singleLineHeight + 4) + 10;
                }
                return height;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                label = EditorGUI.BeginProperty(position, label, property);
                {
                    Rect foldoutPos = new Rect(position);
                    foldoutPos.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(foldoutPos, property, false);

                    if (property.hasMultipleDifferentValues)
                    {
                        Rect errorBox = new Rect(position);
                        errorBox.height = EditorGUIUtility.singleLineHeight;
                        errorBox.width = 150;
                        errorBox.x = position.x + position.width - errorBox.width;
                        EditorGUI.HelpBox(errorBox, "Cannot edit mixed blocks", MessageType.Error);
                    }
                    else
                    {
                        SerializedProperty fieldArr = property.FindPropertyRelative("m_Fields");
                        if (property.isExpanded)
                        {
                            Rect listPos = position;
                            using(new EditorGUI.IndentLevelScope(1))
                            {
                                listPos = EditorGUI.IndentedRect(listPos);
                                float offset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                                listPos.y += offset;
                                listPos.height -= offset;

                                InitializeList(fieldArr);
                                m_List.DoList(listPos);
                            }
                        }

                        {
                            if (!ValidateNames(fieldArr))
                            {
                                Rect errorBox = new Rect(position);
                                errorBox.height = EditorGUIUtility.singleLineHeight;
                                errorBox.width = 150;
                                errorBox.x = position.x + position.width - errorBox.width;
                                EditorGUI.HelpBox(errorBox, "Invalid or duplicate keys", MessageType.Error);
                            }
                        }
                    }
                }
                EditorGUI.EndProperty();
            }

            private void InitializeList(SerializedProperty inProperty)
            {
                if (m_List != null)
                    return;

                m_List = new ReorderableList(inProperty.serializedObject, inProperty);
                m_List.displayAdd = m_List.displayRemove = true;
                m_List.headerHeight = 4;
                m_List.drawHeaderCallback = (r) => { };
                m_List.elementHeight = EditorGUIUtility.singleLineHeight + 4;
                m_List.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    DrawElementGUI(rect, inProperty.GetArrayElementAtIndex(index));
                };
            }

            private void DrawElementGUI(Rect rect, SerializedProperty element)
            {
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;

                float oldLabelWidth = EditorGUIUtility.labelWidth;
                int oldIndent = EditorGUI.indentLevel;
                EditorGUIUtility.labelWidth = 0;
                EditorGUI.indentLevel = 0;

                float nameWidth = Mathf.Max(150f, rect.width * 0.3f);
                float consumedWidth = 0;

                {
                    Rect labelRect = rect;
                    labelRect.width = nameWidth;
                    consumedWidth += nameWidth + 4;
                    SerializedProperty keyProp = element.FindPropertyRelative("Key");
                    EditorGUI.PropertyField(labelRect, keyProp, GUIContent.none);
                }

                FieldType fieldType;
                {
                    Rect typeRect = rect;
                    typeRect.width = 100;
                    typeRect.x += consumedWidth;
                    consumedWidth += 104;
                    SerializedProperty typeProp = element.FindPropertyRelative("Type");
                    FieldType oldType = (FieldType) typeProp.intValue;

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);
                    fieldType = (FieldType) typeProp.intValue;
                    if (EditorGUI.EndChangeCheck())
                    {
                        ResetValues(element, fieldType, oldType);
                    }
                }

                {
                    Rect valueRect = rect;
                    valueRect.x += consumedWidth;
                    valueRect.width -= consumedWidth;
                    switch (fieldType)
                    {
                        case FieldType.Int64:
                            {
                                SerializedProperty longVal = element.FindPropertyRelative("m_IntValue");
                                EditorGUI.PropertyField(valueRect, longVal, GUIContent.none);
                                break;
                            }

                        case FieldType.Bool:
                            {
                                SerializedProperty longVal = element.FindPropertyRelative("m_IntValue");
                                bool bVal = EditorGUI.Toggle(valueRect, longVal.longValue > 0);
                                longVal.longValue = bVal ? 1 : 0;
                                break;
                            }

                        case FieldType.Color:
                            {
                                SerializedProperty vectorVal = element.FindPropertyRelative("m_VectorValue");
                                Color c = EditorGUI.ColorField(valueRect, GUIContent.none, vectorVal.vector4Value, true, true, false);
                                vectorVal.vector4Value = c;
                                break;
                            }

                        case FieldType.HDRColor:
                            {
                                SerializedProperty vectorVal = element.FindPropertyRelative("m_VectorValue");
                                Color c = EditorGUI.ColorField(valueRect, GUIContent.none, vectorVal.vector4Value, true, true, true);
                                vectorVal.vector4Value = c;
                                break;
                            }

                        case FieldType.Double:
                            {
                                SerializedProperty numVal = element.FindPropertyRelative("m_NumberValue");
                                EditorGUI.PropertyField(valueRect, numVal, GUIContent.none);
                                break;
                            }

                        case FieldType.String:
                            {
                                SerializedProperty stringVal = element.FindPropertyRelative("m_StringValue");
                                EditorGUI.PropertyField(valueRect, stringVal, GUIContent.none);
                                break;
                            }

                        case FieldType.UnityObject:
                            {
                                SerializedProperty objectVal = element.FindPropertyRelative("m_UnityObjectValue");
                                EditorGUI.ObjectField(valueRect, objectVal, GUIContent.none);
                                break;
                            }

                        case FieldType.Vector:
                            {
                                SerializedProperty vectorVal = element.FindPropertyRelative("m_VectorValue");
                                vectorVal.vector4Value = EditorGUI.Vector4Field(valueRect, GUIContent.none, vectorVal.vector4Value);
                                break;
                            }

                        case FieldType.Raw:
                            {
                                EditorGUI.HelpBox(valueRect, "Raw fields are not inspectable", MessageType.Warning);
                                break;
                            }
                    }
                }

                EditorGUIUtility.labelWidth = oldLabelWidth;
                EditorGUI.indentLevel = oldIndent;
            }

            private void ResetValues(SerializedProperty inFieldElement, FieldType inType, FieldType inOldType)
            {
                SerializedProperty longVal = inFieldElement.FindPropertyRelative("m_IntValue");
                SerializedProperty vectorVal = inFieldElement.FindPropertyRelative("m_VectorValue");
                SerializedProperty numVal = inFieldElement.FindPropertyRelative("m_NumberValue");
                SerializedProperty stringVal = inFieldElement.FindPropertyRelative("m_StringValue");
                SerializedProperty objectVal = inFieldElement.FindPropertyRelative("m_UnityObjectValue");

                longVal.longValue = 0;

                if (inType == FieldType.Color || inType == FieldType.HDRColor)
                {
                    if (inOldType != FieldType.Color && inOldType != FieldType.HDRColor)
                        vectorVal.vector4Value = Color.white;
                }
                else
                {
                    vectorVal.vector4Value = Vector4.zero;
                }

                numVal.doubleValue = 0;
                stringVal.stringValue = string.Empty;
                objectVal.objectReferenceValue = null;
            }

            private bool ValidateNames(SerializedProperty inFieldsElement)
            {
                s_UsedKeys.Clear();

                if (inFieldsElement.hasMultipleDifferentValues)
                    return false;

                int arrayCount = inFieldsElement.arraySize;
                for (int i = 0; i < arrayCount; ++i)
                {
                    SerializedProperty key = inFieldsElement.GetArrayElementAtIndex(i).FindPropertyRelative("Key");
                    string keyVal = key.stringValue;
                    if (string.IsNullOrEmpty(keyVal) || !s_UsedKeys.Add(keyVal))
                    {
                        s_UsedKeys.Clear();
                        return false;
                    }
                }

                s_UsedKeys.Clear();
                return true;
            }

            static private readonly HashSet<string> s_UsedKeys = new HashSet<string>();
        }

        #endif // UNITY_EDITOR

        #endregion // Editor
    }

    static public class PropertyBlockExtensions
    {
        static public object Get<TKey>(this IReadOnlyPropertyBlock<TKey> inPropertyBlock, TKey inKey) where TKey : IEquatable<TKey>
        {
            return inPropertyBlock.Get<object>(inKey);
        }

        #region Internal

        static internal void GatherKeys<TKey>(IReadOnlyPropertyBlock<TKey> inBlock, HashSet<TKey> ioKeys) where TKey : IEquatable<TKey>
        {
            foreach (var key in inBlock.Keys(false))
                ioKeys.Add(key);

            if (inBlock.Prototype != null)
                GatherKeys(inBlock.Prototype, ioKeys);
        }

        #endregion // Internal
    }
}