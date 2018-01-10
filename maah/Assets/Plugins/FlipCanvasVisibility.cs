using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipCanvasVisibility : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    public void Flip()
    {
        Debug.Log("Flipping");
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            return;
                canvas.enabled = !canvas.enabled; 
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyUp("enter") || Input.GetKeyUp("return"))
        {
            print("space key was pressed");
            Flip();
        }
    }

}

