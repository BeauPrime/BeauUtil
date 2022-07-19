/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    18 July 2022
 * 
 * File:    Batch.cs
 * Purpose: Batch processing utilities.
 */

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Collections;

namespace BeauUtil {
    /// <summary>
    /// Batch processing helpers.
    /// </summary>
    static public class Batch {

        #region Sorting

        /// <summary>
        /// Sorts a list by batch id.
        /// </summary>
        static public void Sort<T>(List<T> inObjects) where T : IBatchId
        {
            inObjects.Sort(BatchIdSorter<T>.Instance);
        }

        /// <summary>
        /// Sorts a list by batch id.
        /// </summary>
        static public void Sort<T>(RingBuffer<T> inObjects) where T : IBatchId {
            inObjects.Sort(BatchIdSorter<T>.Instance);
        }

        /// <summary>
        /// Sorts an array by batch id.
        /// </summary>
        static public void Sort<T>(T[] inObjects) where T : IBatchId
        {
            Array.Sort(inObjects, BatchIdSorter<T>.Instance);
        }

        /// <summary>
        /// Sorts an array range by batch id.
        /// </summary>
        static public void Sort<T>(T[] inObjects, int inStartIdx, int inLength) where T : IBatchId
        {
            Array.Sort(inObjects, inStartIdx, inLength, BatchIdSorter<T>.Instance);
        }

        #if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Sorts a buffer by batch id.
        /// </summary>
        static public unsafe void Sort<T>(T* inObjects, int inLength) where T : unmanaged, IBatchId
        {
            Unsafe.Quicksort(inObjects, inLength, BatchIdSorter<T>.Instance);
        }

        #endif // UNMANAGED_CONSTRAINT

        /// <summary>
        /// Returns the IComparer to sort the given type by its batch id.
        /// </summary>
        static public IComparer<T> Comparer<T>() where T : IBatchId
        {
            return BatchIdSorter<T>.Instance;
        }

        /// <summary>
        /// Returns the Comparison to sort the given type by its batch id.
        /// </summary>
        static public Comparison<T> Comparison<T>() where T : IBatchId
        {
            return BatchIdSorter<T>.AsFunc;
        }

        private class BatchIdSorter<T> : IComparer<T> where T : IBatchId
        {
            static public readonly BatchIdSorter<T> Instance = new BatchIdSorter<T>();
            static public readonly Comparison<T> AsFunc = (x, y) => {
                return x.BatchId - y.BatchId;
            };

            public int Compare(T x, T y)
            {
                return x.BatchId - y.BatchId;
            }
        }

        #endregion // Sorting

        #region Process

        /// <summary>
        /// Delegate for processing a batch of items.
        /// </summary>
        public delegate void ProcessDelegate<TItem>(ListSlice<TItem> inItems, int inBatchId) where TItem : IBatchId;

        /// <summary>
        /// Delegate for processing a batch of items.
        /// </summary>
        public delegate void ProcessDelegate<TItem, TArgs>(ListSlice<TItem> inItems, int inBatchId, TArgs inArgs) where TItem : IBatchId where TArgs : struct;

        #if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Delegate for processing a batch of items.
        /// </summary>
        public unsafe delegate void UnsafeProcessDelegate<TItem>(TItem* inItems, int inItemCount, int inBatchId) where TItem : unmanaged, IBatchId;

        /// <summary>
        /// Delegate for processing a batch of items.
        /// </summary>
        public unsafe delegate void UnsafeProcessDelegate<TItem, TArgs>(TItem* inItems, int inItemCount, int inBatchId, TArgs inArgs) where TItem : unmanaged, IBatchId where TArgs : struct;

        #endif // UNMANAGED_CONSTRAINT

