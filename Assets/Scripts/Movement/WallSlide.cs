using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallSlide : MonoBehaviour
{
    //public Rigidbody rb;
    public PlayerMovement pm;

    void OnTriggerEnter(Collider playerObj)
    {
        pm.wallTrigger = true;
    }

    void OnTriggerExit(Collider playerObj)
    {
        pm.wallTrigger = false;
    }
}
