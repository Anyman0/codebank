using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

//These make sure that we have the components that we need
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(FixedJoint))]
public class GripController : MonoBehaviour
{
    public SteamVR_Input_Sources Hand;//these allow us to set our input source and action.
    public SteamVR_Action_Boolean ToggleGripButton;
    [SerializeField, Tooltip("Index finger is used for distance grabbing.")]
    private GameObject indexFinger;
    [SerializeField, Tooltip("Max distance of distance grab. Default at 50.")]
    private float grabDistance = 50f;
    [SerializeField, Tooltip("This is used to highlight chosen objects for distance grab.")]
    private GameObject highLighter;
    private Rigidbody rigidbody;
    private bool throwing;
    public bool distanceGrab;
    private GameObject dGrabObject;
    private GameObject previousHover;

    private void Awake()
    {
        previousHover = null;
    }
    //DEBUGGING
    public float offSet = -0.5f;

    private GameObject ConnectedObject;//our current connected object
    public List<GameObject> NearObjects = new List<GameObject>();//all objects that we could pick up
    private void Update()
    {
        if (ConnectedObject != null)//if we are holding somthing
        {
            if (ConnectedObject.GetComponent<GrabObject>().touchCount == 0)//if our object isn't touching anything
            {
                //first, we disconnect our object
                GetComponent<ConfigurableJoint>().connectedBody = null;
                GetComponent<FixedJoint>().connectedBody = null;

                //now we step our object slightly towards the position of our controller, this is because the fixed joint just freezes the object in whatever position it currently is in relation to the controller so we need to move it to the position we want it to be in. We could just set position to the position of the controller and be done with it but I like the look of it swinging into position.
                ConnectedObject.transform.position = Vector3.MoveTowards(ConnectedObject.transform.position, transform.position, .25f);
                ConnectedObject.transform.rotation = Quaternion.RotateTowards(ConnectedObject.transform.rotation, transform.rotation, 10);
                
                //reconnect the body to the fixed joint
                GetComponent<FixedJoint>().connectedBody = ConnectedObject.GetComponent<Rigidbody>();
            }
            else if (ConnectedObject.GetComponent<GrabObject>().touchCount > 0)//if it is touching something 
            {
                //switch from fixed joint to configurable joint
                GetComponent<FixedJoint>().connectedBody = null;
                GetComponent<ConfigurableJoint>().connectedBody = ConnectedObject.GetComponent<Rigidbody>();
            }
            if (ToggleGripButton.GetStateDown(Hand) && ToggleGripButton.GetStateDown(SteamVR_Input_Sources.LeftHand)) // Release object by pressing gribs simultaneously
            {
                Release();
            }
            /*if(ToggleGripButton.GetStateDown(Hand))
            {
                ThrowObject();
            }*/
            
        }
        else//if we aren't holding something
        {
            if (ToggleGripButton.GetStateDown(Hand) && !distanceGrab)//cheack if we want to pick somthing up
            {
                Grip();
            }
            else if(ToggleGripButton.GetState(Hand) && distanceGrab)
            {
                DistanceGrab();
            }
            if(ToggleGripButton.GetStateUp(Hand) && dGrabObject != null)
            {       
                foreach(Transform child in dGrabObject.transform)
                {
                    if (child.name == "HighLighter") child.GetComponent<MeshRenderer>().enabled = false;
                } 
                ConnectedObject = dGrabObject;               
                ConnectedObject.GetComponentInChildren<SphereCollider>().enabled = false;
                ConnectedObject.transform.SetParent(this.transform);
            }
        }
    }

