using UnityEngine;
using System.Collections;

public class KonpanionNest : MonoBehaviour {
    public Light PlayerLight;
    public Light HumfClosenEssLight;

	public AudioSource ambient_Audio;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Application.isPlaying && !ambient_Audio.isPlaying)
			ambient_Audio.Play();
	}
}
