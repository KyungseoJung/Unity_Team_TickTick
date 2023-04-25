using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon;
using Photon.Realtime;

using UnityEngine.UI;
//https://www.youtube.com/watch?v=-cKiC0huc_w&ab_channel=RamJack

using TeamInterface;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary; // System.Runtime.Serialization.Formatters.Binary 네임스페이스 추가

public class csPhotonGame : Photon.MonoBehaviour
{
    [Header("맵정보")]
    MapDataClass mapData = new MapDataClass();

    [Header("날씨 스크립트 가져오기")]
    public csDayCtrl dayCtrl;

    [Header("분마다 시간체크용")]
    public string oldTime;

    [Header("블록 정보 3차원으로 저장")]
    public Block[,,] worldBlock=null;
    IHighlighter oldBlock;

    [HideInInspector]
    bool isReady = false;
    bool isBuild=false;//애는 각자가지고있어야함 **
    bool inTheBuilding=false;//건물 안에 들어가면 활성화   

    [Header("대충 플레이어가 들고있을 변수")]
    [HideInInspector]
    public Inventory tPlayer;
    public Enum_PlayerUseItemType UseItemType;
    public GameObject bluePrint;
    bool actionNow = true;//지금 뭐 동작중인지 체크
    public float rayCastRange = 15f;

    [Header("레이 케스팅용")]
    [SerializeField]
    RaycastHit hit;
    [SerializeField]
    Ray ray;

    [Header("포톤 관련")]
    [SerializeField]
    PhotonView pV;

    [Header("스폰 관련")]
    public GameObject enemySpawn;
    public int enemySpawnCount;

    private void Awake()
    {
        pV = GetComponent<PhotonView>();

        //룸 프로퍼티 참조

        StartCoroutine(InitMapData());//클라이언트에 맵만들기 시작
    }

    IEnumerator Start()
    {
        while (!PhotonNetwork.connectedAndReady)
        {
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.1f); 

        if (PhotonNetwork.connectedAndReady && PhotonNetwork.isMasterClient)//방장일때탄다
        {
            dayCtrl = PhotonNetwork.InstantiateSceneObject("SkyDome", new Vector3(mapData.widthX * 2, mapData.height - 134f, mapData.widthZ * 2), Quaternion.identity, 0, null).GetComponent<csDayCtrl>();

            InvokeRepeating("GrowthTimeCheck", 0f, 0.2f);//타이머시작
        }         
    }

