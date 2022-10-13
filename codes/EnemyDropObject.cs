using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using RayFire;
using VD;

[RequireComponent(typeof(Collider))]
public class EnemyDropObject : MonoBehaviour
{
    // Editor fields
    [SerializeField, Tooltip("Delay of the objects explosion.")]
    public float explosionDelay;
    [SerializeField, Tooltip("Explosion effect")]
    private string explosionEffect;
    [SerializeField, Tooltip("How far does the explosion affect.")]
    private float damageRadius;
    [SerializeField, Tooltip("Damage done by this objects explosion.")]
    private float damage;
    [SerializeField, Tooltip("If this is checked, uses RayfireBomb to add force to hit rigidbodies.")]
    private bool isBomb;
    [SerializeField, Tooltip("Determines what does the bomb hit.")]
    private LayerMask layersToAffect;

    // Non-editor fields
    private CoroutineHandle delayHandle;

    private void Explode() 
    {
        var explosion = ItemManager.Instance.Spawn(explosionEffect, transform.position, Quaternion.identity);
        Timing.RunCoroutine(DespawnEffect(explosion));                
        transform.GetComponent<RayfireBomb>().Explode(0f);       
        DealDamage();
        ItemManager.Instance.Despawn(gameObject);
    }

    IEnumerator<float> DespawnEffect(GameObject effect)
    {
        yield return Timing.WaitForSeconds(2f);
        ItemManager.Instance.Despawn(effect);
    }

    IEnumerator<float> DelayExplosion()
    {
        yield return Timing.WaitForSeconds(explosionDelay);       
        Explode();
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && isBomb)
        {
            Explode();           
            if (delayHandle.IsRunning) Timing.KillCoroutines(delayHandle);
        }
    }

    private void DealDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius, layersToAffect);
        int i = 0;
        while (i < hitColliders.Length)
        {
            try
            {
                hitColliders[i].transform.GetComponent<HealthController>().TakeDamage(damage);
            }
            catch
            {

            }
            i++;
        }
    }

    private void OnDisable()
    {
        Timing.KillCoroutines(delayHandle);
    }

    private void OnEnable()
    {
        delayHandle = Timing.RunCoroutine(DelayExplosion());
    }

}
