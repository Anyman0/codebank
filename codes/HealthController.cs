using MEC;
using RayFire;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VD;


public enum typeOfDamageable
{
    NPC,
    Player,
    Destructible
}

public class HealthController : MonoBehaviour, IDamageable
{

    // Inspector fields
    [SerializeField]
    public float Health;
    [SerializeField]
    private string damageEffect;
    [SerializeField]
    private string destructionEffect;
    [SerializeField] private float damageCooldown;
    [SerializeField, Tooltip("How much score player gets from destroying this.")]
    private float scoreIncrementOnDestruction;
    [SerializeField] private typeOfDamageable type;
    [SerializeField, Tooltip("Determines which layers will damage this object.")] private LayerMask damageLayers;
    // Delay death attributes
    [Space(10)]
    [Header("Delay death attributes.")]
    [SerializeField] private bool delayedDeath;
    [SerializeField] private string deathAnimation = "die";
    [SerializeField] private float deathDelayTimer;
    [SerializeField, Tooltip("Do we run Invoke when health reaches 0?")] private bool useInvoke;
    [Header("Invoke when HP <= 0f")]
    [SerializeField] private UnityEvent toInvoke;
    


    // Non-Inspector fields
    private float maxHealth;
    private UIHandler UI;
    private bool isPlayer;
    private float deathAnimLength;
    private bool gotUI;
    private bool incremented;
    private bool canDamage = true;
    private int hitLayer;
    public bool isImmune;

  
    
    DamageableType IDamageable.Type => throw new System.NotImplementedException();

    private void Awake()
    {        
        maxHealth = Health;
        if (type == typeOfDamageable.Player)
        {
            isPlayer = true;            
        }        
        try
        {
            if (UI == null)
            {
                UI = GameObject.FindGameObjectWithTag("UI").GetComponent<UIHandler>();
                gotUI = true;
            }
                
        }
        catch
        {
            Debug.Log("There is no object tagged 'UI'");
            gotUI = false;
        }        
        
    }


    private void Update()
    {
        if(Health <= 0)
        {            
            if(!transform.CompareTag("MenuButton"))
            {
                Die();
                if(useInvoke)
                {
                    toInvoke.Invoke();
                }
            }
            
            else if(transform.CompareTag("MenuButton"))
            {
                MenuButtonClicked(transform.gameObject);
            }
            
        }
        
    }

    IEnumerator<float> DespawnEffect(GameObject effect)
    {
        yield return Timing.WaitForSeconds(2f);
        ItemManager.Instance.Despawn(effect);
    }

    // << Add and Remove Health >>
    public void RemoveHealth(float damage)
    {       
        if (((1 << hitLayer) & damageLayers) != 0 && canDamage && hitLayer != LayerMask.NameToLayer("ContinuousDamage"))
        {
            Health -= damage;
            if (isPlayer && gotUI)
                UI.ModifyHealth(-damage);

            if (damageEffect != null)
            {
                var effect = ItemManager.Instance.Spawn(damageEffect, transform.position, Quaternion.identity);
                Timing.RunCoroutine(DespawnEffect(effect));
            }

            Timing.RunCoroutine(DamageCooldown());
        }

        else if(hitLayer == LayerMask.NameToLayer("ContinuousDamage"))
        {
            Health -= damage;
            if (isPlayer && gotUI)
                UI.ModifyHealth(-damage);

            if (damageEffect != null)
            {
                var effect = ItemManager.Instance.Spawn(damageEffect, transform.position, Quaternion.identity);
                Timing.RunCoroutine(DespawnEffect(effect));
            }            
        }
        
    }

    public void AddHealth(float health)
    {
        if (Health + health > maxHealth)
        {
            health = maxHealth - Health;
            Health += health;
        }
        else
            Health += health;

        if (isPlayer && gotUI)
            UI.ModifyHealth(health);
    }

    
    public bool TakeDamage(float damageToTake)
    {                     
        RemoveHealth(damageToTake);
        //Debug.Log( transform.name+ "is hit from object in layer " + hitLayer);
        return Health <= 0;
    }

    public void GetLayer(int layer)
    {
        hitLayer = layer;        
    }

    public bool AddHP(float hpToAdd)
    {
        AddHealth(hpToAdd);
        return Health >= maxHealth;
    }


    private void Die()
    {        
                               
        if (!delayedDeath)
        {
            // Increment score here based on whats destroyed. 
            if (type == typeOfDamageable.NPC && gotUI) // If this is on NPC
            {
                UI.score += scoreIncrementOnDestruction;
            }
            if (type == typeOfDamageable.Destructible && gotUI)
            {
                UI.score += scoreIncrementOnDestruction;               
            }
            if(type == typeOfDamageable.Destructible)
            GetComponent<RayfireRigid>().Demolish();

            if (destructionEffect != null)
            {
                var effect = ItemManager.Instance.Spawn(destructionEffect, transform.position, Quaternion.identity);
                Timing.RunCoroutine(DespawnEffect(effect));
            }
            Health = maxHealth;
            ItemManager.Instance.Despawn(gameObject);           
        }
        else if(delayedDeath)
        {            
            // Increment score here based on whats destroyed. 
            if (type == typeOfDamageable.NPC && gotUI && !incremented) // If this is on NPC
            {
                UI.score += scoreIncrementOnDestruction;
            }
            if (type == typeOfDamageable.Destructible && gotUI && !incremented)
            {
                UI.score += scoreIncrementOnDestruction;                
            }
            if (type == typeOfDamageable.Destructible)
                GetComponent<RayfireRigid>().Demolish();
            Timing.RunCoroutine(DeathDelay(gameObject).CancelWith(gameObject));
        }
                    
    }
   
    private void MenuButtonClicked(GameObject go) 
    {
        try
        {
            go.GetComponent<Button>().onClick.Invoke();
            Health = 10;
        }
        catch
        {
            Debug.Log("Couldnt invoke onClick!");
        }
    }

    IEnumerator<float> DeathDelay(GameObject go)
    {
        incremented = true;
        if(deathAnimation != null)
        {
            Animator anim = GetComponent<Animator>();
            anim.SetTrigger(deathAnimation);
            //deathAnimLength = anim.GetCurrentAnimatorStateInfo(0).length;
        }
        yield return Timing.WaitForSeconds(deathDelayTimer);
        // OR alternatively use animation length as delay
        //yield return Timing.WaitForSeconds(deathAnimLength);
        if (destructionEffect != null)
        {
            var effect = ItemManager.Instance.Spawn(destructionEffect, transform.position, Quaternion.identity);
            Timing.RunCoroutine(DespawnEffect(effect));
        }
        Health = maxHealth;
        incremented = false;
        ItemManager.Instance.Despawn(go);        
    }

    IEnumerator<float> DamageCooldown()
    {
        canDamage = false;
        yield return Timing.WaitForSeconds(damageCooldown);
        canDamage = true;
    }

    //bool IDamageable.TakeDamage(float damageToTake)
    //{
    //    throw new System.NotImplementedException();
    //}

    //bool IDamageable.TakeDamage(float damageToTake, Vector3 damagePoint)
    //{
    //    throw new System.NotImplementedException();
    //}

    public bool TakeDamage(float damageToTake, Vector3 damagePoint)
    {
        return TakeDamage(damageToTake);
    }
}
