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

    public GameObject graph;

    // Use this for initialization
    void Start()
    {
        Screen.showCursor = true;
        graph.GetComponent<GraphScript>().Create_BFS();
    }

    // Update is called once per frame
    void Update()
    {
        // If a click, find the proper node
        if (Input.GetButtonDown("Fire1"))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            
            // swap a node we find
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.tag == "Node") {

                    Vector3 hitT = hit.transform.localScale;
                    hit.transform.localScale = new Vector3(-hitT.x, -hitT.y, hitT.z);
                }
            }

        }
    }
}
