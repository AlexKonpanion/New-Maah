using ReAct.sys;
using ReAct.sys.annotations;
using ReAct.unity;
using System.Collections.Generic;
using UnityEngine;

public class RobotInterfacePlanner : POSHInnerBehaviour
{
    public RobotInterfacePlanner(AgentBase agent, POSHBehaviour parent, Dictionary<string, object> attributes)
        : base(agent, parent, attributes)
    {
        AttachToLog("init robot movement");
    }

    private RobotInterfaceUnity GetRobotControl()
    {
        return (RobotInterfaceUnity)parent;
    }

    private void AttachToLog(string message)
    {
        //  ((RobotInterfaceUnity)parent).DebugOnScreen("Konpanion" + agent.id + " " + message);
        ((RobotInterfaceUnity)parent).DebugOnScreen(message);
    }

    // Actions
    [ExecutableAction("charging", 0.01f)]
    public void charging()
    {
       
        if (GetRobotControl().charging != null) {
            GetRobotControl().charging.Recharging();
            if (GetRobotControl().charging.Charges)
                AttachToLog("recharges battery");
            else
                AttachToLog("goes to Nest");


        }
    }

    [ExecutableAction("follow", 0.01f)]
    public void Follow()
    {
        AttachToLog("follows you");
		if (GetRobotControl().movement != null)
			GetRobotControl().movement.Following();
    }

    [ExecutableAction("findOwner", 0.01f)]
    public void findOwner()
    {
        GetRobotControl().FindOwner();
    }

    [ExecutableAction("roam", 0.01f)]
    public void roam()
    {

        AttachToLog("is exploring");
		if (GetRobotControl().movement != null)
			GetRobotControl().movement.Roaming();
    }

    [ExecutableAction("greet", 0.01f)]
    public void GreetOwner()
    {

        AttachToLog("greets you");
		if (GetRobotControl().social != null)
			GetRobotControl().social.GreetOwner();
    }

    [ExecutableAction("idling", 0.01f)]
    public void idling()
    { string[] idle = new string[] { "looks around", "enjoys view", "is observing", "is listening" };
        Loom.QueueOnMainThread(() =>
        {
            int i = Random.Range(0, 4);
            AttachToLog(idle[i]);
        });
		if (GetRobotControl().movement != null)
			GetRobotControl().movement.Idling();
    }

    /******************
     * Senses
     * ****************
     */

    [ExecutableSense("game_finished", 0.01f)]
    public bool game_finished()
    {
        return !GetRobotControl().GameRunning();
    }

    [ExecutableSense("wantToFollow", 0.01f)]
    public bool wantToFollow()
    {
		bool res = false;
		if (GetRobotControl ().movement != null) {
			res = GetRobotControl ().movement.WantsToFollow ();
		}

        if (res)
        {
            AttachToLog("wants to follow you");
        } else
        {
            AttachToLog("does not wants to follow you");
        }
        //TODO: this is the place to integrate any kind of need model like ERGo
        return res;
    }


    [ExecutableSense("seeOwner", 0.01f)]
    public bool seeOwner()
    {
        if (GetRobotControl().playerInSight) {
            AttachToLog("can see you");
        } else
        {
            //AttachToLog("cannot see owner");
        }

        return GetRobotControl().playerInSight;
    }

    [ExecutableSense("ownerMoves", 0.01f)]
    public bool OwnerMoves()
    {
        bool res = GetRobotControl().playerIsMoving;
        if (res) {
            AttachToLog("sees you moving");
        } else
        {
           // AttachToLog("sees owner moving");
        }
        return res;
    }

    [ExecutableSense("sawOwnerLast", 0.01f)]
    public bool sawOwnerLast()
    {
        //AttachToLog("in sawOwnerLast");
        if (GetRobotControl().playerInSight == false && !(GetRobotControl().personalLastSighting == new Vector3(1000f, 1000f, 1000f)))
            return true;
        return false;
    }

    [ExecutableSense("needCharging", 0.01f)]
    public bool needCharging()
	{	bool res = false;
        if (GetRobotControl ().charging != null) {
			res = GetRobotControl().alive && GetRobotControl ().charging.WantsToCharge();
		}
        if (res)
        {
            AttachToLog("needs to charge");
        }
        else
        {
           // AttachToLog("checks battery");
        }

		return res;
    }

    [ExecutableSense("wantAttention", 0.01f)]
    public bool wantAttention()
    {
        bool res = false;
		if (GetRobotControl ().social != null) {
			res = GetRobotControl().alive && GetRobotControl ().social.WantsAttention();
		}
        if (res)
        {
            AttachToLog("wants attention");

        }
        return res;
    }

    [ExecutableSense("wantToRoam", 0.01f)]
    public bool wantToRoam()
    {
		bool res = false;
		if (GetRobotControl ().movement != null) {
			res = GetRobotControl().alive && GetRobotControl ().movement.WantsToRoam();
		}
        if (res)
            AttachToLog("wants to explore");
        return res;
    }

    [ExecutableSense("success", 0.01f)]
    public bool success()
    {
        return true;
    }

    [ExecutableSense("fail", 0.01f)]
    public bool fail()
    {
        return false;
    }

}
