using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    

    void Start()
    {
        Debug.Log("Connecting...");

        //PhotonNetwork.GameVersion = "0.0.1";
        PhotonNetwork.ConnectUsingSettings(); // Connect to master server
    }

    public override void OnConnectedToMaster()
    {
        print("Connected to Server");
        //base.OnConnectedToMaster();

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        print("We're connected and in a room now");
        //base.OnJoinedLobby();
        //PhotonNetwork.JoinOrCreateRoom("test", null, null);

        SceneManager.LoadScene("Lobby");
    }

    public override void OnJoinedRoom()
    {
        //base.OnJoinedRoom();

        print("We're connected and in a room!");

        //GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity);
    }

    /*public override void OnDisconnected(DisconnectCause cause)
    {
        print("Disconnected from server for reason " + cause.ToString());
    }*/

}
