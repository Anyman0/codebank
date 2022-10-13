using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using VD;

public class AbilityController : MonoBehaviour
{
    // Inspector fields
    [SerializeField] private string ability;
    [SerializeField] private Transform abilitySpawn;   
    // Inputs 
    [SerializeField] private SteamVR_Action_Boolean useAbilityAction;
    [SerializeField] private SteamVR_Input_Sources abilityActionSource;


    // Non-inspector fields
    private GameObject spawnedAbility;
    private Transform shield;
    private bool isShield;
    private bool abilityActionState;
    private HealthController playerHC;
    


    private void Awake()
    {
        try
        {
            playerHC = GetComponent<HealthController>();
        }
        catch
        {

        }
        
        if(useAbilityAction != null)
        {            
            useAbilityAction.AddOnStateDownListener(UseAbilityAction_onStateDown, abilityActionSource);
        }

        spawnedAbility = ItemManager.Instance.Spawn(ability, Vector3.zero, Quaternion.identity, abilitySpawn);
        if (spawnedAbility.CompareTag("Shield"))
        {
            isShield = true;
            spawnedAbility.SetActive(true);
            shield = spawnedAbility.transform.GetChild(0);            
            shield.gameObject.SetActive(false);
        }
        else spawnedAbility.SetActive(false);
    }

    private void UseAbilityAction_onStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        UseAbility();
    }

    private void Update()
    {
        // Shield stuff
        if (shield.gameObject.activeSelf)
        {
            shield.transform.position = spawnedAbility.transform.position;            
            playerHC.isImmune = true;
        }
        else if (!shield.gameObject.activeSelf) playerHC.isImmune = false;
    }


    private void UseAbility()
    {
        if(isShield)
        {
            if (!shield.gameObject.activeSelf) shield.gameObject.SetActive(true);
            else shield.gameObject.SetActive(false);
        }       
        else
        {
            if (!spawnedAbility.activeSelf) spawnedAbility.SetActive(true);
            else spawnedAbility.SetActive(false);
        }        
    }

}
