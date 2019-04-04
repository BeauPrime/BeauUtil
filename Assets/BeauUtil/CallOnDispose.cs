/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    CallOnDispose.cs
 * Purpose: Calls a method when disposed.
 */

using System;

namespace BeauUtil
{
    /// <summary>
    /// Calls a function when disposed.
    /// </summary>
	public struct CallOnDispose : IDisposable
	{
        private Action m_Action;

        public CallOnDispose(Action inAction)
        {
            m_Action = inAction;
        }

        public void Dispose()
        {
            if (m_Action != null)
            {
                m_Action();
                m_Action = null;
            }
        }
    }
}

