using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class MovementScript : MonoBehaviour
{
    
    Animator animator;
    Vector2 joyStickVector;
    bool Jump;
    private Camera mainCam;
    private float headCenterPosition;
    private bool isGrounded; 

    [SerializeField, Tooltip("Basic forward movement speed of the player. 0.05f as default.")]
    public float movementSpeed = 0.05f;
    [SerializeField, Tooltip("This value determines how high the player can jump. 500 as default.")]
    private float jumpForce = 500f;
    [SerializeField, Tooltip("Movementspeed multiplier. 2 as default.")]
    private float movementBoost = 2; 

    private Transform objectToMove;
    private Transform playerToMove;
    private bool isBoosted = false;


    private float vertical;
    private float horizontal;
        
    public enum MovementState
    {
        UseHeadToStrafe = 0, 
        MoveWithJoystick = 1,  
        MoveWithKeyboard = 2
    }

    [Tooltip("Headstrafing moves the character left and right. All other movement still comes from joystick.")]
    public MovementState moveStates;
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
                joyStickVector = SteamVR_Actions.default_JoystickMove.GetAxis(SteamVR_Input_Sources.LeftHand);
                Jump = SteamVR_Actions.default_JoystickClick.GetStateDown(SteamVR_Input_Sources.LeftHand);
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
    }

    // Update is called once per frame
    void Update()
    {
        
        if (animator == null) animator = GetComponent<Animator>();
        if (objectToMove == null) objectToMove = this.transform.parent;
        if (playerToMove == null) playerToMove = this.transform;
        Player player = Player.instance;
        if (!player)
        {
            return;
        }
            
        // HOX: KeyboardMovement and StrafeWithHead not set in 3rdPD animator.
        if(moveStates == MovementState.UseHeadToStrafe)
        {
            animator.SetBool("StrafeWithHead", true);
            animator.SetBool("JoystickMovement", false);
            animator.SetBool("KeyboardMovement", false);
            var headMovement = (float)Math.Round(Mathf.Abs(headCenterPosition- mainCam.transform.localPosition.x ), 2);           
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

        if(moveStates == MovementState.MoveWithKeyboard)
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
        
        if(moveStates == MovementState.MoveWithJoystick)
        {
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
            float rightStrafe = joyStickVector.x / 10f;
            if (joyStickVector.x > 0.1f && joyStickVector.x <= 1f)
            {
                playerToMove.transform.position += transform.right * rightStrafe;
            }

            // Moving left       
            float leftStrafe = joyStickVector.x / 10f;
            float leftStrafePositive = Mathf.Abs(leftStrafe);
            if (joyStickVector.x < -0.1f && joyStickVector.x >= -1f)
            {
                playerToMove.transform.position += -transform.right * leftStrafePositive;
            }

            // Moving back
            float backDash = joyStickVector.y / 10f;
            float backDashPositive = Mathf.Abs(backDash);
            if (joyStickVector.y < -0.1f && joyStickVector.y >= -1f)
            {
                playerToMove.transform.position += -transform.forward * backDashPositive;
            }

            // Moving forward
            float dash = joyStickVector.y / 10f;
            if (joyStickVector.y > 0.1f && joyStickVector.y <= 1f)
            {
                playerToMove.transform.position += transform.forward * dash;
            }
        }

        else if(moveStates == MovementState.MoveWithKeyboard)
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
        
        
        //Debug.Log(joyStickVector + " And are we boosted?: " + isBoosted); // Track joystick position for debugging 
               
        // TODO: Jump. Maybe left controller X button to toggle? 
        //       MovementBoost. (GameObject with isTrigger-collider to toggle). 
        //       Dodging. Dodge based on head movements? Or toggled by some input action? 
        //       Slides. Slide left, Slide right. Slide also crouches. Do we even have an animation for slides? 

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
            MovementBoost();
            Invoke("MovementBoost", 5f); // TODO: How long does the boost last? Get the value from an object instead of hardcoding it here.
            Destroy(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.transform.tag == "Ground")
        {
            isGrounded = false;
        }
    }


    private void MovementBoost()
    {
        if (!isBoosted)
            isBoosted = true;
        else if (isBoosted)
            isBoosted = false; 
    }
   
}
