using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{

    public float speed;


    /*
     * Move the Camera based on arrow keys
     */
    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, moveVertical, 0);
        rigidbody.velocity = movement * speed;

        if (Input.GetButtonDown("Submit"))
        {
            Camera.main.orthographicSize--;
        }

        if (Input.GetButtonDown("Cancel"))
        {
            Camera.main.orthographicSize++;
        }
    }
}
