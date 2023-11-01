/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    31 Oct 2023
 * 
 * File:    WorkSlice.cs
 * Purpose: Data processing utilities.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Data processing utilities.
    /// </summary>
    static public class WorkSlicer
    {
        /// <summary>
        /// Delegate for processing a piece of data.
        /// </summary>
        public delegate void ElementOperation<T>(T inData);

        /// <summary>
        /// Delegate for processing a piece of data.
        /// Can optionally return an IEnumerator to split process across multiple operations.
        /// </summary>
        public delegate IEnumerator<Result?> EnumeratedElementOperation<T>(T inData);

        /// <summary>
        /// Delegate for running an operation.
        /// Return StepResult.OutOfData if no more processing is required.
        /// </summary>
        public delegate Result StepOperation();

        /// <summary>
        /// Delegate for running an operation.
        /// Return false if no more processing is required.
        /// </summary>
        public delegate bool Operation();

        /// <summary>
        /// Result of a step.
        /// </summary>
        public enum Result
        {
            Processed,
            OutOfData,
            Terminate,
            HaltForFrame
        }

        #region Step

        /// <summary>
        /// Processes a single item from the given set.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Result Step<T>(RingBuffer<T> inObjects, ElementOperation<T> inProcessor)
        {
            if (inObjects.TryPopFront(out T obj))
            {
                inProcessor(obj);
                return Result.Processed;
            }

            return Result.OutOfData;
        }

        /// <summary>
        /// Processes a single item from the given set.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Result Step<T>(RingBuffer<T> inObjects, EnumeratedElementOperation<T> inProcessor, ref EnumeratedState ioState)
        {
            if (ioState.Sequence != null)
            {
                if (!ioState.Sequence.MoveNext())
                {
                    ioState.Clear();
                    return Result.Processed;
                }
                else
                {
                    return ioState.Sequence.Current.GetValueOrDefault(Result.Processed);
                }
            }

            if (inObjects.TryPopFront(out T obj))
            {
                ioState.Sequence = inProcessor(obj);
                return Result.Processed;
            }

            return Result.OutOfData;
        }

        /// <summary>
        /// Processes a single item from the given set.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Result Step<T>(Queue<T> inObjects, ElementOperation<T> inProcessor)
        {
            if (inObjects.TryDequeue(out T obj))
            {
                inProcessor(obj);
                return Result.Processed;
            }

            return Result.OutOfData;
        }

        /// <summary>
        /// Processes a single item from the given set.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Result Step<T>(Queue<T> inObjects, EnumeratedElementOperation<T> inProcessor, ref EnumeratedState ioState)
        {
            if (ioState.Sequence != null)
            {
                if (!ioState.Sequence.MoveNext())
                {
                    ioState.Clear();
                    return Result.Processed;
                }
                else
                {
                    return ioState.Sequence.Current.GetValueOrDefault(Result.Processed);
                }
            }

            if (inObjects.TryDequeue(out T obj))
            {
                ioState.Sequence = inProcessor(obj);
                return Result.Processed;
            }

            return Result.OutOfData;
        }

        /// <summary>
        /// Processes a single step of the given enumerator.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Result Step(IEnumerator<Result?> ioEnumerator)
        {
            return Step(ref ioEnumerator);
        }

        /// <summary>
        /// Processes a single step of the given enumerator.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Result Step(ref IEnumerator<Result?> ioEnumerator)
        {
            if (!ioEnumerator.MoveNext())
            {
                (ioEnumerator as IDisposable)?.Dispose();
                ioEnumerator = null;
                return Result.OutOfData;
            }
            else
            {
                return ioEnumerator.Current.GetValueOrDefault(Result.Processed);
            }
        }

        #endregion // Step

        #region Flush

        /// <summary>
        /// Processes all items from the given set.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Result Flush<T>(RingBuffer<T> inObjects, ElementOperation<T> inProcessor)
        {
            while (inObjects.TryPopFront(out T obj))
            {
                inProcessor(obj);
            }
            return Result.OutOfData;
        }

        /// <summary>
        /// Processes all items from the given set.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Result Flush<T>(RingBuffer<T> inObjects, EnumeratedElementOperation<T> inProcessor, ref EnumeratedState ioState)
        {
            Result result;
            while ((result = Step(inObjects, inProcessor, ref ioState)) == Result.Processed)
                ;
            return result;
        }

        /// <summary>
        /// Processes all items from the given set.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Result Flush<T>(Queue<T> inObjects, ElementOperation<T> inProcessor)
        {
            while (inObjects.TryDequeue(out T obj))
            {
                inProcessor(obj);
            }
            return Result.OutOfData;
        }

        /// <summary>
        /// Processes all items from the given set.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Result Flush<T>(Queue<T> inObjects, EnumeratedElementOperation<T> inProcessor, ref EnumeratedState ioState)
        {
            Result result;
            while ((result = Step(inObjects, inProcessor, ref ioState)) == Result.Processed)
                ;
            return result;
        }

        #endregion // Flush

        #region Time Sliced

        /// <summary>
        /// Processes items from the given set until either empty or given time limit is exceeded.
        /// </summary>
        static public Result TimeSliced<T>(RingBuffer<T> inObjects, ElementOperation<T> inProcessor, float inTimeMS)
        {
            long endTS = Stopwatch.GetTimestamp() + (long) (inTimeMS * Stopwatch.Frequency / 1000);
            Result stepped;
            do
            {
                stepped = Step(inObjects, inProcessor);
            } while (stepped == Result.Processed && Stopwatch.GetTimestamp() < endTS);
            return stepped;
        }

        /// <summary>
        /// Processes items from the given set until either empty or given time limit is exceeded.
        /// </summary>
        static public Result TimeSliced<T>(RingBuffer<T> inObjects, EnumeratedElementOperation<T> inProcessor, ref EnumeratedState ioState, float inTimeMS)
        {
            long endTS = Stopwatch.GetTimestamp() + (long) (inTimeMS * Stopwatch.Frequency / 1000);
            Result stepped;
            do
            {
                stepped = Step(inObjects, inProcessor, ref ioState);
            } while (stepped == Result.Processed && Stopwatch.GetTimestamp() < endTS);
            return stepped;
        }

        /// <summary>
        /// Processes items from the given set until either empty or given time limit is exceeded.
        /// </summary>
        static public Result TimeSliced<T>(Queue<T> inObjects, ElementOperation<T> inProcessor, float inTimeMS)
        {
            long endTS = Stopwatch.GetTimestamp() + (long) (inTimeMS * Stopwatch.Frequency / 1000);
            Result stepped;
            do
            {
                stepped = Step(inObjects, inProcessor);
            } while (stepped == Result.Processed && Stopwatch.GetTimestamp() < endTS);
            return stepped;
        }

        /// <summary>
        /// Processes items from the given set until either empty or given time limit is exceeded.
        /// </summary>
        static public Result TimeSliced<T>(Queue<T> inObjects, EnumeratedElementOperation<T> inProcessor, ref EnumeratedState ioState, float inTimeMS)
        {
            long endTS = Stopwatch.GetTimestamp() + (long) (inTimeMS * Stopwatch.Frequency / 1000);
            Result stepped;
            do
            {
                stepped = Step(inObjects, inProcessor, ref ioState);
            } while (stepped == Result.Processed && Stopwatch.GetTimestamp() < endTS);
            return stepped;
        }

        /// <summary>
        /// Processes the given function until it returns false or the given time limit is exceeded.
        /// </summary>
        static public Result TimeSliced(StepOperation inOperation, float inTimeMS)
        {
            long endTS = Stopwatch.GetTimestamp() + (long) (inTimeMS * Stopwatch.Frequency / 1000);
            Result stepped;
            do
            {
                stepped = inOperation();
            } while (stepped == Result.Processed && Stopwatch.GetTimestamp() < endTS);
            return stepped;
        }

        /// <summary>
        /// Processes the given function until it returns false or the given time limit is exceeded.
        /// </summary>
        static public Result TimeSliced(Operation inOperation, float inTimeMS)
        {
            long endTS = Stopwatch.GetTimestamp() + (long) (inTimeMS * Stopwatch.Frequency / 1000);
            bool stepped;
            do
            {
                stepped = inOperation();
            } while (stepped && Stopwatch.GetTimestamp() < endTS);
            return stepped ? Result.Processed : Result.OutOfData;
        }

        /// <summary>
        /// Processes the given enumerator until it is completed or the given time limit is exceeded.
        /// </summary>
        static public Result TimeSliced(IEnumerator<Result?> inEnumerator, float inTimeMS)
        {
            return TimeSliced(ref inEnumerator, inTimeMS);
        }

        /// <summary>
        /// Processes the given enumerator until it is completed or the given time limit is exceeded.
        /// </summary>
        static public Result TimeSliced(ref IEnumerator<Result?> ioEnumerator, float inTimeMS)
        {
            if (ioEnumerator == null)
            {
                return Result.OutOfData;
            }

            long endTS = Stopwatch.GetTimestamp() + (long) (inTimeMS * Stopwatch.Frequency / 1000);
            Result stepped;
            do
            {
                if (!ioEnumerator.MoveNext())
                {
                    (ioEnumerator as IDisposable)?.Dispose();
                    ioEnumerator = null;
                    stepped = Result.OutOfData;
                }
                else
                {
                    stepped = ioEnumerator.Current.GetValueOrDefault(Result.Processed);
                }
            } while (stepped == Result.Processed && Stopwatch.GetTimestamp() < endTS);
            return stepped;
        }

        #endregion // Time Sliced

        /// <summary>
        /// Enumerated state.
        /// </summary>
        public struct EnumeratedState : IDisposable
        {
            internal IEnumerator<Result?> Sequence;

            public void Clear()
            {
                (Sequence as IDisposable)?.Dispose();
                Sequence = null;
            }

            public void Dispose()
            {
                Clear();
            }
        }
    }
}