using System;
using System.Collections.Generic;
using System.Reflection;

namespace BeauUtil
{
    /// <summary>
    /// Reflection utils.
    /// </summary>
    static public class Reflect
    {
        #region Inheritance

        /// <summary>
        /// Returns all derived types of the given type in all assemblies.
        /// </summary>
        static public IEnumerable<Type> FindAllDerivedTypes(Type inType)
        {
            return FindDerivedTypes(inType, AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>
        /// Returns all derived types of the given type in the given assemblies.
        /// </summary>
        static public IEnumerable<Type> FindDerivedTypes(Type inType, IEnumerable<Assembly> inAssemblies)
        {
            foreach (var assembly in inAssemblies)
            {
                foreach (var derivedType in FindDerivedTypes(inType, assembly))
                    yield return derivedType;
            }
        }

        /// <summary>
        /// Returns all derived types of the given type in the given assembly.
        /// </summary>
        static public IEnumerable<Type> FindDerivedTypes(Type inType, Assembly inAssembly)
        {
            return FindDerivedTypes(inType, inAssembly.GetTypes());
        }

        /// <summary>
        /// Returns all derived types of the given type in the given types.
        /// </summary>
        static public IEnumerable<Type> FindDerivedTypes(Type inType, IEnumerable<Type> inTypes)
        {
            foreach (var type in inTypes)
            {
                if (inType.IsAssignableFrom(type))
                    yield return type;
            }
        }

        /// <summary>
        /// Returns the inheritance depth of the given type.
        /// </summary>
        static public int GetInheritanceDepth(Type inType)
        {
            int depth = 0;
            Type t = inType;
            while(t != null)
            {
                ++depth;
                t = t.BaseType;
            }

            return depth;
        }

        #endregion // Inheritance

        #region Attributes

        #region Generic Attributes

        /// <summary>
        /// Returns all attributes defined on the given member.
        /// </summary>
        static public IEnumerable<TAttribute> GetAttributes<TAttribute>(ICustomAttributeProvider inInfo, bool inbInherit = false) where TAttribute : Attribute
        {
            if (inInfo.IsDefined(typeof(TAttribute), inbInherit))
            {
                foreach (var customAttribute in inInfo.GetCustomAttributes(typeof(TAttribute), inbInherit))
                {
                    yield return (TAttribute) customAttribute;
                }
            }
        }

        /// <summary>
        /// Returns if the given attribute is defined on the given member.
        /// </summary>
        static public bool IsDefined<TAttribute>(ICustomAttributeProvider inInfo, bool inbInherit = false) where TAttribute : Attribute
        {
            return inInfo.IsDefined(typeof(TAttribute), inbInherit);
        }

        /// <summary>
        /// Returns the first attribute defined on the given member.
        /// </summary>
        static public TAttribute GetAttribute<TAttribute>(ICustomAttributeProvider inInfo, bool inbInherit = false) where TAttribute : Attribute
        {
            var customAttributes = inInfo.GetCustomAttributes(typeof(TAttribute), inbInherit);
            if (customAttributes != null && customAttributes.Length > 0)
                return (TAttribute) customAttributes[0];

            return null;
        }

        #endregion // Generic Attributes

        #region Find Types

        /// <summary>
        /// Finds all types all loaded assemblies with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, Type>> FindAllTypes<TAttribute>(bool inbInherit = false) where TAttribute : Attribute
        {
            return FindTypes<TAttribute>(AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>
        /// Finds all types in the given assemblies with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, Type>> FindTypes<TAttribute>(IEnumerable<Assembly> inAssemblies, bool inbInherit = false) where TAttribute : Attribute
        {
            foreach (var assembly in inAssemblies)
            {
                foreach (var binding in FindTypes<TAttribute>(assembly, inbInherit))
                {
                    yield return binding;
                }
            }
        }

        /// <summary>
        /// Finds all types in the given assembly with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, Type>> FindTypes<TAttribute>(Assembly inAssembly, bool inbInherit = false) where TAttribute : Attribute
        {
            return FindTypes<TAttribute>(inAssembly.GetTypes(), inbInherit);
        }

        /// <summary>
        /// Finds all types in the given types with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, Type>> FindTypes<TAttribute>(IEnumerable<Type> inTypes, bool inbInherit = false) where TAttribute : Attribute
        {
            Type attributeType = typeof(TAttribute);

            foreach (var type in inTypes)
            {
                if (type.IsDefined(attributeType, inbInherit))
                {
                    foreach (var customAttribute in type.GetCustomAttributes(attributeType, inbInherit))
                    {
                        yield return new AttributeBinding<TAttribute, Type>((TAttribute) customAttribute, type);
                    }
                }
            }
        }

        #endregion // Find Types

        #region Find Methods

        /// <summary>
        /// Finds all methods in all loaded assemblies with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, MethodInfo>> FindAllMethods<TAttribute>(BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            return FindMethods<TAttribute>(AppDomain.CurrentDomain.GetAssemblies(), inFlags, inbInherit);
        }

        /// <summary>
        /// Finds all methods in the given assemblies with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, MethodInfo>> FindMethods<TAttribute>(IEnumerable<Assembly> inAssemblies, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            foreach (var assembly in inAssemblies)
            {
                foreach (var binding in FindMethods<TAttribute>(assembly, inFlags, inbInherit))
                {
                    yield return binding;
                }
            }
        }

        /// <summary>
        /// Finds all methods in the given assembly with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, MethodInfo>> FindMethods<TAttribute>(Assembly inAssembly, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            return FindMethods<TAttribute>(inAssembly.GetTypes(), inFlags, inbInherit);
        }

        /// <summary>
        /// Finds all methods in the given types with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, MethodInfo>> FindMethods<TAttribute>(IEnumerable<Type> inTypes, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            foreach (var type in inTypes)
            {
                foreach (var binding in FindMethods<TAttribute>(type, inFlags, inbInherit))
                {
                    yield return binding;
                }
            }
        }

