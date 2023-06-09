﻿using System.Collections;
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

public class csPhotonGame : UnityEngine.MonoBehaviour
{
    [Header("맵정보")]
    public MapDataClass mapData = new MapDataClass();   

    [Header("블록 정보 3차원으로 저장")]
    public Block[,,] worldBlock=null;
    public IHighlighter oldBlock;

    public csMap map;

    //[HideInInspector]
    [Header("건물 건설 상태 관련")]
    public bool isReady = false;
    public bool isBuild=false;//애는 각자가지고있어야함 **
    public bool inTheBuilding=false;//건물 안에 들어가면 활성화   
    public bool isCreateFurniture = false;

    [Header("대충 플레이어가 들고있을 변수")]
    [SerializeField]
    public Inventory tPlayer;
    [SerializeField]
    public Enum_PlayerUseItemType UseItemType;
    [SerializeField]
    public GameObject bluePrint=null;
    [SerializeField]
    public GameObject[] bluePrintObj;
    [SerializeField]
    public bool actionNow = true;//지금 뭐 동작중인지 체크
    [SerializeField]
    public float rayCastRange = 20f;
    [SerializeField]
    public PlayerCtrl1 myPlyerCtrl;
    [SerializeField]
    public GameObject craftingUI;
    [SerializeField]
    public GameObject warningWindow;

    [Header("레이 케스팅용")]
    [SerializeField]
    public RaycastHit hit;
    [SerializeField]
    public Ray ray;
    public int LayerMaskBlock;// = 1 << LayerMask.NameToLayer("PreViewCheck");

    [Header("포톤 관련")]
    [SerializeField]
    public PhotonView pV;
    public int myOwnerId;


    [Header("UI 관련")]
    public bool isUiBlock = false;
    public bool keyBlock =true;
    [SerializeField]
    public GameObject crossHair;
    public bool useEnter = false;

    [Header("튜토리얼 캔버스")]
    public GameObject tutorialCanvas;
    public bool gameStart = false;

    public bool mapFinish = false;
    public bool childFinish = false;

    public int GetOwnerID()
    {
        return myOwnerId;
    }

    private void Awake()
    {        
        pV = transform.parent.GetComponent<PhotonView>();
        //this.transform.parent = null;
        // pV.TransferOwnership();
        //룸 프로퍼티 참조
        if (pV.isMine)
        {
            myOwnerId = pV.photonView.ownerId;
            tutorialCanvas.SetActive(true);
            //InitMapData();
            map = GameObject.FindGameObjectWithTag("Map").GetComponent<csMap>();
            StartCoroutine(InitMapData());
        }
    }


    IEnumerator Start()
    {
        if (pV.isMine)
        {
            if (!PhotonNetwork.isMasterClient)
            {
                debugBtn.SetActive(false);
            }
            LayerMaskBlock = 1 << LayerMask.NameToLayer("PreViewCheck");
        }
        //else
        //{
        //    transform.parent.gameObject.SetActive(false);
        //}

        while (!mapFinish)
        {
            yield return new WaitForSeconds(0.2f);
        }

        yield return null;
    }
    
    IEnumerator InitMapData()
    {
        m_nodeArr = new Node[mapData.widthX, mapData.widthZ];

        while (map == null)
        {
            yield return new WaitForSeconds(0.2f);
            map = GameObject.FindGameObjectWithTag("Map").GetComponent<csMap>();
        }

        //기존 맵생성로직
        //worldBlock = new Block[mapData.widthX, mapData.height, mapData.widthZ];
        //List<Vector3> childVector = new List<Vector3>();

        //for (int x = 0; x < mapData.widthX; x++)
        //{
        //    for (int z = 0; z < mapData.widthZ; z++)
        //    {
        //        float xb = (x + 0) / mapData.waveLength;
        //        float zb = (z + 0) / mapData.waveLength;
        //        int y = (int)((Mathf.PerlinNoise(xb, zb) * mapData.amplitude) * mapData.amplitude + mapData.groundHeightOffset);
        //        Vector3 pos = new Vector3(x, y, z);               

        //        CreateBlockData(y, pos, true);               

        //        if (UnityEngine.Random.Range(0, 100) < 40 && worldBlock[(int)pos.x, (int)pos.y , (int)pos.z]!=null && worldBlock[(int)pos.x, (int)pos.y , (int)pos.z].top && !worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].type.Equals(Enum_CubeType.WATER))
        //        {
        //            childVector.Add(pos);
        //        }

        //        while (y > 0)
        //        {
        //            y--;
        //            pos = new Vector3(x, y, z);
        //            CreateBlockData(y, pos, false);
        //        }     
        //    }
        //}

        //SceneManager.LoadScene("addPlayer", LoadSceneMode.Additive);//플레이어스폰포인트로 대체

        //SceneManager.LoadScene("addMain", LoadSceneMode.Additive);//애너미스폰포인트

        worldBlock = map.GetBlock();

        while (worldBlock==null)
        {
            yield return new WaitForSeconds(0.2f);
            worldBlock = map.GetBlock();
        }

        mapFinish = true;

        PhotonNetwork.isMessageQueueRunning = true;        
        

        if (PhotonNetwork.isMasterClient)
        {
            List<Vector3> childVector = new List<Vector3>();

            childVector = map.GetList();

            while (childVector == null)
            {
                yield return new WaitForSeconds(0.2f);
                childVector = map.GetList();
            }

            foreach (Vector3 pos in childVector)
            {
                CreateBlockChild(pos);
            }
        }       

        string tmpStr = "Blueprint_WorkBench";

        if (PhotonNetwork.isMasterClient)
        {
            DropItemCreate(tmpStr, new Vector3(12, 30, 12), 1);
        }
        //yield return new WaitForSeconds(3f);        

        Invoke("LoadInvenDataStart", 3f);

        Invoke("OffTutorialCanvas", 8f);

        //yield return null;
    }

    void LoadInvenDataStart()
    {
        //SceneManager.LoadScene("MainGame_UI", LoadSceneMode.Additive);  //#3-3

        //tPlayer.craftinUI.SetActive(false);
        //##0501 크래프팅 유아이 연결하고 비활성화
        //craftingUI = GameObject.FindGameObjectWithTag("CraftingUI");
        //craftingUI.SetActive(false);

        StartCoroutine(LoadInvenDataStartCoroutine());
    }

    IEnumerator LoadInvenDataStartCoroutine()
    {
        childFinish = true;

        Debug.Log("asdasdbbbaaa");
        yield return new WaitForSeconds(5f);

        tPlayer.LoadInvenData();

        if (tPlayer == null)
        {
            Debug.Log("asdasdbbbaaaccc");
        }

        craftingUI.SetActive(false);

        warningWindow.SetActive(false);

        yield return null;
    }