        /// <summary>
        /// Batch processor.
        /// </summary>
        public struct Processor<TItem> : IDisposable, IEnumerator
            where TItem : IBatchId
        {
            public int BatchSize;
            public ProcessDelegate<TItem> Process;

            public ListSlice<TItem> Items;
            public int CurrentIndex;

            public Processor(ProcessDelegate<TItem> inDelegate, int inBatchSize)
            {
                Process = inDelegate;
                BatchSize = inBatchSize;
                Items = default;
                CurrentIndex = -1;
            }

            object IEnumerator.Current { get { return null; } }

            /// <summary>
            /// Prepares to process the set of items.
            /// </summary>
            public void Prep(ListSlice<TItem> inItems)
            {
                Items = inItems;
                CurrentIndex = 0;
            }

            /// <summary>
            /// Clears the processor.
            /// </summary>
            public void Clear()
            {
                Items = default(ListSlice<TItem>);
                CurrentIndex = -1;
            }

            /// <summary>
            /// Processes all batches.
            /// </summary>
            public void ProcessAll()
            {
                while(MoveNext());
            }

            public void Dispose()
            {
                Clear();
            }

            /// <summary>
            /// Processes one batch.
            /// </summary>
            public bool MoveNext()
            {
                if (CurrentIndex >= Items.Length)
                {
                    return false;
                }

                int start = CurrentIndex;
                int batchId = Items[start].BatchId;
                int idx = start + 1;
                int maxEnd = BatchSize <= 0 ? Items.Length : Math.Min(start + BatchSize, Items.Length);

                for(; idx < maxEnd && Items[idx].BatchId == batchId; idx++);

                CurrentIndex = idx;
                Process(new ListSlice<TItem>(Items, start, idx - start), batchId);
                return true;
            }

            void IEnumerator.Reset()
            {
                Clear();
            }
        }

        /// <summary>
        /// Batch processor.
        /// </summary>
        public struct Processor<TItem, TArgs> : IDisposable, IEnumerator
            where TItem : IBatchId
            where TArgs : struct
        {
            public int BatchSize;
            public ProcessDelegate<TItem, TArgs> Process;

            public ListSlice<TItem> Items;
            public int CurrentIndex;
            public TArgs Arguments;

            public Processor(ProcessDelegate<TItem, TArgs> inDelegate, int inBatchSize)
            {
                Process = inDelegate;
                BatchSize = inBatchSize;
                Items = default;
                CurrentIndex = -1;
                Arguments = default(TArgs);
            }

            object IEnumerator.Current { get { return null; } }

            /// <summary>
            /// Prepares to process the set of items.
            /// </summary>
            public void Prep(ListSlice<TItem> inItems, TArgs inArgs)
            {
                Items = inItems;
                CurrentIndex = 0;
                Arguments = inArgs;
            }

            /// <summary>
            /// Clears the processor.
            /// </summary>
            public void Clear()
            {
                Items = default(ListSlice<TItem>);
                CurrentIndex = -1;
                Arguments = default(TArgs);
            }

            /// <summary>
            /// Processes all batches.
            /// </summary>
            public void ProcessAll()
            {
                while(MoveNext());
            }

            public void Dispose()
            {
                Clear();
            }

            /// <summary>
            /// Processes one batch.
            /// </summary>
            public bool MoveNext()
            {
                if (CurrentIndex >= Items.Length)
                {
                    return false;
                }

                int start = CurrentIndex;
                int batchId = Items[start].BatchId;
                int idx = start + 1;
                int maxEnd = BatchSize <= 0 ? Items.Length : Math.Min(start + BatchSize, Items.Length);

                for(; idx < maxEnd && Items[idx].BatchId == batchId; idx++);

                CurrentIndex = idx;
                Process(new ListSlice<TItem>(Items, start, idx - start), batchId, Arguments);
                return true;
            }

            void IEnumerator.Reset()
            {
                Clear();
            }
        }

