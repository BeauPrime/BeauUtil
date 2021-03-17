/*
* Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
* Author:  Autumn Beauchesne
* Date:    9 March 2021
* 
* File:    Pathfinder.cs
* Purpose: Pathfinding algorithms.
*/

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using UnityEngine;

namespace BeauUtil.Graph
{
    /// <summary>
    /// Pathfinding algorithm.
    /// </summary>
    static public class Pathfinder
    {
        #region Types

        /// <summary>
        /// Filter for nodes/edges.
        /// </summary>
        public struct PathFilter
        {
            public uint Mask;
            public float EdgeCostThreshold;
        }

        /// <summary>
        /// Collection of static graph infos.
        /// </summary>
        private unsafe struct StaticGraphInfo
        {
            public NodeGraph Graph;
            public ReadOnlyNodeData* StaticNodeInfo;
            public ushort NodeCount;
            public ReadOnlyEdgeData* StaticEdgeInfo;
            public ushort EdgeCount;
        }

        /// <summary>
        /// Static node information.
        /// </summary>
        private struct ReadOnlyNodeData
        {
            public ushort EdgeStartIndex;
            public ushort EdgeCount;
            public bool Traversable;
            public float Heuristic;
        }

        /// <summary>
        /// Node traversal information.
        /// </summary>
        private struct NodeCostData
        {
            public NodeState State;
            public NodeTraversal PrevTraversal; // stored in reverse order - we traverse the path back in reverse
            public float Cost;
            public float EstimatedCost;
        }

        /// <summary>
        /// Edge information.
        /// </summary>
        private struct ReadOnlyEdgeData
        {
            public ushort TargetId;
            public bool Traversable;
            public float Cost;
        }

        /// <summary>
        /// State of a node.
        /// </summary>
        private enum NodeState : byte
        {
            Unvisited,
            Open,
            Closed
        }

        /// <summary>
        /// Delegate for the estimated cost of completing a path.
        /// </summary>
        public delegate float Heuristic(NodeGraph inGraph, ushort inStartId, ushort inEndId);

        /// <summary>
        /// Execution flags.
        /// </summary>
        public enum Flags : byte
        {
            ReturnClosestPath = 0x01,
            DontPrecalculateHeuristics = 0x02,
        }

        #endregion // Types

        /// <summary>
        /// Attempts to find a path from the starting node to the ending node.
        /// </summary>
        static public bool AStar(NodeGraph inGraph, ref NodePath ioPath, ushort inStartId, ushort inEndId, Heuristic inHeuristic = null, PathFilter inFilter = default(PathFilter), Flags inFlags = 0)
        {
            if (inGraph == null)
                throw new ArgumentNullException("inGraph");

            ioPath = ioPath ?? new NodePath();

            if (inStartId == NodeGraph.InvalidId || inStartId >= inGraph.NodeCount() || inEndId == NodeGraph.InvalidId || inEndId >= inGraph.NodeCount())
            {
                ioPath.Reset();
                return false;
            }
            else if (inStartId == inEndId)
            {
                ioPath.Reset();
                return true;
            }

            inGraph.OptimizeEdgeOrder();

            unsafe
            {
                StaticGraphInfo graphInfo;

                graphInfo.Graph = inGraph;

                ushort nodeCount = graphInfo.NodeCount = inGraph.NodeCount();
                ushort edgeCount = graphInfo.EdgeCount = inGraph.EdgeCount();

                ReadOnlyNodeData* staticNodeInfo = stackalloc ReadOnlyNodeData[nodeCount];
                ReadOnlyEdgeData* staticEdgeInfo = stackalloc ReadOnlyEdgeData[edgeCount];

                graphInfo.StaticNodeInfo = staticNodeInfo;
                graphInfo.StaticEdgeInfo = staticEdgeInfo;

                bool bPrecacheHeuristics = (inFlags & Flags.DontPrecalculateHeuristics) == 0;

                for (ushort nodeIdx = 0; nodeIdx < nodeCount; ++nodeIdx)
                {
                    ReadOnlyNodeData* nodeData = &staticNodeInfo[nodeIdx];

#if EXPANDED_REFS
                    ref var srcNodeData = ref inGraph.Node(nodeIdx);
#else
                    var srcNodeData = inGraph.Node(nodeIdx);
#endif // EXPANDED_REFS

                    nodeData->Traversable = srcNodeData.Enabled && FilterNode(inGraph, nodeIdx, inFilter);
                    nodeData->Heuristic = bPrecacheHeuristics && nodeData->Traversable && inHeuristic != null ? inHeuristic(inGraph, nodeIdx, inEndId) : 0;
                    nodeData->EdgeStartIndex = srcNodeData.EdgeStartIndex;
                    nodeData->EdgeCount = srcNodeData.EdgeCount;
                }

                for (ushort edgeIdx = 0; edgeIdx < edgeCount; ++edgeIdx)
                {
                    ReadOnlyEdgeData* edgeData = &staticEdgeInfo[edgeIdx];
#if EXPANDED_REFS
                    ref var srcEdgeData = ref inGraph.Edge(edgeIdx);
#else
                    var srcEdgeData = inGraph.Edge(inEdgeId);
#endif // EXPANDED_REFS

                    edgeData->Traversable = srcEdgeData.Enabled && FilterEdge(inGraph, edgeIdx, inFilter);
                    edgeData->Cost = srcEdgeData.Cost;
                    edgeData->TargetId = srcEdgeData.EndIndex;
                }

                return UnsafeAStar(ioPath, inStartId, inEndId, graphInfo, inHeuristic, inFlags);
            }
        }

