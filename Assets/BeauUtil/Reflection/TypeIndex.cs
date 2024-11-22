/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    22 June 2023
 * 
 * File:    TypeIndex.cs
 * Purpose: Static type indices, for faster lookups based on generics.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BeauUtil.Debugger;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.Scripting;

namespace BeauUtil
{
    /// <summary>
    /// Static type to index mapping.
    /// Useful for mapping instances of types to an index in an array instead of using a dictionary.
    /// </summary>
    public class TypeIndex<TRootType>
    {
        /// <summary>
        /// Maximum number of interfaces that can be mapped.
        /// </summary>
        private const int MaxInterfaces = 32;

        /// <summary>
        /// Null index.
        /// </summary>
        private const int NullIndex = -1;

        /// <summary>
        /// Maximum index.
        /// </summary>
        private const int MaxIndex = ushort.MaxValue;

        /// <summary>
        /// Maximum number of type indices.
        /// </summary>
        static public readonly int Capacity;

        /// <summary>
        /// Number of allocated type indices.
        /// </summary>
        static public int Count { get { return s_Allocated; } }

        static private readonly bool s_RootIsInterface;

        static private int s_Allocated = 0;
        static private readonly Dictionary<Type, int> s_TypeMap;
        static private readonly Type[] s_IndexMap;
        static private readonly int[] s_ParentMap;

        //static private ushort s_

        static TypeIndex()
        {
            Type rootType = typeof(TRootType);
            TypeIndexCapacityAttribute capacityAttr = Reflect.GetAttribute<TypeIndexCapacityAttribute>(rootType);
            if (capacityAttr != null)
            {
                Capacity = capacityAttr.Capacity;
            }
            else
            {
                Capacity = 128;
            }

            if (Capacity > MaxIndex || Capacity < 8)
            {
                throw new ArgumentOutOfRangeException("Capacity", "Capacity must be between 8 and " + MaxIndex);
            }

            s_RootIsInterface = rootType.IsInterface;

            s_TypeMap = new Dictionary<Type, int>(Capacity);
            s_IndexMap = new Type[Capacity];
            s_ParentMap = new int[Capacity];

            // ensure index 0 is always the root type
            s_TypeMap.Add(rootType, 0);
            s_IndexMap[0] = rootType;
            s_ParentMap[0] = NullIndex;

            s_Allocated = 1;
        }

        protected TypeIndex()
        {
            throw new NotImplementedException("TypeIndex should not be instantiated");
        }

        /// <summary>
        /// Allocates or retrieves the existing index for the given type.
        /// </summary>
        static private int AllocateIndex(Type inType)
        {
            if (s_Allocated >= Capacity)
            {
                Assert.Fail("Exceeded maximum number of type indices {0} for type '{1}'", Capacity, typeof(TRootType).FullName);
                return NullIndex;
            }

            if (!typeof(TRootType).IsAssignableFrom(inType))
            {
                if (!inType.IsInterface || !inType.IsDefined(typeof(IndexedAttribute), true))
                {
                    Assert.Fail("Attempting to allocate index for type '{0}' that does not inherit from the base type '{1}', and is not an interface with the [Indexed] attribute", inType.FullName, typeof(TRootType).FullName);
                    return NullIndex;
                }
            }

            if (inType.IsDefined((typeof(NonIndexedAttribute)), false))
            {
                Assert.Fail("Attempting to allocate index for type '{0}' that is marked as NonIndexed", inType.FullName);
                return NullIndex;
            }

            int parentIndex = AllocateClassHierarchy(inType);

            lock (s_TypeMap)
            {
                int index = s_Allocated++;
                s_TypeMap.Add(inType, index);
                s_IndexMap[index] = inType;
                s_ParentMap[index] = parentIndex;
                return index;
            }
        }

