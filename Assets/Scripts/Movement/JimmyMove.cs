using System.Collections;
using System;
using UnityEngine;
using TMPro;
using Photon.Pun;


/*
 * 
 * Turbo Tag Player Movement Script and Ability Handler
 * 
 * 
 * C:\Users\Justin\Turbo Tag\Assets\JimmyMove.cs
 * 
 * Thanks to: 
 * Leprawel
 * Dave / GameDevelopment
 * BlenderGuru
 * And a lot of other YouTube tutorials and explainations
 * 
 * 
 */
public class JimmyMove : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode swingKey = KeyCode.Mouse0;
    public KeyCode rockKey = KeyCode.R;
    public KeyCode proneKey = KeyCode.C;
    public KeyCode dashKey = KeyCode.E;
    public KeyCode boomKey = KeyCode.Q;
    public KeyCode discKey = KeyCode.F;

    [Header("Movement Speeds")]
    public float acceleration;
    public float walkSpeed;
    public float sprintSpeed;
    public float proneSpeed;
    public float swingSpeed;
    public float slideSpeed;
    public float climbSpeed;
    public float dashSpeed;
    public float wallRunSpeed;

    [Header("Basic Modifiers")]
    public float jumpStrength;
    public float crouchHeight;
    public float playerHeight;
    public float airControlMult;

    [Header("References")]
    public Rigidbody rb;
    public LayerMask structureLayer;
    public TextMeshProUGUI stats;


    PhotonView view;

    // Movement
    private float targetSpeed;

    // Wall Running
    private float timeOnWall = 0f;

    // Raycasts
    private bool onGround;
    private bool wallLeft;
    private bool wallRight;
    private bool wallForward;

    // State
    private bool isProne = false;
    private bool isClimbing = false;
    public bool isSwinging = false;
    private bool isWallRunning = false;

    public MovementState state;
    public enum MovementState
    {
        idle,
        walk,
        sprint,
        prone,
        climb,
        swing,
        wallRun,
        air
    }

    void Start()
    {
        view = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (view.IsMine)
        {
            SurfaceCheck();
            if (UIController.GameIsPaused) return;
            InputHandler();
            StateUpdate();
            MovePlayer();
            DisplayStats();
        }
    }

    void SurfaceCheck()
    {
        onGround = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.2f, ~0);
        Debug.DrawRay(transform.position, Vector3.down * (playerHeight * 0.2f), Color.yellow);

        wallLeft = Physics.Raycast(transform.position, -transform.right, 1, structureLayer);
        wallRight = Physics.Raycast(transform.position, transform.right, 1, structureLayer);
        //Debug.DrawRay(transform.position, -transform.right * 1f, Color.red);

        // Wall running
        if ((wallLeft && Input.GetKey(KeyCode.A)) || (wallRight && Input.GetKey(KeyCode.D)))
            WallRun();
        else
        {
            rb.useGravity = true;
            isWallRunning = false;
            timeOnWall = 0f;
        }

        // Check for wall in front
        RaycastHit wallHit;
        if (Physics.Raycast(transform.position, transform.forward, out wallHit, 1, structureLayer))
        {
            // Wall climbing
            if (Input.GetKey(KeyCode.W)) //Vector3.Angle(rb.velocity, wallHit.normal) < 30 && 
            {
                isClimbing = true;
                Climb();
            }
        }
        else
        {
            isClimbing = false;
        }

    }

    void InputHandler()
    {


        // Jump
        if (Input.GetKeyDown(jumpKey))
            Jump();

        // Prone
        if (Input.GetKeyDown(proneKey))
            ToggleProne();

        /*
         if (Input.GetKeyDown(Key))
            ();
        */

    }

    void StateUpdate()
    {
        if (isClimbing)                                 // Climb
        {
            state = MovementState.climb;
            targetSpeed = climbSpeed;
        }
        else if (isProne && onGround)                   // Prone
        {
            state = MovementState.prone;
            targetSpeed = proneSpeed;
        }
        else if (Input.GetKey(sprintKey) && onGround)   // Sprint
        {
            state = MovementState.sprint;
            targetSpeed = sprintSpeed;
        }
        else if (onGround && rb.velocity.magnitude > 2f)// Walk
        {
            state = MovementState.walk;
            targetSpeed = walkSpeed;
        }
        else if (onGround)                               // Idle
        {
            state = MovementState.idle;
            targetSpeed = walkSpeed;
        }
        else if (isSwinging)                            // Swing
        {
            state = MovementState.swing;
            targetSpeed = swingSpeed;
        }
        else if (isWallRunning)                         // Wall Run
        {
            state = MovementState.wallRun;
            targetSpeed = wallRunSpeed;
        }
        else                                            // Air
        {
            state = MovementState.air;
        }
    }

    void MovePlayer()
    {
        //if (isClimbing) return;

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        float acc = acceleration;

        // Direction of input, at the magnitude of targetSpeed
        Vector3 intendedDir = (transform.forward * moveVertical + transform.right * moveHorizontal) * (float)targetSpeed;
        //Debug.DrawRay(transform.position, intendedDir, Color.red);

        // The player's current velocity in the X and Z coordinates
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        //Debug.DrawRay(transform.position, flatVel, Color.blue);

        // Allows player to move freely through the air without being pulled to a stop
        if (intendedDir.magnitude == 0 && !onGround)
        {
            intendedDir = flatVel;
        }

        // The vector that will push the player to the intended direction
        Vector3 direction = intendedDir - flatVel;

        // If we are going faster than intended, increase acceleration to help slow us down
        if (flatVel.magnitude > targetSpeed)
        {
            acc *= flatVel.magnitude / targetSpeed;
        }

        // If change is supposed to be very small, then acceleration is very small
        if (direction.magnitude < 0.5f)
        {
            acc *= direction.magnitude * 2f;
        }

        direction = direction.normalized * acc;

        if (onGround)
        {
            // Raycast to find ground normal
            RaycastHit groundHit;
            Vector3 groundNormal = new Vector3(0, 0, 0);
            if (Physics.Raycast(transform.position, Vector3.down, out groundHit, playerHeight * 0.7f + 0.35f, ~0))
            {
                groundNormal = groundHit.normal;
            }

            // Account for slopes
            Vector3 slopeCorrection = groundNormal * Physics.gravity.y / groundNormal.y;
            slopeCorrection.y = 0f;
            direction += slopeCorrection;

            Debug.DrawRay(transform.position, direction, Color.green);
            rb.AddForce(direction, ForceMode.Acceleration);

        }
        else
        {
            Debug.DrawRay(transform.position, direction * airControlMult, Color.green);
            rb.AddForce(direction * airControlMult, ForceMode.Acceleration);
        }
            

    }

    void Jump()
    {
        Vector3 jumpVector = new Vector3();
        if (wallLeft && !onGround)
        {
            jumpVector = transform.up * jumpStrength + transform.right * jumpStrength;
        }
        else if (wallRight && !onGround)
        {
            jumpVector = transform.up * jumpStrength + -transform.right * jumpStrength;
        }
        else if (isClimbing && !onGround)
        {
            jumpVector = transform.up * jumpStrength + -transform.forward * jumpStrength;
        }
        else if(onGround)
        {
            jumpVector = new Vector3(0, jumpStrength, 0);
        }
        Debug.DrawRay(transform.position, jumpVector, Color.red);
        rb.AddForce(jumpVector, ForceMode.Impulse);
    }

    void ToggleProne()
    {
        print(isProne);
        if (!isProne)
        {
            isProne = true;
            transform.localScale = new Vector3(transform.localScale.x, 0.5f, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        else
        {
            isProne = false;
            transform.localScale = new Vector3(transform.localScale.x, .95f, transform.localScale.z);
        }
        // Transform into different mesh with different camera / gun / etc locations

    }

    void Climb()
    {
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
    }

    void WallRun()
    {
        isWallRunning = true;
        rb.useGravity = false;
        
        //Add inital force to negate any falling
        if (timeOnWall == 0)
            rb.AddForce(transform.up * 10);

        timeOnWall += 1 * Time.deltaTime;

        // Stick to wall
        if (wallLeft)
            rb.AddForce(-transform.right * 10); // * Time.deltaTime);
        else
            rb.AddForce(transform.right * 10); // * Time.deltaTime);

        // The force pulling up on the player. At first it's zero, then exponentially decreases
        float pullForce = -1 * Mathf.Pow(4f, timeOnWall - 1.5f);
        rb.AddForce(transform.up * pullForce); // * Time.deltaTime);

    }

    private void DisplayStats()
    {
        string statLine = "";

        // First line - Velocity 
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        //if (OnSlope())
            statLine += Math.Round(rb.velocity.magnitude, 1) + " / " + Math.Round(targetSpeed, 1);
        //else
        //    statLine += Math.Round(flatVel.magnitude, 1) + " / " + Math.Round(moveSpeed, 1) + " Y: " + Math.Round(rb.velocity.y, 1);

        statLine += '\n';
        if (state == MovementState.idle)
            statLine += "IDLE";
        else if (state == MovementState.walk)
            statLine += "WALK";
        else if (state == MovementState.sprint)
            statLine += "SPRINT";
        else if (state == MovementState.prone)
            statLine += "PRONE";
        else if (state == MovementState.climb)
            statLine += "CLIMB";
        else if (state == MovementState.swing)
            statLine += "SWING";
        else if (state == MovementState.wallRun)
            statLine += "WALLRUN";
        else if (state == MovementState.air)
            statLine += "AIR";

        // Third line - Grounded status
        statLine += '\n';
        if (onGround)
            statLine += "Grounded";
        else
            statLine += "Not Grounded";

        statLine += '\n';
        if (isClimbing)
            statLine += "Climbing";
        else
            statLine += "Not Climbing";

        // Fourth line - Gravity status
        statLine += '\n';
        if (rb.useGravity)
            statLine += "Gravity";
        else
            statLine += "Zero Grav";

        // Fourth line - Gravity status
        statLine += '\n';
        if (isWallRunning)
            statLine += "Wall Running";
        else
            statLine += "Not Wall Running";

        stats.SetText(statLine);
    }
}