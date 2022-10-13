using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class BridgeVehicleSpawnerScript : MonoBehaviour
{
    // Editor fields
    [SerializeField, Tooltip("List of vehicles spawned from this spawner.")]
    public List<GameObject> vehicleList;
    [SerializeField, Tooltip("How often do we spawn a vehicle.")]
    private float spawnInterval;
    [SerializeField, Tooltip("Disables the vehicle after this value. (Figured its better to use a value here instead of a separate empty gameobject for disabling.)")]
    private float disableTimer;
    

    // Non-editor fields
    private List<GameObject> vehicleCloneList;
    private CoroutineHandle spawnHandle;
    private CoroutineHandle disableHandle;


    private void Awake()
    {

        vehicleCloneList = new List<GameObject>();

        foreach(GameObject vehicle in vehicleList)
        {
            GameObject car = Instantiate(vehicle, transform.position, vehicle.transform.rotation, transform);
            vehicleCloneList.Add(car);           
            car.SetActive(false);
        }
    }

    private void Update()
    {
        if(!spawnHandle.IsRunning)
        spawnHandle = Timing.RunCoroutine(enableVehicle());       
    }

    IEnumerator<float> enableVehicle() 
    {
        yield return Timing.WaitForSeconds(spawnInterval);
        
        var disabledVehicles = new List<GameObject>();
        foreach(var car in vehicleCloneList)
        {
            if(!car.activeSelf)
            {
                disabledVehicles.Add(car);
            }
        }

        var rndVehicle = Random.Range(0, disabledVehicles.Count - 1);
        disabledVehicles[rndVehicle].SetActive(true);
        if(!disabledVehicles[rndVehicle].GetComponent<BridgeVehicleScript>().EyeDestruction)
        disableHandle = Timing.RunCoroutine(disableVehicle(disabledVehicles[rndVehicle]).CancelWith(disabledVehicles[rndVehicle]));
    }

    // Move vehicle back to origin and disable it unless destroyed
    IEnumerator<float> disableVehicle(GameObject car)
    {
        yield return Timing.WaitForSeconds(disableTimer);
        car.transform.position = transform.position;
        var rotation = vehicleList[0].transform.rotation;
        car.transform.rotation = rotation;
        car.SetActive(false);
    }

    public void disableVehicleOnDestruction(GameObject car)
    {       
        car.transform.position = transform.position;
        var rotation = vehicleList[0].transform.rotation;
        car.transform.rotation = rotation;
        car.SetActive(false);     
    }

}
