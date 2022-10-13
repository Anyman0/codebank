using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VD;
using MEC;

public enum TypeOfObject
{
    Default,
    Vehicle, 
    Missile, 
    Collectable
}

public enum vehicleDriveDirections
{
    Left,
    Right,
    Back, 
    Forward
}

public class ObjectSpawner : MonoBehaviour
{
    // Inspector fields
    [SerializeField]
    private List<string> objectsToSpawn;
    [SerializeField]
    private List<int> numberOfObjects;
    [SerializeField]
    private List<Vector3> preDefinedSpawnPositions;  
    [SerializeField]
    private float spawnInterval;
    [SerializeField]
    private float objectLifeTime;
    [SerializeField]
    private TypeOfObject objectType;
    [SerializeField]
    private vehicleDriveDirections vehicleDriveDirection;
    [SerializeField]
    private bool usePredefinedPosition;
    [SerializeField]
    private bool spawnBunch;
    [SerializeField]
    private bool autoSpawn;
    [SerializeField, Tooltip("Tick this if you want to spawn all the items in list on awake.")]
    private bool spawnOnAwake;
    
    [Space(10)]
    [Header("Random spawn attributes!")]
    [Space(10)]
    [SerializeField, Tooltip("Spawns objects to random positions between min and max values.")]
    private bool rndSpawn;
    [SerializeField]
    private Vector3 rndSpawnMin;
    [SerializeField]    
    private Vector3 rndSpawnMax;
    [SerializeField]
    private float spawnDistance;
    
       

    // Non-inspector fields
    private int rndIndex;
    private int rndCount;
    private int integ;
    private int efficiencyCount; // Debug
    private Vector3 position;
    private List<Vector3> rndPositionList;
    private List<GameObject> spawnedObjects;
    private Quaternion rotation;
    // Handles
    private CoroutineHandle spawnHandle;
    private CoroutineHandle despawnHandle;
    private bool isVehicle;
    private bool isMissile;
    private bool isCollectable;
    private bool canSpawn;
    [HideInInspector]
    public bool destroyWithlaser;    
    private LaserBeamScript laser;
    [HideInInspector]
    public bool dropDebrisOnTrigger;
    private bool doNotRunOnDisable;
   
    private void Awake()
    {
        try
        {
            if (objectType == TypeOfObject.Vehicle)
            {
                isVehicle = true;
                rotation = VehicleRotation();
                laser = GameObject.FindGameObjectWithTag("EyeLaser").GetComponent<LaserBeamScript>();
            }
            else if (objectType == TypeOfObject.Missile)
            {
                isMissile = true;
                rotation = Quaternion.Euler(-90, 0, 0);
            }
            else if (objectType == TypeOfObject.Collectable)
            {
                isCollectable = true;
            }

            if (spawnOnAwake)
            {
                if (spawnInterval == 0) spawnInterval = 1; // This line prevents errors on spawning before ItemPool is set
                spawnHandle = Timing.RunCoroutine(SpawnBunch());
            }

            rndPositionList = new List<Vector3>();
        }
        catch
        {

        }
                    
    }

    private void Update()
    {
        if(autoSpawn)
        {
            if (!spawnHandle.IsRunning && !spawnBunch)
            {                
                spawnHandle = Timing.RunCoroutine(SpawnSingleObject(GetRandomIndex()));               
            }
            else if (!spawnHandle.IsRunning && spawnBunch)
            {
                spawnHandle = Timing.RunCoroutine(SpawnBunch());
            }
        }     
                     
    }

    
    private void SpawnOnTrigger(int index)
    {
        if (usePredefinedPosition) position = preDefinedSpawnPositions[index];
        else position = transform.position;
        var go = ItemManager.Instance.Spawn(objectsToSpawn[index], position, rotation);
        spawnedObjects.Add(go);
    }

    private int GetRandomIndex()
    {
        rndIndex = Random.Range(0, objectsToSpawn.Count);
        return rndIndex; 
    } 

