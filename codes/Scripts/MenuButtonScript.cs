using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using Valve.VR;
using System;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;
using Mirror;

public class MenuButtonScript : MonoBehaviour
{

    public Image circle;
    public Image Icon;
    [HideInInspector]
    public string category;
    [HideInInspector]
    public int healthChange;
    

    [HideInInspector]
    public GameObject pickedItem;

    [HideInInspector]
    public MenuScript myMenu;

    [HideInInspector]
    public GameObject previousItem;

    private GameObject attributeFields;
        
    Color defaultColor;

    /*
    // IPointerEnterHandler, IPointerExitHandler <<<<<--- Add these after MonoBehaviour in class constructor 
    // OnPointerEnter and OnPointerExit for testing functionalities with a mouse
    public void OnPointerEnter(PointerEventData eventData)
    {
        myMenu.selectedButton = this;
        defaultColor = circle.color;
        circle.color = Color.white;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        myMenu.selectedButton = null;
        circle.color = defaultColor;
    }
    // -------------------------------------------------------------------------------
    */

    private void Update()
    {
              
        if(myMenu.selectedButton != null)
        {            
            ChosenItem(myMenu.selectedButton);              
        }
                             
    }

    // Players chosen item. Highlighting the choice by changing the buttons background to white.
    private void ChosenItem(MenuButtonScript btn)
    {
        circle.color = defaultColor;
        myMenu.selectedButton = btn;
        defaultColor = circle.color;        
        btn.circle.color = Color.white;       
    }

    // This is a method to spawn the chosen item in players hand. Deleting the previous spawned item in the process, so the player can only hold one item at a time. 
    public void SpawnAndConsume(MenuButtonScript btn, GameObject hand) 
    {
                
        Debug.Log("Players chosen item: " + btn.pickedItem + ". And the hand to spawn it in: " + hand);   // Debugging to see the chosen item and hand to spawn it in. 
        
        attributeFields = GameObject.FindGameObjectWithTag("Player").GetComponent<Interactable>().attributeCanvas;

        try
        {
            if (btn.category == "Weapon")
            {
                if (hand != null)
                {
                    GameObject weapon = Instantiate(btn.pickedItem, hand.transform) as GameObject;
                    NetworkServer.Spawn(weapon);

                    if (previousItem == null)
                    {
                        previousItem = weapon;
                        Debug.Log("Previous item was null, so adding " + previousItem);
                    }
                    else if (previousItem != null)
                    {
                        Debug.Log("Previous item was not null, so swapping " + previousItem + " now to the new chosen item.");
                        Destroy(previousItem);
                        NetworkServer.Destroy(previousItem);
                        previousItem = null;
                        previousItem = weapon;
                        Debug.Log("And voila. Added " + previousItem + " as the new previous item.");
                    }


                }

            }

            // Consumables. TODO: What do the consumables do? Increase health, add speed etc. For example. 
            else if (btn.category == "Consumable")
            {
                GameObject consumable = btn.pickedItem;
                Debug.Log("Picked consumable was: " + consumable + ". That deducts " + btn.healthChange + " from players health." + " That health is taken from healthbar in " + attributeFields + ". Current health is: " + attributeFields.GetComponent<AttributeBarScript>().currentHealth);   
                                
                attributeFields.GetComponent<AttributeBarScript>().ModifyHealth(btn.healthChange);
            }
        }
        catch
        {
            Debug.Log("Whoopsie. What up!");
        }
               
                       
    }
    
}
