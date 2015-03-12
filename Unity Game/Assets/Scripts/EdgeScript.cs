using UnityEngine;
using System.Collections;

public class EdgeScript : MonoBehaviour
{
    public Sprite[] sprites;
    public int state = 0;

    // Ends
    private NodeScript head;
    private NodeScript tail;

    private int numCrossings = 0;

    /// <summary>
    /// Correctly positions, rotates, and scales this node to connect tail and head
    /// </summary>
    /// <param name="tail"> The tail node</param>
    /// <param name="head"> The head node</param>
    public void Initialize(NodeScript tail, NodeScript head)
    {
        // Destroy ourselves if we've been initialized incorrectly
        if (tail == null || head == null)
        {
            Destroy(gameObject);
            return;
        }

        this.tail = tail;
        this.head = head;

        // Set proper sprite
        SetState(0); // default state

        // Vector from tail to head.
        Vector3 displacement = head.transform.position - tail.transform.position;

        // Position in space
        this.transform.position = tail.transform.position + (displacement / 2);

        // Rotation in space
        this.transform.rotation = Quaternion.FromToRotation(Vector3.up, displacement);

        // Correct scale
        this.transform.localScale = new Vector3(1, displacement.magnitude, 1);

        // Count number of crossings
        RaycastHit[] hits = Physics.RaycastAll(tail.transform.position, displacement, displacement.magnitude);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.tag == "Edge")
            {
                EdgeScript e = hit.transform.gameObject.GetComponent<EdgeScript>();

                if (!e.IncidentTo(this.GetHead()) && !e.IncidentTo(this.GetTail()))
                {
                    this.Cross();
                    e.Cross();
                }
                   
            }
        }
    }


    public void SetState(int state)
    {
        this.state = state;
        gameObject.GetComponent<SpriteRenderer>().sprite = sprites[state];
    }

    /// <summary>
    /// Returns the node that this edge points to
    /// </summary>
    /// <returns> the node that this edge points to</returns>
    public NodeScript GetHead()
    {
        return head;
    }

    /// <summary>
    /// Returns the node that this edge points from
    /// </summary>
    /// <returns> the node that this edge points from</returns>
    public NodeScript GetTail()
    {
        return tail;
    }


    public NodeScript OtherEnd(NodeScript node)
    {
        if (node == null || !(node.Equals(GetHead()) || node.Equals(GetTail())))
        {
            return null;
        }

        return (node.Equals(GetHead())) ? GetTail() : GetHead();
    }


    public bool IncidentTo(NodeScript node)
    {
        return GetHead().Equals(node) || GetTail().Equals(node);
    }


    public int NumCrossings()
    {
        return numCrossings;
    }


    public void Cross()
    {
        numCrossings++;
    }


    public void UnCross()
    {
        numCrossings--;
    }

}
