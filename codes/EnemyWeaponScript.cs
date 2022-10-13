using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VD;
using MEC;


enum EnemyClassType
{
    Default,
    Bouncer, 
    Runner,
    Drone
}

public class EnemyWeaponScript : Weapon, IAim
{  
    
    // Inspector fields
    [SerializeField, Tooltip("Name of the target prefab. IE '[CameraRig]PlayerCrack'")]
    private string target;
    [SerializeField]
    private float shootingInterval;
    [SerializeField]
    private float engageDistance;
    [SerializeField]
    private EnemyClassType EnemyType;
    [SerializeField, Tooltip("Offset to get the bouncer aim towards player. (73 for BouncerBasic)")]
    private float AimOffset = 73f;
    [SerializeField, Tooltip("Offset to make the enemy aim in front of the player instead of directly at player.")] private float projectilePredictionOffset; 
    [SerializeField, Tooltip("IE. In BouncerBasics animator its 'Shoot'")] private string engageAnimationBool; 
    [SerializeField, Tooltip("Name of the Shoot-animation")] private string shootAnimationName;

    
    // Non-inspector fields
    private Transform targetTransform;
    private Transform muzzle;
    private CoroutineHandle fireHandle;
    private Animator animator;

    
      
    private new void Awake()
    {
        base.Awake();
        if (targetTransform == null) targetTransform = GameObject.Find("" + target).transform;
        
        if (muzzle == null) muzzle = currentState.muzzleSockets[0].transform;

        if(EnemyType == EnemyClassType.Bouncer || EnemyType == EnemyClassType.Runner)
        {
            animator = transform.GetComponent<Animator>();
        }
    }
    private new void Update()
    {
        base.Update(); 
             
        // Start shooting at range        
        if((transform.position - targetTransform.position).sqrMagnitude < engageDistance)
        {

            // Aim towards target
            if (EnemyType == EnemyClassType.Drone)
            {
                Aim(muzzle, targetTransform, AimOffset);
            }
            else if (EnemyType == EnemyClassType.Bouncer)
            {
                Aim(transform, targetTransform, AimOffset);
            }

            if (!fireHandle.IsRunning)
            fireHandle = Timing.RunCoroutine(FireProjectile());
        }

        else
        {
            if(EnemyType == EnemyClassType.Bouncer)
            {
                animator.SetBool(""+engageAnimationBool, false);
                Timing.KillCoroutines(fireHandle);                
            }
        }
       
    }


    IEnumerator<float> FireProjectile() 
    {

        if (EnemyType == EnemyClassType.Bouncer)
        {
            animator.SetBool(""+engageAnimationBool, true);            
        }

        yield return Timing.WaitForSeconds(shootingInterval);

        if(EnemyType == EnemyClassType.Bouncer)
        {
            animator.Play(""+shootAnimationName, 1);                       
        }        

        Use();
    }

    // If enemy is destroyed, stop coroutines.
    private void OnDisable()
    {
        Timing.KillCoroutines(fireHandle);
    }
  
    public void Aim(Transform objectToRotate, Transform target, float offSet) 
    {        
        var rotation = Quaternion.LookRotation(objectToRotate.position - target.position, Vector3.up);
        objectToRotate.rotation = Quaternion.Lerp(objectToRotate.rotation, rotation, Time.deltaTime * 10f);        
        //objectToRotate.LookAt(targetTransform);
        objectToRotate.LookAt(targetTransform.position + new Vector3(0, 0, projectilePredictionOffset));
        objectToRotate.rotation *= Quaternion.Euler(0, offSet, 0);
    }
}
