/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    17 Feb 2021
 * 
 * File:    MethodCache.cs
 * Purpose: Cache for methods on types.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Cache for object commands
    /// </summary>
    public class MethodCache<TAttr> : IMethodCache
        where TAttr : ExposedAttribute
    {
        private const BindingFlags InstanceAttributeSearch = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticAttributeSearch = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        #region Types

        /// <summary>
        /// Description for a type.
        /// </summary>
        public sealed class TypeDescription
        {
            public readonly Type Type;
            
            private bool m_Processed;
            private readonly Dictionary<StringHash32, MethodDescription> m_Methods;

            public TypeDescription(Type inType)
            {
                Type = inType;

                m_Methods = new Dictionary<StringHash32, MethodDescription>(4);
            }

            public bool TryGetMethod(StringHash32 inId, out MethodDescription outMethod)
            {
                return m_Methods.TryGetValue(inId, out outMethod);
            }

            public bool TryProcess(MethodCache<TAttr> inParent)
            {
                foreach(var attrPair in Reflect.FindMethods<TAttr>(Type, InstanceAttributeSearch))
                {
                    attrPair.Attribute.AssignId(attrPair.Info);

                    if (m_Methods.ContainsKey(attrPair.Attribute.Id))
                    {
                        UnityEngine.Debug.LogErrorFormat("[MethodCache] Multiple instances of method with id '{0}' found on type '{1}'", attrPair.Attribute.Id.ToDebugString(), Type.FullName);
                        continue;
                    }

                    MethodDescription desc = inParent.CreateDescription(attrPair.Attribute, attrPair.Info);
                    if (!desc.TryProcess(inParent))
                    {
                        UnityEngine.Debug.LogErrorFormat("[MethodCache] Method '{0}' on type '{1}' is incompatible", desc.Id, Type.FullName);
                        continue;
                    }

                    m_Methods.Add(desc.Id, desc);
                }

                return true;
            }
        }

        /// <summary>
        /// Description for a method.
        /// </summary>
        public sealed class MethodDescription
        {
            public readonly StringHash32 Id;
            public readonly TAttr Attribute;
            public readonly MethodInfo Method;
            public MethodInvocationHelper Invoker;

            public MethodDescription(TAttr inAttribute, MethodInfo inInfo)
            {
                Id = inAttribute.Id;
                Attribute = inAttribute;
                Method = inInfo;

                // Static methods with named parent classes get concatenated names
                if (Method.IsStatic)
                {
                    ExposedAttribute containingType = Reflect.GetAttribute<ExposedAttribute>(Method.DeclaringType);
                    if (containingType != null)
                    {
                        containingType.AssignId(Method.DeclaringType);
                        string fullName = string.Join(".", containingType.Name, Attribute.Name);
                        Id = fullName;
                    }
                }
            }

            public bool TryProcess(MethodCache<TAttr> inParent)
            {
                return MethodInvocationHelper.TryCreate(Method, inParent.m_StringConverter, out Invoker);
            }

            public bool TryInvoke(object inTarget, StringSlice inArguments, IStringConverter inConverter, object inContext, out NonBoxedValue outReturnValue)
            {
                return Invoker.TryInvoke(inTarget, inArguments, inConverter, inContext, out outReturnValue);
            }
        }

        #endregion // Types

        private readonly Dictionary<Type, TypeDescription> m_Types;
        private Dictionary<StringHash32, MethodDescription> m_StaticMethods;
        private List<object> m_RelatedObjectCachedList;
        private List<Component> m_RelatedComponentCachedList;
        private Type m_ComponentType;
        private IStringConverter m_StringConverter;
        private bool m_CachingStaticAsyncLock;

        public MethodCache()
            : this(typeof(MonoBehaviour), DefaultStringConverter.Instance)
        {
        }

        public MethodCache(Type inComponentType, IStringConverter inConverter)
        {
            if (inConverter == null)
                throw new ArgumentNullException("inConverter");
            if (inComponentType == null)
                throw new ArgumentNullException("inComponentType");
            
            m_Types = new Dictionary<Type, TypeDescription>();
            m_StringConverter = inConverter;

            m_ComponentType = inComponentType;
        }

        /// <summary>
        /// The string converter.
        /// </summary>
        public IStringConverter StringConverter
        {
            get { return m_StringConverter; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_StringConverter = value;
            }
        }

        #region Types

        /// <summary>
        /// Generates/retrieves the info for the given type.
        /// </summary>
        public TypeDescription Cache(Type inType)
        {
            return GetTypeDescription(inType, true);
        }

        private TypeDescription GetTypeDescription(Type inType, bool inbCreate)
        {
            TypeDescription desc;
            if (!m_Types.TryGetValue(inType, out desc) && inbCreate)
            {
                desc = CreateDescription(inType);
                if (!desc.TryProcess(this))
                    desc = null;
                
                m_Types.Add(inType, desc);
            }
            return desc;
        }

        private void CacheStatic(IEnumerable<Assembly> inAssemblies)
        {
            if (m_StaticMethods != null && !m_CachingStaticAsyncLock)
                return;

            m_StaticMethods = new Dictionary<StringHash32, MethodDescription>(32);
            inAssemblies = inAssemblies ?? Reflect.FindAllUserAssemblies();
            m_CachingStaticAsyncLock = false;

            foreach (var attrPair in Reflect.FindMethods<TAttr>(inAssemblies, StaticAttributeSearch, false))
            {
                attrPair.Attribute.AssignId(attrPair.Info);

                if (m_StaticMethods.ContainsKey(attrPair.Attribute.Id))
                {
                    UnityEngine.Debug.LogErrorFormat("[MethodCache] Multiple instances of static method with id '{0}' found", attrPair.Attribute.Id.ToDebugString());
                    continue;
                }

                MethodDescription desc = CreateDescription(attrPair.Attribute, attrPair.Info);
                if (!desc.TryProcess(this))
                {
                    UnityEngine.Debug.LogErrorFormat("[MethodCache] Static method '{0}' on type '{1}' is incompatible", desc.Id.ToDebugString(), desc.Method.DeclaringType.FullName);
                    continue;
                }

                m_StaticMethods.Add(desc.Id, desc);
            }
        }

        private void CacheStaticFromSet(SerializedAttributeSet inAttributeSet)
        {
            if (m_StaticMethods != null && !m_CachingStaticAsyncLock)
                return;

            m_StaticMethods = new Dictionary<StringHash32, MethodDescription>(32);
            m_CachingStaticAsyncLock = false;

            foreach (var attrPair in inAttributeSet.Read<TAttr>())
            {
                MethodInfo method = attrPair.Info as MethodInfo;
                if (method == null || !method.IsStatic)
                {
                    continue;
                }

                attrPair.Attribute.AssignId(attrPair.Info);

                if (m_StaticMethods.ContainsKey(attrPair.Attribute.Id))
                {
                    UnityEngine.Debug.LogErrorFormat("[MethodCache] Multiple instances of static method with id '{0}' found", attrPair.Attribute.Id.ToDebugString());
                    continue;
                }

                MethodDescription desc = CreateDescription(attrPair.Attribute, method);
                if (!desc.TryProcess(this))
                {
                    UnityEngine.Debug.LogErrorFormat("[MethodCache] Static method '{0}' on type '{1}' is incompatible", desc.Id.ToDebugString(), desc.Method.DeclaringType.FullName);
                    continue;
                }

                m_StaticMethods.Add(desc.Id, desc);
            }
        }

        private IEnumerator CacheStaticAsync(IEnumerable<Assembly> inAssemblies)
        {
            if (m_StaticMethods != null || m_CachingStaticAsyncLock)
                yield break;

            m_StaticMethods = new Dictionary<StringHash32, MethodDescription>(32);
            inAssemblies = inAssemblies ?? Reflect.FindAllUserAssemblies();
            
            m_CachingStaticAsyncLock = true;

            foreach (var attrPair in Reflect.FindMethods<TAttr>(inAssemblies, StaticAttributeSearch, false))
            {
                if (!m_CachingStaticAsyncLock)
                {
                    yield break; // cancelled externally
                }

                attrPair.Attribute.AssignId(attrPair.Info);

                if (m_StaticMethods.ContainsKey(attrPair.Attribute.Id))
                {
                    UnityEngine.Debug.LogErrorFormat("[MethodCache] Multiple instances of static method with id '{0}' found", attrPair.Attribute.Id.ToDebugString());
                    yield return null;

                    if (!m_CachingStaticAsyncLock)
                    {
                        yield break; // cancelled externally
                    }

                    continue;
                }

                MethodDescription desc = CreateDescription(attrPair.Attribute, attrPair.Info);
                if (!desc.TryProcess(this))
                {
                    UnityEngine.Debug.LogErrorFormat("[MethodCache] Static method '{0}' on type '{1}' is incompatible", desc.Id.ToDebugString(), desc.Method.DeclaringType.FullName);
                    yield return null;

                    if (!m_CachingStaticAsyncLock)
                    {
                        yield break; // cancelled externally
                    }

                    continue;
                }

                m_StaticMethods.Add(desc.Id, desc);
                yield return null;

                if (!m_CachingStaticAsyncLock)
                {
                    yield break; // cancelled externally
                }
            }

            m_CachingStaticAsyncLock = false;
        }

        private IEnumerator CacheStaticFromSetAsync(SerializedAttributeSet inAttributeSet)
        {
            if (m_StaticMethods != null || m_CachingStaticAsyncLock)
                yield break;

            m_StaticMethods = new Dictionary<StringHash32, MethodDescription>(32);
            m_CachingStaticAsyncLock = true;

            foreach (var attrPair in inAttributeSet.Read<TAttr>())
            {
                if (!m_CachingStaticAsyncLock)
                {
                    yield break; // cancelled externally
                }

                MethodInfo method = attrPair.Info as MethodInfo;
                if (method == null || !method.IsStatic)
                {
                    yield return null;
                    if (!m_CachingStaticAsyncLock)
                    {
                        yield break; // cancelled externally
                    }
                    continue;
                }

                attrPair.Attribute.AssignId(attrPair.Info);

                if (m_StaticMethods.ContainsKey(attrPair.Attribute.Id))
                {
                    UnityEngine.Debug.LogErrorFormat("[MethodCache] Multiple instances of static method with id '{0}' found", attrPair.Attribute.Id.ToDebugString());
                    yield return null;
                    if (!m_CachingStaticAsyncLock)
                    {
                        yield break; // cancelled externally
                    }
                    continue;
                }

                MethodDescription desc = CreateDescription(attrPair.Attribute, method);
                if (!desc.TryProcess(this))
                {
                    UnityEngine.Debug.LogErrorFormat("[MethodCache] Static method '{0}' on type '{1}' is incompatible", desc.Id.ToDebugString(), desc.Method.DeclaringType.FullName);
                    yield return null;
                    if (!m_CachingStaticAsyncLock)
                    {
                        yield break; // cancelled externally
                    }
                    continue;
                }

                m_StaticMethods.Add(desc.Id, desc);
                yield return null;
                if (!m_CachingStaticAsyncLock)
                {
                    yield break; // cancelled externally
                }
            }

            m_CachingStaticAsyncLock = false;
        }

        #endregion // Types

        #region Methods

        /// <summary>
        /// Locates the method for the given object and id.
        /// </summary>
        public MethodDescription FindMethod(object inTarget, StringHash32 inMethodId)
        {
            if (inTarget == null)
                return null;

            Type t = inTarget.GetType();
            TypeDescription typeDesc;
            MethodDescription methodDesc;
            while(t != null && ShouldCheck(t))
            {
                typeDesc = Cache(t);
                if (typeDesc.TryGetMethod(inMethodId, out methodDesc))
                    return methodDesc;

                t = t.BaseType;
            }

            return null;
        }

        /// <summary>
        /// Locates a method for the given object and id.
        /// Can redirect to related objects (i.e. components)
        /// </summary>
        public MethodDescription FindMethodWithRedirect(ref object ioTarget, StringHash32 inMethodId)
        {
            if (ioTarget == null)
                return null;

            MethodDescription desc = FindMethod(ioTarget, inMethodId);
            if (desc != null)
                return desc;

            object toIgnore = ioTarget;
            GameObject go = GetGameObject(ioTarget);
            if (!go.IsReferenceNull())
            {
                GatherComponents(go, m_RelatedComponentCachedList ?? (m_RelatedComponentCachedList = new List<Component>(8)));
                for(int i = 0, len = m_RelatedComponentCachedList.Count; i < len && desc == null; ++i)
                {
                    ioTarget = m_RelatedComponentCachedList[i];
                    if (ioTarget != toIgnore)
                        desc = FindMethod(ioTarget, inMethodId);
                }
                m_RelatedComponentCachedList.Clear();
            }
            else
            {
                GatherRelatedObjects(ioTarget, m_RelatedObjectCachedList ?? (m_RelatedObjectCachedList = new List<object>(8)));
                for(int i = 0, len = m_RelatedObjectCachedList.Count; i < len && desc == null; ++i)
                {
                    ioTarget = m_RelatedObjectCachedList[i];
                    if (ioTarget != toIgnore)
                        desc = FindMethod(ioTarget, inMethodId);
                }
            }

            return desc;
        }

        /// <summary>
        /// Locates the static method for the given id.
        /// </summary>
        public MethodDescription FindStaticMethod(StringHash32 inMethodId)
        {
            CacheStatic(null);

            MethodDescription description;
            m_StaticMethods.TryGetValue(inMethodId, out description);
            return description;
        }

        static private bool ShouldCheck(Type inType)
        {
            return inType != typeof(object) && inType != typeof(MonoBehaviour) && inType != typeof(ScriptableObject);
        }

        private GameObject GetGameObject(object inObject)
        {
            GameObject go = inObject as GameObject;
            if (go.IsReferenceNull())
            {
                Component c = inObject as Component;
                if (!c.IsReferenceNull())
                    go = c.gameObject;
            }

            return go;
        }

        /// <summary>
        /// Gathers all related objects.
        /// </summary>
        protected virtual void GatherRelatedObjects(object inObject, List<object> outObjects)
        {
        }

        /// <summary>
        /// Gathers all components on the given GameObject.
        /// </summary>
        protected virtual void GatherComponents(GameObject inGameObject, List<Component> outObjects)
        {
            inGameObject.GetComponents(m_ComponentType, outObjects);
        }

        #endregion // Methods

        #region Invoke

        /// <summary>
        /// Attempts to invoke a static method.
        /// </summary>
        public bool TryStaticInvoke(StringHash32 inId, StringSlice inArguments, object inContext, out NonBoxedValue outResult)
        {
            var method = FindStaticMethod(inId);
            if (method == null)
            {
                outResult = default(NonBoxedValue);
                return false;
            }

            return method.TryInvoke(null, inArguments, m_StringConverter, inContext, out outResult);
        }

        /// <summary>
        /// Attempts to invoke an instance method.
        /// </summary>
        public bool TryInvoke(object inTarget, StringHash32 inId, StringSlice inArguments, object inContext, out NonBoxedValue outResult)
        {
            var method = FindMethodWithRedirect(ref inTarget, inId);
            if (method == null)
            {
                outResult = default(NonBoxedValue);
                return false;
            }

            return method.TryInvoke(inTarget, inArguments, m_StringConverter, inContext, out outResult);
        }

        #endregion // Invoke

        #region Creation

        protected virtual TypeDescription CreateDescription(Type inType)
        {
            return new TypeDescription(inType);
        }

        protected virtual MethodDescription CreateDescription(TAttr inAttribute, MethodInfo inInfo)
        {
            return new MethodDescription(inAttribute, inInfo);
        }

        #endregion // Creation

        #region IMethodCache

        IStringConverter IMethodCache.StringConverter { get { return m_StringConverter; } }

        /// <summary>
        /// Loads info for the given type.
        /// </summary>
        public void Load(Type inType)
        {
            Cache(inType);
        }

        /// <summary>
        /// Loads info for static methods from default assemblies.
        /// </summary>
        public void LoadStatic()
        {
            CacheStatic(null);
        }

        /// <summary>
        /// Loads info for static methods from the given attribute set.
        /// </summary>
        public void LoadStaticFromSet(SerializedAttributeSet inAttributeSet)
        {
            CacheStaticFromSet(inAttributeSet);
        }

        /// <summary>
        /// Loads info for static methods from the given assemblies.
        /// </summary>
        public void LoadStatic(IEnumerable<Assembly> inAssemblies)
        {
            CacheStatic(inAssemblies);
        }

        /// <summary>
        /// Loads info for static methods from default assemblies.
        /// </summary>
        public IEnumerator LoadStaticAsync()
        {
            return CacheStaticAsync(null);
        }

        /// <summary>
        /// Loads info for static methods from the given attribute set.
        /// </summary>
        public IEnumerator LoadStaticFromSetAsync(SerializedAttributeSet inAttributeSet)
        {
            return CacheStaticFromSetAsync(inAttributeSet);
        }

        /// <summary>
        /// Loads info for static methods from the given assemblies.
        /// Returns an IEnumerator to actually process the static methods.
        /// </summary>
        public IEnumerator LoadStaticAsync(IEnumerable<Assembly> inAssemblies)
        {
            return CacheStaticAsync(inAssemblies);
        }

        /// <summary>
        /// Returns if the given method exists, either static or instance.
        /// </summary>
        public bool Has(StringHash32 inId)
        {
            return HasStatic(inId) || HasInstance(inId);
        }

        /// <summary>
        /// Returns if a given static method exists.
        /// </summary>
        public bool HasStatic(StringHash32 inId)
        {
            return m_StaticMethods.ContainsKey(inId);
        }

        /// <summary>
        /// Returns if the given instance method exists.
        /// </summary>
        public bool HasInstance(StringHash32 inId)
        {
            foreach(var info in m_Types.Values)
            {
                if (info.TryGetMethod(inId, out MethodDescription _))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion // IMethodCache
    }
}