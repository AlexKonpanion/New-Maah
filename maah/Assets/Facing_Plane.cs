using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Facing_Plane : MonoBehaviour {

    // Use this for initialization
    public Transform player;

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(player.transform);
        Vector3 rot= transform.eulerAngles;
        rot.y += 180;
        rot.x = 0;
        transform.eulerAngles = rot;
    }
}
