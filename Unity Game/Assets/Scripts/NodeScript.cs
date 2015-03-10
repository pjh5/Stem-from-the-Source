using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeScript : MonoBehaviour {

    private List<NodeScript> inNeighbors = new List<NodeScript>();
    private List<NodeScript> outNeighbors = new List<NodeScript>();

    private static readonly int NUM_STATES = 4;
    private int state = 0;

    // Manipulated by GraphScript functions
    private int distance = -1;

    // Original Vector
    private Vector3 baseL;


    public void Initialize()
    {
        Vector3 scale = transform.localScale;
        baseL = new Vector3(scale.x, scale.y, scale.z);
    }


    public void LeftClick()
    {
        ChangeState(1);

    }


    public void RightClick()
    {
        ChangeState(-1);
    }

    // Please Generalize This
    public void ChangeState(int byHowMuch)
    {
        state = Mathf.RoundToInt(Mathf.Repeat(state + byHowMuch, NUM_STATES));

        // Change image
        Vector3 newScale = new Vector3(
            (state > 1) ? -baseL.x : baseL.x, 
            (state == 1 || state == 2)? -baseL.y : baseL.y, 
            baseL.z);
        transform.localScale = newScale;

        // Activate surrounding edges
        GraphScript graph = GetGraph();
        foreach (NodeScript nbr in Neighbors())
        {
            if (this.state > 0 && nbr.State() > 0)
            {
                graph.EdgeBetween(this, nbr).SetState(1);
            }
            else
            {
                graph.EdgeBetween(this, nbr).SetState(0);
            }
        }
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
}
