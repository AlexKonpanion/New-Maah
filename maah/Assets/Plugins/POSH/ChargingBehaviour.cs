using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game;
using ReAct.unity;
using ReAct.sys;

public class ChargingBehaviour : KonpanionBehaviour {

    public Transform chargerLocation;
    public int chargingDistance;
    public int chargingRate;
    
    
    public float energy;
    public int max_energy;
    public float energyUsage;
    public float criticalLowEnergy;

    public bool alive=false;
   
   
    private bool charge = false;  // Trigger to determine if the Behaviour should charge
    public Dictionary<string,RampActivation> ergos;

    public bool Charges = false;

	// Use this for initialization
	void Awake () {
		base.Init ();

        alive = true;
		BehaviourTitle = "charging";
		core = GetComponent<RobotInterfaceUnity> ();
		anim = GetComponent<Animator> ();
		nav = GetComponent<UnityEngine.AI.NavMeshAgent>();

	}

    public bool WantsToCharge()
    {
        return ergos["charging"].Challenge(ergos);
    }


    public bool NeedToCharge()
    {
        if (energy < criticalLowEnergy)
            return true;

        return false;
    }

    internal void Recharging()
	{
		Loom.QueueOnMainThread (() => {
			if (core == null)
				return;

			// If near the next waypoint or there is no destination...
			if (core.navDestination == chargerLocation.position && nav.remainingDistance < nav.stoppingDistance && core.nextToCharger) {
				//patrolTimer += Time.deltaTime;
				Loom.RunAsync (() => {
                    charge = true;
				});
			} else {
				// Set the destination to the charging WayPoint.
				if (core.navDestination != chargerLocation.position) {
					core.navDestination = chargerLocation.position;

				}

			}
		});
	}


	// Update is called once per frame
	void Update () {
        if (alive)
        {
            Charges = charge;
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

        if (core != null) {
			core.alive = alive;
		}
        
	}

}
