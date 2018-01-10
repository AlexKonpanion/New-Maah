using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using Game;

public class UserBehaviour : MonoBehaviour {
    private Animator playerAnim;                        // Reference to the player's animator component.
    private HashIDs hash;                           // Reference to the HashIDs.

    // Use this for initialization
    void Start () {
        playerAnim = GetComponent<Animator>();
        hash = GameObject.FindGameObjectWithTag(GameTags.gameController).GetComponent<HashIDs>();

    }

    // Update is called once per frame
    void Update () {
        float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        float vertical = CrossPlatformInputManager.GetAxis("Vertical");
        if ( (horizontal > - 0.1 && horizontal < 0.1) && ( vertical > -0.1 && vertical < 0.1 ) )
        {// user is not moving 
            //playerAnim.SetInteger(hash.playerLocomationState, 1);
            playerAnim.SetFloat(hash.speedFloat, 0.0f, 0.1f, Time.deltaTime);
        } else
        {
            //playerAnim.SetInteger(hash.playerLocomationState, 1);
            playerAnim.SetFloat(hash.speedFloat, 1.0f, 0.1f, Time.deltaTime);
            
        }
    }
}
