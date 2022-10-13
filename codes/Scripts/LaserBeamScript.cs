using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeamScript : MonoBehaviour
{
    [SerializeField, Tooltip("This is the laser beam fired by this gameobject.")]
    private GameObject LaserBeam;
    [SerializeField, Tooltip("This is the origin where the laserbeam starts.")]
    private GameObject muzzle;
    [SerializeField, Tooltip("This determines how often does this object fire the beam.")]
    private float shootingInterval = 20f;
    [SerializeField, Tooltip("The target of the laserbeam (player)")]
    private GameObject target; 
    [SerializeField]
    private LineRenderer visibleBeam;
    [SerializeField, Tooltip("Determines how long will the beam last.")]
    private float beamDuration = 10f;

    public float enemyAimSpeed = 1.0f;
    Quaternion newRotation;
    float orientTransform;
    float orientTarget;
    private bool isBeaming = false;
    float rotationX;

    private void Awake()
    {
        rotationX = transform.rotation.x;
    }

    private void Update()
    {
        Debug.Log(LaserBeam.activeSelf);
        if(!IsInvoking() && !LaserBeam.activeSelf && !isBeaming)
        {
            Invoke("FireBeam", shootingInterval);          
        }

        if(LaserBeam.activeSelf && isBeaming) 
        {           
            Debug.Log("Controlling the beam?");
            /*visibleBeam.SetPosition(0, this.transform.position);
            visibleBeam.SetPosition(1, target.transform.position);*/

            orientTransform = transform.position.x;
            orientTarget = target.transform.position.x;
            // To Check on which side is the target , i.e. Right or Left of this Object
            if (orientTransform > orientTarget)
            {                
                newRotation = Quaternion.LookRotation(transform.position - target.transform.position, -Vector3.up);
            }
            else
            {
                newRotation = Quaternion.LookRotation(transform.position - target.transform.position, Vector3.up);
            }
            
            var rotation = Quaternion.FromToRotation(transform.position, target.transform.position);
            // Finally rotate and aim towards the target direction using Code below            
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * enemyAimSpeed);
            //transform.rotation = Quaternion.FromToRotation(transform.position, target.transform.position);         
            //transform.LookAt(target.transform);
                       
            
        }

    }

    private void FireBeam()
    {
        LaserBeam.SetActive(true);
        StartCoroutine("beaming");
    }

    IEnumerator beaming()
    {
        isBeaming = true;
        yield return new WaitForSeconds(beamDuration);
        float rot = transform.rotation.x;
        rot = rotationX;
        isBeaming = false;
        LaserBeam.SetActive(false);
    }
   

}
