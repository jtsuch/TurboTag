using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomExplosion : MonoBehaviour
{
    private Rigidbody rb;

    private bool targetHit;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {

        Debug.Log(rb.isKinematic);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (targetHit)
            return;
        else
            targetHit = true;

        rb.isKinematic = true;

        // Stick to hit object
        transform.SetParent(collision.transform);

    }
}
