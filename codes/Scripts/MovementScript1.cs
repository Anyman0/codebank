using MEC;
using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

// Should not use GetComponents or findGameobject in OnGUI()

// Setting animator attributes (movement type, jump) every update. Better to do this only when changing states, should try to avoid unnecessary calls
// Getting rigidbody every jump, dodge. Just get it once and save. Same for IK. 
// If only jumping infrequently, might use Get, but better assume player will be jumping all the time.

// SteamVR_Actions does not contain definitions for default_JoystickMove... and others used? Where does this solution come from?
// thresholds for translating inputs to movement could be set as attributes, instead of hardcoding => if (joyStickVector.x > 0.1f && joyStickVector.x <= 1f)
// unnecessary to save leftStrafe, rightStrafe.. as these are only used locally
// Why absolute left and back input only to set negative next line?

// To use SteamVR Actions, set steamVR actions in SteamVR Menu and allocate them in inspector.
// In code, set listeners for events fired by these actions, according to needs.
// For Axis, onChange used, to fire event when action axis changes. New axis is then set in joyStickVectors.
// For Jump, onStateDown is used, to fire event when button state changes to down. Then Jump is set to true.
// For Menu, onStateDown is used to cycle menu on/off when button state changes to down.

// About Jump:
            // Set jump to false, regardless of whether it was used. Only allow one jump per click and only if grounded when clicked
            //Jump = false;
            // If holding should result in consecutive jumps, set listener to onChange instead, and change it there.
// About Menu: 
    // Listener set for both hands.

// Listeners are now set always, regardless of movement type. But since they are event-based, they will not interfere unless used.
// In ideal world, listeners would be set when used type changes to/away from joystick.


// Made some changes, before thinking of only commenting on non-added. Maybe saving and absoluting on waypoint values on strafing makes for easier to read code (surely it does)
// But, for some reason this irritates me... 

public class MovementScript1 : MonoBehaviour
{    
    Animator animator;
    Vector2 joyStickVector;
    Vector2 joystickSpecialMoves;
    bool Jump;
    private Camera mainCam;
    private float headCenterPosition;
    private bool isGrounded;
    private bool isDodging;
    private bool isSliding;
    private GameObject playerIK;

    [SerializeField, Tooltip("Basic forward movement speed of the player.")]
    public float movementSpeed = 0f;
    [SerializeField, Tooltip("This value determines how high the player can jump.")]
    private float jumpForce;
    [SerializeField, Tooltip("Movementspeed multiplier. 2 as default.")]
    private float movementBoost = 2;
    [SerializeField, Tooltip("How much force do we add in dodge-motion.")]
    private float dodgeForce;
    [SerializeField, Tooltip("Determines how much force we add for motion on ice.")]
    private float onIceMotionForce = 20f;
    [SerializeField, Tooltip("Value to smooth the transition from ground to ice.")]
    private float enterIceForce = 2000f;

    private Transform objectToMove;
    private Transform playerToMove;
    private Rigidbody playerRB;
    private bool isBoosted = false;
    [HideInInspector]
    public bool paused;
    private static bool trigger = false;
    private bool onIce;

    private float vertical;
    private float horizontal;
    private GameObject playerUI;
    private List<GameObject> UIobjectList;
    private GameObject startMenu;
    private GameObject vehicleSpawner;   
        
    public enum MovementState
    {
        UseHeadToStrafe = 0, 
        MoveWithJoystick = 1,  
        MoveWithKeyboard = 2
    }

    [Tooltip("Headstrafing moves the character left and right. All other movement still comes from joystick.")]
    public MovementState moveStates;

    // --- //

    [Header("Steam VR actions")]
    [SerializeField]
    private SteamVR_Action_Vector2 onJoystickLeft_Move;
    [SerializeField]
    private SteamVR_Action_Vector2 onJoystickRight_SpecialMove;
    [SerializeField]
    private SteamVR_Action_Boolean onClick_Jump;
    [SerializeField]
    private SteamVR_Action_Boolean onClick_Menu;
    [SerializeField]
    private SteamVR_Action_Boolean onClick_StartMenu;

