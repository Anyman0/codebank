using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using VD;

public class ProjectileScript : MonoBehaviour
{
    [SerializeField, Tooltip("Lifetime of the fired projectile.")]
    public float lifeTime;
    [SerializeField, Tooltip("Projectile damage for Charge - / and  Droneprojectiles. Default at 10.")]
    public float projectileDamage = 10f;
    [SerializeField, Tooltip("Prefab of charge-/droneprojectile hit effect here. ")]
    private GameObject projectileHitEffect;

    WeaponScript ws;

    private void Awake()
    {
        
        
    }

    private void Update()
    {
                             
    }

    /*public void DestroyProjectile()
    {
        Destroy(this.gameObject);        
    }*/

    private void OnCollisionEnter(Collision collision)
    {


        if (projectileHitEffect != null)
        {
            GameObject effect = Instantiate(projectileHitEffect, this.transform.position, Quaternion.identity);           
            Destroy(effect, 2f);
        }
              
        try
        {
            collision.transform.GetComponent<AttributeSystemScript>().RemoveHealth(projectileDamage);
            
        }
        catch
        {
            Debug.Log("Can't damage! There is no AttributeSystemScript on hit object!");
        }

        /*if(transform.tag != "ChargeProjectile")
        DestroyProjectile();
        else 
        {
            ws = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<WeaponScript>();
            if (collision.transform.tag == "Drone")
            {               
               ws.StopCoroutine(ws.destroyRoutine);
                ws.projectileHolder.Remove(transform);
            }
             
            else
            {                
                ws.StopCoroutine(ws.destroyRoutine);
                ws.projectileHolder.Remove(transform);
            }
            
            DestroyProjectile();
        }*/

    }

    public IEnumerator<float> destroyProjectile()
    {
        yield return Timing.WaitForSeconds(lifeTime);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        ItemManager.Instance.Despawn(gameObject);
    }
}
