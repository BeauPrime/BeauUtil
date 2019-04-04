/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    SortingLayerAttribute.cs
 * Purpose: Marks an integer property as a sorting layer id.
 */

using System;
using UnityEngine;

namespace BeauUtil
{
    public sealed class SortingLayerAttribute : PropertyAttribute
    {
        #if UNITY_EDITOR

        [UnityEditor.CustomPropertyDrawer(typeof(SortingLayerAttribute))]
        private class Drawer : UnityEditor.PropertyDrawer
        {
            static private GUIContent[] s_LayerContent;
            static private int[] s_LayerIds;

            static private void InitializeLayerNames()
            {
                SortingLayer[] layers = SortingLayer.layers;
                Array.Resize(ref s_LayerContent, layers.Length);
                Array.Resize(ref s_LayerIds, layers.Length);

                for (int i = 0; i < layers.Length; ++i)
                {
                    s_LayerContent[i] = new GUIContent(layers[i].name);
                    s_LayerIds[i] = layers[i].id;
                }
            }

            public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
            {
                InitializeLayerNames();

                int layerIdx = property.hasMultipleDifferentValues ? -1 : Array.IndexOf(s_LayerIds, property.intValue);

                GUI.changed = false;
                int nextLayerIdx = UnityEditor.EditorGUI.Popup(position, label, layerIdx, s_LayerContent);
                if (GUI.changed)
                    property.intValue = nextLayerIdx >= 0 ? s_LayerIds[nextLayerIdx] : -1;
            }
        }

        #endif // UNITY_EDITOR
    }
}