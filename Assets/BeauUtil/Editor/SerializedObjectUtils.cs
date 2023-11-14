/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    20 Nov 2019
 * 
 * File:    SerializedObjectUtils.cs
 * Purpose: Utility methods for dealing with serialized objects and properties.
 */

#if UNITY_2018_3_OR_NEWER
#define SUPPORTS_PREFABSTAGEUTILITY
#endif // UNITY_2018_3_OR_NEWER

#if SUPPORTS_PREFABSTAGEUTILITY
#if !UNITY_2021_2_OR_NEWER
#define EXPERIMENTAL_PREFABSTAGEUTILITY
#endif // UNITY_2021_OR_NEWER
#endif // SUPPORTS_PREFABSTAGEUTILITY

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections;

#if EXPERIMENTAL_PREFABSTAGEUTILITY
using PrefabStage = UnityEditor.Experimental.SceneManagement.PrefabStage;
using PrefabStageUtility = UnityEditor.Experimental.SceneManagement.PrefabStageUtility;
#elif SUPPORTS_PREFABSTAGEUTILITY
using PrefabStage = UnityEditor.SceneManagement.PrefabStage;
using PrefabStageUtility = UnityEditor.SceneManagement.PrefabStageUtility;
#endif // EXPERIMENTAL_PREFABSTAGEUTILITY

namespace BeauUtil.Editor
{
    static public class SerializedObjectUtils
    {
        #region Relative Properties

        /// <summary>
        /// Returns the parent property.
        /// </summary>
        static public SerializedProperty FindPropertyParent(this SerializedProperty inProperty)
        {
            StringSlice path = inProperty.propertyPath.Replace("Array.data", "");
            int lastDot = path.LastIndexOf('.');
            if (lastDot < 0)
                return null;

            StringSlice parentPath = path.Substring(0, lastDot);
            return inProperty.serializedObject.FindProperty(parentPath.ToString());
        }

        /// <summary>
        /// Returns a sibling property.
        /// </summary>
        static public SerializedProperty FindPropertySibling(this SerializedProperty inProperty, string inSiblingPath)
        {
            SerializedProperty parentProp = FindPropertyParent(inProperty);
            if (parentProp == null)
                return inProperty.serializedObject.FindProperty(inSiblingPath);

            return parentProp.FindPropertyRelative(inSiblingPath);
        }

        /// <summary>
        /// Evaluates a sibling property.
        /// </summary>
        static public bool EvaluateSiblingCondition(this SerializedProperty inProperty, string inSiblingPath)
        {
            SerializedProperty siblingProp = FindPropertySibling(inProperty, inSiblingPath);
            if (siblingProp != null)
            {
                if (siblingProp.hasMultipleDifferentValues)
                    return false;

                switch(siblingProp.propertyType)
                {
                    case SerializedPropertyType.Boolean:
                    default:
                        return siblingProp.boolValue;

                    case SerializedPropertyType.String:
                        return !string.IsNullOrEmpty(siblingProp.stringValue);

                    case SerializedPropertyType.ObjectReference:
                        return siblingProp.objectReferenceValue != null;
                }
            }

            SerializedProperty immediateParent = inProperty.FindPropertyParent();
            SerializedObject parent = inProperty.serializedObject;
            string methodName = inSiblingPath;
            int lastDot = methodName.IndexOf('.');
            if (lastDot >= 0)
            {
                string relativePath = methodName.Substring(0, lastDot);
                methodName = methodName.Substring(lastDot + 1);
                immediateParent = immediateParent == null ? parent.FindProperty(relativePath) : immediateParent.FindPropertyRelative(relativePath);
            }

            object parentObj = immediateParent == null ? parent.targetObject : FindObject(immediateParent);
            if (parentObj == null)
                return false;

            Type parentType = parentObj.GetType();
            MethodInfo method = parentType.GetMethod(methodName, InstanceBindingFlags);
            if (method == null || method.GetParameters().Length > 0)
                return false;

            object result = method.Invoke(parentObj, Array.Empty<object>());
            if (result == null)
                return false;
            if (result is bool)
                return (bool) result;
            if (result is string)
                return !string.IsNullOrEmpty((string) result);
            
            return false;
        }

