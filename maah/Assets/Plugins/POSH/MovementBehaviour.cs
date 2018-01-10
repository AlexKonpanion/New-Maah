using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game;
using ReAct.unity;
using ReAct.sys;

public class MovementBehaviour : KonpanionBehaviour {

	private SphereCollider col;                         // Reference to the sphere collider trigger component.
	private float followTimer;                               // A timer for the chaseWaitTime.
	private LastPlayerSighting lastPlayerSighting;      // Reference to the last global sighting of the player.

	internal Transform[] patrolWayPoints;                     // An array of transforms for the patrol route.

	public float waitBeforeFollowingTime = 5f;                      // The amount of time to wait when the last sighting is reached.
	public float patrolWaitTime = 1f;                       // The amount of time to wait when the patrol way point is reached.
	public float patrolTimer;                               // A timer for the patrolWaitTime.
	public Dictionary<string,RampActivation> ergos;




	// Use this for initialization
	void Start () {
		BehaviourTitle = "movement";
	}

	void Awake() {
		base.Init ();

		col = GetComponent<SphereCollider> ();
		core = GetComponent<RobotInterfaceUnity> ();
		lastPlayerSighting = GameObject.FindGameObjectWithTag(GameTags.gameController).GetComponent<LastPlayerSighting>();
		if (core != null)
			patrolWayPoints = core.patrolWayPoints;
	}

	public bool V3Equal(Vector3 a, Vector3 b)
	{
		//Debug.Log("V3:"+ (a-b).ToString()+ " "+ Vector3.SqrMagnitude(a - b));
		float diff = Vector3.SqrMagnitude(a - b);
		return diff < 0.005;
	}

	internal void SetStartLocation(string waypoint)
	{
		GameObject wp = GameObject.Find(waypoint);
		if (wp is GameObject)
			gameObject.transform.position = wp.transform.position;
	}

	internal void SetWaypoints(string waypointList)
	{
		string[] wayPoints = waypointList.Trim().Split(' ');
		if (wayPoints.Length > 0)
		{
			List<Transform> locations = new List<Transform>();
			foreach (string wpString in wayPoints)
			{
				Transform wp = GameObject.Find(wpString).transform;

				if (wp is Transform)
					locations.Add(wp);
			}
			patrolWayPoints = locations.ToArray();
		}
	}


	/**
	 * setting of actions and senses
	 */

	internal bool WantsToFollow()
	{
		return ergos["following"].Challenge(ergos);
	}

	internal bool WantsToRoam()
	{
		//ergos["patrol"].Tick(false);

		return ergos["patrol"].Challenge(ergos);
	}

	internal void Idling()
	{
		//ergos["patrol"].Tick(true);
	}

	internal void Patrolling()
	{
		// Set an appropriate speed for the NavMeshAgent.
		Loom.QueueOnMainThread(() =>
			{
				if (core == null)
					return;
				
				core.movementSpeed = core.patrolSpeed;

				// If near the next waypoint or there is no destination...
				if (nav.destination == lastPlayerSighting.resetPosition || nav.remainingDistance < nav.stoppingDistance)
				{
					// If the timer exceeds the wait time...
					if (patrolTimer + patrolWaitTime < Time.time)
					{
						ergos["patrol"].ReachedGoal();

						// ... increment the wayPointIndex.
						if (core.wayPointIndex == patrolWayPoints.Length - 1)
						{
							core.wayPointIndex = 0;


						}
						else
							core.wayPointIndex++;

						// Reset the timer.
						//patrolTimer = 0;
					}

				}

				// Set the destination to the patrolWayPoint.
				core.navDestination = patrolWayPoints[core.wayPointIndex].position;
				if (!V3Equal(core.navDestination, nav.destination))
				{
					patrolTimer = Time.time;
				}
			});
	}

