using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace BeauUtil
{
    /// <summary>
    /// Caches attributes mapped to members.
    /// </summary>
    public sealed class AttributeCache<TAttr> where TAttr : Attribute
    {
        /// <summary>
        /// Attribute processing delegate.
        /// </summary>
        public delegate void ProcessDelegate(TAttr attribute, MemberInfo member);

        private readonly Dictionary<MemberInfo, TAttr> m_Cache;
        private readonly ProcessDelegate m_ProcessFunc;
        private readonly TAttr m_Default;

        public AttributeCache(int inCapacity, ProcessDelegate inProcessor = null, TAttr inDefault = null)
        {
            m_Cache = new Dictionary<MemberInfo, TAttr>(inCapacity);
            m_ProcessFunc = inProcessor;
            m_Default = inDefault;
        }

        /// <summary>
        /// Clears the attribute cache.
        /// </summary>
        public void Clear()
        {
            m_Cache.Clear();
        }

        /// <summary>
        /// Returns the number of cached members.
        /// </summary>
        public int Count
        {
            get { return m_Cache.Count; }
        }

        /// <summary>
        /// Retrieves the cached attribute for the type of the given object.
        /// </summary>
        public TAttr Get(object inObj)
        {
            if (inObj == null)
            {
                return m_Default;
            }

            MemberInfo member = inObj as MemberInfo;
            if (member != null)
            {
                return Get(member);
            }

            return Get(member.GetType());
        }

        /// <summary>
        /// Retrieves the cached attribute for the given member.
        /// </summary>
        public TAttr Get(MemberInfo inInfo)
        {
            if (inInfo == null)
            {
                return m_Default;
            }

            if (!m_Cache.TryGetValue(inInfo, out TAttr attr))
            {
                if (inInfo.IsDefined(typeof(TAttr)))
                {
                    attr = (TAttr) inInfo.GetCustomAttribute(typeof(TAttr));
                    m_ProcessFunc?.Invoke(attr, inInfo);
                }
                else
                {
                    attr = m_Default;
                }
                m_Cache.Add(inInfo, attr);
            }
            return attr;
        }

        /// <summary>
        /// Retrieves the cached attribute for the given type.
        /// </summary>
        public TAttr Get<T>()
        {
            return Get(typeof(T));
        }
    }

    /// <summary>
    /// Caches attribute data mapped to members.
    /// </summary>
    public sealed class AttributeCache<TAttr, TData> where TAttr : Attribute
    {
        /// <summary>
        /// Delegate for mapping between attribute and the cached data type.
        /// </summary>
        public delegate TData MapDelegate(TAttr attribute, MemberInfo member);

        private readonly Dictionary<MemberInfo, TData> m_Cache;
        private readonly MapDelegate m_MapFunc;
        private readonly TData m_Default;

        public AttributeCache(MapDelegate inMapper, int inCapacity, TData inDefault = default(TData))
        {
            if (inMapper == null)
            {
                throw new ArgumentNullException("mapper");
            }

            m_Cache = new Dictionary<MemberInfo, TData>(inCapacity);
            m_MapFunc = inMapper;
            m_Default = inDefault;
        }

        /// <summary>
        /// Clears the attribute data cache.
        /// </summary>
        public void Clear()
        {
            m_Cache.Clear();
        }

        /// <summary>
        /// Returns the number of cached members.
        /// </summary>
        public int Count
        {
            get { return m_Cache.Count; }
        }

        /// <summary>
        /// Retrieves the cached data for the type of the given object.
        /// </summary>
        public TData Get(object inObj)
        {
            if (inObj == null)
            {
                return m_Default;
            }

            MemberInfo member = inObj as MemberInfo;
            if (member != null)
            {
                return Get(member);
            }

            return Get(member.GetType());
        }

        /// <summary>
        /// Retrieves the cached data for the given member.
        /// </summary>
        public TData Get(MemberInfo inInfo)
        {
            if (inInfo == null)
            {
                return m_Default;
            }

            if (!m_Cache.TryGetValue(inInfo, out TData val))
            {
                if (inInfo.IsDefined(typeof(TAttr)))
                {
                    TAttr attr = (TAttr) inInfo.GetCustomAttribute(typeof(TAttr));
                    val = m_MapFunc(attr, inInfo);
                }
                else
                {
                    val = m_Default;
                }
                m_Cache.Add(inInfo, val);
            }
            return val;
        }

        /// <summary>
        /// Retrieves the cached data for the given type.
        /// </summary>
        public TData Get<T>()
        {
            return Get(typeof(T));
        }
    }
}