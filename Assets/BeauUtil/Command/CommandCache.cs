// /*
//  * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
//  * Author:  Autumn Beauchesne
//  * Date:    30 August 2020
//  * 
//  * File:    CommandCache.cs
//  * Purpose: Cache for command attributes.
//  */

// using System;
// using System.Collections.Generic;
// using System.Reflection;
// using BeauUtil.Tags;
// using UnityEngine;

// namespace BeauUtil.Command
// {
//     /// <summary>
//     /// Cache for command metadata.
//     /// </summary>
//     public class CommandCache
//     {
//         #region Static

//         /// <summary>
//         /// Default cache for command info.
//         /// </summary>
//         static public readonly CommandCache Default = new CommandCache();

//         #endregion // Static

//         #region Types

//         protected const BindingFlags CommandAttributeSearch = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

//         [Flags]
//         protected enum InvocationFlags
//         {
//             Field = 0x01,
//             Method = 0x02,
//             Property = 0x04,

//             Static = 0x40,
//             GlobalNamespace = 0x80
//         }

//         private class TypeInfo : IDisposable
//         {
//             internal Type Type;

//             private Dictionary<StringSlice, MetaInfo> m_MetaCommands;
//             private ContentInfo m_ContentCommand;

//             internal TypeInfo(Type inType)
//             {
//                 Type = inType;

//                 m_MetaCommands = new Dictionary<StringSlice, MetaInfo>(8);
//             }

//             internal bool TryGetMetaInfo(TagData inData, out MetaInfo outMeta)
//             {
//                 return m_MetaCommands.TryGetValue(inData.Id, out outMeta);
//             }

//             internal ContentInfo ContentInfo()
//             {
//                 return m_ContentCommand;
//             }

//             internal void Process()
//             {
//                 foreach(var metaAttr in Reflect.FindMembers<BlockMetaAttribute>(Type, CommandAttributeSearch))
//                 {
//                     MetaInfo info = new MetaInfo();
//                     info.Name = metaAttr.Attribute.Name;
//                     if (info.Process(metaAttr.Info))
//                     {
//                         if (m_MetaCommands.ContainsKey(info.Name))
//                         {
//                             info.Dispose();
//                             Debug.LogErrorFormat("[BlockMetaCache] Multiple instances of command '{0}' on type '{1}'", info.Name, Type.Name);
//                             continue;
//                         }

//                         m_MetaCommands.Add(info.Name, info);
//                     }
//                     else
//                     {
//                         Debug.LogErrorFormat("[BlockMetaCache] Command '{0}' on type '{1}' is not valid", info.Name, Type.Name);
//                     }
//                 }

//                 foreach(var contentAttr in Reflect.FindMembers<BlockContentAttribute>(Type, ContentAttributeSearch))
//                 {
//                     ContentInfo info = new ContentInfo();
//                     info.Mode = contentAttr.Attribute.Mode;
//                     if (info.Process(contentAttr.Info))
//                     {
//                         if (info.Mode == BlockContentMode.LineByLine && !info.IsMethod())
//                         {
//                             Debug.LogWarningFormat("[BlockMetaCache] Content command '{0}' on type '{1}' is field or property - cannot set method to LineByLine", info.Name, Type.Name);
//                             info.Mode = BlockContentMode.BatchContent;
//                         }

//                         m_ContentCommand = info;
//                         break;
//                     }
//                     else
//                     {
//                         Debug.LogErrorFormat("[BlockMetaCache] Content command '{0}' on type '{1}' is not valid", info.Name, Type.Name);
//                     }
//                 }
//             }

//             public void Dispose()
//             {
//                 Type = null;
//                 foreach(var command in m_MetaCommands.Values)
//                 {
//                     command.Dispose();
//                 }
//                 m_MetaCommands.Clear();
//                 m_MetaCommands = null;

//                 if (m_ContentCommand != null)
//                 {
//                     m_ContentCommand.Dispose();
//                     m_ContentCommand = null;
//                 }
//             }
//         }

//         internal class MetaInfo : IDisposable
//         {
//             internal string Name;

//             private InvocationFlags m_Flags;
//             private FieldInfo m_Field;
//             private PropertyInfo m_Property;
//             private MethodInfo m_Method;

