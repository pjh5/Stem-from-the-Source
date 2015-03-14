using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public static readonly int ERROR = -1;
    public static readonly int TRUE = 0;
    public static readonly int SUCCESS = 0;
    public static readonly int FALSE = 1;
    public static readonly int FAILURE = 1;

    private static GameController instance;

    // Use this for initialization
    void Start()
    {
        Screen.showCursor = true;
        instance = this.GetComponent<GameController>();
        GraphScript.Get().MakeGraph();
    }

    // Update is called once per frame
    void Update()
    {
        // Make graph
        if (Input.GetButtonDown("Jump"))
        {
            //GraphScript.Get().MakeGraph();
        }

        // If a click, find the proper node
        bool leftClick;
        if ((leftClick = Input.GetButtonDown("Fire1")) || Input.GetButtonDown("Fire2"))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            
            // if clicked on a node, ignore all edges
            // since this click also hit every edge that is incident with this node
            bool hitNode = false;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject.tag == "Node")
                {
                    GraphScript.Get().Click(hit.collider.gameObject, leftClick);
                    hitNode = true;
                    break;
                }
            }

            // if no nodes, then try to find an edge
            bool hitEdge = false;
            if (!hitNode)
            {
                foreach (RaycastHit hit in hits)
                {
                    GraphScript.Get().Click(hit.collider.gameObject, leftClick);
                    hitEdge = (hit.collider.gameObject.tag == "Edge");
                }
            }

            // Still pass the graph a click, even if no object hit
            if (!hitEdge && !hitNode)
            {
                GraphScript.Get().Click(null, leftClick);
            }

        }
    }


    public static GameController Get()
    {
        return instance;
    }
}