        #region Internal

#if EXPANDED_REFS
        static private bool FilterNode(NodeGraph inGraph, ushort inNodeId, in PathFilter inFilter)
#else
        static private bool FilterNode(NodeGraph inGraph, ushort inNodeId, PathFilter inFilter)
#endif // EXPANDED_REFS
        {
            return inFilter.Mask == 0 || (inGraph.Node(inNodeId).Mask & inFilter.Mask) != 0;
        }

#if EXPANDED_REFS
        static private bool FilterEdge(NodeGraph inGraph, ushort inEdgeId, in PathFilter inFilter)
#else
        static private bool FilterEdge(NodeGraph inGraph, ushort inEdgeId, PathFilter inFilter)
#endif // EXPANDED_REFS
        {
#if EXPANDED_REFS
            ref var edgeData = ref inGraph.Edge(inEdgeId);
#else
            var edgeData = inGraph.Edge(inEdgeId);
#endif // EXPANDED_REFS
            return (inFilter.Mask == 0 || (edgeData.Mask & inFilter.Mask) != 0)
                && (inFilter.EdgeCostThreshold <= 0 || edgeData.EndIndex < inFilter.EdgeCostThreshold);
        }

        static private unsafe bool UnsafeAStar(NodePath ioPath, ushort inStartId, ushort inEndId, StaticGraphInfo inGraphInfo, Heuristic inHeuristic, Flags inFlags)
        {
            NodeCostData* nodeCosts = stackalloc NodeCostData[inGraphInfo.NodeCount];
            for (ushort i = 0, len = inGraphInfo.NodeCount; i < len; ++i)
                nodeCosts[i].PrevTraversal = NodeTraversal.Invalid;

            ushort maxOpenListSize = inGraphInfo.NodeCount >= 16 ? (ushort)(12 + inGraphInfo.NodeCount / 4) : inGraphInfo.NodeCount;
            ushort* openList = stackalloc ushort[maxOpenListSize];
            ushort openListLength = 0;

            AddToOpenList(inStartId, openList, ref openListLength);

            bool bRecalculateHeuristics = (inFlags & Flags.DontPrecalculateHeuristics) != 0 && inHeuristic != null;

            ushort currentNodeId = inStartId;

            // initialize the first node info
            {
                NodeCostData* startNodeData = &nodeCosts[currentNodeId];
                ReadOnlyNodeData* startNodeStaticInfo = &inGraphInfo.StaticNodeInfo[currentNodeId];
                float heuristic = startNodeStaticInfo->Heuristic;
                if (bRecalculateHeuristics)
                    heuristic = startNodeStaticInfo->Heuristic = inHeuristic(inGraphInfo.Graph, currentNodeId, inEndId);
                startNodeData->EstimatedCost = heuristic;
            }

            while (openListLength > 0)
            {
                currentNodeId = PopLowestCostRecord(openList, ref openListLength, nodeCosts);
                if (currentNodeId == inEndId)
                    break;

                NodeCostData* nodeData = &nodeCosts[currentNodeId];
                nodeData->State = NodeState.Closed;

                ushort edgeStartIdx = inGraphInfo.StaticNodeInfo[currentNodeId].EdgeStartIndex;
                ushort edgeUpper = (ushort)(edgeStartIdx + inGraphInfo.StaticNodeInfo[currentNodeId].EdgeCount);

                for (ushort edgeIdx = edgeStartIdx; edgeIdx < edgeUpper; edgeIdx++)
                {
                    ReadOnlyEdgeData* edgeData = &inGraphInfo.StaticEdgeInfo[edgeIdx];
                    if (!edgeData->Traversable)
                        continue;

                    float fullCost = nodeData->Cost + edgeData->Cost;

                    ushort nextNodeId = edgeData->TargetId;
                    ReadOnlyNodeData* nextNodeStaticInfo = &inGraphInfo.StaticNodeInfo[nextNodeId];

                    if (!nextNodeStaticInfo->Traversable)
                        continue;

                    NodeCostData* nextNodeData = &nodeCosts[nextNodeId];
                    switch (nextNodeData->State)
                    {
                        case NodeState.Unvisited:
                            {
                                nextNodeData->State = NodeState.Open;

                                if (openListLength >= maxOpenListSize)
                                    throw new Exception("Heuristic for open list size was wrong, more entries required");
                                AddToOpenList(nextNodeId, openList, ref openListLength);

                                // if we aren't precalculating heuristics, calculate it here instead
                                if (bRecalculateHeuristics)
                                {
                                    nextNodeStaticInfo->Heuristic = inHeuristic(inGraphInfo.Graph, nextNodeId, inEndId);
                                }

                                nextNodeData->Cost = fullCost;
                                nextNodeData->EstimatedCost = fullCost + nextNodeStaticInfo->Heuristic;
                                nextNodeData->PrevTraversal = new NodeTraversal(currentNodeId, edgeIdx); // stored in reverse order
                                break;
                            }

                        case NodeState.Open:
                            {
                                if (nextNodeData->Cost <= fullCost)
                                    break;

                                nextNodeData->Cost = fullCost;
                                nextNodeData->EstimatedCost = fullCost + nextNodeStaticInfo->Heuristic;
                                nextNodeData->PrevTraversal = new NodeTraversal(currentNodeId, edgeIdx); // stored in reverse order
                                break;
                            }

                        case NodeState.Closed:
                            {
                                if (nextNodeData->Cost <= fullCost)
                                    break;

                                nextNodeData->State = NodeState.Open;

                                if (openListLength >= maxOpenListSize)
                                    throw new Exception("Heuristic for open list size was wrong, more entries required");
                                AddToOpenList(nextNodeId, openList, ref openListLength);

                                nextNodeData->Cost = fullCost;
                                nextNodeData->EstimatedCost = fullCost + nextNodeStaticInfo->Heuristic;
                                nextNodeData->PrevTraversal = new NodeTraversal(currentNodeId, edgeIdx); // stored in reverse order
                                break;
                            }
                    }
                }
            }

            // if we reached the end, let's construct the path
            bool bReachedEnd = currentNodeId == inEndId;
            bool bGeneratePath = bReachedEnd;
            if (!bReachedEnd && (inFlags & Flags.ReturnClosestPath) != 0)
            {
                if (inHeuristic == null)
                    throw new InvalidOperationException("Cannot make incomplete path to closest point when no heuristic is provided");

                currentNodeId = FindLowestHeuristicVisitedNode(nodeCosts, inGraphInfo.StaticNodeInfo, inGraphInfo.NodeCount);
                bGeneratePath = currentNodeId != NodeGraph.InvalidId;
            }

            if (bGeneratePath)
            {
                ioPath.Reset();

                NodeTraversal traversal;
                while (currentNodeId != inStartId && currentNodeId != NodeGraph.InvalidId)
                {
                    traversal = nodeCosts[currentNodeId].PrevTraversal;
                    // we have to reverse the edge/node order here, since it was recorded in reverse
                    ioPath.AddTraversal(new NodeTraversal(currentNodeId, traversal.EdgeId));
                    currentNodeId = traversal.NodeId;
                }

                return bReachedEnd;
            }

            ioPath.Reset();
            return false;
        }

