using System;
using UnityEngine;
using UnityEngine.Networking;

namespace BeauUtil.Streaming
{
    /// <summary>
    /// Download handler that writes loaded bytes to a provided unsafe buffer.
    /// </summary>
    public unsafe class DownloadHandlerUnsafeBuffer : DownloadHandlerScript
    {
        private ulong m_LastKnownContentLength;
        private ulong m_ReceivedBytes;

        private byte* m_BufferHead;
        private long m_BufferLength;
        private readonly WriteLocation m_WriteLocation;

        private byte* m_BufferWriteHeadAbsolute;
        private byte* m_BufferWriteHeadCurrent;
        private long m_RemainingWriteLength;

        private AllocateBufferDelegate m_BufferAlloc;
        private FreeBufferDelegate m_BufferFree;

        private object m_DataContext;
        private int m_DataContextFlags;

        public DownloadHandlerUnsafeBuffer(byte[] inChunkBuffer, WriteLocation inWriteLocation = WriteLocation.Start)
            : this(inChunkBuffer, DefaultAllocate, DefaultFree, inWriteLocation)
        {
        }

        public DownloadHandlerUnsafeBuffer(byte[] inChunkBuffer, AllocateBufferDelegate inBufferAlloc, FreeBufferDelegate inBufferFree, WriteLocation inWriteLocation = WriteLocation.Start, object inBufferAllocContext = null, int inBufferAllocContextFlags = 0)
            : base(inChunkBuffer)
        {
            m_DataContext = inBufferAllocContext;
            m_DataContextFlags = inBufferAllocContextFlags;
            m_WriteLocation = inWriteLocation;
            m_BufferAlloc = inBufferAlloc;
            m_BufferFree = inBufferFree;
        }

        public DownloadHandlerUnsafeBuffer(byte[] inChunkBuffer, byte* inBufferHead, long inBufferLength, WriteLocation inWriteLocation = WriteLocation.Start)
            : base(inChunkBuffer)
        {
            m_BufferHead = inBufferHead;
            m_BufferLength = inBufferLength;
            m_WriteLocation = inWriteLocation;

            if (inWriteLocation == WriteLocation.Start)
            {
                m_BufferWriteHeadAbsolute = m_BufferWriteHeadCurrent = m_BufferHead;
            }
        }

        ~DownloadHandlerUnsafeBuffer()
        {
            if (m_BufferHead != null && m_BufferFree != null)
            {
                m_BufferFree(m_BufferHead, m_BufferLength, m_DataContext, m_DataContextFlags);
                m_BufferHead = null;
                m_BufferLength = 0;
            }
        }

        protected override void ReceiveContentLengthHeader(ulong contentLength)
        {
            m_LastKnownContentLength = contentLength;
            m_RemainingWriteLength = (int) (contentLength - m_ReceivedBytes);

            if (m_BufferLength == 0 && m_BufferAlloc != null)
            {
                m_BufferHead = m_BufferAlloc(contentLength, out m_BufferLength, m_DataContext, m_DataContextFlags);
                m_BufferAlloc = null;

                if (m_WriteLocation == WriteLocation.Start)
                {
                    m_BufferWriteHeadAbsolute = m_BufferWriteHeadCurrent = m_BufferHead;
                }
                else
                {
                    m_BufferWriteHeadAbsolute = m_BufferWriteHeadCurrent = m_BufferHead + m_BufferLength - (long) m_LastKnownContentLength;
                }
            }

            if (m_LastKnownContentLength > (ulong) m_BufferLength)
            {
                throw new InsufficientMemoryException(string.Format("Provided buffer of size {0} too small for content length {1}", m_BufferLength, m_LastKnownContentLength));
            }

            if (m_WriteLocation == WriteLocation.End)
            {
                if (m_ReceivedBytes > 0)
                {
                    throw new InvalidOperationException("Content length header received after download started for end of buffer");
                }

                m_BufferWriteHeadAbsolute = m_BufferWriteHeadCurrent = m_BufferHead + m_BufferLength - (long) m_LastKnownContentLength;
            }
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            m_ReceivedBytes += (ulong) dataLength;
            if (m_ReceivedBytes > m_LastKnownContentLength)
            {
                throw new InsufficientMemoryException(string.Format("Writing more bytes ({0}) than indicated by content length header {1}", m_ReceivedBytes, m_LastKnownContentLength));
            }

            fixed(byte** writePtr = &m_BufferWriteHeadCurrent)
            fixed(long* lengthPtr = &m_RemainingWriteLength)
            {
                Unsafe.CopyArrayIncrement(data, 0, dataLength, writePtr, lengthPtr);
            }

            return true;
        }

        protected override float GetProgress()
        {
            if (m_LastKnownContentLength == 0)
                return 0;
            return Mathf.Clamp01((float) m_ReceivedBytes / m_LastKnownContentLength);
        }

        public override void Dispose()
        {
            base.Dispose();

            m_LastKnownContentLength = 0;
            m_ReceivedBytes = 0;

            if (m_BufferHead != null && m_BufferFree != null)
            {
                m_BufferFree(m_BufferHead, m_BufferLength, m_DataContext, m_DataContextFlags);
            }

            m_BufferAlloc = null;
            m_BufferFree = null;

            m_DataContext = null;
            m_DataContextFlags = 0;

            m_BufferHead = null;
            m_BufferLength = 0;

            m_BufferWriteHeadAbsolute = null;
            m_BufferWriteHeadCurrent = null;
            m_RemainingWriteLength = 0;
        }

        /// <summary>
        /// Retrieves the number of downloaded bytes.
        /// </summary>
        public long DataLength
        {
            get { return (long) m_ReceivedBytes; }
        }

        /// <summary>
        /// Pointer to the start of the downloaded bytes.
        /// </summary>
        public byte* DataHead
        {
            get { return m_BufferWriteHeadAbsolute; }
        }

        /// <summary>
        /// Location to write the downloaded bytes to.
        /// </summary>
        public enum WriteLocation
        {
            // At the start of the buffer
            Start,

            // at the end of the buffer
            End
        }

        /// <summary>
        /// Delegate used for allocating a buffer.
        /// </summary>
        public delegate byte* AllocateBufferDelegate(ulong inContentLength, out long outBufferLength, object inContext, int inContextFlags);

        /// <summary>
        /// Delegate used for freeing a buffer.
        /// </summary>
        public delegate void FreeBufferDelegate(byte* inBufferHead, long inBufferLength, object inContext, int inContextFlags);

        /// <summary>
        /// Default buffer allocator.
        /// </summary>
        static public readonly AllocateBufferDelegate DefaultAllocate = (ulong contentLength, out long bufferLength, object context, int flags) =>
        {
            bufferLength = (long) contentLength;
            return (byte*) Unsafe.Alloc((int) bufferLength);
        };

        /// <summary>
        /// Default buffer free.
        /// </summary>
        static public readonly FreeBufferDelegate DefaultFree = (byte* head, long bufferLength, object context, int flags) =>
        {
            Unsafe.Free(head);
        };
    }
}