using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VD;

public class ObstacleScript : MonoBehaviour
{
    [SerializeField]
    private bool destroyOnCollision;
    public float damage;


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            collision.transform.GetComponent<HealthController>()?.GetLayer(gameObject.layer);
            collision.transform.GetComponent<HealthController>()?.TakeDamage(damage);
            if(destroyOnCollision)
            ItemManager.Instance.Despawn(gameObject);
        }
    }
   
}
