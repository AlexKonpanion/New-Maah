using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game;
using ReAct.unity;
using ReAct.sys;

public class RobotInterfaceUnity : POSHBehaviour
{
    public float fieldOfViewAngle = 110f;               // Number of degrees, centred on forward, for the enemy see.
    public bool playerInSight;                          // Whether or not the player is currently sighted.
    public bool playerIsMoving;                          // Whether or not the player is currently sighted.
	public Transform[] patrolWayPoints;                     // An array of transforms for the patrol route.
	public float patrolSpeed = 2f;                          // The nav mesh agent's speed when patrolling.
	public float followSpeed = 5f;                           // The nav mesh agent's speed when chasing.
	public float movementSpeed = 2f;
    public bool nextToCharger;                          // Enemy is next to a Charger

    internal void FindOwner()
    {
        throw new NotImplementedException();
    }

    public Vector3 personalLastSighting;                // Last place this enemy spotted the player.

    public string useForPlan;

    public float[] activations;
    public bool[] activegoals;

    public bool alive;

    private LastPlayerSighting lastPlayerSighting;      // Reference to the last global sighting of the player.
    internal ChargingBehaviour charging;                     // Reference to the Charging script.
	internal SocialBehaviour social;
	internal MovementBehaviour movement;                     // Reference to the Charging script.

	private GameObject player;                              // Reference to the player's transform.
    
	internal int wayPointIndex;                              // A counter for the way point array.
	internal bool running = false;                           // A trigger for when the game actually starts and POSH can start interacting.

    private SphereCollider col;                         // Reference to the sphere collider trigger component.
    private Animator anim;                              // Reference to the Animator.
    private Animator playerAnim;                        // Reference to the player's animator component.
    private HashIDs hash;                           // Reference to the HashIDs.
    private Vector3 previousSighting;                   // Where the player was sighted last frame.
    private float previousSightingTime;

	internal Vector3 navDestination;
	internal float tickTimer;
    private string id;

    private Dictionary<int, Collider> objectsInReach;

    public float checkInterval;
    private float checkReachTime;

	internal KonpanionNest baseStation;

    public UnityEngine.UI.Text debugText;
    public String output = "debug";

    // ramps for behaviours
    public Dictionary<string, RampActivation> ergos;

    


    protected override POSHInnerBehaviour InstantiateInnerBehaviour(AgentBase agent)
    {
        ergos = new Dictionary<string, RampActivation>();

        ergos.Add("patrol", new RampActivation(0, 10, 0, 0.1f, 1.1f, 1));
        ergos.Add("charging", new RampActivation(0, 10, 0, 0.1f, 1.1f, 1));
        ergos.Add("following", new RampActivation(0, 10, 0, 0.1f, 1.5f, 1));
		ergos.Add("social", new RampActivation(0, 10, 0, 0.1f, 1.5f, 1)); //controlled through the Social Behaviour
		activations = new float[ergos.Count];
        activegoals = new bool[ergos.Count];
		if (charging != null)
			charging.ergos = ergos;
		if (social != null)
			social.ergos = ergos;
		if (movement != null)
			movement.ergos = ergos;
        running = true;
        previousSightingTime = -3;
        id = "";
        // setting agent compatibility the string should be identical at least to one lap otherwise the behaviour will not be used
        // currently multiple plans are not supported
        POSHInnerBehaviour behave = new RobotInterfacePlanner(agent, this, null);
        behave.SetSuitablePlans(new string[] { useForPlan });
        return behave;

    }

    internal bool Challenge(string behaviour)
    {
        if (!ergos.Keys.Contains(behaviour))
            return false;

        return ergos[behaviour].Challenge(ergos);
    }

    protected override void ConfigureParameters(Dictionary<string, object> parameters) { }


    protected override void ConfigureParameter(string para, object value)
    {
        switch (para)
        {
            case "startloc":
				if (movement != null)
                	movement.SetStartLocation(value as string);
                break;
            case "waypoints":
			if (movement != null)
				movement.SetWaypoints(value as string);
                break;
            case "name":
                id = value as string;
                break;
            default:
                break;
        }

    }

    

