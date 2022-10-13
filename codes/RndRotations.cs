using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RndRotations : MonoBehaviour
{

    // Inspector-fields
    [SerializeField]
    private float rotationSpeed;
    [SerializeField, Tooltip("How often do we change the rotation direction")]
    private float rotationChange;
    [SerializeField]
    private bool changeRotation;


    // Non-inspector fields
    private int rotationDirection;
    private Quaternion rotationToChange;
    private CoroutineHandle changeHandle;
    


    private void Awake()
    {
        RandomRotation();       
    }

    private void Update()
    {

        if(!changeHandle.IsRunning && changeRotation)
        {
            changeHandle = Timing.RunCoroutine(ChangeRotation());
        }

        transform.rotation *= rotationToChange;
    }


    // IENumerators
    IEnumerator<float> ChangeRotation()
    {
        yield return Timing.WaitForSeconds(rotationChange);
        rotationDirection = Random.Range(0, 3);
        if (rotationDirection == 0) rotationToChange = Quaternion.Euler(rotationSpeed, 0, 0);
        else if (rotationDirection == 1) rotationToChange = Quaternion.Euler(0, rotationSpeed, 0);
        else if (rotationDirection == 2) rotationToChange = Quaternion.Euler(0, 0, rotationSpeed);
    }



    private void RandomRotation()
    {       
        var rotX = Random.Range(0.1f, rotationSpeed);
        var rotY = Random.Range(0.1f, rotationSpeed);
        var rotZ = Random.Range(0.1f, rotationSpeed);
        rotationToChange = Quaternion.Euler(rotX, rotY, rotZ);
    }

}