    private void OnEnable()
    {
        SubscribeListeners(true);
    }

    private void OnDisable()
    {
        SubscribeListeners(false);
    }

    // Set event listeneres for SteamVR actions
    private void SubscribeListeners(bool sub)
    {
        // Set starting values, incase used before events
        joyStickVector = Vector2.zero;
        Jump = false;
        joystickSpecialMoves = Vector2.zero;

        if (sub)
        {
            onJoystickLeft_Move.AddOnChangeListener(OnJoystickLeft_Move_onChange, SteamVR_Input_Sources.LeftHand);
            onJoystickRight_SpecialMove.AddOnChangeListener(OnJoystickRight_SpecialMove_onChange, SteamVR_Input_Sources.RightHand);
            onClick_Jump.AddOnStateDownListener(OnClick_Jump_onStateDown, SteamVR_Input_Sources.LeftHand);
            onClick_Menu.AddOnStateDownListener(OnClick_Menu_onStateDown, SteamVR_Input_Sources.LeftHand);
            onClick_Menu.AddOnStateDownListener(OnClick_Menu_onStateDown, SteamVR_Input_Sources.RightHand);
            onClick_StartMenu.AddOnStateDownListener(OnClick_StartMenu_onStateDown, SteamVR_Input_Sources.LeftHand);
        }
        else // unsub
        {
            onJoystickLeft_Move.RemoveOnChangeListener(OnJoystickLeft_Move_onChange, SteamVR_Input_Sources.LeftHand);
            onJoystickRight_SpecialMove.RemoveOnChangeListener(OnJoystickRight_SpecialMove_onChange, SteamVR_Input_Sources.RightHand);
            onClick_Jump.RemoveOnStateDownListener(OnClick_Jump_onStateDown, SteamVR_Input_Sources.LeftHand);
            onClick_Menu.RemoveOnStateDownListener(OnClick_Menu_onStateDown, SteamVR_Input_Sources.LeftHand);
            onClick_Menu.RemoveOnStateDownListener(OnClick_Menu_onStateDown, SteamVR_Input_Sources.RightHand);
            onClick_StartMenu.RemoveOnStateDownListener(OnClick_StartMenu_onStateDown, SteamVR_Input_Sources.LeftHand);
        }
    }