    void Awake()
    {
        // Setting up the references.
        charging = GetComponent<ChargingBehaviour>();
		social = GetComponent<SocialBehaviour>();
		movement = GetComponent<MovementBehaviour>();

        player = GameObject.FindGameObjectWithTag(GameTags.player);
        baseStation = GameObject.FindGameObjectWithTag(GameTags.gameController).GetComponent<KonpanionNest>();
        col = GetComponent<SphereCollider>();
        anim = GetComponent<Animator>();
        playerAnim = player.GetComponent<Animator>();
        hash = GameObject.FindGameObjectWithTag(GameTags.gameController).GetComponent<HashIDs>();
        lastPlayerSighting = GameObject.FindGameObjectWithTag(GameTags.gameController).GetComponent<LastPlayerSighting>();

        checkReachTime = Time.time;
        objectsInReach = new Dictionary<int, Collider>();



        // Set the personal sighting and the previous sighting to the reset position.
        previousSightingTime = -10;
        navDestination = Vector3.one;
        tickTimer = 0;
    }
    void UnityErgoFeedback()
    {
        // displays the activation strength in the Inspector
        activations[0] = ergos["patrol"].GetActivation();
        activations[1] = ergos["charging"].GetActivation();
        activations[2] = ergos["following"].GetActivation();
        // displays which goal is currently active in the Inspector
        activegoals[0] = ergos["patrol"].IsActive();
        activegoals[1] = ergos["charging"].IsActive();
        activegoals[2] = ergos["following"].IsActive();
    }

    void Update()
    {
        running = Application.isPlaying;

        // setting ticks only every third of a second
        if (ergos is Dictionary<string, RampActivation> && ergos.Count > 1 && Time.realtimeSinceStartup - tickTimer > 0.3f)
        {
            ergos["patrol"].Tick(false);

            // applies urgency if robot needs charging
            ergos["charging"].Tick(charging.NeedToCharge());

            if (Time.realtimeSinceStartup - previousSightingTime < 5)
                // applies urgency to chase the player if he is seen to often or if the alarm is on
                ergos["following"].Tick((Time.realtimeSinceStartup - previousSightingTime < movement.waitBeforeFollowingTime));

            UnityErgoFeedback();

            tickTimer = Time.realtimeSinceStartup;

            if (checkReachTime < Time.time + checkInterval)
                CheckReach();

            
            if (personalLastSighting == lastPlayerSighting.resetPosition || Time.realtimeSinceStartup - previousSightingTime > movement.waitBeforeFollowingTime)
            {
                ergos["following"].Reset();

            }

            if (playerInSight && playerAnim != null)
            {
                playerIsMoving = (playerAnim.speed > 0.1);
            }
            if (debugText != null)
                debugText.text = output;
        }

    }

    internal bool GameRunning()
    {
        return running;
    }

    float CalculatePathLength(Vector3 targetPosition)
    {
        // Create a path and set it based on a target position.
        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
        if (movement != null && movement.nav.enabled)
            movement.nav.CalculatePath(targetPosition, path);

        // Create an array of points which is the length of the number of corners in the path + 2.
        Vector3[] allWayPoints = new Vector3[path.corners.Length + 2];

        // The first point is the enemy's position.
        allWayPoints[0] = transform.position;

        // The last point is the target position.
        allWayPoints[allWayPoints.Length - 1] = targetPosition;

        // The points inbetween are the corners of the path.
        for (int i = 0; i < path.corners.Length; i++)
        {
            allWayPoints[i + 1] = path.corners[i];
        }

        // Create a float to store the path length that is by default 0.
        float pathLength = 0;

        // Increment the path length by an amount equal to the distance between each waypoint and the next.
        for (int i = 0; i < allWayPoints.Length - 1; i++)
        {
            pathLength += Vector3.Distance(allWayPoints[i], allWayPoints[i + 1]);
        }

        return pathLength;
    }


