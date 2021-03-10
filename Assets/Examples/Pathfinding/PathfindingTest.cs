using System;
using System.Collections;
using BeauUtil;
using BeauUtil.Graph;
using UnityEngine;

public class PathfindingTest : MonoBehaviour
{
    [SerializeField] public Transform Actor;
    [SerializeField] public Transform GraphRoot;
    [SerializeField] public float MoveSpeed = 2;
    [SerializeField] public Transform LinkPrefab;

    private NodeGraph m_NodeGraph;

    private ushort m_CurrentNode;
    private NodePath m_CurrentPath;
    private NodePath m_PathBuffer;

    private void Awake()
    {
        m_NodeGraph = new NodeGraph();

        PathNode[] nodes = GraphRoot.GetComponentsInChildren<PathNode>();
        for(int i = 0; i < nodes.Length; ++i)
        {
            nodes[i].Id = m_NodeGraph.AddNode(nodes[i].name, 0, nodes[i].transform.position);
            nodes[i].OnClick = OnNodeClick;
            nodes[i].OnEnableOrDisable = OnNodeEnableOrDisable;
        }

        for(int i = 0; i < nodes.Length; ++i)
        {
            foreach(var connected in nodes[i].ConnectedNodes)
            {
                Vector3 center = (nodes[i].transform.position + connected.transform.position) / 2;
                Vector3 vector = connected.transform.position - nodes[i].transform.position;
                float distance = vector.magnitude;
                m_NodeGraph.AddEdge(nodes[i].Id, connected.Id, distance);

                Transform link = Instantiate(LinkPrefab, center, Quaternion.identity, GraphRoot);
                link.localEulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * (float) Math.Atan2(vector.y, vector.x));
                link.localScale = new Vector3(distance, link.localScale.y, 1);
            }
        }

        m_CurrentNode = 0;
        Actor.position = m_NodeGraph.Node(0).Position;

        m_NodeGraph.OptimizeEdgeOrder();

        m_CurrentPath = new NodePath(8);
        StartCoroutine(MoveActor(m_CurrentPath));
    }

    private void OnNodeClick(ushort inNodeId)
    {
        if (Pathfinder.AStar(m_NodeGraph, ref m_PathBuffer, m_CurrentNode, inNodeId, Pathfinder.DefaultDistanceHeuristic))
        {
            m_PathBuffer.CopyTo(m_CurrentPath);
        }
    }

    private void OnNodeEnableOrDisable(ushort inNodeId, bool inbEnabled)
    {
        m_NodeGraph.SetNodeEnabled(inNodeId, inbEnabled);
    }

    private IEnumerator MoveActor(NodePath inPath)
    {
        while(true)
        {
            if (inPath.HasTraversals())
            {
                NodeTraversal traversal = inPath.PopNext();
                if (traversal.IsTraversal())
                {
                    var edge = m_NodeGraph.Edge(traversal.EdgeId);
                    ushort nextNode = edge.EndIndex;
                    if (!edge.Enabled || !m_NodeGraph.GetNodeEnabled(nextNode))
                    {
                        inPath.Reset();
                    }
                    else
                    {
                        m_CurrentNode = nextNode;
                        Vector3 targetPosition = m_NodeGraph.Node(nextNode).Position;
                        float distance = Vector3.Distance(Actor.position, targetPosition);
                        while(distance > 0)
                        {
                            Vector3 newPos = Vector3.MoveTowards(Actor.position, targetPosition, MoveSpeed * Time.deltaTime);
                            Actor.position = newPos;
                            distance = Vector3.Distance(newPos, targetPosition);
                            yield return null;
                        }
                    }
                }
                else
                {
                    m_CurrentNode = traversal.NodeId;
                }
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}