    private void OnClick_StartMenu_onStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if(startMenu.activeSelf)
        {
            startMenu.SetActive(false);
            paused = false;
            Time.timeScale = 1f;
        }
        else if(!startMenu.activeSelf)
        {
            startMenu.SetActive(true);
            paused = true;
            Time.timeScale = 0f;
        }
    }

    private void OnClick_Menu_onStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        foreach (GameObject UIObject in UIobjectList)
        {
            if (UIObject.gameObject.activeSelf) UIObject.gameObject.SetActive(false);
            else UIObject.gameObject.SetActive(true);
        }
    }

    private void OnClick_Jump_onStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Jump = true;
    }

    private void OnJoystickRight_SpecialMove_onChange(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        joystickSpecialMoves = axis;
    }

    private void OnJoystickLeft_Move_onChange(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        joyStickVector = axis;
    }

    // --- //

    private void OnGUI()
    {        
        Player player = Player.instance;
        if (!player)
        {
            return;
        }

        animator = GetComponent<Animator>();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        // Setting the center position of the VR headset
        if (headCenterPosition == 0) headCenterPosition = mainCam.transform.localPosition.x;
        

        if (player.leftHand != null) 
        {
            if(moveStates == MovementState.MoveWithJoystick )
            {
                //joyStickVector = SteamVR_Actions.default_JoystickMove.GetAxis(SteamVR_Input_Sources.LeftHand);
                //Jump = SteamVR_Actions.default_JoystickClick.GetStateDown(SteamVR_Input_Sources.LeftHand);
                //joystickSpecialMoves = SteamVR_Actions.default_JoystickAction.GetAxis(SteamVR_Input_Sources.RightHand);
            }
            else if(moveStates == MovementState.MoveWithKeyboard)
            {
                Jump = Input.GetKeyDown(KeyCode.Space);
            }
            // Below for debugging
            if(moveStates == MovementState.MoveWithJoystick || moveStates == MovementState.UseHeadToStrafe)
            {
                GUILayout.Label(string.Format("Left X: {0:F2}, {1:F2}", joyStickVector.x, joyStickVector.y));
            }                   
        }        
    }

    private void Awake()
    {
        if (objectToMove == null) objectToMove = this.transform.parent;  
        // Get playerUI and populate UIobjectList with UI-objects
        playerUI = GameObject.FindGameObjectWithTag("UI");
        startMenu = GameObject.FindGameObjectWithTag("Menu");        
        startMenu.SetActive(false);
        UIobjectList = new List<GameObject>();
        foreach(Transform uielement in playerUI.transform)
        {
            UIobjectList.Add(uielement.gameObject);
        }
        playerIK = GameObject.FindGameObjectWithTag("playerIK");
    }

    // Update is called once per frame
    void Update() 
    {

        if (!paused)
        {


            if (animator == null) animator = GetComponent<Animator>();
            if (objectToMove == null) objectToMove = transform.parent;
            if (playerToMove == null)
            {
                playerToMove = transform;
                playerRB = playerToMove.GetComponent<Rigidbody>();
            }
                

            /*Player player = Player.instance;
            if (!player)
            {
                return;
            }*/

            // HOX: KeyboardMovement and StrafeWithHead not set in 3rdPD animator.
            if (moveStates == MovementState.UseHeadToStrafe)
            {
                animator.SetBool("StrafeWithHead", true);
                animator.SetBool("JoystickMovement", false);
                animator.SetBool("KeyboardMovement", false);
                var headMovement = (float)Math.Round(Mathf.Abs(headCenterPosition - mainCam.transform.localPosition.x), 2);
                if (headCenterPosition > mainCam.transform.localPosition.x) headMovement = headMovement * -1;
                Debug.Log(headMovement);
                animator.SetFloat("JoystickY", joyStickVector.y, 1f, Time.deltaTime * 10f);
                animator.SetFloat("JoystickX", headMovement, 1f, Time.deltaTime * 10f);
            }

            if (moveStates == MovementState.MoveWithJoystick)
            {
                //Debug.Log(animator + " " + objectToMove);
                animator.SetBool("JoystickMovement", true);
                //animator.SetBool("StrafeWithHead", false);
                animator.SetBool("KeyboardMovement", false);
                animator.SetFloat("JoystickY", joyStickVector.y, 1f, Time.deltaTime * 10f);
                animator.SetFloat("JoystickX", joyStickVector.x, 1f, Time.deltaTime * 10f);

            }

            if (moveStates == MovementState.MoveWithKeyboard)
            {
                animator.SetBool("KeyboardMovement", true);
                //animator.SetBool("StrafeWithHead", false);
                animator.SetBool("JoystickMovement", false);
                horizontal = Input.GetAxis("Horizontal");
                vertical = Input.GetAxis("Vertical");
                animator.SetFloat("JoystickY", vertical, 1f, Time.deltaTime * 10f);
                animator.SetFloat("JoystickX", horizontal, 3f, Time.deltaTime * 10f);
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    animator.SetFloat("JoystickY", vertical + 0.5f, 1f, Time.deltaTime * 10f);
                }
            }

            if (moveStates == MovementState.MoveWithJoystick)
            {
                // Jump!
                if (Jump && isGrounded)
                {
                    playerToMove.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce);
                    animator.SetBool("Jump", true);
                }
                else if (Jump != true)
                {
                    animator.SetBool("Jump", false);
                }
                // Set jump to false, regardless of whether it was used. Only allow one jump per click and only if grounded when clicked
                Jump = false;
                // If holding should result in consecutive jumps, set listener to onChange instead, and change it there.


                // Basic forward moving speed
                if (!isBoosted)
                    objectToMove.transform.position += transform.forward * movementSpeed;
                // IF boosted
                else if (isBoosted)
                    objectToMove.transform.position += transform.forward * (movementSpeed * movementBoost);

                // Moving right 
                if (joyStickVector.x > 0.1f && joyStickVector.x <= 1f)
                {
                    if (!onIce)
                        playerToMove.transform.position += transform.right * joyStickVector.x / 10f;
                    else if (onIce)
                        playerRB.AddForce(transform.right * joyStickVector.x * onIceMotionForce);
                }                

                // Moving left
                if (joyStickVector.x < -0.1f && joyStickVector.x >= -1f)
                {
                    if (!onIce)
                        playerToMove.transform.position += transform.right * joyStickVector.x / 10f;
                    else if (onIce)
                        playerRB.AddForce(transform.right * joyStickVector.x * onIceMotionForce);
                }

                // Moving back
                if (joyStickVector.y < -0.1f && joyStickVector.y >= -1f)
                {
                    if (!onIce)
                        playerToMove.transform.position += transform.forward * joyStickVector.y / 10f;
                    else if (onIce)
                        playerRB.AddForce(transform.forward * joyStickVector.y * onIceMotionForce);
                }

                // Moving forward
                if (joyStickVector.y > 0.1f && joyStickVector.y <= 1f)
                {
                    if (!onIce)
                        playerToMove.transform.position += transform.forward * joyStickVector.y / 10f;
                    else if (onIce)
                        playerRB.AddForce(transform.forward * joyStickVector.y * onIceMotionForce);
                }
            }

            else if (moveStates == MovementState.MoveWithKeyboard)
            {
                Debug.Log(vertical + " And horizontal: " + horizontal);
                // Jump!
                if (Jump && isGrounded)
                {
                    playerToMove.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce);
                    animator.SetBool("Jump", true);
                }
                else if (Jump != true) animator.SetBool("Jump", false);


                // Basic forward moving speed
                if (!isBoosted)
                    objectToMove.transform.position += transform.forward * movementSpeed;
                // IF boosted
                else if (isBoosted)
                    objectToMove.transform.position += transform.forward * (movementSpeed * movementBoost);



                // Moving right 
                float rightStrafe = horizontal / 10f;
                if (horizontal > 0.1f && horizontal <= 1f)
                {
                    playerToMove.transform.position += transform.right * rightStrafe;
                }

                // Moving left       
                float leftStrafe = horizontal / 10f;
                float leftStrafePositive = Mathf.Abs(leftStrafe);
                if (horizontal < -0.1f && horizontal >= -1f)
                {
                    playerToMove.transform.position += -transform.right * leftStrafePositive;
                }

                // Moving back
                float backDash = vertical / 10f;
                float backDashPositive = Mathf.Abs(backDash);
                if (vertical < -0.1f && vertical >= -1f)
                {
                    playerToMove.transform.position += -transform.forward * backDashPositive;
                }

                // Moving forward
                float dash = vertical / 10f;
                if (vertical > 0.1f && vertical <= 1f)
                {
                    playerToMove.transform.position += transform.forward * dash;
                }
            }


            // Show / Hide playerUI
            //if(SteamVR_Actions.default_MenuClick.GetStateDown(SteamVR_Input_Sources.LeftHand))
            //{            
            //    foreach(GameObject UIObject in UIobjectList)
            //    {
            //        if (UIObject.gameObject.activeSelf) UIObject.gameObject.SetActive(false);
            //        else UIObject.gameObject.SetActive(true);
            //    }
            //}


            // Dodge motions. Coroutine added so it cant be spammed. Blocked if player is on ice.
            // Dodge right
            if(!onIce)
            {
                if (joystickSpecialMoves.x > 0.1f && joystickSpecialMoves.x <= 1f && !isDodging && !Jump)
                {
                    animator.SetTrigger("DodgeRight");
                    var rb = playerToMove.GetComponent<Rigidbody>();
                    rb.AddForce(transform.right * dodgeForce);
                    isDodging = true;
                    playerIK.GetComponent<VRIK>().enabled = false;
                    Timing.RunCoroutine(dodgeToFalse());
                }

                // Dodge left
                if (joystickSpecialMoves.x < -0.1f && joystickSpecialMoves.x >= -1f && !isDodging && !isSliding && !Jump)
                {
                    animator.SetTrigger("DodgeLeft");
                    var rb = playerToMove.GetComponent<Rigidbody>();
                    rb.AddForce(-transform.right * dodgeForce);
                    isDodging = true;
                    playerIK.GetComponent<VRIK>().enabled = false;
                    Timing.RunCoroutine(dodgeToFalse());
                }

                // Slide
                if (joystickSpecialMoves.y > 0.1f && joystickSpecialMoves.y <= 1f && !isSliding && !Jump && !isDodging)
                {
                    animator.SetTrigger("Slide");
                    isSliding = true;
                    playerIK.GetComponent<VRIK>().enabled = false;
                    GetComponent<CapsuleCollider>().direction = 2;
                    GetComponent<CapsuleCollider>().center += new Vector3(0, -0.4f, 0);
                    Timing.RunCoroutine(slideToFalse());
                }
            }
            


            //if (joystickSpecialMoves == Vector2.zero) isDodging = false;

            //Debug.Log(joyStickVector + " And are we boosted?: " + isBoosted); // Track joystick position for debugging 

            // TODO: Jump. Maybe left controller X button to toggle? 
            //       MovementBoost. (GameObject with isTrigger-collider to toggle). 
            //       Dodging. Dodge based on head movements? Or toggled by some input action? 
            //       Slides. Slide left, Slide right. Slide also crouches. Do we even have an animation for slides? 

        }
    }

    // Collision checks to determine if we can jump
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Ground")
        {
            isGrounded = true;
        }
        else if(collision.transform.tag == "MovementBooster")
        {
            Timing.RunCoroutine(MovementBoost());            
            Destroy(collision.gameObject);
        }
        else if(collision.transform.tag == "Ice")
        {            
            onIce = true;
            isGrounded = false;
            playerRB.AddForce(transform.forward * enterIceForce);
            Debug.Log("ON ICE NOW! State is: " + onIce);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.transform.tag == "Ground")
        {
            isGrounded = false;
        }
        else if(collision.transform.tag == "Ice")
        {          
            onIce = false;
            isGrounded = true;
        }
    }

   

    IEnumerator<float> MovementBoost()
    {
        isBoosted = true;
        yield return Timing.WaitForSeconds(3f); // This value should come from the booster object.
        isBoosted = false;
    }

    IEnumerator<float> dodgeToFalse()
    {
        yield return Timing.WaitForSeconds(1.5f);
        isDodging = false;
        playerIK.GetComponent<VRIK>().enabled = true;
    }
    IEnumerator<float> slideToFalse()
    {
        yield return Timing.WaitForSeconds(2f);
        isSliding = false;
        playerIK.GetComponent<VRIK>().enabled = true;
        GetComponent<CapsuleCollider>().direction = 1;
        GetComponent<CapsuleCollider>().center += new Vector3(0, 0.4f, 0);       
    }

    private void OnTriggerEnter(Collider other)
    {
        
        // Checking which trigger is the player hitting
        if(other.transform.name == "TriggerPoint")
        {
            try
            {
                vehicleSpawner = GameObject.FindGameObjectWithTag("VehicleSpawner");
                var spawnScript = vehicleSpawner.GetComponent<BridgeVehicleSpawnerScript>();
            }
            catch
            {
                Debug.Log("Couldnt get vehiclespawner.");
            }

            if (vehicleSpawner.GetComponent<BridgeVehicleSpawnerScript>().vehicleTrigger && !trigger)
            {
                vehicleSpawner.GetComponent<BridgeVehicleSpawnerScript>().canDestroy = true;
                vehicleSpawner.GetComponent<BridgeVehicleSpawnerScript>().triggerSpawn();
            }
            trigger = true;
        }

        // CONTINUE HERE! MAY NOT WORK AS INTENDED? O.O
        else if(other.transform.name == "WindTrigger")
        {
            int windDir = 0;                     
            try
            {
                windDir = UnityEngine.Random.Range(0, 2);               
            }
            catch
            {
                Debug.Log("Couldnt randomize wind direction.");
            }
           
            Timing.RunCoroutine(other.transform.GetComponent<WindScript>().addWindForce(windDir));
        }
        
       
    }

    private void OnTriggerExit(Collider other)
    {                
        trigger = false;
    }
}
