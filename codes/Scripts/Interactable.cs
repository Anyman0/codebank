using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Interactable : MonoBehaviour
{
   
    public SteamVR_Action_Vector2 JoystickAction = null;
    private GameObject menuObject;

    [HideInInspector]
    public bool usingAttributes;

    [HideInInspector]
    public GameObject consumable;

    [HideInInspector]
    public int healthChange;
    public enum categories
    {
        Weapon = 0,
        Consumable = 1       
    }

    public enum handToUse
    {
        RightHand = 0, 
        LeftHand = 1
    }

    [System.Serializable]
    public class Action 
    {
        [HideInInspector]
        public Color color;       
        public Sprite sprite;                         
        public GameObject item;
        [HideInInspector]
        public int healthChange;
        public categories Categories; 
       
    }
    public handToUse weaponHand;
    public Action[] options;

    [Tooltip("If you are using attributes such as health in your game, add the AttributeCanvas here.")]
    public GameObject attributeCanvas;

    private void Update()
    {
        menuObject = GameObject.FindGameObjectWithTag("Menu");
        ThumbStick();        
    }


    private void ThumbStick()
    {
        if(JoystickAction.axis == Vector2.zero)
        {
            return;
        }
        
        if(menuObject == null)
        {
            MenuSpawner.ins.SpawnMenu(this);
        }
              
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Interactable))]
    public class RandomScript_Editor : Editor
    {
        public override void OnInspectorGUI()
        {

            DrawDefaultInspector(); // for other non-HideInInspector fields

            Interactable script = (Interactable)target;
            // draw checkbox for the bool
            script.usingAttributes = EditorGUILayout.Toggle("Using attributes", script.usingAttributes);

            if (script.usingAttributes) // if bool is true, show other fields
            {
                foreach (var option in script.options)
                {

                    if (option.Categories.ToString() == "Consumable")
                    {
                        script.consumable = EditorGUILayout.ObjectField("Consumable", option.item, typeof(GameObject), true) as GameObject;
                        script.healthChange = EditorGUILayout.IntField("Health change", option.healthChange);
                        option.healthChange = script.healthChange; // Changing the value of the Action[] options.health change too
                    }
                }



            }


        }
    }
#endif




}
