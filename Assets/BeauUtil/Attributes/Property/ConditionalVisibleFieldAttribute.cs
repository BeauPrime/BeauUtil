/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    20 Nov 2019
 * 
 * File:    ConditionalVisibleAttribute.cs
 * Purpose: Marks a property to be hidden based on another property.
 */

using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Shows/hides this property based on another property's value.
    /// </summary>
    public class ConditionalVisibleFieldAttribute : PropertyAttribute
    {
        public string PropertyName { get; private set; }
        public bool DesiredValue { get; private set; }

        public ConditionalVisibleFieldAttribute(string inPropertyName, bool inbDesiredValue)
        {
            PropertyName = inPropertyName;
            DesiredValue = inbDesiredValue;
        }
    }

    /// <summary>
    /// Shows this property if another property is true.
    /// </summary>
    public class ShowIfFieldAttribute : ConditionalVisibleFieldAttribute
    {
        public ShowIfFieldAttribute(string inPropertyName)
            : base(inPropertyName, true)
        { }
    }

    /// <summary>
    /// Hides this property if another property is true.
    /// </summary>
    public class HideIfFieldAttribute : ConditionalVisibleFieldAttribute
    {
        public HideIfFieldAttribute(string inPropertyName)
            : base(inPropertyName, false)
        { }
    }
}