/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    7 June 2022
 * 
 * File:    CharStream.cs
 * Purpose: Character stream.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

#if UNITY_2018_1_OR_NEWER
#define NATIVE_ARRAY_EXT
#endif // UNITY_2018_1_OR_NEWER

#if UNITY_2021_2_OR_NEWER
#define TEXT_ASSET_NATIVE
#endif // UNITY_2021_2_OR_NEWER

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using BeauUtil.Blocks;
using BeauUtil.Debugger;
using UnityEngine;

namespace BeauUtil.Streaming
{
    /// <summary>
    /// Character stream.
    /// Allows reading characters and bytes from a variety of sources in a unified manner.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CharStream : IDisposable
    {
        internal object Source;
        internal GCHandle PinnedDataHandle;
        internal Decoder Decoder;
        internal byte[] UnpackBuffer;
        
        internal unsafe void* Data;

        internal int DataOffset;
        internal int DataLength;
        internal int DataMaxLength;

        internal CharStreamType Type;
        internal CharStreamResourceOwnership Ownership;
        internal byte DataFinished;

        #region Creation

        /// <summary>
        /// Loads this CharStream from the given setup parameters. 
        /// </summary>
        #if EXPANDED_REFS
        public void LoadParams(in CharStreamParams inParams, byte[] inUnpackBufferOverride = null)
        #else
        public void LoadParams(CharStreamParams inParams, byte[] inUnpackBufferOverride = null)
        #endif // EXPANDED_REFS
        {
            switch(inParams.Type)
            {
                case CharStreamSourceType.Stream:
                    LoadStream((Stream) inParams.Source, inUnpackBufferOverride ?? inParams.UnpackBuffer, inParams.Dispose);
                    break;

                case CharStreamSourceType.String:
                    LoadString((string) inParams.Source);
                    break;

                case CharStreamSourceType.CustomTextAsset:
                    LoadCustomTextAsset((CustomTextAsset) inParams.Source);
                    break;

                case CharStreamSourceType.TextAsset:
                    LoadTextAsset((TextAsset) inParams.Source);
                    break;

                case CharStreamSourceType.ByteArr:
                    LoadBytes((byte[]) inParams.Source);
                    break;

                case CharStreamSourceType.CharArr:
                    LoadChars((char[]) inParams.Source);
                    break;
            }
        }

        /// <summary>
        /// Loads this CharStream with the given Stream instance.
        /// </summary>
        public void LoadStream(Stream inStream, byte[] inUnpackBuffer, bool inbDispose = true)
        {
            if (Type != 0)
            {
                Dispose();
            }

            Source = inStream;
            Type = CharStreamType.Stream;
            Ownership = inbDispose ? CharStreamResourceOwnership.DisposeStream : CharStreamResourceOwnership.None;
            Decoder = Decoder ?? Encoding.UTF8.GetDecoder();
            UnpackBuffer = inUnpackBuffer;
        }

        /// <summary>
        /// Loads this CharStream with the given string.
        /// </summary>
        public void LoadString(string inString)
        {
            if (Type != 0)
            {
                Dispose();
            }

            GCHandle pinned = GCHandle.Alloc(inString, GCHandleType.Pinned);
            unsafe
            {
                Data = (char*) pinned.AddrOfPinnedObject();
            }
            DataOffset = 0;
            DataLength = inString.Length;
            DataMaxLength = inString.Length;
            Type = CharStreamType.CharPtr;
            Ownership = CharStreamResourceOwnership.UnpinData;
            PinnedDataHandle = pinned;
        }

        /// <summary>
        /// Loads this CharStream from the given CustomTextAsset instance.
        /// </summary>
        public void LoadCustomTextAsset(CustomTextAsset inCustomAsset)
        {
            if (Type != 0)
            {
                Dispose();
            }

            unsafe
            {
                byte* buffer = inCustomAsset.PinBytes();
                Data = buffer;
            }
            DataOffset = 0;
            DataLength = inCustomAsset.ByteLength();
            DataMaxLength = inCustomAsset.ByteLength();
            Type = CharStreamType.BytePtr;
            Ownership = CharStreamResourceOwnership.Unpin_CustomTextAsset;
            Source = inCustomAsset;
            Decoder = Decoder ?? Encoding.UTF8.GetDecoder();
        }

        /// <summary>
        /// Loads this CharStream from the given TextAsset.
        /// </summary>
        public void LoadTextAsset(TextAsset inTextAsset)
        {
            if (Type != 0)
            {
                Dispose();
            }

            unsafe
            {
                #if TEXT_ASSET_NATIVE
                Unity.Collections.NativeArray<byte> bytes = inTextAsset.GetData<byte>();
                Data = Unsafe.NativePointerReadOnly<byte>(bytes);
                Ownership = CharStreamResourceOwnership.None;
                #else
                byte[] bytes = inTextAsset.bytes;
                GCHandle pinned = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                Data = (byte*) pinned.AddrOfPinnedObject();
                Ownership = CharStreamResourceOwnership.UnpinData;
                PinnedDataHandle = pinned;
                #endif // TEXT_ASSET_NATIVE

                DataLength = bytes.Length;
                DataMaxLength = bytes.Length;
            }

            DataOffset = 0;
            Type = CharStreamType.BytePtr;
            Source = inTextAsset;
            Decoder = Decoder ?? Encoding.UTF8.GetDecoder();
        }

        /// <summary>
        /// Loads this CharStream from the given byte array.
        /// </summary>
        public void LoadBytes(byte[] inBytes)
        {
            if (Type != 0)
            {
                Dispose();
            }

            unsafe
            {
                GCHandle pinned = GCHandle.Alloc(inBytes, GCHandleType.Pinned);
                Data = (byte*) pinned.AddrOfPinnedObject();
                Ownership = CharStreamResourceOwnership.UnpinData;
                PinnedDataHandle = pinned;

                DataLength = inBytes.Length;
                DataMaxLength = inBytes.Length;
            }

            DataOffset = 0;
            Type = CharStreamType.BytePtr;
            Decoder = Decoder ?? Encoding.UTF8.GetDecoder();
        }

        /// <summary>
        /// Loads this CharStream as a byte buffer.
        /// </summary>
        public void LoadByteBuffer(byte[] inBytes)
        {
            if (Type != 0)
            {
                Dispose();
            }

            unsafe
            {
                GCHandle pinned = GCHandle.Alloc(inBytes, GCHandleType.Pinned);
                Data = (byte*) pinned.AddrOfPinnedObject();
                Ownership = CharStreamResourceOwnership.UnpinData;
                PinnedDataHandle = pinned;

                DataLength = 0;
                DataMaxLength = inBytes.Length;
            }

            DataOffset = 0;
            Type = CharStreamType.BytePtr_Ring;
            Decoder = Decoder ?? Encoding.UTF8.GetDecoder();
        }

        /// <summary>
        /// Loads this CharStream from the given char array.
        /// </summary>
        public void LoadChars(char[] inChars)
        {
            if (Type != 0)
            {
                Dispose();
            }

            unsafe
            {
                GCHandle pinned = GCHandle.Alloc(inChars, GCHandleType.Pinned);
                Data = (char*) pinned.AddrOfPinnedObject();
                Ownership = CharStreamResourceOwnership.UnpinData;
                PinnedDataHandle = pinned;

                DataLength = inChars.Length;
                DataMaxLength = inChars.Length;
            }

            DataOffset = 0;
            Type = CharStreamType.CharPtr;
        }

        /// <summary>
        /// Loads this CharStream as a character buffer.
        /// </summary>
        public void LoadCharBuffer(char[] inChars)
        {
            if (Type != 0)
            {
                Dispose();
            }

            unsafe
            {
                GCHandle pinned = GCHandle.Alloc(inChars, GCHandleType.Pinned);
                Data = (char*) pinned.AddrOfPinnedObject();
                Ownership = CharStreamResourceOwnership.UnpinData;
                PinnedDataHandle = pinned;

                DataLength = 0;
                DataMaxLength = inChars.Length;
            }

            DataOffset = 0;
            Type = CharStreamType.CharPtr_Ring;
        }

        #endregion // Creation

        #region Factory

        /// <summary>
        /// Creates a CharStream from the given Stream instance.
        /// </summary>
        static public CharStream FromStream(Stream inStream, byte[] inUnpackBuffer, bool inbDispose = true)
        {
            CharStream stream = default;
            stream.LoadStream(inStream, inUnpackBuffer, inbDispose);
            return stream;
        }

        /// <summary>
        /// Creates a CharStream from the given string.
        /// </summary>
        static public CharStream FromString(string inString)
        {
            CharStream stream = default;
            stream.LoadString(inString);
            return stream;
        }

        /// <summary>
        /// Creates a CharStream from the given CustomTextAsset.
        /// </summary>
        static public CharStream FromCustomTextAsset(CustomTextAsset inCustomAsset)
        {
            CharStream stream = default;
            stream.LoadCustomTextAsset(inCustomAsset);
            return stream;
        }

        /// <summary>
        /// Creates a CharStream from the given TextAsset.
        /// </summary>
        static public CharStream FromTextAsset(TextAsset inTextAsset)
        {
            CharStream stream = default;
            stream.LoadTextAsset(inTextAsset);
            return stream;
        }

        /// <summary>
        /// Creates a CharStream from the given byte array.
        /// </summary>
        static public CharStream FromBytes(byte[] inBytes)
        {
            CharStream stream = default;
            stream.LoadBytes(inBytes);
            return stream;
        }

        /// <summary>
        /// Creates a CharStream from the given char array.
        /// </summary>
        static public CharStream FromChars(char[] inChars)
        {
            CharStream stream = default;
            stream.LoadChars(inChars);
            return stream;
        }

        #endregion // Factory

        #region ReadChar

        static private unsafe readonly CharStreamReadCharHandler[] s_ReadCharHandlers = new CharStreamReadCharHandler[] {
            null, ReadChar_Stream, ReadChar_CharPtr, ReadChar_CharPtr_Ring, ReadChar_BytePtr, ReadChar_BytePtr_Ring
        };

        /// <summary>
        /// Reads characters into the given array.
        /// </summary>
        public int ReadChars(int inBlockSize, char[] outBuffer, int inOutBufferOffset)
        {
            unsafe
            {
                fixed(char* charBufferPtr = outBuffer)
                {
                    return ReadChars(inBlockSize, charBufferPtr, inOutBufferOffset, outBuffer.Length);
                }
            }
        }

        /// <summary>
        /// Reads characters into the given character buffer.
        /// </summary>
        public unsafe int ReadChars(int inBlockSize, char* outBuffer, int inOutBufferOffset, int inOutBufferLength)
        {
            int bufferType = (int) Type;
            if (bufferType == 0)
            {
                throw new ArgumentNullException("ioStream");
            }

            Assert.True(outBuffer != null, "Null buffer");
            Assert.True(inOutBufferOffset <= inOutBufferLength, "Out-of-bounds write to buffer");
            return s_ReadCharHandlers[bufferType](ref this, inBlockSize, outBuffer + inOutBufferOffset, inOutBufferLength - inOutBufferOffset);
        }

        static private unsafe int ReadChar_Stream(ref CharStream ioStream, int inBlockSize, char* outBuffer, int inOutBufferLength)
        {
            if (ioStream.UnpackBuffer == null)
            {
                throw new NullReferenceException("no buffer available to unpacking stream");
            }
            
            int bytesRead = ((Stream) ioStream.Source).Read(ioStream.UnpackBuffer, 0, inBlockSize);
            if (bytesRead <= 0)
            {
                ioStream.DataFinished = 1;
                return -1;
            }

            fixed(byte* temp = ioStream.UnpackBuffer)
            {
                return ioStream.Decoder.GetChars(temp, bytesRead, outBuffer, inOutBufferLength, false);
            }
        }

        static private unsafe int ReadChar_CharPtr(ref CharStream ioStream, int inBlockSize, char* outBuffer, int inOutBufferLength)
        {
            int charsToRead = Math.Min(ioStream.DataLength, inBlockSize);
            if (charsToRead == 0)
            {
                ioStream.DataFinished = 1;
                return -1;
            }

            Unsafe.CopyArray<char>((char*) ioStream.Data + ioStream.DataOffset, charsToRead, outBuffer, inOutBufferLength);
            ioStream.DataOffset += charsToRead;
            ioStream.DataLength -= charsToRead;
            return charsToRead;
        }

        static private unsafe int ReadChar_CharPtr_Ring(ref CharStream ioStream, int inBlockSize, char* outBuffer, int inOutBufferLength)
        {
            int charsToRead = Math.Min(ioStream.DataLength, inBlockSize);
            if (charsToRead < 0)
            {
                return -1;
            }

            if (charsToRead == 0)
            {
                return 0;
            }

            int head = ioStream.DataOffset;
            int tail = (ioStream.DataOffset + charsToRead - 1) % ioStream.DataMaxLength;
            int nextHead = (tail + 1) % ioStream.DataMaxLength;

            if (head <= tail)
            {
                Unsafe.CopyArray<char>((char*) ioStream.Data + head, charsToRead, outBuffer, inOutBufferLength);
            }
            else
            {
                int segA = ioStream.DataMaxLength - head;
                Unsafe.CopyArray<char>((char*) ioStream.Data + head, segA, outBuffer, inOutBufferLength);
                Unsafe.CopyArray<char>((char*) ioStream.Data, charsToRead - segA, outBuffer + segA, inOutBufferLength - segA);
            }

            ioStream.DataLength -= charsToRead;

            if (ioStream.DataLength == 0)
            {
                ioStream.DataOffset = 0;
                if (ioStream.DataFinished > 0)
                {
                    ioStream.DataLength = -1;
                }
            }
            else
            {
                ioStream.DataOffset = nextHead;
            }
            return charsToRead;
        }

        static private unsafe int ReadChar_BytePtr(ref CharStream ioStream, int inBlockSize, char* outBuffer, int inOutBufferLength)
        {
            int bytesToRead = Math.Min(ioStream.DataLength, inBlockSize);
            if (bytesToRead == 0)
            {
                ioStream.DataFinished = 1;
                return -1;
            }

            int charsRead = ioStream.Decoder.GetChars((byte*) ioStream.Data + ioStream.DataOffset, bytesToRead, outBuffer, inOutBufferLength, false);
            ioStream.DataOffset += bytesToRead;
            ioStream.DataLength -= bytesToRead;
            return charsRead;
        }

        static private unsafe int ReadChar_BytePtr_Ring(ref CharStream ioStream, int inBlockSize, char* outBuffer, int inOutBufferLength)
        {
            int bytesToRead = Math.Min(ioStream.DataLength, inBlockSize);
            if (bytesToRead < 0)
            {
                return -1;
            }
            if (bytesToRead == 0)
            {
                return 0;
            }

            int head = ioStream.DataOffset;
            int tail = (ioStream.DataOffset + bytesToRead - 1) % ioStream.DataMaxLength;
            int nextHead = (tail + 1) % ioStream.DataMaxLength;
            int charsRead;

            if (head <= tail)
            {
                charsRead = ioStream.Decoder.GetChars((byte*) ioStream.Data + head, bytesToRead, outBuffer, inOutBufferLength, false);
            }
            else
            {
                int segA = ioStream.DataMaxLength - head;
                charsRead = ioStream.Decoder.GetChars((byte*) ioStream.Data + head, segA, outBuffer, inOutBufferLength, false);
                charsRead += ioStream.Decoder.GetChars((byte*) ioStream.Data, bytesToRead - segA, outBuffer + segA, inOutBufferLength - segA, false);
            }

            ioStream.DataLength -= bytesToRead;

            if (ioStream.DataLength == 0)
            {
                ioStream.DataOffset = 0;
                if (ioStream.DataFinished > 0)
                {
                    ioStream.DataLength = -1;
                }
            }
            else
            {
                ioStream.DataOffset = nextHead;
            }
            return charsRead;
        }

        #endregion // ReadChar

        #region ReadByte

        static private unsafe readonly CharStreamReadByteHandler[] s_ReadByteHandlers = new CharStreamReadByteHandler[] {
            null, ReadByte_Stream, ReadByte_CharPtr, ReadByte_CharPtr_Ring, ReadByte_BytePtr, ReadByte_BytePtr_Ring
        };

        /// <summary>
        /// Reads bytes into the given array.
        /// </summary>
        public int ReadBytes(int inBlockSize, byte[] outBuffer, int inOutBufferOffset)
        {
            unsafe
            {
                fixed(byte* charBufferPtr = outBuffer)
                {
                    return ReadBytes(inBlockSize, charBufferPtr, inOutBufferOffset, outBuffer.Length);
                }
            }
        }

        /// <summary>
        /// Reads bytes into the given byte buffer.
        /// </summary>
        public unsafe int ReadBytes(int inBlockSize, byte* outBuffer, int inOutBufferOffset, int inOutBufferLength)
        {
            int bufferType = (int) Type;
            if (bufferType == 0)
            {
                throw new ArgumentNullException("ioStream");
            }

            Assert.True(outBuffer != null, "Null buffer");
            Assert.True(inOutBufferOffset <= inOutBufferLength, "Out-of-bounds write to buffer");
            return s_ReadByteHandlers[bufferType](ref this, inBlockSize, outBuffer + inOutBufferOffset, inOutBufferLength - inOutBufferOffset);
        }

        static private unsafe int ReadByte_Stream(ref CharStream ioStream, int inBlockSize, byte* outBuffer, int inOutBufferLength)
        {
            if (ioStream.UnpackBuffer == null)
            {
                throw new NullReferenceException("no buffer available for unpacking stream");
            }
            
            int bytesRead = ((Stream) ioStream.Source).Read(ioStream.UnpackBuffer, 0, inBlockSize);
            if (bytesRead <= 0)
            {
                return -1;
            }

            fixed(byte* temp = ioStream.UnpackBuffer)
            {
                Unsafe.CopyArray<byte>(temp, bytesRead, outBuffer, inOutBufferLength);
            }

            return bytesRead;
        }

        static private unsafe int ReadByte_CharPtr(ref CharStream ioStream, int inBlockSize, byte* outBuffer, int inOutBufferLength)
        {
            int charsToRead = Math.Min(ioStream.DataLength, inBlockSize);
            if (charsToRead == 0)
            {
                return -1;
            }

            int readBytes = StringUtils.EncodeUFT8((char*) ioStream.Data + ioStream.DataOffset, charsToRead, outBuffer, inOutBufferLength);
            ioStream.DataOffset += charsToRead;
            ioStream.DataLength -= charsToRead;
            return readBytes;
        }

        static private unsafe int ReadByte_CharPtr_Ring(ref CharStream ioStream, int inBlockSize, byte* outBuffer, int inOutBufferLength)
        {
            int charsToRead = Math.Min(ioStream.DataLength, inBlockSize);
            if (charsToRead < 0)
            {
                return -1;
            }

            if (charsToRead == 0)
            {
                return 0;
            }

            int head = ioStream.DataOffset;
            int tail = (ioStream.DataOffset + charsToRead - 1) % ioStream.DataMaxLength;
            int nextHead = (tail + 1) % ioStream.DataMaxLength;
            int readBytes;

            if (head <= tail)
            {
                readBytes = StringUtils.EncodeUFT8((char*) ioStream.Data + head, charsToRead, outBuffer, inOutBufferLength);
            }
            else
            {
                int segA = ioStream.DataMaxLength - head;
                readBytes = StringUtils.EncodeUFT8((char*) ioStream.Data + head, segA, outBuffer, inOutBufferLength);
                readBytes += StringUtils.EncodeUFT8((char*) ioStream.Data, charsToRead - segA, outBuffer + segA, inOutBufferLength - segA);
            }

            ioStream.DataLength -= charsToRead;

            if (ioStream.DataLength == 0)
            {
                ioStream.DataOffset = 0;
                if (ioStream.DataFinished > 0)
                {
                    ioStream.DataLength = -1;
                }
            }
            else
            {
                ioStream.DataOffset = nextHead;
            }
            return readBytes;
        }

        static private unsafe int ReadByte_BytePtr(ref CharStream ioStream, int inBlockSize, byte* outBuffer, int inOutBufferLength)
        {
            int bytesToRead = Math.Min(ioStream.DataLength, inBlockSize);
            if (bytesToRead == 0)
            {
                return -1;
            }

            Unsafe.CopyArray<byte>((byte*) ioStream.Data + ioStream.DataOffset, bytesToRead, outBuffer, inOutBufferLength);
            ioStream.DataOffset += bytesToRead;
            ioStream.DataLength -= bytesToRead;
            return bytesToRead;
        }

        static private unsafe int ReadByte_BytePtr_Ring(ref CharStream ioStream, int inBlockSize, byte* outBuffer, int inOutBufferLength)
        {
            int bytesToRead = Math.Min(ioStream.DataLength, inBlockSize);
            if (bytesToRead < 0)
            {
                return -1;
            }
            if (bytesToRead == 0)
            {
                return 0;
            }

            int head = ioStream.DataOffset;
            int tail = (ioStream.DataOffset + bytesToRead - 1) % ioStream.DataMaxLength;
            int nextHead = (tail + 1) % ioStream.DataMaxLength;

            if (head <= tail)
            {
                Unsafe.CopyArray<byte>((byte*) ioStream.Data + head, bytesToRead, outBuffer, inOutBufferLength);
            }
            else
            {
                int segA = ioStream.DataMaxLength - head;
                Unsafe.CopyArray<byte>((byte*) ioStream.Data + head, segA, outBuffer, inOutBufferLength);
                Unsafe.CopyArray<byte>((byte*) ioStream.Data, bytesToRead - segA, outBuffer + segA, inOutBufferLength - segA);
            }

            ioStream.DataLength -= bytesToRead;

            if (ioStream.DataLength == 0)
            {
                ioStream.DataOffset = 0;
                if (ioStream.DataFinished > 0)
                {
                    ioStream.DataLength = -1;
                }
            }
            else
            {
                ioStream.DataOffset = nextHead;
            }
            return bytesToRead;
        }

        #endregion // ReadByte

        #region Dispose

        static private readonly CharStreamDisposeHandler[] s_DisposeHandlers = new CharStreamDisposeHandler[] {
            null, Dispose_Stream, Dispose_UnpinData, Dispose_FreeData_Heap, Dispose_Unpin_CustomTextAsset
        };

        /// <summary>
        /// Releases all temporary resources owned by the stream.
        /// </summary>
        public void Dispose()
        {
            if (Type == 0)
                return;

            int disposeType = (int) Ownership;
            if (disposeType != 0)
            {
                s_DisposeHandlers[disposeType](ref this);
            }

            Decoder?.Reset();
            Source = null;
            DataFinished = 0;
            DataLength = -1;
            DataMaxLength = 0;
            DataOffset = 0;
            Type = 0;
            Ownership = 0;
        }

        static private unsafe void Dispose_Stream(ref CharStream ioStream)
        {
            IDisposable disposableSource = ioStream.Source as IDisposable;
            if (disposableSource != null)
            {
                disposableSource.Dispose();
            }
            ioStream.Source = null;
            ioStream.UnpackBuffer = null;
        }

        static private unsafe void Dispose_UnpinData(ref CharStream ioStream)
        {
            if (ioStream.PinnedDataHandle.IsAllocated)
            {
                ioStream.PinnedDataHandle.Free();
            }

            ioStream.Data = null;
        }

        static private unsafe void Dispose_FreeData_Heap(ref CharStream ioStream)
        {
            Unsafe.TryFree(ref ioStream.Data);
        }
    
        static private void Dispose_Unpin_CustomTextAsset(ref CharStream ioStream)
        {
            CustomTextAsset asset = ioStream.Source as CustomTextAsset;
            asset.ReleasePinnedBytes();
            ioStream.Source = null;
        }

        #endregion // Dispose
    
        #region Queue

        /// <summary>
        /// Writes characters to the end of the stream to be later read.
        /// This is only a valid operation for a Character Buffer stream.
        /// </summary>
        public unsafe void QueueChars(char* inData, int inDataCount)
        {
            if (Type != CharStreamType.CharPtr_Ring)
                throw new InvalidOperationException(string.Format("Cannot queue chars to a buffer of type {0}", Type.ToString()));

            if (DataLength + inDataCount > DataMaxLength)
                throw new InvalidOperationException(string.Format("No more room in buffer - attempting to add {0} when only {1} are available", inDataCount, DataMaxLength - DataLength));

            if (inDataCount <= 0)
                return;

            int writeHead = (DataOffset + DataLength) % DataMaxLength;
            int writeTail = (writeHead + inDataCount - 1) % DataMaxLength;

            if (writeHead <= writeTail)
            {
                Unsafe.CopyArray<char>(inData, inDataCount, (char*) Data + writeHead, inDataCount);
            }
            else
            {
                int segA = DataMaxLength - writeHead;
                Unsafe.CopyArray<char>(inData, segA, (char*) Data + writeHead, segA);
                Unsafe.CopyArray<char>(inData + segA, inDataCount - segA, (char*) Data, inDataCount - segA);
            }

            DataLength += inDataCount;
        }

        /// <summary>
        /// Writes characters to the head of the stream to be later read.
        /// This is only a valid operation for a Character Buffer stream.
        /// </summary>
        public unsafe void InsertChars(char* inData, int inDataCount)
        {
            if (Type != CharStreamType.CharPtr_Ring)
                throw new InvalidOperationException(string.Format("Cannot queue chars to a buffer of type {0}", Type.ToString()));

            if (DataLength + inDataCount > DataMaxLength)
                throw new InvalidOperationException(string.Format("No more room in buffer - attempting to add {0} when only {1} are available", inDataCount, DataMaxLength - DataLength));

            if (inDataCount <= 0)
                return;

            int writeHead = (DataOffset + DataMaxLength - DataLength) % DataMaxLength;
            int writeTail = (writeHead + inDataCount - 1) % DataMaxLength;

            if (writeHead <= writeTail)
            {
                Unsafe.CopyArray<char>(inData, inDataCount, (char*) Data + writeHead, inDataCount);
            }
            else
            {
                int segA = DataMaxLength - writeHead;
                Unsafe.CopyArray<char>(inData, segA, (char*) Data + writeHead, segA);
                Unsafe.CopyArray<char>(inData + segA, inDataCount - segA, (char*) Data, inDataCount - segA);
            }

            DataOffset = writeHead;
            DataLength += inDataCount;
        }

        /// <summary>
        /// Writes bytes to the end of the stream to be later read.
        /// This is only a valid operation for a Byte Buffer stream.
        /// </summary>
        public unsafe void QueueBytes(byte* inData, int inDataCount)
        {
            if (Type != CharStreamType.BytePtr_Ring)
                throw new InvalidOperationException(string.Format("Cannot queue bytes to a buffer of type {0}", Type.ToString()));

            if (DataLength + inDataCount > DataMaxLength)
                throw new InvalidOperationException(string.Format("No more room in buffer - attempting to add {0} when only {1} are available", inDataCount, DataMaxLength - DataLength));

            if (inDataCount <= 0)
                return;

            int writeHead = (DataOffset + DataLength) % DataMaxLength;
            int writeTail = (writeHead + inDataCount - 1) % DataMaxLength;

            if (writeHead <= writeTail)
            {
                Unsafe.CopyArray<byte>(inData, inDataCount, (byte*) Data + writeHead, inDataCount);
            }
            else
            {
                int segA = DataMaxLength - writeHead;
                Unsafe.CopyArray<byte>(inData, segA, (byte*) Data + writeHead, segA);
                Unsafe.CopyArray<byte>(inData + segA, inDataCount - segA, (byte*) Data, inDataCount - segA);
            }
            
            DataLength += inDataCount;
        }

        /// <summary>
        /// Writes bytes to the head of the stream to be later read.
        /// This is only a valid operation for a Byte Buffer stream.
        /// </summary>
        public unsafe void InsertBytes(byte* inData, int inDataCount)
        {
            if (Type != CharStreamType.BytePtr_Ring)
                throw new InvalidOperationException(string.Format("Cannot queue bytes to a buffer of type {0}", Type.ToString()));

            if (DataLength + inDataCount > DataMaxLength)
                throw new InvalidOperationException(string.Format("No more room in buffer - attempting to add {0} when only {1} are available", inDataCount, DataMaxLength - DataLength));

            if (inDataCount <= 0)
                return;

            int writeHead = (DataOffset + DataMaxLength - DataLength) % DataMaxLength;
            int writeTail = (writeHead + inDataCount - 1) % DataMaxLength;

            if (writeHead <= writeTail)
            {
                Unsafe.CopyArray<byte>(inData, inDataCount, (byte*) Data + writeHead, inDataCount);
            }
            else
            {
                int segA = DataMaxLength - writeHead;
                Unsafe.CopyArray<byte>(inData, segA, (byte*) Data + writeHead, segA);
                Unsafe.CopyArray<byte>(inData + segA, inDataCount - segA, (byte*) Data, inDataCount - segA);
            }
            
            DataLength += inDataCount;
            DataOffset = writeHead;
        }

        #endregion // Queue
    }

