using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeScript : MonoBehaviour {

    public Sprite[] sprites;
    private int state = 0;

    private List<NodeScript> inNeighbors = new List<NodeScript>();
    private List<NodeScript> outNeighbors = new List<NodeScript>();

    // Manipulated by GraphScript functions
    private int distance = -1;


    public void Initialize()
    {
    }


    // Please Generalize This
    public void SetState(int state)
    {
        this.state = state;
        gameObject.GetComponent<SpriteRenderer>().sprite = sprites[state];
    }


    public void SetDistance(int distance)
    {
        this.distance = distance;
    }


    public int State()
    {
        return state;
    }


    public int Distance()
    {
        return distance;
    }


    public int Degree()
    {
        return inNeighbors.Count + outNeighbors.Count;
    }


    public int OutDegree()
    {
        return outNeighbors.Count;
    }


    public bool PointsTo(NodeScript node)
    {
        return outNeighbors.Contains(node);
    }


    public bool PointedToBy(NodeScript node)
    {
        return inNeighbors.Contains(node);
    }


    public bool IncidentTo(NodeScript node)
    {
        return this.PointsTo(node) || this.PointedToBy(node);
    }


    public List<NodeScript> Neighbors()
    {
        List<NodeScript> neighbors = new List<NodeScript>();
        neighbors.AddRange(inNeighbors);
        neighbors.AddRange(outNeighbors);
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
        if (head == null || outNeighbors.Contains(head))
        {
            return GameController.FALSE;
        }

        outNeighbors.Add(head);
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
        if (tail == null || inNeighbors.Contains(tail))
        {
            return GameController.FALSE;
        }

        inNeighbors.Add(tail);
        return GameController.TRUE;
    }

    /// <summary>
    /// Deregisters node as an inneighbor of this
    /// </summary>
    /// <param name="node">the node to remove from the list of inneighbors</param>
    /// <returns>whether the node was in inneighbors</returns>
    public int RemoveInNeighbor(NodeScript node) {
        return inNeighbors.Remove(node)? GameController.TRUE : GameController.FALSE;
    }

    /// <summary>
    /// Deregisters node as an outneighbor of this
    /// </summary>
    /// <param name="node">the node to remove from the list of outneighbors</param>
    /// <returns>whether the node was in outneighbors</returns>
    public int RemoveOutNeighbor(NodeScript node) {
        return outNeighbors.Remove(node)? GameController.TRUE : GameController.FALSE;
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
