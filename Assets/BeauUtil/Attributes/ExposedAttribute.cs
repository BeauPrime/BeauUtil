/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    17 Feb 2021
 * 
 * File:    ExposedAttribute.cs
 * Purpose: Attribute exposing a member.
 */

using System;
using System.Reflection;

namespace BeauUtil
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class ExposedAttribute : Attribute
    {
        public StringHash32 Id;
        public string Name;

        public ExposedAttribute() { }
        public ExposedAttribute(string inName)
        {
            Name = inName;
            Id = Name;
        }

        public void AssignId(MemberInfo inInfo)
        {
            if (Id.IsEmpty)
            {
                Name = inInfo.Name;
                Id = inInfo.Name;
            }
        }
    }
}