//             private ParameterInfo[] m_Parameters;
//             private object[] m_MethodParams;
//             private object[] m_DefaultParams;
//             private int m_RequiredParamCount;

//             internal bool IsMethod()
//             {
//                 return (m_Flags & InvocationFlags.Method) != 0;
//             }

//             #region Process

//             internal bool Process(MemberInfo inInfo)
//             {
//                 Name = Name ?? inInfo.Name;

//                 if (inInfo.MemberType == MemberTypes.Field)
//                 {
//                     return ProcessField((FieldInfo) inInfo);
//                 }
//                 else if (inInfo.MemberType == MemberTypes.Method)
//                 {
//                     return ProcessMethod((MethodInfo) inInfo);
//                 }
//                 else if (inInfo.MemberType == MemberTypes.Property)
//                 {
//                     return ProcessProperty((PropertyInfo) inInfo);
//                 }

//                 return false;
//             }

//             private bool ProcessField(FieldInfo inField)
//             {
//                 if (!StringParser.CanConvertTo(inField.FieldType))
//                 {
//                     return false;
//                 }

//                 m_Field = inField;

//                 m_Flags |= InvocationFlags.Field;
//                 if (inField.IsStatic)
//                     m_Flags |= InvocationFlags.Static;
                
//                 return true;
//             }

//             private bool ProcessMethod(MethodInfo inMethod)
//             {
//                 if (inMethod.ReturnType != typeof(void))
//                 {
//                     return false;
//                 }

//                 m_Parameters = inMethod.GetParameters();
//                 if (m_Parameters.Length > 0)
//                 {
//                     m_DefaultParams = new object[m_Parameters.Length];

//                     for(int i = 0; i < m_Parameters.Length; ++i)
//                     {
//                         ParameterInfo info = m_Parameters[i];
//                         Type paramType = info.ParameterType;
//                         if (!StringParser.CanConvertTo(paramType))
//                         {
//                             m_Parameters = null;
//                             m_DefaultParams = null;
//                             return false;
//                         }

//                         if ((info.Attributes & ParameterAttributes.HasDefault) != 0)
//                         {
//                             m_DefaultParams[i] = info.DefaultValue;
//                         }
//                         else
//                         {
//                             ++m_RequiredParamCount;
//                         }
//                     }
//                 }

//                 m_Method = inMethod;

//                 m_Flags |= InvocationFlags.Method;
//                 m_MethodParams = new object[m_Parameters.Length];
//                 return true;
//             }
        
//             private bool ProcessProperty(PropertyInfo inProperty)
//             {
//                 if (!StringParser.CanConvertTo(inProperty.PropertyType))
//                     return false;
                
//                 MethodInfo setter = inProperty.SetMethod;
//                 if (setter == null)
//                     return false;

//                 m_Flags |= InvocationFlags.Property;
//                 if (setter.IsStatic)
//                     m_Flags |= InvocationFlags.Static;

//                 m_Method = setter;
//                 m_MethodParams = new object[1];
//                 return true;
//             }

//             #endregion // Process

//             #region Invoke

//             internal bool Invoke(object inThis, StringSlice inData, SplitResources inSplitResources)
//             {
//                 if ((m_Flags & InvocationFlags.Static) != 0)
//                 {
//                     inThis = null;
//                 }

//                 if ((m_Flags & InvocationFlags.Field) != 0)
//                 {
//                     return InvokeField(inThis, inData);
//                 }
//                 if ((m_Flags & InvocationFlags.Method) != 0)
//                 {
//                     return InvokeMethod(inThis, inData, inSplitResources);
//                 }
//                 if ((m_Flags & InvocationFlags.Property) != 0)
//                 {
//                     return InvokeProperty(inThis, inData);
//                 }

//                 return false;
//             }

//             private bool InvokeField(object inThis, StringSlice inData)
//             {
//                 object val;
//                 if (!StringParser.TryConvertTo(inData, m_Field.FieldType, out val))
//                 {
//                     return false;
//                 }

//                 try
//                 {
//                     m_Field.SetValue(inThis, val);
//                     return true;
//                 }
//                 catch(Exception e)
//                 {
//                     Debug.LogException(e);
//                     return false;
//                 }
//             }

