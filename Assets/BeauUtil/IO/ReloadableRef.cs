using System;
using BeauUtil.Debugger;
using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace BeauUtil.IO
{
    /// <summary>
    /// Reference to a potentially reloadable asset.
    /// </summary>
    [Serializable]
    public struct ReloadableRef<T> : IEquatable<ReloadableRef<T>>
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif // UNITY_EDITOR
        where T : UnityEngine.Object
    {
        [SerializeField] private T m_Value;

#if UNITY_EDITOR
        [NonSerialized] private string m_CachedPath;
#endif // UNITY_EDITOR

        public ReloadableRef(T inObject)
        {
            Assert.True(inObject == null || UnityHelper.IsPersistent(inObject), "Cannot set as a non-persistent asset");
            m_Value = inObject;
#if UNITY_EDITOR
            m_CachedPath = inObject ? AssetDatabase.GetAssetPath(inObject) : null;
#endif // UNITY_EDITOR
        }

        /// <summary>
        /// Reference to the asset.
        /// </summary>
        public T Asset
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if UNITY_EDITOR
                if (m_Value.IsReferenceDestroyed())
                {
                    m_Value = AssetDatabase.LoadAssetAtPath<T>(m_CachedPath);
                }
#endif // UNITY_EDITOR
                return m_Value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Assert.True(value == null || UnityHelper.IsPersistent(value), "Cannot set as a non-persistent asset");
                m_Value = value;
#if UNITY_EDITOR
                m_CachedPath = value ? AssetDatabase.GetAssetPath(value) : null;
#endif // UNITY_EDITOR
            }
        }

        #region Interfaces

        public bool Equals(ReloadableRef<T> other)
        {
            return ReferenceEquals(Asset, other.Asset);
        }

        #endregion // Interfaces

        #region Overloads

        public override int GetHashCode()
        {
            var asset = Asset;
            return asset ? asset.GetHashCode() : 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is ReloadableRef<T>)
            {
                return Equals((ReloadableRef<T>) obj);
            }
            return false;
        }

        #endregion // Overloads

        #region Operators

        static public implicit operator bool(ReloadableRef<T> a)
        {
            return (bool) a.Asset;
        }

        static public bool operator==(ReloadableRef<T> a, ReloadableRef<T> b)
        {
            return a.Equals(b);
        }

        static public bool operator !=(ReloadableRef<T> a, ReloadableRef<T> b)
        {
            return !a.Equals(b);
        }

        static public implicit operator T(ReloadableRef<T> inValue)
        {
            return inValue.Asset;
        }

        static public implicit operator ReloadableRef<T>(T inValue)
        {
            return new ReloadableRef<T>(inValue);
        }

        #endregion // Operators

        #region Editor

#if UNITY_EDITOR
        public void OnAfterDeserialize()
        {
        }

        public void OnBeforeSerialize()
        {
            m_CachedPath = m_Value ? AssetDatabase.GetAssetPath(m_Value) : null;
        }
#endif // UNITY_EDITOR

        #endregion // Editor
    }
}