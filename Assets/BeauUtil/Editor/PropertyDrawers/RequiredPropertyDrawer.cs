using System;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    [CustomPropertyDrawer(typeof(RequiredAttribute), true)]
    internal sealed class RequiredPropertyDrawer : PropertyDrawer
    {
        static RequiredPropertyDrawer()
        {
            ErrorIcon = new GUIContent(EditorGUIUtility.IconContent("console.erroricon"));
            ErrorIcon.tooltip = "This field must have a value assigned to it";
        }

        private const float ElementPadding = 2;
        
        private const float ErrorIconWidth = 18;
        private const float ErrorIconWidthWithPadding = ErrorIconWidth + ElementPadding;
        private const float FindButtonWidth = 60;
        private const float FindButtonWidthWithPadding = FindButtonWidth + ElementPadding;
        
        static private readonly GUIContent ErrorIcon;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RequiredAttribute attr = (RequiredAttribute) attribute;
            ComponentLookupDirection? lookupDir = attr.AutoAssignDirection;
            
            if (property.hasMultipleDifferentValues || !IsMissing(property))
            {
                label = EditorGUI.BeginProperty(position, label, property);
                RenderNothing();
                EditorGUI.PropertyField(position, property, label, true);
                RenderNothing();
            }
            else
            {
                Rect iconRect = EditorGUI.IndentedRect(new Rect(position.x, position.y, ErrorIconWidth, position.height));
                iconRect.width = ErrorIconWidth;

                Rect shiftedRect = new Rect(position.x + ErrorIconWidthWithPadding, position.y, position.width - ErrorIconWidthWithPadding, position.height);
                using(GUIScopes.LabelWidthScope.Adjust(-ErrorIconWidthWithPadding))
                {
                    if (CanAutoAssign(property, lookupDir))
                    {
                        Rect buttonRect = new Rect(position.x + position.width - FindButtonWidth, position.y, FindButtonWidth, position.height);
                        position.width -= FindButtonWidthWithPadding;
                        shiftedRect.width -= FindButtonWidthWithPadding;

                        label = EditorGUI.BeginProperty(position, label, property);
                        RenderErrorIcon(iconRect);
                        EditorGUI.PropertyField(shiftedRect, property, label, true);
                        if (GUI.Button(buttonRect, "Find"))
                        {
                            TryAutoAssign(property, lookupDir.Value);
                        }
                    }
                    else
                    {
                        label = EditorGUI.BeginProperty(position, label, property);
                        RenderErrorIcon(iconRect);
                        EditorGUI.PropertyField(shiftedRect, property, label, true);
                        RenderNothing();
                    }
                }
            }

            EditorGUI.EndProperty();
        }

        static private void RenderErrorIcon(Rect inRect)
        {
            GUI.Label(inRect, ErrorIcon);
        }

        static private void RenderNothing()
        {
            GUI.Label(Rect.zero, GUIContent.none);
        }

        static private bool CanAutoAssign(SerializedProperty inProperty, ComponentLookupDirection? inLookup)
        {
            if (inLookup == null || inProperty.serializedObject.isEditingMultipleObjects)
                return false;
            
            if (inProperty.propertyType != SerializedPropertyType.ObjectReference)
                return false;

            if (!typeof(UnityEngine.Component).IsAssignableFrom(inProperty.GetPropertyType()))
                return false;

            return inProperty.serializedObject.targetObject is Component;
        }

        static private void TryAutoAssign(SerializedProperty inProperty, ComponentLookupDirection inLookup)
        {
            Type componentType = inProperty.GetPropertyType();
            Component c = (Component) inProperty.serializedObject.targetObject;

            Component val;
            switch(inLookup)
            {
                case ComponentLookupDirection.Self:
                default:
                    val = c.GetComponent(componentType);
                    break;

                case ComponentLookupDirection.Parent:
                    val = c.GetComponentInParent(componentType);
                    break;

                case ComponentLookupDirection.Children:
                    val = c.GetComponentInChildren(componentType, true);
                    break;
            }

            if (val)
                inProperty.objectReferenceValue = val;
        }

        static private bool IsMissing(SerializedProperty inProperty)
        {
            switch(inProperty.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    return inProperty.objectReferenceValue == null;

                case SerializedPropertyType.String:
                    return string.IsNullOrEmpty(inProperty.stringValue);

                default:
                    return false;
            }
        }
    }
}