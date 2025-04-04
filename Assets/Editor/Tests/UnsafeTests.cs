﻿#if UNITY_2021_2_OR_NEWER && !BEAUUTIL_DISABLE_FUNCTION_POINTERS
#define SUPPORTS_FUNCTION_POINTERS
#endif // UNITY_2021_2_OR_NEWER

using System;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using BeauUtil.Graph;

using Log = BeauUtil.Debugger.Log;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using System.Reflection;

namespace BeauUtil.UnitTests
{
    public unsafe class UnsafeTests
    {
        static private Unsafe.ArenaHandle Allocator = default;

        static private void Setup()
        {
            Unsafe.TryDestroyArena(ref Allocator);
            Allocator = Unsafe.CreateArena(4096);
        }

        [Test]
        static public void CopyBufferToArray()
        {
            Setup();

            Vector2[] dest = new Vector2[64];
            Vector2* src = (Vector2*) Unsafe.AllocArray<Vector2>(Allocator, 64);
            src[0] = new Vector2(64, 77);
            src[13] = new Vector2(1, 55);
            src[63] = new Vector2(-25, 787);

            Unsafe.CopyArray(src, 64, dest);

            Assert.AreEqual(src[0].x, dest[0].x, float.Epsilon);
            Assert.AreEqual(src[0].y, dest[0].y, float.Epsilon);

            Log.Msg("uninitialized memory: {0} {1} {2}", src[15], src[44], src[62]);

            Assert.AreEqual(src[63].x, dest[63].x, float.Epsilon);
            Assert.AreEqual(src[63].y, dest[63].y, float.Epsilon);

            Unsafe.TryDestroyArena(ref Allocator);
        }

        [Test]
        static public void CopyArrayToBuffer()
        {
            Setup();

            using(BeauUtil.Debugger.Profiling.Time("doing thing"))
            {
                Vector2[] src = new Vector2[64];
                Vector2* dest = (Vector2*) Unsafe.AllocArray<Vector2>(Allocator, 64);
                src[0] = new Vector2(64, 77);
                src[13] = new Vector2(1, 55);
                src[63] = new Vector2(-25, 787);

                Unsafe.CopyArray(src, dest, 64);

                Assert.AreEqual(src[0].x, dest[0].x, float.Epsilon);
                Assert.AreEqual(src[0].y, dest[0].y, float.Epsilon);

                Assert.AreEqual(src[63].x, dest[63].x, float.Epsilon);
                Assert.AreEqual(src[63].y, dest[63].y, float.Epsilon);

                Unsafe.TryDestroyArena(ref Allocator);
            }
        }

        [Test]
        static public void CopyBufferToSelf()
        {
            Setup();

            Vector2* src = (Vector2*) Unsafe.AllocArray<Vector2>(Allocator, 70);
            Vector2* dest = src + 32;
            src[0] = new Vector2(64, 77);
            src[1] = new Vector2(4, 4);
            src[2] = new Vector2(3, 3);
            src[13] = new Vector2(1, 55);
            src[31] = new Vector2(-25, 787);

            StringBuilder sb = new StringBuilder(256);
            for(int i = 0; i < 70; i++) {
                sb.Append(src[i]).Append('\n');;
            }
            Log.Msg(sb.ToString());

            Unsafe.CopyArray<Vector2>(src, 32, dest, 32);

            sb = new StringBuilder(256);
            for(int i = 0; i < 70; i++) {
                sb.Append(src[i]).Append('\n');;
            }
            Log.Msg(sb.ToString());

            Unsafe.TryDestroyArena(ref Allocator);
        }

        [Test]
        static public void QuickSortOnBuffer()
        {
            int count = ushort.MaxValue;
            float* buffer = stackalloc float[count];
            for(int i = 0; i < count; i++)
                buffer[i] = UnityEngine.Random.value;
            
            Unsafe.Quicksort<float>(buffer, count);

            for(int i = 1; i < count; i++)
            {
                Assert.GreaterOrEqual(buffer[i], buffer[i - 1]);
            }
        }

        [Test]
        static public void CanPinStringBuilder()
        {
            StringBuilder sb = new StringBuilder(1024);
            sb.Append("abcdef");
            using(var pin = Unsafe.PinString(sb))
            {
                pin.Address[0] = 'g';
                pin.Address[4] = '9';
            }
            string result = sb.Flush();
            Assert.AreEqual(result, "gbcd9f");
        }

