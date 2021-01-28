/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    25 Jan 2021
 * 
 * File:    InjectionCache.cs
 * Purpose: Cache for fields and settable properties.
 */

using System;
using System.Collections.Generic;
using System.Reflection;

namespace BeauUtil
{
    /// <summary>
    /// Cache for injecting values into a type or system.
    /// Detects fields and settable properties.
    /// </summary>
    public sealed class InjectionCache<T> where T : Attribute
    {
        #region Types

        private class CacheEntry
        {
            public AttributeBinding<T, MemberInfo>[] InstanceMembers;
        }

        #endregion // Types

        static private readonly MemberTypes MemberTypeMask = MemberTypes.Property | MemberTypes.Field;
        static private readonly BindingFlags BindingFlagMask = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly Dictionary<Type, CacheEntry> m_Entries = new Dictionary<Type, CacheEntry>();

        /// <summary>
        /// Retrieves the attribute bindings for the given type.
        /// </summary>
        public IReadOnlyList<AttributeBinding<T, MemberInfo>> Get(Type inType)
        {
            var entry = GetEntry(inType);
            return entry.InstanceMembers;
        }

        private void BuildEntry(Type inType, CacheEntry inEntry)
        {
            List<AttributeBinding<T, MemberInfo>> members = new List<AttributeBinding<T, MemberInfo>>(16);
            foreach(var binding in Reflect.FindMembers<T>(inType, BindingFlagMask))
            { 
                if ((binding.Info.MemberType & MemberTypeMask) == 0)
                    continue;
                
                if (binding.Info.MemberType == MemberTypes.Property)
                {
                    PropertyInfo prop = (PropertyInfo) binding.Info;
                    if (prop.GetSetMethod() == null)
                        continue;
                }

                members.Add(binding);
            }

            inEntry.InstanceMembers = members.ToArray();
        }

        private CacheEntry GetEntry(Type inType)
        {
            CacheEntry entry;
            if (!m_Entries.TryGetValue(inType, out entry))
            {
                entry = new CacheEntry();
                BuildEntry(inType, entry);
                m_Entries.Add(inType, entry);
            }
            return entry;
        }
    }
}