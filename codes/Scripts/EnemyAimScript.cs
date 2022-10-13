using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using VD;

public class EnemyAimScript : MonoBehaviour
{
    public GameObject target; // Target of the enemy. (Player)
    public float enemyAimSpeed = 5.0f;
    Quaternion newRotation;
    float orientTransform;
    float orientTarget;

    [SerializeField, Tooltip("Projectile launched from this transform.")]
    private GameObject projectile;
    [SerializeField, Tooltip("String ID of the projectile from ItemManager that we want to spawn.")]
    private string projectileID;
    [SerializeField, Tooltip("Determines how fast does the fired projectile move. 1000 as default.")]
    private float projectileSpeed = 1000;  
    [SerializeField, Tooltip("Determines when the enemy engages the target. Based on this distance. Default at 35")]
    private float enemyEngageDistance = 35f;
    [SerializeField, Tooltip("Determines how often the enemy fires projectiles towards target. 5 as default")]
    private float shootingInterval = 5;
    [SerializeField, Tooltip("Prefab slot for drone shoot-effect.")]
    protected GameObject droneMuzzleEffect;
    [SerializeField, Tooltip("This is where the droneMuzzleEffect instantiates.")]
    protected GameObject droneMuzzle;
    [SerializeField, Tooltip("Determines the drones movement speed from origin to DroneMoveTargets. Default at 0.3f.")]
    private float movementSpeed = 0.5f; 

    
    private float hoverHeight = 2.6f;
    //private float movementSpeed;
    private bool canDestroy = false;
    private bool moveToTarget = true;
    private bool waitRandomMovement;
    private int moveTargetInt = -1;
    private List<Transform> droneMoveTargets;
    private GameObject droneTargetHolder;
    private Vector3 randomPosition;
    private float distance;
    private List<Transform> drones;
    private GameObject projectileClone;
    [HideInInspector]
    public CoroutineHandle fireHandle;
    [HideInInspector]
    public CoroutineHandle destroyHandle;
    

    // Hover attributes  
    [SerializeField,Tooltip("Horizontal hovering speed. Default se at 0.01f.")]
    private float horizontalSpeed = 0.01f;
    [SerializeField, Tooltip("Vertical hovering speed. Default set to 1.")]
    private float verticalSpeed = 1f;
    [SerializeField, Tooltip("Determines how much the drone ascends and descends. Default at 0.1f")]
    private float amplitude = 0.1f;
    private Vector3 tempPos;
    private float yHoverOffset = 0f;
    private bool isCoRoutineRunning;

    private void Awake()
    {        
        droneMoveTargets = new List<Transform>();
        drones = new List<Transform>();
        droneTargetHolder = GameObject.Find("DroneMoveTargets");       
        //movementSpeed = GameObject.FindGameObjectWithTag("Player").GetComponent<MovementScript>().movementSpeed;
        target = GameObject.FindGameObjectWithTag("Player");     
    }

    void Update()
    {
       
        if(droneMoveTargets.Count < 1)
        DroneMoveTargetsPopulate();

        if(moveTargetInt == -1)
        {
            moveTargetInt = Random.Range(0, droneMoveTargets.Count - 1);                     
        }
        
        if(moveToTarget)
        {
            //Debug.Log(moveTargetInt);
            transform.parent.position = Vector3.Lerp(transform.parent.position, droneMoveTargets[moveTargetInt].transform.position, Time.deltaTime * movementSpeed);            
        }
        
        if((transform.parent.position - droneMoveTargets[moveTargetInt].transform.position).magnitude  < 1.5f)
        {           
            moveToTarget = false;          
        }
        

        orientTransform = transform.position.x;
        orientTarget = target.transform.position.x;
        // To Check on which side is the target , i.e. Right or Left of this Object
        if (orientTransform > orientTarget)
        {            
            newRotation = Quaternion.LookRotation(transform.position - target.transform.position, -Vector3.up);           
        }
        else
        {
            newRotation = Quaternion.LookRotation(transform.position - target.transform.position, Vector3.up);
        }

        
        // Finally rotate and aim towards the target direction using Code below
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * enemyAimSpeed);
        transform.LookAt(target.transform);

