using UnityEngine;
using System.Collections;

public class EdgeScript : MonoBehaviour
{

    // Ends
    private NodeScript head;
    private NodeScript tail;

    private bool isDirected;

    /// <summary>
    /// Correctly positions, rotates, and scales this node to connect tail and head
    /// </summary>
    /// <param name="tail"> The tail node</param>
    /// <param name="head"> The head node</param>
    public void Initialize(NodeScript tail, NodeScript head, bool isDirected)
    {
        // Destroy ourselves if we've been initialized incorrectly
        if (tail == null || head == null)
        {
            Destroy(gameObject);
            return;
        }

        this.tail = tail;
        this.head = head;
        this.isDirected = isDirected;

        // Vector from tail to head.
        Vector3 displacement = head.transform.position - tail.transform.position;

        // Position in space
        this.transform.position = tail.transform.position + (displacement / 2);

        // Rotation in space
        this.transform.rotation = Quaternion.FromToRotation(Vector3.up, displacement);

        // Correct scale
        this.transform.localScale = new Vector3(1, displacement.magnitude, 1);
    }

    /// <summary>
    /// Returns the node that this edge points to
    /// </summary>
    /// <returns> the node that this edge points to</returns>
    NodeScript GetHead()
    {
        return head;
    }

    /// <summary>
    /// Returns the node that this edge points from
    /// </summary>
    /// <returns> the node that this edge points from</returns>
    NodeScript GetTail()
    {
        return tail;
    }

}
