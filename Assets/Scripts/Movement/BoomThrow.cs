using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomThrow : MonoBehaviour
{

    [Header("References")]
    public Transform playerCam;
    public Transform throwPoint;
    public GameObject theBoom;
    public PlayerMovement pm;

    [Header("Throwing")]
    public float throwForce;
    public float throwUpwardForce;

    [Header("Cooldown")]
    public float boomCd;
    private float boomCdTimer;

    void Update()
    {
        if (UIController.GameIsPaused) return;
        if (boomCdTimer > 0)
            boomCdTimer -= Time.deltaTime;
        else if (Input.GetKeyDown(pm.boomKey))
            Boom();
    }

    void Boom()
    {
        boomCdTimer = boomCd;

        // instantiate object to throw
        GameObject theBoomBomb = Instantiate(theBoom, throwPoint.position, playerCam.rotation);

        // get rigidbody component
        Rigidbody projectileRb = theBoomBomb.GetComponent<Rigidbody>();

        // calculate direction
        //Vector3 forceDirection = cam.transform.forward;

        //RaycastHit hit;

        //if (Physics.Raycast(cam.position, cam.forward, out hit, 500f))
        //{
        //    forceDirection = (hit.point - attackPoint.position).normalized;
        //}

        // add force
        //Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;
        Vector3 forceToAdd = playerCam.forward * throwForce + transform.up * throwUpwardForce;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

    }
}
