using System;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using BeauUtil.Graph;

using Log = BeauUtil.Debugger.Log;
using System.Collections.Generic;

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
            float* buffer = stackalloc float[64];
            for(int i = 0; i < 64; i++)
                buffer[i] = UnityEngine.Random.value;
            
            Unsafe.Quicksort<float>(buffer, 64);

            for(int i = 1; i < 64; i++)
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
        static public void NestedArena() {
            Setup();

            Unsafe.ArenaHandle nestedArena = Unsafe.CreateArena(Allocator, 1024);
            Assert.IsTrue(nestedArena.Validate());
            Assert.IsTrue(Allocator.UsedBytes() > 0);
            Log.Msg("header size = {0}", Allocator.UsedBytes() - 1024);

            Unsafe.TryDestroyArena(ref Allocator);
        }
    }
}