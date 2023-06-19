/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    27 Jan 2021
 * 
 * File:    ServiceDependencyAttribute.cs
 * Purpose: Attribute marking a class or interface as having dependencies
            on a set of services.
 */

using System;
using UnityEngine.Scripting;

namespace BeauUtil.Services
{
    /// <summary>
    /// Indicates that a service class or interface is dependent
    /// on other services.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true)]
    [Preserve]
    public class ServiceDependencyAttribute : PreserveAttribute
    {
        public Type[] Dependencies { get; private set; }

        public ServiceDependencyAttribute(params Type[] inDependencies)
        {
            Dependencies = inDependencies;
        }
    }
}