﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public static readonly int ERROR = -1;
    public static readonly int TRUE = 0;
    public static readonly int SUCCESS = 0;
    public static readonly int FALSE = 1;
    public static readonly int FAILURE = 1;

    public GameObject graphGameObject;
    private GraphScript graph;

    // Use this for initialization
    void Start()
    {
        Screen.showCursor = true;
        graph = graphGameObject.GetComponent<GraphScript>();
        graph.MakeGraph();
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
                    graph.Click(hit.collider.gameObject, leftClick);
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
                    graph.Click(hit.collider.gameObject, leftClick);
                    hitEdge = (hit.collider.gameObject.tag == "Edge");
                }
            }

            // Still pass the graph a click, even if no object hit
            if (!hitEdge && !hitNode)
            {
                graph.Click(null, leftClick);
            }

        }
    }
}
