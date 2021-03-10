/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    9 March 2021
 * 
 * File:    NodePath.cs
 * Purpose: Path of nodes.
 */

using System;

namespace BeauUtil.Graph
{
    /// <summary>
    /// Path through a set of nodes.
    /// </summary>
    public class NodePath
    {
        public const int DefaultCapacity = 8;

        private readonly RingBuffer<NodeTraversal> m_Traversals;
        private bool m_Finished;

        public NodePath()
            : this(DefaultCapacity)
        { }

        public NodePath(int inCapacity)
        {
            m_Traversals = new RingBuffer<NodeTraversal>(inCapacity, RingBufferMode.Expand);
        }

        /// <summary>
        /// Resets the path.
        /// </summary>
        public void Reset()
        {
            m_Finished = false;
            m_Traversals.Clear();
        }

        /// <summary>
        /// Adds a traversal to the path.
        /// Note that traversals should be added in reverse order.
        /// </summary>
        public void AddTraversal(NodeTraversal inTraversal)
        {
            if (m_Finished)
                throw new InvalidOperationException("Traversals cannot be added if after FinishTraversals unless Reset is called");
            m_Traversals.PushFront(inTraversal);
        }

        /// <summary>
        /// Finalizes the traversal list.
        /// </summary>
        public void FinishTraversals()
        {
            if (!m_Finished)
            {
                m_Finished = true;
            }
        }

        /// <summary>
        /// Copies the contents of this path to another path.
        /// </summary>
        public void CopyTo(NodePath inTarget)
        {
            if (inTarget == null)
                throw new ArgumentNullException("inTarget");
            if (inTarget == this)
                return;

            m_Traversals.CopyTo(inTarget.m_Traversals);
        }

        /// <summary>
        /// Returns the length of the path.
        /// </summary>
        public int Length() { return m_Traversals.Count; }

        /// <summary>
        /// Returns if the path has one or more traversals.
        /// </summary>
        public bool HasTraversals() { return m_Traversals.Count > 0; }

        /// <summary>
        /// Returns the traversal at the given index.
        /// </summary>
        public NodeTraversal Traversal(int inIndex)
        {
            return m_Traversals[inIndex];
        }

        /// <summary>
        /// Returns the next traversal at the given index.
        /// </summary>
        public NodeTraversal PopNext()
        {
            if (m_Traversals.Count > 0)
                return m_Traversals.PopFront();
            return NodeTraversal.Invalid;
        }
    }
}