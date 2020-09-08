/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 August 2020
 * 
 * File:    TypeReference.cs
 * Purpose: Reference to a specific c# type.
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Reference to a System.Type
    /// </summary>
    [Serializable]
    public struct SystemTypeReference : IEquatable<SystemTypeReference>
    {
        #region Inspector

        [SerializeField] private string m_AssemblyQualifiedName;

        #endregion // Inspector

        [NonSerialized] private string m_CachedName;
        [NonSerialized] private System.Type m_CachedType;

        public SystemTypeReference(System.Type inType)
            : this()
        {
            AssignType(inType);
        }

        public System.Type Type
        {
            get { UpdateCache(); return m_CachedType; }
            set { AssignType(value); }
        }

        private void AssignType(Type inType)
        {
            m_AssemblyQualifiedName = inType == null ? null : inType.AssemblyQualifiedName;
            m_CachedType = inType;
            m_CachedName = m_AssemblyQualifiedName;
        }

        private void UpdateCache()
        {
            if (StringComparer.Ordinal.Equals(m_CachedName, m_AssemblyQualifiedName))
                return;

            m_CachedName = m_AssemblyQualifiedName;
            m_CachedType = string.IsNullOrEmpty(m_CachedName) ? null : Type.GetType(m_CachedName, false);
        }

        #region Overrides

        public bool Equals(SystemTypeReference other)
        {
            return StringComparer.Ordinal.Equals(m_AssemblyQualifiedName, other.m_AssemblyQualifiedName);
        }

        public override bool Equals(object obj)
        {
            if (obj is SystemTypeReference)
            {
                return Equals((SystemTypeReference) obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(m_AssemblyQualifiedName);
        }

        public override string ToString()
        {
            return m_AssemblyQualifiedName;
        }

        #endregion // Overrides

        #region Operators

        static public bool operator==(SystemTypeReference left, SystemTypeReference right)
        {
            return left.Equals(right);
        }

        static public bool operator!=(SystemTypeReference left, SystemTypeReference right)
        {
            return !left.Equals(right);
        }

        static public implicit operator System.Type(SystemTypeReference inReference)
        {
            return inReference.Type;
        }

        static public implicit operator SystemTypeReference(System.Type inType)
        {
            return new SystemTypeReference(inType);
        }

        #endregion // Operators
    }
}