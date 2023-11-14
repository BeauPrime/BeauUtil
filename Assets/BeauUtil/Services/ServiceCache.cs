/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    25 Jan 2021
 * 
 * File:    ServiceCache.cs
 * Purpose: Cache for services.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeauUtil.Services
{
    /// <summary>
    /// Cache of services.
    /// Will attempt to resolve dependencies when initializing jobs.
    /// </summary>
    public sealed class ServiceCache : IServiceProvider
    {
        #region Consts

        static private readonly Type[] IgnoredTypes = new Type[] { typeof(object), typeof(IDisposable), typeof(MonoBehaviour), typeof(ScriptableObject) };
        static private readonly string[] IgnoredTypePatterns = new string[] { "IComparable*", "IEquatable*", "IEnumerable*" };

        #endregion // Consts

        #region Types

        private class TypeCache
        {
            private readonly Type m_Type;
            private readonly bool m_SupportsMultiple;
            private readonly Type[] m_Dependencies;
            private readonly Type[] m_BaseCacheTypes;

            private IService m_First;
            private readonly List<IService> m_All;

            public TypeCache(Type inType, Type[] inDependencies, Type[] inBaseCacheTypes)
            {
                m_Type = inType;
                m_SupportsMultiple = !IsConcrete(inType);
                m_Dependencies = inDependencies;
                m_BaseCacheTypes = inBaseCacheTypes;

                if (m_SupportsMultiple)
                    m_All = new List<IService>(4);
                else
                    m_All = new List<IService>(1);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Has() { return m_First != null; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public IService Get() { return m_First; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public IReadOnlyList<IService> All() { return m_All; }

            public IEnumerable<T> All<T>()
            {
                for(int i = 0, len = m_All.Count; i < len; ++i)
                    yield return (T) m_All[i];
            }
            
            public Type[] Dependencies() { return m_Dependencies; }
            public Type[] BaseCacheTypes() { return m_BaseCacheTypes; }

            #region Collection

            public void Add(IService inService)
            {
                if (m_First == null)
                {
                    m_First = inService;
                    m_All.Add(inService);
                }
                else
                {
                    if (m_SupportsMultiple)
                        m_All.Add(inService);
                    else
                        throw new ServiceAlreadyRegisteredException(m_Type);
                }
            }

            public void Remove(IService inService)
            {
                if (m_All.FastRemove(inService))
                {
                    if (m_First == inService)
                    {
                        m_First = m_SupportsMultiple && m_All.Count > 0 ? m_All[0] : null;
                    }
                }
            }
        
            #endregion // Collection
        }

        #endregion // Types

        private readonly HashSet<IService> m_RegisteredServices = new HashSet<IService>();
        private readonly RingBuffer<IService> m_AddQueue = new RingBuffer<IService>();
        private readonly RingBuffer<IService> m_RemoveStack = new RingBuffer<IService>();
        private readonly Dictionary<Type, TypeCache> m_ServiceLookup = new Dictionary<Type, TypeCache>();
        
        private bool m_ScannedStaticMembers;
        private readonly List<PropertyInfo> m_StaticServiceProperties = new List<PropertyInfo>();
        private readonly List<FieldInfo> m_StaticServiceFields = new List<FieldInfo>();
        private readonly InjectionCache<ServiceReferenceAttribute> m_ServiceInjectionCache = new InjectionCache<ServiceReferenceAttribute>();

        #region Operations

        /// <summary>
        /// Adds the service.
        /// </summary>
        public bool Add(IService inService)
        {
            if (inService == null)
                throw new ArgumentNullException("inService");

            if (m_RemoveStack.FastRemove(inService) || m_RegisteredServices.Contains(inService) || m_AddQueue.Contains(inService))
                return false;

            m_AddQueue.PushBack(inService);
            return true;
        }

        /// <summary>
        /// Removes the service.
        /// </summary>
        public bool Remove(IService inService)
        {
            if (inService == null)
                throw new ArgumentNullException("inService");

            if (m_AddQueue.FastRemove(inService) || !m_RegisteredServices.Contains(inService) || m_RemoveStack.Contains(inService))
                return false;

            m_RemoveStack.PushBack(inService);
            return true;
        }

        /// <summary>
        /// Clears all services.
        /// </summary>
        public void ClearAll()
        {
            m_AddQueue.Clear();

            foreach(var service in m_RegisteredServices)
            {
                Remove(service);
            }

            if (HasPendingOperations())
            {
                Process();
            }
        }

        /// <summary>
        /// Returns if the service cache has any pending operations.
        /// </summary>
        public bool HasPendingOperations()
        {
            return m_AddQueue.Count > 0 || m_RemoveStack.Count > 0;
        }

        /// <summary>
        /// Processes all pending operations.
        /// </summary>
        public void Process()
        {
            ScanForStaticInjection();

            ProcessRemoveStack();
            ProcessAddQueue();
        }

        #endregion // Operations

        #region Access

        /// <summary>
        /// Returns the service for the given type.
        /// </summary>
        public IService Get(Type inServiceType)
        {
            TypeCache cache;
            if (m_ServiceLookup.TryGetValue(inServiceType, out cache))
                return cache.Get();
            return null;
        }

        /// <summary>
        /// Returns all services for the given type.
        /// </summary>
        public IReadOnlyList<IService> All(Type inServiceType)
        {
            TypeCache cache;
            if (m_ServiceLookup.TryGetValue(inServiceType, out cache))
                return cache.All();
            return null;
        }

        /// <summary>
        /// Returns the service for the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>()
        {
            return (T) Get(typeof(T));
        }

        /// <summary>
        /// Returns all services for the given type.
        /// </summary>
        public IEnumerable<T> All<T>()
        {
            TypeCache cache;
            if (m_ServiceLookup.TryGetValue(typeof(T), out cache))
                return cache.All<T>();
            return null;
        }

        #endregion // Access

        #region Process

        private void ProcessAddQueue()
        {
            IService service;
            Type serviceType;
            int skipCount = 0;
            int initialQueueSize = m_AddQueue.Count;

            while(m_AddQueue.Count > 0)
            {
                service = m_AddQueue.PopFront();
                serviceType = service.GetType();

                if (m_RegisteredServices.Contains(service))
                {
                    skipCount--;
                    continue;
                }

                // Debug.LogFormat("[ServiceCache] Attempting to add service '{0}'", serviceType.Name);

                if (!HasSatisfiedDependencies(serviceType))
                {
                    m_AddQueue.PushBack(service);

                    if (++skipCount >= initialQueueSize)
                        throw new UnresolvableDependenciesException(serviceType);
                    
                    continue;
                }

                RegisterService(service);
                skipCount = 0;
            }
        }

        private void ProcessRemoveStack()
        {
            IService service;
            Type serviceType;

            while(m_RemoveStack.Count > 0)
            {
                service = m_RemoveStack.PeekFront();
                serviceType = service.GetType();

                if (!m_RegisteredServices.Contains(service))
                {
                    m_RemoveStack.PopFront();
                    continue;
                }

                // Debug.LogFormat("[ServiceCache] Attempting to remove service '{0}'", serviceType.Name);

                if (AddDependenciesToRemoveStack(serviceType))
                    continue;

                DeregisterService(service);
                m_RemoveStack.PopFront();
            }
        }

        private bool HasSatisfiedDependencies(Type inType)
        {
            TypeCache cache = GetCache(inType, true);
            var dependencies = cache.Dependencies();
            if (dependencies == null || dependencies.Length == 0)
                return true;

            TypeCache depCache;
            foreach(var type in dependencies)
            {
                depCache = GetCache(type, false);
                if (depCache == null || !depCache.Has())
                    return false;
            }

            return true;
        }

        private bool AddDependenciesToRemoveStack(Type inType)
        {
            bool bHasRemainingDependencies = false;
            Type serviceType;
            TypeCache cache;
            foreach(var service in m_RegisteredServices)
            {
                serviceType = service.GetType();
                
                if (serviceType == inType)
                    continue;

                cache = GetCache(serviceType, true);
                var depArr = cache.Dependencies();
                if (depArr != null && Array.IndexOf(depArr, inType) >= 0)
                {
                    m_RemoveStack.FastRemove(service);
                    m_RemoveStack.PushFront(service);
                    bHasRemainingDependencies = true;
                }
            }

            return bHasRemainingDependencies;
        }

        private bool HasDependency(Type inType, Type inOnType)
        {
            TypeCache cache = GetCache(inType, true);
            var dependencies = cache.Dependencies();
            if (dependencies == null || dependencies.Length == 0)
                return false;

            return Array.IndexOf(dependencies, inOnType) >= 0;
        }

        #endregion // Process

        #region Unity

        /// <summary>
        /// Gathers and adds all active services from the given root.
        /// </summary>
        public int AddFromHierarchy(Transform inRoot)
        {
            int count = 0;

            foreach(var service in inRoot.GetComponentsInChildren<IService>())
            {
                if (Add(service))
                    ++count;
            }

            return count;
        }

        /// <summary>
        /// Gathers and adds all active services from the given scene.
        /// </summary>
        public int AddFromScene(Scene inScene)
        {
            int count = 0;

            GameObject[] roots = inScene.GetRootGameObjects();

            foreach(var root in roots)
            {
                count += AddFromHierarchy(root.transform);
            }

            return count;
        }

        /// <summary>
        /// Gathers and adds all active services from the given scene.
        /// </summary>
        public int RemoveFromScene(Scene inScene)
        {
            int count = 0;

            foreach(var service in m_RegisteredServices)
            {
                if (FromScene(service, inScene))
                {
                    Remove(service);
                    ++count;
                }
            }

            for(int i = m_AddQueue.Count - 1; i >= 0; --i)
            {
                if (FromScene(m_AddQueue[i], inScene))
                {
                    m_AddQueue.FastRemoveAt(i);
                }
            }

            return count;
        }

        static private bool FromScene(IService inService, Scene inScene)
        {
            MonoBehaviour behaviour = inService as MonoBehaviour;
            if (behaviour != null)
            {
                return behaviour.gameObject.scene == inScene;
            }

            return false;
        }

        #endregion // Unity

        #region Register/Deregister

        private void RegisterService(IService inService)
        {
            m_RegisteredServices.Add(inService);

            Type serviceType = inService.GetType();
            TypeCache selfCache = GetCache(serviceType, true);
            selfCache.Add(inService);

            var linked = selfCache.BaseCacheTypes();
            if (linked != null)
            {
                foreach(var type in linked)
                {
                    GetCache(type, true).Add(inService);
                }
            }

            InjectStaticReferences(inService, serviceType);
            InjectReferences(inService);
            inService.InitializeService();
        }

        private void DeregisterService(IService inService)
        {
            m_RegisteredServices.Remove(inService);

            Type serviceType = inService.GetType();
            TypeCache selfCache = GetCache(serviceType, false);
            if (selfCache != null)
            {
                selfCache.Remove(inService);

                var linked = selfCache.BaseCacheTypes();
                if (linked != null)
                {
                    foreach(var type in linked)
                    {
                        GetCache(type, false)?.Remove(inService);
                    }
                }

                ClearStaticReferences(inService, serviceType);
                inService.ShutdownService();
            }
        }

        #endregion // Register/Deregister

        #region Types

        private TypeCache GetCache(Type inType, bool inbCreate)
        {
            TypeCache cache;
            if (!m_ServiceLookup.TryGetValue(inType, out cache) && inbCreate)
            {
                Type[] dependencies = null;
                Type[] cacheTypes = null;
                if (IsConcrete(inType))
                {
                    GatherLinkedTypes(inType, out dependencies, out cacheTypes);
                }

                cache = new TypeCache(inType, dependencies, cacheTypes);
                m_ServiceLookup.Add(inType, cache);
            }

            return cache;
        }

        private void GatherLinkedTypes(Type inType, out Type[] outDependencies, out Type[] outCacheTypes)
        {
            HashSet<Type> dependencies = new HashSet<Type>();
            HashSet<Type> cacheTypes = new HashSet<Type>();

            foreach(var serviceDep in Reflect.GetAttributes<ServiceDependencyAttribute>(inType, true))
            {
                foreach(var type in serviceDep.Dependencies)
                {
                    dependencies.Add(type);
                }
            }

            foreach(var type in Reflect.GetBaseTypes(inType))
            {
                if (ShouldIgnoreType(type))
                    break;

                if (type != inType)
                {
                    cacheTypes.Add(type);
                }

                foreach(var dependency in m_ServiceInjectionCache.Get(type))
                {
                    Type depType = Reflect.GetValueType(dependency.Info);
                    if (!dependency.Attribute.Optional)
                        dependencies.Add(depType);
                }
            }

            foreach(var _interface in inType.GetInterfaces())
            {
                if (ShouldIgnoreType(_interface))
                    continue;

                cacheTypes.Add(_interface);
            }

            outDependencies = ToArray(dependencies);
            outCacheTypes = ToArray(cacheTypes);
        }

        static private bool ShouldIgnoreType(Type inType)
        {
            if (Array.IndexOf(IgnoredTypes, inType) >= 0)
                return true;

            if (WildcardMatch.Match(inType.Name, IgnoredTypePatterns))
                return true;

            return false;
        }

        static private bool IsConcrete(Type inType)
        {
            return !inType.IsInterface && !inType.IsAbstract;
        }

        static private Type[] ToArray(HashSet<Type> inSet)
        {
            if (inSet == null || inSet.Count == 0)
                return null;

            Type[] arr = new Type[inSet.Count];
            inSet.CopyTo(arr);
            return arr;
        }

        #endregion // Types

        #region Injections

        private void ScanForStaticInjection()
        {
            if (m_ScannedStaticMembers)
                return;

            foreach(var member in Reflect.FindMembers<ServiceReferenceAttribute>(Reflect.FindAllUserAssemblies(), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                FieldInfo field = member.Info as FieldInfo;
                if (field != null)
                {
                    m_StaticServiceFields.Add(field);
                    continue;
                }

                PropertyInfo property = member.Info as PropertyInfo;
                if (property != null)
                {
                    m_StaticServiceProperties.Add(property);
                    continue;
                }
            }

            m_ScannedStaticMembers = true;
        }

        private void ScanForStaticInjectionFromSet(SerializedAttributeSet attributeSet)
        {
            if (m_ScannedStaticMembers)
                return;

            foreach (var member in attributeSet.Read<ServiceReferenceAttribute>())
            {
                FieldInfo field = member.Info as FieldInfo;
                if (field != null)
                {
                    m_StaticServiceFields.Add(field);
                    continue;
                }

                PropertyInfo property = member.Info as PropertyInfo;
                if (property != null)
                {
                    m_StaticServiceProperties.Add(property);
                    continue;
                }
            }

            m_ScannedStaticMembers = true;
        }

        private void InjectStaticReferences(IService inService, Type inServiceType)
        {
            foreach(var field in m_StaticServiceFields)
            {
                if (field.FieldType.IsAssignableFrom(inServiceType))
                {
                    field.SetValue(null, inService);
                }
            }

            foreach(var prop in m_StaticServiceProperties)
            {
                if (prop.PropertyType.IsAssignableFrom(inServiceType))
                {
                    prop.SetValue(null, inService);
                }
            }
        }

        private void ClearStaticReferences(IService inService, Type inServiceType)
        {
            foreach(var field in m_StaticServiceFields)
            {
                if (field.GetValue(null) == inService)
                {
                    field.SetValue(null, null);
                }
            }

            foreach(var prop in m_StaticServiceProperties)
            {
                if (prop.GetValue(null) == inService)
                {
                    prop.SetValue(null, null);
                }
            }
        }

        /// <summary>
        /// Injects service references into the given object.
        /// </summary>
        public void InjectReferences(object inObject)
        {
            if (inObject == null)
                return;

            Type t = inObject.GetType();
            while(t != null)
            {
                foreach(var member in m_ServiceInjectionCache.Get(t))
                {
                    Type serviceType = Reflect.GetValueType(member.Info);
                    IService service = Get(serviceType);
                    if (service != null)
                    {
                        Reflect.SetValue(member.Info, inObject, service);
                    }
                    else
                    {
                        if (!member.Attribute.Optional)
                            throw new ServiceNotRegisteredException(serviceType);
                    }
                }
                t = t.BaseType;
            }
        }

        #endregion // Injections

        #region IServiceProvider

        object IServiceProvider.GetService(Type serviceType)
        {
            return Get(serviceType);
        }

        #endregion // IServiceProvider

        #region Exceptions

        private class ServiceAlreadyRegisteredException : Exception
        {
            public readonly Type ServiceType;

            public ServiceAlreadyRegisteredException(Type inType)
                : base(string.Format("A different service of concrete type '{0}' has already been registered", inType.Name))
            {
                ServiceType = inType;
            }
        }

        private class ServiceNotRegisteredException : Exception
        {
            public readonly Type ServiceType;

            public ServiceNotRegisteredException(Type inType)
                : base(string.Format("A service of type '{0}' is not registered", inType.Name))
            {
                ServiceType = inType;
            }
        }

        private class UnresolvableDependenciesException : Exception
        {
            public readonly Type ServiceType;

            public UnresolvableDependenciesException(Type inType)
                : base(string.Format("Circular or unresolvable dependencies detected around '{0}'", inType.Name))
            {
                ServiceType = inType;
            }
        }

        #endregion // Exceptions
    }
}