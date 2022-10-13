using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleDamageScript : MonoBehaviour
{
    [SerializeField]
    private float damage;
    [SerializeField, Tooltip("How much damage can this object take before destruction.")]
    private float durability;
    [SerializeField, Tooltip("How much score is added by destruction of this object.")]
    private float scoreIncrement;
        

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            collision.transform.GetComponent<AttributeSystemScript>().RemoveHealth(damage);
            destroyObstacle();
        }
        else if(collision.gameObject.tag == "ChargeProjectile")
        {
            durability -= collision.gameObject.GetComponent<ProjectileScript>().projectileDamage;
            collision.transform.GetComponent<AttributeBarScript>().score += scoreIncrement;
        }
    }


    private void Update()
    {
        if(durability <= 0f)
        {
            destroyObstacle();
        }
    }

    private void destroyObstacle()
    {
        // Destroy obstacle. Shatter? Explosion?
    }
}