        [Test]
        static public void CanPinList()
        {
            List<Vector3> list = new List<Vector3>();
            list.Add(Vector3.up);
            list.Add(Vector3.up);
            list.Add(Vector3.up);
            list.Add(Vector3.down);
            list.Add(Vector3.left);
            list.Add(Vector3.right);
            list.Add(Vector3.left);
            list.Add(Vector3.left);
            using(var pin = Unsafe.PinList(list))
            {
                pin.Address[2] = Vector3.down;
                pin.Address[7] = Vector3.right;
            }
            Assert.AreEqual(list[7], Vector3.right);
            Assert.AreEqual(list[1], Vector3.up);
            Assert.AreEqual(list[2], Vector3.down);
        }

        [Test]
        static public void StackArena() {
            byte* allocStack = stackalloc byte[2048];
            Unsafe.ArenaHandle allocator = Unsafe.CreateArena(allocStack, 2048);
            Assert.IsTrue(allocator.Validate());
        }

        [Test]
        static public void ArenaRewind() {
            Setup();

            Allocator.Alloc(56);

            Allocator.Push();

            int size = Allocator.UsedBytes();

            Allocator.Alloc(64);
            Allocator.Alloc(64);

            Allocator.Pop();

            Assert.AreEqual(size, Allocator.UsedBytes());

            Unsafe.TryDestroyArena(ref Allocator);
        }

        [Test]
        static public void NestedArena() {
            Setup();

            Unsafe.ArenaHandle nestedArena = Unsafe.CreateArena(Allocator, 1024);
            Assert.IsTrue(nestedArena.Validate());
            Assert.IsTrue(Allocator.UsedBytes() > 0);
            Log.Msg("header size = {0}", Allocator.UsedBytes() - 1024);

            Unsafe.TryDestroyArena(ref Allocator);
        }

        [Test]
        static public void CanSwapEndiannessUint()
        {
            uint initialValue = 0xBADDF00D;
            uint swappedValue = initialValue;
            Unsafe.SwapEndian(ref swappedValue);
            AssertByteOrderIsReversed(initialValue, swappedValue);
            Unsafe.SwapEndian(ref swappedValue);
            Assert.IsTrue(initialValue == swappedValue, "Swap of {0} is not equal", initialValue.GetType().FullName);
        }

        [Test]
        static public void CanSwapEndiannessInt()
        {
            int initialValue = 0xADDF00D;
            int swappedValue = initialValue;
            Unsafe.SwapEndian(ref swappedValue);
            AssertByteOrderIsReversed(initialValue, swappedValue);
            Unsafe.SwapEndian(ref swappedValue);
            Assert.IsTrue(initialValue == swappedValue, "Swap of {0} is not equal", initialValue.GetType().FullName);
        }

        [Test]
        static public void CanSwapEndiannessFloat()
        {
            float initialValue = 8.50423f;
            float swappedValue = initialValue;
            Unsafe.SwapEndian(ref swappedValue);
            AssertByteOrderIsReversed(initialValue, swappedValue);
            Unsafe.SwapEndian(ref swappedValue);
            Assert.IsTrue(initialValue == swappedValue, "Swap of {0} is not equal", initialValue.GetType().FullName);
        }

        [Test]
        static public void CanSwapEndiannessUshort()
        {
            ushort initialValue = 0xF00D;
            ushort swappedValue = initialValue;
            Unsafe.SwapEndian(ref swappedValue);
            AssertByteOrderIsReversed(initialValue, swappedValue);
            Unsafe.SwapEndian(ref swappedValue);
            Assert.IsTrue(initialValue == swappedValue, "Swap of {0} is not equal", initialValue.GetType().FullName);
        }

        [Test]
        static public void CanSwapEndiannessShort()
        {
            short initialValue = 0xADD;
            short swappedValue = initialValue;
            Unsafe.SwapEndian(ref swappedValue);
            AssertByteOrderIsReversed(initialValue, swappedValue);
            Unsafe.SwapEndian(ref swappedValue);
            Assert.IsTrue(initialValue == swappedValue, "Swap of {0} is not equal", initialValue.GetType().FullName);
        }

        [Test]
        static public void CanSwapEndiannessLong()
        {
            long initialValue = 0xABCDEF01FF;
            long swappedValue = initialValue;
            Unsafe.SwapEndian(ref swappedValue);
            AssertByteOrderIsReversed(initialValue, swappedValue);
            Unsafe.SwapEndian(ref swappedValue);
            Assert.IsTrue(initialValue == swappedValue, "Swap of {0} is not equal", initialValue.GetType().FullName);
        }

        [Test]
        static public void CanSwapEndiannessUlong()
        {
            ulong initialValue = 0xFBCDEF01FF;
            ulong swappedValue = initialValue;
            Unsafe.SwapEndian(ref swappedValue);
            AssertByteOrderIsReversed(initialValue, swappedValue);
            Unsafe.SwapEndian(ref swappedValue);
            Assert.IsTrue(initialValue == swappedValue, "Swap of {0} is not equal", initialValue.GetType().FullName);
        }

