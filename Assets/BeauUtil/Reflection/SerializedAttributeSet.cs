using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using Attribute = System.Attribute;

namespace BeauUtil
{
    /// <summary>
    /// Serializable reflection information.
    /// Describes attribute bindings in assemblies.
    /// This allows lists of attribute bindings to be cached at build time.
    /// </summary>
    [Serializable]
    public class SerializedAttributeSet
    {
        public const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        #region Types

        [Serializable]
        public struct AssemblyBucket
        {
            public string AssemblyName;
            public uint TypeCount;

            public override string ToString()
            {
                return string.Format("{0} {1}", AssemblyName, TypeCount);
            }
        }

        [Serializable]
        public struct TypeBucket
        {
            public string TypeName;
            public bool Self;
            public uint MemberCount;
            public uint OverloadCount;

            public override string ToString()
            {
                return string.Format("{0} ({1}) | Members {2} | Overloaded Methods {3}", TypeName, Self ? "self" : "n/a", MemberCount, OverloadCount);
            }
        }

        [Serializable]
        public struct Member
        {
            public string MemberName;
            public byte Type;

            public override string ToString()
            {
                return string.Format("{0} ({1})", MemberName, ((MemberTypes) Type));
            }
        }

        [Serializable]
        public struct OverloadedMethod
        {
            public string MethodName;
            public ushort[] ParameterTypeRefs;

            public override string ToString()
            {
                return string.Format("{0} (overloaded)", MethodName);
            }
        }

        [Serializable]
        public struct TypeReference
        {
            public string AssemblyQualifiedName;
            [NonSerialized] public Type CachedType;

            public override string ToString()
            {
                return AssemblyQualifiedName;
            }
        }

        public struct AttributePair<TAttr> where TAttr : Attribute
        {
            public readonly TAttr Attribute;
            public readonly MemberInfo Info;

            public AttributePair(TAttr inAttribute, MemberInfo inMember)
            {
                Info = inMember;
                Attribute = inAttribute;
            }
        }

        #endregion // Types

        #region Data

        public string AttributeTypeName;
        public BindingFlags LookupFlags;
        public bool LookupInherit;

        [Header("Buckets")]
        public AssemblyBucket[] Assemblies;
        public TypeBucket[] Types;

        [Header("Lookups")]
        public Member[] Members;
        public OverloadedMethod[] OverloadedMethods;
        public TypeReference[] TypeReferences;

        #endregion // Data

        [NonSerialized] private readonly Type[][] m_OverloadedMethodArgWorkArray = new Type[12][];

        #region Reading