        #endregion // Relative Properties

        #region Object

        /// <summary>
        /// Returns the object that owns this property.
        /// </summary>
        static public object FindOwner(this SerializedProperty inProperty)
        {
            SerializedProperty parent = inProperty.FindPropertyParent();
            if (parent == null)
            {
                return inProperty.serializedObject.targetObject;
            }

            return FindObject(parent);
        }

        /// <summary>
        /// Returns the C# type of the given property
        /// </summary>
        static public Type GetPropertyType(this SerializedProperty inProperty)
        {
            switch(inProperty.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    return typeof(AnimationCurve);
                case SerializedPropertyType.ArraySize:
                    return typeof(int);
                case SerializedPropertyType.Boolean:
                    return typeof(bool);
                case SerializedPropertyType.Bounds:
                    return typeof(Bounds);
                case SerializedPropertyType.BoundsInt:
                    return typeof(BoundsInt);
                case SerializedPropertyType.Character:
                    return typeof(char);
                case SerializedPropertyType.Color:
                    return typeof(Color);
                case SerializedPropertyType.FixedBufferSize:
                    return typeof(int);
                case SerializedPropertyType.Float:
                    return inProperty.type == "double" ? typeof(double) : typeof(float);
                case SerializedPropertyType.Integer:
                    return inProperty.type == "long" ? typeof(long) : typeof(int);
                case SerializedPropertyType.Quaternion:
                    return typeof(Quaternion);
                case SerializedPropertyType.Rect:
                    return typeof(Rect);
                case SerializedPropertyType.RectInt:
                    return typeof(RectInt);
                case SerializedPropertyType.String:
                    return typeof(String);
                case SerializedPropertyType.Vector2:
                    return typeof(Vector2);
                case SerializedPropertyType.Vector2Int:
                    return typeof(Vector2Int);
                case SerializedPropertyType.Vector3:
                    return typeof(Vector3);
                case SerializedPropertyType.Vector3Int:
                    return typeof(Vector3Int);
                case SerializedPropertyType.Vector4:
                    return typeof(Vector4);
                case SerializedPropertyType.Gradient:
                    return typeof(Gradient);
                case SerializedPropertyType.LayerMask:
                    return typeof(LayerMask);
                
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.ObjectReference:
                default:
                    return LocateReflectedObjectType(inProperty.serializedObject.targetObject, inProperty.propertyPath);
            }
        }

        /// <summary>
        /// Returns the object this property represents.
        /// </summary>
        static public object FindObject(this SerializedProperty inProperty)
        {
            switch(inProperty.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    return inProperty.animationCurveValue;
                case SerializedPropertyType.ArraySize:
                    return inProperty.arraySize;
                case SerializedPropertyType.Boolean:
                    return inProperty.boolValue;
                case SerializedPropertyType.Bounds:
                    return inProperty.boundsValue;
                case SerializedPropertyType.BoundsInt:
                    return inProperty.boundsIntValue;
                case SerializedPropertyType.Character:
                    return (char) inProperty.intValue;
                case SerializedPropertyType.Color:
                    return inProperty.colorValue;
                case SerializedPropertyType.ExposedReference:
                    return inProperty.exposedReferenceValue;
                case SerializedPropertyType.FixedBufferSize:
                    return inProperty.fixedBufferSize;
                case SerializedPropertyType.Float:
                    return inProperty.type == "double" ? inProperty.doubleValue : inProperty.floatValue;
                case SerializedPropertyType.Integer:
                    return inProperty.type == "long" ? inProperty.longValue : inProperty.intValue;
                case SerializedPropertyType.ObjectReference:
                    return inProperty.objectReferenceValue;
                case SerializedPropertyType.Quaternion:
                    return inProperty.quaternionValue;
                case SerializedPropertyType.Rect:
                    return inProperty.rectValue;
                case SerializedPropertyType.RectInt:
                    return inProperty.rectIntValue;
                case SerializedPropertyType.String:
                    return inProperty.stringValue;
                case SerializedPropertyType.Vector2:
                    return inProperty.vector2Value;
                case SerializedPropertyType.Vector2Int:
                    return inProperty.vector2IntValue;
                case SerializedPropertyType.Vector3:
                    return inProperty.vector3Value;
                case SerializedPropertyType.Vector3Int:
                    return inProperty.vector3IntValue;
                case SerializedPropertyType.Vector4:
                    return inProperty.vector4Value;
                
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.LayerMask:
                default:
                    return LocateReflectedObject(inProperty.serializedObject.targetObject, inProperty.propertyPath);
            }
        }

