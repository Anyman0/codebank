using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class StartMenuScript : MonoBehaviour
{
    [Space(10)]
    [Header("Menu must be tagged 'Menu'")]
    
    [SerializeField, Tooltip("Hand used to aim and shoot")]
    private GameObject aimHand;
       

    private void Awake()
    {
        
    }

    private void Update()
    {
        RaycastHit hit;

        // For debugging
        Debug.DrawRay(aimHand.transform.position, aimHand.transform.forward, Color.magenta);

        // Raycast select for menu - buttons
        if(Physics.Raycast(aimHand.transform.position, aimHand.transform.forward, out hit))
        {
            if (hit.collider.transform.tag == "MenuButton")
            {
                var btn = hit.collider.transform.GetComponent<Button>();
                btn.Select();                
                if (SteamVR_Actions.default_GrabPinch.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    buttonClick(btn);
                }

            }
            
        }
    }

    // call the buttons assigned OnClick - method.
    private void buttonClick(Button btn)
    {
        Debug.Log(btn.transform.name);
        btn.onClick.Invoke();
    }


    // OnClick methods defined below
    public void ResumeClick()
    {
        Debug.Log("Clicked the resume button!");
    }

    public void NewGameButton()
    {
        Debug.Log("Clicked the New Game - button.");
    }

    public void DunnoButton()
    {
        Debug.Log("Clicked dunno - button.");
    }

    
}
