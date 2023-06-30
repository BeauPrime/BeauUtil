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
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Vertex utility methods.
    /// </summary>
    static public class VertexUtility
    {
        /// <summary>
        /// Generates a VertexLayout instance describing the given vertex format type.
        /// </summary>
        static public VertexLayout GenerateLayout(Type inVertexType, int inStream = 0)
        {
            if (inVertexType == null)
                throw new ArgumentNullException("inVertexType");

            if (!inVertexType.IsValueType || inVertexType.StructLayoutAttribute == null || inVertexType.StructLayoutAttribute.Value != LayoutKind.Sequential || inVertexType.StructLayoutAttribute.Pack != 1)
                throw new InvalidOperationException(string.Format("Type '{0}' is not valid as a vertex - must be sequential layout with pack = 1", inVertexType.FullName));

            unsafe
            {
                byte* offsets = stackalloc byte[14];
                ushort attributeMask = 0;

                FieldInfo[] fields = inVertexType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                VertexAttributeDescriptor[] descriptors = new VertexAttributeDescriptor[fields.Length];
                int descriptorCount = 0;
                int offset = 0;

                foreach (var field in fields)
                {
                    VertexAttrAttribute attr = Reflect.GetAttribute<VertexAttrAttribute>(field);
                    if (attr != null)
                    {
                        VertexAttributeDescriptor descriptor = VertexAttrAttribute.AsDescriptor(attr, field, inStream);
                        descriptors[descriptorCount++] = descriptor;
                        offsets[(int) descriptor.attribute] = (byte) offset;
                        offset += GetSize(descriptor.format) * descriptor.dimension;
                        attributeMask |= (ushort) (1 << (int) descriptor.attribute);
                    }
                }

                if (descriptorCount == 0)
                    throw new InvalidOperationException(string.Format("Type '{0}' had no valid fields marked with VertexAttr", inVertexType.FullName));

                Array.Resize(ref descriptors, descriptorCount);
                VertexLayout layout;
                layout.Descriptors = descriptors;
                layout.AttributeMask = attributeMask;
                layout.SourceType = inVertexType;
                layout.Stride = Marshal.SizeOf(inVertexType);
                for (int i = 0; i < 14; i++)
                    layout.AttributeOffsets[i] = offsets[i];

                return layout;
            }
        }

        #region Mesh Data

        /// <summary>
        /// Maximum size of a mesh vertex stream (2 GiB).
        /// </summary>
        public const long MaxMeshVertexStreamSize = 2147483648L;

        /// <summary>
        /// Maximum size of a 16-bit mesh index buffer (1 GiB).
        /// </summary>
        public const int MaxMeshIndexStreamSize16 = 1073741824 / 2;

        /// <summary>
        /// Maximum size of a 32-bit mesh index buffer (1 GiB).
        /// </summary>
        public const int MaxMeshIndexStreamSize32 = 1073741824 / 4;

        /// <summary>
        /// Writes mesh data to the given mesh and clears the mesh data.
        /// </summary>
        static public void Flush(this IMeshData ioData, Mesh ioMesh)
        {
            ioData.Upload(ioMesh);
            ioData.Clear();
        }

        #endregion // Mesh Data

        #region Size

        static private int GetSize(VertexAttributeFormat inFormat)
        {
            switch (inFormat)
            {
                case VertexAttributeFormat.Float32:
                case VertexAttributeFormat.SInt32:
                case VertexAttributeFormat.UInt32:
                default:
                    return 4;
                case VertexAttributeFormat.Float16:
                case VertexAttributeFormat.UInt16:
                case VertexAttributeFormat.SInt16:
                case VertexAttributeFormat.UNorm16:
                case VertexAttributeFormat.SNorm16:
                    return 2;
                case VertexAttributeFormat.UNorm8:
                case VertexAttributeFormat.SNorm8:
                case VertexAttributeFormat.SInt8:
                case VertexAttributeFormat.UInt8:
                    return 1;
            }
        }

        #endregion // Size
    }

    /// <summary>
    /// Vertex layout data.
    /// </summary>
    public struct VertexLayout
    {
        /// <summary>
        /// Length of the vertex.
        /// </summary>
        public int Stride;

        /// <summary>
        /// Mask representing which vertex attributes are present.
        /// </summary>
        public ushort AttributeMask;
        
        /// <summary>
        /// Field offsets for each vertex attribute type.
        /// </summary>
        public unsafe fixed byte AttributeOffsets[14];

        /// <summary>
        /// Array of descriptors to use with Mesh.
        /// </summary>
        public VertexAttributeDescriptor[] Descriptors;

        /// <summary>
        /// Source vertex type.
        /// </summary>
        public Type SourceType;

        /// <summary>
        /// Returns the offset for the given attribute.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Offset(VertexAttribute inAttribute)
        {
            unsafe
            {
                return AttributeOffsets[(int) inAttribute];
            }
        }

        /// <summary>
        /// Returns if the given attribute is present.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(VertexAttribute inAttribute)
        {
            return (AttributeMask & (1 << (int) inAttribute)) != 0;
        }

        static public implicit operator bool(VertexLayout layout)
        {
            return layout.SourceType != null;
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
    /// Position, color, uv.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexP3C1U2
    {
        [VertexAttr(VertexAttribute.Position)] public Vector3 Position;
        [VertexAttr(VertexAttribute.Color)] public Color32 Color;
        [VertexAttr(VertexAttribute.TexCoord0)] public Vector2 UV;
    }

    /// <summary>
    /// Position, color.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexP3C1
    {
        [VertexAttr(VertexAttribute.Position)] public Vector3 Position;
        [VertexAttr(VertexAttribute.Color)] public Color32 Color;
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