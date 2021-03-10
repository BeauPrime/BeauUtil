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
        /// Filter for 
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
            public NodeTraversal PrevTraversal;
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

        #endregion // Types

        /// <summary>
        /// Attempts to find a path from the starting node to the ending node.
        /// </summary>
        static public bool AStar(NodeGraph inGraph, ref NodePath ioPath, ushort inStartId, ushort inEndId, Heuristic inHeuristic = null, PathFilter inFilter = default(PathFilter))
        {
            ioPath = ioPath ?? new NodePath();

            if (inStartId == NodeGraph.InvalidId || inEndId == NodeGraph.InvalidId)
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

                ushort nodeCount = graphInfo.NodeCount = inGraph.NodeCount();
                ushort edgeCount = graphInfo.EdgeCount = inGraph.EdgeCount();

                ReadOnlyNodeData* staticNodeInfo = stackalloc ReadOnlyNodeData[nodeCount];
                ReadOnlyEdgeData* staticEdgeInfo = stackalloc ReadOnlyEdgeData[edgeCount];

                graphInfo.StaticNodeInfo = staticNodeInfo;
                graphInfo.StaticEdgeInfo = staticEdgeInfo;

                for(ushort nodeIdx = 0; nodeIdx < nodeCount; ++nodeIdx)
                {
                    ReadOnlyNodeData* nodeData = &staticNodeInfo[nodeIdx];

                    #if EXPANDED_REFS
                    ref var srcNodeData = ref inGraph.Node(nodeIdx);
                    #else
                    var srcNodeData = inGraph.Node(nodeIdx);
                    #endif // EXPANDED_REFS

                    nodeData->Traversable = srcNodeData.Enabled && FilterNode(inGraph, nodeIdx, inFilter);
                    nodeData->Heuristic = nodeData->Traversable && inHeuristic != null ? inHeuristic(inGraph, nodeIdx, inEndId) : 0;
                    nodeData->EdgeStartIndex = srcNodeData.EdgeStartIndex;
                    nodeData->EdgeCount = srcNodeData.EdgeCount;
                }

                for(ushort edgeIdx = 0; edgeIdx < edgeCount; ++edgeIdx)
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

                return AStar(ioPath, inStartId, inEndId, graphInfo);
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

        static private unsafe bool AStar(NodePath ioPath, ushort inStartId, ushort inEndId, StaticGraphInfo inGraphInfo)
        {
            NodeCostData* nodeCosts = stackalloc NodeCostData[inGraphInfo.NodeCount];

            ushort maxOpenListSize = inGraphInfo.NodeCount >= 16 ? (ushort) (12 + inGraphInfo.NodeCount / 4) : inGraphInfo.NodeCount;
            ushort* openList = stackalloc ushort[maxOpenListSize];
            ushort openListLength = 0;

            AddToOpenList(inStartId, openList, ref openListLength);
            
            ushort currentNodeId = inStartId;
            while(openListLength > 0)
            {
                currentNodeId = PopLowestCostRecord(openList, ref openListLength, nodeCosts);
                if (currentNodeId == inEndId)
                    break;

                NodeCostData* nodeData = &nodeCosts[currentNodeId];
                nodeData->State = NodeState.Closed;

                ushort edgeStartIdx = inGraphInfo.StaticNodeInfo[currentNodeId].EdgeStartIndex;
                ushort edgeUpper = (ushort) (edgeStartIdx + inGraphInfo.StaticNodeInfo[currentNodeId].EdgeCount);

                for(ushort edgeIdx = edgeStartIdx; edgeIdx < edgeUpper; ++edgeIdx)
                {
                    ReadOnlyEdgeData* edgeData = &inGraphInfo.StaticEdgeInfo[edgeIdx];
                    if (!edgeData->Traversable)
                        continue;
                    
                    float fullCost = nodeData->Cost + edgeData->Cost;

                    ushort nextNodeId = edgeData->TargetId;
                    ReadOnlyNodeData* nextNodeData = &inGraphInfo.StaticNodeInfo[nextNodeId];

                    if (!nextNodeData->Traversable)
                        continue;

                    NodeCostData* nextNodeCost = &nodeCosts[nextNodeId];
                    switch(nextNodeCost->State)
                    {
                        case NodeState.Unvisited:
                            {
                                nextNodeCost->State = NodeState.Open;

                                if (openListLength >= maxOpenListSize)
                                    throw new Exception("Heuristic for open list size was wrong, more entries required");
                                AddToOpenList(nextNodeId, openList, ref openListLength);

                                nextNodeCost->Cost = fullCost;
                                nextNodeCost->EstimatedCost = fullCost + nextNodeData->Heuristic;
                                nextNodeCost->PrevTraversal = new NodeTraversal(currentNodeId, edgeIdx);
                                break;
                            }

                        case NodeState.Open:
                            {
                                if (nextNodeCost->Cost <= fullCost)
                                    break;

                                nextNodeCost->Cost = fullCost;
                                nextNodeCost->EstimatedCost = fullCost + nextNodeData->Heuristic;
                                nextNodeCost->PrevTraversal = new NodeTraversal(currentNodeId, edgeIdx);
                                break;
                            }

                        case NodeState.Closed:
                            {
                                if (nextNodeCost->Cost <= fullCost)
                                    break;

                                nextNodeCost->State = NodeState.Open;

                                if (openListLength >= maxOpenListSize)
                                    throw new Exception("Heuristic for open list size was wrong, more entries required");
                                AddToOpenList(nextNodeId, openList, ref openListLength);

                                nextNodeCost->Cost = fullCost;
                                nextNodeCost->EstimatedCost = fullCost + nextNodeData->Heuristic;
                                nextNodeCost->PrevTraversal = new NodeTraversal(currentNodeId, edgeIdx);
                                break;
                            }
                    }
                }
            }

            // if we reached the end, let's construct the path
            if (currentNodeId == inEndId)
            {
                ioPath.Reset();

                NodeTraversal traversal = new NodeTraversal(currentNodeId);
                while(traversal.NodeId != inStartId)
                {
                    ioPath.AddTraversal(traversal);
                    traversal = nodeCosts[traversal.NodeId].PrevTraversal;
                }
                ioPath.AddTraversal(traversal);
                ioPath.FinishTraversals();
                return true;
            }

            ioPath.Reset();
            return false;
        }

        #region OpenList

        static private unsafe void AddToOpenList(ushort inId, ushort* ioOpenList, ref ushort ioLength)
        {
            ioOpenList[ioLength++] = inId;
        }

        static private unsafe bool RemoveFromOpenList(ushort inId, ushort* ioOpenList, ref ushort ioLength)
        {
            int end = ioLength - 1;
            for(int i = 0; i < ioLength; ++i)
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
            for(int i = 1; i < ioOpenListLength; ++i)
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
    
        #endregion // OpenList

        #endregion // Internal

        /// <summary>
        /// Default distance heuristic. Generates heuristic based on node 
        /// </summary>
        static public readonly Heuristic DefaultDistanceHeuristic = (graph, inStartId, inEndId) => {
            return Vector3.Distance(graph.Node(inStartId).Position, graph.Node(inEndId).Position);
        };
    }
}