using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class AttributeBarScript : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    SteamVR_Input_Sources source;

    // Attribute fields declared here
    [SerializeField]
    private int maxHealth;
    // FOR DEBUGGING
    public int currentHealth;

    [SerializeField]
    private Image healthBar;

    [SerializeField]
    public float updateSpeed;

    public event Action<float> OnHealthPercentChanged = delegate { };

    private void Awake()
    {
        OnHealthPercentChanged += HandleHealthChange;
    }

    private void OnEnable()
    {
        currentHealth = maxHealth; 
    }

    // Health changes handled in below methods
    private void HandleHealthChange(float percentage)
    {
        StartCoroutine(ChangeToPercentage(percentage));
    }

    private IEnumerator ChangeToPercentage(float percentage) // Handle health changes smoothly
    {
        float startPct = healthBar.fillAmount;
        float elapsed = 0f;

        while (elapsed < updateSpeed)
        {
            elapsed += Time.deltaTime;
            healthBar.fillAmount = Mathf.Lerp(startPct, percentage, elapsed / updateSpeed);
            yield return null;
        }

        healthBar.fillAmount = percentage;
    }

    public void ModifyHealth(int amount)
    {
        // IF the player is trying to consume an item that exceeds the maximum health, change the amount to be exactly enough to reach max health. 
        if (currentHealth + amount <= maxHealth)
        {
            currentHealth += amount;
            float currentHealthPercent = (float)currentHealth / (float)maxHealth;
            OnHealthPercentChanged(currentHealthPercent);
        }

        
        else if (currentHealth + amount > maxHealth)
        {
            amount = maxHealth - currentHealth;
            currentHealth += amount;
            float currentHealthPercent = (float)currentHealth / (float)maxHealth;
            OnHealthPercentChanged(currentHealthPercent);
        }
        
              
    }


    // TODO: ADD MORE POSSIBLE ATTRIBUTES





    // USELESS FOR NOW. TODO: Add input-based actions for many things such as shooting. 
    /*  
    private void Update() 
    {
        if (SteamVR_Input.GetStateDown("Left_Click_X", source)) // Testing health reduction
        {
            ModifyHealth(-10);
        }

        
    }*/


}
