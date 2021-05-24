/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    6 Sept 2020
 * 
 * File:    Variant.cs
 * Purpose: Simple compact data container.
 *          Can hold bools, ints, floats, and string hashes.
 */

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BeauUtil.Variants
{
    /// <summary>
    /// Data variant.
    /// Can be null, or can contain a bool, an int, a float, or a string hash.
    /// </summary>
    [DebuggerDisplay("{ToDebugString()}")]
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct Variant : IEquatable<Variant>, IComparable<Variant>, IDebugString
        #if USING_BEAUDATA
        , BeauData.ISerializedObject
        #endif // USING_BEAUDATA
    {
        [FieldOffset(0)] private VariantType m_Type;
        
        [FieldOffset(4)] private bool m_BoolValue;
        [FieldOffset(4)] private int m_IntValue;
        [FieldOffset(4)] private uint m_UIntValue;
        [FieldOffset(4)] private float m_FloatValue;
        [FieldOffset(4)] private StringHash32 m_StringHashValue;

        [FieldOffset(4)] internal readonly uint RawValue;

        #region Constructors

        internal Variant(VariantType inType, uint inRaw)
            : this()
        {
            m_Type = inType;
            RawValue = inRaw;
        }

        public Variant(bool inbValue)
            : this()
        {
            m_Type = VariantType.Bool;
            m_BoolValue = inbValue;
        }

        public Variant(int inValue)
            : this()
        {
            m_Type = VariantType.Int;
            m_IntValue = inValue;
        }

        public Variant(uint inValue)
            : this()
        {
            m_Type = VariantType.UInt;
            m_UIntValue = inValue;
        }

        public Variant(float inValue)
            : this()
        {
            m_Type = VariantType.Float;
            m_FloatValue = inValue;
        }

        public Variant(StringHash32 inValue)
            : this()
        {
            m_Type = VariantType.StringHash;
            m_StringHashValue = inValue;
        }

        #endregion // Constructors

        #region Cast

        public VariantType Type { get { return m_Type; } }

        public bool AsBool()
        {
            switch(m_Type)
            {
                case VariantType.Null:
                    return false;

                case VariantType.Bool:
                    return m_BoolValue;

                case VariantType.Int:
                    return m_IntValue > 0;

                case VariantType.UInt:
                    return m_UIntValue > 0;

                case VariantType.Float:
                    return m_FloatValue > 0;

                case VariantType.StringHash:
                    return !m_StringHashValue.IsEmpty;

                default:
                    throw new InvalidOperationException("Variant type " + m_Type.ToString() + " not recognized");
            }
        }

        public int AsInt()
        {
            switch(m_Type)
            {
                case VariantType.Int:
                    return m_IntValue;

                case VariantType.UInt:
                    return (int) m_UIntValue;

                case VariantType.Float:
                    return (int) m_FloatValue;

                case VariantType.Null:
                    return 0;

                default:
                    throw new InvalidOperationException("Variant type " + m_Type.ToString() + " cannot be treated as an int");
            }
        }

        public uint AsUInt()
        {
            switch(m_Type)
            {
                case VariantType.Int:
                    return (uint) m_IntValue;

                case VariantType.UInt:
                    return m_UIntValue;

                case VariantType.Float:
                    return (uint) m_FloatValue;

                case VariantType.Null:
                    return 0;

                default:
                    throw new InvalidOperationException("Variant type " + m_Type.ToString() + " cannot be treated as a uint");
            }
        }

        public float AsFloat()
        {
            switch(m_Type)
            {
                case VariantType.Float:
                    return m_FloatValue;

                case VariantType.Int:
                    return m_IntValue;

                case VariantType.UInt:
                    return m_UIntValue;

                case VariantType.Null:
                    return 0;

                default:
                    throw new InvalidOperationException("Variant type " + m_Type.ToString() + " cannot be treated as a float");
            }
        }

        public StringHash32 AsStringHash()
        {
            switch(m_Type)
            {
                case VariantType.Null:
                    return StringHash32.Null;

                case VariantType.StringHash:
                    return m_StringHashValue;

                default:
                    throw new InvalidOperationException("Variant type " + m_Type.ToString() + " cannot be treated as a string hash");
            }
        }

        #endregion // Cast

        #region Consts

        static public readonly Variant Null = new Variant();
        static public readonly Variant True = new Variant(true);
        static public readonly Variant False = new Variant(false);
        static public readonly Variant Zero = new Variant(0);
        static public readonly Variant One = new Variant(1);

        #endregion // Consts

        #region IEquatable

        public bool Equals(Variant other)
        {   
            if (other.m_Type == VariantType.Bool)
                return AsBool() == other.m_BoolValue;

            switch(m_Type)
            {
                case VariantType.Null:
                    {
                        if (other.m_Type == VariantType.Null)
                            return true;
                        return other.Equals(this);
                    }

                case VariantType.Bool:
                    return m_BoolValue == other.AsBool();

                case VariantType.Int:
                    {
                        if (other.m_Type == VariantType.Null)
                        {
                            return m_IntValue == 0;
                        }
                        else if (other.m_Type == VariantType.Int)
                        {
                            return m_IntValue == other.m_IntValue;
                        }
                        else if (other.m_Type == VariantType.UInt)
                        {
                            return m_IntValue == other.m_UIntValue;
                        }
                        else if (other.m_Type == VariantType.Float)
                        {
                            return Mathf.Approximately(m_IntValue, other.m_FloatValue);
                        }
                        else
                        {
                            return false;
                        }
                    }

                case VariantType.UInt:
                    {
                        if (other.m_Type == VariantType.Null)
                        {
                            return m_UIntValue == 0;
                        }
                        else if (other.m_Type == VariantType.Int)
                        {
                            return m_UIntValue == other.m_IntValue;
                        }
                        else if (other.m_Type == VariantType.UInt)
                        {
                            return m_UIntValue == other.m_UIntValue;
                        }
                        else if (other.m_Type == VariantType.Float)
                        {
                            return Mathf.Approximately(m_UIntValue, other.m_FloatValue);
                        }
                        else
                        {
                            return false;
                        }
                    }

                case VariantType.Float:
                    {
                        if (CanBeTreatedAsNumber(other.m_Type))
                        {
                            return Mathf.Approximately(m_FloatValue, other.AsFloat());
                        }
                        else
                        {
                            return false;
                        }
                    }

                case VariantType.StringHash:
                    {
                        if (other.m_Type == VariantType.Null)
                            return m_StringHashValue.IsEmpty;
                        return other.m_Type == VariantType.StringHash && m_StringHashValue == other.m_StringHashValue;
                    }
                
                default:
                    throw new InvalidOperationException("Unrecognized variant type " + m_Type.ToString());
            }
        }

        public bool StrictEquals(Variant inOther)
        {
            return m_Type == inOther.m_Type && RawValue == inOther.RawValue;
        }

        #endregion // IEquatable

        #region IComparable

        public int CompareTo(Variant other)
        {
            switch(m_Type)
            {
                case VariantType.Null:
                    {
                        if (other.m_Type == VariantType.Null)
                        {
                            return 0;
                        }
                        return -other.CompareTo(this);
                    }

                case VariantType.Bool:
                    {
                        if (other.m_Type == VariantType.Bool) 
                        {
                            return m_BoolValue.CompareTo(other.m_BoolValue);
                        }
                        else if (other.m_Type == VariantType.Null)
                        {
                            return m_BoolValue.CompareTo(false);
                        }
                        break;
                    }

                case VariantType.Int:
                    {
                        if (other.m_Type == VariantType.Int)
                        {
                            return m_IntValue.CompareTo(other.m_IntValue);
                        }
                        else if (other.m_Type == VariantType.Float)
                        {
                            return ((float) m_IntValue).CompareTo(other.m_FloatValue);
                        }
                        else if (other.m_Type == VariantType.Null)
                        {
                            return m_IntValue.CompareTo(0);
                        }
                        break;
                    }

                case VariantType.Float:
                    {
                        if (CanBeTreatedAsNumber(other.m_Type))
                        {
                            return m_FloatValue.CompareTo(other.AsFloat());
                        }
                        else if (other.m_Type == VariantType.Null)
                        {
                            return m_FloatValue.CompareTo(0f);
                        }
                        break;
                    }

                case VariantType.StringHash:
                    {
                        if (other.m_Type == VariantType.StringHash)
                        {
                            return m_StringHashValue.CompareTo(other.m_StringHashValue);
                        }
                        else if (other.m_Type == VariantType.Null)
                        {
                            return m_StringHashValue.CompareTo(StringHash32.Null);
                        }
                        break;
                    }
                
                default:
                    throw new InvalidOperationException("Unrecognized variant type " + m_Type.ToString());
            }

            throw new InvalidOperationException(string.Format("Cannot compare variant types {0} and {1}", m_Type, other.m_Type));
        }

        #endregion // IComparable

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is Variant)
                return Equals((Variant) obj);

            return false;
        }

        public override int GetHashCode()
        {
            return (m_Type.GetHashCode() << 5) ^ RawValue.GetHashCode();
        }

        public override string ToString()
        {
            switch(m_Type)
            {
                case VariantType.Null:
                    return string.Empty;
                case VariantType.Bool:
                    return m_BoolValue ? "true" : "false";
                case VariantType.Int:
                    return m_IntValue.ToString();
                case VariantType.UInt:
                    return m_UIntValue.ToString("X8");
                case VariantType.Float:
                    return m_FloatValue.ToString();
                case VariantType.StringHash:
                    return m_StringHashValue.ToString();
                
                default:
                    throw new InvalidOperationException("Unrecognized variant type " + m_Type.ToString());
            }
        }

        public string ToDebugString()
        {
            switch(m_Type)
            {
                case VariantType.Null:
                    return "null";
                case VariantType.Bool:
                    return m_BoolValue ? "true" : "false";
                case VariantType.Int:
                    return m_IntValue.ToString();
                case VariantType.UInt:
                    return m_UIntValue.ToString("X8");
                case VariantType.Float:
                    return m_FloatValue.ToString();
                case VariantType.StringHash:
                    return string.Format("\"{0}\"", m_StringHashValue.ToDebugString());
                
                default:
                    throw new InvalidOperationException("Unrecognized variant type " + m_Type.ToString());
            }
        }

        #endregion // Overrides

        #region Operators

        static public bool operator==(Variant left, Variant right)
        {
            return left.Equals(right);
        }

        static public bool operator!=(Variant left, Variant right)
        {
            return !left.Equals(right);
        }

        static public bool operator <(Variant left, Variant right)
        {
            return left.CompareTo(right) < 0;
        }

        static public bool operator <=(Variant left, Variant right)
        {
            return left.CompareTo(right) <= 0;
        }

        static public bool operator >(Variant left, Variant right)
        {
            return left.CompareTo(right) > 0;
        }

        static public bool operator >=(Variant left, Variant right)
        {
            return left.CompareTo(right) >= 0;
        }

        static public Variant operator+(Variant left, Variant right)
        {
            if (!CanBeTreatedAsNumber(left.m_Type) || !CanBeTreatedAsNumber(right.m_Type))
                throw new InvalidCastException(string.Format("Cannot perform math on non-numeric types '{0}' and '{1}'", left.m_Type, right.m_Type));

            if (left.m_Type == VariantType.Float || right.m_Type == VariantType.Float)
            {
                return new Variant(left.AsFloat() + right.AsFloat());
            }

            return new Variant(left.AsInt() + right.AsInt());
        }

        static public Variant operator-(Variant left, Variant right)
        {
            if (!CanBeTreatedAsNumber(left.m_Type) || !CanBeTreatedAsNumber(right.m_Type))
                throw new InvalidCastException(string.Format("Cannot perform math on non-numeric types '{0}' and '{1}'", left.m_Type, right.m_Type));

            if (left.m_Type == VariantType.Float || right.m_Type == VariantType.Float)
            {
                return new Variant(left.AsFloat() - right.AsFloat());
            }

            return new Variant(left.AsInt() - right.AsInt());
        }

        static public Variant operator*(Variant left, Variant right)
        {
            if (!CanBeTreatedAsNumber(left.m_Type) || !CanBeTreatedAsNumber(right.m_Type))
                throw new InvalidCastException(string.Format("Cannot perform math on non-numeric types '{0}' and '{1}'", left.m_Type, right.m_Type));

            if (left.m_Type == VariantType.Float || right.m_Type == VariantType.Float)
            {
                return new Variant(left.AsFloat() * right.AsFloat());
            }

            return new Variant(left.AsInt() * right.AsInt());
        }

        static public Variant operator/(Variant left, Variant right)
        {
            if (!CanBeTreatedAsNumber(left.m_Type) || !CanBeTreatedAsNumber(right.m_Type))
                throw new InvalidCastException(string.Format("Cannot perform math on non-numeric types '{0}' and '{1}'", left.m_Type, right.m_Type));

            return new Variant(left.AsFloat() / right.AsFloat());
        }

        static public Variant operator++(Variant value)
        {
            if (!CanBeTreatedAsNumber(value.m_Type))
                throw new InvalidCastException(string.Format("Cannot perform math on non-numeric type '{0}'", value.m_Type));

            if (value.m_Type == VariantType.Float)
            {
                return new Variant(value.AsFloat() + 1);
            }

            return new Variant(value.AsInt() + 1);
        }

        static public Variant operator--(Variant value)
        {
            if (!CanBeTreatedAsNumber(value.m_Type))
                throw new InvalidCastException(string.Format("Cannot perform math on non-numeric type '{0}'", value.m_Type));

            if (value.m_Type == VariantType.Float)
            {
                return new Variant(value.AsFloat() - 1);
            }

            return new Variant(value.AsInt() - 1);
        }

        static public implicit operator Variant(bool inbValue)
        {
            return new Variant(inbValue);
        }

        static public implicit operator Variant(int inValue)
        {
            return new Variant(inValue);
        }

        static public implicit operator Variant(uint inValue)
        {
            return new Variant(inValue);
        }

        static public implicit operator Variant(float inValue)
        {
            return new Variant(inValue);
        }

        static public implicit operator Variant(string inValue)
        {
            return inValue == null ? Variant.Null : new Variant(new StringHash32(inValue));
        }

        static public implicit operator Variant(StringHash32 inValue)
        {
            return new Variant(inValue);
        }

        #endregion // Operators

        #region Parse

        /// <summary>
        /// Attempts to parse a string slice into a variant.
        /// </summary>
        static public bool TryParse(StringSlice inSlice, out Variant outValue)
        {
            return TryParse(inSlice, false, out outValue);
        }

        /// <summary>
        /// Attempts to parse a string slice into a variant.
        /// </summary>
        static public bool TryParse(StringSlice inSlice, bool inbAllowImplicitHash, out Variant outValue)
        {
            inSlice = inSlice.Trim();

            if (inSlice.Length <= 0 || inSlice.Equals("null", true))
            {
                outValue = Variant.Null;
                return true;
            }

            if (inSlice.StartsWith(StringHashing.CustomHashPrefix) || inSlice.StartsWith(StringHashing.StringPrefix)
                || (inSlice.StartsWith('"') && inSlice.EndsWith('"')))
            {
                StringHash32 hash;
                if (StringHash32.TryParse(inSlice, out hash))
                {
                    outValue = hash;
                    return true;
                }
            }

            bool bBoolVal;
            if (StringParser.TryParseBool(inSlice, out bBoolVal))
            {
                outValue = new Variant(bBoolVal);
                return true;
            }

            int intVal;
            if (StringParser.TryParseInt(inSlice, out intVal))
            {
                outValue = new Variant(intVal);
                return true;
            }

            uint uintVal;
            if (StringParser.TryParseUInt(inSlice, out uintVal))
            {
                outValue = new Variant(uintVal);
                return true;
            }

            float floatVal;
            if (StringParser.TryParseFloat(inSlice, out floatVal))
            {
                outValue = new Variant(floatVal);
                return true;
            }

            if (inbAllowImplicitHash)
            {
                StringHash32 hash;
                if (StringHash32.TryParse(inSlice, out hash))
                {
                    outValue = hash;
                    return true;
                }
            }

            outValue = default(Variant);
            return false;
        }

        /// <summary>
        /// Parses the string slice into a variant.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public Variant Parse(StringSlice inSlice, bool inbAllowImplicitHash, Variant inDefault = default(Variant))
        {
            Variant val;
            if (!TryParse(inSlice, inbAllowImplicitHash, out val))
                val = inDefault;
            return val;
        }

        /// <summary>
        /// Parses the string slice into a variant.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public Variant Parse(StringSlice inSlice, Variant inDefault = default(Variant))
        {
            return Parse(inSlice, false, inDefault);
        }

        #endregion // Parse

        #region Utilities

        static internal bool CanBeTreatedAsNumber(VariantType inType)
        {
            return inType <= VariantType.Float;
        }

        #endregion // Utilities

        #region ISerializedObject

        #if USING_BEAUDATA

        public void Serialize(BeauData.Serializer ioSerializer)
        {
            ioSerializer.Enum("type", ref m_Type);

            switch(m_Type)
            {
                case VariantType.Bool:
                    {
                        ioSerializer.Serialize("value", ref m_BoolValue);
                        break;
                    }

                case VariantType.Float:
                    {
                        ioSerializer.Serialize("value", ref m_FloatValue);
                        break;
                    }

                case VariantType.Int:
                    {
                        ioSerializer.Serialize("value", ref m_IntValue);
                        break;
                    }

                case VariantType.UInt:
                    {
                        ioSerializer.Serialize("value", ref m_UIntValue);
                        break;
                    }

                case VariantType.StringHash:
                    {
                        ioSerializer.UInt32Proxy("value", ref m_StringHashValue);
                        break;
                    }
            }
        }

        #endif // USING_BEAUDATA

        #endregion // ISerializedObject
    }

    /// <summary>
    /// All possible types of values in a Variant.
    /// </summary>
    public enum VariantType : byte
    {
        Null = 0,
        Int,
        UInt,
        Float,

        Bool,
        StringHash
    }
}