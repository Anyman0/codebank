using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class WindScript : MonoBehaviour
{
    // Editor fields
    [SerializeField, Tooltip("How much force is added to push player.")]
    private float windVolume;
    [SerializeField, Tooltip("How long does the wind last.")]
    private float windLength;
    [SerializeField, Tooltip("Visual effect of the wind.")]
    private GameObject windEffect;
    [SerializeField, Tooltip("Wind sound prefab here. (If we need a separate prefab for it)")]
    private GameObject windSound;

    // Non-editor fields
    private GameObject player;
    private Rigidbody playerRB;
    private Vector3 windDirection;
    private bool windRight;
    private bool isWindy;
    
    
    // IEnumerators <<<<< This method is called when player enters wind-trigger >>>>>
    public IEnumerator<float> addWindForce(int direction)
    {
        // Defining the wind direction here
        if(direction == 1)
        {
            windDirection = Vector3.right;
        }
        else if(direction == 0)
        {
            windDirection = Vector3.left;
        }

        isWindy = true;
        Debug.Log("ADDING WINDFORCE...");
        yield return Timing.WaitForSeconds(windLength);

        isWindy = false;
        
    }

    // 
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerRB = player.GetComponent<Rigidbody>();
    }

    // FixedUpdate so that opening Menu stops it
    private void FixedUpdate()
    {
       
        if(isWindy)
        {            
            // Adding the actual force here
            playerRB.AddForce(windDirection * windVolume);
            playerRB.velocity = Vector3.zero;
            // Effect & Sound
            if (windEffect != null)
            {
                // Play windEffect here. 
            }

            if (windSound != null)
            {
                // Play wind sound here. 
            }
        }
    }
}
