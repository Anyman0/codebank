using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    // Inspector fields
    [SerializeField] private GameObject cam;
    [SerializeField, Tooltip("Layer of the raycast. 'ie 9 for player.'")] private int raycastLayer;
    [SerializeField, Tooltip("This value is the distance between player and the object above when the camera starts to descend.")] private float distanceToLowerCamera;
    [SerializeField, Tooltip("How much do we move the camera down.")] private float cameraDescendValue;
    [SerializeField] private float cameraDescendSpeed;


    // Non-inspector fields
    private RaycastHit hit;
    private Vector3 defaultCameraPosition;
    private Vector3 newCameraPosition;
    private float dist;
    private bool lerpToNewPos = false;
    private bool startLerp = false;


    private void Awake()
    {                
        defaultCameraPosition = cam.transform.localPosition;
        newCameraPosition = new Vector3(cam.transform.localPosition.x, cameraDescendValue, cam.transform.localPosition.z);       
    }


    private void Update()
    {
        //Debug.DrawRay(transform.position, Vector3.up * 100, Color.red);

        if (Physics.Raycast(transform.position, Vector3.up, out hit, 100, raycastLayer))
        {
            dist = (transform.position - hit.transform.position).sqrMagnitude;
            if (dist < distanceToLowerCamera)
            {                
                lerpToNewPos = true;
            }
        }

        else lerpToNewPos = false;

        if ((cam.transform.localPosition - defaultCameraPosition).sqrMagnitude > 0.05f || lerpToNewPos)
            LerpPosition();
        else cam.transform.localPosition = defaultCameraPosition;
        
    }

    private void LerpPosition()
    {        
        if (lerpToNewPos)
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, newCameraPosition, Time.deltaTime * cameraDescendSpeed);
        else if (!lerpToNewPos)
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, defaultCameraPosition, Time.deltaTime * cameraDescendSpeed);
    }
}
