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

namespace BeauUtil
{
    /// <summary>
    /// Data variant.
    /// Can be null, or can contain a bool, an int, a float, or a string hash.
    /// </summary>
    [DebuggerDisplay("{ToDebugString()}")]
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct Variant : IEquatable<Variant>, IComparable<Variant>
    {
        [FieldOffset(0)] private readonly VariantType m_Type;
        [FieldOffset(4)] private readonly bool m_BoolValue;
        [FieldOffset(4)] private readonly int m_IntValue;
        [FieldOffset(4)] private readonly float m_FloatValue;
        [FieldOffset(4)] private readonly StringHash m_StringHashValue;

        #region Constructors

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

        public Variant(float inValue)
            : this()
        {
            m_Type = VariantType.Float;
            m_FloatValue = inValue;
        }

        public Variant(StringHash inValue)
            : this()
        {
            if (inValue)
            {
                m_Type = VariantType.StringHash;
                m_StringHashValue = inValue;
            }
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

                case VariantType.Float:
                    return (int) m_FloatValue;

                default:
                    throw new InvalidOperationException("Variant type " + m_Type.ToString() + " cannot be treated as an int");
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

                default:
                    throw new InvalidOperationException("Variant type " + m_Type.ToString() + " cannot be treated as a float");
            }
        }

        public StringHash AsStringHash()
        {
            if (m_Type == VariantType.StringHash)
            {
                return m_StringHashValue;
            }

            throw new InvalidOperationException("Variant type " + m_Type.ToString() + " cannot be treated as a string hash");
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
            switch(m_Type)
            {
                case VariantType.Null:
                    return other.m_Type == VariantType.Null;

                case VariantType.Bool:
                    return other.m_Type == VariantType.Bool && m_BoolValue == other.m_BoolValue;

                case VariantType.Int:
                    {
                        if (other.m_Type == VariantType.Int)
                        {
                            return m_IntValue == other.m_IntValue;
                        }
                        else if (other.m_Type == VariantType.Float)
                        {
                            float diff = (float) m_IntValue - other.m_FloatValue;
                            return diff >= -float.Epsilon && diff <= float.Epsilon;
                        }
                        else
                        {
                            return false;
                        }
                    }

                case VariantType.Float:
                    {
                        if (IsNumeric(other.m_Type))
                        {
                            float diff = m_FloatValue - other.AsFloat();
                            return diff >= -float.Epsilon && diff <= float.Epsilon;
                        }
                        else
                        {
                            return false;
                        }
                    }

                case VariantType.StringHash:
                    return other.m_Type == VariantType.StringHash && m_StringHashValue == other.m_StringHashValue;
                
                default:
                    throw new InvalidOperationException("Unrecognized variant type " + m_Type.ToString());
            }
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
                        break;
                    }

                case VariantType.Bool:
                    {
                        if (other.m_Type == VariantType.Bool) 
                        {
                            return m_BoolValue.CompareTo(other.m_BoolValue);
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
                        break;
                    }

                case VariantType.Float:
                    {
                        if (IsNumeric(other.m_Type))
                        {
                            return m_FloatValue.CompareTo(other.AsFloat());
                        }
                        break;
                    }

                case VariantType.StringHash:
                    {
                        if (other.m_Type == VariantType.StringHash)
                        {
                            return m_StringHashValue.CompareTo(other.m_StringHashValue);
                        }
                        break;
                    }
                
                default:
                    throw new InvalidOperationException("Unrecognized variant type " + m_Type.ToString());
            }

            return m_Type < other.m_Type ? -1 : (m_Type > other.m_Type ? 1 : 0);
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
            int hash = m_Type.GetHashCode();
            switch(m_Type)
            {
                case VariantType.Bool:
                    hash = (hash << 5) ^ m_BoolValue.GetHashCode();
                    break;

                case VariantType.Int:
                    hash = (hash << 5) ^ m_IntValue.GetHashCode();
                    break;

                case VariantType.Float:
                    hash = (hash << 5) ^ m_FloatValue.GetHashCode();
                    break;

                case VariantType.StringHash:
                    hash = (hash << 5) ^ m_StringHashValue.GetHashCode();
                    break;
            }
            return hash;
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
            if (!IsNumeric(left.m_Type) || !IsNumeric(right.m_Type))
                throw new InvalidCastException(string.Format("Cannot perform math on non-numeric types '{0}' and '{1}'", left.m_Type, right.m_Type));

