using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon;
using Photon.Realtime;

//https://www.youtube.com/watch?v=-cKiC0huc_w&ab_channel=RamJack

using TeamInterface;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary; // System.Runtime.Serialization.Formatters.Binary 네임스페이스 추가

public class csRPCManager : UnityEngine.MonoBehaviour
{
    [SerializeField]
    PhotonView pV;

    public csPhotonGame csPG;

    public GameObject[] childObj;

    public csMap map;

    private void Awake()
    {
        pV = GetComponent<PhotonView>();
        csPG = GetComponentInChildren<csPhotonGame>();
        map= GameObject.FindGameObjectWithTag("Map").GetComponent<csMap>();

        csPG.pV = pV;

        if (!pV.isMine)
        { 
            //this.gameObject.SetActive(false);

            foreach(GameObject obj in childObj)
            {
                obj.SetActive(false);
            }
        }
        // csPG.InitMap();  
    }

    [PunRPC]
    public void CreateBlockChildRPC(Vector3 pos, Enum_CubeState tmpCS, int tmpNum)
    {
        //Debug.Log("자식생성타니");

        //CreateBlockChildRPCAction(pos,tmpCS,tmpNum);
        map.CreateBlockChildRPCAction(pos,tmpCS,tmpNum);
    }   

    [PunRPC]
    public void DropItemCreateRPC(string objName, Vector3 pos, int count = 1)
    {
        //마스터 클라이언트가 처리 함
        GameObject tmpObj = PhotonNetwork.InstantiateSceneObject(objName, pos, Quaternion.identity, 0, null);
        tmpObj.GetComponent<Item>().count = count;
    }

    [PunRPC]
    public void DestroyRoomRPC()
    {
        csPG.tPlayer.SaveInvenData();

        Invoke("DestroyRoom", 2f);
    }

    public void DestroyRoom()
    {
        PhotonNetwork.LeaveRoom(true);
    }


    [PunRPC]
    public void PlayEffectSoundPhotonRPC(Vector3 pos, int tpye)
    {
        csLevelManager.Ins.PlayAudioClip(pos, tpye);
    }

    [PunRPC]
    public void NextDayRPC()
    {
        csPG.GoodMorningPG();
    }

    [PunRPC]
    public void CreateBluePrintRPC(string objName, Vector3 pos)
    {
        PhotonNetwork.InstantiateSceneObject(objName, pos, Quaternion.identity, 0, null);
    }

    [PunRPC]
    public void SetObjDMGRPC(Vector3 pos, float dmg, Enum_PlayerUseItemType ui)
    {
        map.SetObjDMGRPCAction(pos, dmg, ui);
    }

    [PunRPC]
    public void RPCActionHOE(Vector3 blockPos)
    {
        //Debug.Log("누군가 밭만듬");
        map.RPCActionHOEAction(blockPos);
    }

    [PunRPC]
    public void CreateCube(Vector3 blockPos, Enum_CubeType type)
    {
        map.CreateCubeAction(blockPos, type);
    }

    [PunRPC]
    public void ActionSHOVELRPC(Vector3 blockPos)
    {
        map.ActionSHOVELRPCAction(blockPos);
    }

    [PunRPC]
    public void DrawBlock(Vector3 blockPos)//블록 그리는 함수
    {
        map.DrawBlockAction(blockPos);
    }

    [PunRPC]
    public void CreateDropItemRPC(Vector3 pos, string str)
    {
        //Debug.Log(dropItem.name);
        GameObject tmp = PhotonNetwork.InstantiateSceneObject(str, pos, Quaternion.identity, 0, null);
        tmp.GetComponent<Rigidbody>().AddForce(Vector3.up * Time.deltaTime * 6000f);
        tmp.transform.SetParent(null);
    }

    [PunRPC]
    public void DelChildObjRPC(Vector3 pos)
    {
        map.DelChildObjRPCAction(pos);
    }

    public void OnLeftRoom()
    {
        //포톤 방나감 콜백 대충 여기서 세이브
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("scLobby0");
    }


    [PunRPC]
    public void StartSmile()
    {
        //if (pV.isMine)
        {
            csPG.myPlyerCtrl.StartSmile();
        }
    }



}
