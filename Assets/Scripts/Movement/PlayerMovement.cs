using System.Collections;
using System;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode dashKey = KeyCode.E;
    public KeyCode slideKey = KeyCode.LeftControl;
    public KeyCode swingKey = KeyCode.Mouse0;
    public KeyCode boomKey = KeyCode.Q;
    public KeyCode discKey = KeyCode.F;

    [Header("References")]
    public Transform orientation;
    public LayerMask structureLayer;
    public Climbing climbingScript;
    public PlayerCam cam;
    public TextMeshProUGUI statText;

    [Header("Adjustables")]
    public float sensitivity;
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float swingSpeed;
    public float slideSpeed;
    public float climbSpeed;
    public float dashSpeed;

    [Header("To Get Rid Of")]
    public float playerHeight;
    public bool grounded;
    public bool wallTrigger;
    public float maxFov = 85f;
    public bool freeze;
    public bool unlimited;
    public bool restricted;
    public bool swinging;
    public bool sliding;
    public bool climbing;
    public bool dashing;

    [Header("Unsure")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    public float dashSpeedChangeFactor;
    public float maxYSpeed;
    public float speedIncreaseMultiplier; // Lower = speeds up slower
    public float slopeIncreaseMultiplier;
    public float wallClimbStrength;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float swingMultiplier;
    bool readyToJump;
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Wall running")]
    public float wallrunForce, maxWallrunTime, maxWallSpeed;
    bool isWallRight, isWallLeft;
    bool isWallRunning;
    public float maxWallRunCameraTilt, wallRunCameraTilt;

    //  Private variables
    private float xRotation;
    private float sensMultiplier = 1f;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        freeze,
        unlimited,
        swinging,
        walking,
        sprinting,
        crouching,
        dashing,
        sliding,
        climbing,
        air
    }

    /*
     * 
     * Basic functions that the game calls itself
     * 
     */
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, structureLayer);

        MyInput();
        SpeedControl();
        StateHandler();
        CheckForWall();
        WallRunInput();
        Look();

        // handle drag
        if (grounded && state != MovementState.dashing)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

        // Field of view
        cam.DoFov(60f + Math.Min(maxFov, moveSpeed * 1.6f)); // speed btwn 0 and 15, 15*X+60=95, X=5/3

        DisplayStats();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    /*
     * 
     * Handling events that take place
     * 
     */
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(UIController.GameIsPaused)
        {
            horizontalInput = 0;
            verticalInput = 0;
            //Debug.Log("Paused in MyInput");
        }

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && (grounded || isWallRunning))
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouch
        if (!swinging && Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        // Mode - Freeze
        if (freeze)
        {
            state = MovementState.freeze;
            desiredMoveSpeed = 0f;
            rb.velocity = Vector3.zero;
        }

        // Mode - Unlimited
        else if (unlimited)
        {
            state = MovementState.unlimited;
            moveSpeed = 999f;
            return;
        }

        // Mode - Dashing
        else if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }

        // Mode - Swinging
        else if (swinging)
        {
            state = MovementState.swinging;
            desiredMoveSpeed = swingSpeed;
        }

        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.velocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
                keepMomentum = true;
            }
            else
                desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Crouching
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Mode - Climbing
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }

        // Mode - Sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing) keepMomentum = true;

        // check if desiredMoveSpeed has changed drastically
        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
        if (Mathf.Abs(desiredMoveSpeed - moveSpeed) < 0.1f) keepMomentum = false;
    }

    /*
     * 
     * Basic player movements
     * 
     */
    private void MovePlayer()
    {
        if (restricted) return;
        if (climbingScript.exitingWall) return;
        //if (swinging) return;
        if (state == MovementState.dashing) return;

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // If swinging
        if (swinging && !grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * swingMultiplier, ForceMode.Force);
        }

        // on slope
        else if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // in air
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

        // limit y vel
        if (maxYSpeed != 0 && rb.velocity.y > maxYSpeed)
            rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);
        else if (!isWallRunning && wallTrigger && rb.velocity.y < -4) // If sliding down wall, limit Y velocity to -5
        {
            rb.velocity = new Vector3(rb.velocity.x, -4, rb.velocity.z);
        }
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private float speedChangeFactor;
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }

    /*
     * 
     * Jumping
     * 
     */
    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        //rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (isWallLeft)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            rb.AddForce(transform.right * jumpForce, ForceMode.Impulse);
        }
        else if (isWallRight)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            rb.AddForce(-transform.right * jumpForce, ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    /*
     * 
     * Wall Running
     * 
     */
    private float timeOnWall;
    private void WallRunInput() //make sure to call in void Update
    {
        //Wallrun
        if (Input.GetKey(KeyCode.D) && isWallRight) StartWallrun();
        else if (Input.GetKey(KeyCode.A) && isWallLeft) StartWallrun();
        else timeOnWall = 0;
    }

    private void StartWallrun()
    {
        rb.useGravity = false;
        isWallRunning = true;
        timeOnWall += 400 * Time.deltaTime; // Used so that the longer you're on a wall, the quicker you drop
        //allowDashForceCounter = false;

        if (rb.velocity.magnitude <= maxWallSpeed)
        {
            rb.AddForce(orientation.forward * wallrunForce * Time.deltaTime);
        }

        //Make sure char sticks to wall
        if (isWallRight)
            rb.AddForce(orientation.right * wallrunForce / 5 * Time.deltaTime);
        else
            rb.AddForce(-orientation.right * wallrunForce / 5 * Time.deltaTime);

        // Reduce fall speed while wall runnning
        rb.AddForce(orientation.up * (wallClimbStrength-timeOnWall) * Time.deltaTime);
    }

    private void StopWallRun()
    {
        isWallRunning = false;
        rb.useGravity = true;
        timeOnWall = 0;
    }

    private void CheckForWall()
    {
        isWallRight = Physics.Raycast(transform.position, orientation.right, 1f, structureLayer);
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, 1f, structureLayer);

        //leave wall run
        if (!isWallLeft && !isWallRight) StopWallRun();
        //reset double jump (if you have one :D)
        //if (isWallLeft || isWallRight) doubleJumpsLeft = startDoubleJumps;
    }

    /*
     * 
     * Camera control
     * 
     */
    private float desiredX;
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        if (UIController.GameIsPaused)
        {
            mouseX = 0;
            mouseY = 0;
        }

        //Find current look rotation
        Vector3 rot = cam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Perform the rotations
        cam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, wallRunCameraTilt);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);

        //While Wallrunning
        //Tilts camera in .5 second
        if (Math.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallRunning && isWallRight)
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
        if (Math.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallRunning && isWallLeft)
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;

        //Tilts camera back again
        if (wallRunCameraTilt > 0 && !isWallRight && !isWallLeft)
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;
        if (wallRunCameraTilt < 0 && !isWallRight && !isWallLeft)
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
    }

    /*
     * 
     * UI Canvas
     * 
     */
    private void DisplayStats()
    {
        string statLine = "";

        // First line - Velocity 
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (OnSlope())
            statLine += Math.Round(rb.velocity.magnitude, 1) + " / " + Math.Round(moveSpeed, 1);
        else
            statLine += Math.Round(flatVel.magnitude, 1) + " / " + Math.Round(moveSpeed, 1) + " Y: " + Math.Round(rb.velocity.y, 1);

        // Second line - State
        statLine += '\n';
        if (freeze)
            statLine += "FREEZE";
        else if (unlimited)
            statLine += "UNLIMITED";
        else if (swinging)
            statLine += "SWING";
        else if (sliding)
            statLine += "SLIDE";
        else if (!swinging && Input.GetKey(crouchKey))
            statLine += "CROUCH";
        else if (climbing)
            statLine += "CLIMB";
        else if (grounded && Input.GetKey(sprintKey) && !swinging)
            statLine += "SPRINT";
        else if (grounded)
            statLine += "WALK";
        else
            statLine += "AIR";

        // Third line - Grounded status
        statLine += '\n';
        if (grounded)
            statLine += "Grounded";
        else
            statLine += "Not Grounded";

        // Fourth line - Gravity status
        statLine += '\n';
        if (rb.useGravity)
            statLine += "Gravity";
        else
            statLine += "Zero Grav";

        statText.SetText(statLine);
    }
}
