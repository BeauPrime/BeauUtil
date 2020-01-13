/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    20 Nov 2019
 * 
 * File:    ConditionalInputAttribute.cs
 * Purpose: Marks a property to be disabled based on another property.
 */

using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Shows/hides this property based on another property's value.
    /// </summary>
    public class ConditionalInputFieldAttribute : PropertyAttribute
    {
        public string PropertyName { get; private set; }
        public bool DesiredValue { get; private set; }

        public ConditionalInputFieldAttribute(string inPropertyName, bool inbDesiredValue)
        {
            PropertyName = inPropertyName;
            DesiredValue = inbDesiredValue;
        }
    }

    /// <summary>
    /// Enables this property if another property is true.
    /// </summary>
    public class EnableIfFieldAttribute : ConditionalInputFieldAttribute
    {
        public EnableIfFieldAttribute(string inPropertyName)
            : base(inPropertyName, true)
        { }
    }

    /// <summary>
    /// Disables this property if another property is false.
    /// </summary>
    public class DisableIfField : ConditionalInputFieldAttribute
    {
        public DisableIfField(string inPropertyName)
            : base(inPropertyName, false)
        { }
    }
}