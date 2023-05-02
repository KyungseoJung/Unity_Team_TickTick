using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csGameStart : MonoBehaviour
{
    private void Awake()
    {
        PhotonNetwork.Instantiate("MainGamePrefap", Vector3.zero, Quaternion.identity, 0);
    }
}
