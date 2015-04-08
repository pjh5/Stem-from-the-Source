using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UpdatePathLength : MonoBehaviour {

    private Text text;

	// Use this for initialization
	void Start () 
    {
        text = gameObject.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        int length = Graph.Get().PathLength();
        text.text = "Path length: " + ((length == -1) ? "--" : length.ToString());
	}
}
