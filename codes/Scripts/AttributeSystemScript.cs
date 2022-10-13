using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class AttributeSystemScript : MonoBehaviour
{
    [SerializeField, Tooltip("How much health does this entity have.")]
    public float Health;
    [SerializeField, Tooltip("Prefab slot for 'take damage' - effect.")]
    protected GameObject DamageEffect;
    [SerializeField, Tooltip("Prefab slot for destruction - effect.")]
    protected GameObject DestructionEffect;

    private GameObject destruction;

    public bool isPlayer;
       
    private void Awake()
    {
       
        
    }



    private void Update()
    {
        if(Health <= 0)
        {
            if(DestructionEffect != null)
            {
                if (destruction == null)
                {
                    destruction = Instantiate(DestructionEffect, transform.position, Quaternion.identity);
                }
                Destroy(destruction, 2f);                
            }
            Die();
        }
       
      
    }
    
    public void RemoveHealth(float damage)
    {
        
        Health -= damage;

        if (isPlayer) GameObject.FindGameObjectWithTag("UI").GetComponent<AttributeBarScript>().ModifyHealth(-damage);  
        
        if(DamageEffect != null)
        {
            GameObject tookDamage = Instantiate(DamageEffect, transform.position, Quaternion.identity);
            Destroy(tookDamage, 2f);
        }         

    }

    private void Die()
    {   
        // Increment score here based on whats destroyed. 
        if(transform.gameObject.layer == 30) // If transform is on EnemyTarget layer, increment score by 10.
        {
            GameObject player = GameObject.FindGameObjectWithTag("UI");
            player.GetComponent<AttributeBarScript>().score += 10f;
            if (transform.gameObject.tag == "EnemyThrower") transform.GetComponent<EnemyThrowableScript>().explodeOnDestroy();
        }
        
        if(transform.tag == "Drone")
        {
            Timing.KillCoroutines(GetComponentInChildren<EnemyAimScript>().fireHandle);
            Timing.KillCoroutines(GetComponentInChildren<EnemyAimScript>().destroyHandle);
        }
        Destroy(gameObject); 
    }



}