    private void FixedUpdate()
    {
        /*if(throwing)
        {
            Transform origin;
            origin = ConnectedObject.transform.parent;

            if(origin != null)
            {
                rigidbody.velocity = origin.TransformVector(this.transform.GetComponent<Rigidbody>().velocity);
                rigidbody.angularVelocity = origin.TransformVector(this.transform.GetComponent<Rigidbody>().angularVelocity);
            }
            else
            {
                rigidbody.velocity = this.transform.GetComponent<Rigidbody>().velocity;
                rigidbody.angularVelocity = this.transform.GetComponent<Rigidbody>().angularVelocity;
            }

            throwing = false;
        }*/

        

    }
    private void Grip()
    {
        throwing = false;
        rigidbody = null;
        foreach(Transform child in this.transform) // Checking if player already has a weapon in hand. If so, release.
        {
            if(child.transform.tag == "Grabbable")
            {                
                Release();               
            }
            Debug.Log(child.transform.name + ". And all tags in order: " + child.transform.tag);
        }

        GameObject NewObject = ClosestGrabbable();

        if (NewObject != null)
        {
            ConnectedObject = ClosestGrabbable();//find the Closest Grabbable and set it to the connected objectif it isn't null 
            ConnectedObject.transform.SetParent(this.transform); // Move the picked up object to a children of the chosen hand            
        }

        NearObjects.Remove(ConnectedObject);
    }
    private void Release()
    {
        //disconnect all joints and set the connected object to null   
        ConnectedObject.transform.SetParent(null);
        GetComponent<ConfigurableJoint>().connectedBody = null;
        GetComponent<FixedJoint>().connectedBody = null;
        ConnectedObject = null;
    }

    /*private void ThrowObject()
    {
        
        if(GetComponent<FixedJoint>().connectedBody != null)
        {
            rigidbody = GetComponent<FixedJoint>().connectedBody;
            ConnectedObject.transform.SetParent(null);
            GetComponent<ConfigurableJoint>().connectedBody = null;
            GetComponent<FixedJoint>().connectedBody = null;
            ConnectedObject.GetComponent<Rigidbody>().isKinematic = false;
            ConnectedObject.GetComponent<Rigidbody>().velocity = this.transform.GetComponent<Rigidbody>().velocity;
            ConnectedObject.GetComponent<Rigidbody>().angularVelocity = this.transform.GetComponent<Rigidbody>().angularVelocity;
            throwing = true;
        }
    }*/

    void OnTriggerEnter(Collider other)
    {
        //Add grabbable objects in range of our hand to a list
        if (other.CompareTag("Grabbable"))
        {
            NearObjects.Add(other.gameObject);
        }
        //Debug.Log(NearObjects);
    }
    void OnTriggerExit(Collider other)
    {
        //remove grabbable objects going out of range from our list
        if (other.CompareTag("Grabbable"))
        {
            NearObjects.Remove(other.gameObject);
        }
    }
    GameObject ClosestGrabbable()
    {
        //find the object in our list of grabbable that is closest and return it.
        GameObject ClosestGameObj = null;
        float Distance = float.MaxValue;
        if (NearObjects != null)
        {
            foreach (GameObject GameObj in NearObjects)
            {
                if ((GameObj.transform.position - transform.position).sqrMagnitude < Distance)
                {
                    ClosestGameObj = GameObj;
                    Distance = (GameObj.transform.position - transform.position).sqrMagnitude;
                }
            }
        }
        return ClosestGameObj;
    }

    void DistanceGrab() // Grab a gun from distance with rayfire
    {
        RaycastHit hit;        
        Vector3 rayfireOffset = new Vector3(0, offSet, 0);
        Debug.DrawRay(indexFinger.transform.position, (indexFinger.transform.forward + rayfireOffset) * grabDistance, Color.green);
        
        if (Physics.Raycast(indexFinger.transform.position, indexFinger.transform.forward + rayfireOffset, out hit, grabDistance)) 
        {           
            Debug.Log(hit.transform.name + " And the hitpoint: " + hit.point);
            try
            {
                if (previousHover != hit.transform.parent.gameObject && previousHover != null)
                {
                    Debug.Log("Disabling Meshrenderer ");
                    previousHover.GetComponent<MeshRenderer>().enabled = false;
                }
            }
            catch
            {
                
            }

            if (hit.transform.name == "HighLighter")
            {
                dGrabObject = hit.transform.parent.gameObject;
                hit.transform.GetComponent<MeshRenderer>().enabled = true;
                previousHover = hit.transform.gameObject;
            }
            
                       
        }
             
    }
}
