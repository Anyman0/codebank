using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VD;
using MEC;
using System;

public class OnPickUp : MonoBehaviour
{
    // Inspector fields    
    [SerializeField]
    private float amount; 
    [SerializeField]
    private string healEffect;
    [SerializeField]
    private string healSound;
    [SerializeField]
    private pickUpType pickUp;

    // Non-inspector fields
    private UIHandler UI;
    private bool gotUI;
         
    private void Awake()
    {
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
            Debug.Log("NO UI");
            gotUI = false;
        }
        
            
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && gotUI)
        {
            IncrementValue(amount, other.gameObject);     
        }
    }

    IEnumerator<float> DespawnEffect(GameObject effect)
    {
        yield return Timing.WaitForSeconds(2f);
        ItemManager.Instance.Despawn(effect);
    }
    
    private void DespawnObject()
    {
        ItemManager.Instance.Despawn(gameObject);
    }
     
    public void IncrementValue(float amount, GameObject go)
    {
        if(pickUp == pickUpType.healingItem)
        {
            if (healEffect != null)
            {
                var effect = ItemManager.Instance.Spawn(healEffect, UI.transform.parent.transform.position, Quaternion.identity);
                Timing.RunCoroutine(DespawnEffect(effect));
            }
            go.GetComponent<HealthController>().AddHP(amount);            
        }
        else if(pickUp == pickUpType.throwable)
        {
            if(UI.throwableCount < 3)
            {
                UI.throwableCount += Convert.ToInt32(amount);
                UI.ModifyNadeCount(Convert.ToInt32(amount));
                DespawnObject();
            }
            
        }
    }
}
