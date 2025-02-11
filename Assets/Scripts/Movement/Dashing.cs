using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Dashing : MonoBehaviour
{

    [Header("References")]
    public JimmyMove pm;
    public Rigidbody rb;
    public Transform cam;

    [Header("Modifiers")]
    public float dashStrength;
    public float dashDuration;


    PhotonView view;

    void Start()
    {
        view = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (UIController.GameIsPaused) return;
        if (view.IsMine)
            if (Input.GetKeyDown(pm.dashKey)) Dash();
    }

    private void Dash()
    {
        // First, cut off momentum
        rb.velocity = new Vector3(0, 0, 0);

        // Then, add new momentum
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 dashForce;
        if(moveHorizontal != 0 || moveVertical != 0)
            dashForce = (cam.forward * moveVertical + cam.right * moveHorizontal) * (float)dashStrength;
        else
            dashForce = cam.forward * (float)dashStrength;

        rb.useGravity = false;
        rb.AddForce(dashForce, ForceMode.Impulse);
        Invoke(nameof(StopDash), dashDuration);
    }

    private void StopDash()
    {
        rb.useGravity = true;
    }

    /*[Header("References")]
    public Transform orientation;
    public Transform playerCam;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float maxDashYSpeed;
    public float dashDuration;

    [Header("CameraEffects")]
    public PlayerCam cam;
    //public float dashFov;

    [Header("Settings")]
    public bool useCameraForward = true;
    public bool allowAllDirections = true;
    public bool disableGravity = false;
    public bool resetVel = true;

    [Header("Cooldown")]
    public float dashCd;
    private float dashCdTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if(dashCdTimer > 0)
            dashCdTimer -= Time.deltaTime;
        else if (Input.GetKeyDown(pm.dashKey))
            Dash();
    }

    private void Dash()
    {
        if (dashCdTimer > 0) return;
        else dashCdTimer = dashCd;

        pm.dashing = true;
        pm.maxYSpeed = maxDashYSpeed;

        Vector3 direction = GetDirection(playerCam);

        Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardForce;

        if (disableGravity)
            rb.useGravity = false;

        delayedForceToApply = forceToApply;
        Invoke(nameof(DelayedDashForce), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector3 delayedForceToApply;
    private void DelayedDashForce()
    {
        if (resetVel)
            rb.velocity = Vector3.zero;

        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        pm.dashing = false;
        pm.maxYSpeed = 0;

        //cam.DoFov(85f);

        if (disableGravity)
            rb.useGravity = true;
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3();

        if (allowAllDirections)
            direction = forwardT.forward * verticalInput + forwardT.right * horizontalInput;
        else
            direction = forwardT.forward;

        if (verticalInput == 0 && horizontalInput == 0)
            direction = forwardT.forward;

        return direction.normalized;
    }*/
}