/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    9 March 2021
 * 
 * File:    NodeTraversal.cs
 * Purpose: Traversal from one node to another across an edge.
 */

namespace BeauUtil.Graph
{
    /// <summary>
    /// Node traversal.
    /// </summary>
    public struct NodeTraversal
    {
        public ushort NodeId;
        public ushort EdgeId;

        public NodeTraversal(ushort inNodeId, ushort inEdgeId = NodeGraph.InvalidId)
        {
            NodeId = inNodeId;
            EdgeId = inEdgeId;
        }

        public bool IsNode()
        {
            return NodeId != NodeGraph.InvalidId;
        }

        public bool IsTraversal()
        {
            return EdgeId != NodeGraph.InvalidId;
        }

        static private readonly NodeTraversal s_Invalid = new NodeTraversal(NodeGraph.InvalidId);

        /// <summary>
        /// Invalid traversal.
        /// </summary>
        static public NodeTraversal Invalid { get { return s_Invalid; } }
    }
}