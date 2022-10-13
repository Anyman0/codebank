using RayFire;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Throwable : MonoBehaviour
{
    [SerializeField]
    private SteamVR_Input_Sources throwingHand;

    [SerializeField, Tooltip("This value determines how much force we add to throw motion.")]
    private float throwingSpeedMultiplier;
    [SerializeField, Tooltip("Explosion effect")]
    private GameObject explosionEffect;

    // Inputs
    private bool pinchUp;
    private bool pinchDown;
    private bool grabUp;
    private bool grabDown;
   

    public Hand hand;
    public Vector3 velocity;
    public Vector3 angularVelocity;
    private bool canThrow = false;
    WeaponScript ws;
    private void Awake()
    {
        ws = transform.parent.parent.GetComponent<WeaponScript>();
        transform.gameObject.SetActive(false);
    }


    private void Update()
    {

        if (throwingHand == SteamVR_Input_Sources.RightHand)
        {
            pinchUp = SteamVR_Actions.default_GrabPinch.GetState(SteamVR_Input_Sources.RightHand);
            grabUp = SteamVR_Actions.default_GrabGrip.GetState(SteamVR_Input_Sources.RightHand);
            pinchDown = SteamVR_Actions.default_GrabPinch.GetState(SteamVR_Input_Sources.RightHand);
            grabDown = SteamVR_Actions.default_GrabGrip.GetState(SteamVR_Input_Sources.RightHand);
            hand = GameObject.FindGameObjectWithTag("RightHand").GetComponent<Hand>();
        }
        else if (throwingHand == SteamVR_Input_Sources.LeftHand)
        {
            pinchUp = SteamVR_Actions.default_GrabPinch.GetStateUp(SteamVR_Input_Sources.LeftHand);
            grabUp = SteamVR_Actions.default_GrabGrip.GetStateUp(SteamVR_Input_Sources.LeftHand);
            pinchDown = SteamVR_Actions.default_GrabPinch.GetState(SteamVR_Input_Sources.LeftHand);
            grabDown = SteamVR_Actions.default_GrabGrip.GetState(SteamVR_Input_Sources.LeftHand);
            hand = GameObject.FindGameObjectWithTag("LeftHand").GetComponent<Hand>();
        }

        if(transform.parent != null)
        {
            if (transform.parent.parent.tag == "" + throwingHand && pinchDown && grabDown)
            {
                calculateThrowSpeed(out velocity, out angularVelocity);
                Debug.Log(velocity.magnitude + " Angular: " + angularVelocity.magnitude + " States: " + pinchUp + grabUp);
                canThrow = true;
            }

            if (!pinchUp && !grabUp && canThrow)
            {
                Debug.Log("Release!");
                ReleaseObject();
            }
        }
                      
    }


    private void ReleaseObject()
    {       
        transform.SetParent(null);
        var rb = transform.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        rb.velocity = velocity * throwingSpeedMultiplier;
        rb.angularVelocity = angularVelocity * throwingSpeedMultiplier;             
        canThrow = false;
        Invoke("delayExplode", 3f);
    }

    private void calculateThrowSpeed(out Vector3 velocity, out Vector3 angularVelocity) 
    {
        
        velocity = hand.GetTrackedObjectVelocity();
        angularVelocity = hand.GetTrackedObjectAngularVelocity();
    }




    // These should come from Rayfire somewhere?? -------------->
    private void explodeGrenade()
    {
        var nadeHolder = GameObject.Find("GrenadeHolder");               
        transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        transform.SetParent(nadeHolder.transform);               
        ws.enabled = true;
        this.transform.gameObject.SetActive(false);
        GameObject xplosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Destroy(xplosion, 2f);
        CancelInvoke();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag != "Player")
        {
            Debug.Log("COLLISION" + collision.transform.tag);
            // Explode-function from RayFire.
            transform.GetComponent<RayfireBomb>().Explode(0f);
            explodeGrenade();
        }        
    }

    private void delayExplode()
    {
        var nadeHolder = GameObject.Find("GrenadeHolder");
        transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        transform.SetParent(nadeHolder.transform);        
        ws.enabled = true;
        this.transform.gameObject.SetActive(false);
        GameObject xplosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Destroy(xplosion, 2f);
    }
   

}
