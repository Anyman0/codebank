using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VD.Infinity;

public enum TriggerType
{
    Portal, 
    Wind, 
    Ice,
    Vehicle
}

public class OnTriggerEvent : MonoBehaviour
{
    // Inspector fields
    [SerializeField]
    private TriggerType triggerType;
    [SerializeField]
    private GameObject skyboxController;    
    [SerializeField, Tooltip("This is the number of skyboxtexture from SkyboxControllers list.")]
    private int skyboxNumber;
    [SerializeField]
    private float skyboxTransition;
    [SerializeField, Tooltip("Check this if you want to spawn skybox with given value instead of from infinityarena chunk.")]
    private bool skyboxWithGivenValue;
    [SerializeField]
    private GameObject infinityArena;

    // Non-inspector fields
    private MovementScript1 ms;

    private void Awake()
    {
        
    }

    // Call actions on trigger
    private void OnTriggerEnter(Collider other)
    {        
        if(other.CompareTag("Player"))
        {            
            if (triggerType == TriggerType.Portal)
            {            
                if(skyboxWithGivenValue)
                Timing.RunCoroutine(skyboxController.GetComponent<SkyboxController>().ChangeSkyBox(skyboxNumber, 0, skyboxTransition));
                else
                {
                    var i = infinityArena.GetComponent<InfinityArena>().unityRandomSeed; // unityRandomSeed is nothing. NEED TO GET THE NEXT ARENA HERE.
                    Timing.RunCoroutine(skyboxController.GetComponent<SkyboxController>().ChangeSkyBox(i, 0, skyboxTransition));
                }                
            }

            else if(triggerType == TriggerType.Wind)
            {                
                Timing.RunCoroutine(GetComponent<WindScript>().AddWindForce(Random.Range(0,2)));
            }   
            
            else if(triggerType == TriggerType.Vehicle)
            {
                GameObject.FindGameObjectWithTag("VehicleSpawner").GetComponent<ObjectSpawner>().destroyWithlaser = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
    }

    // Call actions on collision (For ICE) 
    private void OnCollisionEnter(Collision collision)
    {
        if (triggerType == TriggerType.Ice)
        {
            Debug.Log("ENTER ICE");
            ms = collision.transform.GetComponent<MovementScript1>();
            ms.onIce = true;
            ms.isGrounded = false;
            collision.transform.GetComponent<Rigidbody>().AddForce(transform.forward * ms.enterIceForce);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (triggerType == TriggerType.Ice)
        {
            ms.onIce = false;
            ms.isGrounded = true;
            Debug.Log("EXIT ICE");
        }
    }
}
