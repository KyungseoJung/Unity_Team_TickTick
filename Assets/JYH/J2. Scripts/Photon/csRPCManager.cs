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

public class csRPCManager : Photon.MonoBehaviour
{
    [SerializeField]
    PhotonView pV;

    public csPhotonGame csPG;

    private void Awake()
    {
        pV = GetComponent<PhotonView>();
        //if (pV.isMine)
        //{
        //    csPG = GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>();
        //    csPG.pV = pV;
        //}
       // csPG.InitMap();
    }


    [PunRPC]
    public void CreateBlockChildRPC(Vector3 pos, Enum_CubeState tmpCS, int tmpNum)
    {
        csPG.worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj.GetComponent<csCube>().SetObj(tmpCS, tmpNum);
    }    

    [PunRPC]
    public void DropItemCreateRPC(string objName, Vector3 pos, int count = 1)
    {
        GameObject tmpObj = PhotonNetwork.InstantiateSceneObject(objName, pos, Quaternion.identity, 0, null);
        tmpObj.GetComponent<Item>().count = count;
    }

    [PunRPC]
    public void DestroyRoomRPC()
    {
        StopAllCoroutines();
        CancelInvoke();

        GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>().SaveInvenData();

        Invoke("DestroyRoom", 1f);
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
        csPG.worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj.GetComponent<csCube>().StartAction(dmg, ui);
    }

    [PunRPC]
    public void RPCActionHOE(Vector3 blockPos)
    {
        csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj.GetComponent<csCube>().SetObj(Enum_CubeState.FIELD);
    }

    [PunRPC]
    public void CreateCube(Vector3 blockPos, Enum_CubeType type)
    {
        switch (type)
        {
            case Enum_CubeType.SOIL:
                {
                    GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[3], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.SOIL, true, tmpObj, true, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    //m_nodeArr[(int)blockPos.x, (int)blockPos.z] = tmpObj.GetComponent<Node>();
                }
                break;
            case Enum_CubeType.WATER:
                {
                    GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[5], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.WATER, true, tmpObj, true, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    // m_nodeArr[(int)blockPos.x, (int)blockPos.z] = tmpObj.GetComponent<Node>();

                    int tmpY = (int)blockPos.y;
                    while (tmpY > 0)
                    {
                        if (csPG.worldBlock[(int)blockPos.x, tmpY, (int)blockPos.z] == null)
                        {
                            csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.WATER, false, tmpObj, false, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);

                            tmpY--;
                            //Debug.Log(blockPos);
                        }
                        else if (csPG.worldBlock[(int)blockPos.x, tmpY, (int)blockPos.z] != null)
                        {
                            break;
                        }
                    }
                }
                break;
        }
    }

    [PunRPC]
    public void ActionSHOVELRPC(Vector3 blockPos)
    {
        Destroy(csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj);
        csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = null;
    }

    [PunRPC]
    public void DrawBlock(Vector3 blockPos)//블록 그리는 함수
    {
        if (csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] == null)
        {
            //Debug.Log(1);
            return;
        }


        if (!csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].vis)
        {
            csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].vis = true;

            GameObject tmpObj = null;

            bool tmpTop = false;

            if (csPG.worldBlock[(int)blockPos.x, (int)blockPos.y + 1, (int)blockPos.z] == null)
            {
                tmpTop = true;
            }

            switch (csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].type)
            {
                case Enum_CubeType.DARKSOIL:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[0], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.DARKSOIL, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
                case Enum_CubeType.STON:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[1], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.STON, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
                case Enum_CubeType.GRASS:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[2], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.GRASS, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
                case Enum_CubeType.SOIL:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[3], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.SOIL, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
                case Enum_CubeType.SEND:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[4], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.SEND, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
                case Enum_CubeType.WATER:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[5], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.WATER, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(csPG.worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
            }
        }
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
        csPG.worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj.GetComponent<csCube>().DestroyChild();
    }

    [PunRPC]
    public void DestroyRoom()
    {
        PhotonNetwork.LeaveRoom(true);
    }

    public void OnLeftRoom()
    {
        //포톤 방나감 콜백 대충 여기서 세이브
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("scLobby0");
    }   
}
