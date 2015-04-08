using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Prefabs
{
    public GameObject edge;
    public GameObject node;
}


public class Graph : MonoBehaviour
{
    private static readonly int PATH_MODE = 0;
    private bool graphExists = false;

    // Public Parameters
    public Prefabs myPrefabs;

    // Calculated Parameters
    private int bound;
    private int N;
    private int minDeg;
    private int maxDeg;

    // Aesthetic parameters
    private int maxCrosses;
    private int nodeMoat;
    private float nodeRadius;
    private int xBound;
    private int yBound;

    // Graph State Variables
    private Node source, sink;
    private int mode = PATH_MODE;

    // PATH_MODE Variables
    private List<Node> userPath = new List<Node>();
    private List<Node> fastPath;

    // Node and edge lists
    private List<Node> nodes = new List<Node>();
    private List<Edge> edges = new List<Edge>();

    // Maps
    private Dictionary<Vector3, Node> nodeMap = new Dictionary<Vector3, Node>();
    private Dictionary<NodePair, Edge> edgeBetween = new Dictionary<NodePair, Edge>();

    // Instance for singleton pattern
    private static Graph instance;


    /*
     * GUI METHODS
     */

    public void SetToPathMode()
    {
        mode = PATH_MODE;

        // Initialize userPath
        if (userPath.Count == 0)
        {
            userPath.Add(source);
            source.SetState(1); // left click
        }

        // Color Path
        for (int i = 1; i < userPath.Count; i++)
        {
            EdgeBetween(userPath[i - 1], userPath[i]).SetState(1);
        }

        // Focus on last node in userPath
        PlayerMovement.Get().FocusOn(userPath[userPath.Count - 1].gameObject);
    }


    public void Click(GameObject gObj, bool left)
    {
        if (mode == PATH_MODE)
        {
            if (left)
            {
                AddToPath(gObj);
            }
            else
            {
                BacktrackPath();
            }
        }
    }


    public void AddToPath(GameObject gObj)
    {
        if (gObj == null)
            return;

        // If userPath is empty, then just add gObj as only element
        if (userPath.Count == 0)
        {
            if (gObj.tag == "Node")
            {
                Node node = gObj.GetComponent<Node>();
                userPath.Add(node);
            }
            else
                return;
        }
        Node next = null, pathEnd = userPath[userPath.Count - 1];

        // If edge, find the other side (or null if not incidnet to userPath end)
        if (gObj.tag == "Edge")
        {
            Edge edge = gObj.GetComponent<Edge>();
            next = edge.OtherEnd(pathEnd); // null if edge not incident to pathEnd
        }
        // if node, just check if incident to userPath end
        else if (gObj.tag == "Node")
        {
            next = gObj.GetComponent<Node>();
        }

        // Not an edge or node, or not incident to userPath end
        if (next == null || !next.IncidentTo(pathEnd) || userPath.Contains(next))
            return;

        // Color next edge
        EdgeBetween(pathEnd, next).SetState(1);

        // Add next to userPath 
        next.SetState(1);
        userPath.Add(next);

        // Focus camera on next
        PlayerMovement.Get().FocusOn(next.gameObject);
    }

    public void BacktrackPath()
    {
        if (userPath.Count == 1)
        {
            return;
        }

        // Save last node
        Node removed = userPath[userPath.Count - 1];

        // Remove last node
        if (!removed.Equals(sink))
            removed.SetState(0);
        userPath.RemoveAt(userPath.Count - 1);

        // Uncolor last edge
        EdgeBetween(removed, userPath[userPath.Count - 1]).SetState(0);

        // Refocus Camera on userPath end
        PlayerMovement.Get().FocusOn(userPath[userPath.Count - 1].gameObject);
    }

    /*
     * GETTERS
     */

    public static Graph Get()
    {
        return instance;
    }


    public List<Node> GetNodes()
    {
        return nodes;
    }


    public Edge EdgeBetween(Node n1, Node n2)
    {
        NodePair np = new NodePair(n1, n2);
        return edgeBetween.ContainsKey(np) ? edgeBetween[np] : null;
    }