        [Test]
        static public void CanSwapEndiannessDouble()
        {
            double initialValue = 8.50423;
            double swappedValue = initialValue;
            Unsafe.SwapEndian(ref swappedValue);
            AssertByteOrderIsReversed(initialValue, swappedValue);
            Unsafe.SwapEndian(ref swappedValue);
            Assert.IsTrue(initialValue == swappedValue, "Swap of {0} is not equal", initialValue.GetType().FullName);
        }

        static private void AssertByteOrderIsReversed<T>(T a, T b)
            where T : unmanaged
        {
            int byteSize = sizeof(T);
            byte* aPtr = (byte*) &a;
            byte* bPtr = (byte*) &b;

            byte* aNow = aPtr;
            byte* bNow = bPtr + byteSize - 1;

            for(int i = 0; i < byteSize; i++)
            {
                Assert.IsTrue(*aNow == *bNow);
                aNow++;
                bNow--;
            }
        }

        [Test]
        static public void CanWriteUTF8()
        {
            byte* writeBuffer = stackalloc byte[1024];

            string testString = "ABCDeFGhIjklM1841kljdfà, è, ì, ò, ù, ❤";

            byte* writeHead = writeBuffer;
            int written = 0, capacity = 1024;
            Unsafe.WriteUTF8(testString, ref writeHead, ref written, capacity);

            byte* readHead = writeBuffer;
            int remaining = written;
            string hopefullyTestStringAgain = Unsafe.ReadUTF8(ref readHead, ref remaining);

            Assert.AreEqual(testString, hopefullyTestStringAgain);
        }

        [Test]
        static public void CanClearMemory()
        {
            Setup();

            byte* alloc = (byte*) Allocator.Alloc(256);
            
            for(int i = 0; i < 256; i++)
            {
                alloc[i] = (byte) RNG.Instance.Next(1, 205);
            }

            Unsafe.Clear(alloc, 256);

            for(int i = 0; i < 256; i++)
            {
                Assert.AreEqual(0, alloc[i]);
            }
        }

        [Test]
        static public void ZeroOnAllocateWorks()
        {
            byte* localBUffer = stackalloc byte[1024];
            for(int i = 0; i < 1024; i++)
            {
                localBUffer[i] = (byte) RNG.Instance.Next(1, 205);
            }

            Unsafe.ArenaHandle arena = Unsafe.CreateArena(localBUffer, 1024, null, Unsafe.AllocatorFlags.ZeroOnAllocate);

            UnsafeSpan<byte> internalBuf = arena.AllocSpan<byte>(128);

            for(int i = 0; i < internalBuf.Length; i++)
            {
                Assert.AreEqual(0, internalBuf[i]);
            }

            UnsafeSpan<int> internalBuf2 = arena.AllocSpan<int>(64);
        }

        static void InternalFunc_JitTest(int a)
        {
            int x = 20;
            while(x-- > 0) { }
            Debug.Log("from pointer: " + a);
        }

        static void InternalFunc2(float a)
        {
            a *= 1.3f;
            Debug.Log("from casted delegate x1.3: " + a);
        }

#if SUPPORTS_FUNCTION_POINTERS

        static float itof(int i) {
            return (float) i;
        }

        [Test]
        static public void CanTakeFunctionPointers_JitTest()
        {
            CastableAction<int> a = CastableAction<int>.Create(&InternalFunc_JitTest);
            CastableAction<int> b = CastableAction<int>.Create<float>(InternalFunc2);

            CastableArgument.RegisterConverter<int, float>(&itof);

            a.Invoke(15); 
            b.Invoke(16);

            // invoke many times in succession to hopefully trigger JIT compilation
            for (int i = 0; i < 10000; i++) {
                a.Invoke(i);
                //b.Invoke(i);
            }

            Debug.Log(a.Equals(&InternalFunc_JitTest));
        }

#endif // SUPPORTS_FUNCTION_POINTERS

        static private void SpecializedTestMethod()
        {
            Log.Msg("Hey you invoked me!");
        }

        [Test]
        static public void CanUseMethodInvocationHelper()
        {
            MethodInvocationHelper helper = default;
            bool loaded = helper.TryLoad(typeof(UnsafeTests).GetMethod("SpecializedTestMethod", BindingFlags.Static | BindingFlags.NonPublic), DefaultStringConverter.Instance);
            Assert.IsTrue(loaded);

            bool invoked = helper.TryInvoke(null, default, DefaultStringConverter.Instance, null, out var _);
            Assert.IsTrue(invoked);
        }

