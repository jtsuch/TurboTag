using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomList : MonoBehaviourPunCallbacks
{
    public GameObject RoomPrefab;
    public GameObject[] AllRooms;

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Destroy every room
        for (int i = 0; i < AllRooms.Length; i++)
        {
            if (AllRooms[i] != null)
            {
                Destroy(AllRooms[i]);
            }
        }

        // Create new room list
        AllRooms = new GameObject[roomList.Count];

        // Repopulate room list with all rooms that are still open and visible
        for (int j = 0; j < roomList.Count; j++)
        {
            if (roomList[j].IsOpen && roomList[j].IsVisible && roomList[j].PlayerCount >= 1)
            {
                // Create game object "Room" from Prefab and put it under "Content"
                GameObject Room = Instantiate(RoomPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("Content").transform);

                // Correct the name on button
                Room.GetComponent<Room>().Name.text = roomList[j].Name;

                AllRooms[j] = Room;
            }
        }
    }
}
