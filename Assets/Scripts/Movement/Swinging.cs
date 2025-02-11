using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Swinging : MonoBehaviour
{
    [Header("References")]
    public JimmyMove pm;
    public Transform cam, gunTip, player;
    public LayerMask canGrapple;
    public GameObject rope;
    public LineRenderer lr;

    [Header("Modifiers")]
    public float grappleLength;
    public float springyness; // Recommended 4.5f
    public Color startRopeColor;
    public Color endRopeColor;


    PhotonView view;

    private SpringJoint joint;
    private Vector3 swingPoint;
    private float distanceFromPoint;

    private void Start()
    {
        view = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (UIController.GameIsPaused) return;
        if (view.IsMine)
        {
            if (Input.GetKeyDown(pm.swingKey)) StartSwing();
            if (Input.GetKeyUp(pm.swingKey)) StopSwing();

            if (joint) // IF we are grappling
            {
                if (Input.GetKey(pm.sprintKey))
                {
                    pullUp();
                }
                else if (Input.GetKey(pm.crouchKey))
                {
                    pushDown();
                }
            }
        }
            
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void StartSwing()
    {
        pm.isSwinging = true;

        rope.SetActive(true);

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, grappleLength, canGrapple))
        {
            swingPoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;

            distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            // the distance grapple will try to keep from grapple point. 
            joint.maxDistance = distanceFromPoint;
            joint.minDistance = 0f; // distanceFromPoint * 0.25f;

            // Spring details
            joint.spring = springyness; // 4.5f
            joint.damper = 2f; // 7f
            joint.massScale = 30f; // 4.5f

            lr.positionCount = 2;
        }
    }

    public void StopSwing()
    {
        pm.isSwinging = false;
        lr.positionCount = 0;
        rope.SetActive(false);
        Destroy(joint);
    }

    public void pullUp()
    {
        if (distanceFromPoint < joint.maxDistance)
            joint.maxDistance = distanceFromPoint;
        joint.maxDistance -= 20f * Time.deltaTime;
    }

    public void pushDown()
    {
        joint.maxDistance += 20f * Time.deltaTime;
    }

    private void DrawRope()
    {
        // if not grappling, don't draw rope
        if (!joint) return;

        //gunTip.position = Vector3.Lerp(gunTip.position, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
    }
}