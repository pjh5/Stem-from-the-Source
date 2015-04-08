using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

public class GameController : MonoBehaviour
{
    public static readonly int ERROR = -1;
    public static readonly int TRUE = 0;
    public static readonly int SUCCESS = 0;
    public static readonly int FALSE = 1;
    public static readonly int FAILURE = 1;

    public Button solutionButton;
    public Text solutionText;

    // Use this for initialization
    void Start()
    {
        Screen.showCursor = true;
    }

    // Update is called once per frame
    void Update()
    {

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
                    Graph.Get().Click(hit.collider.gameObject, leftClick);
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
                    Graph.Get().Click(hit.collider.gameObject, leftClick);
                    hitEdge = (hit.collider.gameObject.tag == "Edge");
                }
            }

            // Still pass the graph a click, even if no object hit
            if (!hitEdge && !hitNode)
            {
                Graph.Get().Click(null, leftClick);
            }

        }

        // Activate possible solutions
        if (Graph.Get().Exists())
        {
            //solutionButton.gameObject.SetActive(Graph.Get().PathFinished());
            //solutionText.gameObject.SetActive(Graph.Get().PathFinished());
            solutionText.text = "Shortest possible userPath length: " + Graph.Get().GetShortestDistance();
        }
    }


    public void TraceOutFastestPath()
    {
        // Erase old path
        List<Node> path = Graph.Get().GetFastestPath();
        for (int i = 1; i < path.Count; i++)
        {
            Graph.Get().EdgeBetween(path[i - 1], path[i]).RevertState();
        }

        Graph.Get().FindFastPath();
        path = Graph.Get().GetFastestPath();

        for (int i = 1; i < path.Count; i++)
        {
            Graph.Get().EdgeBetween(path[i - 1], path[i]).SetState(2);
            //userPath[i].SetState(2);
        }
    }


    public void MakeGraph()
    {
        // Delete old graph
        Graph.Get().ClearGraph();

        // Make new graph
        StartCoroutine(WaitThenDo(delegate
        {
            Parameters p = Parameters.Get();
            Graph.Get().Initialize(p);
            Graph.Get().MakeGraph();
            p.Close();
        }));
    }


    private IEnumerator WaitThenDo(Action doThis)
    {
        yield return 0;

        doThis.Invoke();
    }
}