        #region OpenList

        // custom list management code

        static private unsafe void AddToOpenList(ushort inId, ushort* ioOpenList, ref ushort ioLength)
        {
            ioOpenList[ioLength++] = inId;
        }

        static private unsafe bool RemoveFromOpenList(ushort inId, ushort* ioOpenList, ref ushort ioLength)
        {
            int end = ioLength - 1;
            for (int i = 0; i < ioLength; ++i)
            {
                if (ioOpenList[i] == inId)
                {
                    if (i < end)
                        ioOpenList[i] = ioOpenList[end];
                    --ioLength;
                    return true;
                }
            }
            return false;
        }

        static private unsafe ushort PopLowestCostRecord(ushort* ioOpenList, ref ushort ioOpenListLength, NodeCostData* inCostData)
        {
            int lowestCostIdx = 0;
            ushort lowestCostId = ioOpenList[0];
            float lowestCost = inCostData[lowestCostId].EstimatedCost;

            ushort id;
            float cost;
            for (int i = 1; i < ioOpenListLength; ++i)
            {
                id = ioOpenList[i];
                cost = inCostData[id].EstimatedCost;
                if (cost < lowestCost)
                {
                    lowestCostIdx = i;
                    lowestCostId = id;
                    lowestCost = cost;
                }
            }

            int end = ioOpenListLength - 1;
            if (lowestCostIdx < end)
                ioOpenList[lowestCostIdx] = ioOpenList[end];
            --ioOpenListLength;

            return lowestCostId;
        }