            if (left.m_Type == VariantType.Float || right.m_Type == VariantType.Float)
            {
                return new Variant(left.AsFloat() + right.AsFloat());
            }

            return new Variant(left.AsInt() + right.AsInt());
        }

        static public Variant operator-(Variant left, Variant right)
        {
            if (!IsNumeric(left.m_Type) || !IsNumeric(right.m_Type))
                throw new InvalidCastException(string.Format("Cannot perform math on non-numeric types '{0}' and '{1}'", left.m_Type, right.m_Type));

            if (left.m_Type == VariantType.Float || right.m_Type == VariantType.Float)
            {
                return new Variant(left.AsFloat() - right.AsFloat());
            }

            return new Variant(left.AsInt() - right.AsInt());
        }

        static public Variant operator*(Variant left, Variant right)
        {
            if (!IsNumeric(left.m_Type) || !IsNumeric(right.m_Type))
                throw new InvalidCastException(string.Format("Cannot perform math on non-numeric types '{0}' and '{1}'", left.m_Type, right.m_Type));

            if (left.m_Type == VariantType.Float || right.m_Type == VariantType.Float)
            {
                return new Variant(left.AsFloat() * right.AsFloat());
            }

            return new Variant(left.AsInt() * right.AsInt());
        }

        static public Variant operator/(Variant left, Variant right)
        {
            if (!IsNumeric(left.m_Type) || !IsNumeric(right.m_Type))
                throw new InvalidCastException(string.Format("Cannot perform math on non-numeric types '{0}' and '{1}'", left.m_Type, right.m_Type));

            if (left.m_Type == VariantType.Float || right.m_Type == VariantType.Float)
            {
                return new Variant(left.AsFloat() / right.AsFloat());
            }

            return new Variant(left.AsInt() / right.AsInt());
        }

        static public Variant operator++(Variant value)
        {
            if (!IsNumeric(value.m_Type))
                throw new InvalidCastException(string.Format("Cannot perform math on non-numeric type '{0}'", value.m_Type));

            if (value.m_Type == VariantType.Float)
            {
                return new Variant(value.AsFloat() + 1);
            }

            return new Variant(value.AsInt() + 1);
        }

        static public Variant operator--(Variant value)
        {
            if (!IsNumeric(value.m_Type))
                throw new InvalidCastException(string.Format("Cannot perform math on non-numeric type '{0}'", value.m_Type));

            if (value.m_Type == VariantType.Float)
            {
                return new Variant(value.AsFloat() - 1);
            }

            return new Variant(value.AsInt() - 1);
        }

        static public implicit operator bool(Variant inValue)
        {
            return inValue.AsBool();
        }

        static public implicit operator Variant(bool inbValue)
        {
            return new Variant(inbValue);
        }

        static public implicit operator Variant(int inValue)
        {
            return new Variant(inValue);
        }

        static public implicit operator Variant(float inValue)
        {
            return new Variant(inValue);
        }

        static public implicit operator Variant(string inValue)
        {
            return string.IsNullOrEmpty(inValue) ? Variant.Null : new Variant(new StringHash(inValue));
        }

        static public implicit operator Variant(StringHash inValue)
        {
            return inValue.IsEmpty ? Variant.Null : new Variant(inValue);
        }

        #endregion // Operators

        #region Utilities

        static internal bool IsNumeric(VariantType inType)
        {
            return inType == VariantType.Int || inType == VariantType.Float;
        }

        #endregion // Utilities
    }

    /// <summary>
    /// All possible types of values in a Variant.
    /// </summary>
    public enum VariantType : byte
    {
        Null = 0,
        Bool,
        Int,
        Float,
        StringHash
    }
}