    public int PathLength()
    {
        return InPathMode() ? userPath.Count - 1 : 0;
    }


    public bool InPathMode()
    {
        return mode == PATH_MODE;
    }


    public bool PathFinished()
    {
        return InPathMode() && userPath.Count > 0 && userPath[userPath.Count - 1].Equals(sink);
    }


    public bool Exists()
    {
        return graphExists;
    }

    /*
     * GRAPH ALGORITHIMS
     */

    void Start()
    {
        instance = this.GetComponent<Graph>();
    }


    /// <summary>
    /// Returns a list mapping a node to its distance from source, where -1 = inf
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    private void RunBFS()
    {
        // Set up queue
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(source);
        source.SetDistance(0);

        // BFS
        while (queue.Count > 0)
        {
            Node node = queue.Dequeue();
            int dist = node.GetDistance() + 1;

            foreach (Node nbr in node.Neighbors())
            {
                if (nbr.GetDistance() == -1)
                {
                    nbr.SetDistance(dist);
                    queue.Enqueue(nbr);
                }
            }
        }

        // Set fast path
        FindFastPath();
    }

    public List<Node> GetFastestPath()
    {
        return fastPath;
    }

    public void FindFastPath()
    {
        List<Node> path = new List<Node>();
        path.Add(sink);

        Node last = sink;
        while (last != source)
        {
            IEnumerable<Node> rands = last.Neighbors().OrderBy(i => Random.value);
            foreach (Node nbr in rands)
            {
                if (nbr.GetDistance() < last.GetDistance())
                {
                    last = nbr;
                    path.Add(nbr);
                    break;
                }
            }
        }

        fastPath = path;
    }

    public int GetShortestDistance()
    {
        return sink.GetDistance();
    }

    /*
     * GRAPH CONSTRUCTION METHODS
     */

