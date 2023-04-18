using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
//https://www.youtube.com/watch?v=-cKiC0huc_w&ab_channel=RamJack

using TeamInterface;

using System;

public class csLevelManager : csGenericSingleton<csLevelManager>
{
    [Header("블록")]
    public GameObject[] cube;

    [Header("맵정보")]
    public Transform map;//만든큐브 저장할 오브젝트
    static public int widthX = 64;
    static public int widthZ = 64;
    static public int height = 64;
    public float waveLength = 0;// 파장
    public float amplitude = 0;//진폭 최대높이

    [Header("블록 정보 3차원으로 저장")]
    public Block[,,] worldBlock = new Block[widthX, height, widthZ];

    [Header("필드 오브젝트")]
    public GameObject[] fieldObj;
    public GameObject field;

    IHighlighter oldBlock;

    [HideInInspector]
    bool isBuild;

    [HideInInspector]
    public Inventory tPlayer;


    [Header("대충 플레이어가 들고있을 변수")]
    public Enum_PlayerUseItemType UseItemType;
    public GameObject bluePrint;
    bool actionNow=true;//지금 뭐 동작중인지 체크

    [Header("날씨 스크립트 가져오기")]
    public csDayCtrl dayCtrl;

    [Header("분마다 시간체크용")]
    [SerializeField]
    string oldTime;

    protected override void Awake()
    {    
        base.Awake();

        dayCtrl = GameObject.FindGameObjectWithTag("SkyDome").GetComponent<csDayCtrl>();
        //tPlayer = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
    }

    private void Start()
    {
       
        Debug.Assert(dayCtrl);
        //Debug.Assert(tPlayer);

        StartCoroutine(InitGame());
        //for(int x = 0; x < widthX; x++)
        //{
        //    for (int z = 0; z < widthZ; z++)
        //    {
        //        blockList.Add(Instantiate(cube[0], new Vector3(x, 0, z), Quaternion.identity));
        //    }
        //}

        //foreach(GameObject t in blockList)
        //{
        //    t.transform.parent = map;
        //}

        //for(int i=0;i< blockList.Count; i++)
        //{
        //    float x = (blockList[i].transform.position.x) / waveLength;
        //    float z = (blockList[i].transform.position.z) / waveLength;
        //    int y = (int)((Mathf.PerlinNoise(x, z) * amplitude));
        //    blockList[i].transform.position = new Vector3(blockList[i].transform.position.x, y*0.3f, blockList[i].transform.position.z);
        //}

        //UseItemType = Enum_PlayerUseItemType.HAND;

        InvokeRepeating("GrowthTimeCheck", 0f, 0.2f);

       
    }

    IEnumerator InitGame()
    {
        //맵만들기
        yield return StartCoroutine(MapInit());
    }

    private float groundHeightOffset = 20f;
    //private float seed=0f;
    IEnumerator MapInit()
    {
        for (int x = 0; x < widthX; x++)
        {
            for (int z = 0; z < widthZ; z++)
            {
                float xb = (x + 0) / waveLength;
                float zb = (z + 0) / waveLength;
                int y = (int)((Mathf.PerlinNoise(xb, zb) * amplitude) * amplitude + groundHeightOffset);
                Vector3 pos = new Vector3(x, y, z);

                StartCoroutine(CreateBlock(y, pos, true));

                //if (y + 1 == 27)
                //{
                //    if (worldBlock[(int)pos.x, (int)pos.y + (y + 1), (int)pos.z] == null)
                //    {
                //        //Debug.Log(111);
                //        GameObject BlockObj = (GameObject)Instantiate(cube[5], new Vector3(pos.x, (pos.y + (y + 1)) * 0.5f, pos.z), Quaternion.identity);
                //        worldBlock[(int)pos.x, (int)pos.y + (y + 1), (int)pos.z] = new Block(Enum_CubeType.WATER, true, BlockObj, true);
                //        //BlockObj.transform.SetParent(map);
                //        BlockObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y + (y + 1), (int)pos.z]);
                //    }

                //    int tmpi = 1;
                //    while (y - tmpi >= 24)
                //    {
                //        if (worldBlock[(int)pos.x, (int)pos.y - tmpi, (int)pos.z] == null)
                //        {
                //            worldBlock[(int)pos.x, (int)pos.y - tmpi, (int)pos.z] = new Block(Enum_CubeType.WATER, false, null, false);
                //        }
                //        tmpi++;
                //    }
                //}

                if (y + 1 <= 27 && y + 1 >= 24)
                {
                    for (int i=y+1; i<=27;i++)
                    {
                        if (worldBlock[(int)pos.x, (int)pos.y + (i-y), (int)pos.z] == null && i==27)
                        {
                            //Debug.Log(111);
                            GameObject BlockObj = (GameObject)Instantiate(cube[5], new Vector3(pos.x, (pos.y + (i - y)) * 0.5f, pos.z), Quaternion.identity);
                            worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z] = new Block(Enum_CubeType.WATER, true, BlockObj, true);
                            //BlockObj.transform.SetParent(map);
                            BlockObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z]);
                        }
                        else if (worldBlock[(int)pos.x, (int)pos.y + (i - y), (int)pos.z] == null)
                        {
                            worldBlock[(int)pos.x, (int)pos.y +(i-y), (int)pos.z] = new Block(Enum_CubeType.WATER, false, null, false);
                        }
                    }
                    //Debug.Log(1111);                    
                }

