using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csGameStart : MonoBehaviour
{
    public GameObject mg;

    private void Start()
    {
        PhotonNetwork.Instantiate(mg.name, Vector3.zero, Quaternion.identity, 0);
    } 

}