    public void Initialize(Parameters p)
    {
        if (graphExists) return;

        this.N = p.GetNumNodes();
        this.minDeg = p.GetMinDegree();
        this.maxDeg = p.GetMaxDegree();

        // Aesthetics
        this.maxCrosses = p.GetMaxCrosses();
        this.nodeMoat = p.GetNodeMoat();
        this.nodeRadius = p.GetNodeRadius();
        this.xBound = p.GetxBound();
        this.yBound = p.GetyBound();

        // Canvas hacking
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, xBound);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, yBound);
    }

    public void MakeGraph()
    {
        // Don't mess things up
        if (graphExists) return;

        // Add all nodes
        for (int n = 0; n < N; n++)
        {
            bool added = false;
            while (!added)
            {
                // Pick a random place to put the node
                int x = Random.Range(0, xBound);
                int y = Random.Range(0, yBound);

                if (IsAdjacentNode(new Vector3(x, y, 0)) == GameController.FALSE)
                {
                    added = (this.AddNode(new Vector3(x, y, 0)) == GameController.SUCCESS);
                }
            }
        }

        // Add edges
        foreach (Node node in nodes)
        {
            List<Node> posNbrs = PotentialNeighbors(node);

            // Pick that many random neighbors
            int curMax = Mathf.Min(posNbrs.Count, maxDeg);
            int nbrs = Mathf.Min(posNbrs.Count, Random.Range(minDeg, curMax));
            for (int i = 0; i < nbrs; i++)
            {
                int choice = Mathf.RoundToInt(Random.Range(0, posNbrs.Count));
                AddEdge(node, posNbrs[choice]);
                posNbrs.RemoveAt(choice);
            }
        }

        // Choose start and ending nodes
        source = nodes[0];
        sink = nodes[0];
        foreach (Node node in nodes)
        {
            float pos = node.transform.position.x + node.transform.position.y;
            if (pos < source.transform.position.x + source.transform.position.y)
            {
                source = node;
            }
            if (pos > sink.transform.position.x + sink.transform.position.y)
            {
                sink = node;
            }
        }
        source.SetState(1);
        sink.SetState(1);

        // Initialize to PATH_MODE
        SetToPathMode();
        RunBFS();

        graphExists = true;
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
            return GameController.FAILURE;

        // Make node
        Node node = ((GameObject)Instantiate(myPrefabs.node, where, Quaternion.identity))
            .GetComponent<Node>();
        node.transform.parent = this.transform.Find("Nodes");

        // Update state to keep track of new node
        nodes.Add(node);
        nodeMap.Add(where, node);

        // Signal success
        return GameController.SUCCESS;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tail"></param>
    /// <param name="head"></param>
    /// <returns></returns>
    private int AddEdge(Node tail, Node head)
    {
        // Make the edge
        Edge edge = ((GameObject)Instantiate(myPrefabs.edge)).GetComponent<Edge>();
        edge.transform.parent = this.transform.Find("Edges");
        edge.Initialize(tail, head);

        // Register edge with its ends
        tail.AddNeighbor(head);
        head.AddNeighbor(tail);
        edges.Add(edge);

        // Keep track of edge in map
        edgeBetween[new NodePair(tail, head)] = edge;

        // Signal success
        return GameController.SUCCESS;
    }


    private List<Node> PotentialNeighbors(Node node)
    {
        List<Node> possibilities = new List<Node>();
        foreach (Node nbr in nodes)
        {
            if (CanAddEdgeBetween(node, nbr))
            {
                possibilities.Add(nbr);
            }
        }
        return possibilities;
    }


    public void ClearGraph()
    {
        // Have to manually delete all nodes and edges, cause they have GUIs
        nodes.ForEach((node) => Destroy(node.gameObject));
        edges.ForEach((edge) => Destroy(edge.gameObject));
        nodes.Clear();
        edges.Clear();

        // Path variables
        userPath.Clear();
        source = null;
        sink = null;

        // Maps
        nodeMap.Clear();
        edgeBetween.Clear();

        graphExists = false;
    }

    /*
     * METHODS FOR VALID NODE AND EDGE PLACEMENT
     */

    private bool CanAddEdgeBetween(Node tail, Node head)
    {
        // Don't add an edge between null values
        if (tail.Equals(head))
            return false;

        // Don't add an edge to a node that's already full
        if (tail.Degree() >= maxDeg || head.Degree() >= maxDeg)
            return false;

        // Don't add an edge if they're too far apart
        if ((head.transform.position - tail.transform.position).magnitude > nodeRadius)
            return false;

        // Don't duplicate an edge
        if (tail.IncidentTo(head))
            return false;

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
                return false;

            // Watch for crossings if maxCrossings is not -1 (unlimited)
            else if (maxCrosses != -1 && hit.collider.tag == "Edge")
            {
                Edge edge = hit.transform.gameObject.GetComponent<Edge>();

                // Ignore an edge that's being crossed at an endpoint
                if (edge.IncidentTo(head) || edge.IncidentTo(tail))
                    continue;

                // Keep track of this edge's crossings
                if (++numCrossings > maxCrosses)
                    return false;

                // Don't cross an edge that's already been crossed too many times
                if (hit.transform.gameObject.GetComponent<Edge>().NumCrossings() == maxCrosses)
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
        int wx = Mathf.RoundToInt(where.x),
            wy = Mathf.RoundToInt(where.y);

        // Check all surrounding 26 positions
        int r = nodeMoat;
        for (int x = wx - r; x <= wx + r; x++)
        {
            for (int y = wy - r; y <= wy + r; y++)
            {
                // Only check surrounding cube, not the center
                if (x != wx || y != wy)
                {
                    if (nodeMap.ContainsKey(new Vector3(x, y, 0)))
                    {
                        return GameController.TRUE;
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

    // Used to map two nodes to the edge between them
    private class NodePair
    {
        public readonly Node n1;
        public readonly Node n2;

        public NodePair(Node n1, Node n2)
        {
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
            return (this.n1.Equals(np.n1) && this.n2.Equals(np.n2)) ||
                (this.n1.Equals(np.n2) && this.n2.Equals(np.n1));
        }

        // Note that this is symmetric
        public override int GetHashCode()
        {
            return n1.GetHashCode() + n2.GetHashCode();
        }
    }
}