    IEnumerator InitMapData()
    {
        worldBlock = new Block[mapData.widthX, mapData.height, mapData.widthZ];
        List<Vector3> childVector = new List<Vector3>();

        for (int x = 0; x < mapData.widthX; x++)
        {
            for (int z = 0; z < mapData.widthZ; z++)
            {
                float xb = (x + 0) / mapData.waveLength;
                float zb = (z + 0) / mapData.waveLength;
                int y = (int)((Mathf.PerlinNoise(xb, zb) * mapData.amplitude) * mapData.amplitude + mapData.groundHeightOffset);
                Vector3 pos = new Vector3(x, y, z);

                CreateBlockData(y, pos, true);

                if (UnityEngine.Random.Range(0, 100) < 40)
                {
                    childVector.Add(pos);
                }

                if (y + 1 <= 27 && y + 1 >= 24)
                {
                    for (int i = y + 1; i <= 27; i++)
                    {
                        if (worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z] == null && i == 27)
                        {
                            GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[5], new Vector3(pos.x, (pos.y + (i - y)) * 0.5f, pos.z), Quaternion.identity);
                            worldBlock[x, y, z] = new Block(Enum_CubeType.WATER, true, tmpObj,true);
                            tmpObj.GetComponent<csCube>().SetCube(worldBlock[x, y, z]);
                        }
                        else if (worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z] == null)
                        {
                            worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z] = new Block(Enum_CubeType.WATER, false, null,false);
                        }
                    }
                    //Debug.Log(1111);                    
                }

                while (y > 0)
                {
                    y--;
                    pos = new Vector3(x, y, z);
                    CreateBlockData(y, pos, false);
                }
            }
        }

        SceneManager.LoadScene("MainGame_UI", LoadSceneMode.Additive);  //#3-3
        //SceneManager.LoadScene("addMain", LoadSceneMode.Additive);//애너미스폰포인트
        SceneManager.LoadScene("addPlayer", LoadSceneMode.Additive);//플레이어스폰포인트

        PhotonNetwork.isMessageQueueRunning = true;

        if (PhotonNetwork.isMasterClient)
        {
            foreach (Vector3 pos in childVector)
            {
                CreateBlockChild(pos);
            }
        }

        isReady = true;

        yield return PlayerSpawn();
    }

    IEnumerator PlayerSpawn()
    {
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("방장용 플레이어스폰");
            yield return EnemySpawn();
        }
        else
        {
            yield return null;
        }
    }

    IEnumerator EnemySpawn()
    {
        Debug.Log("애너미스폰");

        Transform[] enemySpawnPoint = enemySpawn.GetComponentsInChildren<Transform>();

        Debug.Log(enemySpawnPoint.Length);

        int maxEnemyPrefaps = csLevelManager.Ins.enemyPrefaps.Length;

        while (enemySpawnCount < 20)
        {
            PhotonNetwork.InstantiateSceneObject(csLevelManager.Ins.enemyPrefaps[UnityEngine.Random.Range(0, maxEnemyPrefaps)].name, enemySpawnPoint[UnityEngine.Random.Range(1, enemySpawnPoint.Length)].position, Quaternion.identity, 0, null);
            
            enemySpawnCount++;
            //Debug.Log(enemySpawnCount);
        }

        yield return null;
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
            pV.RPC("CreateBlockChildRPC", PhotonTargets.AllBuffered, pos, tmpCS, tmpNum);
        }       
    }

    [PunRPC]
    public void CreateBlockChildRPC(Vector3 pos, Enum_CubeState tmpCS, int tmpNum)
    {
        worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj.GetComponent<csCube>().SetObj(tmpCS, tmpNum);
    }

    void CreateBlockData(float y, Vector3 pos, bool v)
    {

        if (y > 28)
        {
            if (v)
            {
                GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[2], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.GRASS, v, tmpObj);
                tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z]);
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
            }
            else
            {
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.STON, v, null, false);
            }
        }
    }


    void GrowthTimeCheck()//타이머
    {
        pV.RPC("RPCGrowthTimeCheck", PhotonTargets.All, null);
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

        //Debug.Log(string.Format("{0}\n{1}", date, time));
    }

    public void NextDay()
    {
        pV.RPC("NextDayRPC", PhotonTargets.AllBuffered, null);
    }

    [PunRPC]
    public void NextDayRPC()
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

    private void Update()
    {
        if (!isReady)
        {
            return;
        }

        ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (!isBuild && Physics.Raycast(ray, out hit, rayCastRange))
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
            }
        }

        if (UseItemType.Equals(Enum_PlayerUseItemType.BLUEPRINT) && !isBuild)//청사진 들고있을때
        {
            isBuild = true;//빌드모드 시작
        }

        if (!UseItemType.Equals(Enum_PlayerUseItemType.BLUEPRINT) && isBuild)//빌드모드 끝
        {
            bluePrint.GetComponent<IPreViewBase>().HiedPreView();//빌딩 미리보기 제거
            isBuild = false;
        }

        if (isBuild)//빌드모드일 때 미리보기 그려주기
        {
            if (Physics.Raycast(ray, out hit, rayCastRange))
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
                    Debug.Log(tmpCheck);
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

        if (actionNow && Input.GetMouseButtonDown(1))
        {
            StartCoroutine(PlayerUseItem(UseItemType));
        }

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
                    UseItemType = Enum_PlayerUseItemType.BLUEPRINT;
                    break;
                case Enum_PlayerUseItemType.BLUEPRINT:
                    UseItemType = Enum_PlayerUseItemType.PLAYERWEAPONAXE1;
                    break;
                case Enum_PlayerUseItemType.PLAYERWEAPONAXE1:
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
                    UseItemType = Enum_PlayerUseItemType.PLAYERWEAPONAXE1;
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
                case Enum_PlayerUseItemType.BLUEPRINT:
                    UseItemType = Enum_PlayerUseItemType.BLOCKSOIL;
                    break;
                case Enum_PlayerUseItemType.PLAYERWEAPONAXE1:
                    UseItemType = Enum_PlayerUseItemType.BLUEPRINT;
                    break;
            }

            Debug.Log("EEEEEEEEE" + UseItemType);
        }
    }

    public void SetObjDMG(Vector3 pos, float dmg, Enum_PlayerUseItemType ui)
    {
        pV.RPC("SetObjDMGRPC", PhotonTargets.AllBuffered, pos, dmg, ui);
    }

    [PunRPC]
    void SetObjDMGRPC(Vector3 pos, float dmg, Enum_PlayerUseItemType ui)
    {
        worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj.GetComponent<csCube>().StartAction(dmg, ui);
    }

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
            case Enum_PlayerUseItemType.BLUEPRINT://청사진
                UseItemType = Enum_PlayerUseItemType.HAND;
                bluePrint.GetComponent<IPreViewBase>().CreateBuilding();
                isBuild = false;
                break;
            case Enum_PlayerUseItemType.BLOCKSOIL://흙 블럭
                ActionAddBlock(Enum_CubeType.SOIL);
                break;
            case Enum_PlayerUseItemType.BLOCKWATER://물 블럭
                ActionAddBlock(Enum_CubeType.WATER);
                break;
        }

        yield return new WaitForSeconds(0.1f);

        actionNow = true;
    }

    void ActionHOE()
    {
        if (Physics.Raycast(ray, out hit, rayCastRange))
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
            pV.RPC("RPCActionHOE", PhotonTargets.AllBuffered, blockPos);
        }
    }

    [PunRPC]
    public void RPCActionHOE(Vector3 blockPos)
    {
        worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj.GetComponent<csCube>().SetObj(Enum_CubeState.FIELD);
    }

    void ActionAddBlock(Enum_CubeType type)
    {
        if (Physics.Raycast(ray, out hit, rayCastRange))
        {
            if (hit.transform.tag != "Block")
            {
                return;
            }

            Vector3 blockPos = hit.transform.position;

            blockPos.y *= 2f;

            if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].haveChild)//땅위에 뭐 있으면 탈출
            {
                return;
            }

            if (!worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].top)//제일위에있는거 아니면 탈출
            {
                return;
            }

            if (blockPos.y + 1 > mapData.height)
            {
                return;
            }

            if (worldBlock[(int)blockPos.x, (int)blockPos.y + 1, (int)blockPos.z] == null)
            {
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].top = false;

                pV.RPC("CreateCube", PhotonTargets.AllBuffered, new Vector3(blockPos.x, blockPos.y+1, blockPos.z), type);
            }
        }
    }

    [PunRPC]
    public void CreateCube(Vector3 blockPos, Enum_CubeType type)
    {
        switch (type)
        {
            case Enum_CubeType.SOIL:
                {
                    GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[3], new Vector3(blockPos.x,(blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.SOIL, true, tmpObj,true, false,Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                }
                break;
            case Enum_CubeType.WATER:
                {
                    GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[5], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.WATER, true, tmpObj, true, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                }
                break;
        }
    }

    void ActionSHOVEL()
    {
        if (Physics.Raycast(ray, out hit, rayCastRange))
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

            pV.RPC("ActionSHOVELRPC", PhotonTargets.All, blockPos);            

            bool waterCheck = WaterCheck(blockPos);

            if (!waterCheck && ((int)blockPos.y - 1) > 0)
            {
                worldBlock[(int)blockPos.x, (int)blockPos.y - 1, (int)blockPos.z].top = true;//지운거 바로 아랫칸 탑으로
            }
            else if (waterCheck)
            {
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

    [PunRPC]
    public void ActionSHOVELRPC(Vector3 blockPos)
    {
        Destroy(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj);
        worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = null;
    }

    void CreateWaterAuto(Vector3 blockPos)//근처 빈 공간 있으면 자동으로 물로 채우고 
    {
        GameObject tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[5], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
        worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.WATER, true, tmpObj, true, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
        tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);

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

        int tmpY = (int)blockPos.y;
        while (tmpY > 0)
        {
            if (worldBlock[(int)blockPos.x, tmpY, (int)blockPos.z] == null)
            {
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.WATER,false, tmpObj, false, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);

                tmpY--;
                //Debug.Log(blockPos);
            }
            else if (worldBlock[(int)blockPos.x, tmpY, (int)blockPos.z] != null)
            {
                break;
            }
        }
    }

    bool WaterCheck(Vector3 pos)
    {
        //worldBlock[(int)pos.x, (int)pos.y +(i-y), (int)pos.z] = new Block(Enum_CubeType.WATER, false, null, false);

        if (pos.x - 1 > 0 && worldBlock[(int)pos.x - 1, (int)pos.y, (int)pos.z] != null && worldBlock[(int)pos.x - 1, (int)pos.y, (int)pos.z].type.Equals(Enum_CubeType.WATER))
        {
            //Debug.Log(1);
            return true;
        }
        else if (pos.x + 1 < mapData.widthX && worldBlock[(int)pos.x + 1, (int)pos.y, (int)pos.z] != null && worldBlock[(int)pos.x + 1, (int)pos.y, (int)pos.z].type.Equals(Enum_CubeType.WATER))
        {
            //Debug.Log(2);
            return true;
        }
        else if (pos.z - 1 > 0 && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z - 1] != null && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z - 1].type.Equals(Enum_CubeType.WATER))
        {
            //Debug.Log(3);
            return true;
        }
        else if (pos.z + 1 < mapData.widthZ && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z + 1] != null && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z + 1].type.Equals(Enum_CubeType.WATER))
        {
            //Debug.Log(4);
            return true;
        }

        return false;
    }

    [PunRPC]
    void DrawBlock(Vector3 blockPos)//블록 그리는 함수
    {
        if (!worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].vis)
        {
            worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].vis = true;

            GameObject tmpObj = null;

            switch (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].type)
            {
                case Enum_CubeType.DARKSOIL:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[0], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.DARKSOIL, true, tmpObj, true, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
                case Enum_CubeType.STON:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[1], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.STON, true, tmpObj, true, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
                case Enum_CubeType.GRASS:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[2], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.GRASS, true, tmpObj, true, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
                case Enum_CubeType.SOIL:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[3], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.SOIL, true, tmpObj, true, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
                case Enum_CubeType.SEND:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[4], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.SEND, true, tmpObj, true, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
                case Enum_CubeType.WATER:
                    tmpObj = (GameObject)Instantiate(csLevelManager.Ins.cube[5], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
                    worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.WATER, true, tmpObj, true, false, Enum_CubeState.NONE, 0, Enum_ObjectGrowthLevel.ZERO);
                    tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);
                    break;
            }
            //newBlock.transform.SetParent(map);
            tmpObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);

            if (tmpObj != null && worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj == null)
            {
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj = tmpObj; 
            }
        }
    }
}
