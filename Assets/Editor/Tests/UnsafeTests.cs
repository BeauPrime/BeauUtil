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
        static private Unsafe.ArenaHandle Allocator;

        static private void Setup()
        {
            Unsafe.TryFreeArena(ref Allocator);
            Allocator = Unsafe.CreateArena(1024);
        }

        [Test]
        static public void CopyBufferToArray()
        {
            Setup();

            Vector2[] dest = new Vector2[64];
            Vector2* src = (Vector2*) Unsafe.AllocArray<Vector2>(64);
            src[0] = new Vector2(64, 77);
            src[13] = new Vector2(1, 55);
            src[63] = new Vector2(-25, 787);

            Unsafe.Copy(src, 64, dest);

            Assert.AreEqual(src[0].x, dest[0].x, float.Epsilon);
            Assert.AreEqual(src[0].y, dest[0].y, float.Epsilon);

            Log.Msg("uninitialized memory: {0} {1} {2}", src[15], src[44], src[62]);

            Assert.AreEqual(src[63].x, dest[63].x, float.Epsilon);
            Assert.AreEqual(src[63].y, dest[63].y, float.Epsilon);
        }

        [Test]
        static public void CopyArrayToBuffer()
        {
            Setup();

            Vector2[] src = new Vector2[64];
            Vector2* dest = (Vector2*) Unsafe.AllocArray<Vector2>(64);
            src[0] = new Vector2(64, 77);
            src[13] = new Vector2(1, 55);
            src[63] = new Vector2(-25, 787);

            Unsafe.Copy(src, 64, dest, 64);

            Assert.AreEqual(src[0].x, dest[0].x, float.Epsilon);
            Assert.AreEqual(src[0].y, dest[0].y, float.Epsilon);

            Assert.AreEqual(src[63].x, dest[63].x, float.Epsilon);
            Assert.AreEqual(src[63].y, dest[63].y, float.Epsilon);
        }
    }
}