    void OnTriggerExit(Collider other)
    {
        // If the player leaves the trigger zone...
        if (other.gameObject == player)
        {
            // ... the player is not in sight.
            playerInSight = false;
            playerIsMoving = false;
            anim.SetBool(hash.playerInSightBool, false);
            //lastPlayerSighting.position = other.transform.position;
            lastPlayerSighting.playerLight.enabled = false;
        }
        else if (other.gameObject == charging.chargerLocation.gameObject)
            nextToCharger = false;

        objectsInReach.Remove(other.GetHashCode());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player || other.transform == charging.chargerLocation)
            objectsInReach[other.GetHashCode()] = other;
    }

    void CheckReach()
    {
        checkReachTime = Time.time;
        foreach (Collider entity in objectsInReach.Values)
            CheckReach(entity);
    }

    void CheckReach(Collider other)
    {
        // If the player has entered the trigger sphere...
        if (other.gameObject == player)
            TriggeredPlayer(other.transform);
        else if (other.transform == charging.chargerLocation)
            TriggerCharger(other.transform);
    }

    private void TriggerCharger(Transform other)
    {
        // Create a vector from the enemy to the charger
        Vector3 direction = other.position - transform.position;

        RaycastHit hit;
		// ... and if a raycast towards the charger hits something...
        if (Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, col.radius))
        {
			// ... and if the raycast hits the charger location...
            if (hit.collider.gameObject == charging.chargerLocation.gameObject)
            {
				if (direction.sqrMagnitude <= charging.chargingDistance * charging.chargingDistance)
                {
					nextToCharger = true;
                    baseStation.HumfClosenEssLight.enabled = true;

                }
                else
                {
                    nextToCharger = false;
                    baseStation.HumfClosenEssLight.enabled = false;
                }
            }
        }

    }

    private void TriggeredPlayer(Transform other)
    {
        // By default the player is not in sight.
        playerInSight = false;
        
        // Create a vector from the enemy to the player and store the angle between it and forward.
        Vector3 direction = other.position - transform.position;
        float angle = Vector3.Angle(direction, transform.forward);

        // If the angle between forward and where the player is, is less than half the angle of view...
        if (angle < fieldOfViewAngle * 0.5f)
        {
            RaycastHit hit;

            // ... and if a raycast towards the player hits something...
            if (Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, col.radius))
            {
                // ... and if the raycast hits the player...
                if (hit.collider.gameObject == player)
                {
                    // ... the player is in sight.
                    playerInSight = true;
                    //DebugOnScreen("See Owner" + playerInSight);
                    lastPlayerSighting.playerLight.enabled = true;
                    previousSightingTime = Time.realtimeSinceStartup;

                    // Set the last global sighting is the players current position.
                    lastPlayerSighting.position = player.transform.position;
                    personalLastSighting = player.transform.position;
                }
                else
                {
                    anim.SetBool(hash.playerInSightBool, playerInSight);
                }

            }
        }
        if (playerAnim == null) //FIXME: dirty hack until player has animations
            return;
        // Store the name hashes of the current states.
        int playerLayerZeroStateHash = playerAnim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        int playerLayerOneStateHash = playerAnim.GetCurrentAnimatorStateInfo(1).fullPathHash;

        // If the player is running or is attracting attention...
        if (playerLayerZeroStateHash == hash.locomationState || playerLayerOneStateHash == hash.shoutState)
        {
            // ... and if the player is within hearing range...
            if (CalculatePathLength(player.transform.position) <= col.radius)
                // ... set the last personal sighting of the player to the player's current position.
                personalLastSighting = player.transform.position;


        }
    }

	private int IndexInString(string testString, string test, int occurance)
    {
        int output, result = 0;

        while (result > -1 && occurance > 0)
        {
            output = result;
            result = testString.IndexOf(test, result + 1);
            occurance--;
        }

        return result;
    }
    public void DebugOnScreen(string message)
    {

        //Loom.QueueOnMainThread(() =>
        // {  
        int idx = 0;

        if ((idx = IndexInString(output, Environment.NewLine, 10)) > 0)
            output = output.Substring(IndexInString(output, Environment.NewLine, 1));

        output += message + Environment.NewLine;
        //       });
    }
}
