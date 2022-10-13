using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class MenuSpawner : MonoBehaviour
{

    public static MenuSpawner ins;
    public MenuScript menuPrefab;
   
    private void Awake()
    {
        ins = this;
    }

    public void SpawnMenu(Interactable obj)
    {
        MenuScript newMenu = Instantiate(menuPrefab) as MenuScript;
        newMenu.transform.SetParent(transform, false);
        
        newMenu.SpawnButtons(obj);
    }
}
