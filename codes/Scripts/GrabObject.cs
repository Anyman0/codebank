using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GrabObject : MonoBehaviour
{
    public int touchCount;
    void start()
    {
        if (gameObject.tag != "Grabbable")
        {
            Debug.LogError("Interactable's tag is not set to grabbable");
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        touchCount++;
    }
    private void OnCollisionExit(Collision collision)
    {
        touchCount--;
    }

}
