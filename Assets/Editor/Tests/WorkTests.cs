using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace BeauUtil.UnitTests
{
    static public class WorkTests
    {
        [Test]
        static public void BatchTest()
        {
            int lastBatchId = -1;
            int[] sums = new int[6];
            Batch.ProcessDelegate<BatchItem, int> processDelegate = (items, id, args) =>
            {
                Assert.GreaterOrEqual(id, lastBatchId);
                lastBatchId = id;
                Debug.LogFormat("Batch {0} with {1} items", id, items.Length);
                for (int i = 0; i < items.Length; i++)
                {
                    Debug.LogFormat("Batch {0}, item {1}, arg {2}", id, items[i].Id, args);
                    sums[id] += items[i].Id;
                }
            };
            Batch.Processor<BatchItem, int> processor = new Batch.Processor<BatchItem, int>(processDelegate, 4);
            RingBuffer<BatchItem> allItems = new RingBuffer<BatchItem>(32);
            allItems.PushBack(new BatchItem(1, 1));
            allItems.PushBack(new BatchItem(2, 3));
            allItems.PushBack(new BatchItem(3, 2));
            allItems.PushBack(new BatchItem(4, 3));
            allItems.PushBack(new BatchItem(5, 2));
            allItems.PushBack(new BatchItem(6, 4));
            allItems.PushBack(new BatchItem(7, 4));
            allItems.PushBack(new BatchItem(8, 5));
            allItems.PushBack(new BatchItem(9, 0));
            allItems.PushBack(new BatchItem(10, 2));
            allItems.PushBack(new BatchItem(11, 3));
            allItems.PushBack(new BatchItem(12, 2));
            allItems.PushBack(new BatchItem(13, 1));
            allItems.PushBack(new BatchItem(14, 5));
            allItems.PushBack(new BatchItem(15, 3));
            allItems.PushBack(new BatchItem(16, 4));
            allItems.PushBack(new BatchItem(17, 0));
            allItems.PushBack(new BatchItem(18, 2));

            Batch.Sort(allItems);
            processor.Prep(allItems, 5);
            processor.ProcessAll();

            Debug.LogFormat("Sums: {0} {1} {2} {3} {4} {5}", sums[0], sums[1], sums[2], sums[3], sums[4], sums[5]);

            Assert.True(sums[0] == 26, "Sum of 0 index is incorrect");
            Assert.True(sums[1] == 14, "Sum of 1 index is incorrect");
            Assert.True(sums[2] == 48, "Sum of 2 index is incorrect");
            Assert.True(sums[3] == 32, "Sum of 3 index is incorrect");
            Assert.True(sums[4] == 29, "Sum of 4 index is incorrect");
            Assert.True(sums[5] == 22, "Sum of 5 index is incorrect");
        }

        private struct BatchItem : IBatchId
        {
            public int Id;
            public int BatchId { get; private set; }

            public BatchItem(int inId, int inBatchId)
            {
                Id = inId;
                BatchId = inBatchId;
            }
        }

        [Test]
        static public void CanRunProcessorOnSimpleElements()
        {
            RingBuffer<int> ints = new RingBuffer<int>(8, RingBufferMode.Expand);
            for (int i = 0; i < ints.Capacity; i++)
            {
                ints.PushBack(i);
            }

            int total = 0;
            WorkSlicer.Step(ints, (i) => total += i);

            Assert.AreEqual(0, total);

            WorkSlicer.Step(ints, (i) => total += i);
            WorkSlicer.Step(ints, (i) => total += i);

            Assert.AreEqual(5, ints.Count);
            Assert.AreEqual(3, total);

            WorkSlicer.Flush(ints, (i) => total += i);
            Assert.AreEqual(28, total);
            Assert.AreEqual(0, ints.Count);

            for (int i = 0; i < ints.Capacity; i++)
            {
                ints.PushBack(1 + i);
            }

            WorkSlicer.Result result = WorkSlicer.TimeSliced(ints, (i) =>
            {
                Thread.Sleep(i);
            }, 50);

            Debug.LogFormat("remaining {0}", ints.Count);
            Assert.IsTrue(ints.Count > 0);
            Assert.AreEqual(WorkSlicer.Result.Processed, result);
        }

        [Test]
        static public void CanRunProcessorOnEnumeratedElements()
        {
            RingBuffer<int> ints = new RingBuffer<int>(8, RingBufferMode.Expand);
            for (int i = 0; i < ints.Capacity; i++)
            {
                ints.PushBack(1 + i);
            }

            int total = 0;
            IEnumerator<WorkSlicer.Result?> Op(int i)
            {
                yield return null;
                yield return null;
                total += i;
            }

            WorkSlicer.EnumeratedState state = new WorkSlicer.EnumeratedState();

            WorkSlicer.Step(ints, Op, ref state);

            Assert.AreEqual(0, total);

            WorkSlicer.Step(ints, Op, ref state);
            WorkSlicer.Step(ints, Op, ref state);

            Assert.AreEqual(7, ints.Count);
            Assert.AreEqual(0, total);

            WorkSlicer.Step(ints, Op, ref state);
            Assert.AreEqual(1, total);

            WorkSlicer.Flush(ints, Op, ref state);
            Assert.AreEqual(36, total);
            Assert.AreEqual(0, ints.Count);

            for (int i = 0; i < ints.Capacity; i++)
            {
                ints.PushBack(1 + i);
            }

            WorkSlicer.Result result = WorkSlicer.TimeSliced(ints, Op, ref state, 10);
            Assert.AreEqual(WorkSlicer.Result.OutOfData, result);
        }

        [Test]
        static public void CanRunProcessorOnSingleFunction()
        {
            int total = 0;
            bool Op1()
            {
                total++;
                return total < 50;
            }

            WorkSlicer.Result Op2() 
            {
                total++;
                return total < 50 ? WorkSlicer.Result.Processed : WorkSlicer.Result.OutOfData;
            }

            WorkSlicer.Result result = WorkSlicer.TimeSliced(Op1, 50);

            Assert.AreEqual(WorkSlicer.Result.OutOfData, result);
        }

        [Test]
        static public void CanRunProcessorOnBigEnumerator()
        {
            int total = 0;
            IEnumerator<WorkSlicer.Result?> Op()
            {
                while (total < 20)
                {
                    total++;
                    yield return null;
                }

                yield return WorkSlicer.Result.HaltForFrame;

                while (total < 50)
                {
                    total++;
                    yield return null;
                }
            }

            var enumerator = Op();

            WorkSlicer.Result result = WorkSlicer.TimeSliced(ref enumerator, 5);

            Assert.AreEqual(WorkSlicer.Result.HaltForFrame, result);

            result = WorkSlicer.TimeSliced(ref enumerator, 5);

            Assert.AreEqual(WorkSlicer.Result.OutOfData, result);
        }
    }
}