    void OffTutorialCanvas()
    {
        tutorialCanvas.SetActive(false);

        isReady = true;
        gameStart = true;
        //Destroy(tutorialCanvas);
    }

    public bool GetGameStart()
    {
        if(myPlyerCtrl != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void CreateBlockChild(Vector3 pos)
    {
        //큐브 위에 뭐 만들기 RPC로 해야함
        Enum_CubeState tmpCS = (Enum_CubeState)UnityEngine.Random.Range(0, 100);//어떤 오브젝트 생성될지

        int tmpNum = 0;//생성된 오브젝트의 종류

        switch (tmpCS)
        {
            case Enum_CubeState.GRASS1:
                tmpNum = UnityEngine.Random.Range(0, 3);
                break;
            case Enum_CubeState.TREE1:
                tmpNum = UnityEngine.Random.Range(15, 27);
                break;
            case Enum_CubeState.TREE2:
                tmpNum = UnityEngine.Random.Range(22, 27);
                break;
            case Enum_CubeState.GRASS2:
                tmpNum = UnityEngine.Random.Range(0, 15);
                break;
            case Enum_CubeState.ROCK1:
                tmpNum = UnityEngine.Random.Range(27, 35);
                break;
            default:
                tmpCS = Enum_CubeState.NONE;
                break;
        }

        if (!tmpCS.Equals(Enum_CubeState.NONE))
        {
            //Debug.Log("자식생성");
            pV.RPC("CreateBlockChildRPC", PhotonTargets.AllBufferedViaServer, pos, tmpCS, tmpNum);
        }       
    }


    //[PunRPC]
    //public void DestroyRoomRPC()
    //{
    //    StopAllCoroutines();
    //    CancelInvoke();

    //    GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>().SaveInvenData();

    //    Invoke("DestroyRoom", 1f);
    //}


    //[PunRPC]
    //public void CreateBlockChildRPC(Vector3 pos, Enum_CubeState tmpCS, int tmpNum)
    //{
    //    worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj.GetComponent<csCube>().SetObj(tmpCS, tmpNum);
    //}

    public void PlayEffectSoundPhoton(Vector3 pos, int type)
    {
        pV.RPC("PlayEffectSoundPhotonRPC", PhotonTargets.All, pos, type);
    }

    //[PunRPC]
    //public void PlayEffectSoundPhotonRPC(Vector3 pos, int tpye)
    //{
    //    csLevelManager.Ins.PlayAudioClip(pos, tpye);
    //}

    void CreateBlockData(int y, Vector3 pos, bool v)
    {
        if(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] != null)
        {
            return;
        }

        if (y > 28)
        {
            if (v)
            {
                GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[2], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.GRASS, v, tmpObj);
                tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z]);

                //m_nodeArr[(int)pos.x, (int)pos.z] = tmpObj.GetComponent<Node>();
            }
            else
            {
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.GRASS, v, null, false);
            }
        }
        else if (y > 5)
        {
            if (v)
            {
                GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[3], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.SOIL, v, tmpObj);
                tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z]);
                //m_nodeArr[(int)pos.x, (int)pos.z] = tmpObj.GetComponent<Node>();
            }
            else
            {
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.SOIL, v, null, false);
            }
        }
        else if (y >= 0)
        {
            if (v)
            {
                GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[1], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.STON, v, tmpObj);
                tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z]);
                //m_nodeArr[(int)pos.x, (int)pos.z] = tmpObj.GetComponent<Node>();
            }
            else
            {
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.STON, v, null, false);
            }
        }

        if (y >= 23 && y<27&& worldBlock[(int)pos.x, (int)pos.y, (int)pos.z]!=null && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].vis)
        {
            worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].vis = false;
            worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].top = false;
            Destroy(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj);

            int tmpY = y;

            while (tmpY <= 27)
            {
                if (tmpY == 27)
                {
                    GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[5], new Vector3(pos.x, (27 * 0.5f), pos.z), Quaternion.identity);
                    worldBlock[(int)pos.x, 27, (int)pos.z] = new Block(Enum_CubeType.WATER, true, tmpObj, true);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, 27, (int)pos.z]);
                }
                else
                {
                    worldBlock[(int)pos.x, tmpY, (int)pos.z] = new Block(Enum_CubeType.WATER, false, null, false);
                }
                tmpY++;
            }

           
        }

        //if (y + 1 <= 27 && y + 1 >= 24)
        //{
        //    for (int i = y + 1; i <= 27; i++)
        //    {
        //        if (worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z] == null && i == 27)
        //        {
        //            GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[5], new Vector3(pos.x, (pos.y + (i - y)) * 0.5f, pos.z), Quaternion.identity);
        //            worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z] = new Block(Enum_CubeType.WATER, true, tmpObj, true);
        //            tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z]);
        //            //m_nodeArr[(int)pos.x, (int)pos.z] = tmpObj.GetComponent<Node>();
        //        }
        //        else if (worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z] == null)
        //        {
        //            worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z] = new Block(Enum_CubeType.WATER, false, null, false);
        //        }
        //        else if (worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z] != null && !worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z].type.Equals(Enum_CubeType.WATER))
        //        {
        //            Debug.Log("물 밑에 땅 제거");
        //            childVector.Remove(new Vector3(pos.x, pos.y + (i - y), pos.z));
        //            worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z].top = false;
        //            worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z].vis = false;
        //            Destroy(worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z].obj);
        //        }


        //    }
        //    //Debug.Log(1111);
        //}
    }


    //[PunRPC]
    //public void RPCGrowthTimeCheck()
    //{   
    //    string date = DateTime.Now.ToString("yy.MM.dd ") + DateTime.Now.DayOfWeek.ToString().ToUpper().Substring(0, 3);
    //    //or date = DateTime.Now.ToString("yyyy. MM. dd. ddd");
    //    string time = DateTime.Now.ToString("HH:mm");

    //    if (oldTime == null)
    //    {
    //        oldTime = DateTime.Now.ToString("HH:mm");

    //    }

    //    if (!oldTime.Equals(time))
    //    {
    //        oldTime = time;

    //        if (PhotonNetwork.isMasterClient)
    //        {
    //            dayCtrl.NextTime();
    //        }
    //    }

    //    //ui text에 넣을 수 있음
    //    //text_date.text = date;
    //    //text_time.text = time;
    //    timeText.text = date + "\n" + time;
    //    //Debug.Log(string.Format("{0}\n{1}", date, time));
    //}    

    //[PunRPC]
    //public void NextDayRPC()
    //{
    //    StartCoroutine(GoodMorning());
    //}

    public void GoodMorningPG()
    {
        StartCoroutine(GoodMorning());
    }

    IEnumerator GoodMorning()//아침마다 무럭무럭 자라렴
    {
        //나무들아 아침이다~
        GameObject[] allTree = GameObject.FindGameObjectsWithTag("Tree");

        foreach (GameObject tree in allTree)
        {
            tree.SendMessage("GrowthDay");
        }


        //덤불들아 아침이다~
        GameObject[] allGrass = GameObject.FindGameObjectsWithTag("Grass");

        foreach (GameObject grass in allGrass)
        {
            grass.SendMessage("GrowthDay");
        }

        yield return null;
    }

    public void InTheBuilding(bool ib)
    {
        inTheBuilding = ib;
       // Debug.Log("집이니?" + ib);

        if (inTheBuilding)//건물에 있으면 하우징용 변수 활성화
        {
            if (isBuild&& bluePrint!=null)
            {
                bluePrint.GetComponent<csPreViewBase>().HiedPreView();
                isBuild = false;
            }

            myPlyerCtrl.SetInTheHouse(true);

            if (UseItemType.Equals(Enum_PreViewType.HOUSE_CHAIR))
            {
                bluePrint = bluePrintObj[2];
                bluePrint.SetActive(true);
                isCreateFurniture = true;//빌드모드 시작
            }
            else if (UseItemType.Equals(Enum_PreViewType.HOUSE_TABLE))
            {
                bluePrint = bluePrintObj[3];
                bluePrint.SetActive(true);
                isCreateFurniture = true;//빌드모드 시작
            }            
        }
        else
        {
            if (isCreateFurniture&& bluePrint!=null)
            {
                bluePrint.GetComponent<csPreViewBase>().HiedPreView();
                isCreateFurniture = false;
            }

            myPlyerCtrl.SetInTheHouse(false);

            if (UseItemType.Equals(Enum_PreViewType.FIRE))
            {
                bluePrint = bluePrintObj[0];
                bluePrint.SetActive(true);
                isBuild = true;//빌드모드 시작
            }
            else if (UseItemType.Equals(Enum_PreViewType.TENT))
            {
                bluePrint = bluePrintObj[1];
                bluePrint.SetActive(true);
                isBuild = true;//빌드모드 시작
            }
            else if (UseItemType.Equals(Enum_PreViewType.WORKBENCH))
            {
                bluePrint = bluePrintObj[4];
                bluePrint.SetActive(true);
                isBuild = true;//빌드모드 시작
            }
        }
    }

    public void DropItemCreate(string objName, Vector3 pos, int count=1)
    {
        pV.RPC("DropItemCreateRPC", PhotonTargets.MasterClient, objName, pos, count);
    }
    
    //[PunRPC]
    //public void DropItemCreateRPC(string objName, Vector3 pos, int count=1)
    //{
    //    GameObject tmpObj = PhotonNetwork.InstantiateSceneObject(objName, pos, Quaternion.identity, 0, null);
    //    tmpObj.GetComponent<Item>().count = count;
    //}

    void KeyBlockFct()
    {
        keyBlock = true;
    }

    private void Update()
    {
        if (!pV.isMine)
        {
            return;
        }

        // //맵 로드 안됬으면 아무것도 안한다
        if (!isReady || !gameStart)
        {
            return;
        }

        if (isOM)
        {
            return;
        }

        if (myPlyerCtrl == null)
        {
            return;
        }
        else if(myPlyerCtrl!=null && smile == null)
        {
            smile = myPlyerCtrl.smilePos.gameObject;
        }

        if (!useEnter && Input.GetKeyDown(KeyCode.Return))
        {

            if (!isUiBlock)
            {
                isUiBlock = true;
                //crossHair.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else if (useEnter && Input.GetKeyDown(KeyCode.Return))
        {
            useEnter = false;

            if (isUiBlock)
            {
                isUiBlock = false;
                //crossHair.SetActive(true);
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        if (keyBlock && Input.GetKeyDown(KeyCode.LeftAlt))
        {
            keyBlock = false;

            if (!isUiBlock)
            {
                isUiBlock = true;
                //crossHair.SetActive(false);
                //Debug.Log("uiblock");
                Cursor.lockState = CursorLockMode.None;

                //if (bluePrint != null)
                //{
                //    bluePrint.GetComponent<IPreViewBase>().HiedPreView();
                //}
            }
            else
            {
                isUiBlock = false;
                //crossHair.SetActive(true);
                //Debug.Log("enuiblock");
                Cursor.lockState = CursorLockMode.Locked;
            }

            Invoke("KeyBlockFct", 0.2f);
        }

        ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        //블록 하이라이트 활성화/비활성화
        if (!isBuild && Physics.Raycast(ray, out hit, rayCastRange, LayerMaskBlock))
        {
            if (hit.transform.root.tag == "Block")
            {
                if (oldBlock == null)
                {
                    //Debug.Log(1);
                    oldBlock = hit.transform.root.GetComponent<IHighlighter>();
                    oldBlock.OnHighlighter();
                }
                else if (oldBlock != null && oldBlock != hit.transform.root.GetComponent<IHighlighter>())
                {
                    //Debug.Log(2);
                    oldBlock.OffHighlighter();
                    oldBlock = hit.transform.root.GetComponent<IHighlighter>();
                    oldBlock.OnHighlighter();
                }

                if (hit.transform.GetComponent<csCube>() != null && (hit.transform.GetComponent<csCube>().cubeInfo.haveChild == false && !hit.transform.GetComponent<csCube>().cubeInfo.type.Equals(Enum_CubeType.WATER)))
                {
                    Vector3 tmpPos = hit.transform.position;
                    targetPos = new Vector3(tmpPos.x, tmpPos.y * 2, tmpPos.z);
                }
            }
        }

        //if (UseItemType.Equals(Enum_PlayerUseItemType.BLUEPRINT) && !isBuild)//청사진 들고있을때
        //{
        //    isBuild = true;//빌드모드 시작
        //}                

        if (UseItemType.Equals(Enum_PlayerUseItemType.BLUEPRINTWATCHFIRE) || UseItemType.Equals(Enum_PlayerUseItemType.BLUEPRINTTENT) || UseItemType.Equals(Enum_PlayerUseItemType.BLUEPRINTWORKBENCH)
            && isBuild && !inTheBuilding)//&& !isUiBlock)
        {
            if (bluePrint == null)
            {
                SelectSlot.Ins.ReSetShowItem();
            }
        }
        else if (UseItemType.Equals(Enum_PlayerUseItemType.BLUEPRINTCHAIR) || UseItemType.Equals(Enum_PlayerUseItemType.BLUEPRINTTABLE)
            && isCreateFurniture && inTheBuilding)//&& !isUiBlock)
        {
            if (bluePrint == null)
            {
                SelectSlot.Ins.ReSetShowItem();
            }
        }


        if (isBuild && bluePrint != null)//빌드모드일 때 미리보기 그려주기
        {
            if (Physics.Raycast(ray, out hit, rayCastRange, LayerMaskBlock))
            {
                if (hit.transform.tag == "Block")
                {
                    if (oldBlock != null)
                    {
                        oldBlock.OffHighlighter();
                        oldBlock = null;
                    }

                    Vector3 blockPos = hit.transform.position;
                    IPreViewBase tmpPreView = bluePrint.GetComponent<IPreViewBase>();
                    bool tmpCheck = true;//땅에 닿아있는지 체크

                    if (tmpPreView == null)
                    {
                        return;
                    }

                    for (int x = -(tmpPreView.SizeX / 2); x <= tmpPreView.SizeX / 2 && tmpCheck; x++)
                    {
                        for (int z = -(tmpPreView.SizeZ / 2); z <= tmpPreView.SizeZ / 2 && tmpCheck; z++)
                        {
                            if (blockPos.x + x < 0 || blockPos.x > mapData.widthX)
                            {
                                tmpCheck = false;
                                continue;
                            }
                            if (blockPos.z + z < 0 || blockPos.z > mapData.widthZ)
                            {
                                tmpCheck = false;
                                continue;
                            }

                            if (worldBlock[(int)(blockPos.x + x), (int)(blockPos.y * 2f), (int)(blockPos.z + z)] == null)
                            {
                                tmpCheck = false;
                            }
                            else if (worldBlock[(int)(blockPos.x + x), (int)(blockPos.y * 2f), (int)(blockPos.z + z)] != null)
                            {
                                if (worldBlock[(int)(blockPos.x + x), (int)(blockPos.y * 2f), (int)(blockPos.z + z)].type.Equals(Enum_CubeType.WATER))
                                {
                                    tmpCheck = false;
                                }
                            }
                        }
                    }
                    //Debug.Log(tmpCheck);
                    tmpPreView.ShowPreView(blockPos, tmpCheck);
                }
                else if (hit.transform.root.tag == "Block")
                {
                    if (oldBlock != null)
                    {
                        oldBlock.OffHighlighter();
                        oldBlock = null;
                    }

                    Vector3 blockPos = hit.transform.root.position;
                    IPreViewBase tmpPreView = bluePrint.GetComponent<IPreViewBase>();
                    bool tmpCheck = true;

                    if (tmpPreView == null)
                    {
                        return;
                    }

                    for (int x = -(tmpPreView.SizeX / 2); x <= tmpPreView.SizeX / 2 && tmpCheck; x++)
                    {
                        for (int z = -(tmpPreView.SizeZ / 2); z <= tmpPreView.SizeZ / 2 && tmpCheck; z++)
                        {
                            if (blockPos.x + x < 0 || blockPos.x > mapData.widthX)
                            {
                                tmpCheck = false;
                                continue;
                            }
                            if (blockPos.z + z < 0 || blockPos.z > mapData.widthZ)
                            {
                                tmpCheck = false;
                                continue;
                            }

                            if (worldBlock[(int)(blockPos.x + x), (int)(blockPos.y * 2f), (int)(blockPos.z + z)] == null)
                            {
                                tmpCheck = false;
                            }
                        }
                    }

                    tmpPreView.ShowPreView(blockPos, tmpCheck);
                }
            }
        }
        //else
        if (isCreateFurniture && bluePrint != null)
        {
            //int layerMask = ~(1 << LayerMask.NameToLayer("Item"));

            //Debug.Log("가구보여주기 시작");
            if (Physics.Raycast(ray, out hit, rayCastRange, LayerMaskBlock))
            {
                //Debug.Log(hit.transform.name+"뭐에 맞고있냐..?");

                if (hit.transform.tag == "HouseBlock")
                {
                    if (oldBlock != null)
                    {
                        oldBlock.OffHighlighter();
                        oldBlock = null;
                    }

                    Vector3 blockPos = hit.transform.position;
                    IPreViewBase tmpPreView = bluePrint.GetComponent<IPreViewBase>();

                    if (tmpPreView == null)
                    {
                        return;
                    }
                    //Debug.Log("가구보여줘");
                    tmpPreView.ShowPreView(blockPos, true);
                }
            }
        }

        if (isUiBlock)
        {
            return;
        }

        if (myPlyerCtrl != null && !myPlyerCtrl.m_execute && Input.GetMouseButtonDown(0) && !isBuild && !isCreateFurniture && !inTheBuilding)
        {
            myPlyerCtrl.FindPathCoroutine(targetPos);
        }

        if (actionNow && Input.GetMouseButtonDown(1))
        {
            StartCoroutine(PlayerUseItem(UseItemType));
        }
        /*
            //// 여기서부터 안쓰일 예정
        if (actionNow && Input.GetKeyDown(KeyCode.Q))
        {

            switch (UseItemType)
            {
                case Enum_PlayerUseItemType.HAND:
                    UseItemType = Enum_PlayerUseItemType.AXE;
                    break;
                case Enum_PlayerUseItemType.AXE:
                    UseItemType = Enum_PlayerUseItemType.PICKAXE;
                    break;
                case Enum_PlayerUseItemType.PICKAXE:
                    UseItemType = Enum_PlayerUseItemType.SHOVEL;
                    break;
                case Enum_PlayerUseItemType.SHOVEL:
                    UseItemType = Enum_PlayerUseItemType.HOE;
                    break;
                case Enum_PlayerUseItemType.HOE:
                    UseItemType = Enum_PlayerUseItemType.BLOCKSOIL;
                    break;
                case Enum_PlayerUseItemType.BLOCKSOIL:
                    UseItemType = Enum_PlayerUseItemType.HAND;
                    break;
            }

            Debug.Log("QQQQQQQQ" + UseItemType);
        }
        else if (actionNow && Input.GetKeyDown(KeyCode.E))
        {

            switch (UseItemType)
            {
                case Enum_PlayerUseItemType.HAND:
                    UseItemType = Enum_PlayerUseItemType.BLOCKSOIL;
                    break;
                case Enum_PlayerUseItemType.AXE:
                    UseItemType = Enum_PlayerUseItemType.HAND;
                    break;
                case Enum_PlayerUseItemType.PICKAXE:
                    UseItemType = Enum_PlayerUseItemType.AXE;
                    break;
                case Enum_PlayerUseItemType.SHOVEL:
                    UseItemType = Enum_PlayerUseItemType.PICKAXE;
                    break;
                case Enum_PlayerUseItemType.HOE:
                    UseItemType = Enum_PlayerUseItemType.SHOVEL;
                    break;
                case Enum_PlayerUseItemType.BLOCKSOIL:
                    UseItemType = Enum_PlayerUseItemType.HOE;
                    break;
            }

            Debug.Log("EEEEEEEEE" + UseItemType);
        }
        */
    }

    private void LateUpdate()
    {
        if (!pV.isMine)
        {
            return;
        }

        if (!UseItemType.Equals(Enum_PlayerUseItemType.BLUEPRINTWATCHFIRE) && !UseItemType.Equals(Enum_PlayerUseItemType.BLUEPRINTTENT) && !UseItemType.Equals(Enum_PlayerUseItemType.BLUEPRINTWORKBENCH)
           && isBuild)// && !isUiBlock)//빌드모드 끝
        {
            if (bluePrint != null)
            {
                bluePrint.GetComponent<IPreViewBase>().HiedPreView();//빌딩 미리보기 제거
            }
            isBuild = false;
        }
        else if (!UseItemType.Equals(Enum_PlayerUseItemType.BLUEPRINTCHAIR) && !UseItemType.Equals(Enum_PlayerUseItemType.BLUEPRINTTABLE)
            && isCreateFurniture)//&& !isUiBlock)//빌드모드 끝
        {
            if (bluePrint != null)
            {
                bluePrint.GetComponent<IPreViewBase>().HiedPreView();//빌딩 미리보기 제거
            }
            isCreateFurniture = false;
        }
    }

    public void CreateBluePrint(string objName, Vector3 pos)
    {
        pV.RPC("CreateBluePrintRPC", PhotonTargets.MasterClient, objName, pos);
        SelectSlot.Ins.nowUsingSlot.UpdateSlotCount(-1);
        PlayEffectSoundPhoton(transform.position, 9);
    }

    //[PunRPC]
    //public void CreateBluePrintRPC(string objName, Vector3 pos)
    //{
    //    PhotonNetwork.InstantiateSceneObject(objName, pos, Quaternion.identity, 0, null);
    //}

    public void SetPlayerHand(Enum_DropItemType type)
    {
        if (bluePrint != null)
        {
            isBuild = false;
            isCreateFurniture = false;
            bluePrint.GetComponent<csPreViewBase>().HiedPreView();
            bluePrint = null;
        }

        switch (type)
        {
            case Enum_DropItemType.FRUIT:
                break;
            case Enum_DropItemType.STON:
                break;
            case Enum_DropItemType.WOOD:
                break;
            case Enum_DropItemType.CARROT:
                break;
        }
    }
    public void SetPlayerUseUtem(Enum_PlayerUseItemType type)//인벤토리 슬룻에 들어있는걸 들었다고 친다
    {
        if (bluePrint != null)
        {
            isBuild = false;
            isCreateFurniture = false;
            bluePrint.GetComponent<csPreViewBase>().HiedPreView();
            bluePrint = null;            
        }

        switch (type)
        {
            case Enum_PlayerUseItemType.HAND:
                UseItemType = Enum_PlayerUseItemType.HAND;
                break;
            case Enum_PlayerUseItemType.HOE:
                UseItemType = Enum_PlayerUseItemType.HOE;
                break;
            case Enum_PlayerUseItemType.AXE:
                UseItemType = Enum_PlayerUseItemType.AXE;
                break;
            case Enum_PlayerUseItemType.PICKAXE:
                UseItemType = Enum_PlayerUseItemType.PICKAXE;
                break;
            case Enum_PlayerUseItemType.SHOVEL:
                UseItemType = Enum_PlayerUseItemType.SHOVEL;
                break;
            case Enum_PlayerUseItemType.BLOCKSOIL:
                UseItemType = Enum_PlayerUseItemType.BLOCKSOIL;
                break;
            default:
                UseItemType = Enum_PlayerUseItemType.HAND;
                break;
        }
    }

    public void SetBluePrintItme(Enum_PreViewType type)//인벤토리에 청사진이 들어있으면 어떤 청사진인지 알려준다
    {
        if ( bluePrint!=null)
        {
            bluePrint.GetComponent<IPreViewBase>().HiedPreView();//빌딩 미리보기 제거

            bluePrint = null;
            isCreateFurniture = false;
            isBuild = false;
        }


        switch (type)
        {
            case Enum_PreViewType.FIRE:
                UseItemType = Enum_PlayerUseItemType.BLUEPRINTWATCHFIRE;
                if (!inTheBuilding)
                {
                    bluePrint = bluePrintObj[0];
                    bluePrint.SetActive(true);
                    isBuild = true;//빌드모드 시작
                }
                break;
            case Enum_PreViewType.TENT:
                UseItemType = Enum_PlayerUseItemType.BLUEPRINTTENT;
                if (!inTheBuilding)
                {
                    bluePrint = bluePrintObj[1];
                    bluePrint.SetActive(true);
                    isBuild = true;//빌드모드 시작
                }
                break;
            case Enum_PreViewType.WORKBENCH:
                UseItemType = Enum_PlayerUseItemType.BLUEPRINTWORKBENCH;
                if (!inTheBuilding)
                {
                    bluePrint = bluePrintObj[4];
                    bluePrint.SetActive(true);
                    isBuild = true;//빌드모드 시작
                }
                break; 
            case Enum_PreViewType.HOUSE_CHAIR://하우징용
                UseItemType = Enum_PlayerUseItemType.BLUEPRINTCHAIR;
                if (inTheBuilding)
                {
                    bluePrint = bluePrintObj[2];
                    bluePrint.SetActive(true);
                    isCreateFurniture = true;//빌드모드 시작
                }
                break;
            case Enum_PreViewType.HOUSE_TABLE:
                UseItemType = Enum_PlayerUseItemType.BLUEPRINTTABLE;
                if (inTheBuilding)
                {
                    bluePrint = bluePrintObj[3];
                    bluePrint.SetActive(true);
                    isCreateFurniture = true;//빌드모드 시작
                }
                break;
        }

        //if (bluePrint != null)
        //{
        //    bluePrint.GetComponent<csPreViewBase>().ShowPreView();
        //}
    }

    public void SetObjDMG(Vector3 pos, float dmg, Enum_PlayerUseItemType ui)
    {
        pV.RPC("SetObjDMGRPC", PhotonTargets.AllBuffered, pos, dmg, ui);
    }

    //[PunRPC]
    //public void SetObjDMGRPC(Vector3 pos, float dmg, Enum_PlayerUseItemType ui)
    //{
    //    worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj.GetComponent<csCube>().StartAction(dmg, ui);
    //}

    IEnumerator PlayerUseItem(Enum_PlayerUseItemType type)//플레이어가 들고있는 장비에 따라 다른 액션
    {
        actionNow = false;

        switch (type)
        {
            case Enum_PlayerUseItemType.HAND://맨손
            case Enum_PlayerUseItemType.AXE://도끼
            case Enum_PlayerUseItemType.PICKAXE://곡괭이

                //Debug.Log("탄다");
                if (oldBlock != null)//흔들기
                {
                    //Debug.Log("hit");
                    //oldBlock.StartAction(1, UseItemType);
                    SetObjDMG(oldBlock.GetCubePos(), 1, UseItemType);
                    // Debug.Log("탄다2");
                }
                break;
            case Enum_PlayerUseItemType.HOE://괭이                
                ActionHOE();
                break;
            case Enum_PlayerUseItemType.SHOVEL://삽
                ActionSHOVEL();
                break;
            //case Enum_PlayerUseItemType.BLUEPRINT://청사진
            //    UseItemType = Enum_PlayerUseItemType.HAND;//여기도 바꿔야함#####
            //    bluePrint.GetComponent<IPreViewBase>().CreateBuilding();
            //    isBuild = false;
            //    break;
            case Enum_PlayerUseItemType.BLUEPRINTWATCHFIRE:
            case Enum_PlayerUseItemType.BLUEPRINTTENT:
            case Enum_PlayerUseItemType.BLUEPRINTCHAIR:
            case Enum_PlayerUseItemType.BLUEPRINTTABLE:
            case Enum_PlayerUseItemType.BLUEPRINTWORKBENCH:
                if (bluePrint != null)
                {
                    bluePrint.GetComponent<IPreViewBase>().CreateBuilding();                    
                }
                break;
                
            case Enum_PlayerUseItemType.BLOCKSOIL://흙 블럭
                ActionAddBlock(Enum_CubeType.SOIL);                
                break;
        }

        yield return new WaitForSeconds(0.1f);

        actionNow = true;
    }

    void ActionHOE()
    {
        if (Physics.Raycast(ray, out hit, rayCastRange, LayerMaskBlock))
        {
            if (hit.transform.tag != "Block")
            {
                return;
            }

            Vector3 blockPos = hit.transform.position;

            blockPos.y *= 2f;

            if (blockPos.y <= 1)
            {
                return;
            }

            if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].type.Equals(Enum_CubeType.WATER))//땅위에 뭐 있으면 탈출
            {
                return;
            }

            if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].haveChild)//땅위에 뭐 있으면 탈출
            {
                if (oldBlock != null)//흔들기
                {
                    //Debug.Log("hit");
                    //oldBlock.StartAction(1, UseItemType);
                    SetObjDMG(oldBlock.GetCubePos(), 1, UseItemType);
                    // Debug.Log("탄다2");
                }
                return;
            }

            if (!worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].top)//제일위에있는거 아니면 탈출
            {
                return;
            }

            //Debug.Log(123123123);

            //밭 설치
            pV.RPC("RPCActionHOE", PhotonTargets.AllBufferedViaServer, blockPos);            
        }
    }

    //[PunRPC]
    //public void RPCActionHOE(Vector3 blockPos)
    //{
    //    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj.GetComponent<csCube>().SetObj(Enum_CubeState.FIELD);
    //}

    void ActionAddBlock(Enum_CubeType type)
    {
        if (Physics.Raycast(ray, out hit, rayCastRange, LayerMaskBlock))
        {
            //Debug.Log(hit.transform.tag + "블럭생성중 1");

            if (hit.transform.tag != "Block")
            {
                return;
            }

            Vector3 blockPos = hit.transform.position;

            blockPos.y *= 2f;

            if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].haveChild)//땅위에 뭐 있으면 탈출
            {
                //Debug.Log(hit.transform.tag + "블럭 생성중 땅위에 뭐 있어서 리턴");
                return;
            }

            if (!worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].top)//제일위에있는거 아니면 탈출
            {
                //Debug.Log(hit.transform.tag + "블럭 생성중 탑 블럭이 아니라 리턴");
                return;
            }

            if (blockPos.y + 1 > mapData.height)
            {
                //Debug.Log(hit.transform.tag + "블럭 생성중 맵 최대 높이 벗어나서 리턴");
                return;
            }

            if (worldBlock[(int)blockPos.x, (int)blockPos.y + 1, (int)blockPos.z] == null)//바로 위가 빈 공간이면
            {
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].top = false;//지금 내위치는 탑이아님

                pV.RPC("CreateCube", PhotonTargets.AllBuffered, new Vector3(blockPos.x, blockPos.y+1, blockPos.z), type);//블록 생성
                SelectSlot.Ins.nowUsingSlot.UpdateSlotCount(-1);//블록 아이템 갯수 차감
            }
        }
    }

    //[PunRPC]
    //public void CreateCube(Vector3 blockPos, Enum_CubeType type)
    //{
    //    switch (type)
    //    {
    //        case Enum_CubeType.SOIL:
    //            {
    //                GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[3], new Vector3(blockPos.x,(blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
    //                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.SOIL, true, tmpObj,true, false,Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
    //                tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
    //                //m_nodeArr[(int)blockPos.x, (int)blockPos.z] = tmpObj.GetComponent<Node>();
    //            }
    //            break;
    //        case Enum_CubeType.WATER:
    //            {
    //                GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[5], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
    //                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.WATER, true, tmpObj, true, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
    //                tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
    //               // m_nodeArr[(int)blockPos.x, (int)blockPos.z] = tmpObj.GetComponent<Node>();

    //                int tmpY = (int)blockPos.y;
    //                while (tmpY > 0)
    //                {
    //                    if (worldBlock[(int)blockPos.x, tmpY, (int)blockPos.z] == null)
    //                    {
    //                        worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.WATER, false, tmpObj, false, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);

    //                        tmpY--;
    //                        //Debug.Log(blockPos);
    //                    }
    //                    else if (worldBlock[(int)blockPos.x, tmpY, (int)blockPos.z] != null)
    //                    {
    //                        break;
    //                    }
    //                }
    //            }
    //            break;
    //    }
    //}

    void ActionSHOVEL()
    {
        if (Physics.Raycast(ray, out hit, rayCastRange, LayerMaskBlock))
        {
            bool tmpCheck = false;

            if (hit.transform.tag != "Block")
            {
                if (hit.transform.root.tag != "Block")
                {
                    return;
                }
                else
                {
                    tmpCheck = true;
                }
            }

            Vector3 blockPos;

            if (tmpCheck)
            {
                blockPos = hit.transform.root.position;
            }
            else
            {
                blockPos = hit.transform.position;
            }

            blockPos.y *= 2f;

            if (blockPos.y <= 1)
            {
                return;
            }

            if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] == null)
            {
                return;
            }

            if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].type.Equals(Enum_CubeType.WATER))//물이면 탈출
            {
                return;
            }

            if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].haveChild)//땅위에 뭐 있으면 탈출
            {
                //Debug.Log("hit1");
                if (oldBlock != null)//흔들기
                {
                    //Debug.Log("hit2");
                    //oldBlock.StartAction(1, UseItemType);
                    SetObjDMG(oldBlock.GetCubePos(), 1, UseItemType);
                    // Debug.Log("탄다2");
                }
                return;
            }

            if (!worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].top)//제일위에있는거 아니면 탈출
            {
                return;
            }
            
            oldBlock = null;
            bool waterCheck = WaterCheck(blockPos);

            {
                //Destroy(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj);
                //worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = null;
                pV.RPC("ActionSHOVELRPC", PhotonTargets.AllBuffered, blockPos);
            }

            DropItemCreate("ITEM_Cube", blockPos, 1);

            //Debug.Log(waterCheck+"무슨일이 일어나는거지");
            if (!waterCheck && ((int)blockPos.y - 1) > 0)
            {
                if (worldBlock[(int)blockPos.x, (int)blockPos.y - 1, (int)blockPos.z] != null)
                {
                    worldBlock[(int)blockPos.x, (int)blockPos.y - 1, (int)blockPos.z].top = true;//지운거 바로 아랫칸 탑으로
                }
            }
            else if (waterCheck)
            {
                //Debug.Log(blockPos);
                CreateWaterAuto(blockPos);
            }

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        if ((!(x == 0 && y == 0 && z == 0)))
                        {
                            if (blockPos.x + x < 0 || blockPos.x > mapData.widthX)
                            {
                                continue;
                            }
                            if (blockPos.y + y < 0 || blockPos.y > mapData.height)
                            {
                                continue;
                            }
                            if (blockPos.z + z < 0 || blockPos.z > mapData.widthZ)
                            {
                                continue;
                            }

                            Vector3 tmpPos = new Vector3(blockPos.x + x, (blockPos.y + y), blockPos.z + z);

                            if (worldBlock[(int)tmpPos.x, (int)tmpPos.y, (int)tmpPos.z] != null)
                            {
                                pV.RPC("DrawBlock", PhotonTargets.AllBuffered, tmpPos);
                            }
                            // Debug.Log(tmpPos + "///" + hit.transform.position);
                        }
                    }
                }
            }
        }
    }

    //[PunRPC]
    //public void ActionSHOVELRPC(Vector3 blockPos)
    //{
    //    Destroy(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj);
    //    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = null;
    //}

    void CreateWaterAuto(Vector3 blockPos)//근처 빈 공간 있으면 자동으로 물로 채우고 
    {
        pV.RPC("CreateCube", PhotonTargets.AllBuffered, new Vector3(blockPos.x, blockPos.y, blockPos.z), Enum_CubeType.WATER);
        //GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[5], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
        //worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.WATER, true, tmpObj, true, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
        //tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);

        if (blockPos.x - 1 > 0 && worldBlock[(int)blockPos.x - 1, (int)blockPos.y, (int)blockPos.z] == null)
        {
            CreateWaterAuto(new Vector3((int)blockPos.x - 1, (int)blockPos.y, (int)blockPos.z));
        }

        if (blockPos.x + 1 < mapData.widthX && worldBlock[(int)blockPos.x + 1, (int)blockPos.y, (int)blockPos.z] == null)
        {
            CreateWaterAuto(new Vector3((int)blockPos.x + 1, (int)blockPos.y, (int)blockPos.z));
        }

        if (blockPos.z - 1 > 0 && worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z - 1] == null)
        {
            CreateWaterAuto(new Vector3((int)blockPos.x, (int)blockPos.y, (int)blockPos.z - 1));
        }

        if (blockPos.z + 1 < mapData.widthZ && worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z + 1] == null)
        {
            CreateWaterAuto(new Vector3((int)blockPos.x, (int)blockPos.y, (int)blockPos.z + 1));
        }
        
    }

    bool WaterCheck(Vector3 pos)
    {
        if (pos.x - 1 > 0 && worldBlock[(int)pos.x - 1, (int)pos.y, (int)pos.z] != null && worldBlock[(int)pos.x - 1, (int)pos.y, (int)pos.z].type.Equals(Enum_CubeType.WATER))
        {
            //Debug.Log(worldBlock[(int)pos.x-1, (int)pos.y, (int)pos.z ].type);
            return true;
        }
        else if (pos.x + 1 < mapData.widthX && worldBlock[(int)pos.x + 1, (int)pos.y, (int)pos.z] != null && worldBlock[(int)pos.x + 1, (int)pos.y, (int)pos.z].type.Equals(Enum_CubeType.WATER))
        {
            //Debug.Log(worldBlock[(int)pos.x+1, (int)pos.y, (int)pos.z ].type);
            return true;
        }
        else if (pos.z - 1 > 0 && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z - 1] != null && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z - 1].type.Equals(Enum_CubeType.WATER))
        {
            //Debug.Log(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z - 1].type);
            return true;
        }
        else if (pos.z + 1 < mapData.widthZ && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z + 1] != null && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z + 1].type.Equals(Enum_CubeType.WATER))
        {
            //Debug.Log(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z + 1].type);
            return true;
        }
        //Debug.Log(55555555);
        return false;
    }

    //[PunRPC]
    //public void DrawBlock(Vector3 blockPos)//블록 그리는 함수
    //{
    //    if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] == null)
    //    {
    //        //Debug.Log(1);
    //        return;
    //    }


    //    if (!worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].vis)
    //    {
    //        worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].vis = true;

    //        GameObject tmpObj = null;

    //        bool tmpTop = false;

    //        if (worldBlock[(int)blockPos.x, (int)blockPos.y + 1, (int)blockPos.z] == null)
    //        {
    //            tmpTop = true;
    //        }

    //        switch (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].type)
    //        {
    //            case Enum_CubeType.DARKSOIL:
    //                tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[0], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
    //                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.DARKSOIL, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
    //                tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
    //                break;
    //            case Enum_CubeType.STON:
    //                tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[1], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
    //                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.STON, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
    //                tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
    //                break;
    //            case Enum_CubeType.GRASS:
    //                tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[2], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
    //                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.GRASS, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
    //                tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
    //                break;
    //            case Enum_CubeType.SOIL:
    //                tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[3], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
    //                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.SOIL, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
    //                tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
    //                break;
    //            case Enum_CubeType.SEND:
    //                tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[4], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
    //                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.SEND, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
    //                tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
    //                break;
    //            case Enum_CubeType.WATER:
    //                tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[5], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
    //                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.WATER, true, tmpObj, tmpTop, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
    //                tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
    //                break;
    //        }
    //        //newBlock.transform.SetParent(map);
    //        //tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);

    //        //if (tmpObj != null && worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj == null)
    //        //{
    //            //worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj = tmpObj;

    //            //if (worldBlock[(int)blockPos.x, (int)blockPos.y + 1, (int)blockPos.z] == null)
    //            //{
    //            //    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].top = true;
    //            //}

    //        //    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
    //        //}

    //        //if (worldBlock[(int)blockPos.x, (int)blockPos.y+1, (int)blockPos.z] ==null)
    //        //{
    //        //    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].top = true;
    //        //    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj.GetComponent<csCube>().AddNode();
    //        //    //for(int i = 0; i < 64; i++)
    //        //    //{
    //        //    //    for(int j = 0; j < 64; j++)
    //        //    //    {
    //        //    //        if (m_nodeArr[i, j] != null)
    //        //    //        {
    //        //    //            Debug.Log("m_nodeArr [" + i+","+j+"] >> "+m_nodeArr[i, j].m_nodeType);
    //        //    //        }
    //        //    //        else
    //        //    //        {
    //        //    //            Debug.Log("m_nodeArr [" + i + "," + j + "] >> NULL");
    //        //    //        }
    //        //    //    }
    //        //    //}
    //        //    //m_nodeArr[(int)blockPos.x, (int)blockPos.z] = tmpObj.GetComponent<Node>();
    //        //}
    //    }
    //}

    public void AddNode(Vector3 blockPos, Node n)
    {
        m_nodeArr[(int)blockPos.x, (int)blockPos.z] = n;
    }

    public void CreateDropItem(Vector3 pos, string objName)
    {
        pV.RPC("CreateDropItemRPC", PhotonTargets.MasterClient, pos, objName);
    }

    //[PunRPC]
    //public void CreateDropItemRPC(Vector3 pos, string str)
    //{
    //    //Debug.Log(dropItem.name);
    //    GameObject tmp = PhotonNetwork.InstantiateSceneObject(str, pos, Quaternion.identity, 0, null);
    //    tmp.GetComponent<Rigidbody>().AddForce(Vector3.up * Time.deltaTime * 6000f);
    //    tmp.transform.SetParent(null);
    //}

    public void DelChildObj(Vector3 pos)
    {
        pV.RPC("DelChildObjRPC", PhotonTargets.AllBuffered, pos);
    }

    //[PunRPC]
    //public void DelChildObjRPC(Vector3 pos)
    //{
    //    worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj.GetComponent<csCube>().DestroyChild();
    //}

    //a*       

    [Header("A* 관련")]
    public bool startPathFinding = false;
    [SerializeField]
    private List<Node> m_neighbours = new List<Node>();
    public Node[,] m_nodeArr;
    public Vector3 targetPos;

    public bool CheckNode(int row, int col)//x,z
    {
        if (row < 0 || row >= mapData.widthX)
        {
            return false;
        }
        if (col < 0 || col >= mapData.widthZ)
        {
            return false;
        }

        return true;
    }

    public Node[] Neighbours(Vector3 pos)
    {
        return Neighbours(m_nodeArr[(int)pos.x, (int)pos.z]);
    }

    public Node[] Neighbours(Node node)
    {
        m_neighbours.Clear();

        if (node == null)
        {
            return null;
        }

        if (CheckNode(node.Row - 1, node.Col - 1))
        {
            m_neighbours.Add(m_nodeArr[node.Row - 1, node.Col - 1]);
        }
        if (CheckNode(node.Row - 1, node.Col))
        {
            m_neighbours.Add(m_nodeArr[node.Row - 1, node.Col]);
        }
        if (CheckNode(node.Row - 1, node.Col + 1))
        {
            m_neighbours.Add(m_nodeArr[node.Row - 1, node.Col + 1]);
        }
        if (CheckNode(node.Row, node.Col - 1))
        {
            m_neighbours.Add(m_nodeArr[node.Row, node.Col - 1]);
        }
        if (CheckNode(node.Row, node.Col + 1))
        {
            m_neighbours.Add(m_nodeArr[node.Row, node.Col + 1]);
        }
        if (CheckNode(node.Row + 1, node.Col - 1))
        {
            m_neighbours.Add(m_nodeArr[node.Row + 1, node.Col - 1]);
        }
        if (CheckNode(node.Row + 1, node.Col))
        {
            m_neighbours.Add(m_nodeArr[node.Row + 1, node.Col]);
        }
        if (CheckNode(node.Row + 1, node.Col + 1))
        {
            m_neighbours.Add(m_nodeArr[node.Row + 1, node.Col + 1]);
        }

        return m_neighbours.ToArray();
    }

    public Node FindNode(Vector3 pos)
    {
        return m_nodeArr[(int)pos.x, (int)pos.z];
    }

    public void ResetNode()
    {
        for (int row = 0; row < mapData.widthX; ++row)
        {
            for (int col = 0; col < mapData.widthZ; ++col)
            {
                m_nodeArr[row, col].Reset();
            }
        }
    }

    public void StartSetNode()
    {
        for (int row = 0; row < mapData.widthX; ++row)
        {
            for (int col = 0; col < mapData.widthZ; ++col)
            {
                if (m_nodeArr[row, col].m_nodeType.Equals(NodeType.None))
                {
                    m_nodeArr[row, col].Reset();
                }
            }
        }
    }


    //채팅기능  

    


    public bool isOM = false;
    
    public void OnClickOculusBtn()
    {
        if (isOM)
        {
            isOM = false;
            myPlyerCtrl.SetOulusMode(isOM);
        }
        else
        {
            isOM = true;
            myPlyerCtrl.SetOulusMode(isOM);
        }
    }

    public GameObject smile;
    public void OnClickSmileBtn()
    {
        if (pV.isMine)
        {            
            pV.RPC("StartSmile", PhotonTargets.All, null);
        }
    }

    public void OnClickExitBtn()
    {
        //OptionManager.Ins.PlayClickSound();

        string msg = "\n\t<color=#999999>[" + PhotonNetwork.player.NickName + "] Disconnected</color>";

        //RPC 함수 호출
        GameObject.FindGameObjectWithTag("SceneOwner").GetComponent<csSceneOwner>().LogMsgAll(msg);

        StopAllCoroutines();
        CancelInvoke();

        //마스터가 나가면 방폭
        if (PhotonNetwork.isMasterClient)
        {
            //Debug.Log("asdasdaaa");
            pV.RPC("DestroyRoomRPC", PhotonTargets.All, null);
        }
        else
        {
            tPlayer.SaveInvenData();
            tutorialCanvas.SetActive(true);
            Invoke("DestroyRoom", 4f);
        }        
    }

    public void DestroyRoom()
    {
        PhotonNetwork.LeaveRoom(true);
    }

    [Header("디버그 관련")]
    public GameObject debugBtn;
    public bool isDebugMode = false;

    public void OnClickDebugBtn()
    {
       // GameObject.FindGameObjectWithTag("SceneOwner").GetComponent<csSceneOwner>().NextDay();
        pV.RPC("NextDayRPC", PhotonTargets.AllBuffered, null);
    }

    public void NextDay()
    {
        pV.RPC("NextDayRPC", PhotonTargets.AllBuffered, null);
    }
}
