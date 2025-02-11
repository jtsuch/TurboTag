using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;

    /*
     * Move the camera with the player
     */
    private void Update()
    {
        //Debug.Log("Trans.pos = " + transform.position + "\ncameraPos.pos = " + cameraPosition.position);
        transform.position = cameraPosition.position;
    }
}
