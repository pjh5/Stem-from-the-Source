using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeScript : MonoBehaviour {

    private List<NodeScript> inNeighbors = new List<NodeScript>();
    private List<NodeScript> outNeighbors = new List<NodeScript>();


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
}