                while (y > 0)
                {
                    y--;
                    pos = new Vector3(x, y, z);
                    StartCoroutine(CreateBlock(y, pos, false));
                }
            }
        }

        yield return null;
    }

    IEnumerator CreateBlock(float y, Vector3 pos, bool v)
    {
        if (y > 28)
        {
            if (v)
            {
                GameObject BlockObj = (GameObject)Instantiate(cube[2], new Vector3(pos.x, pos.y*0.5f, pos.z), Quaternion.identity);
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.GRASS, v, BlockObj, true);
                //BlockObj.transform.SetParent(map);
                BlockObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z]);
                BlockObj.GetComponent<csCube>().SetObj();
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
                GameObject BlockObj = (GameObject)Instantiate(cube[3], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.SOIL, v, BlockObj, true);
                //BlockObj.transform.SetParent(map);
                BlockObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z]);
                BlockObj.GetComponent<csCube>().SetObj();
            }
            else
            {
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.SOIL, v, null, false);
            }
        }
        else if (y > 0)
        {
            if (v)
            {
                GameObject BlockObj = (GameObject)Instantiate(cube[1], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.STON, v, BlockObj, true);
                //BlockObj.transform.SetParent(map);
                BlockObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z]);
                BlockObj.GetComponent<csCube>().SetObj();
            }
            else
            {
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.STON, v, null, false);
            }
        }
        else if (y == 0)
        {
            if (v)
            {
                GameObject BlockObj = (GameObject)Instantiate(cube[1], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.STON, v, BlockObj, true);
                //BlockObj.transform.SetParent(map);
                BlockObj.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z]);
                BlockObj.GetComponent<csCube>().SetObj();
            }
            else
            {
                //GameObject BlockObj = (GameObject)Instantiate(cube[5], pos, Quaternion.identity);
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(Enum_CubeType.STON, v, null, false);
            }
        }        

        //땅에 뭔가 심을때는 그자리에 그려져있으면지우고 보이는상태면 그림
        //if(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj != null) 
        //{
        //    Destroy(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj);
        //}

        yield return null;
    }

    private void Update()
    {
        RaycastHit hit;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (!isBuild && Physics.Raycast(ray, out hit, 1000f))
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
            if (Physics.Raycast(ray, out hit, 300f))
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
                            if (blockPos.x + x < 0 || blockPos.x > widthX)
                            {
                                tmpCheck = false;
                                continue;
                            }
                            if (blockPos.z + z < 0 || blockPos.z > widthZ)
                            {
                                tmpCheck = false;
                                continue;
                            }

                            if (worldBlock[(int)(blockPos.x + x), (int)(blockPos.y * 2f), (int)(blockPos.z + z)] == null)
                            {
                                tmpCheck = false;
                            }
                            else if(worldBlock[(int)(blockPos.x + x), (int)(blockPos.y * 2f), (int)(blockPos.z + z)] != null)
                            {
                                if(worldBlock[(int)(blockPos.x + x), (int)(blockPos.y * 2f), (int)(blockPos.z + z)].type.Equals(Enum_CubeType.WATER))
                                {
                                    tmpCheck = false;
                                }
                            }
                        }
                    }

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
                            if (blockPos.x + x < 0 || blockPos.x > widthX)
                            {
                                tmpCheck = false;
                                continue;
                            }
                            if (blockPos.z + z < 0 || blockPos.z > widthZ)
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

        if(actionNow && Input.GetMouseButtonDown(1))
        {
            StartCoroutine(PlayerUseItem(UseItemType));
        }

        /*
        //0411          
         
        if (Input.GetKeyDown(KeyCode.Q))//상호작용 뭔가 흔들기/공격
        {
            if (oldBlock != null)
            {
                //Debug.Log("hit");
                oldBlock.StartAction(1, UseItemType);
            }
        }

        if (isBuild && Input.GetMouseButtonDown(1))
        {
            bluePrint.GetComponent<IPreViewBase>().CreateBuilding();
            isBuild = false;
        }        
        else if (!isBuild && Input.GetMouseButtonDown(1))//우클릭시
        {
            //Debug.Log(isBuild);

            if (Physics.Raycast(ray, out hit, 1000f))
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

                if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].haveChild)//땅위에 뭐 있으면 탈출
                {
                    return;
                }

                if (!worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].top)//제일위에있는거 아니면 탈출
                {
                    return;
                }

                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = null;
                oldBlock = null;

                //Debug.Log(hit.collider.gameObject.GetComponent<csCube>().CubeInfo.vis);
                Destroy(hit.collider.gameObject);

                if (((int)blockPos.y - 1) > 0)
                {
                    worldBlock[(int)blockPos.x, (int)blockPos.y - 1, (int)blockPos.z].top = true;//지운거 바로 아랫칸 탑으로
                }

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            if ((!(x == 0 && y == 0 && z == 0)))
                            {
                                if (blockPos.x + x < 0 || blockPos.x > widthX)
                                {
                                    continue;
                                }
                                if (blockPos.y + y < 0 || blockPos.y > height)
                                {
                                    continue;
                                }
                                if (blockPos.z + z < 0 || blockPos.z > widthZ)
                                {
                                    continue;
                                }

                                Vector3 tmpPos = new Vector3(blockPos.x + x, (blockPos.y + y), blockPos.z + z);
                                DrawBlock(tmpPos);

                                // Debug.Log(tmpPos + "///" + hit.transform.position);
                            }
                        }
                    }
                }
            }
        }
        */

        /*//0410
        //if (Input.GetMouseButtonDown(1))//우클릭시
        //{
        //    if (Physics.Raycast(ray, out hit, 1000f))
        //    {
        //        Vector3 blockPos = hit.transform.position;

        //        blockPos.y *= 3.333333f;

        //        if (blockPos.y <= 1)
        //        {
        //            return;
        //        }

        //        worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = null;
        //        oldBlock = null;
        //        Destroy(hit.collider.gameObject);

        //        for (int x = -1; x <= 1; x++)
        //        {
        //            for (int y = -1; y <= 1; y++)
        //            {
        //                for (int z = -1; z <= 1; z++)
        //                {
        //                    if ((!(x == 0 && y == 0 && z == 0)))
        //                    {
        //                        if (blockPos.x + x < 0 || blockPos.x > widthX)
        //                        {
        //                            continue;
        //                        }
        //                        if (blockPos.y + y < 0 || blockPos.y > height)
        //                        {
        //                            continue;
        //                        }
        //                        if (blockPos.z + z < 0 || blockPos.z > widthZ)
        //                        {
        //                            continue;
        //                        }


        //                        Vector3 tmpPos = new Vector3(blockPos.x + x, (blockPos.y + y), blockPos.z + z);
        //                        DrawBlock(tmpPos);

        //                        // Debug.Log(tmpPos + "///" + hit.transform.position);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        */
    }

    IEnumerator PlayerUseItem(Enum_PlayerUseItemType type)
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
                    oldBlock.StartAction(1, UseItemType);
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
        RaycastHit hit;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out hit, 2.5f))
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
                    oldBlock.StartAction(1, UseItemType);
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
            worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj.GetComponent<csCube>().SetObj(Enum_CubeState.FIELD);
        }
    }

    void ActionAddBlock(Enum_CubeType type)
    {
        RaycastHit hit;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out hit, 1000f))
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

            if (blockPos.y+1 > height)
            {
                return;
            }

            if (worldBlock[(int)blockPos.x, (int)blockPos.y + 1, (int)blockPos.z] == null)
            {
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].top = false;

                switch (type) {
                    case Enum_CubeType.SOIL:
                        {
                            GameObject BlockObj = (GameObject)Instantiate(cube[3], new Vector3(blockPos.x, (blockPos.y + 1) * 0.5f, blockPos.z), Quaternion.identity);
                            worldBlock[(int)blockPos.x, (int)blockPos.y + 1, (int)blockPos.z] = new Block(Enum_CubeType.SOIL, true, BlockObj, true);
                            //BlockObj.transform.SetParent(map);
                            BlockObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y + 1, (int)blockPos.z]);
                        }
                        break;
                    case Enum_CubeType.WATER:
                        {
                            GameObject BlockObj = (GameObject)Instantiate(cube[5], new Vector3(blockPos.x, (blockPos.y + 1) * 0.5f, blockPos.z), Quaternion.identity);
                            worldBlock[(int)blockPos.x, (int)blockPos.y + 1, (int)blockPos.z] = new Block(Enum_CubeType.WATER, true, BlockObj, true);
                            //BlockObj.transform.SetParent(map);
                            BlockObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y + 1, (int)blockPos.z]);
                        }
                        break;
                }
            }
        }
    }

    void ActionSHOVEL()
    {
        RaycastHit hit;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out hit, 1000f))
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
                return;
            }

            if (!worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].top)//제일위에있는거 아니면 탈출
            {
                return;
            }

            worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = null;
            oldBlock = null;

            //Debug.Log(hit.collider.gameObject.GetComponent<csCube>().CubeInfo.vis);
            Destroy(hit.collider.gameObject);

            bool waterCheck = WaterCheck(blockPos);

            if (!waterCheck && ((int)blockPos.y - 1) > 0)
            {
                worldBlock[(int)blockPos.x, (int)blockPos.y - 1, (int)blockPos.z].top = true;//지운거 바로 아랫칸 탑으로
            }
            else if(waterCheck)
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
                            if (blockPos.x + x < 0 || blockPos.x > widthX)
                            {
                                continue;
                            }
                            if (blockPos.y + y < 0 || blockPos.y > height)
                            {
                                continue;
                            }
                            if (blockPos.z + z < 0 || blockPos.z > widthZ)
                            {
                                continue;
                            }

                            Vector3 tmpPos = new Vector3(blockPos.x + x, (blockPos.y + y), blockPos.z + z);
                            DrawBlock(tmpPos);

                            // Debug.Log(tmpPos + "///" + hit.transform.position);
                        }
                    }
                }
            }
        }
    }

    void CreateWaterAuto(Vector3 blockPos)//근처 빈 공간 있으면 자동으로 물로 채우고 
    {
        GameObject BlockObj = (GameObject)Instantiate(cube[5], new Vector3(blockPos.x, (blockPos.y) * 0.5f, blockPos.z), Quaternion.identity);
        worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Block(Enum_CubeType.WATER, true, BlockObj, true);
        //BlockObj.transform.SetParent(map);
        BlockObj.GetComponent<csCube>().SetCube(worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z]);

        if(worldBlock[(int)blockPos.x - 1, (int)blockPos.y, (int)blockPos.z] == null)
        {
            CreateWaterAuto(new Vector3((int)blockPos.x - 1, (int)blockPos.y, (int)blockPos.z));
        }

        if (worldBlock[(int)blockPos.x + 1, (int)blockPos.y, (int)blockPos.z] == null)
        {
            CreateWaterAuto(new Vector3((int)blockPos.x + 1, (int)blockPos.y, (int)blockPos.z));
        }

        if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z-1] == null)
        {
            CreateWaterAuto(new Vector3((int)blockPos.x , (int)blockPos.y, (int)blockPos.z-1));
        }

        if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z+1] == null)
        {
            CreateWaterAuto(new Vector3((int)blockPos.x , (int)blockPos.y, (int)blockPos.z+1));
        }

        int tmpY = (int)blockPos.y;
        while (tmpY > 0)
        {
            if (worldBlock[(int)blockPos.x, tmpY, (int)blockPos.z] == null)
            {
                worldBlock[(int)blockPos.x, tmpY, (int)blockPos.z] = new Block(Enum_CubeType.WATER, false, null, false);
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

        if (worldBlock[(int)pos.x - 1, (int)pos.y, (int)pos.z] != null && worldBlock[(int)pos.x - 1, (int)pos.y, (int)pos.z].type.Equals(Enum_CubeType.WATER))
        {
            //Debug.Log(1);
            return true;
        }
        else if (worldBlock[(int)pos.x + 1, (int)pos.y, (int)pos.z] != null && worldBlock[(int)pos.x + 1, (int)pos.y, (int)pos.z].type.Equals(Enum_CubeType.WATER))
        {
            //Debug.Log(2);
            return true;
        }
        else if (worldBlock[(int)pos.x, (int)pos.y, (int)pos.z - 1] != null && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z - 1].type.Equals(Enum_CubeType.WATER))
        {
            //Debug.Log(3);
            return true;
        }
        else if (worldBlock[(int)pos.x, (int)pos.y, (int)pos.z + 1] != null && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z + 1].type.Equals(Enum_CubeType.WATER))
        {
            //Debug.Log(4);
            return true;
        }

        return false;
    }

    void DrawBlock(Vector3 pos)//블록 그리는 함수
    {
        if (worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] == null)
        {
            //Debug.Log(1);
            return;
        }

        if (!worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].vis)
        {
            GameObject newBlock = null;

            worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].vis = true;

            switch (worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].type)
            {
                case Enum_CubeType.DARKSOIL:
                    newBlock = (GameObject)Instantiate(cube[0], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                    break;
                case Enum_CubeType.STON:
                    newBlock = (GameObject)Instantiate(cube[1], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                    break;
                case Enum_CubeType.GRASS:
                    newBlock = (GameObject)Instantiate(cube[2], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                    break;
                case Enum_CubeType.SOIL:
                    newBlock = (GameObject)Instantiate(cube[3], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                    break;
                case Enum_CubeType.SEND:
                    newBlock = (GameObject)Instantiate(cube[4], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                    break;
                case Enum_CubeType.WATER:
                    newBlock = (GameObject)Instantiate(cube[5], new Vector3(pos.x, pos.y * 0.5f, pos.z), Quaternion.identity);
                    break;
                default:
                    worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].vis = false;
                    break;
            }
            //newBlock.transform.SetParent(map);
            newBlock.GetComponent<csCube>().SetCube(worldBlock[(int)pos.x, (int)pos.y, (int)pos.z]);

            if (newBlock != null && worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj == null)
            {
                worldBlock[(int)pos.x, (int)pos.y, (int)pos.z].obj = newBlock;
            }
        }
    }

    void GrowthTimeCheck()//타이머
    {
        //return; 

        string date = DateTime.Now.ToString("yy.MM.dd ") + DateTime.Now.DayOfWeek.ToString().ToUpper().Substring(0, 3);
        //or date = DateTime.Now.ToString("yyyy. MM. dd. ddd");
        string time = DateTime.Now.ToString("HH:mm:ss");

        if (oldTime == null)
        {
            oldTime = DateTime.Now.ToString("HH:mm:ss");

        }

        if (!oldTime.Equals(time))
        {
            oldTime = time;
            dayCtrl.NextTime();
        }

        //ui text에 넣을 수 있음
        //text_date.text = date;
        //text_time.text = time;

        Debug.Log(string.Format("{0}\n{1}", date, time));
    }

    public void NextDay()
    {
        StartCoroutine(GoodMorning());
    }

    IEnumerator GoodMorning()//아침마다 무럭무럭 자라렴
    {
        //나무들아 아침이다~
        GameObject[] allTree = GameObject.FindGameObjectsWithTag("Tree");

        foreach(GameObject tree in allTree)
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
}