    /// <summary>
    /// CharStream creation parameters.
    /// </summary>
    public struct CharStreamParams
    {
        internal string Name;
        internal object Source;
        internal unsafe void* DataPtr;
        internal int DataLength;
        internal CharStreamSourceType Type;
        internal bool Dispose;
        internal byte[] UnpackBuffer;

        /// <summary>
        /// Will create a CharStream from the given Stream.
        /// </summary>
        static public CharStreamParams FromStream(Stream inStream, byte[] inUnpackBuffer, bool inbDispose = true, string inName = null)
        {
            if (inStream == null)
                return default(CharStreamParams);

            CharStreamParams stream;
            stream.Type = CharStreamSourceType.Stream;

            stream.Source = inStream;
            stream.UnpackBuffer = inUnpackBuffer;
            stream.Dispose = inbDispose;
            unsafe
            {
                stream.DataPtr = null;
            }
            stream.DataLength = 0;
            stream.Name = inName;
            
            return stream;
        }

        /// <summary>
        /// Will create a CharStream from the given string.
        /// </summary>
        static public CharStreamParams FromString(string inString, string inName = null)
        {
            CharStreamParams stream;
            stream.Type = CharStreamSourceType.String;

            stream.Source = inString ?? string.Empty;
            stream.Dispose = false;
            unsafe
            {
                stream.DataPtr = null;
            }
            stream.DataLength = 0;
            stream.UnpackBuffer = null;
            stream.Name = inName;

            return stream;
        }

