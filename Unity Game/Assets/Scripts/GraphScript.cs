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
public class GraphParameters
{
    public int N;
    public bool isDirected;

    public float nodeRadius;
    public int nodeMoat;
    public int spread;

    // between 0 and 1 as percentage
    public float minDeg;
    public float maxDeg; // -1 for unlimited

    public int maxCrossings; // -1 for unlimited
}


public class GraphScript : MonoBehaviour {
    private static readonly int MIN_RADIUS = 3;

    // Public Parameters
    public Prefabs myPrefabs;
    public GraphParameters P;
    
    // Calculated Parameters
    private int bound;
    private float nodeRadius;
    private int minDeg;
    private int maxDeg;

    private List<NodeScript> nodes = new List<NodeScript>();
    private List<EdgeScript> edges = new List<EdgeScript>();

    /// <summary>
    /// Map from a position vector to the node at that point </summary>
    private Dictionary<Vector3, NodeScript> nodeMap = new Dictionary<Vector3, NodeScript>();
    private Dictionary<NodePair, EdgeScript> edgeBetween = new Dictionary<NodePair, EdgeScript>();

    // For BFS
    private NodeScript source, sink;

    /*
     * GETTERS
     */

    public EdgeScript EdgeBetween(NodeScript n1, NodeScript n2)
    {
        NodePair np = new NodePair(n1, n2);
        return edgeBetween.ContainsKey(np) ? edgeBetween[np] : null;
    }


    /*
     * GRAPH ALGORITHIMS
     */

    public void SetNodeDistancesFrom(NodeScript source)
    {
        Queue<NodeScript> queue = new Queue<NodeScript>();
        queue.Enqueue(source);
        source.SetDistance(0);

        // BFS
        while (queue.Count > 0)
        {
            NodeScript node = queue.Dequeue();
            foreach (NodeScript nbr in node.Neighbors())
            {
                if (nbr.Distance() < 0)
                {
                    nbr.SetDistance(node.Distance() + 1);
                    queue.Enqueue(nbr);
                }
            }
        }
    }


    public void ColorForBFS()
    {
        foreach (EdgeScript e in edges)
        {
            int diff = e.GetTail().Distance() - e.GetHead().Distance();

            if (diff == 0)
            {
                e.SetState(0);
            }
            else
            {
                NodeScript first = (e.GetTail().Distance() < e.GetHead().Distance()) 
                    ? e.GetTail() : e.GetHead();
                e.SetState((first.Distance() % 2 == 0) ? 1 : 2);
            }
        }
    }

    /*
     * GRAPH CONSTRUCTION METHODS
     */

    public void ValidateParameters()
    {
        // Make sure nodeMoat makes sense
        P.nodeMoat = (P.nodeMoat < 1) ? 1 : P.nodeMoat;

        // Calculate the size of the game boundary
        this.bound = Mathf.RoundToInt(Mathf.Ceil(
            (Mathf.Sqrt(P.N) * (2 * P.nodeMoat + 1)) * Mathf.Max(P.spread, 1)
            ));

        // Convert percentages to ints
        this.nodeRadius = (0f < P.nodeRadius && P.nodeRadius < 1f) ? P.nodeRadius * 2 * bound : P.nodeRadius;
        this.minDeg = (0f < P.minDeg && P.minDeg < 1f) ? Mathf.RoundToInt(P.minDeg * P.N) : Mathf.RoundToInt(P.minDeg);
        this.maxDeg = (0f < P.maxDeg && P.maxDeg < 1f) ? Mathf.RoundToInt(P.maxDeg * P.N) : Mathf.RoundToInt(P.maxDeg);

        // Make sure nodeRadius isn't prohibitively small
        this.nodeRadius = (nodeRadius > 0) ? Mathf.Max(nodeRadius, MIN_RADIUS) : nodeRadius;

        // Make sure maxDeg > minDeg (by making it unlimited otherwise)
        if (this.minDeg > this.maxDeg)
        {
            this.maxDeg = -1;
        }
    }