        #if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Batch processor.
        /// </summary>
        public unsafe struct UnsafeProcessor<TItem> : IDisposable, IEnumerator
            where TItem : unmanaged, IBatchId
        {
            public int BatchSize;
            public UnsafeProcessDelegate<TItem> Process;

            public TItem* Items;
            public int Count;
            public int CurrentIndex;

            public UnsafeProcessor(UnsafeProcessDelegate<TItem> inDelegate, int inBatchSize)
            {
                Process = inDelegate;
                BatchSize = inBatchSize;
                Items = default;
                Count = 0;
                CurrentIndex = -1;
            }

            object IEnumerator.Current { get { return null; } }

            /// <summary>
            /// Prepares to process the set of items.
            /// </summary>
            public void Prep(TItem* inItems, int inCount)
            {
                Items = inItems;
                Count = inCount;
                CurrentIndex = 0;
            }

            /// <summary>
            /// Clears the processor.
            /// </summary>
            public void Clear()
            {
                Items = null;
                Count = 0;
                CurrentIndex = -1;
            }

            /// <summary>
            /// Processes all batches.
            /// </summary>
            public void ProcessAll()
            {
                while(MoveNext());
            }

            public void Dispose()
            {
                Clear();
            }

            /// <summary>
            /// Processes one batch.
            /// </summary>
            public bool MoveNext()
            {
                if (CurrentIndex >= Count)
                {
                    return false;
                }

                int start = CurrentIndex;
                int batchId = Items[start].BatchId;
                int idx = start + 1;
                int maxEnd = BatchSize <= 0 ? Count : Math.Min(start + BatchSize, Count);

                for(; idx < maxEnd && Items[idx].BatchId == batchId; idx++);

                CurrentIndex = idx;
                Process(&Items[start], idx - start, batchId);
                return true;
            }

            void IEnumerator.Reset()
            {
                Clear();
            }
        }

        /// <summary>
        /// Batch processor.
        /// </summary>
        public unsafe struct UnsafeProcessor<TItem, TArgs> : IDisposable, IEnumerator
            where TItem : unmanaged, IBatchId
            where TArgs : struct
        {
            public int BatchSize;
            public UnsafeProcessDelegate<TItem, TArgs> Process;

            public TItem* Items;
            public int Count;
            public int CurrentIndex;
            public TArgs Arguments;

            public UnsafeProcessor(UnsafeProcessDelegate<TItem, TArgs> inDelegate, int inBatchSize)
            {
                Process = inDelegate;
                BatchSize = inBatchSize;
                Items = default;
                Count = 0;
                CurrentIndex = -1;
                Arguments = default(TArgs);
            }

            object IEnumerator.Current { get { return null; } }

            /// <summary>
            /// Prepares to process the set of items.
            /// </summary>
            public void Prep(TItem* inItems, int inCount, TArgs inArgs)
            {
                Items = inItems;
                Count = inCount;
                CurrentIndex = 0;
                Arguments = inArgs;
            }

            /// <summary>
            /// Clears the processor.
            /// </summary>
            public void Clear()
            {
                Items = null;
                Count = 0;
                CurrentIndex = -1;
                Arguments = default(TArgs);
            }

            /// <summary>
            /// Processes all batches.
            /// </summary>
            public void ProcessAll()
            {
                while(MoveNext());
            }

            public void Dispose()
            {
                Clear();
            }

            /// <summary>
            /// Processes one batch.
            /// </summary>
            public bool MoveNext()
            {
                if (CurrentIndex >= Count)
                {
                    return false;
                }

                int start = CurrentIndex;
                int batchId = Items[start].BatchId;
                int idx = start + 1;
                int maxEnd = BatchSize <= 0 ? Count : Math.Min(start + BatchSize, Count);

                for(; idx < maxEnd && Items[idx].BatchId == batchId; idx++);

                CurrentIndex = idx;
                Process(&Items[start], idx - start, batchId, Arguments);
                return true;
            }

            void IEnumerator.Reset()
            {
                Clear();
            }
        }
        
        #endif // UNMANAGED_CONSTRAINT

        #endregion // Process
    }

    /// <summary>
    /// Interface for an object specifying a batch id.
    /// </summary>
    public interface IBatchId
    {
        /// <summary>
        /// Batch identifier.
        /// </summary>
        int BatchId { get; }
    }
}