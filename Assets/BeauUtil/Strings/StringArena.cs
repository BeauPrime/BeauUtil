#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using BeauUtil.Debugger;

namespace BeauUtil
{
    /// <summary>
    /// Allocator that stores temporary StringSlices.
    /// </summary>
    public sealed unsafe class StringArena
    {
        private int m_Capacity;
        private int m_Offset;
        private int m_StringCount;
        private string m_Buffer;

        public StringArena(int inCapacity)
        {
            m_Buffer = new string(' ', inCapacity);
            m_Capacity = inCapacity;
            m_Offset = 0;
            m_StringCount = 0;
        }

        /// <summary>
        /// String buffer capacity.
        /// </summary>
        public int Capacity { get { return m_Capacity; } }

        /// <summary>
        /// Characters remaining in the string buffer.
        /// </summary>
        public int Remaining { get { return m_Capacity - m_Offset; } }

        /// <summary>
        /// Number of strings allocated.
        /// </summary>
        public int Count { get { return m_StringCount; } }

        /// <summary>
        /// Resets the buffer.
        /// </summary>
        public void Reset()
        {
            m_Offset = 0;
            m_StringCount = 0;
        }

        #region Alloc

        /// <summary>
        /// Allocates a new StringSlice within the arena.
        /// </summary>
        public StringSlice Alloc(string inSource)
        {
            Assert.True(inSource != m_Buffer);

            if (string.IsNullOrEmpty(inSource))
                return inSource;

            fixed(char* srcPtr = inSource)
            {
                return AllocInternal(srcPtr, inSource.Length);
            }
        }

        /// <summary>
        /// Allocates a new StringSlice within the arena.
        /// </summary>
        public StringSlice Alloc(StringSlice inSource)
        {
            if (inSource.Length == 0)
                return inSource;

            inSource.Unpack(out string str, out int off, out int len);
            Assert.True(str != m_Buffer || off + len <= m_Offset, "Attempting to re-add a StringSlice taken from this StringArena");
            fixed(char* srcPtr = str)
            {
                return AllocInternal(srcPtr + off, len);
            }
        }

        /// <summary>
        /// Allocates a new StringSlice within the arena.
        /// </summary>
        public StringSlice Alloc(StringBuilder inSource)
        {
            if (inSource == null || inSource.Length == 0)
                return StringSlice.Empty;

            int srcLength = inSource.Length;

            if (m_Offset + inSource.Length + 1 >= m_Capacity)
            {
                Assert.Fail("[StringArena] Out of space in StringArena: attempting to add string of size {0} when only {1} characters available", srcLength, m_Capacity - m_Offset);
                return StringSlice.Empty;
            }

            fixed (char* bufferPtr = m_Buffer)
            {
                char* head = bufferPtr + m_Offset;
                head[srcLength] = '\0';

                StringSlice result = new StringSlice(m_Buffer, m_Offset, srcLength);
                StringUtils.Copy(inSource, head, srcLength);
                m_Offset += srcLength + 1;
                m_StringCount++;
                return result;
            }
        }

        /// <summary>
        /// Allocates a new StringSlice within the arena.
        /// </summary>
        public StringSlice Alloc(StringBuilderSlice inSource)
        {
            if (inSource.Length == 0)
                return StringSlice.Empty;

            int srcLength = inSource.Length;

            if (m_Offset + inSource.Length + 1 >= m_Capacity)
            {
                Assert.Fail("[StringArena] Out of space in StringArena: attempting to add string of size {0} when only {1} characters available", srcLength, m_Capacity - m_Offset);
                return StringSlice.Empty;
            }

            fixed (char* bufferPtr = m_Buffer)
            {
                char* head = bufferPtr + m_Offset;
                head[srcLength] = '\0';

                StringSlice result = new StringSlice(m_Buffer, m_Offset, srcLength);
                inSource.CopyTo(head, srcLength);
                m_Offset += srcLength + 1;
                m_StringCount++;
                return result;
            }
        }

        /// <summary>
        /// Allocates a new StringSlice within the arena.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringSlice Alloc(char* inSource, int inSourceLength)
        {
            if (inSourceLength <= 0)
                return StringSlice.Empty;

            return AllocInternal(inSource, inSourceLength);
        }

        /// <summary>
        /// Allocates a new StringSlice within the arena.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringSlice Alloc(UnsafeString inSource)
        {
            if (inSource.Length <= 0)
                return StringSlice.Empty;

            inSource.Unpack(out char* ptr, out int len);
            return AllocInternal(ptr, len);
        }

#if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Allocates a new StringSlice within the arena.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringSlice Alloc(UnsafeSpan<char> inSource)
        {
            if (inSource.Length <= 0)
                return StringSlice.Empty;
            return AllocInternal(inSource.Ptr, inSource.Length);
        }

#endif // UNMANAGED_CONSTRAINT

        private StringSlice AllocInternal(char* inSource, int inSourceLength)
        {
            if (m_Offset + inSourceLength + 1 >= m_Capacity)
            {
                Assert.Fail("[StringArena] Out of space in StringArena: attempting to add string of size {0} when only {1} characters available", inSourceLength, m_Capacity - m_Offset);
                return StringSlice.Empty;
            }

            fixed(char* bufferPtr = m_Buffer)
            {
                char* head = bufferPtr + m_Offset;
                head[inSourceLength] = '\0';

                StringSlice result = new StringSlice(m_Buffer, m_Offset, inSourceLength);
                Unsafe.FastCopyArray<char>(inSource, inSourceLength, head);
                m_Offset += inSourceLength + 1;
                m_StringCount++;
                return result;
            }
        }

        #endregion // Alloc
    }
}