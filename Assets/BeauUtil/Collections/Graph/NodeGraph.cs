/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    25 Jan 2021
 * 
 * File:    NodeGraph.cs
 * Purpose: Node Graph.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeauUtil.Graph
{
    public sealed class NodeGraph
    {
        public const ushort InvalidId = ushort.MaxValue;

        /// <summary>
        /// Node data.
        /// </summary>
        public struct NodeData
        {
            public Vector3 Position;
            public StringHash32 Name;
            public uint Mask;
            public ushort MetaIndex;
            public ushort EdgeCount;
            public ushort EdgeStartIndex;
            public bool Enabled;
        }

        /// <summary>
        /// Edge data.
        /// </summary>
        public struct EdgeData
        {
            public ushort StartIndex;
            public ushort EndIndex;
            public float Cost;
            public uint Mask;
            public ushort MetaIndex;
            public bool Enabled;
        }

        private readonly RingBuffer<NodeData> m_Nodes;
        private readonly RingBuffer<EdgeData> m_Edges;
        private readonly RingBuffer<object> m_Meta;
        private bool m_Locked;

        public NodeGraph()
        {
            m_Nodes = new RingBuffer<NodeData>();
            m_Edges = new RingBuffer<EdgeData>();
            m_Meta = new RingBuffer<object>();
        }

        #region Clearing

        public void Clear()
        {
            m_Locked = false;
            m_Nodes.Clear();
            m_Edges.Clear();
            m_Meta.Clear();
        }

        #endregion // Clearing

        #region Nodes

        /// <summary>
        /// Adds a node to the graph.
        /// </summary>
        public ushort AddNode(StringHash32 inName = default(StringHash32), uint inMask = 0, Vector3 inPosition = default(Vector3), object inMetadata = null)
        {
            if (m_Locked)
                throw new InvalidOperationException("Cannot modify graph after locking - must call Clear first");

            ushort id = (ushort) m_Nodes.Count;
            if (id >= InvalidId)
                throw new InvalidOperationException(string.Format("Cannot add more than {0} nodes to graph", InvalidId - 1));
            
            NodeData node;
            node.Name = inName;
            node.Mask = inMask;
            node.EdgeCount = node.EdgeStartIndex = 0;
            node.Position = inPosition;
            node.Enabled = true;
            if (inMetadata != null)
            {
                node.MetaIndex = (ushort) m_Meta.Count;
                if (node.MetaIndex >= InvalidId)
                    throw new InvalidOperationException(string.Format("Cannot add more than {0} metadata objects to graph", InvalidId - 1));

                m_Meta.PushBack(inMetadata);
            }
            else
            {
                node.MetaIndex = InvalidId;
            }

            m_Nodes.PushBack(node);
            return id;
        }

        /// <summary>
        /// Returns the total number of nodes in the graph.
        /// </summary>
        public ushort NodeCount() { return (ushort) m_Nodes.Count; }

        /// <summary>
        /// Returns the data for the given node.
        /// </summary>
        #if EXPANDED_REFS
        public ref NodeData Node(ushort inId) { return ref m_Nodes[inId]; }
        #else
        public NodeData Node(ushort inId) { return m_Nodes[inId]; }
        #endif // EXPANDED_REFS

        /// <summary>
        /// Returns if the given node is enabled.
        /// </summary>
        public bool GetNodeEnabled(ushort inId)
        {
            return m_Nodes[inId].Enabled;
        }

        /// <summary>
        /// Sets whether or not a node is enabled.
        /// </summary>
        public void SetNodeEnabled(ushort inId, bool inbEnabled)
        {
            #if EXPANDED_REFS
            ref var nodeData = ref m_Nodes[inId];
            #else
            var nodeData = m_Nodes[inId];
            #endif // EXPANDED_REFS

            if (nodeData.Enabled != inbEnabled)
            {
                nodeData.Enabled = inbEnabled;
                #if !EXPANDED_REFS
                m_Nodes[inId] = nodeData;
                #endif // !EXPANDED_REFS
            }
        }

        #endregion // Nodes

        #region Edges

        /// <summary>
        /// Adds an edge to the graph.
        /// </summary>
        public ushort AddEdge(ushort inStartId, ushort inEndId, float inCost = 1, uint inMask = 0, object inMetadata = null)
        {
            if (m_Locked)
                throw new InvalidOperationException("Cannot modify graph after locking - must call Clear first");

            ushort id = (ushort) m_Edges.Count;
            if (id >= InvalidId)
                throw new InvalidOperationException(string.Format("Cannot add more than {0} edges to graph", InvalidId - 1));

            EdgeData edge;
            edge.StartIndex = inStartId;
            edge.EndIndex = inEndId;
            edge.Cost = inCost;
            edge.Mask = inMask;
            edge.Enabled = true;
            if (inMetadata != null)
            {
                edge.MetaIndex = (ushort) m_Meta.Count;
                if (edge.MetaIndex >= InvalidId)
                    throw new InvalidOperationException(string.Format("Cannot add more than {0} metadata objects to graph", InvalidId - 1));

                m_Meta.PushBack(inMetadata);
            }
            else
            {
                edge.MetaIndex = InvalidId;
            }

            m_Edges.PushBack(edge);
            return id;
        }

        /// <summary>
        /// Returns the total number of edges in the graph.
        /// </summary>
        public ushort EdgeCount() { return (ushort) m_Edges.Count; }

        /// <summary>
        /// Returns the data for the given edge.
        /// </summary>
        #if EXPANDED_REFS
        public ref EdgeData Edge(ushort inId) { return ref m_Edges[inId]; }
        #else
        public EdgeData Edge(ushort inId) { return m_Edges[inId]; }
        #endif // EXPANDED_REFS

        /// <summary>
        /// Returns if the given edge is enabled.
        /// </summary>
        public bool GetEdgeEnabled(ushort inId)
        {
            return m_Edges[inId].Enabled;
        }

        /// <summary>
        /// Sets whether or not an edge is enabled.
        /// </summary>
        public void SetEdgeEnabled(ushort inId, bool inbEnabled)
        {
            #if EXPANDED_REFS
            ref var edgeData = ref m_Edges[inId];
            #else
            var edgeData = m_Edges[inId];
            #endif // EXPANDED_REFS

            if (edgeData.Enabled != inbEnabled)
            {
                edgeData.Enabled = inbEnabled;
                #if !EXPANDED_REFS
                m_Edges[inId] = edgeData;
                #endif // !EXPANDED_REFS
            }
        }

        #endregion // Edges

        #region Meta

        /// <summary>
        /// Attempts to retrieve the meta with the given index.
        /// </summary>
        public bool TryGetMeta(ushort inMetaIndex, out object outMeta)
        {
            if (inMetaIndex >= m_Meta.Count)
            {
                outMeta = null;
                return false;
            }

            outMeta = m_Meta[inMetaIndex];
            return true;
        }

        #endregion // Meta

        /// <summary>
        /// Bakes edge order into the graph.
        /// </summary>
        public void OptimizeEdgeOrder()
        {
            if (m_Locked)
                return;

            // sort all edges by starting index
            m_Edges.Sort(EdgeSorter.Instance);

            ushort startIdx = InvalidId;
            for(ushort i = 0, len = (ushort) m_Edges.Count; i < len; ++i)
            {
                ushort edgeStart = m_Edges[i].StartIndex;

                if (edgeStart != startIdx)
                {
                    startIdx = edgeStart;

                    #if EXPANDED_REFS
                    ref var nodeData = ref m_Nodes[startIdx];
                    #else
                    var nodeData = m_Nodes[startIdx];
                    #endif // EXPANDED_REFS

                    nodeData.EdgeStartIndex = i;
                    nodeData.EdgeCount = 1;

                    #if !EXPANDED_REFS
                    m_Nodes[startIdx] = nodeData;
                    #endif // !EXPANDED_REFS
                }
                else
                {
                    #if EXPANDED_REFS
                    ref var nodeData = ref m_Nodes[startIdx];
                    #else
                    var nodeData = m_Nodes[startIdx];
                    #endif // EXPANDED_REFS

                    ++nodeData.EdgeCount;

                    #if !EXPANDED_REFS
                    m_Nodes[startIdx] = nodeData;
                    #endif // !EXPANDED_REFS
                }
            }

            m_Locked = true;
        }

        private class EdgeSorter : IComparer<EdgeData>
        {
            static internal readonly EdgeSorter Instance = new EdgeSorter();

            public int Compare(EdgeData x, EdgeData y)
            {
                return x.StartIndex < y.StartIndex ? -1 : (x.StartIndex > y.StartIndex ? 1 : 0);
            }
        }
    }
}