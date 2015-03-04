using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Prefabs
{
    public GameObject edge;
    public GameObject node;
}

[System.Serializable]
public class Boundary
{
    public int minX;
    public int maxX;
    public int minY;
    public int maxY;
}

[System.Serializable]
public class BFS_Parameters
{
    public float dispersionFactor;
    public float connectionProbability;

    public int minDegree;
    public int maxDegree;
}

[System.Serializable]
public class GraphParameters
{
    public int nodeCount;
    public bool isDirected;

    public float nodeRadius;
    public int nodeBoundary;
    public Boundary bounds;

    public BFS_Parameters BFS;
}


public class GraphScript : MonoBehaviour {

    public Prefabs myPrefabs;
    public GraphParameters myParams;

    private List<NodeScript> nodes = new List<NodeScript>();
    private List<EdgeScript> edges = new List<EdgeScript>();

    /// <summary>
    /// Map from a position vector to the node at that point </summary>
    private Dictionary<Vector3, NodeScript> nodeMap = new Dictionary<Vector3, NodeScript>();


    public void Create_BFS()
    {
        // Add nodes
        int bound = (int) Mathf.Ceil(Mathf.Sqrt(myParams.nodeCount) 
            * Mathf.Max(myParams.BFS.dispersionFactor, 3.5f));
        for (int n = 0; n < myParams.nodeCount; n++)
        {
            bool added = false;
            while (!added)
            {
                int x = (int)Mathf.Round(Random.Range(-bound, bound));
                int y = (int)Mathf.Round(Random.Range(-bound, bound));

                if (IsAdjacentNode(new Vector3(x, y, 0)) == GameController.FALSE)
                {
                    added = this.AddNode(new Vector3(x, y, 0)) == GameController.SUCCESS;
                }
            }
        }

        // Add some edges
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                if (CanAddEdgeBetween(nodes[i], nodes[j]) && Random.value < myParams.BFS.connectionProbability)
                {
                    this.AddEdge(nodes[i], nodes[j]);
                }
            }
        } // finished adding edges

        foreach (NodeScript node in nodes)
        {
            while (node.OutDegree() < myParams.BFS.minDegree)
            {
                
            }
        }
    }

    /// <summary>
    /// Adds a node at the given position, if there's not one there already.
    /// </summary>
    /// <remarks>
    /// Also updates all fields so as to preserve proper state. This is the 
    /// preferred way to add nodes to the graph.</remarks>
    /// 
    /// <param name="where">the position vector where this node should be added</param>
    /// <returns> if a node was sucessfully added</returns>
    private int AddNode(Vector3 where)
    {
        // Don't add anything if there's already a node there
        if (nodeMap.ContainsKey(where) || !IsValidPosition(where))
        {
            return GameController.FAILURE;
        }

        NodeScript node = ((GameObject)Instantiate(myPrefabs.node, where, Quaternion.identity))
            .GetComponent<NodeScript>();
        node.transform.parent = this.transform.Find("Nodes");

        nodes.Add(node);
        nodeMap.Add(where, node);
        return GameController.SUCCESS;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tail"></param>
    /// <param name="head"></param>
    /// <returns></returns>
    private int AddEdge(NodeScript tail, NodeScript head)
    {
        // Don't add an invalid edge
        if (!CanAddEdgeBetween(tail, head))
        {
            return GameController.FAILURE;
        }

        // Make the edge
        EdgeScript edge = ((GameObject)Instantiate(myPrefabs.edge)).GetComponent<EdgeScript>();
        edge.transform.parent = this.transform.Find("Edges");
        edge.Initialize(tail, head, myParams.isDirected);

        // Register edge with its ends
        tail.AddOutNeighbor(head);
        head.AddInNeighbor(tail);
        edges.Add(edge);

        // Signal success
        return GameController.SUCCESS;
    }


    private bool CanAddEdgeBetween(NodeScript tail, NodeScript head)
    {
        // Don't add an edge between null values
        if (tail == null || head == null)
        {
            return false;
        }
        
        // Don't add an edge if they're too far apart
        if ((head.transform.position - tail.transform.position).magnitude > myParams.nodeRadius)
        {
            return false;
        }

        // Don't duplicate an edge
        if (tail.PointsTo(head) || head.PointedToBy(tail))
        {
            return false;
        }

        // Check that this edge won't intersect any other node
        Vector3 displacement = head.transform.position - tail.transform.position;
        RaycastHit[] hits = Physics.RaycastAll(tail.transform.position, displacement);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform != tail.transform && hit.transform != head.transform
                && (hit.transform.position - tail.transform.position).magnitude < displacement.magnitude)
            {
                // There is a node in between tail and head
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returs true if there's node in one of the 26 surrounding areas in the grid (3D)
    /// </summary>
    /// <param name="where"></param>
    /// <returns></returns>
    private int IsAdjacentNode(Vector3 where)
    {
        // Return -1 if there's an error with the where vector
        if (!IsValidPosition(where))
        {
            return GameController.ERROR;
        }
        int wx = (int)Mathf.Round(where.x),
            wy = (int)Mathf.Round(where.y),
            wz = (int)Mathf.Round(where.z);

        // Check all surrounding 26 positions
        int r = myParams.nodeBoundary;
        for (int x = wx - r; x <= wx + r; x++)
        {
            for (int y = wy - r; y <= wy + r; y++)
            {
                for (int z = wz - r; z <= wz + r; z++)
                {
                    // Only check surrounding cube, not the center
                    if (x != wx || y != wy || z != wz)
                    {
                        if (nodeMap.ContainsKey(new Vector3(x, y, z)))
                        {
                            return GameController.TRUE;
                        }
                    }
                }
            }
        }
        return GameController.FALSE;
    }

    /// <summary>
    /// Returns if v corresponds to an integral grid position
    /// </summary>
    /// <param name="v">the vector to check</param>
    /// <returns>true if all components of v are integral</returns>
    private bool IsValidPosition(Vector3 v)
    {
        return IsInteger(v.x) && IsInteger(v.y) && IsInteger(v.z);
    }

    /// <summary>
    /// Returns if this float is basically an integer
    /// </summary>
    /// <param name="f">the float to check</param>
    /// <returns>true if f is integral</returns>
    private bool IsInteger(float f)
    {
        return Mathf.Approximately(f, Mathf.Round(f));
    }
}