        static private unsafe ushort FindLowestHeuristicVisitedNode(NodeCostData* inCosts, ReadOnlyNodeData* inStaticNodeInfo, ushort inNodeCount)
        {
            ushort lowestId = NodeGraph.InvalidId;
            float lowestCost = float.MaxValue;
            for (ushort i = 0; i < inNodeCount; i++)
            {
                NodeCostData* cost = &inCosts[i];
                ReadOnlyNodeData* node = &inStaticNodeInfo[i];

                if (cost->State > NodeState.Unvisited && node->Heuristic < lowestCost)
                {
                    lowestId = i;
                    lowestCost = node->Heuristic;
                }
            }
            return lowestId;
        }

        #endregion // OpenList

        #endregion // Internal

        /// <summary>
        /// Default distance heuristic. Generates heuristic based on node distance.
        /// </summary>
        static public readonly Heuristic DefaultDistanceHeuristic = (graph, inStartId, inEndId) =>
        {
            return Vector3.Distance(graph.Node(inStartId).Position, graph.Node(inEndId).Position);
        };

        /// <summary>
        /// Manhattan distance heuristic. Generates heuristic based on node distance.
        /// </summary>
        static public readonly Heuristic ManhattanDistanceHeuristic = (graph, inStartId, inEndId) =>
        {
            Vector3 start = graph.Node(inStartId).Position, end = graph.Node(inEndId).Position;
            return Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y) - Math.Abs(start.z - end.z);
        };
    }
}