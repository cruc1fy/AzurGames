using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class MazeEdge
{
    public int n1;
    public int n2;
    public GameObject wall;
    public bool destroyed;
}

public class MazeNode
{
    public int n;
    public bool visited;
    public int distance;
    public List<MazeEdge> edges = new List<MazeEdge>();
}

public class MazeGenerator : MonoBehaviour
{
    public int start;
    public int finish;

    public MazeEdge[] edges;

    public int maxStep = 30;

    private int[] used;

    private bool open;

    const int attempts = 10;
    static int[] maxLong = new int[attempts];
    static MazeEdge[][] mazes = new MazeEdge[attempts][];

    void Start()
    {
        //Draw();
    }

    public MazeNode[] BuildTree()
    {
        var nodes = new MazeNode[finish + 1];
        for(int i = 0; i < finish+1; i++)
        {
            var node = new MazeNode();
            node.n = i;
            nodes[i] = node;
        }

        foreach(var edge in edges)
        {
            var node1 = nodes[edge.n1];
            var node2 = nodes[edge.n2];

            node1.edges.Add(edge);
            node2.edges.Add(edge);
        }

        return nodes;
    }

    void GenerateByTree(MazeNode[] tree)
    {
        var unvisitedNodes = new List<MazeNode>();
        foreach(var node in tree)
        {
            if (node.n != finish)
            {
                unvisitedNodes.Add(node);
            }
        }

        var startNode = unvisitedNodes[0];
        while(unvisitedNodes.Count != 0)
        {
            var distance = startNode.distance + 1;
            unvisitedNodes.Remove(startNode);
            if (unvisitedNodes.Count == 0) break;

            startNode = VisitNode(startNode, tree);
            if(startNode == null)
            {
                var unvisitedEdges = new List<MazeEdge>();
                foreach (var node in unvisitedNodes)
                {
                    foreach(var edge in node.edges)
                    {
                        var node1 = tree[edge.n1];
                        var node2 = tree[edge.n2];
                        if(node1.visited || node2.visited)
                        {
                            unvisitedEdges.Add(edge);
                        }
                    }
                }
                if (unvisitedEdges.Count == 0) break;
                var nextEdge = unvisitedEdges[Random.Range(0, unvisitedEdges.Count)];
                nextEdge.destroyed = true;

                var nextNode1 = tree[nextEdge.n1];
                var nextNode2 = tree[nextEdge.n2];
                if (!nextNode1.visited)
                {
                    startNode = tree[nextEdge.n1];
                    distance = nextNode2.distance + 1;
                }
                else
                {
                    startNode = tree[nextEdge.n2];
                    distance = nextNode1.distance + 1;
                }
            }

            startNode.distance = distance;
        }

        var lastNode = tree[finish];
        MazeEdge maxDistEdge = null;
        var maxDist = 0;
        for(int i = 0; i < lastNode.edges.Count; i++)
        {
            var edge = lastNode.edges[i];
            var node1 = tree[edge.n1];
            var node2 = tree[edge.n2];
            if(node1 != lastNode)
            {
                if(node1.distance > maxDist || maxDistEdge == null)
                {
                    maxDistEdge = edge;
                    maxDist = node1.distance;
                }
            }

            if (node2 != lastNode)
            {
                if (node2.distance > maxDist || maxDistEdge == null)
                {
                    maxDistEdge = edge;
                    maxDist = node2.distance;
                }
            }
        }
        maxDistEdge.destroyed = true;
    }

    private MazeNode VisitNode(MazeNode startNode, MazeNode[] tree)
    {
        startNode.visited = true;
        var unvisitedEdges = new List<MazeEdge>();
        foreach (var edge in startNode.edges)
        {
            var node1 = tree[edge.n1];
            var node2 = tree[edge.n2];
            if (node1 != startNode && node1.n != finish && !node1.visited)
            {
                unvisitedEdges.Add(edge);
            }

            if (node2 != startNode && node2.n != finish && !node2.visited)
            {
                unvisitedEdges.Add(edge);
            }
        }
        if (unvisitedEdges.Count == 0) return null;
        var nextEdge = unvisitedEdges[Random.Range(0, unvisitedEdges.Count)];
        nextEdge.destroyed = true;

        if (nextEdge.n1 != startNode.n) return tree[nextEdge.n1];
        return tree[nextEdge.n2];
    }

    public void Draw()
    {
        BuildBack();
        GenerateByTree(BuildTree());
        for (int i = 0; i < edges.Length; i++)
        {
            MazeEdge edge = edges[i];
            edge.wall.SetActive(!edge.destroyed);
        }
        
    }

    

    public void BuildBack()
    {
        used = new int[finish + 1];
        open = false;

        for (int i = 0; i < edges.Length; i++)
        {   
            MazeEdge edge = edges[i];
            edge.wall.SetActive(true);
            edge.destroyed = false;
        }
    }

   
}