//             private bool InvokeMethod(object inThis, StringSlice inData, SplitResources inSplitResources)
//             {
//                 lock(inSplitResources.LockObj)
//                 {
//                     inSplitResources.ArgList.Clear();

//                     int providedArgCount = inData.Split(inSplitResources.Splitter, StringSplitOptions.None, inSplitResources.ArgList);
//                     if (providedArgCount < m_RequiredParamCount || providedArgCount > m_Parameters.Length)
//                         return false;

//                     for(int i = 0; i < providedArgCount; ++i)
//                     {
//                         if (!StringParser.TryConvertTo(inSplitResources.ArgList[i], m_Parameters[i].ParameterType, out m_MethodParams[i]))
//                             return false;
//                     }

//                     for(int i = providedArgCount; i < m_Parameters.Length; ++i)
//                     {
//                         m_MethodParams[i] = m_DefaultParams[i];
//                     }
//                 }
                
//                 try
//                 {
//                     m_Method.Invoke(inThis, m_MethodParams);
//                     return true;
//                 }
//                 catch(Exception e)
//                 {
//                     Debug.LogException(e);
//                     return false;
//                 }
//             }

//             private bool InvokeProperty(object inThis, StringSlice inData)
//             {
//                 if (!StringParser.TryConvertTo(inData, m_Property.PropertyType, out m_MethodParams[0]))
//                     return false;

//                 try
//                 {
//                     m_Method.Invoke(inThis, m_MethodParams);
//                     return true;
//                 }
//                 catch(Exception e)
//                 {
//                     Debug.LogException(e);
//                     return false;
//                 }
//             }

//             #endregion // Invoke

//             public void Dispose()
//             {
//                 Name = null;
//                 m_Flags = 0;
//                 m_Field = null;
//                 m_Property = null;
//                 m_Method = null;
//                 ArrayUtils.Dispose(ref m_Parameters);
//                 ArrayUtils.Dispose(ref m_MethodParams);
//                 ArrayUtils.Dispose(ref m_DefaultParams);
//                 m_RequiredParamCount = 0;
//             }
//         }

//         internal class ContentInfo : MetaInfo
//         {
//             internal BlockContentMode Mode;
//         }

//         private class SplitResources
//         {
//             internal readonly object LockObj;
//             internal readonly List<StringSlice> ArgList;
//             internal readonly StringUtils.ArgsList.Splitter Splitter;

//             internal SplitResources()
//             {
//                 LockObj = new object();
//                 ArgList = new List<StringSlice>(8);
//                 Splitter = new StringUtils.ArgsList.Splitter(true);
//             }
//         }
    
//         #endregion // Types

//         private readonly Dictionary<Type, TypeInfo> m_TypeCache = new Dictionary<Type, TypeInfo>(32);
//         private readonly SplitResources m_SharedSplitter = new SplitResources();

//         /// <summary>
//         /// Attempts to evaluate a command on an object.
//         /// </summary>
//         internal bool TryEvaluateCommand(object inObject, TagData inData)
//         {
//             Type t = inObject.GetType();
//             TypeInfo info = GetCache(t, true);
//             if (info != null)
//             {
//                 MetaInfo meta;
//                 if (!info.TryGetMetaInfo(inData, out meta))
//                     return false;

//                 return meta.Invoke(inObject, inData.Data, m_SharedSplitter);
//             }

//             return false;
//         }

//         private TypeInfo GetCache(Type inType, bool inbCreate)
//         {
//             TypeInfo info;
//             if (!m_TypeCache.TryGetValue(inType, out info) && inbCreate)
//             {
//                 info = new TypeInfo(inType);
//                 info.Process();
//                 m_TypeCache.Add(inType, info);
//             }
//             return info;
//         }

//         /// <summary>
//         /// Caches information for the given type.
//         /// </summary>
//         public void Cache(Type inType)
//         {
//             GetCache(inType, true);
//         }

//         /// <summary>
//         /// Clears out all cached type information.
//         /// </summary>
//         public void ClearCache()
//         {
//             foreach(var info in m_TypeCache.Values)
//             {
//                 info.Dispose();
//             }
//             m_TypeCache.Clear();
//         }
//     }
// }