        /// <summary>
        /// Will create a CharStream from the given CustomTextAsset.
        /// </summary>
        static public CharStreamParams FromCustomTextAsset(CustomTextAsset inCustomAsset)
        {
            if (inCustomAsset == null)
                return default(CharStreamParams);

            CharStreamParams stream;
            stream.Type = CharStreamSourceType.CustomTextAsset;

            stream.Source = inCustomAsset;
            stream.Dispose = false;
            unsafe
            {
                stream.DataPtr = null;
            }
            stream.DataLength = 0;
            stream.UnpackBuffer = null;
            stream.Name = inCustomAsset.name;

            return stream;
        }

        /// <summary>
        /// Will create a CharStream from the given TextAsset.
        /// </summary>
        static public CharStreamParams FromTextAsset(TextAsset inTextAsset)
        {
            if (inTextAsset == null)
                return default(CharStreamParams);

            CharStreamParams stream;
            stream.Type = CharStreamSourceType.TextAsset;

            stream.Source = inTextAsset;
            stream.Dispose = false;
            unsafe
            {
                stream.DataPtr = null;
            }
            stream.DataLength = 0;
            stream.UnpackBuffer = null;
            stream.Name = inTextAsset.name;

            return stream;
        }

        /// <summary>
        /// Will create a CharStream from the given byte array.
        /// </summary>
        static public CharStreamParams FromBytes(byte[] inBytes, string inName = null)
        {
            if (inBytes == null)
                return default(CharStreamParams);

            CharStreamParams stream;
            stream.Type = CharStreamSourceType.ByteArr;

            stream.Source = inBytes;
            stream.Dispose = false;
            unsafe
            {
                stream.DataPtr = null;
            }
            stream.DataLength = 0;
            stream.UnpackBuffer = null;
            stream.Name = inName;

            return stream;
        }

