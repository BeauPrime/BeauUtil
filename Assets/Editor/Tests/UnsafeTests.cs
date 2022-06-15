using System;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using BeauUtil.Graph;

using Log = BeauUtil.Debugger.Log;

namespace BeauUtil.UnitTests
{
    public unsafe class UnsafeTests
    {
        static private Unsafe.ArenaHandle Allocator = default;

        static private void Setup()
        {
            Unsafe.TryDestroyArena(ref Allocator);
            Allocator = Unsafe.CreateArena(1024);
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
    }
}