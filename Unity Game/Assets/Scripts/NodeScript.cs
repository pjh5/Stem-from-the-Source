using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeScript : MonoBehaviour {

    // GUI
    public Sprite[] sprites;
    private int state = 0;

    // Info
    private List<NodeScript> inNbrs = new List<NodeScript>();
    private List<NodeScript> outNbrs = new List<NodeScript>();

    // Index
    private int index; // better be unique
    private static List<int> usedIndices = new List<int>();

    public int Initialize(int index)
    {
        // Prevent duplicate indices
        if (usedIndices.Contains(index))
        {
            Destroy(this.gameObject);
            return GameController.FAILURE;
        }

        this.index = index;
        return GameController.SUCCESS;
    }

    public int Index()
    {
        return index;
    }

    // Please Generalize This
    public void SetState(int state)
    {
        this.state = state;
        gameObject.GetComponent<SpriteRenderer>().sprite = sprites[state];
    }

    /// <summary>
    /// Returns which state the node is in
    /// </summary>
    /// <returns>the state of the node</returns>
    public int State()
    {
        return state;
    }

    /// <summary>
    /// Returns the out degree if directed, total degree otherwise
    /// </summary>
    /// <returns>the out degree if directed, total degree otherwise</returns>
    public int Degree()
    {
        return GraphScript.Get().IsDirected()? 
            outNbrs.Count : inNbrs.Count + outNbrs.Count;
    }


    public bool PointsTo(NodeScript node)
    {
        return GraphScript.Get().IsDirected()? 
            outNbrs.Contains(node) : outNbrs.Contains(node) || inNbrs.Contains(node);
    }


    public bool PointedToBy(NodeScript node)
    {
        return inNbrs.Contains(node);
    }


    public bool IncidentTo(NodeScript node)
    {
        return this.PointsTo(node) || this.PointedToBy(node);
    }


    public List<NodeScript> Neighbors()
    {
        List<NodeScript> neighbors = new List<NodeScript>();
        neighbors.AddRange(inNbrs);
        neighbors.AddRange(outNbrs);
        return neighbors;
    }

    /// <summary>
    /// Adds head as an outneighbor of this node, if its not already one
    /// </summary>
    /// <param name="head">the node to add to the outneighbors</param>
    /// <returns>if head wasn't already an outneighbor</returns>
    public int AddOutNeighbor(NodeScript head)
    {
        // Don't allow multiple edges
        if (head == null || outNbrs.Contains(head))
        {
            return GameController.FALSE;
        }

        outNbrs.Add(head);
        return GameController.TRUE;
    }

    /// <summary>
    /// Adds tail as an inneighbor of this node, if its not already one
    /// </summary>
    /// <param name="tail">the node to add as pointing to this node</param>
    /// <returns>if tail wasn't already an inneighbor</returns>
    public int AddInNeighbor(NodeScript tail)
    {
        // Don't allow multiple edges
        if (tail == null || inNbrs.Contains(tail))
        {
            return GameController.FALSE;
        }

        inNbrs.Add(tail);
        return GameController.TRUE;
    }

    /// <summary>
    /// Deregisters node as an inneighbor of this
    /// </summary>
    /// <param name="node">the node to remove from the list of inneighbors</param>
    /// <returns>whether the node was in inneighbors</returns>
    public int RemoveInNeighbor(NodeScript node) {
        return inNbrs.Remove(node)? GameController.TRUE : GameController.FALSE;
    }

    /// <summary>
    /// Deregisters node as an outneighbor of this
    /// </summary>
    /// <param name="node">the node to remove from the list of outneighbors</param>
    /// <returns>whether the node was in outneighbors</returns>
    public int RemoveOutNeighbor(NodeScript node) {
        return outNbrs.Remove(node)? GameController.TRUE : GameController.FALSE;
    }


    private GraphScript GetGraph()
    {
        return transform.parent.transform.parent.GetComponent<GraphScript>();
    }


    public override bool Equals(System.Object obj)
    {
        return obj != null && this.Equals(obj as NodeScript);
    }


    public bool Equals(NodeScript node)
    {
        return this.gameObject.Equals(node.gameObject);
    }


    public override int GetHashCode()
    {
        Vector3 t = this.gameObject.transform.position;
        return Mathf.RoundToInt(31*Mathf.Abs(t.x) + 15*Mathf.Abs(t.y) + 7*Mathf.Abs(t.z));
    }

}
