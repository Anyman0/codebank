using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Boss_0 : MonoBehaviour
{
    // Inspector fields
        // Floats
    [SerializeField, Tooltip("Interval between bosses abilities.")] private float interval;
    [SerializeField] private float fatigue;  
    [SerializeField] private float fatigueDrainedPerAbility;   
    [SerializeField, Tooltip("Determines how much time does the player have to damage the boss when its fatigue reaches 0.")] private float windowOfOpportunity;
        // Effects    
    [SerializeField, Tooltip("Enable/Disable this shield transform.")] private Transform shieldEffect;


    // Non-inspector fields
        // Handles
    private CoroutineHandle abilityHandle;
    private CoroutineHandle wooHandle;    
    private float maxFatigue;    
    private int nextAbility = 0;    
    private HealthController bossHC;
    private float health;
    private int phase;

    private void Update()
    {
        if(!abilityHandle.IsRunning && !wooHandle.IsRunning)
        {
            abilityHandle = Timing.RunCoroutine(StartAbility());
        }
        // Keep the health updated.
        health = bossHC.Health;
        // Determine the phase we are on based on bosses health. 
        if (health >= 70) phase = 1;
        else if (health > 50 && health < 70) phase = 2;
        else if (health > 20 && health < 50) phase = 3;
        else if (health <= 20) phase = 4;

        // BELOW IS PURELY FOR DEBUGGING
        if(Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Current phase is: " + phase);
        }

        // IF fatigue reaches 0, disable shield and make the boss damageable.
        if(fatigue <= 0)
        {            
            if(!wooHandle.IsRunning)
            {
                Timing.KillCoroutines(abilityHandle);
                Shield();
                wooHandle = Timing.RunCoroutine(DamageWindow());
            }
        }
    }

    private void Awake()
    {
        maxFatigue = fatigue;
       
        bossHC = GetComponent<HealthController>();       
    }

    private void OnEnable()
    {
        // Activate bosses shield as the battle begins.
        Shield();
    }

    // BOSS ABILITIES
    private void EarthQuake()
    {
        // Do the actual QUAKE
        Debug.Log("EARTHQUAKE");
    }

    private void Whip()
    {
        // Do the actual WHIP
        Debug.Log("WHIPPED");
    }

    private void Smash()
    {
        // Do the actual SMASH
        Debug.Log("SMASHED");
    }

    private void Shield()
    {
        if(shieldEffect.gameObject.activeSelf)
        {
            shieldEffect.gameObject.SetActive(false);            
            bossHC.enabled = true;
        }
        else
        {
            shieldEffect.gameObject.SetActive(true);            
            bossHC.enabled = false;
        }
    }

    IEnumerator<float> StartAbility()
    {
        yield return Timing.WaitForSeconds(interval);
        if(phase == 1)
        {
            if(nextAbility == 0)
            {
                Smash();
            }            
        }
        else if(phase == 2)
        {
            if(nextAbility == 0)
            {
                Whip();
                nextAbility++;
            }
            else if(nextAbility == 1)
            {
                Smash();
                nextAbility = 0;
            }
           
        }
        else if(phase == 3 || phase == 4)
        {
            if(nextAbility == 0)
            {
                EarthQuake();
                nextAbility++;
            }
            else if(nextAbility == 1)
            {
                Whip();
                nextAbility++;
            }
            else if(nextAbility == 2)
            {
                Smash();
                nextAbility = 0;
            }
        }       

        fatigue -= fatigueDrainedPerAbility;        
    }

    IEnumerator<float> DamageWindow()
    {
        yield return Timing.WaitForSeconds(windowOfOpportunity);
        fatigue = maxFatigue;
        Shield();
    }
    

/*BOSS MECHANICS EXPLAINED BELOW
100% HP ---> 
Shield
Smash
Interval 5

70%  HP --->  
Shield
Smash
Whip
Interval 5

50% HP ---> 
Shield
Smash
Whip
Earth Quake
Interval 5

20% HP ---> 
Shield
Smash
Whip
Earth Quake
Interval 3

Skills: 

- Earth Quake
  * Creates a crack in the middle of the environment, 
    forcing the player to either side of the map.
- Whip
  * Lashes a gigantic whip towards player in vertical angle,
    forcing the player to either slide under or jump over it.
- Smash
  * Smashes the player with a gigantic hammer. 
    It's about 3 times the size of the player. Player sees the shadow of where the 
    hammer is going to land.
- Shield
  * Boss is covered with a shield that blocks all incoming damage.
    Shield falls when bosses fatigue runs out after using it's power
    on skills to destroy player.It recovers and battle continues.
   Player needs to damage the boss in these "windows" of opportunities.

*/
}
