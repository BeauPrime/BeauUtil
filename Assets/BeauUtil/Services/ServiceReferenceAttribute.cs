/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    25 Jan 2021
 * 
 * File:    ServiceReferenceAttribute.cs
 * Purpose: Attribute marking a reference to an IService.
 */

using System;
using UnityEngine.Scripting;

namespace BeauUtil.Services
{
    /// <summary>
    /// Indicates this is a reference to a service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    [Preserve]
    public class ServiceReferenceAttribute : PreserveAttribute
    {
        /// <summary>
        /// If not set, a missing service will result in an exception.
        /// </summary>
        public bool Optional { get; set; }
    }
}