        /// <summary>
        /// Finds all methods in the given type with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, MethodInfo>> FindMethods<TAttribute>(Type inType, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            Type attributeType = typeof(TAttribute);

            foreach (var method in inType.GetMethods(inFlags))
            {
                if (method.IsDefined(attributeType, inbInherit))
                {
                    foreach (var customAttribute in method.GetCustomAttributes(attributeType, inbInherit))
                    {
                        yield return new AttributeBinding<TAttribute, MethodInfo>((TAttribute) customAttribute, method);
                    }
                }
            }
        }

        #endregion // Find Methods

        #region Find Fields

        /// <summary>
        /// Finds all fields in all loaded assemblies with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, FieldInfo>> FindAllFields<TAttribute>(BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            return FindFields<TAttribute>(AppDomain.CurrentDomain.GetAssemblies(), inFlags, inbInherit);
        }

        /// <summary>
        /// Finds all fields in the given assemblies with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, FieldInfo>> FindFields<TAttribute>(IEnumerable<Assembly> inAssemblies, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            foreach (var assembly in inAssemblies)
            {
                foreach (var binding in FindFields<TAttribute>(assembly, inFlags, inbInherit))
                {
                    yield return binding;
                }
            }
        }

        /// <summary>
        /// Finds all fields in the given assembly with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, FieldInfo>> FindFields<TAttribute>(Assembly inAssembly, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            return FindFields<TAttribute>(inAssembly.GetTypes(), inFlags, inbInherit);
        }

        /// <summary>
        /// Finds all fields in the given types with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, FieldInfo>> FindFields<TAttribute>(IEnumerable<Type> inTypes, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            foreach (var type in inTypes)
            {
                foreach (var binding in FindFields<TAttribute>(type, inFlags, inbInherit))
                {
                    yield return binding;
                }
            }
        }

        /// <summary>
        /// Finds all fields in the given type with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, FieldInfo>> FindFields<TAttribute>(Type inType, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            Type attributeType = typeof(TAttribute);

            foreach (var field in inType.GetFields(inFlags))
            {
                if (field.IsDefined(attributeType, inbInherit))
                {
                    foreach (var customAttribute in field.GetCustomAttributes(attributeType, inbInherit))
                    {
                        yield return new AttributeBinding<TAttribute, FieldInfo>((TAttribute) customAttribute, field);
                    }
                }
            }
        }

        #endregion // Find Fields

        #region Find Properties

        /// <summary>
        /// Finds all properties in all loaded assemblies with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, PropertyInfo>> FindAllProperties<TAttribute>(BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            return FindProperties<TAttribute>(AppDomain.CurrentDomain.GetAssemblies(), inFlags, inbInherit);
        }

        /// <summary>
        /// Finds all properties in the given assemblies with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, PropertyInfo>> FindProperties<TAttribute>(IEnumerable<Assembly> inAssemblies, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            foreach (var assembly in inAssemblies)
            {
                foreach (var binding in FindProperties<TAttribute>(assembly, inFlags, inbInherit))
                {
                    yield return binding;
                }
            }
        }

        /// <summary>
        /// Finds all properties in the given assembly with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, PropertyInfo>> FindProperties<TAttribute>(Assembly inAssembly, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            return FindProperties<TAttribute>(inAssembly.GetTypes(), inFlags, inbInherit);
        }

        /// <summary>
        /// Finds all properties in the given types with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, PropertyInfo>> FindProperties<TAttribute>(IEnumerable<Type> inTypes, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            foreach (var type in inTypes)
            {
                foreach (var binding in FindProperties<TAttribute>(type, inFlags, inbInherit))
                {
                    yield return binding;
                }
            }
        }

        /// <summary>
        /// Finds all properties in the given type with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, PropertyInfo>> FindProperties<TAttribute>(Type inType, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            Type attributeType = typeof(TAttribute);

            foreach (var property in inType.GetProperties(inFlags))
            {
                if (property.IsDefined(attributeType, inbInherit))
                {
                    foreach (var customAttribute in property.GetCustomAttributes(attributeType, inbInherit))
                    {
                        yield return new AttributeBinding<TAttribute, PropertyInfo>((TAttribute) customAttribute, property);
                    }
                }
            }
        }

        #endregion // Properties

        #region Find Events

        /// <summary>
        /// Finds all events in all loaded assemblies with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, EventInfo>> FindAllEvents<TAttribute>(BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            return FindEvents<TAttribute>(AppDomain.CurrentDomain.GetAssemblies(), inFlags, inbInherit);
        }

        /// <summary>
        /// Finds all events in the given assemblies with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, EventInfo>> FindEvents<TAttribute>(IEnumerable<Assembly> inAssemblies, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            foreach (var assembly in inAssemblies)
            {
                foreach (var binding in FindEvents<TAttribute>(assembly, inFlags, inbInherit))
                {
                    yield return binding;
                }
            }
        }

        /// <summary>
        /// Finds all events in the given assembly with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, EventInfo>> FindEvents<TAttribute>(Assembly inAssembly, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            return FindEvents<TAttribute>(inAssembly.GetTypes(), inFlags, inbInherit);
        }

        /// <summary>
        /// Finds all events in the given assembly with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, EventInfo>> FindEvents<TAttribute>(IEnumerable<Type> inTypes, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            foreach (var type in inTypes)
            {
                foreach (var binding in FindEvents<TAttribute>(type, inFlags, inbInherit))
                {
                    yield return binding;
                }
            }
        }

        /// <summary>
        /// Finds all events in the given type with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, EventInfo>> FindEvents<TAttribute>(Type inType, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            Type attributeType = typeof(TAttribute);

            foreach (var evt in inType.GetEvents(inFlags))
            {
                if (evt.IsDefined(attributeType, inbInherit))
                {
                    foreach (var customAttribute in evt.GetCustomAttributes(attributeType, inbInherit))
                    {
                        yield return new AttributeBinding<TAttribute, EventInfo>((TAttribute) customAttribute, evt);
                    }
                }
            }
        }

        #endregion // Find Events

        #region Find Members

        /// <summary>
        /// Finds all members in all loaded assemblies with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, MemberInfo>> FindAllMembers<TAttribute>(BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            return FindMembers<TAttribute>(AppDomain.CurrentDomain.GetAssemblies(), inFlags, inbInherit);
        }

        /// <summary>
        /// Finds all members in the given assemblies with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, MemberInfo>> FindMembers<TAttribute>(IEnumerable<Assembly> inAssemblies, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            foreach (var assembly in inAssemblies)
            {
                foreach (var binding in FindMembers<TAttribute>(assembly, inFlags, inbInherit))
                {
                    yield return binding;
                }
            }
        }

        /// <summary>
        /// Finds all members in the given assembly with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, MemberInfo>> FindMembers<TAttribute>(Assembly inAssembly, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            return FindMembers<TAttribute>(inAssembly.GetTypes(), inFlags, inbInherit);
        }

        /// <summary>
        /// Finds all members in the given assembly with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, MemberInfo>> FindMembers<TAttribute>(IEnumerable<Type> inTypes, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            Type attributeType = typeof(TAttribute);

            foreach (var type in inTypes)
            {
                foreach (var customAttribute in type.GetCustomAttributes(attributeType, inbInherit))
                {
                    yield return new AttributeBinding<TAttribute, MemberInfo>((TAttribute) customAttribute, type);
                }

                foreach (var binding in FindMembers<TAttribute>(type, inFlags, inbInherit))
                {
                    yield return binding;
                }
            }
        }

        /// <summary>
        /// Finds all members in the given type with the given attribute type defined.
        /// </summary>
        static public IEnumerable<AttributeBinding<TAttribute, MemberInfo>> FindMembers<TAttribute>(Type inType, BindingFlags inFlags, bool inbInherit = false) where TAttribute : Attribute
        {
            Type attributeType = typeof(TAttribute);

            foreach (var member in inType.GetMembers(inFlags))
            {
                foreach (var customAttribute in member.GetCustomAttributes(attributeType, inbInherit))
                {
                    yield return new AttributeBinding<TAttribute, MemberInfo>((TAttribute) customAttribute, member);
                }
            }
        }

        #endregion // Find Members

        #endregion // Attributes

        #region Assemblies

        /// <summary>
        /// Filter for assembly types.
        /// </summary>
        [Flags]
        public enum AssemblyType
        {
            /// <summary>
            /// System libraries (System.Core)
            /// </summary>
            System = 0x001,

            /// <summary>
            /// Microsoft libraries (mscorlib)
            /// </summary>
            Microsoft = 0x002,

            /// <summary>
            /// Mono libraries
            /// </summary>
            Mono = 0x004,

            /// <summary>
            /// NUnit libraries (nunit.framework)
            /// </summary>
            NUnit = 0x008,

            /// <summary>
            /// Default Unity libraries (UnityEngine, UnityEditor)
            /// </summary>
            Unity = 0x010,

            /// <summary>
            /// Default Unity-generated libraries (Assembly-CSharp)
            /// </summary>
            UnityDefaultUser = 0x020,

            /// <summary>
            /// Mask for all assemblies
            /// </summary>
            EmptyMask = 0,

            /// <summary>
            /// Mask for the default non-user assemblies
            /// </summary>
            DefaultNonUserMask = System | Microsoft | Mono | NUnit | Unity
        }

        /// <summary>
        /// Filters all loaded assemblies by type.
        /// </summary>
        static public IEnumerable<Assembly> FindAllAssemblies(AssemblyType inAllowMask, AssemblyType inIgnoreMask)
        {
            return FindAssemblies(AppDomain.CurrentDomain.GetAssemblies(), inAllowMask, inIgnoreMask);
        }

        /// <summary>
        /// Filters all loaded assemblies to the ones that aren't
        /// the default non-user assemblies.
        /// </summary>
        static public IEnumerable<Assembly> FindAllUserAssemblies()
        {
            return FindAllAssemblies(0, AssemblyType.DefaultNonUserMask);
        }

        /// <summary>
        /// Filters the given set of assemblies by type.
        /// </summary>
        static public IEnumerable<Assembly> FindAssemblies(IEnumerable<Assembly> inAssemblies, AssemblyType inAllowMask, AssemblyType inIgnoreMask)
        {
            foreach (var assembly in inAssemblies)
            {
                if (!CheckAssemblyAgainstFilters(assembly, inAllowMask, inIgnoreMask))
                    continue;

                yield return assembly;
            }
        }

        static private bool CheckAssemblyAgainstFilters(Assembly inAssembly, AssemblyType inAllowMask, AssemblyType inIgnoreMask)
        {
            string assemblyName = inAssembly.GetName().Name;
            if (inIgnoreMask != 0)
            {
                if (CheckNameAgainstFilters(assemblyName, inIgnoreMask))
                    return false;
            }
            if (inAllowMask != 0)
            {
                if (!CheckNameAgainstFilters(assemblyName, inAllowMask))
                    return false;
            }
            return true;
        }

        static private bool CheckNameAgainstFilters(string inName, AssemblyType inTypeMask)
        {
            if ((inTypeMask & AssemblyType.System) == AssemblyType.System)
            {
                if (StringUtils.WildcardMatch(inName, SystemAssemblyFilters))
                    return true;
            }
            if ((inTypeMask & AssemblyType.Microsoft) == AssemblyType.Microsoft)
            {
                if (StringUtils.WildcardMatch(inName, MicrosoftAssemblyFilters))
                    return true;
            }
            if ((inTypeMask & AssemblyType.Mono) == AssemblyType.Mono)
            {
                if (StringUtils.WildcardMatch(inName, MonoAssemblyFilters))
                    return true;
            }
            if ((inTypeMask & AssemblyType.NUnit) == AssemblyType.NUnit)
            {
                if (StringUtils.WildcardMatch(inName, NUnitAssemblyFilters))
                    return true;
            }
            if ((inTypeMask & AssemblyType.Unity) == AssemblyType.Unity)
            {
                if (StringUtils.WildcardMatch(inName, UnityAssemblyFilters))
                    return true;
            }
            if ((inTypeMask & AssemblyType.UnityDefaultUser) == AssemblyType.UnityDefaultUser)
            {
                if (StringUtils.WildcardMatch(inName, UnityDefaultUserAssemblyFilters))
                    return true;
            }

            return false;
        }

        static private readonly string[] SystemAssemblyFilters = new string[]
        {
            "System",
            "System.*",
        };

        static private readonly string[] MicrosoftAssemblyFilters = new string[]
        {
            "mscorlib",
            "Microsoft.*"
        };

        static private readonly string[] MonoAssemblyFilters = new string[]
        {
            "Mono.*"
        };

        static private readonly string[] NUnitAssemblyFilters = new string[]
        {
            "nunit.framework"
        };

        static private readonly string[] UnityAssemblyFilters = new string[]
        {
            "Unity",
            "Unity.*",
            "UnityEngine",
            "UnityEngine.*",
            "UnityEditor",
            "UnityEditor.*",
            "Boo.Lang",
            "ExCSS.Unity"
        };

        static private readonly string[] UnityDefaultUserAssemblyFilters = new string[]
        {
            "Assembly-*"
        };

        #endregion // Assemblies
    
        #region Generics

        static private readonly Type GenericIEnumerableType = typeof(IEnumerable<>);

        /// <summary>
        /// Returns the element type of the given type, if it's an IEnumerable.
        /// </summary>
        static public Type GetEnumerableType(this Type inType)
        {
            if (inType == null)
                throw new ArgumentNullException("inType");

            if (inType.IsGenericType)
            {
                if (inType.GetGenericTypeDefinition() == GenericIEnumerableType)
                    return inType.GetGenericArguments()[0];
            }

            foreach(var interfaceType in inType.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == GenericIEnumerableType)
                    return interfaceType.GetGenericArguments()[0];
            }

            return null;
        }

        #endregion // Generics
    }

    /// <summary>
    /// Binding of an attribute to a member.
    /// </summary>
    public struct AttributeBinding<TAttribute, TInfo>
        where TAttribute : Attribute
		where TInfo : MemberInfo
    {
        public readonly TAttribute Attribute;
        public readonly TInfo Info;

        public AttributeBinding(TAttribute inAttribute, TInfo inInfo)
        {
            Attribute = inAttribute;
            Info = inInfo;
        }
    }
}