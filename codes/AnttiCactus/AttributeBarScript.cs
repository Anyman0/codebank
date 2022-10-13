using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using MEC;

public class AttributeBarScript : MonoBehaviour
{
   
    // Attribute fields declared here    
    private float maxHealth;
    [HideInInspector]
    public float maxHeat;
    [HideInInspector]
    public float currentHealth;
    [SerializeField, Tooltip("What nades.(Nade prefab)")]
    private GameObject grenadeCount;
    [SerializeField, Tooltip("Grenade Icon holder.")]
    private GameObject grenadeIconHolder;
    [SerializeField, Tooltip("Chargevalue gameobject. (Controller(right/left))")]
    private GameObject heatValueGO;
    [SerializeField, Tooltip("UI text object of players plasmagun charge.")]
    private GameObject heatTextField;  
    /*[SerializeField, Tooltip("UI healthbar object.")]
    private Image healthBar;*/
    [SerializeField, Tooltip("This value is used to smooth healthbar fill/unfill.")]
    public float updateSpeed;
    [SerializeField, Tooltip("Prefab containing Score- text component.")]
    private GameObject scoreText;    
    [SerializeField, Tooltip("This determines how much the score increments automatically during time.")]
    public float autoScoreIncrement;
    [HideInInspector]
    public float score = 0f;

    private List<Transform> newHealthBars;
    private List<Transform> grenadeIcons;
    private float currentHealthPercent;

    public event Action<float> OnHealthPercentChanged = delegate { };

    private void Awake()
    {
        //OnHealthPercentChanged += HandleHealthChange;
        maxHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<AttributeSystemScript>().Health;
        maxHeat = heatValueGO.GetComponent<WeaponScript>().weaponHeat;
        newHealthBars = new List<Transform>();
        foreach(Transform hpBar in GameObject.Find("HPBars").transform)
        {
            newHealthBars.Add(hpBar);            
        }
        grenadeIcons = new List<Transform>();
        foreach(Transform nade in grenadeIconHolder.transform)
        {
            grenadeIcons.Add(nade);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        currentHealth = maxHealth; 
    }

    // Health changes handled in below methods
    /*private void HandleHealthChange(float percentage)
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
    }*/

    public void ModifyHealth(float amount)
    {
        // IF the player is trying to consume an item that exceeds the maximum health, change the amount to be exactly enough to reach max health. 
        if (currentHealth + amount <= maxHealth)
        {
            currentHealth += amount;
            currentHealthPercent = (float)currentHealth / (float)maxHealth;
            //OnHealthPercentChanged(currentHealthPercent);
            Debug.Log(currentHealthPercent);
        }

        
        else if (currentHealth + amount > maxHealth)
        {
            amount = maxHealth - currentHealth;
            currentHealth += amount;
            currentHealthPercent = (float)currentHealth / (float)maxHealth;
            //OnHealthPercentChanged(currentHealthPercent);
        }

        if(amount < 0) // If taking damage
        {
            List<Transform> activeBars = new List<Transform>();
            foreach (Transform hpBar in newHealthBars)
            {
                if(hpBar.gameObject.activeSelf)
                {
                    activeBars.Add(hpBar);
                }               
            }
            for (int i = 1; i <= (Mathf.Abs(amount / 10)); i++)
            {
                Debug.Log(i + ". Is it smaller than " + (Mathf.Abs(amount / 10)));
                activeBars[activeBars.Count - i].gameObject.SetActive(false);
            }
        }

        else if(amount > 0) // If healing
        {
            List<Transform> deactivatedBars = new List<Transform>();
            foreach(Transform hpBar in newHealthBars)
            {
                if(!hpBar.gameObject.activeSelf)
                {
                    deactivatedBars.Add(hpBar);
                }
            }
            for(int i = 0; i <= amount / 10; i++)
            {
                deactivatedBars[0].gameObject.SetActive(true);
            }
        }

       
    }

    public void modifyNadeCount(int count)
    {
        // Count determines how many visible grenades we have in UI 
        List<Transform> visibles = new List<Transform>();
        foreach(Transform nade in grenadeIcons)
        {
            if(nade.gameObject.activeSelf)
            {
                visibles.Add(nade); // Visibles = how many visible nades we got now
            }
        }

        // In case of nade pickup
        if(visibles.Count < count)
        {
            grenadeIcons[count - 1].gameObject.SetActive(true);
        }
        // in case of throwing a nade 
        else if(visibles.Count > count)
        {
            visibles[visibles.Count - 1].gameObject.SetActive(false);
        }
    }

   

    
      
    private void FixedUpdate()  // Changed from Update to FixedUpdate. <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
    {
        // Testing health deduction and incrementing
        if (Input.GetKeyDown(KeyCode.K))
        {
            ModifyHealth(-10);            
        }
        if(Input.GetKeyDown(KeyCode.H))
        {
            ModifyHealth(10);
        }

        // WeaponHeat
        maxHeat = heatValueGO.GetComponent<WeaponScript>().weaponHeat;
        heatTextField.GetComponent<TextMeshProUGUI>().text = Math.Round(maxHeat, 2).ToString();

        // Score
        score += autoScoreIncrement;
        scoreText.GetComponent<TextMeshProUGUI>().text = Math.Round(score, 2).ToString();

    }


}
