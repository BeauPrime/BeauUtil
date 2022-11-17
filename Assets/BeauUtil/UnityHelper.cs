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
using System.Reflection;
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
                List<Component> components = s_CachedComponentList ?? (s_CachedComponentList = new List<Component>());
                inTransform.GetComponents(typeof(T), components);
                if (components.Count > 0)
                {
                    for (int i = 0; i < components.Count; ++i)
                        outComponents.Add((T) components[i]);
                    components.Clear();
                    return;
                }
            }

            int childCount = inTransform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                GetImmediateComponentsInChildren<T>(inTransform.GetChild(i), true, inbIncludeInactive, outComponents);
            }
        }

        static private List<Component> s_CachedComponentList; // all operations involving this are locked to main thread

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

        static public int ComponentCount(this GameObject inGameObject)
        {
            List<Component> components = s_CachedComponentList ?? (s_CachedComponentList = new List<Component>());
            inGameObject.GetComponents(typeof(Component), components);
            int count = components.Count;
            components.Clear();
            return count;
        }

        static public int ComponentCount(this Component inComponent)
        {
            List<Component> components = s_CachedComponentList ?? (s_CachedComponentList = new List<Component>());
            inComponent.GetComponents(typeof(Component), components);
            int count = components.Count;
            components.Clear();
            return count;
        }

        static public T GetComponentInParent<T>(this GameObject inGameObject, bool inbIncludeInactive)
        {
            if (!inbIncludeInactive)
                return inGameObject.GetComponentInParent<T>();

            Transform transform = inGameObject.transform;
            T component = default(T);
            while(transform != null && component == null)
            {
                component = transform.GetComponent<T>();
                transform = transform.parent;
            }

            return component;
        }

        static public T GetComponentInParent<T>(this Component inComponent, bool inbIncludeInactive)
        {
            if (!inbIncludeInactive)
                return inComponent.GetComponentInParent<T>();

            Transform transform = inComponent.transform;
            T component = default(T);
            while(transform != null && component == null)
            {
                component = transform.GetComponent<T>();
                transform = transform.parent;
            }

            return component;
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

        #region Render Texture

        /// <summary>
        /// Blits the source texture to the destination in a "best effort pixel perfect" manner.
        /// This will scale up to the nearest integer scale with point filtering, and then bilinear scale up to the final scale.
        /// </summary>
        static public void BlitPixelPerfect(RenderTexture inSrc, RenderTexture inDest, Camera inCamera = null)
        {
            float destHeight;
            if (inDest != null)
            {
                destHeight = inDest.height;
            }
            else
            {
                destHeight = Screen.height;
                if (inCamera == null)
                {
                    inCamera = Camera.current;
                }
                if (inCamera != null)
                {
                    destHeight *= inCamera.rect.height;
                }
            }
            float scale = destHeight / (float) inSrc.height;
            bool integerScale = Mathf.Approximately(scale % 1, 0);
            if (scale >= 2 && !integerScale)
            {
                int intScale = (int) scale;
                var tempRT = RenderTexture.GetTemporary(inSrc.width * intScale, inSrc.height * intScale, inSrc.depth, inSrc.format);
                try
                {
                    inSrc.filterMode = FilterMode.Point;
                    tempRT.filterMode = FilterMode.Point;
                    Graphics.Blit(inSrc, tempRT);
                    tempRT.filterMode = FilterMode.Bilinear;
                    Graphics.Blit(tempRT, inDest);
                }
                finally
                {
                    RenderTexture.ReleaseTemporary(tempRT);
                }
            }
            else
            {
                inSrc.filterMode = integerScale ? FilterMode.Point : FilterMode.Bilinear;
                Graphics.Blit(inSrc, inDest);
            }
        }

        #endregion // Render Textures
    
        #region Memory Usage

        /// <summary>
        /// Calculates the approximate memory usage of a given texture's pixel data.
        /// </summary>
        static public long CalculateMemoryUsage(Texture2D inTexture)
        {
            int numPixels = inTexture.width * inTexture.height;

            switch(inTexture.format)
            {
                case TextureFormat.Alpha8: return numPixels;

                case TextureFormat.ARGB4444: return numPixels * 2;
                case TextureFormat.RGB24: return numPixels * 3;
                case TextureFormat.RGBA32: return numPixels * 4;
                case TextureFormat.ARGB32: return numPixels * 4;

                case TextureFormat.RGB565: return numPixels * 2;
                case TextureFormat.R16: return numPixels * 2;

                case TextureFormat.DXT1: return numPixels / 2;
                case TextureFormat.DXT5: return numPixels;

                case TextureFormat.RGBA4444: return numPixels * 2;
                case TextureFormat.BGRA32: return numPixels * 4;

                case TextureFormat.RHalf: return numPixels * 2;
                case TextureFormat.RGHalf: return numPixels * 4;
                case TextureFormat.RGBAHalf: return numPixels * 8;

                case TextureFormat.RFloat: return numPixels * 4;
                case TextureFormat.RGFloat: return numPixels * 8;
                case TextureFormat.RGBAFloat: return numPixels * 16;

                case TextureFormat.YUY2: return numPixels;

                case TextureFormat.DXT1Crunched: return numPixels / 2;
                case TextureFormat.DXT5Crunched: return numPixels;

                case TextureFormat.PVRTC_RGB2: return numPixels / 4;
                case TextureFormat.PVRTC_RGBA2: return numPixels / 4;
                case TextureFormat.PVRTC_RGB4: return numPixels / 2;
                case TextureFormat.PVRTC_RGBA4: return numPixels / 4;

                case TextureFormat.ETC_RGB4: return numPixels / 2;
                case TextureFormat.EAC_R: return numPixels / 2;
                case TextureFormat.EAC_R_SIGNED: return numPixels / 2;
                case TextureFormat.EAC_RG: return numPixels;
                case TextureFormat.EAC_RG_SIGNED: return numPixels;

                case TextureFormat.ETC2_RGB: return numPixels / 2;
                case TextureFormat.ETC2_RGBA1: return numPixels * 5 / 8;
                case TextureFormat.ETC2_RGBA8: return numPixels;

                #if UNITY_5_5_OR_NEWER
                case TextureFormat.BC4: return numPixels / 2;
                case TextureFormat.BC5: return numPixels;
                case TextureFormat.BC6H: return numPixels;
                case TextureFormat.BC7: return numPixels;
                #endif // UNITY_5_5_OR_NEWER

                #if UNITY_5_6_OR_NEWER
                case TextureFormat.RGB9e5Float: return numPixels * 4;
                case TextureFormat.RG16: return numPixels / 2;
                case TextureFormat.R8: return numPixels;
                #endif // UNITY_5_6_OR_NEWER

                #if UNITY_2017_3_OR_NEWER
                case TextureFormat.ETC_RGB4Crunched: return numPixels / 2;
                case TextureFormat.ETC2_RGBA8Crunched: return numPixels;
                #endif // UNITY_2017_3_OR_NEWER

                #if UNITY_2019_1_OR_NEWER

                #if UNITY_2020_1_OR_NEWER

                case TextureFormat.ASTC_4x4: return numPixels;
                case TextureFormat.ASTC_5x5: return numPixels * 16 / 25;
                case TextureFormat.ASTC_6x6: return numPixels * 16 / 36;
                case TextureFormat.ASTC_8x8: return numPixels * 16 / 64;
                case TextureFormat.ASTC_10x10: return numPixels * 16 / 100;
                case TextureFormat.ASTC_12x12: return numPixels * 16 / 144;

                #else

                case TextureFormat.ASTC_RGB_4x4: return numPixels;
                case TextureFormat.ASTC_RGBA_4x4: return numPixels;
                case TextureFormat.ASTC_RGB_5x5: return numPixels * 16 / 25;
                case TextureFormat.ASTC_RGBA_5x5: return numPixels * 16 / 25;
                case TextureFormat.ASTC_RGB_6x6: return numPixels * 16 / 36;
                case TextureFormat.ASTC_RGBA_6x6: return numPixels * 16 / 36;
                case TextureFormat.ASTC_RGB_8x8: return numPixels * 16 / 64;
                case TextureFormat.ASTC_RGBA_8x8: return numPixels * 16 / 64;
                case TextureFormat.ASTC_RGB_10x10: return numPixels * 16 / 100;
                case TextureFormat.ASTC_RGBA_10x10: return numPixels * 16 / 100;
                case TextureFormat.ASTC_RGB_12x12: return numPixels * 16 / 144;
                case TextureFormat.ASTC_RGBA_12x12: return numPixels * 16 / 144;

                #endif // !UNITY_2020_1_OR_NEWER

                case TextureFormat.ASTC_HDR_4x4: return numPixels;
                case TextureFormat.ASTC_HDR_5x5: return numPixels * 16 / 25;
                case TextureFormat.ASTC_HDR_6x6: return numPixels * 16 / 36;
                case TextureFormat.ASTC_HDR_8x8: return numPixels * 16 / 64;
                case TextureFormat.ASTC_HDR_10x10: return numPixels * 16 / 100;
                case TextureFormat.ASTC_HDR_12x12: return numPixels * 16 / 144;

                #endif // UNITY_2019_1_OR_NEWER

                #if UNITY_2019_4_OR_NEWER
                case TextureFormat.RG32: return numPixels * 4;
                case TextureFormat.RGB48: return numPixels * 6;
                case TextureFormat.RGBA64: return numPixels * 8;
                #endif // UNITY_2019_4_OR_NEWER 

                default: return numPixels;
            }
        }

        // static public long CalculateMemoryUsage(AudioClip inAudioClip)
        // {
        //     switch(inAudioClip.samples)
        // }

        #endregion // Memory Usage
    
        #region Lookups

        private delegate UnityEngine.Object FindDelegate(int inInstanceId);
        private delegate bool AlivePredicate(int inInstanceId);
        static private readonly FindDelegate s_FindDelegate;
        static private readonly AlivePredicate s_AliveDelegate;

        static UnityHelper()
        {
            MethodInfo findInfo = typeof(UnityEngine.Object).GetMethod("FindObjectFromInstanceID", BindingFlags.NonPublic | BindingFlags.Static);
            if (findInfo != null)
            {
                s_FindDelegate = (FindDelegate) findInfo.CreateDelegate(typeof(FindDelegate));
            }

            MethodInfo aliveInfo = typeof(UnityEngine.Object).GetMethod("DoesObjectWithInstanceIDExist", BindingFlags.NonPublic | BindingFlags.Static);
            if (aliveInfo != null)
            {
                s_AliveDelegate = (AlivePredicate) aliveInfo.CreateDelegate(typeof(AlivePredicate));
            }
        }

        /// <summary>
        /// Finds the Object instance with the given id.
        /// </summary>
        [MethodImpl(256)]
        static public UnityEngine.Object Find(int inInstanceId)
        {
            if (inInstanceId == 0 || s_FindDelegate == null)
            {
                return null;
            }
            return s_FindDelegate(inInstanceId);
        }

        /// <summary>
        /// Finds the Object instance with the given id.
        /// Will throw an exception if the Object type is not castable.
        /// </summary>
        [MethodImpl(256)]
        static public T Find<T>(int inInstanceId) where T : UnityEngine.Object
        {
            if (inInstanceId == 0 || s_FindDelegate == null) {
                return null;
            }
            return (T) s_FindDelegate(inInstanceId);
        }

        /// <summary>
        /// Finds the Object instance with the given id.
        /// </summary>
        [MethodImpl(256)]
        static public T SafeFind<T>(int inInstanceId) where T : UnityEngine.Object
        {
            if (inInstanceId == 0 || s_FindDelegate == null)
            {
                return null;
            }
            return s_FindDelegate(inInstanceId) as T;
        }

        /// <summary>
        /// Returns if the Object instance with the given id exists.
        /// </summary>
        [MethodImpl(256)]
        static public bool IsAlive(int inInstanceId)
        {
            if (inInstanceId == 0 || s_AliveDelegate == null)
            {
                return false;
            }
            return s_AliveDelegate(inInstanceId);
        }

        /// <summary>
        /// Returns the instance id for the given Object.
        /// </summary>
        [MethodImpl(256)]
        static public int Id(UnityEngine.Object inObject)
        {
            return object.ReferenceEquals(inObject, null) ? 0 : inObject.GetInstanceID();
        }

        #endregion // Lookups
    }
}