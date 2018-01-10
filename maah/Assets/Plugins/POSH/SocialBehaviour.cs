using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game;
using ReAct.unity;
using ReAct.sys;

public class SocialBehaviour : KonpanionBehaviour {

	private SphereCollider col;                         // Reference to the sphere collider trigger component.

	public Dictionary<string,RampActivation> ergos;
	public AudioSource greeting;

	// Use this for initialization
	void Start () {
		BehaviourTitle = "social";

	}

	void Awake() {
		base.Init ();

		col = GetComponent<SphereCollider> ();
		anim = GetComponent<Animator> ();
		hash = GameObject.FindGameObjectWithTag (GameTags.gameController).GetComponent<HashIDs> ();
	}

	internal bool WantsAttention()
	{
		return false;
	}

	internal void GreetOwner()
	{
		Loom.QueueOnMainThread(() =>
			{
				if (greeting != null)
					greeting.Play();
			});

	}




	
	// Update is called once per frame
	void Update () {
		/*	if (alive)
			{
				if (charge)
				{
					energy += chargingRate;
					//anim.SetBool(hash.locomotionState, false);
					charge = false;
				}
				else
				{
					energy -= energyUsage;

				}

				if (energy > max_energy)
					ergos["charging"].ReachedGoal();

				if (energy <= 0)
					alive = false;


			}
			else
			{
				energy += energyUsage;
				if (energy > criticalLowEnergy)
					alive = true;
			}

*/


		}

}
