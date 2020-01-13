/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    13 Nov 2019
 * 
 * File:    KeyValueAttribute.cs
 * Purpose: Marks a property to use a single-line key value inspector.
 */

using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Marks a property to use a single-line key value inspector
    /// </summary>
    public sealed class KeyValuePairAttribute : PropertyAttribute
    {
        public string KeyPropertyName { get; private set; }
        public string ValuePropertyName { get; private set; }

        public KeyValuePairAttribute() : this("Key", "Value") { }

        public KeyValuePairAttribute(string inKeyPropertyName, string inValuePropertyName)
        {
            KeyPropertyName = inKeyPropertyName;
            ValuePropertyName = inValuePropertyName;
        } 
    }
}