        /// <summary>
        /// Reads all attribute pairs from this info set.
        /// </summary>
        public IEnumerable<AttributePair<TAttr>> Read<TAttr>(IEnumerable<Assembly> inAssemblies) where TAttr : Attribute
        {
            if (!typeof(TAttr).FullName.Equals(AttributeTypeName, StringComparison.Ordinal))
            {
                throw new ArgumentException("TAttr");
            }

            if (Assemblies.Length == 0)
            {
                yield break;
            }

            Assembly[] assemblyArr = GetAssemblyArray(inAssemblies);

            uint typeIndex = 0;
            uint memberIndex = 0;
            uint overloadIndex = 0;

            for (int asmIndex = 0, asmCount = Assemblies.Length; asmIndex < asmCount; asmIndex++)
            {
                AssemblyBucket asmBucket = Assemblies[asmIndex];
                Assembly asm = FindAssembly(assemblyArr, asmBucket.AssemblyName);
                if (asm == null)
                {
                    Debug.LogWarningFormat("[SerializedAttributeSet] Assembly '{0}' not found in provided set - skipping", asmBucket.AssemblyName);
                    SkipAssemblyBucket(asmBucket, ref typeIndex, ref memberIndex, ref overloadIndex);
                    continue;
                }

                while (asmBucket.TypeCount-- > 0)
                {
                    TypeBucket typeBucket = Types[typeIndex++];
                    Type type = asm.GetType(typeBucket.TypeName, true, false);

                    if (typeBucket.Self)
                    {
                        foreach (TAttr attr in type.GetCustomAttributes(typeof(TAttr), LookupInherit))
                        {
                            yield return new AttributePair<TAttr>(attr, type);
                        }
                    }

                    while (typeBucket.MemberCount-- > 0)
                    {
                        Member member = Members[memberIndex++];
                        MemberInfo info;
                        switch ((MemberTypes) member.Type)
                        {
                            case MemberTypes.Field:
                            {
                                info = type.GetField(member.MemberName, LookupFlags);
                                break;
                            }
                            case MemberTypes.Property:
                            {
                                info = type.GetProperty(member.MemberName, LookupFlags);
                                break;
                            }
                            case MemberTypes.Event:
                            {
                                info = type.GetEvent(member.MemberName, LookupFlags);
                                break;
                            }
                            case MemberTypes.Method:
                            {
                                info = type.GetMethod(member.MemberName, LookupFlags);
                                break;
                            }
                            default:
                            {
                                throw new NotSupportedException();
                            }
                        }
                        foreach (TAttr attr in info.GetCustomAttributes(typeof(TAttr), LookupInherit))
                        {
                            yield return new AttributePair<TAttr>(attr, info);
                        }
                    }

                    while (typeBucket.OverloadCount-- > 0)
                    {
                        OverloadedMethod overloadedMethod = OverloadedMethods[overloadIndex++];
                        MemberInfo method = type.GetMethod(overloadedMethod.MethodName, LookupFlags, Type.DefaultBinder, RetrieveTypeArray(overloadedMethod.ParameterTypeRefs), Array.Empty<ParameterModifier>());
                        foreach (TAttr attr in method.GetCustomAttributes(typeof(TAttr), LookupInherit))
                        {
                            yield return new AttributePair<TAttr>(attr, method);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reads all attribute pairs from this info set.
        /// </summary>
        public IEnumerable<AttributePair<Attribute>> Read(IEnumerable<Assembly> inAssemblies, Type inAttributeType)
        {
            if (inAttributeType == null)
            {
                throw new ArgumentNullException("inAttributeType");
            }

            if (!inAttributeType.FullName.Equals(AttributeTypeName, StringComparison.Ordinal))
            {
                throw new ArgumentException("inAttributeType");
            }

            if (Assemblies.Length == 0)
            {
                yield break;
            }

            Assembly[] assemblyArr = GetAssemblyArray(inAssemblies);

            uint typeIndex = 0;
            uint memberIndex = 0;
            uint overloadIndex = 0;

            for (int asmIndex = 0, asmCount = Assemblies.Length; asmIndex < asmCount; asmIndex++)
            {
                AssemblyBucket asmBucket = Assemblies[asmIndex];
                Assembly asm = FindAssembly(assemblyArr, asmBucket.AssemblyName);
                if (asm == null)
                {
                    Debug.LogWarningFormat("[SerializedAttributeSet] Assembly '{0}' not found in provided set - skipping", asmBucket.AssemblyName);
                    SkipAssemblyBucket(asmBucket, ref typeIndex, ref memberIndex, ref overloadIndex);
                    continue;
                }

                while (asmBucket.TypeCount-- > 0)
                {
                    TypeBucket typeBucket = Types[typeIndex++];
                    Type type = asm.GetType(typeBucket.TypeName, true, false);

                    if (typeBucket.Self)
                    {
                        foreach (Attribute attr in type.GetCustomAttributes(inAttributeType, LookupInherit))
                        {
                            yield return new AttributePair<Attribute>(attr, type);
                        }
                    }

                    while (typeBucket.MemberCount-- > 0)
                    {
                        Member member = Members[memberIndex++];
                        MemberInfo info;
                        switch ((MemberTypes) member.Type)
                        {
                            case MemberTypes.Field:
                            {
                                info = type.GetField(member.MemberName, LookupFlags);
                                break;
                            }
                            case MemberTypes.Property:
                            {
                                info = type.GetProperty(member.MemberName, LookupFlags);
                                break;
                            }
                            case MemberTypes.Event:
                            {
                                info = type.GetEvent(member.MemberName, LookupFlags);
                                break;
                            }
                            case MemberTypes.Method:
                            {
                                info = type.GetMethod(member.MemberName, LookupFlags);
                                break;
                            }
                            default:
                            {
                                throw new NotSupportedException();
                            }
                        }
                        foreach (Attribute attr in info.GetCustomAttributes(inAttributeType, LookupInherit))
                        {
                            yield return new AttributePair<Attribute>(attr, info);
                        }
                    }

                    while (typeBucket.OverloadCount-- > 0)
                    {
                        OverloadedMethod overloadedMethod = OverloadedMethods[overloadIndex++];
                        MemberInfo method = type.GetMethod(overloadedMethod.MethodName, LookupFlags, Type.DefaultBinder, RetrieveTypeArray(overloadedMethod.ParameterTypeRefs), Array.Empty<ParameterModifier>());
                        foreach (Attribute attr in method.GetCustomAttributes(inAttributeType, LookupInherit))
                        {
                            yield return new AttributePair<Attribute>(attr, method);
                        }
                    }
                }
            }
        }

        static private Assembly FindAssembly(Assembly[] inAssemblies, string inFullName)
        {
            foreach (var assembly in inAssemblies)
            {
                if (assembly.FullName.Equals(inFullName, StringComparison.Ordinal))
                {
                    return assembly;
                }
            }
            return null;
        }

        private void SkipAssemblyBucket(AssemblyBucket inBucket, ref uint ioTypeIndex, ref uint ioMemberIndex, ref uint ioOverloadIndex)
        {
            while (inBucket.TypeCount-- > 0)
            {
                TypeBucket typeBucket = Types[ioTypeIndex++];
                ioMemberIndex += typeBucket.MemberCount;
                ioOverloadIndex += typeBucket.OverloadCount;
            }
        }

        private Type[] RetrieveTypeArray(ushort[] inTypes)
        {
            if (inTypes == null || inTypes.Length == 0)
            {
                return Array.Empty<Type>();
            }

            int idx = inTypes.Length - 1;
            Type[] arr;
            if (idx >= m_OverloadedMethodArgWorkArray.Length)
            {
                arr = new Type[inTypes.Length];
            }
            else
            {
                arr = m_OverloadedMethodArgWorkArray[idx];
                if (arr == null)
                {
                    m_OverloadedMethodArgWorkArray[idx] = arr = new Type[inTypes.Length];
                }
            }

            for (int i = 0; i < inTypes.Length; i++)
            {
                arr[i] = RetrieveType(inTypes[i]);
            }
            return arr;
        }

        private Type RetrieveType(ushort inTypeIndex)
        {
            ref TypeReference typeRef = ref TypeReferences[inTypeIndex];
            if (typeRef.CachedType == null)
            {
                typeRef.CachedType = Type.GetType(typeRef.AssemblyQualifiedName);
            }
            return typeRef.CachedType;
        }

        static private Assembly[] GetAssemblyArray(IEnumerable<Assembly> inAssemblies)
        {
            Assembly[] asms = inAssemblies as Assembly[];
            if (asms != null)
                return asms;
            List<Assembly> list = new List<Assembly>(inAssemblies);
            return list.ToArray();
        }

        #endregion // Reading

        #region Writing

        /// <summary>
        /// Overwrites with data describing attributes present in the given assembly.
        /// </summary>
        public void Write<TAttr>(IEnumerable<Assembly> inAssemblies, BindingFlags inSearchFlags = DefaultBindingFlags, bool inbInherit = false) where TAttr : Attribute
        {
            Write(inAssemblies, typeof(TAttr), inSearchFlags, inbInherit);
        }

        /// <summary>
        /// Overwrites with data describing attributes present in the given assembly.
        /// </summary>
        public void Write(IEnumerable<Assembly> inAssemblies, Type inAttributeType, BindingFlags inSearchFlags = DefaultBindingFlags, bool inbInherit = false)
        {
            if (inAttributeType == null)
            {
                throw new ArgumentNullException("inAttributeType");
            }

            AttributeTypeName = inAttributeType.FullName;
            LookupFlags = inSearchFlags;
            LookupInherit = inbInherit;

            List<AssemblyBucket> asmBuckets = new List<AssemblyBucket>(8);
            List<TypeBucket> typeBuckets = new List<TypeBucket>();
            List<Member> members = new List<Member>();
            List<OverloadedMethod> overloadedMethods = new List<OverloadedMethod>();
            List<TypeReference> typeRefs = new List<TypeReference>();

            // scan assemblies
            foreach (var assembly in inAssemblies)
            {
                int typeCount = 0;

                // scan types in assembly
                foreach (var type in assembly.GetTypes())
                {
                    int memberCount = 0;
                    int overloadedCount = 0;
                    bool self = type.IsDefined(inAttributeType, inbInherit);

                    foreach (var field in type.GetFields(inSearchFlags))
                    {
                        if (field.IsDefined(inAttributeType, inbInherit))
                        {
                            members.Add(new Member()
                            {
                                MemberName = field.Name,
                                Type = (byte) MemberTypes.Field
                            });
                            memberCount++;
                        }
                    }

                    foreach (var prop in type.GetProperties(inSearchFlags))
                    {
                        if (prop.IsDefined(inAttributeType, inbInherit))
                        {
                            members.Add(new Member()
                            {
                                MemberName = prop.Name,
                                Type = (byte) MemberTypes.Property
                            });
                            memberCount++;
                        }
                    }

                    foreach (var evt in type.GetEvents(inSearchFlags))
                    {
                        if (evt.IsDefined(inAttributeType, inbInherit))
                        {
                            members.Add(new Member()
                            {
                                MemberName = evt.Name,
                                Type = (byte) MemberTypes.Event
                            });
                            memberCount++;
                        }
                    }

                    MethodInfo[] allMethods = type.GetMethods(inSearchFlags);
                    foreach (var method in allMethods)
                    {
                        if (method.IsGenericMethod)
                        {
                            continue;
                        }

                        if (method.IsDefined(inAttributeType, inbInherit))
                        {
                            if (IsPotentialOverloadedMethod(allMethods, method))
                            {
                                OverloadedMethod overload;
                                overload.MethodName = method.Name;
                                overload.ParameterTypeRefs = RetrieveOverloadedParameterReferences(typeRefs, method);
                                overloadedMethods.Add(overload);
                                overloadedCount++;
                            }
                            else
                            {
                                members.Add(new Member()
                                {
                                    MemberName = method.Name,
                                    Type = (byte) MemberTypes.Method
                                });
                                memberCount++;
                            }
                        }
                    }

                    if (self || memberCount > 0 || overloadedCount > 0)
                    {
                        typeBuckets.Add(new TypeBucket()
                        {
                            TypeName = type.FullName,
                            Self = self,
                            MemberCount = (uint) memberCount,
                            OverloadCount = (uint) overloadedCount
                        });

                        typeCount++;
                    }
                }

                // only write assembly if types were found
                if (typeCount > 0)
                {
                    asmBuckets.Add(new AssemblyBucket()
                    {
                        AssemblyName = assembly.FullName,
                        TypeCount = (uint) typeCount
                    });
                }
            }

            Assemblies = asmBuckets.ToArray();
            Types = typeBuckets.ToArray();
            Members = members.ToArray();
            OverloadedMethods = overloadedMethods.ToArray();
            TypeReferences = typeRefs.ToArray();
        }

        /// <summary>
        /// Creates an attribute set from the given assemblies.
        /// </summary>
        static public SerializedAttributeSet Create<TAttr>(IEnumerable<Assembly> inAssemblies, BindingFlags inSearchFlags = DefaultBindingFlags, bool inbInherit = false) where TAttr : Attribute
        {
            SerializedAttributeSet set = new SerializedAttributeSet();
            set.Write(inAssemblies, typeof(TAttr), inSearchFlags, inbInherit);
            return set;
        }

        /// <summary>
        /// Creates an attribute set from the given assemblies.
        /// </summary>
        static public SerializedAttributeSet Create(IEnumerable<Assembly> inAssemblies, Type inAttributeType, BindingFlags inSearchFlags = DefaultBindingFlags, bool inbInherit = false)
        {
            SerializedAttributeSet set = new SerializedAttributeSet();
            set.Write(inAssemblies, inAttributeType, inSearchFlags, inbInherit);
            return set;
        }

        /// <summary>
        /// Returns if the given method is part of an overloaded set of methods.
        /// </summary>
        static private bool IsPotentialOverloadedMethod(MethodInfo[] inMethodSet, MethodInfo inSearch)
        {
            foreach (var method in inMethodSet)
            {
                if (method != inSearch && method.Name.Equals(inSearch.Name, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        static private ushort[] RetrieveOverloadedParameterReferences(List<TypeReference> ioTypeReferences, MethodInfo inMethod)
        {
            ParameterInfo[] parameters = inMethod.GetParameters();
            ushort[] output = new ushort[parameters.Length];

            for (int paramIdx = 0; paramIdx < parameters.Length; paramIdx++)
            {
                Type paramType = parameters[paramIdx].ParameterType;
                int foundType = -1;
                for (int typeIdx = 0; typeIdx < ioTypeReferences.Count; typeIdx++)
                {
                    if (ioTypeReferences[typeIdx].CachedType == paramType)
                    {
                        foundType = typeIdx;
                        break;
                    }
                }

                if (foundType == -1)
                {
                    foundType = ioTypeReferences.Count;
                    ioTypeReferences.Add(new TypeReference()
                    {
                        CachedType = paramType,
                        AssemblyQualifiedName = paramType.AssemblyQualifiedName
                    });
                }

                output[paramIdx] = (ushort) foundType;
            }

            return output;
        }

        /// <summary>
        /// Clears this attribute set.
        /// </summary>
        public void Clear()
        {
            AttributeTypeName = string.Empty;
            LookupFlags = 0;
            LookupInherit = false;

            Assemblies = Array.Empty<AssemblyBucket>();
            Types = Array.Empty<TypeBucket>();
            Members = Array.Empty<Member>();
            OverloadedMethods = Array.Empty<OverloadedMethod>();
            TypeReferences = Array.Empty<TypeReference>();
        }

        #endregion // Writing
    }
}