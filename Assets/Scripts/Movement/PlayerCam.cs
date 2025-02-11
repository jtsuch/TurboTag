using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;

public class PlayerCam : MonoBehaviour
{
    public float sensValue;
    public Transform player;

    //public UIController UIScript;

    float xRotation;
    float yRotation;

    PhotonView view;

    private void Start()
    {
        view = GetComponent<PhotonView>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /*
     * Called once per frame
     * 
     * Updates direction the camera is facing
     */
    private void FixedUpdate()
    {
        if (UIController.GameIsPaused) return;
        if (view.IsMine)
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensValue;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensValue;

            //if(UIController.GameIsPaused)
            //{
            //    mouseX = 0;
            //    mouseY = 0;
            //}

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0); // Rotate camera
            player.rotation = Quaternion.Euler(0, yRotation, 0); // Rotate player

            // Move the camera with the player (adjust vertical height
            Vector3 camPosition = new Vector3(player.position.x, player.position.y + 2f, player.position.z);
            transform.position = camPosition;
        }
           
    }

    public void DoFov(float endValue)
    {
        if (!UIController.GameIsPaused) GetComponent<Camera>().DOFieldOfView(endValue, 0.25f); // Transition FOV in X amount of seconds
    }
}