        /// <summary>
        /// Sets the object value of this property.
        /// </summary>
        static public void SetObject(this SerializedProperty inProperty, object inValue)
        {
            switch(inProperty.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    inProperty.animationCurveValue = (AnimationCurve) inValue;
                    break;
                case SerializedPropertyType.ArraySize:
                    inProperty.arraySize = (int) inValue;
                    break;
                case SerializedPropertyType.Boolean:
                    inProperty.boolValue = (bool) inValue;
                    break;
                case SerializedPropertyType.Bounds:
                    inProperty.boundsValue = (Bounds) inValue;
                    break;
                case SerializedPropertyType.BoundsInt:
                    inProperty.boundsIntValue = (BoundsInt) inValue;
                    break;
                case SerializedPropertyType.Character:
                    inProperty.intValue = (char) inValue;
                    break;
                case SerializedPropertyType.Color:
                    inProperty.colorValue = (Color) inValue;
                    break;
                case SerializedPropertyType.ExposedReference:
                    inProperty.exposedReferenceValue = (UnityEngine.Object) inValue;
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    throw new InvalidOperationException("Cannot assign a value to a FixedBufferSize property");
                case SerializedPropertyType.Float:
                    {
                        if (inProperty.type == "double")
                        {
                            inProperty.doubleValue = (double) inValue;
                        }
                        else
                        {
                            inProperty.floatValue = (float) inValue;
                        }
                        break;
                    }
                case SerializedPropertyType.Integer:
                    {
                        if (inProperty.type == "long")
                        {
                            inProperty.longValue = (long) inValue;
                        }
                        else
                        {
                            inProperty.intValue = (int) inValue;
                        }
                        break;
                    }
                case SerializedPropertyType.ObjectReference:
                    inProperty.objectReferenceValue = (UnityEngine.Object) inValue;
                    break;
                case SerializedPropertyType.Quaternion:
                    inProperty.quaternionValue = (Quaternion) inValue;
                    break;
                case SerializedPropertyType.Rect:
                    inProperty.rectValue = (Rect) inValue;
                    break;
                case SerializedPropertyType.RectInt:
                    inProperty.rectIntValue = (RectInt) inValue;
                    break;
                case SerializedPropertyType.String:
                    inProperty.stringValue = (string) inValue;
                    break;
                case SerializedPropertyType.Vector2:
                    inProperty.vector2Value = (Vector2) inValue;
                    break;
                case SerializedPropertyType.Vector2Int:
                    inProperty.vector2IntValue = (Vector2Int) inValue;
                    break;
                case SerializedPropertyType.Vector3:
                    inProperty.vector3Value = (Vector3) inValue;
                    break;
                case SerializedPropertyType.Vector3Int:
                    inProperty.vector3IntValue = (Vector3Int) inValue;
                    break;
                case SerializedPropertyType.Vector4:
                    inProperty.vector4Value = (Vector4) inValue;
                    break;
                
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.LayerMask:
                default:
                    Undo.RecordObject(inProperty.serializedObject.targetObject, string.Format("Set '{0}'", inProperty.name));

                    SetReflectedObject(inProperty.serializedObject.targetObject, inProperty.propertyPath, inValue);
                    
                    EditorUtility.SetDirty(inProperty.serializedObject.targetObject);
                    inProperty.serializedObject.ApplyModifiedProperties();
                    break;
            }
        }

