using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMovement : MonoBehaviour
{
    Quaternion newRotation;
    float orientTransform;
    float orientTarget;

    [SerializeField, Tooltip("Determines the drones movement speed from origin to DroneMoveTargets. Default at 0.3f.")]
    private float movementSpeed = 0.5f;
    // Non-editor fields
    private float hoverHeight = 2.6f;   
    private bool canDestroy = false;
    [HideInInspector]
    public bool moveToTarget = true;
    private bool waitRandomMovement;
    private int moveTargetInt = -1;
    private List<Transform> droneMoveTargets;
    private GameObject droneTargetHolder;
    private Vector3 randomPosition;
    private float setHorizontalSpeed;
    // Hover attributes  
    [SerializeField, Tooltip("Horizontal hovering speed. Default se at 0.01f.")]
    private float horizontalSpeed = 0.01f;
    [SerializeField, Tooltip("Vertical hovering speed. Default set to 1.")]
    private float verticalSpeed = 1f;
    [SerializeField, Tooltip("Determines how much the drone ascends and descends. Default at 0.1f")]
    private float amplitude = 0.1f;
    [SerializeField] private bool isSaucer;
    [HideInInspector]
    public Vector3 tempPos;
    private float yHoverOffset = 0f;

    private bool isCoRoutineRunning;
    private Vector3 spawnPoint;
    private Quaternion spawnRotation;


    private void Awake()
    {
        spawnPoint = transform.position;
        spawnRotation = transform.rotation;
        droneMoveTargets = new List<Transform>();        
        droneTargetHolder = GameObject.Find("DroneMoveTargets");       
        setHorizontalSpeed = horizontalSpeed;        
    }

    private void OnEnable()
    {
        if (isSaucer) moveToTarget = false;
    }

    private void DroneMoveTargetsPopulate()
    {
        // Populating the list of droneMoveTargets. These are the positions where drones move from the "Deathstar"
        if (droneMoveTargets.Count < 1)
        {
            foreach (Transform child in droneTargetHolder.transform)
            {
                droneMoveTargets.Add(child);
            }
            Debug.Log(droneMoveTargets.Count);
        }
    }


    private void Update()
    {
        if (droneMoveTargets.Count < 1)
            DroneMoveTargetsPopulate();

        if (moveTargetInt == -1)
        {
            moveTargetInt = Random.Range(0, droneMoveTargets.Count - 1);
        }

        if (moveToTarget)
        {            
            transform.position = Vector3.Lerp(transform.position, droneMoveTargets[moveTargetInt].transform.position, Time.deltaTime * movementSpeed);
        }

        if ((transform.position - droneMoveTargets[moveTargetInt].transform.position).sqrMagnitude < 1.5f)
        {
            moveToTarget = false;
        }       
    }

    private void FixedUpdate()
    {
        if (!moveToTarget)
        {

            if (tempPos == Vector3.zero)
            {
                tempPos = transform.position;
                yHoverOffset = tempPos.y;
            }
            if (!isCoRoutineRunning) StartCoroutine(getRandomHoverX());
            
            tempPos.x += horizontalSpeed;
            tempPos.y = yHoverOffset + Mathf.Sin(Time.realtimeSinceStartup * verticalSpeed) * amplitude;
            transform.position = tempPos;
        }
    }

    IEnumerator getRandomHoverX()
    {
        isCoRoutineRunning = true;
        yield return new WaitForSeconds(5);
        horizontalSpeed = setHorizontalSpeed;
        var hz = Random.Range(-horizontalSpeed, horizontalSpeed);
        horizontalSpeed = hz;
        isCoRoutineRunning = false;
    }
    private void OnDisable()
    {        
        moveToTarget = true;
        tempPos = Vector3.zero;
        transform.position = spawnPoint;
        transform.rotation = spawnRotation;
    }
}
