using System;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace BeauUtil.UnitTests
{
    static public class CollectionTests
    {
        [Test]
        static public void BitTest()
        {
            int fullMask = 0;
            for(int i = 0; i < Bits.Length; ++i)
            {
                Bits.Add(ref fullMask, i);
                Debug.LogFormat("base10: {0}; base2: {1}", fullMask, Convert.ToString(fullMask, 2).PadLeft(32, '0'));
            }

            Assert.AreEqual(fullMask, Bits.All32);

            for(int i = 0; i < 32; ++i)
            {
                bool bMatch = Bits.Contains(fullMask, i);
                Assert.True(bMatch);
            }
        }

        [Test]
        static public void FixedRingBufferTest()
        {
            RingBuffer<int> buffer = new RingBuffer<int>(4, RingBufferMode.Fixed);
            buffer.PushBack(5);
            buffer.PushBack(16);
            buffer.PushBack(17);

            Assert.AreEqual(5, buffer.PopFront());
            Assert.AreEqual(17, buffer.PopBack());
            Assert.AreEqual(16, buffer.PopFront());
            Assert.AreEqual(buffer.Count, 0);
        }

        [Test]
        static public void OverwriteRingBufferTest()
        {
            RingBuffer<int> buffer = new RingBuffer<int>(4, RingBufferMode.Overwrite);
            buffer.PushBack(0);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);
            buffer.PushBack(4);

            Assert.IsFalse(buffer.Contains(0));
            Assert.AreEqual(1, buffer.PopFront());
            Assert.AreEqual(4, buffer.PopBack());
            Assert.AreEqual(2, buffer.PopFront());
            Assert.AreEqual(3, buffer.PopFront());
            Assert.AreEqual(buffer.Count, 0);

            buffer.PushFront(5);
            buffer.PushFront(6);
            buffer.PushFront(7);
            buffer.PushFront(8);
            buffer.PushFront(9);

            Assert.IsFalse(buffer.Contains(5));
            Assert.AreEqual(6, buffer.PopBack());
            Assert.AreEqual(9, buffer.PopFront());
            Assert.AreEqual(7, buffer.PopBack());
            Assert.AreEqual(8, buffer.PopFront());
            Assert.AreEqual(buffer.Count, 0);
        }

        [Test]
        static public void ExpandingRingBufferTest()
        {
            RingBuffer<int> buffer = new RingBuffer<int>();
            buffer.PushBack(5);
            buffer.PushBack(16);
            buffer.PushBack(17);

            Assert.Greater(buffer.Capacity, 0);

            Assert.AreEqual(5, buffer.PopFront());
            Assert.AreEqual(17, buffer.PopBack());
            
            buffer.Clear();

            Assert.AreEqual(0, buffer.Count);

            buffer.PushBack(5);
            buffer.PushBack(6);
            buffer.PushBack(7);
            buffer.PushBack(8);
            buffer.PushBack(9);

            buffer.SetCapacity(16);
        }

        [Test]
        static public void WeightedSetTest()
        {
            System.Random r = new System.Random(600);

            WeightedSet<char> set = new WeightedSet<char>().Add('a', 3).Add('b', 4).Add('c', 5).Add('d', 6);
            set.Remove('b');

            Assert.AreEqual(0, set.GetWeight('b'));
            Assert.AreEqual(5, set.GetWeight('c'));

            set.SetWeight('c', 7);
            Assert.AreEqual(7, set.GetWeight('c'));

            float newWeight = set.ChangeWeight('d', -10);
            Assert.AreEqual(0, newWeight);
            Assert.AreEqual(0, set.GetWeight('d'));

            set.ChangeWeight('d', 15);
            Assert.AreEqual(15, set.GetWeight('d'));

            set.ChangeWeight('a', 10);
            Assert.AreEqual(13, set.GetWeight('a'));

            int a = 0, b = 0, c = 0, d = 0;

            int iter = 100000;
            for(int i = 0; i < iter; ++i)
            {
                switch (r.Choose(set))
                {
                    case 'a':
                        ++a;
                        break;

                    case 'b':
                        ++b;
                        break;

                    case 'c':
                        ++c;
                        break;

                    case 'd':
                        ++d;
                        break;
                }
            }

            Debug.LogFormat("a={0}%, b={1}%, c={2}%, d={3}%, ({4} samples)",
                100f * a / iter,
                100f * b / iter,
                100f * c / iter,
                100f * d / iter,
                iter);
        }
    }
}