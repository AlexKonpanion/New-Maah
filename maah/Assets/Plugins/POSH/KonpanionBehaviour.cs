using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game;
using ReAct.unity;
using ReAct.sys;

namespace ReAct.unity
{
    public class KonpanionBehaviour : MonoBehaviour
    {

        public string BehaviourTitle = "None";
        public RampActivation Ergo = null;
		protected RobotInterfaceUnity core;
		protected Animator anim;                              // Reference to the Animator.
		protected HashIDs hash;                           // Reference to the HashIDs.
        protected float tick;
        internal UnityEngine.AI.NavMeshAgent nav;                               // Reference to the nav mesh agent.

        // Use this for initialization
        protected void Init()
        {
			// default activation for the ramp 
            Ergo = new RampActivation(0, 10, 0, 0.1f, 1.1f, 1);
			anim = GetComponent<Animator> ();
			hash = GameObject.FindGameObjectWithTag (GameTags.gameController).GetComponent<HashIDs> ();
			nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
            tick = 0;
        }

        public bool Challenge()
        {
            return gameObject.GetComponent<RobotInterfaceUnity>().Challenge(BehaviourTitle);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
