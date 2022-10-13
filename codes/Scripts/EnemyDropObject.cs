using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using RayFire;


public class EnemyDropObject : MonoBehaviour
{
    // Editor fields
    [SerializeField, Tooltip("Delay of the objects explosion.")]
    public float explosionDelay;
    [SerializeField, Tooltip("Explosion effect")]
    private GameObject explosionEffect;
    [SerializeField, Tooltip("How far does the explosion affect.")]
    private float damageRadius;
    [SerializeField, Tooltip("Damage done by this objects explosion.")]
    private float damage;
    [SerializeField, Tooltip("If this is checked, uses RayfireBomb to add force to hit rigidbodies.")]
    private bool isBomb;

    // Non-editor fields
    private CoroutineHandle delayHandle;

    private void Awake()
    {
        delayHandle = Timing.RunCoroutine(delayExplosion());
    }

    private void explode()
    {
       // Debug.Log(transform.position);
        GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        // If transform is a bomb and has RayfireBomb-component in it
        if(isBomb)
        transform.GetComponent<RayfireBomb>().Explode(0f);
        Destroy(explosion, 2f);
        Destroy(transform.gameObject);
    }

    IEnumerator<float> delayExplosion()
    {
        yield return Timing.WaitForSeconds(explosionDelay);       
        explode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Player")
        {
            explode();
            if (delayHandle.IsRunning) Timing.KillCoroutines(delayHandle);
            collision.transform.GetComponent<AttributeSystemScript>().RemoveHealth(damage);
        }
        
    }

}
