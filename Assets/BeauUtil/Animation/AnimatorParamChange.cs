using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace BeauUtil
{
    [Serializable]
    public struct AnimatorParamChange : IEquatable<AnimatorParamChange>
    {
        #region Inspector

        [SerializeField]
        private string m_Name;

        [SerializeField]
        private AnimatorControllerParameterType m_Type;

        [SerializeField]
        private int m_IntValue;

        [SerializeField]
        private float m_FloatValue;

        #endregion // Inspector

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        #region Parameter Types

        public void SetInteger(string inName, int inValue)
        {
            m_Name = inName;
            SetInteger(inValue);
        }

        public void SetInteger(int inValue)
        {
            m_Type = AnimatorControllerParameterType.Int;
            m_IntValue = inValue;
            m_FloatValue = 0;
        }

        public void SetBool(string inName, bool inbValue)
        {
            m_Name = inName;
            SetBool(inbValue);
        }

        public void SetBool(bool inbValue)
        {
            m_Type = AnimatorControllerParameterType.Bool;
            m_IntValue = inbValue ? 1 : 0;
            m_FloatValue = 0;
        }

        public void SetFloat(string inName, float inValue)
        {
            m_Name = inName;
            SetFloat(inValue);
        }

        public void SetFloat(float inValue)
        {
            m_Type = AnimatorControllerParameterType.Float;
            m_IntValue = 0;
            m_FloatValue = inValue;
        }

        public void SetTrigger(string inName)
        {
            m_Name = inName;
            SetTrigger();
        }

        public void SetTrigger()
        {
            m_Type = AnimatorControllerParameterType.Trigger;
            m_IntValue = 1;
            m_FloatValue = 0;
        }

        public void ResetTrigger(string inName)
        {
            m_Name = inName;
            ResetTrigger();
        }

        public void ResetTrigger()
        {
            m_Type = AnimatorControllerParameterType.Trigger;
            m_IntValue = 0;
            m_FloatValue = 0;
        }

        #endregion // Parameter Types

        /// <summary>
        /// Applies this parameter change to the given animator.
        /// </summary>
        public bool Apply(Animator inAnimator)
        {
            if (string.IsNullOrEmpty(m_Name))
                return false;

            switch (m_Type)
            {
                case AnimatorControllerParameterType.Bool:
                    {
                        inAnimator.SetBool(m_Name, m_IntValue > 0);
                        break;
                    }

                case AnimatorControllerParameterType.Float:
                    {
                        inAnimator.SetFloat(m_Name, m_FloatValue);
                        break;
                    }

                case AnimatorControllerParameterType.Int:
                    {
                        inAnimator.SetInteger(m_Name, m_IntValue);
                        break;
                    }

                case AnimatorControllerParameterType.Trigger:
                    {
                        if (m_IntValue > 0)
                            inAnimator.SetTrigger(m_Name);
                        else
                            inAnimator.ResetTrigger(m_Name);
                        break;
                    }

                default:
                    {
                        throw new Exception("Unknown AnimatorControllerParameterType " + m_Type);
                    }
            }

            return true;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(m_Name);
        }

        #region Overrides

        public bool Equals(AnimatorParamChange other)
        {
            return m_Name == other.m_Name &&
                m_Type == other.m_Type &&
                m_IntValue == other.m_IntValue &&
                m_FloatValue == other.m_FloatValue;
        }

        #endregion // Overrides

        #if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(AnimatorParamChange))]
        private sealed class Inspector : PropertyDrawer
        {
            private const float TYPE_WIDTH = 80;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                Rect originalRect = position;
                Rect indentedRect = EditorGUI.IndentedRect(originalRect);

                label = EditorGUI.BeginProperty(originalRect, label, property);

                originalRect.width = EditorGUIUtility.labelWidth;
                indentedRect.x = originalRect.xMax;
                indentedRect.width = position.xMax - indentedRect.xMin;

                EditorGUI.LabelField(originalRect, label);

                int prevIndent = EditorGUI.indentLevel;
                {
                    EditorGUI.indentLevel = 0;

                    SerializedProperty nameProp = property.FindPropertyRelative("m_Name");
                    SerializedProperty typeProp = property.FindPropertyRelative("m_Type");
                    SerializedProperty intProp = property.FindPropertyRelative("m_IntValue");
                    SerializedProperty floatProp = property.FindPropertyRelative("m_FloatValue");

                    float nonTypeWidth = (indentedRect.width - TYPE_WIDTH) / 2 - 4;

                    // name
                    {
                        Rect nameRect = indentedRect;
                        nameRect.width = nonTypeWidth;

                        EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);
                    }
                    // type
                    {
                        Rect typeRect = indentedRect;
                        typeRect.width = TYPE_WIDTH;
                        typeRect.x += nonTypeWidth + 4;

                        EditorGUI.BeginChangeCheck();
                        EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);
                        if (EditorGUI.EndChangeCheck())
                        {
                            intProp.intValue = 0;
                            floatProp.floatValue = 0;
                        }
                    }
                    // value
                    {
                        Rect valueRect = indentedRect;
                        valueRect.width = nonTypeWidth;
                        valueRect.x += nonTypeWidth + TYPE_WIDTH + 8;

                        if (typeProp.hasMultipleDifferentValues)
                        {
                            EditorGUI.HelpBox(valueRect, "----", MessageType.Error);
                        }
                        else
                        {
                            switch ((AnimatorControllerParameterType) typeProp.intValue)
                            {
                                case AnimatorControllerParameterType.Bool:
                                case AnimatorControllerParameterType.Trigger:
                                    {
                                        EditorGUI.showMixedValue = intProp.hasMultipleDifferentValues;

                                        EditorGUI.BeginChangeCheck();
                                        bool bNewValue = EditorGUI.Toggle(valueRect, intProp.intValue > 0);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            intProp.intValue = bNewValue ? 1 : 0;
                                        }
                                        break;
                                    }

                                case AnimatorControllerParameterType.Int:
                                    {
                                        EditorGUI.showMixedValue = intProp.hasMultipleDifferentValues;

                                        EditorGUI.BeginChangeCheck();
                                        int newValue = EditorGUI.DelayedIntField(valueRect, intProp.intValue);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            intProp.intValue = newValue;
                                        }
                                        break;
                                    }

                                case AnimatorControllerParameterType.Float:
                                    {
                                        EditorGUI.showMixedValue = floatProp.hasMultipleDifferentValues;

                                        EditorGUI.BeginChangeCheck();
                                        float newValue = EditorGUI.DelayedFloatField(valueRect, floatProp.floatValue);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            floatProp.floatValue = newValue;
                                        }
                                        break;
                                    }

                                default:
                                    {
                                        EditorGUI.HelpBox(valueRect, "Unknown Type", MessageType.Error);
                                        break;
                                    }
                            }
                        }
                    }
                }
                EditorGUI.showMixedValue = false;
                EditorGUI.indentLevel = prevIndent;

                EditorGUI.EndProperty();
            }
        }

        #endif // UNITY_EDITOR
    }
}