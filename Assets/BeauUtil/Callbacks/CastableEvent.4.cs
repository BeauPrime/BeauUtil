/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 Dec 2023
 * 
 * File:    CastableEvent.3.cs
 * Purpose: Invokable list of CastableAction instances.
 */

#if UNITY_2021_2_OR_NEWER && !BEAUUTIL_DISABLE_FUNCTION_POINTERS
#define SUPPORTS_FUNCTION_POINTERS
#endif // UNITY_2021_2_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Invokable list of CastableAction instances.
    /// </summary>
    public class CastableEvent<TInput0, TInput1, TInput2, TInput3>
    {
        private int m_Length = 0;
        private CastableAction<TInput0, TInput1, TInput2, TInput3>[] m_Actions;
        private int[] m_ContextIds = Array.Empty<int>();

        public CastableEvent()
        {
            m_Actions = Array.Empty<CastableAction<TInput0, TInput1, TInput2, TInput3>>();
            m_ContextIds = Array.Empty<int>();
        }
        
        public CastableEvent(int inCapacity)
        {
            if (inCapacity < 0)
                throw new ArgumentOutOfRangeException("inCapacity");

            m_Actions = new CastableAction<TInput0, TInput1, TInput2, TInput3>[inCapacity];
            m_ContextIds = new int[inCapacity];
        }

        #region Add

        /// <summary>
        /// Registers an action.
        /// </summary>
        public void Register(Action<TInput0, TInput1, TInput2, TInput3> inAction, UnityEngine.Object inContext = null)
        {
            EnsureCapacity(m_Length + 1);
            m_Actions[m_Length] = CastableAction<TInput0, TInput1, TInput2, TInput3>.Create(inAction);
            m_ContextIds[m_Length] = UnityHelper.Id(inContext ?? inAction.Target as UnityEngine.Object);
            m_Length++;
        }

        /// <summary>
        /// Registers an action.
        /// </summary>
        public void Register(RefAction<TInput0, TInput1, TInput2, TInput3> inAction, UnityEngine.Object inContext = null)
        {
            EnsureCapacity(m_Length + 1);
            m_Actions[m_Length] = CastableAction<TInput0, TInput1, TInput2, TInput3>.Create(inAction);
            m_ContextIds[m_Length] = UnityHelper.Id(inContext ?? inAction.Target as UnityEngine.Object);
            m_Length++;
        }

        /// <summary>
        /// Registers an action.
        /// </summary>
        public void Register(Action inAction, UnityEngine.Object inContext = null)
        {
            EnsureCapacity(m_Length + 1);
            m_Actions[m_Length] = CastableAction<TInput0, TInput1, TInput2, TInput3>.Create(inAction);
            m_ContextIds[m_Length] = UnityHelper.Id(inContext ?? inAction.Target as UnityEngine.Object);
            m_Length++;
        }

#if SUPPORTS_FUNCTION_POINTERS

        /// <summary>
        /// Registers an action.
        /// </summary>
        public unsafe IntPtr Register(delegate*<TInput0, TInput1, TInput2, TInput3, void> inPointer)
        {
            EnsureCapacity(m_Length + 1);
            m_Actions[m_Length] = CastableAction<TInput0, TInput1, TInput2, TInput3>.Create(inPointer);
            m_ContextIds[m_Length] = 0;
            m_Length++;
            return (IntPtr) inPointer;
        }

        /// <summary>
        /// Registers an action.
        /// </summary>
        public unsafe IntPtr Register(delegate*<ref TInput0, ref TInput1, ref TInput2, ref TInput3, void> inPointer)
        {
            EnsureCapacity(m_Length + 1);
            m_Actions[m_Length] = CastableAction<TInput0, TInput1, TInput2, TInput3>.Create(inPointer);
            m_ContextIds[m_Length] = 0;
            m_Length++;
            return (IntPtr) inPointer;
        }

        /// <summary>
        /// Registers an action.
        /// </summary>
        public unsafe IntPtr Register(delegate*<void> inPointer)
        {
            EnsureCapacity(m_Length + 1);
            m_Actions[m_Length] = CastableAction<TInput0, TInput1, TInput2, TInput3>.Create(inPointer);
            m_ContextIds[m_Length] = 0;
            m_Length++;
            return (IntPtr) inPointer;
        }

#endif // SUPPORTS_FUNCTION_POINTERS

        #endregion // Add

        #region Remove

        /// <summary>
        /// Removes the registered action.
        /// </summary>
        public void Deregister(Action<TInput0, TInput1, TInput2, TInput3> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            for(int i = m_Length - 1; i >= 0; i--)
            {
                if (m_Actions[i].Equals(inAction))
                {
                    RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Removes the registered action.
        /// </summary>
        public void Deregister(RefAction<TInput0, TInput1, TInput2, TInput3> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            for(int i = m_Length - 1; i >= 0; i--)
            {
                if (m_Actions[i].Equals(inAction))
                {
                    RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Removes the registered action.
        /// </summary>
        public void Deregister(Action inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            for(int i = m_Length - 1; i >= 0; i--)
            {
                if (m_Actions[i].Equals(inAction))
                {
                    RemoveAt(i);
                    break;
                }
            }
        }

#if SUPPORTS_FUNCTION_POINTERS

        // NOTE: On JIT platforms, function pointers are not guaranteed to be the same
        //       On AOT it's fine, but for this reason we'll have users call Deregister
        //       with the same IntPtr they got back from the Register call

        /// <summary>
        /// Removes the registered action.
        /// </summary>
        public unsafe void Deregister(IntPtr inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inAction");

            for (int i = m_Length - 1; i >= 0; i--)
            {
                if (m_Actions[i].Equals(inPointer))
                {
                    RemoveAt(i);
                    break;
                }
            }
        }

#endif // SUPPORTS_FUNCTION_POINTERS

        /// <summary>
        /// Removes all actions bound to the given context.
        /// </summary>
        public int DeregisterAll(UnityEngine.Object inContext)
        {
            if (object.ReferenceEquals(inContext, null))
            {
                return 0;
            }

            int matchId = UnityHelper.Id(inContext);
            int deregisterCount = 0;
            for(int i = m_Length - 1; i >= 0; i--)
            {
                if (m_ContextIds[i] == matchId)
                {
                    RemoveAt(i);
                    deregisterCount++;
                }
            }
            return deregisterCount;
        }

        /// <summary>
        /// Removes all actions bound to a dead context.
        /// </summary>
        public int DeregisterAllWithDeadContext()
        {
            int deregisterCount = 0;
            for(int i = m_Length - 1; i >= 0; i--)
            {
                if (m_ContextIds[i] != 0 && !UnityHelper.IsAlive(m_ContextIds[i]))
                {
                    RemoveAt(i);
                    deregisterCount++;
                }
            }
            return deregisterCount;
        }

        /// <summary>
        /// Clears all actions from the list.
        /// </summary>
        public void Clear()
        {
            Array.Clear(m_Actions, 0, m_Length);
            Array.Clear(m_ContextIds, 0, m_Length);
            m_Length = 0;
        }

        #endregion // Remove

        #region Invoke

        /// <summary>
        /// Returns if the list has no entries.
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_Length == 0; }
        }

        /// <summary>
        /// Returns the number of entries in the list.
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_Length; }
        }

        /// <summary>
        /// Invokes all currently registered actions.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(TInput0 inInput0, TInput1 inInput1, TInput2 inInput2, TInput3 inInput3)
        {
            Invoke(ref inInput0, ref inInput1, ref inInput2, ref inInput3);
        }

        /// <summary>
        /// Invokes all currently registered actions.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public void Invoke(ref TInput0 inInput0, ref TInput1 inInput1, ref TInput2 inInput2, ref TInput3 inInput3)
        {
            int idx = 0;
            int end = m_Length;
            while(idx < end)
            {
                m_Actions[idx++].Invoke(ref inInput0, ref inInput1, ref inInput2, ref inInput3);
            }
        }

        #endregion // Invoke

        private void EnsureCapacity(int inSize)
        {
            if (m_Actions.Length < inSize)
            {
                int newSize = Mathf.NextPowerOfTwo(inSize);
                Array.Resize(ref m_Actions, newSize);
                Array.Resize(ref m_ContextIds, newSize);
            }
        }
    
        private void RemoveAt(int inIndex)
        {
            ArrayUtils.FastRemoveAt(m_Actions, m_Length, inIndex);
            ArrayUtils.FastRemoveAt(m_ContextIds, m_Length, inIndex);
            m_Length--;
        }
    }
}