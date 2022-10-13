using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class MenuScript : MonoBehaviour
{

    public MenuButtonScript buttonPrefab;  

    [HideInInspector]
    public MenuButtonScript selectedButton;

    public SteamVR_Action_Vector2 JoystickAction = null;  
    
    [HideInInspector]
    public List<MenuButtonScript> selectedButtonsList; // Creating a list of all the items declared in Interactable class

    private GameObject weaponHand;
      

    // Spawning all the predefined buttons to the radial inventory menu.
    public void SpawnButtons(Interactable obj)
    {
        
        for(int i = 0; i < obj.options.Length; i++)
        {
            MenuButtonScript newButton = Instantiate(buttonPrefab) as MenuButtonScript;
            newButton.transform.SetParent(transform, false);           
            // Counting the positions in the circle based on the amount of predefined items, so that all the items have the same distance between each other.
            float theta = (2 * Mathf.PI / obj.options.Length) * i; // Distance around circle
            float xPos = Mathf.Sin(theta);
            float yPos = Mathf.Cos(theta);
            
            newButton.transform.localPosition = new Vector3(xPos, yPos, 0f) * 10f;               
            newButton.Icon.sprite = obj.options[i].sprite;
            newButton.circle.color = obj.options[i].color;           
            newButton.category = obj.options[i].Categories.ToString();
            newButton.pickedItem = obj.options[i].item;
            newButton.healthChange = obj.options[i].healthChange;
            selectedButtonsList.Add(newButton); // Populating the list of items
            newButton.myMenu = this;                      
        }      
    }
   
    void Update()
    {
             
        if(weaponHand == null)
        {
            var WeaponHand = GameObject.FindGameObjectWithTag("Player").GetComponent<Interactable>().weaponHand;
            if (WeaponHand == Interactable.handToUse.LeftHand) weaponHand = GameObject.FindGameObjectWithTag("LeftHand");
            else if (WeaponHand == Interactable.handToUse.RightHand) weaponHand = GameObject.FindGameObjectWithTag("RightHand");
            Debug.Log("Weaponhand was null, so: " + weaponHand + " is now added.");
        }

        // If joystick is released, close the inventory menu
        if (JoystickAction.axis == Vector2.zero) 
        {            
            Destroy(gameObject);
            try
            {
                Debug.Log("Menu closed. Last item picked: " + selectedButton.pickedItem + " . Previous item spawned: " + buttonPrefab.previousItem);
                //buttonPrefab.SpawnAndConsume(selectedButton, weaponHand);
                
            }
            catch
            {
                Debug.Log("Whoopsie. No item was chosen :'( ");
            }
                                  
        }

        // Spawning the item after player presses grip on the chosen item. 
        if(SteamVR_Actions.default_GrabGrip.GetStateDown(SteamVR_Input_Sources.RightHand))
        {           
            buttonPrefab.SpawnAndConsume(selectedButton, weaponHand);
        }
                                              

        // This loops through each button in the list of buttons and if the joystick is on a button, assigns that value to the selectedButton, 
        // which is then used in MenuButtonScripts method ChosenItem. 
        if (JoystickAction.axis != Vector2.zero)
        {
            Debug.Log(JoystickAction.axis);
            int i = 0;
            foreach(var btn in selectedButtonsList)
            {               
                if(Vector2.Angle(JoystickAction.axis, selectedButtonsList[i].transform.localPosition) < 10f) // Was 10. Decreasing this value makes it more accurate?
                {
                    //Debug.Log("At object number: " + i);              // Debug to see the object we're at.
                    selectedButton = selectedButtonsList[i];
                }               
                i++;
            }
        }         

    }

}
