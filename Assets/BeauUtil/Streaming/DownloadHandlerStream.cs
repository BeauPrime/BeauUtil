using UnityEngine;
using UnityEngine.Networking;

namespace BeauUtil.Streaming
{
    /// <summary>
    /// Download handler that passes loaded bytes to a custom handler.
    /// </summary>
    public class DownloadHandlerStream : DownloadHandlerScript
    {
        private ulong m_LastKnownContentLength;
        private ulong m_ReceivedBytes;

        private object m_DataContext;
        private int m_DataContextFlags;
        private DataHandler m_SafeDataHandler;
        private UnsafeDataHandler m_UnsafeHandler;

        public DownloadHandlerStream(byte[] inPreallocatedBuffer, DataHandler inHandler, object inHandlerContext = null, int inContextFlags = 0)
            : base(inPreallocatedBuffer)
        {
            m_SafeDataHandler = inHandler;
            m_DataContext = inHandlerContext;
            m_DataContextFlags = inContextFlags;
        }

        public DownloadHandlerStream(byte[] inPreallocatedBuffer, UnsafeDataHandler inHandler, object inHandlerContext = null, int inContextFlags = 0)
            : base(inPreallocatedBuffer)
        {
            m_UnsafeHandler = inHandler;
            m_DataContext = inHandlerContext;
            m_DataContextFlags = inContextFlags;
        }

        protected override void ReceiveContentLengthHeader(ulong contentLength)
        {
            m_LastKnownContentLength = contentLength;
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            m_ReceivedBytes += (ulong) dataLength;
            bool okay = true;
            if (m_SafeDataHandler != null)
            {
                okay &= m_SafeDataHandler(data, dataLength, m_DataContext, m_DataContextFlags);
            }
            else if (m_UnsafeHandler != null)
            {
                unsafe
                {
                    fixed(byte* ptr = data)
                    {
                        okay &= m_UnsafeHandler(ptr, dataLength, m_DataContext, m_DataContextFlags);
                    }
                }
            }

            return okay;
        }

        protected override float GetProgress()
        {
            if (m_LastKnownContentLength == 0)
                return 0;
            return Mathf.Clamp01((float) m_ReceivedBytes / m_LastKnownContentLength);
        }

        /// <summary>
        /// Unsafe callback.
        /// Return "false" to end the download.
        /// </summary>
        public unsafe delegate bool UnsafeDataHandler(byte* inData, int inCount, object inContext, int inContextFlags);
        
        /// <summary>
        /// Safe callback.
        /// Return "false" to end the download.
        /// </summary>
        public delegate bool DataHandler(byte[] inData, int inCount, object inContext, int inContextFlags);
    }
}