        static private object LocateReflectedObject(UnityEngine.Object inRoot, string inPath)
        {
            if (string.IsNullOrEmpty(inPath))
                return inRoot;

            inPath = inPath.Replace("Array.data", "");

            object obj = inRoot;
            foreach(var pathSegment in StringSlice.EnumeratedSplit(inPath, PathSplitChars, StringSplitOptions.None))
            {
                if (obj == null)
                    break;

                StringSlice fieldName = pathSegment;
                Type objType = obj.GetType();
                int arrayIdx = -1;

                // capture array
                if (pathSegment.EndsWith(']'))
                {
                    int elementIndexStart = pathSegment.IndexOf('[');
                    int length = pathSegment.Length - elementIndexStart - 1;
                    StringSlice elementIndexSlice = pathSegment.Substring(elementIndexStart + 1, length);
                    arrayIdx = Convert.ToInt32(elementIndexSlice.ToString());
                    fieldName = pathSegment.Substring(0, elementIndexStart);
                }

                FieldInfo field = objType.GetField(fieldName.ToString(), InstanceBindingFlags);
                if (field == null)
                    return null;
                
                obj = field.GetValue(obj);
                if (arrayIdx >= 0)
                {
                    IEnumerable enumerable = obj as IEnumerable;
                    if (enumerable == null)
                        return null;

                    IEnumerator enumerator = enumerable.GetEnumerator();
                    for(int i = 0; i <= arrayIdx; ++i)
                    {
                        if (!enumerator.MoveNext())
                            return null;
                    }

                    obj = enumerator.Current;
                }
            }

            return obj;
        }

        static private Type LocateReflectedObjectType(UnityEngine.Object inRoot, string inPath)
        {
            if (string.IsNullOrEmpty(inPath))
                return inRoot.GetType();

            inPath = inPath.Replace("Array.data", "");

            object obj = inRoot;
            Type type = inRoot.GetType();
            foreach(var pathSegment in StringSlice.EnumeratedSplit(inPath, PathSplitChars, StringSplitOptions.None))
            {
                if (obj == null)
                    break;

                StringSlice fieldName = pathSegment;
                Type objType = obj.GetType();
                int arrayIdx = -1;

                // capture array
                if (pathSegment.EndsWith(']'))
                {
                    int elementIndexStart = pathSegment.IndexOf('[');
                    int length = pathSegment.Length - elementIndexStart - 1;
                    StringSlice elementIndexSlice = pathSegment.Substring(elementIndexStart + 1, length);
                    arrayIdx = Convert.ToInt32(elementIndexSlice.ToString());
                    fieldName = pathSegment.Substring(0, elementIndexStart);
                }

                FieldInfo field = objType.GetField(fieldName.ToString(), InstanceBindingFlags);
                if (field == null)
                    return null;
                
                obj = field.GetValue(obj);
                if (arrayIdx >= 0)
                {
                    IEnumerable enumerable = obj as IEnumerable;
                    if (enumerable == null)
                        return null;

                    type = enumerable.GetType().GetEnumerableType();

                    IEnumerator enumerator = enumerable.GetEnumerator();
                    for(int i = 0; i <= arrayIdx; ++i)
                    {
                        if (!enumerator.MoveNext())
                            return null;
                    }

                    obj = enumerator.Current;
                }
                else
                {
                    type = field.FieldType;
                }
            }

            return obj != null ? obj.GetType() : type;
        }

