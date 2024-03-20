using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace BeauUtil.UnitTests
{
    static public class CollectionTests
    {
        [Test]
        static public void BitTest()
        {
            int fullMask = 0;
            for (int i = 0; i < Bits.Length; ++i)
            {
                Bits.Add(ref fullMask, i);
                Debug.LogFormat("base10: {0}; base2: {1}", fullMask, Convert.ToString(fullMask, 2).PadLeft(32, '0'));
            }

            Assert.AreEqual(fullMask, Bits.All32);
            Assert.AreEqual(32, Bits.Count(fullMask));

            for (int i = 0; i < 32; ++i)
            {
                bool bMatch = Bits.Contains(fullMask, i);
                Assert.True(bMatch);
            }

            Bits.Count(-1);

            int someMask = 1 | 2 | 16;
            Assert.True(Bits.Count(someMask) == 3);
        }

        private enum TestEnum : byte
        {
            A0 = 0,
            A1,
            A2,
            A3,
            A4 = 255,
            A5 = 90,
            A6 = 160
        }

        [Flags]
        private enum TestFlagEnum : byte
        {
            F0 = 1,
            F1 = 2,
            F2 = 4,
            F3 = 8,
            F4 = 16
        }

        [Test]
        static public void EnumsTest()
        {
            Assert.True(Enums.ToInt(TestEnum.A4) == (int) TestEnum.A4);
            Assert.True(Enums.ToInt(TestEnum.A6) == (int) TestEnum.A6);

            int a5 = Enums.ToInt(TestEnum.A5);
            TestEnum a5e = Enums.ToEnum<TestEnum>(a5);
            Assert.True(a5e == TestEnum.A5);

            TestFlagEnum f = TestFlagEnum.F0 | TestFlagEnum.F1;
            Assert.True(Bits.ContainsAny(f, TestFlagEnum.F1));
            Assert.False(Bits.ContainsAny(f, TestFlagEnum.F4));

            Bits.Add(ref f, TestFlagEnum.F4);
            Assert.True(Bits.ContainsAny(f, TestFlagEnum.F4));
            Assert.True(Bits.ContainsAll(f, TestFlagEnum.F4 | TestFlagEnum.F1));

            TestFlagEnum[] contents = new TestFlagEnum[3];
            int i = 0;
            foreach(var flag in Bits.Enumerate(f))
            {
                Debug.Log(flag);
                contents[i++] = flag;
            }

            Assert.AreEqual(TestFlagEnum.F0, contents[0]);
            Assert.AreEqual(TestFlagEnum.F1, contents[1]);
            Assert.AreEqual(TestFlagEnum.F4, contents[2]);
        }

        [Test]
        static public void BitSetTest32()
        {
            BitSet32 bitSet = new BitSet32();
            bitSet.Set(17);
            bitSet.Set(23);
            bitSet.Set(2);

            Assert.True(bitSet.IsSet(17));
            Assert.AreEqual(3, bitSet.Count);

            bitSet = ~bitSet;

            Assert.False(bitSet.IsSet(17));
            Assert.True(bitSet.IsSet(3));
        }

        [Test]
        static public void BitSetTest64()
        {
            BitSet64 bitSet = new BitSet64();
            bitSet.Set(17);
            bitSet.Set(53);

            Assert.True(bitSet.IsSet(17));
            Assert.AreEqual(2, bitSet.Count);

            bitSet = ~bitSet;

            Assert.False(bitSet.IsSet(17));
        }

        [Test]
        static public void BitSetTest128()
        {
            BitSet128 bitSet = new BitSet128();
            bitSet.Set(98);
            bitSet.Set(13);

            Assert.True(bitSet.IsSet(13));
            Assert.AreEqual(2, bitSet.Count);

            bitSet = ~bitSet;

            Assert.False(bitSet.IsSet(13));
        }

        [Test]
        static public void BitSetTest256()
        {
            BitSet256 bitSet = new BitSet256();
            bitSet.Set(98);
            bitSet.Set(13);
            bitSet.Set(99);
            bitSet.Set(99);
            bitSet.Set(199);
            bitSet.Set(213);

            Assert.True(bitSet.IsSet(213));
            Assert.True(bitSet.IsSet(99));
            Assert.AreEqual(5, bitSet.Count);

            bitSet = ~bitSet;

            Assert.False(bitSet.IsSet(13));
        }

        [Test]
        static public void BitSetTest512()
        {
            BitSet512 bitSet = new BitSet512();
            bitSet.Set(98);
            bitSet.Set(13);
            bitSet.Set(99);
            bitSet.Set(99);
            bitSet.Set(199);
            bitSet.Set(501);

            Assert.True(bitSet.IsSet(501));
            Assert.False(bitSet.IsSet(203));
            Assert.True(bitSet.IsSet(99));
            Assert.AreEqual(5, bitSet.Count);

            bitSet = ~bitSet;

            Assert.False(bitSet.IsSet(13));
        }

        [Test]
        static public void BitSetTestN()
        {
            BitSetN bitSet = new BitSetN(8192);
            bitSet.Set(98);
            bitSet.Set(13);
            bitSet.Set(99);
            bitSet.Set(99);
            bitSet.Set(199);
            bitSet.Set(501);
            bitSet.Set(8000);

            Assert.True(bitSet.IsSet(501));
            Assert.False(bitSet.IsSet(203));
            Assert.True(bitSet.IsSet(99));
            Assert.AreEqual(6, bitSet.Count);
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
        static public void RingBufferMoveTest()
        {
            RingBuffer<int> buffer = new RingBuffer<int>();

            buffer.PushBack(2);
            buffer.PushBack(4);
            buffer.PushBack(3);
            buffer.PushBack(7);

            buffer.MoveFrontToBack();

            Assert.AreEqual(buffer.PeekFront(), 4);
            Assert.AreEqual(buffer.PeekBack(), 2);

            buffer.MoveBackToFront();
            buffer.MoveBackToFront();

            Assert.AreEqual(buffer.PeekFront(), 7);
            Assert.AreEqual(buffer.PeekBack(), 3);

            buffer.MoveFrontToBackWhere((a, b) => a > b, 5);

            Assert.AreEqual(buffer.PeekFront(), 2);

            buffer.MoveBackToFrontWhere((a, b) => a > b, 2);

            Assert.AreEqual(buffer.PeekFront(), 4);
        }

        [Test]
        static public void RingBufferLinearizeTest()
        {
            RingBuffer<int> buffer = new RingBuffer<int>(6, RingBufferMode.Fixed);

            buffer.PushBack(2);
            buffer.PushBack(4);
            buffer.PushBack(3);
            buffer.PushBack(7);

            buffer.PopFront();
            buffer.PopFront();
            buffer.PopFront();

            buffer.PushBack(15);
            buffer.PushBack(13);
            buffer.PushBack(12);
            buffer.PushBack(10);

            buffer.Unpack(out int[] arr, out int offset, out int length);

            Assert.AreEqual(0, offset);
            Assert.AreEqual(5, length);
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
            for (int i = 0; i < iter; ++i)
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

        [Test]
        static public void LruCacheTest()
        {
            CacheCallbacks<int, string> config = new CacheCallbacks<int, string>()
            {
                Fetch = (k) => k.ToString()
            };
            int hitCount = 0, missCount = 0, evictCount = 0;
            LruCache<int, string> toStringCache = new LruCache<int, string>(8, config);
            toStringCache.OnHit += (k, op) => { hitCount++; Debug.LogFormat("hit '{0}' for operation {1}", k, op); };
            toStringCache.OnMiss += (k, op) => { missCount++; Debug.LogFormat("missed '{0}' for operation {1}", k, op); };
            toStringCache.OnEvict += (k, op) => { evictCount++; Debug.LogFormat("evicted '{0}' for operation {1}", k, op); };

            toStringCache.Read(4);
            Assert.AreEqual(1, missCount);

            toStringCache.Read(4);
            Assert.AreEqual(1, missCount);
            Assert.AreEqual(1, hitCount);

            Assert.AreEqual(missCount, toStringCache.Stats.MissCount);
            Assert.AreEqual(hitCount, toStringCache.Stats.HitCount);

            toStringCache.Read(16);
            toStringCache.Read(4);
            toStringCache.Read(8);
            toStringCache.Read(16);
            toStringCache.Read(7);
            toStringCache.Read(4);
            toStringCache.Read(3);
            toStringCache.Read(9);
            toStringCache.Read(4);
            toStringCache.Read(0);
            toStringCache.Read(1);
            toStringCache.Read(19);
            Assert.AreEqual(1, evictCount);
            Assert.False(toStringCache.Contains(8));
        }

        [Test]
        static public void CanMergeOffsetRanges32()
        {
            OffsetLengthU32[] ranges = new OffsetLengthU32[10];
            int length = 0;
            ranges[length++] = new OffsetLengthU32(4, 4);
            ranges[length++] = new OffsetLengthU32(7, 10);
            ranges[length++] = new OffsetLengthU32(12, 8);
            ranges[length++] = new OffsetLengthU32(14, 8);
            ranges[length++] = new OffsetLengthU32(32, 7);
            ranges[length++] = new OffsetLengthU32(45, 3);
            ranges[length++] = new OffsetLengthU32(46, 1);
            int newLength = OffsetLength.MergePreSorted(ranges, 0, length);
            Assert.AreEqual(3, newLength);
            Assert.AreEqual(4, ranges[0].Offset);
            Assert.AreEqual(22, ranges[0].End);
        }

        [Test]
        static public void CanMergeOffsetRanges16()
        {
            OffsetLength16[] ranges = new OffsetLength16[10];
            int length = 0;
            ranges[length++] = new OffsetLength16(4, 4);
            ranges[length++] = new OffsetLength16(7, 10);
            ranges[length++] = new OffsetLength16(12, 8);
            ranges[length++] = new OffsetLength16(14, 8);
            ranges[length++] = new OffsetLength16(32, 7);
            ranges[length++] = new OffsetLength16(45, 3);
            ranges[length++] = new OffsetLength16(46, 1);
            int newLength = OffsetLength.MergePreSorted(ranges, 0, length);
            Assert.AreEqual(3, newLength);
            Assert.AreEqual(4, ranges[0].Offset);
            Assert.AreEqual(22, ranges[0].End);
        }

        [Test]
        static public void CanUseCustomDefaultComparers()
        {
            HashSet<StringHash32> set = new HashSet<StringHash32>(CompareUtils.DefaultEquals<StringHash32>());
        }

        [Test]
        static public void CanUseLLTable()
        {
            LLTable table = new LLTable(8);
            LLIndexList list = LLIndexList.Empty;
            int first = table.PushBack(ref list);
            int second = table.PushBack(ref list);

            Assert.AreEqual(0, first);
            Assert.AreEqual(1, second);

            table.MoveToBack(ref list, first);

            int third = table.PushBack(ref list);

            table.Remove(ref list, second);

            Assert.AreEqual(2, list.Length);

            third = table.PushBack(ref list);
            int fourth = table.PushBack(ref list);

            Assert.AreEqual(4, list.Length);
            table.MoveToFront(ref list, third);

            table.Linearize(ref list);

            table.OptimizeFreeList();

            table.PushBack(ref list);
            table.PushBack(ref list);
            table.PushBack(ref list);
            table.PushBack(ref list);
            table.PushBack(ref list);
            table.PushBack(ref list);

            table.PopBack(ref list);

            table.PopFront(ref list);

            table.Validate(list);

            table.OptimizeFreeList();
        }

        [Test]
        static public void CanUseTaggedLLTable()
        {
            LLTable<byte> table = new LLTable<byte>(8);
            LLIndexList list = LLIndexList.Empty;
            int first = table.PushBack(ref list, 2);
            int second = table.PushBack(ref list, 4);

            Assert.AreEqual(0, first);
            Assert.AreEqual(1, second);

            table.MoveToBack(ref list, first);

            int third = table.PushBack(ref list, 4);

            table.Remove(ref list, second);

            Assert.AreEqual(2, list.Length);

            third = table.PushBack(ref list, 4);
            int fourth = table.PushBack(ref list, 4);

            Assert.AreEqual(4, list.Length);
            table.MoveToFront(ref list, third);

            table.Linearize(ref list);

            table.OptimizeFreeList();

            table.PushBack(ref list, 0);
            table.PushBack(ref list, 16);
            table.PushBack(ref list, 12);
            table.PushBack(ref list, 99);
            table.PushBack(ref list, 1);
            table.PushBack(ref list, 14);

            var popped = table.PopBack(ref list);
            Assert.AreEqual(popped.Tag, 14);

            table.PopFront(ref list);

            table.Validate(list);

            bool removed = table.RemoveTag(ref list, 4);
            Assert.True(removed);

            table.Clear(ref list);
            Assert.AreEqual(0, list.Length);

            table.OptimizeFreeList();
        }

        [Test]
        static public void UniqueId16Tests()
        {
            UniqueIdAllocator16 alloc = new UniqueIdAllocator16(64);
            Assert.False(alloc.IsValid(UniqueId16.Invalid));

            var first = alloc.Alloc();
            Assert.AreEqual(0, first.Index);
            Assert.AreEqual(1, first.Version);
            Assert.True(alloc.IsValid(first));

            Assert.False(alloc.IsValid(new UniqueId16(2, 0)));

            alloc.Alloc();
            alloc.Alloc();

            for (int i = 0; i < 64; i++)
            {
                alloc.Alloc();
            }

            alloc.Free(first);

            Assert.False(alloc.IsValid(first));

            var next = alloc.Alloc();
            Assert.AreEqual(0, next.Index);
            Assert.AreNotEqual(first, next);

            UniqueIdMap16<float> map = new UniqueIdMap16<float>(32);
            var mapped = map.Add(16);
            Assert.True(map.Contains(mapped));

            Assert.AreEqual(0, map[UniqueId16.Invalid]);

            map.Remove(mapped);

            Assert.False(map.Contains(mapped));
        }

        [Test]
        static public void UniqueId32Tests()
        {
            UniqueIdAllocator32 alloc = new UniqueIdAllocator32(64);
            Assert.False(alloc.IsValid(UniqueId32.Invalid));

            var first = alloc.Alloc();
            Assert.AreEqual(0, first.Index);
            Assert.AreEqual(1, first.Version);
            Assert.True(alloc.IsValid(first));

            Assert.False(alloc.IsValid(new UniqueId32(2, 0)));

            alloc.Alloc();
            alloc.Alloc();

            for (int i = 0; i < 64; i++)
            {
                alloc.Alloc();
            }

            alloc.Free(first);

            Assert.False(alloc.IsValid(first));

            var next = alloc.Alloc();
            Assert.AreEqual(0, next.Index);
            Assert.AreNotEqual(first, next);
        }

        [TypeIndexCapacity(1024)]
        private class ReflectA { }

        private class ReflectB : ReflectA { }
        private class ReflectC : ReflectB { }

        [Test]
        static public void TypeIndexWithCapacityTest()
        {
            int b = TypeIndex<ReflectA>.Get<ReflectB>();
            int c = TypeIndex<ReflectB>.Get<ReflectC>();
            int a = TypeIndex<ReflectA>.Get<ReflectA>();

            Assert.AreEqual(0, a);
        }

        [Test]
        static public void TypeIndexTest()
        {
            int b = TypeIndex<ReflectB>.Get<ReflectB>();
            int c = TypeIndex<ReflectB>.Get<ReflectC>();

            Assert.AreEqual(0, b);

            foreach(var idx in TypeIndex<ReflectA>.GetAll<ReflectC>()) {
                Debug.Log(TypeIndex<ReflectA>.Type(idx).Name);
            }
        }

        public class SomeImportantAttribute : Attribute
        {
            public readonly int Value;

            public SomeImportantAttribute(int val)
            {
                Value = val;
            }
        }

        public struct UntaggedStruct
        {

        }

        [SomeImportant(-5)]
        public struct TaggedStructA
        {

        }

        [SomeImportant(5)]
        public struct TaggedStructB
        {

        }

        [Test]
        static public void CanCacheAttributeMappedCache()
        {
            AttributeCache<SomeImportantAttribute, int> cache = new AttributeCache<SomeImportantAttribute, int>((v, t) => v.Value, 8);

            int untagged = cache.Get<UntaggedStruct>();
            Assert.AreEqual(0, untagged);
            Assert.AreEqual(1, cache.Count);

            int a = cache.Get<TaggedStructA>();
            int b = cache.Get<TaggedStructB>();

            Assert.AreEqual(-5, a);
            Assert.AreEqual(5, b);
            Assert.AreEqual(3, cache.Count);
        }

        [Test]
        static public void CanCacheAttributeCache()
        {
            AttributeCache<SomeImportantAttribute> cache = new AttributeCache<SomeImportantAttribute>(8);

            SomeImportantAttribute untagged = cache.Get<UntaggedStruct>();
            Assert.AreEqual(null, untagged);
            Assert.AreEqual(1, cache.Count);

            SomeImportantAttribute a = cache.Get<TaggedStructA>();
            SomeImportantAttribute b = cache.Get<TaggedStructB>();

            Assert.AreEqual(-5, a.Value);
            Assert.AreEqual(5, b.Value);
            Assert.AreEqual(3, cache.Count);
        }

        [Test]
        static public void CanCreateAttributeSet()
        {
            SerializedAttributeSet attrSet = new SerializedAttributeSet();
            attrSet.Write<PreserveAttribute>(Reflect.FindAllUserAssemblies());

            using (BeauUtil.Debugger.Log.DisableMsgStackTrace())
            {
                foreach (var binding in attrSet.Read<PreserveAttribute>(Reflect.FindAllUserAssemblies()))
                {
                    Assert.NotNull(binding.Info);
                    Debug.LogFormat("Attribute {0} on {1}::{2}", binding.Attribute.GetType(), binding.Info.DeclaringType?.FullName, binding.Info.Name);
                }

                attrSet.Write<SerializableAttribute>(Reflect.FindAllAssemblies(Reflect.AssemblyType.Unity, 0));

                string setJSON = JsonUtility.ToJson(attrSet);
                System.IO.File.WriteAllText("Temp/TestBindingExport.json", setJSON);

                foreach (var binding in attrSet.Read<SerializableAttribute>(Reflect.FindAllAssemblies(Reflect.AssemblyType.Unity, 0)))
                {
                    Assert.NotNull(binding.Info);
                    //Debug.LogFormat("Attribute {0} on {1}::{2}", binding.Attribute.GetType(), binding.Info.DeclaringType?.FullName, binding.Info.Name);
                }

                attrSet.Write<UnityEngine.RuntimeInitializeOnLoadMethodAttribute>(Reflect.FindAllAssemblies(Reflect.AssemblyType.Unity, 0));

                foreach (var binding in attrSet.Read<RuntimeInitializeOnLoadMethodAttribute>(Reflect.FindAllAssemblies(Reflect.AssemblyType.Unity, 0)))
                {
                    Assert.NotNull(binding.Info);
                    Debug.LogFormat("Attribute {0} on {1}::{2}", binding.Attribute.GetType(), binding.Info.DeclaringType?.FullName, binding.Info.Name);
                }

                setJSON = JsonUtility.ToJson(attrSet);
                System.IO.File.WriteAllText("Temp/TestBindingExport2.json", setJSON);

                Type nativeType = Type.GetType("UnityEngine.Bindings.NativeHeaderAttribute, UnityEngine.SharedInternalsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                attrSet.Write(Reflect.FindAllAssemblies(Reflect.AssemblyType.Unity, 0), nativeType);

                foreach (var binding in attrSet.Read(Reflect.FindAllAssemblies(Reflect.AssemblyType.Unity, 0), nativeType))
                {
                    Assert.NotNull(binding.Info);
                    Debug.LogFormat("Attribute {0} on {1}::{2}", binding.Attribute.GetType(), binding.Info.DeclaringType?.FullName, binding.Info.Name);
                }

                setJSON = JsonUtility.ToJson(attrSet);
                System.IO.File.WriteAllText("Temp/TestBindingExport3.json", setJSON);
            }
        }

        [Test]
        static public void TestBitRotates()
        {
            uint bits = 0;
            Bits.Add(ref bits, 31);
            Bits.Add(ref bits, 0);

            Assert.AreEqual(2, Bits.Count(bits));

            uint rotatedbits = Bits.RotateLeft(bits, 1);

            Assert.AreEqual(3, rotatedbits);

            uint shouldBeOriginalBits = Bits.RotateRight(rotatedbits, 1);

            Assert.AreEqual(bits, shouldBeOriginalBits);
        }

        [Test]
        static public void CanUseMapUtils()
        {
            foreach(var field in typeof(Dictionary<StringHash32, int>).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)) {
                Debug.Log(field);
            }

            Dictionary<StringHash32, int> dict = MapUtils.Create<StringHash32, int>(90);

            int capacity = dict.GetCapacity();
            Assert.AreEqual(107, capacity);

            dict.EnsureCapacity(256);

            Assert.AreEqual(293, dict.GetCapacity());
        }

        [Test]
        static public void CanUseSetUtils() {
            foreach (var field in typeof(HashSet<StringHash32>).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)) {
                Debug.Log(field);
            }

            HashSet<StringHash32> set = SetUtils.Create<StringHash32>(90);

            int capacity = set.GetCapacity();
            Assert.AreEqual(107, capacity);

            set.EnsureCapacity(256);

            Assert.AreEqual(293, set.GetCapacity());
        }
    }
}