    public void Create_BFS()
    {
        ValidateParameters();

        // Add all nodes
        for (int n = 0; n < P.N; n++)
        {
            int numTries = 0;
            bool added = false;
            while (!added)
            {
                // if it's hard to place the node, make the bounds bigger
                if (++numTries >= 20)
                {
                    bound = Mathf.RoundToInt(1.2f * bound);
                    numTries = 0;
                }

                int x = Mathf.RoundToInt(Random.Range(-bound, bound));
                int y = Mathf.RoundToInt(Random.Range(-bound, bound));

                if (IsAdjacentNode(new Vector3(x, y, 0)) == GameController.FALSE)
                {
                    added = (this.AddNode(new Vector3(x, y, 0)) == GameController.SUCCESS);
                }
            }
        }

        // Add edges
        foreach (NodeScript node in nodes)
        {
            List<NodeScript> posNbrs = PotentialNeighbors(node);

            // Calculate number of neighbors to pick for this node
            int curMaxDeg = (maxDeg == -1) ? posNbrs.Count : maxDeg + 1;
            int nNbrs = Mathf.FloorToInt(Mathf.Min(posNbrs.Count, 
                Random.Range(minDeg, curMaxDeg)));

            // Pick that many random neighbors
            for (int i = 0; i < nNbrs; i++)
            {
                int choice = Mathf.RoundToInt(Random.Range(0, posNbrs.Count));
                AddEdge(node, posNbrs[choice]);
                posNbrs.RemoveAt(choice);
            }
        }

        // Choose start and ending nodes
        source = nodes[0];
        sink = nodes[0];
        foreach (NodeScript node in nodes)
        {
            float pos = node.transform.position.x + source.transform.position.y;
            if (pos < source.transform.position.x + source.transform.position.y)
            {
                source = node;
            }
            if (pos > sink.transform.position.x + sink.transform.position.y)
            {
                sink = node;
            }
        }

        // TEST BFS CODE
        SetNodeDistancesFrom(source);
        ColorForBFS();
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
        node.Initialize();

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
        // Make the edge
        EdgeScript edge = ((GameObject)Instantiate(myPrefabs.edge)).GetComponent<EdgeScript>();
        edge.transform.parent = this.transform.Find("Edges");
        edge.Initialize(tail, head, P.isDirected);

        // Register edge with its ends
        tail.AddOutNeighbor(head);
        head.AddInNeighbor(tail);
        edges.Add(edge);

        // Keep track of edge in map
        NodePair np = new NodePair(tail, head);
        edgeBetween[np] = edge;

        // Signal success
        return GameController.SUCCESS;
    }


    private List<NodeScript> PotentialNeighbors(NodeScript node)
    {
        List<NodeScript> possibilities = new List<NodeScript>();
        foreach (NodeScript nbr in nodes) {
            if (!nbr.Equals(node) && CanAddEdgeBetween(node, nbr)) {
                possibilities.Add(nbr);
            }
        }
        return possibilities;
    }

    /*
     * METHODS FOR VALID NODE AND EDGE PLACEMENT
     */

    private bool CanAddEdgeBetween(NodeScript tail, NodeScript head)
    {
        // Don't add an edge between null values
        if (tail == null || head == null)
        {
            return false;
        }

        // Don't add an edge to a node that's already full
        if (maxDeg >= 1 && (tail.Degree() >= maxDeg || head.Degree() >= maxDeg))
        {
            return false;
        }
        
        // Don't add an edge if they're too far apart
        if ((Mathf.Approximately(nodeRadius, MIN_RADIUS) || nodeRadius >= MIN_RADIUS) && 
            (head.transform.position - tail.transform.position).magnitude > nodeRadius)
        {
            return false;
        }

        // Don't duplicate an edge
        if (tail.PointsTo(head) || tail.PointedToBy(head))
        {
            return false;
        }

        // Check that this edge won't intersect any other node, and count edge crossings
        int numCrossings = 0;
        Vector3 displacement = head.transform.position - tail.transform.position;
        RaycastHit[] hits = Physics.RaycastAll(tail.transform.position, displacement, displacement.magnitude);
        foreach (RaycastHit hit in hits)
        {
            // Don't add an edge if it'll intersect another node
            if (hit.collider.tag == "Node" 
                && hit.transform != tail.transform && hit.transform != head.transform
                && (hit.transform.position - tail.transform.position).magnitude < displacement.magnitude)
            {
                // There is a node in between tail and head
                return false;
            }

            // Watch for crossings if maxCrossings is not -1 (unlimited)
            else if (P.maxCrossings != -1 && hit.collider.tag == "Edge")
            {
                EdgeScript edge = hit.transform.gameObject.GetComponent<EdgeScript>();

                // Ignore an edge that's being crossed at an endpoint
                if (edge.IncidentTo(head) || edge.IncidentTo(tail))
                {
                    continue;
                }
                
                // Keep track of this edge's crossings
                if (++numCrossings > P.maxCrossings)
                {
                    return false;
                }

                // Don't cross an edge that's already been crossed too many times
                if (hit.transform.gameObject.GetComponent<EdgeScript>().NumCrossings() == P.maxCrossings)
                {
                    return false;
                }
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
        int r = P.nodeMoat;
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

    /*
     * UTILITY HELPER FUNCTIONS
     */

    /// <summary>
    /// Returns if this float is basically an integer
    /// </summary>
    /// <param name="f">the float to check</param>
    /// <returns>true if f is integral</returns>
    private bool IsInteger(float f)
    {
        return Mathf.Approximately(f, Mathf.Round(f));
    }


    private class NodePair
    {
        public readonly NodeScript n1;
        public readonly NodeScript n2;

        public NodePair(NodeScript n1, NodeScript n2) {
            this.n1 = n1;
            this.n2 = n2;
        }


        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false; // Clearly not equal
            }
            return this.Equals(obj as NodePair);
        }


        public bool Equals(NodePair np)
        {
            if (np == null)
            {
                return false;
            }
            return (this.n1 == np.n1 && this.n2 == np.n2) || 
                (this.n1 == np.n2 && this.n2 == np.n1);
        }

        // Note that this is symmetric
        public override int GetHashCode()
        {
            return n1.GetHashCode() + n2.GetHashCode();
        }
    }
}