    private Quaternion VehicleRotation() 
    {
        if (vehicleDriveDirection == vehicleDriveDirections.Left)
        {
            rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (vehicleDriveDirection == vehicleDriveDirections.Right)
        {
            rotation = Quaternion.Euler(0, -90, 0);
        }
        else if (vehicleDriveDirection == vehicleDriveDirections.Back)
        {
            rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (vehicleDriveDirection == vehicleDriveDirections.Forward)
        {
            rotation = Quaternion.Euler(0, 180, 0);
        }

        return rotation;
    }

    public Vector3 RandomVector(Vector3 min, Vector3 max)
    {
        position = new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
        return position;
    }


    // IEnumerators
    IEnumerator<float> DespawnObject(GameObject go)
    {
        yield return Timing.WaitForSeconds(objectLifeTime);
        ItemManager.Instance.Despawn(go);
    }
    IEnumerator<float> SpawnSingleObject(int index)
    {
        yield return Timing.WaitForSeconds(spawnInterval);

        if (usePredefinedPosition && !rndSpawn) position = preDefinedSpawnPositions[index];
        else if (!usePredefinedPosition && rndSpawn) position = RandomVector(rndSpawnMin, rndSpawnMax);
        else if (!usePredefinedPosition && !rndSpawn) position = transform.position;
        var singleObject = ItemManager.Instance.Spawn(objectsToSpawn[index], position, rotation, transform);
        spawnedObjects.Add(singleObject);

        // VEHICLE SPECIFIC
        if (isVehicle && !destroyWithlaser)
        {
            despawnHandle = Timing.RunCoroutine(DespawnObject(singleObject));
        }
        else if (isVehicle && destroyWithlaser)
        {           
            Timing.RunCoroutine(laser.destroyVehicle(singleObject));
            singleObject.GetComponent<HealthController>().Health = 10000;
            destroyWithlaser = false;
            dropDebrisOnTrigger = true;
        }
    }
    IEnumerator<float> SpawnBunch()
    {
        yield return Timing.WaitForSeconds(spawnInterval);
        
        if(!rndSpawn)
        {
            for (int i = 0; i < objectsToSpawn.Count; i++)
            {
                while (integ < numberOfObjects[i])
                {
                    if (usePredefinedPosition && !rndSpawn) position = preDefinedSpawnPositions[i];
                    else if (!usePredefinedPosition && !rndSpawn) position = transform.position;
                    var go = ItemManager.Instance.Spawn(objectsToSpawn[i], position, rotation);
                    spawnedObjects.Add(go);
                    integ++;
                    if(integ == numberOfObjects[i])
                    {
                        integ = 0;
                        break;
                    }
                    else if(integ < numberOfObjects[i])
                    {
                        i--;
                        break;
                    }
                }
                
            }
        }       

        if (rndSpawn)
        {
            
            for (int i = 0; i < objectsToSpawn.Count; i++)
            {                                               
                               
                position = RandomVector(rndSpawnMin, rndSpawnMax);                
                rndCount = 0;
                if (rndPositionList.Count < 1) rndPositionList.Add(position);
                
                while ((position - rndPositionList[rndCount]).sqrMagnitude > spawnDistance && integ < numberOfObjects[i])
                {
                    if (rndCount + 1 >= rndPositionList.Count)
                    {
                        rndPositionList.Add(position);
                        var go = ItemManager.Instance.Spawn(objectsToSpawn[i], position, rotation, transform);
                        spawnedObjects.Add(go);
                        integ++;
                        Debug.Log(integ + " And i here: " + i); 
                        if(integ == numberOfObjects[i])
                        {
                            integ = 0;
                            Debug.Log("Spawned!");
                            break;
                        }
                        else if(integ < numberOfObjects[i])
                        {
                            i--;
                            Debug.Log("Theres more than one of these!");
                            break; 
                        }
                        
                    }
                    else
                        rndCount++;
                }

                if ((position - rndPositionList[rndCount]).sqrMagnitude < spawnDistance)
                {
                    efficiencyCount++;
                    i--;                   
                }
                
            }

        }

    }


    private void OnEnable()
    {
        if (objectType == TypeOfObject.Vehicle)
        {
            isVehicle = true;
            rotation = VehicleRotation();
            laser = GameObject.FindGameObjectWithTag("EyeLaser").GetComponent<LaserBeamScript>();
        }
        else if (objectType == TypeOfObject.Missile)
        {
            isMissile = true;
            rotation = Quaternion.Euler(-90, 0, 0);
        }
        else if (objectType == TypeOfObject.Collectable)
        {
            isCollectable = true;
        }

        if (spawnOnAwake)
        {
            if (spawnInterval == 0) spawnInterval = 1; // This line prevents errors on spawning before ItemPool is set
            spawnHandle = Timing.RunCoroutine(SpawnBunch());
        }

        rndPositionList = new List<Vector3>();
        spawnedObjects = new List<GameObject>();       
        
        Debug.Log("Enabled!");
    }

    private void OnDisable()
    {        
        Debug.Log("Disabled!");
        if(!doNotRunOnDisable)
        {
            foreach (GameObject go in spawnedObjects)
            {
                Timing.RunCoroutine(DespawnObject(go));
            }
        }
        
    }
    private void OnApplicationQuit()
    {
        doNotRunOnDisable = true;
    }

}
