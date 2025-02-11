using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrisbeeThrow : MonoBehaviour
{


    [Header("References")]
    public Transform playerCam;
    public Transform throwPoint;
    public GameObject disc;
    public PlayerMovement pm;

    [Header("Throwing")]
    public float throwForce;

    [Header("Cooldown")]
    public float discCd;
    private float discCdTimer;

    void Update()
    {
        if (UIController.GameIsPaused) return;
        if (discCdTimer > 0)
            discCdTimer -= Time.deltaTime;
        else if (Input.GetKeyDown(pm.discKey))
            Toss();
    }

    void Toss()
    {
        discCdTimer = discCd;

        // Correct the rotation of the disc
        Quaternion newRotation = Quaternion.Euler(playerCam.eulerAngles.x-90, playerCam.eulerAngles.y, playerCam.eulerAngles.z);

        // instantiate object to throw
        GameObject discObj = Instantiate(disc, throwPoint.position, newRotation);

        // get rigidbody component
        Rigidbody projectileRb = discObj.GetComponent<Rigidbody>();

        // calculate direction
        //Vector3 forceDirection = cam.transform.forward;

        //RaycastHit hit;

        //if (Physics.Raycast(cam.position, cam.forward, out hit, 500f))
        //{
        //    forceDirection = (hit.point - attackPoint.position).normalized;
        //}

        // add force
        //Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;
        Vector3 forceToAdd = playerCam.forward * throwForce + transform.up * 1;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

    }
}
