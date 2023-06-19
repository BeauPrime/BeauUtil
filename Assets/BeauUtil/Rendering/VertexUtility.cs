/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    19 June 2023
 * 
 * File:    VertexUtility.cs
 * Purpose: Vertex utilities.
*/

using System.Reflection;
using System.Runtime.InteropServices;
using System;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Vertex utility methods.
    /// </summary>
    static public class VertexUtility
    {
        /// <summary>
        /// Generates an array of VertexAttributeDescriptor instances
        /// describing the given vertex format type.
        /// </summary>
        static public VertexAttributeDescriptor[] GenerateLayout(Type inVertexType, int inStream = 0)
        {
            if (inVertexType == null)
                throw new ArgumentNullException("inVertexType");

            if (!inVertexType.IsValueType || inVertexType.StructLayoutAttribute == null || inVertexType.StructLayoutAttribute.Value != LayoutKind.Sequential || inVertexType.StructLayoutAttribute.Pack != 1)
                throw new InvalidOperationException(string.Format("Type '{0}' is not valid as a vertex - must be sequential layout with pack = 1", inVertexType.FullName));

            FieldInfo[] fields = inVertexType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            VertexAttributeDescriptor[] descriptors = new VertexAttributeDescriptor[fields.Length];
            int descriptorCount = 0;

            foreach (var field in fields)
            {
                VertexAttrAttribute attr = Reflect.GetAttribute<VertexAttrAttribute>(field);
                if (attr != null)
                {
                    descriptors[descriptorCount++] = VertexAttrAttribute.AsDescriptor(attr, field, inStream);
                }
            }

            if (descriptorCount == 0)
                throw new InvalidOperationException(string.Format("Type '{0}' had no valid fields marked with VertexAttr", inVertexType.FullName));

            Array.Resize(ref descriptors, descriptorCount);
            return descriptors;
        }
    }

    /// <summary>
    /// Default sprite renderer vertex format.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DefaultSpriteVertexFormat
    {
        [VertexAttr(VertexAttribute.Position)] public Vector4 Position;
        [VertexAttr(VertexAttribute.Color)] public Color Color;
        [VertexAttr(VertexAttribute.TexCoord0)] public Vector2 UV;
    }

    /// <summary>
    /// Attribute used for generating vertex attribute descriptors.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class VertexAttrAttribute : PreserveAttribute
    {
        public VertexAttribute Attribute;
        public VertexAttributeFormat? Format;
        public int Dimension;

        public VertexAttrAttribute(VertexAttribute inAttribute, int inDimension = 1)
        {
            Attribute = inAttribute;
            Format = null;
            Dimension = inDimension;
        }

        public VertexAttrAttribute(VertexAttribute inAttribute, VertexAttributeFormat inFormat, int inDimension = 1)
        {
            Attribute = inAttribute;
            Format = inFormat;
            Dimension = inDimension;
        }

        /// <summary>
        /// Creates a VertexAttributeDescriptor from the given field and attribute.
        /// </summary>
        static public VertexAttributeDescriptor AsDescriptor(VertexAttrAttribute inAttribute, FieldInfo inFieldInfo, int inStream = 0)
        {
            VertexAttributeFormat format;
            int dimension = inAttribute.Dimension;

            if (inAttribute.Format.HasValue)
            {
                format = inAttribute.Format.Value;
            }
            else
            {
                Type fieldType = inFieldInfo.FieldType;
                switch (Type.GetTypeCode(fieldType))
                {
                    case TypeCode.Byte:
                        format = VertexAttributeFormat.UInt8;
                        break;
                    case TypeCode.SByte:
                        format = VertexAttributeFormat.SInt8;
                        break;
                    case TypeCode.UInt16:
                        format = VertexAttributeFormat.UInt16;
                        break;
                    case TypeCode.Int16:
                        format = VertexAttributeFormat.SInt16;
                        break;
                    case TypeCode.UInt32:
                        format = VertexAttributeFormat.UInt32;
                        break;
                    case TypeCode.Int32:
                        format = VertexAttributeFormat.SInt32;
                        break;
                    case TypeCode.Single:
                        format = VertexAttributeFormat.Float32;
                        break;
                    case TypeCode.Object:
                        if (fieldType == typeof(Vector2))
                        {
                            format = VertexAttributeFormat.Float32;
                            dimension *= 2;
                        }
                        else if (fieldType == typeof(Vector3))
                        {
                            format = VertexAttributeFormat.Float32;
                            dimension *= 3;
                        }
                        else if (fieldType == typeof(Vector4))
                        {
                            format = VertexAttributeFormat.Float32;
                            dimension *= 4;
                        }
                        else if (fieldType == typeof(Quaternion))
                        {
                            format = VertexAttributeFormat.Float32;
                            dimension *= 4;
                        }
                        else if (fieldType == typeof(Color32))
                        {
                            format = VertexAttributeFormat.UNorm8;
                            dimension *= 4;
                        }
                        else if (fieldType == typeof(Color))
                        {
                            format = VertexAttributeFormat.Float32;
                            dimension *= 4;
                        }
                        else
                        {
                            throw new InvalidOperationException(string.Format("Vertex attribute format for type '{0}' cannot be automatically inferred - specify vertex attribute format manually", fieldType.FullName));
                        }
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Vertex attribute format for type '{0}' cannot be automatically inferred - specify vertex attribute format manually", fieldType.FullName));
                }
            }

            return new VertexAttributeDescriptor(inAttribute.Attribute, format, dimension, inStream);
        }
    }
}