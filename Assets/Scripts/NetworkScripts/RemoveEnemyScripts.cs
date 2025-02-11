using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RemoveEnemyScripts : MonoBehaviour
{

    public GameObject enemyCamera;
    public GameObject enemyScripts;
    public SkinnedMeshRenderer myPenguinBody;

    void Start()
    {
        // IF this isn't my player
        if (!GetComponent<PhotonView>().IsMine)
        {
            // Destroy camera view
            Destroy(enemyCamera);

            // Disable their movement script
            enemyScripts.GetComponent<JimmyMove>().enabled = false;
            //enemyScripts.GetComponent<Dashing>().enabled = false;
            //enemyScripts.GetComponent<>().enabled = false;
            //enemyScripts.GetComponent<>().enabled = false;
        }
        else
        {
            // Hide my body so it's not visible to me
            myPenguinBody.enabled = false;
        }

    }
}
