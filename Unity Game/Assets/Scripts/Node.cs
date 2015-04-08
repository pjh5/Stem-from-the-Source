using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node : MonoBehaviour {

    // GUI
    public Sprite[] sprites;
    private int state = 0;
    private int prevState = 0;

    // Info
    private List<Node> neighbors = new List<Node>();

    // BFS specific
    private int distance = -1;

    /*
     * SETTERS
     */

    // Please Generalize This
    public void SetState(int state)
    {
        prevState = this.state;
        this.state = state;
        gameObject.GetComponent<SpriteRenderer>().sprite = sprites[state];
    }

    public void RevertState()
    {
        SetState(prevState);
    }

    public void SetDistance(int distance)
    {
        this.distance = distance;
    }

    public int AddNeighbor(Node node)
    {
        if (neighbors.Contains(node))
        {
            return GameController.FAILURE;
        }
        neighbors.Add(node);
        return GameController.SUCCESS;
    }


    /*
     * GETTERS
     */
    public int State()
    {
        return state;
    }

    public int Degree()
    {
        return neighbors.Count;
    }

    public bool IncidentTo(Node node)
    {
        return neighbors.Contains(node);
    }

    public List<Node> Neighbors()
    {
        return neighbors;
    }

    public int GetDistance()
    {
        return distance;
    }

    /*
     * EQUALS
     */
    public override bool Equals(System.Object obj)
    {
        return obj != null && this.Equals(obj as Node);
    }

    public bool Equals(Node node)
    {
        return this.gameObject.Equals(node.gameObject);
    }

    public override int GetHashCode()
    {
        Vector3 t = this.gameObject.transform.position;
        return Mathf.RoundToInt(31*Mathf.Abs(t.x) + 15*Mathf.Abs(t.y) + 7*Mathf.Abs(t.z));
    }

}
