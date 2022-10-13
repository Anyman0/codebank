using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneSpawner : MonoBehaviour
{
    [SerializeField, Tooltip("List of spawnable drones")]
    private List<GameObject> spawnableDrones;
    [SerializeField, Tooltip("This value determines how often do we spawn a drone. This is in seconds.")]
    private float spawnTimer;

    private float timer;
    private float seconds;
    
    private void Awake()
    {
        
    }
    private void OnGUI()
    {
        //GUILayout.Label(string.Format( "                                                        " + " Seconds passed: " + seconds));
    }
    private void Update()
    {
        timer += Time.deltaTime;
        seconds = timer % 60;

        if (seconds >= spawnTimer && GameObject.FindGameObjectWithTag("Drone") == null) // Currently just one drone at a time
        {           
            SpawnDrone(0);
            timer = 0;
        }
    }


    private void SpawnDrone(int droneNumber)
    {
        GameObject droneClone;
        Vector3 dronePositionOffset = new Vector3(0, 0, 50f);
        Vector3 dronePos = this.transform.position + dronePositionOffset;
        droneClone = Instantiate(spawnableDrones[droneNumber], dronePos, Quaternion.identity);
    }

}
