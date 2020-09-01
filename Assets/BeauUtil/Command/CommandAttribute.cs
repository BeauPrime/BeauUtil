/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 August 2020
 * 
 * File:    CommandAttribute.cs
 * Purpose: Attribute marking a command on an object.
 */

using System;

namespace BeauUtil.Command
{
    /// <summary>
    /// Attribute marking a command on an object.
    /// Either a field, property, or a method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; protected set; }
        public bool GlobalNamespace { get; protected set; }

        public CommandAttribute() { }
        public CommandAttribute(string inName, bool inbStatic = false)
        {
            Name = inName;
            GlobalNamespace = inbStatic;
        }
    }
}