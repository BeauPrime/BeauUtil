using System;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using BeauUtil.Graph;

namespace BeauUtil.UnitTests
{
    static public class GraphTests
    {
        [Test]
        static public void EdgesCanBeAddedOutOfOrder()
        {
            NodeGraph graph = new NodeGraph();
            ushort start = graph.AddNode();
            ushort mid = graph.AddNode();
            ushort end = graph.AddNode();
            graph.AddEdge(start, mid);
            graph.AddEdge(mid, start);
            graph.AddEdge(end, mid);
            graph.AddEdge(mid, end);
            graph.OptimizeEdgeOrder();

            Assert.AreEqual(0, graph.Node(start).EdgeStartIndex);
            Assert.AreEqual(1, graph.Node(start).EdgeCount);
            Assert.AreEqual(2, graph.Node(mid).EdgeCount);
        }

        [Test]
        static public void AStarPathCanBeRun()
        {
            NodeGraph graph = new NodeGraph();
            ushort start = graph.AddNode();
            ushort mid = graph.AddNode();
            ushort end = graph.AddNode();
            graph.AddEdge(start, mid);
            graph.AddEdge(mid, start);
            graph.AddEdge(end, mid);
            graph.AddEdge(mid, end);
            graph.OptimizeEdgeOrder();

            NodePath path = new NodePath();
            bool bFoundPath = Pathfinder.AStar(graph, ref path, start, end);
            Assert.IsTrue(bFoundPath);
        }
    }
}