	internal void Roaming()
	{
		// Set an appropriate speed for the NavMeshAgent.
		Loom.QueueOnMainThread(() =>
			{
				if (core == null)
					return;
				
				core.movementSpeed = core.patrolSpeed;
				//nav.Stop();
				// If near the next waypoint or there is no destination...

				if (nav.destination == lastPlayerSighting.resetPosition || nav.remainingDistance < nav.stoppingDistance)
				{
					// If the timer exceeds the wait time...
					if (patrolTimer + patrolWaitTime < Time.time)
					{
						ergos["patrol"].ReachedGoal();


						// ... increment the wayPointIndex.
						if (core.wayPointIndex == patrolWayPoints.Length - 1)
						{
							core.wayPointIndex = 0;
						}
						else 
						{
							// picks a random location to go to from the list of possible points
							float value = UnityEngine.Random.value;

							core.wayPointIndex = (int) (patrolWayPoints.Count() * value);
						}
						// Reset the timer.
						//patrolTimer = 0;
					}

				}

				// Set the destination to the patrolWayPoint.
				core.navDestination = patrolWayPoints[core.wayPointIndex].position;
				if (!V3Equal(core.navDestination, nav.destination))
				{
					patrolTimer = Time.time;
				}
			});
	}

	internal void Following()
	{

		Loom.QueueOnMainThread(() =>
			{
				if (core == null)
					return;

				// Create a vector from the enemy to the last sighting of the player.
				Vector3 sightingDeltaPos = core.personalLastSighting - transform.position;



				// If the the last personal sighting of the player is not close...
				if (sightingDeltaPos.sqrMagnitude > 4f)
				{
					// ... set the destination for the NavMeshAgent to the last personal sighting of the player.
					core.navDestination = core.personalLastSighting;
					if (nav.destination != core.navDestination)
					{
						followTimer = Time.time;
					}
				}

				// If near the last personal sighting...
				if (nav.remainingDistance < nav.stoppingDistance)
				{

					// Fullfilled the need to chase player
					ergos["following"].ReachedGoal();

					// If the timer exceeds the wait time...
					if (followTimer + waitBeforeFollowingTime < Time.time)
					{
						// ... reset last global sighting, the last personal sighting and the timer.
						lastPlayerSighting.position = lastPlayerSighting.resetPosition;
						core.personalLastSighting = lastPlayerSighting.resetPosition;
						core.navDestination = Vector3.zero;

					}
				} else {
					// Set the appropriate speed for the NavMeshAgent.
					core.movementSpeed = core.followSpeed;

				}

			});
	}

	private void Move()
	{
		// Set an appropriate speed for the NavMeshAgent.
		Loom.QueueOnMainThread(() =>
			{	if (core == null) 
				return;

				if (core.navDestination == Vector3.zero){
					nav.Stop();
					return;
				}

				if (nav.speed != core.movementSpeed)
					nav.speed = core.movementSpeed;

				// Set the destination to the charging WayPoint.
				if (nav.destination != core.navDestination)
					nav.destination = core.navDestination;
				
				// If near the next waypoint or there is no destination...
				if (nav.remainingDistance < nav.stoppingDistance)
				{
					anim.SetFloat(hash.speedFloat, 0.0f, 0.1f, Time.deltaTime);
					core.navDestination = Vector3.zero;

					nav.Stop();
				} else
				{
					anim.SetFloat(hash.speedFloat, 1.0f, 0.1f, Time.deltaTime);
					if (anim.GetCurrentAnimatorStateInfo(0).fullPathHash == hash.locomationState){
						nav.Resume();
					}
					//Debug.Log(anim.GetCurrentAnimatorStateInfo(0).fullPathHash);
				}

			});
	}


	void Update() {
		if (ergos is Dictionary<string, RampActivation> && ergos.Count > 1 && tick != core.tickTimer) {
			
			if (!core.alive)
				nav.Stop ();
			else
				Move ();
			//Debug.Log(anim.GetFloat(hash.speedFloat));

            tick = core.tickTimer;

		}
	}

}