        /// <summary>
        /// Allocates the base type chain for the given type.
        /// </summary>
        static private int AllocateClassHierarchy(Type inType)
        {
            int parentIndex = NullIndex;
            if (!inType.IsInterface && !inType.IsValueType)
            {
                Type parentType = inType.BaseType;
                if (parentType != typeof(TRootType) && typeof(TRootType).IsAssignableFrom(parentType) && !parentType.IsDefined(typeof(NonIndexedAttribute), false))
                {
                    parentIndex = Get(parentType);
                }
            }

            return parentIndex;
        }

        /// <summary>
        /// Retrieves the index for the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public int Get(Type inType)
        {
            if (!s_TypeMap.TryGetValue(inType, out int index))
            {
                index = AllocateIndex(inType);
            }
            return index;
        }

        /// <summary>
        /// Retrieves the index for the given type.
        /// Faster than Get(Type).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int Get<U>()
        {
            return Cache<U>.Index;
        }

        /// <summary>
        /// Retrieves an enumerator for all indices for a given type,
        /// traversing the indices for its base types.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BaseTypeEnumerator GetAll(Type inType)
        {
            return new BaseTypeEnumerator(Get(inType));
        }

        /// <summary>
        /// Retrieves an enumerator for all indices for a given type,
        /// traversing the indices for its base types.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BaseTypeEnumerator GetAll<U>()
        {
            return new BaseTypeEnumerator(Cache<U>.Index);
        }

        /// <summary>
        /// Retrieves an enumerator for all indices for the given type index,
        /// traversing the indices for its parent types.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BaseTypeEnumerator GetAll(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < s_Allocated, "Index {0} is out of mapped range 0-{1}", inIndex, s_Allocated - 1);
            return new BaseTypeEnumerator(inIndex);
        }

        /// <summary>
        /// Returns the type for the given index.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        static public Type Type(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < s_Allocated, "Index {0} is out of mapped range 0-{1}", inIndex, s_Allocated - 1);
            return s_IndexMap[inIndex];
        }

        /// <summary>
        /// All thanks to generics!
        /// </summary>
        private struct Cache<U>
        {
            static public readonly int Index;

            static Cache()
            {
                Index = Get(typeof(U));
            }
        }

        /// <summary>
        /// Enumerator that iterates over all indices in the inheritance chain for a type.
        /// </summary>
        public struct BaseTypeEnumerator : IEnumerable<int>, IEnumerator<int>, IDisposable
        {
            private int m_Current;
            private int m_Next;

            internal BaseTypeEnumerator(int inIndex)
            {
                m_Current = NullIndex;
                m_Next = inIndex;
            }

            public int Current { get { return m_Current; } }

            object IEnumerator.Current { get { return m_Current; } }

            public void Dispose()
            {
                m_Current = m_Next = NullIndex;
            }

            public bool MoveNext()
            {
                m_Current = m_Next;
                m_Next = m_Next == NullIndex ? NullIndex : s_ParentMap[m_Next];
                return m_Current != NullIndex;
            }

            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            public BaseTypeEnumerator GetEnumerator()
            {
                return this;
            }

            IEnumerator<int> IEnumerable<int>.GetEnumerator()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this;
            }
        }
    }

    /// <summary>
    /// Sets maximun number of types a TypeIndex specialization for this type can index.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    [Preserve]
    public sealed class TypeIndexCapacityAttribute : PreserveAttribute
    {
        /// <summary>
        /// Maximum number of indexed types.
        /// </summary>
        public readonly int Capacity;

        public TypeIndexCapacityAttribute(int inCapacity)
        {
            if (inCapacity < 8)
                throw new ArgumentOutOfRangeException("inCapacity", "Capacity must be at least 8");
            Capacity = Unsafe.AlignUp8(inCapacity);
        }
    }

    /// <summary>
    /// Marks a type or interface as available to be tracked by TypeIndex.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum, AllowMultiple = false, Inherited = true)]
    [Preserve]
    public sealed class IndexedAttribute : Attribute
    {
    }

    /// <summary>
    /// Marks a type or interface as explicitly ignored by TypeIndex.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum, AllowMultiple = false, Inherited = true)]
    [Preserve]
    public sealed class NonIndexedAttribute : Attribute
    {
    }
}