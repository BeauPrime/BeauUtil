using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BeauUtil.Debugger;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Expansion policy for array/buffer resizing.
    /// Controls how array size/capacity changes over time.
    /// </summary>
    [Il2CppEagerStaticClassConstruction]
    public readonly struct BufferExpansionPolicy
    {
        private const byte FractionQuantization = 255;

        public readonly BufferExpansionType Type;
        public readonly byte Flags;
        public readonly byte GrowthFraction;
        public readonly int ChunkSize;
        public readonly uint Alignment;

        private BufferExpansionPolicy(BufferExpansionType type, byte flags, float growthFraction, int chunkSize, uint alignment)
        {
            Type = type;
            Flags = flags;
            GrowthFraction = (byte) Math.Max(0, Math.Min((growthFraction * FractionQuantization), FractionQuantization));
            ChunkSize = chunkSize;
            Assert.True(alignment < 2 || (alignment & (alignment - 1)) == 0, "Alignment must be power of 2");
            Alignment = (ushort) alignment;

            switch (type)
            {
                case BufferExpansionType.Geometric:
                    Assert.True(GrowthFraction > 0, "Geometric growth fraction cannot be 0");
                    Assert.True(ChunkSize > 0, "Geometric cap cannot be zero");
                    break;
                case BufferExpansionType.Chunked:
                    Assert.True(ChunkSize > 0, "Chunk size cannot be zero");
                    break;
            }
        }

        public int NextCapacity(int inCapacity, int inMaxCapacity = int.MaxValue)
        {
            switch (Type)
            {
                case BufferExpansionType.Fixed:
                    return inCapacity;
                case BufferExpansionType.Exponential:
                    return AlignAndClamp(Math.Max(1, (long) inCapacity << 1), int.MaxValue);
                case BufferExpansionType.Chunked:
                    return AlignAndClamp((long) inCapacity + (long) ChunkSize, int.MaxValue);
                case BufferExpansionType.Geometric:
                    return AlignAndClamp((long) inCapacity + Math.Min(((long) (inCapacity + 1) * GrowthFraction / FractionQuantization), ChunkSize), int.MaxValue);
                default:
                    throw new InvalidOperationException("Unknown expansion type");
            }
        }

        private int AlignAndClamp(long inUnclamped, int inMaxCapacity)
        {
            if (Alignment > 1)
            {
                return (int) Math.Min(((inUnclamped + (Alignment - 1)) & ~(Alignment - 1)), inMaxCapacity);
            }
            return (int) Math.Min(inUnclamped, inMaxCapacity);
        }

        public bool TryExpand(int inCapacity, out int outNewCapacity)
        {
            outNewCapacity = NextCapacity(inCapacity, int.MaxValue);
            return inCapacity < outNewCapacity;
        }

        public bool TryExpand(int inCapacity, int inMaxCapacity, out int outNewCapacity)
        {
            outNewCapacity = NextCapacity(inCapacity, inMaxCapacity);
            return inCapacity < outNewCapacity;
        }

        #region Factory

        /// <summary>
        /// Will expand by powers of 2, and align capacity upwards to the given alignment.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BufferExpansionPolicy ExponentialAligned(uint inAlignment)
        {
            return new BufferExpansionPolicy(BufferExpansionType.Exponential, 0, 0, 0, inAlignment);
        }

        /// <summary>
        /// Will expand geometrically.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BufferExpansionPolicy Geometric(float inFraction, int inGrowthCap)
        {
            return new BufferExpansionPolicy(BufferExpansionType.Geometric, 0, inFraction, inGrowthCap, 0);
        }

        /// <summary>
        /// Will expand geometrically, and align capacity upwards to the given alignment.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BufferExpansionPolicy GeometricAligned(float inFraction, int inGrowthCap, uint inAlignment)
        {
            return new BufferExpansionPolicy(BufferExpansionType.Geometric, 0, inFraction, inGrowthCap, inAlignment);
        }

        /// <summary>
        /// Will expand by a set amount.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BufferExpansionPolicy Chunked(int inChunkSize)
        {
            return new BufferExpansionPolicy(BufferExpansionType.Chunked, 0, 0, inChunkSize, 0);
        }

        /// <summary>
        /// Will expand by a set amount, and align capacity upwards to the given alignment.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BufferExpansionPolicy ChunkedAligned(int inChunkSize, uint inAlignment)
        {
            return new BufferExpansionPolicy(BufferExpansionType.Chunked, 0, 0, 0, inAlignment);
        }

        #endregion // Factory

        #region Presets

        /// <summary>
        /// Will not expand.
        /// </summary>
        static public readonly BufferExpansionPolicy Fixed = default;

        /// <summary>
        /// Will expand by powers of 2.
        /// </summary>
        static public readonly BufferExpansionPolicy Exponential = new BufferExpansionPolicy(BufferExpansionType.Exponential, 0, 0, 0, 0);

        /// <summary>
        /// Will by 20% each increment, up to a maximum of 32 entries per increment.
        /// </summary>
        static public readonly BufferExpansionPolicy DefaultGeometric = Geometric(0.2f, 32);

        #endregion // Presets
    }

    public enum BufferExpansionType : byte
    {
        Fixed,
        Exponential,
        Geometric,
        Chunked,
    }
}