        static private void SetReflectedObject(UnityEngine.Object inRoot, string inPath, object inValue)
        {
            inPath = inPath.Replace("Array.data", "");

            // locate the parent object
            StringSlice path = inPath;
            int lastDot = path.LastIndexOf('.');
            if (lastDot < 0)
                return;

            StringSlice parentPath = path.Substring(0, lastDot);
            StringSlice fieldName = path.Substring(lastDot + 1);
            object parentObject = LocateReflectedObject(inRoot, parentPath.ToString());
            if (parentObject == null)
                return;

            Type objType = parentObject.GetType();
            int arrayIdx = -1;

            // capture array
            if (fieldName.EndsWith(']'))
            {
                int elementIndexStart = fieldName.IndexOf('[');
                int length = fieldName.Length - elementIndexStart - 1;
                StringSlice elementIndexSlice = fieldName.Substring(elementIndexStart + 1, length);
                arrayIdx = Convert.ToInt32(elementIndexSlice.ToString());
                fieldName = fieldName.Substring(0, elementIndexStart);
            }

            FieldInfo field = objType.GetField(fieldName.ToString(), InstanceBindingFlags);
            if (field == null)
                return;
            
            if (arrayIdx >= 0)
            {
                IList list = field.GetValue(parentObject) as IList;
                if (list != null)
                {
                    list[arrayIdx] = inValue;
                }
                return;
            }

            field.SetValue(parentObject, inValue);
        }

        static private readonly BindingFlags InstanceBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        static private readonly char[] PathSplitChars = new char[] { '.' };

        #endregion // Object
        
        #region Prefab

        /// <summary>
        /// Returns if this is editing a prefab or a prefab within the prefab editing stage.
        /// </summary>
        static public bool IsEditingPrefab(this SerializedProperty inProperty)
        {
#if SUPPORTS_PREFABSTAGEUTILITY
            foreach (UnityEngine.Object target in inProperty.serializedObject.targetObjects)
            {
                GameObject go = target as GameObject;
                if (ReferenceEquals(go, null))
                {
                    Component component = target as Component;
                    if (!ReferenceEquals(component, null))
                        go = component.gameObject;
                }

                if (ReferenceEquals(go, null))
                    return false;

                PrefabStage stage = PrefabStageUtility.GetPrefabStage(go);
                if (stage == null)
                    return false;

                if (inProperty.isInstantiatedPrefab)
                    return false;
            }
            return true;
#else
            throw new NotImplementedException("IsEditingPrefab not implemented for versions before 2018.3");
#endif // SUPPORTS_PREFABSTAGEUTILITY
        }

        /// <summary>
        /// Returns if this is editing a prefab instance.
        /// </summary>
        static public bool IsEditingPrefabInstance(this SerializedProperty inProperty)
        {
            return inProperty.isInstantiatedPrefab;
        }

        #endregion // Prefab

        #region Exposed Methods

        private delegate FieldInfo GetFieldInfoFromPropertyDelegate(SerializedProperty inProperty, out Type outType);
        static private readonly GetFieldInfoFromPropertyDelegate CachedGetFieldInfoFromProperty;

        static SerializedObjectUtils()
        {
            Type scriptAttributeUtility = Assembly.GetAssembly(typeof(ScriptableWizard)).GetType("UnityEditor.ScriptAttributeUtility");
            CachedGetFieldInfoFromProperty = (GetFieldInfoFromPropertyDelegate) scriptAttributeUtility.GetMethod("GetFieldInfoFromProperty", BindingFlags.NonPublic | BindingFlags.Static)?.CreateDelegate(typeof(GetFieldInfoFromPropertyDelegate));
        }

        static public FieldInfo GetFieldInfoFromProperty(SerializedProperty inProperty, out Type outType)
        {
            if (CachedGetFieldInfoFromProperty != null)
            {
                return CachedGetFieldInfoFromProperty(inProperty, out outType);
            }
            else
            {
                outType = null;
                return null;
            }
        }

        #endregion // Exposed Methods
    }
}