        static private readonly byte[] SomeHashableByteSequence = Encoding.UTF8.GetBytes("WhoaThereThisIsAHashableStringPardner");
        static private readonly byte[] AnotherHashableByteSequence = Encoding.UTF8.GetBytes("What'reyadoingreadingthesetestingstringsanywho??");

        [Test]
        static public void CanHash32() {
            string testInput = "CanThisHashMurmur";
            fixed(char* ptr = testInput) {
                uint hash = Unsafe.Hash32(ptr, testInput.Length * sizeof(char));
                Debug.LogFormat("Hash32 of '{0}' of length {1} is {2}", testInput, testInput.Length * sizeof(char), hash);
                Assert.AreEqual(657669070, hash);
            }

            uint* alignedSome = stackalloc uint[SomeHashableByteSequence.Length / 4 + 1];
            byte* alignedSomeAsByte = (byte*) alignedSome;
            fixed (byte* someBytes = SomeHashableByteSequence) {
                Unsafe.FastCopy(someBytes, SomeHashableByteSequence.Length, alignedSomeAsByte);
            }

            uint someHash = Unsafe.Hash32(alignedSomeAsByte, SomeHashableByteSequence.Length);
            Debug.LogFormat("Hash32 of SomeHashable of length {0} is {1}", SomeHashableByteSequence.Length, someHash);
            Assert.AreEqual(288014056, someHash);

            uint* alignedAnother = stackalloc uint[AnotherHashableByteSequence.Length / 4 + 1];
            byte* alignedAnotherAsByte = (byte*) alignedAnother;
            fixed (byte* anotherBytes = AnotherHashableByteSequence) {
                Unsafe.FastCopy(anotherBytes, AnotherHashableByteSequence.Length, alignedAnother);
            }

            uint anotherHash = Unsafe.Hash32(alignedAnotherAsByte, AnotherHashableByteSequence.Length);
            Debug.LogFormat("Hash32 of AnotherHashable of length {0} is {1}", AnotherHashableByteSequence.Length, anotherHash);
            Assert.AreEqual(1034883058, anotherHash);
        }

        [Test]
        static public void CanHash64() {
            string testInput = "CanThisHashMurmur";
            fixed (char* ptr = testInput) {
                ulong* copy = stackalloc ulong[sizeof(char) * testInput.Length / 8 + 1];
                Unsafe.FastCopy(ptr, testInput.Length * sizeof(char), copy);

                ulong hash = Unsafe.Hash64(copy, testInput.Length * sizeof(char));
                Debug.LogFormat("Hash64 of '{0}' of length {1} is {2}", testInput, testInput.Length * sizeof(char), hash);
                Assert.AreEqual(17327309007015566179, hash);
            }

            ulong* alignedSome = stackalloc ulong[SomeHashableByteSequence.Length / 8 + 1];
            byte* alignedSomeAsByte = (byte*) alignedSome;
            fixed (byte* someBytes = SomeHashableByteSequence) {
                Unsafe.FastCopy(someBytes, SomeHashableByteSequence.Length, alignedSomeAsByte);
            }

            ulong someHash = Unsafe.Hash64(alignedSomeAsByte, SomeHashableByteSequence.Length);
            Debug.LogFormat("Hash64 of SomeHashable of length {0} is {1}", SomeHashableByteSequence.Length, someHash);
            Assert.AreEqual(7260654691530379117, someHash);

            ulong* alignedAnother = stackalloc ulong[AnotherHashableByteSequence.Length / 8 + 1];
            byte* alignedAnotherAsByte = (byte*) alignedAnother;
            fixed (byte* anotherBytes = AnotherHashableByteSequence) {
                Unsafe.FastCopy(anotherBytes, AnotherHashableByteSequence.Length, alignedAnother);
            }

            ulong anotherHash = Unsafe.Hash64(alignedAnotherAsByte, AnotherHashableByteSequence.Length);
            Debug.LogFormat("Hash64 of AnotherHashable of length {0} is {1}", AnotherHashableByteSequence.Length, anotherHash);
            Assert.AreEqual(12925064926933939798, anotherHash); 
        }

        [Test]
        static public void CanConvertBetweenPointersAndRefs()
        {
            int x = 0;
            int* xPtr = &x;
            *xPtr = 8;

            ref int xRef = ref Unsafe.AsRef(xPtr);

            Assert.AreEqual(8, *xPtr);
            Assert.AreEqual(8, xRef);

            *xPtr = 16;

            Assert.AreEqual(16, xRef);

            xRef = 12;

            Assert.AreEqual(12, *xPtr);
            Assert.AreEqual(12, x);
        }
    }
}