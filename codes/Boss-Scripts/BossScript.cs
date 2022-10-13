using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VD;

[System.Flags]
public enum BossSkills
{    
    Smash = 1 << 1, 
    Whip = 1 << 2, 
    Earthquake = 1 << 3 
}
public class BossScript : MonoBehaviour
{

    // Inspector fields
                // SKILLS HERE IS ONLY USED TO SHOW WHAT SKILLS ARE THERE. PRETTY USELESS ATM.
    [SerializeField, Tooltip("Skills that the boss has.")] private BossSkills skills;
    [SerializeField, Tooltip("How many phases this boss has and at what HP-percentage we enter them.")] private List<int> phases;
    [SerializeField, Tooltip("Skills in each phase. Type them in format (ie. 'SmashWhipEarthquake')")] private List<string> skillsPerPhase;    
        // Floats
    [SerializeField, Tooltip("Interval between bosses abilities.")] private float interval;
    // Fatigue/Shield attributes
    [Header("Fatigue/Shield attributes.")]
    [Space(10)]
    [SerializeField] private bool useFatigueShieldSystem; 
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
    private int savedPhase;
    private string[] splitSkills;
    private int abilities = 0;
    private bool getAbilities = true;

    private void Awake()
    {
        bossHC = GetComponent<HealthController>();
        Debug.Log("BOSSHC IS: " + bossHC.name);
                
        if (useFatigueShieldSystem)
        {
            maxFatigue = fatigue;
            Shield();
        }        
    }


    private void Update()
    {

        // Keep the health updated.
        health = bossHC.Health;

        // Keep the phase up to date
        for(int i = 0; i < phases.Count; i++)
        {
            if(health >= phases[i])
            {
                phase = i;
                break;
            }
        }

        // if phase changes, setting getAbilities to true gets the next phases abilities. 
        if (savedPhase != phase) getAbilities = true;

                  
        // BELOW IS PURELY FOR DEBUGGING
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Current phase is: " + phase);
        }

        // IF we are using fatigue/shield system.
        if(useFatigueShieldSystem)
        {
            if (!abilityHandle.IsRunning && !wooHandle.IsRunning)
            {
                abilityHandle = Timing.RunCoroutine(StartAbility());
            }
            // IF fatigue reaches 0, disable shield and make the boss damageable.
            if (fatigue <= 0)
            {
                if (!wooHandle.IsRunning)
                {
                    Timing.KillCoroutines(abilityHandle);
                    Shield();
                    wooHandle = Timing.RunCoroutine(DamageWindow());
                }
            }
        }

        else if(!useFatigueShieldSystem)
        {
            if(!abilityHandle.IsRunning)
            {
                abilityHandle = Timing.RunCoroutine(StartAbility());
            }
        }
        
    }

  
    // IEnumerators
    IEnumerator<float> StartAbility()
    {
        yield return Timing.WaitForSeconds(interval);
                  
        if(getAbilities)
        {
            savedPhase = phase;
            ChangePhase(phase);                
        }            
        if (nextAbility < abilities-1)
        {               
            Invoke("" + splitSkills[nextAbility], 0f);
            nextAbility++;
        }
        else if(nextAbility >= abilities-1)
        {
            Invoke("" + splitSkills[nextAbility], 0f);
            nextAbility = 0;                              
        }
         
        // Decrement fatigue based on set abilityDrain value if we are using FatigueShieldSystem
        if(useFatigueShieldSystem)
        fatigue -= fatigueDrainedPerAbility;
    }

    IEnumerator<float> DamageWindow()
    {
        yield return Timing.WaitForSeconds(windowOfOpportunity);
        fatigue = maxFatigue;
        Shield();
    }


    private void ChangePhase(int phase)
    {
        splitSkills = skillsPerPhase[phase].Split(char.Parse("_"));
        abilities = 0;
        foreach (var a in splitSkills)
        {
            abilities++;
        }
        getAbilities = false;
    }

    // BOSS ABILITIES
    private void Earthquake()
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
        // Do the actual SMASH - Basic skill
        Debug.Log("SMASHED");
    }

    private void Shoot()
    {
        // Shoot - Basic skill
        // Shoot could be buffed based on phase. Maybe by changing shoot interval, projectile size and firemodes.
        Debug.Log("SHOT!");
    }

    private void Scream()
    {
        // Do the actual SCREAM
        Debug.Log("SCREAMED!");
    }

    // Boss phases that are not controlled by interval, but instead starts when defined. 
    private void FirePhase()
    {
        // Fire - Increasing temp. Player needs to collect Ice to cooldown.
        Debug.Log("We are in Fire-phase!");
    }

    private void IcePhase()
    {
        // Ice - The very opposite. Player needs to collect "Fire" to heat up. 
        Debug.Log("We are in Ice-phase!");
    }

    private void ToxinPhase() 
    {
        // Toxin - Player needs to either get in safe zone from toxin, or get an item "gasmask?" that protects from it.
        Debug.Log("We are in Toxin-phase!");
    }


    private void Shield()
    {
        if (shieldEffect.gameObject.activeSelf)
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

}
