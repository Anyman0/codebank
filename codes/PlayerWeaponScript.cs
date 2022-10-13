using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using VD;
using VD.Infinity;

public class PlayerWeaponScript : InfinityWeapon
{
    // Inspector fields
    [SerializeField]
    public float weaponHeat;
    [SerializeField]
    private float heatPerShot;
    [SerializeField]
    private float heatIncrement;
    [SerializeField]
    private float coolingDownTime;
    [SerializeField]
    private string coolingDownEffect; 
    [SerializeField, Tooltip("Heat increment after cooling ends.")]
    private float incrementAfterCD;
    [SerializeField]
    private SteamVR_Input_Sources weaponHand;
    [SerializeField]
    private string throwableID;   
    [SerializeField]
    private GameObject UI;

    // Non-inspector fields
    private bool isCooling;
    private float maxHeat = 0;
    private UIHandler UIhandler;

    private CoroutineHandle passiveCoolingHandle;


    private new void Awake()
    {
        base.Awake();
        if(maxHeat == 0)
        maxHeat = weaponHeat;
        if(UIhandler == null)
        UIhandler = UI.GetComponent<UIHandler>();
    }

    private new void Update()
    {

        base.Update();

        if(!isCooling && weaponHeat > heatPerShot)
        {           
            if (SteamVR_Actions.default_GrabPinch.GetStateDown(weaponHand))
            {
                Shoot();
            }
            if(SteamVR_Actions.default_Grenade.GetStateDown(weaponHand) && UIhandler.throwableCount > 0)
            {
                EquipThrowable(throwableID);
            }
        }

        else if(weaponHeat < heatPerShot && !isCooling)
        {
            Timing.RunCoroutine(CoolingDown());
        }        
    }

    private void FixedUpdate()
    {
        if(weaponHeat < maxHeat && !passiveCoolingHandle.IsRunning && !isCooling)
        passiveCoolingHandle = Timing.RunCoroutine(PassiveCoolingPerSecond());
    }


    private void Shoot()
    {
        weaponHeat -= heatPerShot;        
        Use();
    }

    private void EquipThrowable(string throwable) 
    {
        var go = ItemManager.Instance.Spawn(throwable, currentState.muzzleSockets[0].transform.position, Quaternion.identity, transform);
        enabled = false;
    }

    // IEnumerators 
    IEnumerator<float> PassiveCoolingPerSecond() 
    {
        yield return Timing.WaitForSeconds(1f);        
        weaponHeat += heatIncrement;
    }
    IEnumerator<float> CoolingDown()
    {
        isCooling = true;
        if(coolingDownEffect != null)
        {
            var cdEffect = ItemManager.Instance.Spawn(coolingDownEffect, transform.position, Quaternion.identity);
            DespawnEffect(cdEffect, coolingDownTime);
        }        
        yield return Timing.WaitForSeconds(coolingDownTime);
        weaponHeat += incrementAfterCD;
        isCooling = false;
    }
    IEnumerator<float> DespawnEffect(GameObject effect, float cdTime)
    {
        yield return Timing.WaitForSeconds(cdTime);
        ItemManager.Instance.Despawn(effect);        
    }

}
