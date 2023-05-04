using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamInterface;
using System;
using UnityEngine.UI;

public class csSceneOwner : MonoBehaviour
{
    [Header("날씨 스크립트 가져오기")]
    public csDayCtrl dayCtrl;

    [Header("분마다 시간체크용")]
    public string oldTime;

    [Header("UI 관련")]
    public Text timeText;

    [Header("포톤 관련")]
    public PhotonView pV;


    [Header("채팅 관련")]
    public Text txtConnect;
    public Text txtLogMsg;
    public InputField enterText;    
    public bool useEnter = false;

    [Header("스폰 관련")]
    public GameObject enemySpawn;
    public int enemySpawnCount;


    private void Awake()
    {
        pV = GetComponent<PhotonView>();

        PhotonNetwork.Instantiate("RPCManager", Vector3.zero, Quaternion.identity, 0);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (PhotonNetwork.connectedAndReady && PhotonNetwork.isMasterClient)//방장일때탄다
        {
            StartCoroutine(EnemySpawn());

            dayCtrl = PhotonNetwork.InstantiateSceneObject("SkyDome", new Vector3(128, -74f, 128), Quaternion.identity, 0, null).GetComponent<csDayCtrl>();

            dayCtrl.masterPG = GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>();

            InvokeRepeating("GrowthTimeCheck", 0f, 0.2f);//타이머시작
        }

        enterText.text = "";
        enterText.gameObject.SetActive(false);

        PhotonNetwork.Instantiate("Player1", new Vector3(10, 30, 10), Quaternion.identity, 0);
    }

    private void Update()
    {
        if (!useEnter && Input.GetKeyDown(KeyCode.Return))
        {
            useEnter = true;
            enterText.gameObject.SetActive(true);
            enterText.ActivateInputField();
        }
        else if (useEnter && Input.GetKeyDown(KeyCode.Return))
        {
            useEnter = false;
            enterText.gameObject.SetActive(false);
            enterText.DeactivateInputField();
        }
    }

    [PunRPC]
    public void RPCGrowthTimeCheck()
    {
        string date = DateTime.Now.ToString("yy.MM.dd ") + DateTime.Now.DayOfWeek.ToString().ToUpper().Substring(0, 3);
        //or date = DateTime.Now.ToString("yyyy. MM. dd. ddd");
        string time = DateTime.Now.ToString("HH:mm");

        if (oldTime == null)
        {
            oldTime = DateTime.Now.ToString("HH:mm");

        }

        if (!oldTime.Equals(time))
        {
            oldTime = time;

            if (PhotonNetwork.isMasterClient)
            {
                dayCtrl.NextTime();
            }
        }

        //ui text에 넣을 수 있음
        //text_date.text = date;
        //text_time.text = time;
        timeText.text = date + "\n" + time;
        //Debug.Log(string.Format("{0}\n{1}", date, time));
    }
    void GrowthTimeCheck()//타이머
    {
        pV.RPC("RPCGrowthTimeCheck", PhotonTargets.All, null);
    }  

    public void OnEnterChat()
    {
        string msg = "\n\t<color=#ffffff>[" + PhotonNetwork.player.NickName + "] : " + enterText.text + "</color>";

        pV.RPC("LogMsg", PhotonTargets.AllBufferedViaServer, msg);

        enterText.text = "";
    }

    public void GetConnectPlayerCount()
    {
        Room currRoom = PhotonNetwork.room;

        txtConnect.text = currRoom.PlayerCount.ToString() + "/" + currRoom.MaxPlayers.ToString();
    }

    public void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log(newPlayer.ToStringFull());

        GetConnectPlayerCount();

        string msg = "\n\t<color=#00ff00>[" + newPlayer.NickName + "] Connected</color>";

        pV.RPC("LogMsg", PhotonTargets.AllBuffered, msg);
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer outPlayer)
    {
        GetConnectPlayerCount();
    }

    public void LogMsgAll(string msg)
    {
        pV.RPC("LogMsg", PhotonTargets.AllBuffered, msg);
    }


    [PunRPC]
    public void LogMsg(string msg)
    {
        txtLogMsg.text = txtLogMsg.text + msg;
    }

    IEnumerator EnemySpawn()
    {
        //Debug.Log("애너미스폰");

        Transform[] enemySpawnPoint = enemySpawn.GetComponentsInChildren<Transform>();

        //Debug.Log(enemySpawnPoint.Length);

        int maxEnemyPrefaps = csLevelManager.Ins.enemyPrefaps.Length;

        while (enemySpawnCount < 20)
        {
            PhotonNetwork.InstantiateSceneObject(csLevelManager.Ins.enemyPrefaps[UnityEngine.Random.Range(0, maxEnemyPrefaps)].name, enemySpawnPoint[UnityEngine.Random.Range(1, enemySpawnPoint.Length)].position, Quaternion.identity, 0, null);

            enemySpawnCount++;
            //Debug.Log(enemySpawnCount);
        }
        yield return null;
    }
}