        // Ray for debugging
        Debug.DrawRay(transform.position, transform.forward, Color.red);

        
        distance = (transform.parent.position - target.transform.position).magnitude;

        // IF drone needs to move                   
       /* if(distance <= enemyEngageDistance)
        {            
            this.transform.parent.position += transform.parent.forward * movementSpeed;
            canDestroy = true;
        }*/
              
        // If player escapes the drone
        if(distance > 40 && canDestroy)
        {            
            Destroy(transform.parent.gameObject);
        }     
        
        if(!moveToTarget && distance <= enemyEngageDistance && !fireHandle.IsRunning/*!IsInvoking()*/) // Enemy has reached its destination and can start shooting
        {
            //Debug.Log("Ready to shoot!" + IsInvoking());
            fireHandle = Timing.RunCoroutine(fireProjectile());
            //Invoke("FireProjectile", shootingInterval);              
            /*if(projectileClone == null)
            FireProjectile();*/
        }

        
        
    }

    private void FixedUpdate()
    {
        if (!moveToTarget)
        {
                      
            if(tempPos == new Vector3(0,0,0))
            {
                tempPos = transform.parent.position;
                yHoverOffset = tempPos.y;
            }
            if (!isCoRoutineRunning) StartCoroutine(getRandomHoverX());

            //Debug.Log(horizontalSpeed);
            tempPos.x += horizontalSpeed;
            tempPos.y = yHoverOffset + Mathf.Sin(Time.realtimeSinceStartup * verticalSpeed) * amplitude;             
            transform.parent.position = tempPos;
        }
    }

    IEnumerator getRandomHoverX()
    {
        isCoRoutineRunning = true;
        yield return new WaitForSeconds(5);
        var hz = Random.Range(-horizontalSpeed, horizontalSpeed);
        horizontalSpeed = hz;
        isCoRoutineRunning = false;
    }

    /*private void FireProjectile()
    {
        //GameObject projectileClone;        
        projectileClone = Instantiate(projectile, this.transform.position, this.transform.rotation);
        Physics.IgnoreCollision(projectileClone.GetComponent<CapsuleCollider>(), transform.parent.GetComponent<SphereCollider>());
        //projectile.GetComponent<SphereCollider>().enabled = false;
        GameObject effect = Instantiate(droneMuzzleEffect, droneMuzzle.transform.position, Quaternion.identity);
        Destroy(effect, 2f);
        projectileClone.GetComponent<Rigidbody>().AddForce(this.transform.forward * projectileSpeed);
        StartCoroutine(DestroyProjectile(projectileClone, projectileClone.GetComponent<ProjectileScript>().lifeTime));
    }*/

    IEnumerator<float> fireProjectile()
    {
        yield return Timing.WaitForSeconds(shootingInterval);
        projectileClone = ItemManager.Instance.Spawn("" + projectileID, transform.position, transform.rotation);
        Physics.IgnoreCollision(projectileClone.GetComponent<CapsuleCollider>(), transform.parent.GetComponent<SphereCollider>());
        GameObject effect = Instantiate(droneMuzzleEffect, droneMuzzle.transform.position, Quaternion.identity);
        Destroy(effect, 2f);
        projectileClone.GetComponent<Rigidbody>().AddForce(transform.forward * projectileSpeed);
        destroyHandle = Timing.RunCoroutine(projectileClone.GetComponent<ProjectileScript>().destroyProjectile().CancelWith(projectileClone));
    }

    // lifetime here comes from projectiles ProjectileScript
    IEnumerator<float> DestroyProjectile(GameObject projectile, float lifetime)
    {
        yield return Timing.WaitForSeconds(lifetime);
        //Destroy(projectile);
        
        projectileClone = null;
    }

    private void DroneMoveTargetsPopulate()
    {
        // Populating the list of droneMoveTargets. These are the positions where drones move from the "Deathstar"
        if (droneMoveTargets.Count < 1)
        {
            foreach (Transform child in droneTargetHolder.transform)
            {
                droneMoveTargets.Add(child);
            }
            Debug.Log(droneMoveTargets.Count);            
        }
    }
          
}
