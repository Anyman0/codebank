using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CactusEnemyScript : MonoBehaviour
{

    // Editor fields
    [SerializeField, Tooltip("Target is object tagged 'Player' if left empty")]
    private GameObject target;
    [SerializeField, Tooltip("When will the cactus attack.")]
    private float attackDistance = 3;    
    [SerializeField]
    private float damage;
    [SerializeField]
    private GameObject attackSound;
    [Space(10)]
    [Header("IGNORE BELOW IF ENRAGE NOT NEEDED.")]
    //Enrage Fields
    [SerializeField, Tooltip("Determines if the cactus can go mad or not xd")]
    private bool canEnrage;
    [SerializeField]
    private float enlargeSize;
    [SerializeField]
    private Material enrageMaterial;
    [SerializeField, Tooltip("What chance does the cactus have of enraging.")]
    private float enlargeChance;
    [SerializeField, Tooltip("When does it try to enrage.")]
    private float enrageDistance;
    [SerializeField, Tooltip("How long does it take the cactus to grow into enlarged size.")]
    private float enrageTime;
    [SerializeField]
    private float enrageDamage;




    // Non-Editor fields
    private Animator animator;
    private bool hitPlayer;
    private Collider col;
    private bool enRageTrigger;
    private bool setEnrageValues;

    private void Awake()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();       
    }

    private void Update()
    {
               
        if ((transform.position - target.transform.position).magnitude < attackDistance)
        {            
            Attack();            
        }
        
        if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {            
            hitPlayer = false;
        } 

        if((transform.position - target.transform.position).magnitude < enrageDistance && canEnrage && !enRageTrigger)
        {
            var rnd = Random.Range(0, enlargeChance);
            if (rnd == enlargeChance)
            {
                if(!setEnrageValues)
                {
                    attackDistance = attackDistance + enlargeSize / 2;
                    damage = enrageDamage;
                    foreach (Transform c in transform)
                    {                        
                        try
                        {
                            c.GetComponent<SkinnedMeshRenderer>().material = enrageMaterial;
                        }
                        catch
                        {

                        }
                    }
                    setEnrageValues = true;
                }
                
                transform.localScale = Vector3.Slerp(transform.localScale, new Vector3(enlargeSize, enlargeSize, enlargeSize), enrageTime);
            }
            else if(rnd != enlargeChance)
            {
                enRageTrigger = true;
            }
            if(transform.localScale.x > enlargeSize-0.15f)
            {
                enRageTrigger = true;
            }
        }
    
    }

    
    private void Attack()
    {
        // Still missing: Play the attackSound
        animator.SetTrigger("Attack");       
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.CompareTag("Player") && !hitPlayer)
        {
            Debug.Log("HIT A PLAYER");
            target.GetComponent<AttributeSystemScript>().RemoveHealth(damage);
            hitPlayer = true;            
        }
        
    }
    

}
