using UnityEngine;
using System.Collections;

namespace Game
{
    public class HashIDs : MonoBehaviour
    {
        // Here we store the hash tags for various strings used in our animators.
        public int shoutState;
        public int speedFloat;
        public int sneakingBool;
        public int shoutingBool;
        public int playerInSightBool;
        public int aimWeightFloat;
        public int angularSpeedFloat;
        public int openBool;
        public int locomationState;
		public int showAffection;
		public int showSadness;
        public int idleState;
        public int walkingState;
        
        void Awake()
        {
            walkingState = Animator.StringToHash("Walking");
            locomationState = Animator.StringToHash("Base Layer.Locomotion");
            idleState = Animator.StringToHash("Base Layer.Idle");
            shoutState = Animator.StringToHash("Shouting.Shout");
            speedFloat = Animator.StringToHash("Speed");
            sneakingBool = Animator.StringToHash("Sneaking");
            shoutingBool = Animator.StringToHash("Shouting");
            playerInSightBool = Animator.StringToHash("PlayerInSight");
			showAffection = Animator.StringToHash("ShowAffection");
			showSadness = Animator.StringToHash("ShowSadness");
            aimWeightFloat = Animator.StringToHash("AimWeight");
            angularSpeedFloat = Animator.StringToHash("AngularSpeed");
            openBool = Animator.StringToHash("Open");
        }
    }

}