/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    UnityHelper.cs
 * Purpose: Unity-specific helper functions.
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Contains Unity-specific utility functions.
    /// </summary>
    static public class UnityHelper
    {
        #region Null Helper

        /// <summary>
        /// Returns if the given object is null by reference.
        /// Avoids calling Unity's overridden equality operator.
        /// </summary>
        [MethodImpl(256)]
        static public bool IsReferenceNull(this UnityEngine.Object inObject)
        {
            return System.Object.ReferenceEquals(inObject, null);
        }

        /// <summary>
        /// Returns if the given object is equal by reference to another object.
        /// Avoids calling Unity's overridden equality operator.
        /// </summary>
        [MethodImpl(256)]
        static public bool IsReferenceEquals(this UnityEngine.Object inObject, UnityEngine.Object inOther)
        {
            return System.Object.ReferenceEquals(inObject, inOther);
        }

        /// <summary>
        /// Returns if the given object reference was destroyed.
        /// This occurs if you have a hanging reference to an object that was destroyed.
        /// </summary>
        [MethodImpl(256)]
        static public bool IsReferenceDestroyed(this UnityEngine.Object inObject)
        {
            return !System.Object.ReferenceEquals(inObject, null) && !inObject;
        }

        #endregion // Null Helper

        #region SafeDestroy

        /// <summary>
        /// Safely disposes of a Unity object and sets the reference to null.
        /// </summary>
        static public void SafeDestroy<T>(ref T ioObject) where T : UnityEngine.Object
        {
            // This is to avoid calling Unity's overridden equality operator
            if (object.ReferenceEquals(ioObject, null))
                return;

            // This is to see if the object hasn't been destroyed yet
            if (ioObject)
            {
                UnityEngine.Object.Destroy(ioObject);
            }

            ioObject = null;
        }

        /// <summary>
        /// Safely disposes of the GameObject and sets
        /// the reference to null.
        /// </summary>
        static public void SafeDestroyGO(ref GameObject ioGameObject)
        {
            // This is to avoid calling Unity's overridden equality operator
            if (object.ReferenceEquals(ioGameObject, null))
                return;

            // This is to see if the object hasn't been destroyed yet
            if (ioGameObject)
            {
                UnityEngine.Object.Destroy(ioGameObject);
            }

            ioGameObject = null;
        }

        /// <summary>
        /// Safely disposes of the parent GameObject of the transform and sets
        /// the reference to null.
        /// </summary>
        static public void SafeDestroyGO(ref Transform ioTransform)
        {
            // This is to avoid calling Unity's overridden equality operator
            if (object.ReferenceEquals(ioTransform, null))
                return;

            // This is to see if the object hasn't been destroyed yet
            if (ioTransform && ioTransform.gameObject)
            {
                UnityEngine.Object.Destroy(ioTransform.gameObject);
            }

            ioTransform = null;
        }

        /// <summary>
        /// Safely disposes of the parent GameObject of the component and sets
        /// the reference to null.
        /// </summary>
        static public void SafeDestroyGO<T>(ref T ioComponent) where T : Component
        {
            // This is to avoid calling Unity's overridden equality operator
            if (object.ReferenceEquals(ioComponent, null))
                return;

            // This is to see if the object hasn't been destroyed yet
            if (ioComponent && ioComponent.gameObject)
            {
                UnityEngine.Object.Destroy(ioComponent.gameObject);
            }

            ioComponent = null;
        }

        #endregion // SafeDestroy

        #region GetComponentsInChildren

        /// <summary>
        /// Recursively retrieves components in children.
        /// If a child has a component of this type, its children will not be searched.
        /// </summary>
        static public void GetImmediateComponentsInChildren<T>(this GameObject inGameObject, bool inbIncludeSelf, List<T> outComponents) where T : Component
        {
            GetImmediateComponentsInChildren<T>(inGameObject.transform, inbIncludeSelf, false, outComponents);
        }

        /// <summary>
        /// Recursively retrieves components in children.
        /// If a child has a component of this type, its children will not be searched.
        /// </summary>
        static public void GetImmediateComponentsInChildren<T>(this GameObject inGameObject, bool inbIncludeSelf, bool inbIncludeInactive, List<T> outComponents) where T : Component
        {
            GetImmediateComponentsInChildren<T>(inGameObject.transform, inbIncludeSelf, inbIncludeInactive, outComponents);
        }

        /// <summary>
        /// Recursively retrieves components in children.
        /// If a child has a component of this type, its children will not be searched.
        /// </summary>
        static public void GetImmediateComponentsInChildren<T>(this Component inGameObject, bool inbIncludeSelf, List<T> outComponents) where T : Component
        {
            GetImmediateComponentsInChildren<T>(inGameObject.transform, inbIncludeSelf, false, outComponents);
        }

        /// <summary>
        /// Recursively retrieves components in children.
        /// If a child has a component of this type, its children will not be searched.
        /// </summary>
        static public void GetImmediateComponentsInChildren<T>(this Component inGameObject, bool inbIncludeSelf, bool inbIncludeInactive, List<T> outComponents) where T : Component
        {
            GetImmediateComponentsInChildren<T>(inGameObject.transform, inbIncludeSelf, inbIncludeInactive, outComponents);
        }

        /// <summary>
        /// Recursively retrieves components in children.
        /// If a child has a component of this type, its children will not be searched.
        /// </summary>
        static public void GetImmediateComponentsInChildren<T>(this Transform inTransform, bool inbIncludeSelf, List<T> outComponents) where T : Component
        {
            GetImmediateComponentsInChildren<T>(inTransform, inbIncludeSelf, false, outComponents);
        }

        /// <summary>
        /// Recursively retrieves components in children.
        /// If a child has a component of this type, its children will not be searched.
        /// </summary>
        static public void GetImmediateComponentsInChildren<T>(this Transform inTransform, bool inbIncludeSelf, bool inbIncludeInactive, List<T> outComponents) where T : Component
        {
            if (!inbIncludeInactive && !inTransform.gameObject.activeInHierarchy)
                return;

            if (inbIncludeSelf)
            {
                if (s_CachedComponentList == null)
                    s_CachedComponentList = new List<Component>();
                inTransform.GetComponents(typeof(T), s_CachedComponentList);
                if (s_CachedComponentList.Count > 0)
                {
                    for (int i = 0; i < s_CachedComponentList.Count; ++i)
                        outComponents.Add((T) s_CachedComponentList[i]);
                    s_CachedComponentList.Clear();
                    return;
                }
            }

            int childCount = inTransform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                GetImmediateComponentsInChildren<T>(inTransform.GetChild(i), true, inbIncludeInactive, outComponents);
            }
        }

        [ThreadStatic]
        static private List<Component> s_CachedComponentList;

        #endregion // GetComponentsInChildren
    
        #region Components

        static public T CacheComponent<T>(this GameObject inGameObject, ref T ioComponent)
        {
            if (object.ReferenceEquals(ioComponent, null))
                ioComponent = inGameObject.GetComponent<T>();
            return ioComponent;
        }

        static public T CacheComponent<T>(this Component inComponent, ref T ioComponent)
        {
            if (object.ReferenceEquals(ioComponent, null))
                ioComponent = inComponent.GetComponent<T>();
            return ioComponent;
        }

        static public Transform CacheComponent(this GameObject inGameObject, ref Transform ioComponent)
        {
            if (object.ReferenceEquals(ioComponent, null))
                ioComponent = inGameObject.transform;
            return ioComponent;
        }

        static public Transform CacheComponent(this Component inComponent, ref Transform ioComponent)
        {
            if (object.ReferenceEquals(ioComponent, null))
                ioComponent = inComponent.transform;
            return ioComponent;
        }

        static public RectTransform CacheComponent(this GameObject inGameObject, ref RectTransform ioComponent)
        {
            if (object.ReferenceEquals(ioComponent, null))
                ioComponent = inGameObject.transform as RectTransform;
            return ioComponent;
        }

        static public RectTransform CacheComponent(this Component inComponent, ref RectTransform ioComponent)
        {
            if (object.ReferenceEquals(ioComponent, null))
                ioComponent = inComponent.transform as RectTransform;
            return ioComponent;
        }

        static public T EnsureComponent<T>(this GameObject inGameObject, ref T ioComponent) where T : Component
        {
            if (IsReferenceNull(ioComponent))
            {
                ioComponent = EnsureComponent<T>(inGameObject);
            }
            return ioComponent;
        }

        static public T EnsureComponent<T>(this Component inComponent, ref T ioComponent) where T : Component
        {
            if (IsReferenceNull(ioComponent))
            {
                ioComponent = EnsureComponent<T>(inComponent.gameObject);
            }
            return ioComponent;
        }

        static public T EnsureComponent<T>(this GameObject inGameObject) where T : Component
        {
            T component = inGameObject.GetComponent<T>();
            if (component == null)
                component = inGameObject.AddComponent<T>();
            return component;
        }

        static public T EnsureComponent<T>(this Component inComponent) where T : Component
        {
            return EnsureComponent<T>(inComponent.gameObject);
        }

        static public T GetComponent<T>(this GameObject inGameObject, ComponentLookupDirection inDirection, bool inbIncludeInactive = false)
        {
            switch(inDirection)
            {
                case ComponentLookupDirection.Self:
                    return inGameObject.GetComponent<T>();

                case ComponentLookupDirection.Parent:
                    return inGameObject.GetComponentInParent<T>();

                case ComponentLookupDirection.Children:
                    return inGameObject.GetComponentInChildren<T>(inbIncludeInactive);

                default:
                    throw new ArgumentOutOfRangeException("inDirection");
            }
        }

        static public T GetComponent<T>(this Component inComponent, ComponentLookupDirection inDirection, bool inbIncludeInactive = false)
        {
            return GetComponent<T>(inComponent.gameObject, inDirection, inbIncludeInactive);
        }

        #endregion // Components
    
        #region Path

        /// <summary>
        /// Returns the full path for this gameObject
        /// </summary>
        static public string FullPath(this GameObject inGameObject, bool inbIncludeScene = false)
        {
            if (!inGameObject)
                return string.Empty;
            
            StringBuilder builder = new StringBuilder();
            if (inbIncludeScene)
            {
                builder.Append(inGameObject.scene.name).Append(":/");
            }
            WritePath(inGameObject.transform, builder);
            return builder.Flush();
        }

        static private void WritePath(Transform inTransform, StringBuilder ioBuilder)
        {
            Transform parent = inTransform.parent;
            if (!parent.IsReferenceNull())
            {
                WritePath(parent, ioBuilder);
                ioBuilder.Append('/');
            }

            ioBuilder.Append(inTransform.name);
        }

        #endregion // Path
    }
}