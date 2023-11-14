using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BeauUtil.Variants;

namespace BeauUtil
{
    /// <summary>
    /// Alternative to object for variant return values.
    /// Can represent built-in numerics, booleans, chars, strings, and several BeauUtil types.
    /// </summary>
    public readonly struct NonBoxedValue
    {
        public enum ValueType : byte
        {
            Null,
            
            Int8,
            Int16,
            Int32,
            Int64,

            UInt8,
            UInt16,
            UInt32,
            UInt64,

            Float32,
            Float64,

            Char,
            Boolean,

            IntPtr,
            StringHash32,

            String,
            Object,

            StringSlice
        }

        [StructLayout(LayoutKind.Explicit, Size = 8)]
        private unsafe struct PackedValues
        {
            [FieldOffset(0)] public sbyte Int8;
            [FieldOffset(0)] public Int16 Int16;
            [FieldOffset(0)] public Int32 Int32;
            [FieldOffset(0)] public Int64 Int64;

            [FieldOffset(0)] public byte UInt8;
            [FieldOffset(0)] public UInt16 UInt16;
            [FieldOffset(0)] public UInt32 UInt32;
            [FieldOffset(0)] public UInt64 UInt64;

            [FieldOffset(0)] public float Float32;
            [FieldOffset(0)] public double Float64;

            [FieldOffset(0)] public Char Char;
            [FieldOffset(0)] public Boolean Boolean;

            [FieldOffset(0)] public IntPtr IntPtr;
            [FieldOffset(0)] public StringHash32 StringHash32;

            // if storing a 4-byte value, we can store an additional one (for struct packing)

            [FieldOffset(4)] public Int32 Int32_B;
            [FieldOffset(4)] public UInt32 UInt32_B;

            [FieldOffset(4)] public float Float32_B;
            [FieldOffset(4)] public StringHash32 StringHash32_B;
        }
    
        private readonly PackedValues m_Packed;
        private readonly object m_ObjectRef;
        private readonly ValueType m_Type;

        #region Constructors

        public NonBoxedValue(sbyte inValue) : this()
        {
            m_Type = ValueType.Int8;
            m_Packed.Int8 = inValue;
        }

        public NonBoxedValue(Int16 inValue) : this()
        {
            m_Type = ValueType.Int16;
            m_Packed.Int16 = inValue;
        }

        public NonBoxedValue(Int32 inValue) : this()
        {
            m_Type = ValueType.Int32;
            m_Packed.Int32 = inValue;
        }

        public NonBoxedValue(Int64 inValue) : this()
        {
            m_Type = ValueType.Int64;
            m_Packed.Int64 = inValue;
        }

        public NonBoxedValue(byte inValue) : this()
        {
            m_Type = ValueType.UInt8;
            m_Packed.UInt8 = inValue;
        }

        public NonBoxedValue(UInt16 inValue) : this()
        {
            m_Type = ValueType.UInt16;
            m_Packed.UInt16 = inValue;
        }

        public NonBoxedValue(UInt32 inValue) : this()
        {
            m_Type = ValueType.UInt32;
            m_Packed.UInt32 = inValue;
        }

        public NonBoxedValue(UInt64 inValue) : this()
        {
            m_Type = ValueType.UInt64;
            m_Packed.UInt64 = inValue;
        }

        public NonBoxedValue(float inValue) : this()
        {
            m_Type = ValueType.Float32;
            m_Packed.Float32 = inValue;
        }

        public NonBoxedValue(double inValue) : this()
        {
            m_Type = ValueType.Float64;
            m_Packed.Float64 = inValue;
        }

        public NonBoxedValue(char inValue) : this()
        {
            m_Type = ValueType.Char;
            m_Packed.Char = inValue;
        }

        public NonBoxedValue(bool inValue) : this()
        {
            m_Type = ValueType.Boolean;
            m_Packed.Boolean = inValue;
        }

        public NonBoxedValue(IntPtr inValue) : this()
        {
            m_Type = ValueType.IntPtr;
            m_Packed.IntPtr = inValue;
        }

        public NonBoxedValue(StringHash32 inValue) : this()
        {
            m_Type = ValueType.StringHash32;
            m_Packed.StringHash32 = inValue;
        }

        public NonBoxedValue(string inValue) : this()
        {
            if (inValue != null)
            {
                m_Type = ValueType.String;
                m_ObjectRef = inValue;
            }
        }

        public NonBoxedValue(StringSlice inValue) : this()
        {
            m_Type = ValueType.StringSlice;
            string source;
            inValue.Unpack(out source, out m_Packed.Int32, out m_Packed.Int32_B);
            m_ObjectRef = source;
        }

        public NonBoxedValue(Variant inVariant) : this()
        {
            GetInfo(inVariant, ref m_Type, ref m_ObjectRef, ref m_Packed);
        }

        public NonBoxedValue(object inValue) : this()
        {
            GetInfo(inValue, ref m_Type, ref m_ObjectRef, ref m_Packed);
        }

        #endregion // Constructions

        #region Casts

        public ValueType Type { get { return m_Type; } }

        #region Integrals

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsIntegral() { return (m_Type >= ValueType.Int8 && m_Type <= ValueType.UInt64) || m_Type == ValueType.Char; }

        public sbyte AsInt8()
        {
            switch(m_Type)
            {
                case ValueType.Int8:
                    return m_Packed.Int8;
                case ValueType.Int16:
                    return (sbyte) m_Packed.Int16;
                case ValueType.Int32:
                    return (sbyte) m_Packed.Int32;
                case ValueType.Int64:
                    return (sbyte) m_Packed.Int64;

                case ValueType.UInt8:
                    return (sbyte) m_Packed.UInt8;
                case ValueType.UInt16:
                    return (sbyte) m_Packed.UInt16;
                case ValueType.UInt32:
                    return (sbyte) m_Packed.UInt32;
                case ValueType.UInt64:
                    return (sbyte) m_Packed.UInt64;

                case ValueType.Float32:
                    return (sbyte) m_Packed.Float32;
                case ValueType.Float64:
                    return (sbyte) m_Packed.Float64;

                case ValueType.Char:
                    return (sbyte) m_Packed.Char;

                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to an integral", m_Type));
            }
        }

        public Int16 AsInt16()
        {
            switch(m_Type)
            {
                case ValueType.Int8:
                    return (Int16) m_Packed.Int8;
                case ValueType.Int16:
                    return m_Packed.Int16;
                case ValueType.Int32:
                    return (Int16) m_Packed.Int32;
                case ValueType.Int64:
                    return (Int16) m_Packed.Int64;

                case ValueType.UInt8:
                    return (Int16) m_Packed.UInt8;
                case ValueType.UInt16:
                    return (Int16) m_Packed.UInt16;
                case ValueType.UInt32:
                    return (Int16) m_Packed.UInt32;
                case ValueType.UInt64:
                    return (Int16) m_Packed.UInt64;

                case ValueType.Float32:
                    return (Int16) m_Packed.Float32;
                case ValueType.Float64:
                    return (Int16) m_Packed.Float64;

                case ValueType.Char:
                    return (Int16) m_Packed.Char;

                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to an integral", m_Type));
            }
        }

        public Int32 AsInt32()
        {
            switch(m_Type)
            {
                case ValueType.Int8:
                    return (Int32) m_Packed.Int8;
                case ValueType.Int16:
                    return (Int32) m_Packed.Int16;
                case ValueType.Int32:
                    return m_Packed.Int32;
                case ValueType.Int64:
                    return (Int32) m_Packed.Int64;

                case ValueType.UInt8:
                    return (Int32) m_Packed.UInt8;
                case ValueType.UInt16:
                    return (Int32) m_Packed.UInt16;
                case ValueType.UInt32:
                    return (Int32) m_Packed.UInt32;
                case ValueType.UInt64:
                    return (Int32) m_Packed.UInt64;

                case ValueType.Float32:
                    return (Int32) m_Packed.Float32;
                case ValueType.Float64:
                    return (Int32) m_Packed.Float64;

                case ValueType.Char:
                    return (Int32) m_Packed.Char;

                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to an integral", m_Type));
            }
        }

        public Int64 AsInt64()
        {
            switch(m_Type)
            {
                case ValueType.Int8:
                    return (Int64) m_Packed.Int8;
                case ValueType.Int16:
                    return (Int64) m_Packed.Int16;
                case ValueType.Int32:
                    return (Int64) m_Packed.Int32;
                case ValueType.Int64:
                    return m_Packed.Int64;

                case ValueType.UInt8:
                    return (Int64) m_Packed.UInt8;
                case ValueType.UInt16:
                    return (Int64) m_Packed.UInt16;
                case ValueType.UInt32:
                    return (Int64) m_Packed.UInt32;
                case ValueType.UInt64:
                    return (Int64) m_Packed.UInt64;

                case ValueType.Float32:
                    return (Int64) m_Packed.Float32;
                case ValueType.Float64:
                    return (Int64) m_Packed.Float64;

                case ValueType.Char:
                    return (Int64) m_Packed.Char;

                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to an integral", m_Type));
            }
        }

        public byte AsUInt8()
        {
            switch(m_Type)
            {
                case ValueType.Int8:
                    return (byte) m_Packed.Int8;
                case ValueType.Int16:
                    return (byte) m_Packed.Int16;
                case ValueType.Int32:
                    return (byte) m_Packed.Int32;
                case ValueType.Int64:
                    return (byte) m_Packed.Int64;

                case ValueType.UInt8:
                    return (byte) m_Packed.UInt8;
                case ValueType.UInt16:
                    return (byte) m_Packed.UInt16;
                case ValueType.UInt32:
                    return (byte) m_Packed.UInt32;
                case ValueType.UInt64:
                    return (byte) m_Packed.UInt64;

                case ValueType.Float32:
                    return (byte) m_Packed.Float32;
                case ValueType.Float64:
                    return (byte) m_Packed.Float64;

                case ValueType.Char:
                    return (byte) m_Packed.Char;

                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to an integral", m_Type));
            }
        }

        public UInt16 AsUInt16()
        {
            switch(m_Type)
            {
                case ValueType.Int8:
                    return (UInt16) m_Packed.Int8;
                case ValueType.Int16:
                    return (UInt16) m_Packed.Int16;
                case ValueType.Int32:
                    return (UInt16) m_Packed.Int32;
                case ValueType.Int64:
                    return (UInt16) m_Packed.Int64;

                case ValueType.UInt8:
                    return (UInt16) m_Packed.UInt8;
                case ValueType.UInt16:
                    return (UInt16) m_Packed.UInt16;
                case ValueType.UInt32:
                    return (UInt16) m_Packed.UInt32;
                case ValueType.UInt64:
                    return (UInt16) m_Packed.UInt64;

                case ValueType.Float32:
                    return (UInt16) m_Packed.Float32;
                case ValueType.Float64:
                    return (UInt16) m_Packed.Float64;

                case ValueType.Char:
                    return (UInt16) m_Packed.Char;

                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to an integral", m_Type));
            }
        }

        public UInt32 AsUInt32()
        {
            switch(m_Type)
            {
                case ValueType.Int8:
                    return (UInt32) m_Packed.Int8;
                case ValueType.Int16:
                    return (UInt32) m_Packed.Int16;
                case ValueType.Int32:
                    return (UInt32) m_Packed.Int32;
                case ValueType.Int64:
                    return (UInt32) m_Packed.Int64;

                case ValueType.UInt8:
                    return (UInt32) m_Packed.UInt8;
                case ValueType.UInt16:
                    return (UInt32) m_Packed.UInt16;
                case ValueType.UInt32:
                    return (UInt32) m_Packed.UInt32;
                case ValueType.UInt64:
                    return (UInt32) m_Packed.UInt64;

                case ValueType.Float32:
                    return (UInt32) m_Packed.Float32;
                case ValueType.Float64:
                    return (UInt32) m_Packed.Float64;

                case ValueType.Char:
                    return (UInt32) m_Packed.Char;

                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to an integral", m_Type));
            }
        }

        public UInt64 AsUInt64()
        {
            switch(m_Type)
            {
                case ValueType.Int8:
                    return (UInt64) m_Packed.Int8;
                case ValueType.Int16:
                    return (UInt64) m_Packed.Int16;
                case ValueType.Int32:
                    return (UInt64) m_Packed.Int32;
                case ValueType.Int64:
                    return (UInt64) m_Packed.Int64;

                case ValueType.UInt8:
                    return (UInt64) m_Packed.UInt8;
                case ValueType.UInt16:
                    return (UInt64) m_Packed.UInt16;
                case ValueType.UInt32:
                    return (UInt64) m_Packed.UInt32;
                case ValueType.UInt64:
                    return (UInt64) m_Packed.UInt64;

                case ValueType.Float32:
                    return (UInt64) m_Packed.Float32;
                case ValueType.Float64:
                    return (UInt64) m_Packed.Float64;

                case ValueType.Char:
                    return (UInt64) m_Packed.Char;

                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to an integral", m_Type));
            }
        }

        public char AsChar()
        {
            return (char) AsUInt16();
        }

        #endregion // Integrals

        #region Floats

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsFloat() { return m_Type >= ValueType.Float32 && m_Type <= ValueType.Float64; }

        public float AsFloat()
        {
            switch(m_Type)
            {
                case ValueType.Int8:
                    return (float) m_Packed.Int8;
                case ValueType.Int16:
                    return (float) m_Packed.Int16;
                case ValueType.Int32:
                    return (float) m_Packed.Int32;
                case ValueType.Int64:
                    return (float) m_Packed.Int64;

                case ValueType.UInt8:
                    return (float) m_Packed.UInt8;
                case ValueType.UInt16:
                    return (float) m_Packed.UInt16;
                case ValueType.UInt32:
                    return (float) m_Packed.UInt32;
                case ValueType.UInt64:
                    return (float) m_Packed.UInt64;

                case ValueType.Float32:
                    return (float) m_Packed.Float32;
                case ValueType.Float64:
                    return (float) m_Packed.Float64;

                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to a float", m_Type));
            }
        }

        public double AsDouble()
        {
            switch(m_Type)
            {
                case ValueType.Int8:
                    return (double) m_Packed.Int8;
                case ValueType.Int16:
                    return (double) m_Packed.Int16;
                case ValueType.Int32:
                    return (double) m_Packed.Int32;
                case ValueType.Int64:
                    return (double) m_Packed.Int64;

                case ValueType.UInt8:
                    return (double) m_Packed.UInt8;
                case ValueType.UInt16:
                    return (double) m_Packed.UInt16;
                case ValueType.UInt32:
                    return (double) m_Packed.UInt32;
                case ValueType.UInt64:
                    return (double) m_Packed.UInt64;

                case ValueType.Float32:
                    return (double) m_Packed.Float32;
                case ValueType.Float64:
                    return (double) m_Packed.Float64;

                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to a float", m_Type));
            }
        }

        #endregion // Floats

        public bool AsBool()
        {
            switch(m_Type)
            {
                case ValueType.Boolean:
                    return m_Packed.Boolean;

                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to a boolean", m_Type));
            }
        }

        public IntPtr AsPtr()
        {
            switch(m_Type)
            {
                case ValueType.IntPtr:
                    return m_Packed.IntPtr;
                case ValueType.Null:
                    return (IntPtr) 0;

                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to an IntPtr", m_Type));
            }
        }

        public StringHash32 AsStringHash()
        {
            switch(m_Type)
            {
                case ValueType.StringHash32:
                    return m_Packed.StringHash32;
                case ValueType.Null:
                    return StringHash32.Null;

                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to a StringHash", m_Type));
            }
        }

        public string AsString()
        {
            switch(m_Type)
            {
                case ValueType.String:
                    return (string) m_ObjectRef;
                case ValueType.StringSlice:
                    return ((string) m_ObjectRef).Substring(m_Packed.Int32, m_Packed.Int32_B);
                case ValueType.Null:
                    return null;
                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to a string", m_Type));
            }
        }

        public StringSlice AsStringSlice()
        {
            switch(m_Type)
            {
                case ValueType.String:
                    return (string) m_ObjectRef;
                case ValueType.StringSlice:
                    return new StringSlice((string) m_ObjectRef, m_Packed.Int32, m_Packed.Int32_B);
                case ValueType.Null:
                    return null;
                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to a StringSlice", m_Type));
            }
        }

        public Variant AsVariant()
        {
            Variant v;
            if (!TryGetVariant(out v))
                throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to a Variant", m_Type));
            return v;
        }

        public bool TryGetVariant(out Variant outVariant)
        {
            switch(m_Type)
            {
                case ValueType.Null:
                    outVariant = Variant.Null;
                    return true;

                case ValueType.Int8:
                case ValueType.Int16:
                case ValueType.Int32:
                    outVariant = new Variant(AsInt32());
                    return true;

                case ValueType.UInt8:
                case ValueType.UInt16:
                case ValueType.UInt32:
                    outVariant = new Variant(AsUInt32());
                    return true;

                case ValueType.Float32:
                case ValueType.Float64:
                    outVariant = new Variant(AsFloat());
                    return true;

                case ValueType.Boolean:
                    outVariant = new Variant(AsBool());
                    return true;

                case ValueType.StringHash32:
                    outVariant = new Variant(AsStringHash());
                    return true;

                default:
                    outVariant = default(Variant);
                    return false;
            }
        }

        public object AsObject()
        {
            switch(m_Type)
            {
                case ValueType.String:
                case ValueType.Object:
                    return m_ObjectRef;
                case ValueType.Null:
                    return null;
                
                case ValueType.Int8:
                    return m_Packed.Int8;
                case ValueType.Int16:
                    return m_Packed.Int16;
                case ValueType.Int32:
                    return m_Packed.Int32;
                case ValueType.Int64:
                    return m_Packed.Int64;

                case ValueType.UInt8:
                    return m_Packed.UInt8;
                case ValueType.UInt16:
                    return m_Packed.UInt16;
                case ValueType.UInt32:
                    return m_Packed.UInt32;
                case ValueType.UInt64:
                    return m_Packed.UInt64;

                case ValueType.Float32:
                    return m_Packed.Float32;
                case ValueType.Float64:
                    return m_Packed.Float64;

                case ValueType.Char:
                    return m_Packed.Char;
                case ValueType.Boolean:
                    return m_Packed.Boolean;

                case ValueType.IntPtr:
                    return m_Packed.IntPtr;
                case ValueType.StringHash32:
                    return m_Packed.StringHash32;

                case ValueType.StringSlice:
                    return AsStringSlice();

                default:
                    throw new InvalidOperationException(string.Format("Value type {0} cannot be casted to an object", m_Type));
            }
        }

        #endregion // Casts

        #region Conversions

        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator sbyte(NonBoxedValue inValue) { return inValue.AsInt8(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator Int16(NonBoxedValue inValue) { return inValue.AsInt16(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator Int32(NonBoxedValue inValue) { return inValue.AsInt32(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator Int64(NonBoxedValue inValue) { return inValue.AsInt64(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator byte(NonBoxedValue inValue) { return inValue.AsUInt8(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator UInt16(NonBoxedValue inValue) { return inValue.AsUInt16(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator UInt32(NonBoxedValue inValue) { return inValue.AsUInt32(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator UInt64(NonBoxedValue inValue) { return inValue.AsUInt64(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator float(NonBoxedValue inValue) { return inValue.AsFloat(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator double(NonBoxedValue inValue) { return inValue.AsDouble(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator char(NonBoxedValue inValue) { return inValue.AsChar(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator bool(NonBoxedValue inValue) { return inValue.AsBool(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator IntPtr(NonBoxedValue inValue) { return inValue.AsPtr(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator StringHash32(NonBoxedValue inValue) { return inValue.AsStringHash(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator string(NonBoxedValue inValue) { return inValue.AsString(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public explicit operator StringSlice(NonBoxedValue inValue) { return inValue.AsStringSlice(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(sbyte inValue) { return new NonBoxedValue(inValue); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(Int16 inValue) { return new NonBoxedValue(inValue); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(Int32 inValue) { return new NonBoxedValue(inValue); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(Int64 inValue) { return new NonBoxedValue(inValue); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(byte inValue) { return new NonBoxedValue(inValue); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(UInt16 inValue) { return new NonBoxedValue(inValue); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(UInt32 inValue) { return new NonBoxedValue(inValue); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(UInt64 inValue) { return new NonBoxedValue(inValue); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(float inValue) { return new NonBoxedValue(inValue); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(double inValue) { return new NonBoxedValue(inValue); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(char inValue) { return new NonBoxedValue(inValue); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(bool inValue) { return new NonBoxedValue(inValue); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(IntPtr inValue) { return new NonBoxedValue(inValue); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(StringHash32 inValue) { return new NonBoxedValue(inValue); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(string inValue) { return new NonBoxedValue(inValue); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static public implicit operator NonBoxedValue(StringSlice inValue) { return new NonBoxedValue(inValue); }

        #endregion // Conversions
    
        #region Utils

        static public readonly NonBoxedValue Null = default(NonBoxedValue);

        static private void GetInfo(object inValue, ref ValueType outType, ref object outObjectRef, ref PackedValues outPacked)
        {
            if (inValue != null)
            {
                if (inValue is NonBoxedValue)
                {
                    NonBoxedValue other = (NonBoxedValue) inValue;
                    outType = other.m_Type;
                    outObjectRef = other.m_ObjectRef;
                    outPacked = other.m_Packed;
                    return;
                }

                Type objType = inValue.GetType();
                switch(System.Type.GetTypeCode(objType))
                {
                    case TypeCode.SByte:
                        outType = ValueType.Int8;
                        outPacked.Int8 = (sbyte) inValue;
                        break;

                    case TypeCode.Int16:
                        outType = ValueType.Int16;
                        outPacked.Int16 = (Int16) inValue;
                        break;

                    case TypeCode.Int32:
                        outType = ValueType.Int32;
                        outPacked.Int32 = (Int32) inValue;
                        break;

                    case TypeCode.Int64:
                        outType = ValueType.Int64;
                        outPacked.Int64 = (Int64) inValue;
                        break;

                    case TypeCode.Byte:
                        outType = ValueType.UInt8;
                        outPacked.UInt8 = (byte) inValue;
                        break;

                    case TypeCode.UInt16:
                        outType = ValueType.UInt16;
                        outPacked.UInt16 = (UInt16) inValue;
                        break;

                    case TypeCode.UInt32:
                        outType = ValueType.UInt32;
                        outPacked.UInt32 = (UInt32) inValue;
                        break;

                    case TypeCode.UInt64:
                        outType = ValueType.UInt64;
                        outPacked.UInt64 = (UInt64) inValue;
                        break;

                    case TypeCode.Single:
                        outType = ValueType.Float32;
                        outPacked.Float32 = (float) inValue;
                        break;

                    case TypeCode.Double:
                        outType = ValueType.Float64;
                        outPacked.Float64 = (double) inValue;
                        break;

                    case TypeCode.Char:
                        outType = ValueType.Char;
                        outPacked.Char = (char) inValue;
                        break;

                    case TypeCode.Boolean:
                        outType = ValueType.Boolean;
                        outPacked.Boolean = (bool) inValue;
                        break;

                    case TypeCode.String:
                        outType = ValueType.String;
                        outObjectRef = (string) inValue;
                        break;

                    case TypeCode.Object:
                    {
                        if (objType == typeof(Variant))
                        {
                            GetInfo((Variant) inValue, ref outType, ref outObjectRef, ref outPacked);
                        }
                        else if (objType == typeof(StringHash32))
                        {
                            outType = ValueType.StringHash32;
                            outPacked.StringHash32 = (StringHash32) inValue;
                        }
                        else if (objType == typeof(IntPtr))
                        {
                            outType = ValueType.IntPtr;
                            outPacked.IntPtr = (IntPtr) inValue;
                        }
                        else if (objType == typeof(StringSlice))
                        {
                            outType = ValueType.StringSlice;
                            string source;
                            ((StringSlice) inValue).Unpack(out source, out outPacked.Int32, out outPacked.Int32_B);
                            outObjectRef = source;
                        }
                        else
                        {
                            outType = ValueType.Object;
                            outObjectRef = inValue;
                        }
                        break;
                    }
                }
            }
        }

        static private void GetInfo(Variant inValue, ref ValueType outType, ref object outObjectRef, ref PackedValues outPacked)
        {
            switch(inValue.Type)
            {
                case VariantType.Float:
                    outType = ValueType.Float32;
                    outPacked.Float32 = inValue.AsFloat();
                    break;

                case VariantType.Int:
                    outType = ValueType.Int32;
                    outPacked.Int32 = inValue.AsInt();
                    break;

                case VariantType.UInt:
                    outType = ValueType.UInt32;
                    outPacked.UInt32 = inValue.AsUInt();
                    break;

                case VariantType.Bool:
                    outType = ValueType.Boolean;
                    outPacked.Boolean = inValue.AsBool();
                    break;

                case VariantType.StringHash:
                    outType = ValueType.StringHash32;
                    outPacked.StringHash32 = inValue.AsStringHash();
                    break;
            }
        }

        #endregion // Utils
    }
}