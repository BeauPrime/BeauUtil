/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    22 June 2023
 * 
 * File:    TypeIndex.cs
 * Purpose: Static type indices, for faster lookups based on generics.
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BeauUtil.Debugger;
using UnityEngine.Scripting;

namespace BeauUtil {
    /// <summary>
    /// Static type to index mapping.
    /// Useful for mapping instances of types to an index in an array instead of using a dictionary.
    /// </summary>
    static public class TypeIndex<T>
    {
        /// <summary>
        /// Maximum number of type indices.
        /// </summary>
        static public readonly int Capacity;

        static private int s_Allocated = 0;
        static private readonly Dictionary<Type, int> s_TypeMap;
        static private readonly Type[] s_IndexMap;

        static TypeIndex()
        {
            Type rootType = typeof(T);
            TypeIndexCapacityAttribute capacityAttr = Reflect.GetAttribute<TypeIndexCapacityAttribute>(rootType);
            if (capacityAttr != null)
            {
                Capacity = capacityAttr.Capacity;
            }
            else
            {
                Capacity = 512;
            }

            s_TypeMap = new Dictionary<Type, int>(Capacity);
            s_IndexMap = new Type[Capacity];

            Get(rootType);
        }

        /// <summary>
        /// Allocates or retrieves the existing index for the given type.
        /// </summary>
        static private int AllocateIndex(Type inType)
        {
            if (s_Allocated >= Capacity)
            {
                Assert.Fail("Exceeded maximum number of type indices {0} for type '{1}'", Capacity, typeof(T).FullName);
            }
            int index = s_Allocated++;
            s_TypeMap.Add(inType, index);
            s_IndexMap[index] = inType;
            return index;
        }

        /// <summary>
        /// Retrieves the index for the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// Returns the type for the given index.
        /// </summary>
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
            static public readonly int Index = Get(typeof(U));
        }
    }

    /// <summary>
    /// Sets maximun number of types a TypeIndex specialization for this type can index.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class TypeIndexCapacityAttribute : PreserveAttribute
    {
        /// <summary>
        /// Maximum number of indexed types.
        /// </summary>
        public readonly int Capacity;

        public TypeIndexCapacityAttribute(int inCapacity)
        {
            if (inCapacity < 4)
                throw new ArgumentOutOfRangeException("inCapacity", "Capacity must be at least 4");
            Capacity = Unsafe.AlignUp8(inCapacity);
        }
    }
}