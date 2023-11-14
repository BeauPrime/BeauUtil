#if !UNITY_2021_1_OR_NEWER
#define DISPOSABLE_HACK
#endif // !UNITY_2021_1_OR_NEWER

using System;
using UnityEngine;
using UnityEngine.Networking;

namespace BeauUtil.Streaming
{
    /// <summary>
    /// Download handler that passes loaded bytes to a custom handler.
    /// </summary>
    public class DownloadHandlerStream : DownloadHandlerScript
#if DISPOSABLE_HACK
        , IDisposable
#endif // DISPOSABLE_HACK
    {
        private ulong m_LastKnownContentLength;
        private ulong m_ReceivedBytes;

        private object m_DataContext;
        private int m_DataContextFlags;
        private DataHandler m_SafeDataHandler;
        private UnsafeDataHandler m_UnsafeHandler;

        public DownloadHandlerStream(byte[] inChunkBuffer, DataHandler inHandler, object inHandlerContext = null, int inContextFlags = 0)
            : base(inChunkBuffer)
        {
            m_SafeDataHandler = inHandler;
            m_DataContext = inHandlerContext;
            m_DataContextFlags = inContextFlags;
        }

        public DownloadHandlerStream(byte[] inChunkBuffer, UnsafeDataHandler inHandler, object inHandlerContext = null, int inContextFlags = 0)
            : base(inChunkBuffer)
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

#if DISPOSABLE_HACK
        public new void Dispose()
#else
        public override void Dispose()
#endif // DISPOSABLE_HACK
        {
            base.Dispose();

            m_LastKnownContentLength = 0;
            m_ReceivedBytes = 0;

            m_DataContext = null;
            m_DataContextFlags = 0;
            m_SafeDataHandler = null;
            m_UnsafeHandler = null;
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