        /// <summary>
        /// Will create a CharStream from the given char array.
        /// </summary>
        static public CharStreamParams FromChars(char[] inChars, string inName = null)
        {
            if (inChars == null)
                return default(CharStreamParams);

            CharStreamParams stream;
            stream.Type = CharStreamSourceType.CharArr;

            stream.Source = inChars;
            stream.Dispose = false;
            unsafe
            {
                stream.DataPtr = null;
            }
            stream.DataLength = 0;
            stream.UnpackBuffer = null;
            stream.Name = inName;

            return stream;
        }
    }

    internal enum CharStreamType : byte
    {
        None = 0,
        Stream, // c# stream
        CharPtr, // char pointer (advancing)
        CharPtr_Ring, // char pointer (buffer)
        BytePtr, // byte pointer (advancing)
        BytePtr_Ring, // byte pointer (buffer)
    }

    internal enum CharStreamSourceType : byte
    {
        None,
        Stream,
        CharArr,
        CharArr_Ring,
        CharPtr,
        CharPtr_Ring,
        ByteArr,
        ByteArr_Ring,
        BytePtr,
        BytePtr_Ring,
        CustomTextAsset,
        TextAsset,
        String
    }

    internal enum CharStreamResourceOwnership : byte
    {
        None = 0,
        DisposeStream,
        UnpinData,
        FreeData_Heap,
        Unpin_CustomTextAsset
    }

    internal unsafe delegate int CharStreamReadCharHandler(ref CharStream ioStream, int inBlockSize, char* outBuffer, int inOutBufferLength);
    internal unsafe delegate int CharStreamReadByteHandler(ref CharStream ioStream, int inBlockSize, byte* outBuffer, int inOutBufferLength);
    internal unsafe delegate void CharStreamDisposeHandler(ref CharStream ioStream);
}