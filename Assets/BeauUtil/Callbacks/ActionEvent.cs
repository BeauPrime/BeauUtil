/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    19 Dec 2022
 * 
 * File:    ActionEvent.cs
 * Purpose: Invokable list of Action instances.
 */

using System;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Invokable list of Action instances.
    /// </summary>
    public class ActionEvent
    {
        private int m_Length = 0;
        private Action[] m_Actions;
        private int[] m_ContextIds = Array.Empty<int>();

        public ActionEvent()
        {
            m_Actions = Array.Empty<Action>();
            m_ContextIds = Array.Empty<int>();
        }
        
        public ActionEvent(int inCapacity)
        {
            if (inCapacity < 0)
                throw new ArgumentOutOfRangeException("inCapacity");

            m_Actions = new Action[inCapacity];
            m_ContextIds = new int[inCapacity];
        }

        #region Add

        /// <summary>
        /// Registers an action.
        /// </summary>
        public void Register(Action inAction, UnityEngine.Object inContext = null)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            EnsureCapacity(m_Length + 1);
            m_Actions[m_Length] = inAction;
            m_ContextIds[m_Length] = UnityHelper.Id(inContext ?? inAction.Target as UnityEngine.Object);
            m_Length++;
        }

        #endregion // Add

        #region Remove

        /// <summary>
        /// Removes the registered action.
        /// </summary>
        public void Deregister(Action inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            for(int i = m_Length - 1; i >= 0; i--)
            {
                if (m_Actions[i] == inAction)
                {
                    RemoveAt(i);
                    break;
                }
            }
        }

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
            get { return m_Length == 0; }
        }

        /// <summary>
        /// Returns the number of entries in the list.
        /// </summary>
        public int Count
        {
            get { return m_Length; }
        }

        /// <summary>
        /// Invokes all currently registered actions.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public void Invoke()
        {
            int idx = 0;
            int end = m_Length;
            while(idx < end)
            {
                m_Actions[idx++].Invoke();
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