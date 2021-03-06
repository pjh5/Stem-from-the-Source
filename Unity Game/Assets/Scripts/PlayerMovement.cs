﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{

    public float speed;
    private static Vector3 startingPos;
    private static Vector3 lastPos;
    private float lastSize;

    private bool showingAll = false;

    private static PlayerMovement instance;

    void Start()
    {
        startingPos = transform.position;
        instance = this.GetComponent<PlayerMovement>();
    }

    /*
     * Move the Camera based on arrow keys
     */
    void FixedUpdate()
    {
        // Disable camera movements in show all mode
        if (!showingAll)
        {
            // Arrow Keys
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(moveHorizontal, moveVertical, 0);
            rigidbody.velocity = movement * speed;
        }
        // ESC
        if (Input.GetButtonDown("Cancel"))
        {
            if (showingAll)
            {
                // Revert to previous state
                transform.position = lastPos;
                Camera.main.orthographicSize = lastSize;
                showingAll = false;
            }
            else
            {
                // Save current position to come back to
                lastPos = transform.position;
                lastSize = Camera.main.orthographicSize;

                // Show all
                ShowAllNodes(Graph.Get().GetNodes());
                showingAll = true;
            }
        }
    }


    public void FocusOn(GameObject obj) 
    {
        lastPos = obj.transform.position + startingPos;

        if (!showingAll)
            transform.position = lastPos;
    }


    public void ShowAllNodes(List<Node> nodes)
    {
        // Find bounds
        float left = 0f, right = 0f, top = 0f, bottom = 0f;
        foreach (Node node in nodes) 
        {
            Vector3 p = node.transform.position;

            left = Mathf.Min(left, p.x);
            right = Mathf.Max(right, p.x);
            top = Mathf.Max(top, p.y);
            bottom = Mathf.Min(bottom, p.y);
        }

        // Center camera in bounds
        transform.position = new Vector3((left + right) / 2, (top + bottom) / 2, -10);

        // Zoom out camera enough
        Camera.main.orthographicSize = 1 + Mathf.Min(Mathf.Abs(left - right), Mathf.Abs(top - bottom)) / 2;
    }

    // singleton method
    public static PlayerMovement Get()
    {
        return instance;
    }
}
