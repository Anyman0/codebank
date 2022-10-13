using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeVehicleScript : MonoBehaviour
{
    // Editor fields
    [SerializeField, Tooltip("Determines the speed of the vehicle.")]
    private float vehicleSpeed;    
    [SerializeField, Tooltip("Objects to drop from the vehicle.")]
    private GameObject objectToDrop;
    [SerializeField, Tooltip("Determines how many objects do we drop at once.")]
    private float objectAmount;
    [SerializeField, Tooltip("This is the debris we drop if vehicle was destroyed by the laser from the eye.")]
    private GameObject destroyDebris;
    [SerializeField, Tooltip("How many debris objects do we drop.")]
    private float debrisAmount;
    [SerializeField, Tooltip("Minimum force added to debris/drops.")]
    private float minDropForce;
    [SerializeField, Tooltip("Maximum force added to debris/drops.")]
    private float maxDropForce;
    [SerializeField, Tooltip("Determine if this vehicle will drop objects or not.")]
    private bool willDropObjects;
    [SerializeField, Tooltip("Determines if this vehicle will be destroyed by the eye.")]
    public bool isEyeTarget;
    [SerializeField, Tooltip("Effect to play when laser destroys the vehicle.")]
    private GameObject explosionEffect;
    [SerializeField, Tooltip("This value determines how often does the laser destroy vehicle. 4 for 25% chance, 5 for 20% and so on.")]
    private int vehicleLaserChance = 4;
    
    // Non-editor fields   
    [HideInInspector]
    public bool EyeDestruction;
    private List<float> forceList;
    private List<Vector3> directionList;
    private bool canDrop;
    private Vector3 dropOffset;
    [HideInInspector]
    public GameObject targetForEye;
    private bool onEnableBlock;
    private GameObject eyeLaserPrefab;

    private void Awake()
    {
        eyeLaserPrefab = GameObject.Find("EyeLaserPrefab");
        dropOffset = new Vector3(0, 0,-4);
        forceList = new List<float>();
        directionList = new List<Vector3>();
        directionList.Add(Vector3.up);
        directionList.Add(Vector3.down);
        directionList.Add(Vector3.right);
        directionList.Add(Vector3.left);        
    }

    private void Update()
    {
        transform.position += Vector3.left * vehicleSpeed;

        if(willDropObjects && canDrop)
        {
            dropObjects();
        }        
        
    }

    private void OnEnable()
    {
        if (isEyeTarget)
        {
            //Debug.Log(EyeDestruction);
            if(onEnableBlock)
            {
                // 25% chance to destroy vehicle with laser
                int debrisOrNot = Random.Range(0, vehicleLaserChance);                
                if (debrisOrNot == 0) EyeDestruction = true;
                if (EyeDestruction)
                {
                    
                    if (!eyeLaserPrefab.GetComponent<LaserBeamScript>().isBeaming)
                        Timing.RunCoroutine(eyeLaserPrefab.GetComponent<LaserBeamScript>().destroyVehicle(transform.gameObject));
                }
                else
                {
                    Debug.Log("Not a target for the eye.");
                }
            }

            onEnableBlock = true;
        }
    }

    // Method to drop objects / debris
    public void dropObjects()
    {
        randomizeDrops(); 

        if(!EyeDestruction)
        {
            foreach(float force in forceList)
            {
                GameObject drop = Instantiate(objectToDrop, transform.position + dropOffset, Quaternion.identity);
                var rb = drop.GetComponent<Rigidbody>();
                var rndDir = Random.Range(2, 3);
                rb.AddForce(directionList[rndDir] * force);                  
                Destroy(drop, 5);
            }           
        }
        // If object is destroyed with laser and will drop debris
        else if(EyeDestruction)
        {
            foreach(float force in forceList)
            {
                dropOffset = dropOffset + new Vector3(Random.Range(-2f, 4f), Random.Range(-2f, 3f), 0);
                GameObject debris = Instantiate(destroyDebris, transform.position + dropOffset, Quaternion.identity);                
                var rb = debris.GetComponent<Rigidbody>();
                var rndDir = Random.Range(0, 0);               
                rb.AddForce(directionList[rndDir] * force);                
                Destroy(debris, 5);               
            }
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosion, 2);
            transform.parent.GetComponent<BridgeVehicleSpawnerScript>().disableVehicleOnDestruction(transform.gameObject);           
            dropOffset = new Vector3(0, 0, -4);
        }
        forceList.Clear();
        canDrop = false;
        EyeDestruction = false;
    }

    // This is where we randomize the direction and the force of the dropped objects
    private void randomizeDrops()
    {
         
        if(isEyeTarget)
        {
            for (var i = 0; i < debrisAmount; i++)
            {
                var force = Random.Range(minDropForce, maxDropForce);
                forceList.Add(force);
            }
        }
        if(willDropObjects)
        {
            for(var i = 0; i < objectAmount; i++)
            {
                var force = Random.Range(minDropForce, maxDropForce);
                forceList.Add(force);
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(willDropObjects)
        canDrop = true;
    }
    
    IEnumerator<float> colliderDelay(GameObject obj)
    {        
        yield return Timing.WaitForSeconds(0.2f);
        obj.GetComponent<CapsuleCollider>().enabled = true;
    }
}
