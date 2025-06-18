#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BeauUtil.Debugger;

namespace BeauUtil
{
    /// <summary>
    /// Allocator that stores temporary StringSlices.
    /// </summary>
    public sealed unsafe class StringArena : IEnumerable<StringSlice>
    {
        private const int LengthChars = sizeof(int) / sizeof(char);

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
            Assert.True(str != m_Buffer, "Attempting to re-add a StringSlice taken from this StringArena");
            fixed(char* srcPtr = str)
            {
                return AllocInternal(srcPtr + off, len);
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

            if (m_Offset + LengthChars + inSource.Length + 1 >= m_Capacity)
            {
                Assert.Fail("[StringArena] Out of space in StringArena: attempting to add string of size {0} when only {1} characters available", srcLength, m_Capacity - m_Offset - LengthChars);
                return StringSlice.Empty;
            }

            fixed (char* bufferPtr = m_Buffer)
            {
                char* head = bufferPtr + m_Offset;
                WriteLength(head, srcLength);

                inSource.CopyTo(head + LengthChars, srcLength);
                head[LengthChars + srcLength] = '\0';

                StringSlice result = new StringSlice(m_Buffer, m_Offset + LengthChars, srcLength);
                m_Offset += LengthChars + srcLength + 1;
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
            if (m_Offset + LengthChars + inSourceLength + 1 >= m_Capacity)
            {
                Assert.Fail("[StringArena] Out of space in StringArena: attempting to add string of size {0} when only {1} characters available", inSourceLength, m_Capacity - m_Offset - LengthChars);
                return StringSlice.Empty;
            }

            fixed(char* bufferPtr = m_Buffer)
            {
                char* head = bufferPtr + m_Offset;
                WriteLength(head, inSourceLength);

                Unsafe.FastCopyArray(inSource, inSourceLength, head + LengthChars);
                head[LengthChars + inSourceLength] = '\0';

                StringSlice result = new StringSlice(m_Buffer, m_Offset + LengthChars, inSourceLength);
                m_Offset += LengthChars + inSourceLength + 1;
                m_StringCount++;
                return result;
            }
        }

        #endregion // Alloc

        #region Length Markers

        static private void WriteLength(char* ioBuffer, int inLength)
        {
            byte* ptr = (byte*) ioBuffer;
            ptr[0] = (byte) ((inLength >> 24) & 0xFF);
            ptr[1] = (byte) ((inLength >> 16) & 0xFF);
            ptr[2] = (byte) ((inLength >> 8) & 0xFF);
            ptr[3] = (byte) ((inLength) & 0xFF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private int ReadLength(char* inBuffer)
        {
            byte* ptr = (byte*) inBuffer;
            return (ptr[0] << 24) | (ptr[1] << 16) | (ptr[2] << 8) | ptr[3];
        }

        #endregion // Length Markers

        #region IEnumerable

        public struct Enumerator : IEnumerator<StringSlice>, IDisposable
        {
            private StringArena m_Src;
            private StringSlice m_CurrentSlice;
            private int m_Offset;

            public Enumerator(StringArena inArena)
            {
                if (inArena == null)
                    throw new ArgumentNullException("inArena");
                m_Src = inArena;
                m_CurrentSlice = default;
                m_Offset = 0;
            }

            public StringSlice Current { get { return m_CurrentSlice; } }

            object IEnumerator.Current { get { return m_CurrentSlice; } }

            public void Dispose()
            {
                m_Src = null;
            }

            public bool MoveNext()
            {
                if (m_Src == null || m_Offset >= m_Src.m_Offset)
                    return false;

                fixed(char* buffer = m_Src.m_Buffer)
                {
                    char* ptr = buffer + m_Offset;
                    int len = ReadLength(ptr);
                    ptr += LengthChars;

                    if (len > m_Src.Remaining || ptr[len] != '\0')
                    {
                        throw new InvalidOperationException("StringArena reset during enumeration");
                    }

                    m_CurrentSlice = new StringSlice(m_Src.m_Buffer, m_Offset + LengthChars, len);
                    m_Offset += LengthChars + len + 1;
                    return true;
                }
            }

            public void Reset()
            {
                m_CurrentSlice = default;
                m_Offset = 0;
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<StringSlice> IEnumerable<StringSlice>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion // IEnumerable
    }
}