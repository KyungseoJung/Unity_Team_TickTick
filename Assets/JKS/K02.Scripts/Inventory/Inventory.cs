using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using System;   //#2-3 정렬 기능 Array.Sort()함수

using TeamInterface;

public class Inventory : MonoBehaviour, IInventoryBase
{
    public static bool inventoryOpen = false;    //인벤토리 활성화 여부
                                                    // 활성화(true) 인 경우, 다른 부분 세팅 (예 : 카메라 움직임 금지)
    private bool AscSort = true;    // 오름차순(AscSort), 내림차순(DescSort)
    public bool alreadyAsc = false;    // 이미 오름차순 버튼 눌렀을 때, 반복해서 함수 실행되지 않도록  
                                       //Slot에서도 접근할 거임(드래그해서 위치 움직인 후에는 false)
    public bool alreadyDesc = false;   // 이미 내림차순 버튼 눌렀을 때, 반복해서 함수 실행되지 않도록
    public int sortTimes = 0;          //#9-2 정렬한 횟수가 2번이상이 되면, 더이상 이전 값을 저장하지 않도록

    [SerializeField]
    private GameObject ImgInventory;
    [SerializeField]
    private GameObject SortingButtonsObj;   //#4-3 정렬 버튼 모음 오브젝트 (Sorting 오브젝트)
    [SerializeField]
    private GameObject DropItemZone;   //#4-3 아이템 드랍 존
    [SerializeField]
    private GameObject gridInventory;

    [SerializeField]
    private Slot[] slots;   // 캔버스에 보이는 실제 슬롯 배열로 모두 가져오기 - 데이터 저장하는 건 - itemInventory, itemInventoryCount 배열에!
    private Slot[] previousSlots;
    // [SerializeField]
    // private Slot[] sortSlots;       //#9-2 정렬할 슬롯들
    [SerializeField]
    private GameObject toolTip;         // 툴팁 창 (활성화/ 비활성화)
    [SerializeField]
    private GameObject destructionOpt;  // 슬롯 조정 버튼 창 (활성화/ 비활성화)

    [Header("테스트 목적 아이템 추가")]
    [Space(10)]
    public GameObject objFruit;
    public GameObject objSton;
    public GameObject objWood;
    public GameObject objCarrot;
    public GameObject equipment1;
    public GameObject blueprint_BONFIRE;
    public GameObject blueprint_TENT;
    public GameObject blueprint_CHAIR;
    public GameObject blueprint_TABLE;
    public GameObject blueprint_WORKBENCH;
    public GameObject eq_Axe;
    public GameObject eq_PickAxe;
    public GameObject eq_Hoe;
    public GameObject eq_Shovel;
    public GameObject objBlockSoil;

    [Header("정렬 임시 버튼")]
    [Space(10)]
    public GameObject[] btnInventory;
    // 0번부터 순서대로 오름차순 정렬, 내림차순 정렬, 원위치로 돌아가는 버튼 

    public Button btnAscSorting;    // 오름차순 정렬
    public Button btnDescSorting;   // 내림차순 정렬
    public Button btnGetPrevious;   // 원위치로

    public Enum_DropItemType[,] itemInventory;  //#7-1 Slot에서 가져와서 쓰고 싶은데, static으로 선언해야 되나? - 그냥 Inventory에서 실행하는 함수 만들면 되려나?
    public int[,] itemInventoryCount;

    [SerializeField]
    int row=4;
    [SerializeField]
    int col=4;

//#11-6 인벤토리 아이템 저장 목적
    private InventoryInfo invenInfo;
//# CollectItem 함수 사용 목적
    private Item _loadItem;     //JSON 데이터를 이용해서 가져올 Item 클래스 데이터 저장용
    private Enum_DropItemType _loadItemType;    
//#12-3
    public CraftingRecipe[] allRecipe;  //#14-3 멀티 보완중 //인스펙터로 연결하기
    public GameObject warningWindow;
//#14-2 제작대 오브젝트 연결
    public GameObject CraftingUI;   //인스펙터로 직접 연결

    //##
    public csPhotonGame csPG;

    //void Awake()
    //{
        //   allRecipe = GameObject.FindGameObjectsWithTag("Recipe");    //#12-3
        //InfoManager.Ins.LoadInvenJSONData();
    //}
    void Start()
    {
        //slots = gridInventory.GetComponentsInChildren<Slot>();

        previousSlots = new Slot[slots.Length];    // 크기가 할당되어 있지 않으면, null로 초기화 됨.

        itemInventory = new Enum_DropItemType[row, col];
        itemInventoryCount = new int[row, col];

        UpdateItemInvenData();  //#12-1

        btnAscSorting.onClick.AddListener(AscSorting);
        btnDescSorting.onClick.AddListener(DescSorting);
        btnGetPrevious.onClick.AddListener(GetPrevious);

        btnInventory[2].SetActive(false);

        ImgInventory.SetActive(false);  //인벤토리는 비활성화 상태로 시작
        SortingButtonsObj.SetActive(false); //#4-3 비활성화 상태로 시작
        DropItemZone.SetActive(false);  //#4-3

        InfoManager.Ins.LoadInvenJSONData();    //#11-6 리스트 자체는 한번 싹 Clear하고 JSON 데이터로 리스트 값 채워넣기

        // warningWindow.SetActive(false); //#12-4 꺼놓은 상태로 시작
    }

    void Update()
    {
        //퀵슬룻 선택 키
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectSlot.Ins.nowUsingSlot = slots[0];
            SelectSlot.Ins.transform.position = slots[0].transform.position;
            SelectSlot.Ins.ReSetShowItem();
            OptionManager.Ins.PlayClickSound();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectSlot.Ins.nowUsingSlot = slots[1];
            SelectSlot.Ins.transform.position = slots[1].transform.position;
            SelectSlot.Ins.ReSetShowItem();
            OptionManager.Ins.PlayClickSound();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectSlot.Ins.nowUsingSlot = slots[2];
            SelectSlot.Ins.transform.position = slots[2].transform.position;
            SelectSlot.Ins.ReSetShowItem();
            OptionManager.Ins.PlayClickSound();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectSlot.Ins.nowUsingSlot = slots[3];
            SelectSlot.Ins.transform.position = slots[3].transform.position;
            SelectSlot.Ins.ReSetShowItem();
            OptionManager.Ins.PlayClickSound();
        }

        // 테스트용 아이템 줍기 =========================
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            //Debug.Log("1 클릭");

            switch (UnityEngine.Random.Range(0, 5))//아이템 랜덤획득
            {
                case 0://과일
                    CollectItem(objFruit.GetComponent<Item>().ItemType, objFruit.GetComponent<Item>());
                    break;
                case 1://돌
                    CollectItem(objSton.GetComponent<Item>().ItemType, objSton.GetComponent<Item>());
                    break;
                case 2://나무
                    CollectItem(objWood.GetComponent<Item>().ItemType, objWood.GetComponent<Item>());
                    break;
                case 3://당근
                    CollectItem(objCarrot.GetComponent<Item>().ItemType, objCarrot.GetComponent<Item>());
                    break;
                case 4://흙블럭
                    CollectItem(objBlockSoil.GetComponent<Item>().ItemType, objBlockSoil.GetComponent<Item>());
                    break;
            }
            //CollectItem(objFruit.GetComponent<Item>().ItemType, objFruit.GetComponent<Item>());
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            //Debug.Log("2 클릭");
            switch (UnityEngine.Random.Range(0, 5))//청사진 랜덤획득
            {
                case 0://모닥불
                    CollectItem(blueprint_BONFIRE.GetComponent<Item>().ItemType, blueprint_BONFIRE.GetComponent<Item>());
                    break;
                case 1:
                    CollectItem(blueprint_TENT.GetComponent<Item>().ItemType, blueprint_TENT.GetComponent<Item>());
                    break;
                case 2:
                    CollectItem(blueprint_CHAIR.GetComponent<Item>().ItemType, blueprint_CHAIR.GetComponent<Item>());
                    break;
                case 3:
                    CollectItem(blueprint_TABLE.GetComponent<Item>().ItemType, blueprint_TABLE.GetComponent<Item>());
                    break;
                case 4:
                    CollectItem(blueprint_WORKBENCH.GetComponent<Item>().ItemType, blueprint_WORKBENCH.GetComponent<Item>());
                    break;
            }
            //CollectItem(objSton.GetComponent<Item>().ItemType, objSton.GetComponent<Item>());
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            //Debug.Log("3 클릭");//장비류 랜덤생성하는거 어떨지
            switch (UnityEngine.Random.Range(0, 4))//장비 랜덤획득
            {
                case 0://모닥불
                    CollectItem(eq_Axe.GetComponent<Item>().ItemType, eq_Axe.GetComponent<Item>());
                    break;
                case 1:
                    CollectItem(eq_PickAxe.GetComponent<Item>().ItemType, eq_PickAxe.GetComponent<Item>());
                    break;
                case 2:
                    CollectItem(eq_Hoe.GetComponent<Item>().ItemType, eq_Hoe.GetComponent<Item>());
                    break;
                case 3:
                    CollectItem(eq_Shovel.GetComponent<Item>().ItemType, eq_Shovel.GetComponent<Item>());
                    break;
            }
        }

        //if (Input.GetKey(KeyCode.Alpha4))
        //{
        //    Debug.Log("4 클릭");
        //    CollectItem(Enum_DropItemType.WEAPON_SWORD, equipment1);
        //}

        if (Input.GetKeyDown(KeyCode.Tab))       // Tab 누르면 인벤토리 활성화 (On/ Off)
        {
            csLevelManager.Ins.PlayAudioClip(Vector3.zero, 7);
            //Debug.Log("Tab 클릭");
            inventoryOpen = !inventoryOpen;

            ImgInventory.SetActive(inventoryOpen);
            SortingButtonsObj.SetActive(inventoryOpen); //#4-3
            DropItemZone.SetActive(inventoryOpen);  //#4-3

            if(!inventoryOpen)//#11-1 만약 인벤토리를 닫는 거라면 - toolTip, destructionOpt 창도 비활성화 시키기
            {
                toolTip.SetActive(false);
                destructionOpt.SetActive(false);
            }
        }
    }

    public void SaveInvenData() //#11-6 게임 퇴장할 때, 인벤토리 데이터 저장하기 - 한번씩만 해야할 듯. 나중에 JSON끼리 꼬여버려서 데이터 축적 돼
    {   
        InfoManager.Ins.InitializeInvenJSONData();  //JSON 파일 싹 밀어버린 후, 나가는 타이밍의 내 인벤토리 데이터를 저장하기

        for(int i=0; i<row; i++)
        {
            for(int j=0; j<col; j++)
            {
                invenInfo = new InventoryInfo();
                invenInfo.itemType = (int)itemInventory[i,j];
                // Debug.Log("#11-6 아이템 정보 : " + invenInfo.itemType);

                invenInfo.itemCount = itemInventoryCount[i,j];

                InfoManager.Ins.SetInvenInfo(invenInfo);
            }
        }
/*
        InventoryInfo invenInfo = new InventoryInfo();
        invenInfo.itemType = Enum_DropItemType.NONE;
        invenInfo.itemCount = 0;
*/        
        Invoke("ExecuteSaveInvenJson", 1.0f);   // 데이터가 꼬여서 잘 저장되지 않는 걸 막기 위해 조금 늦게 호출
    }
    void ExecuteSaveInvenJson()
    {
        InfoManager.Ins.SaveInvenJSONData();
    }

    public void LoadInvenData() //#11-6 게임 입장할 때, JSON (인벤토리) 데이터 가져와서 슬롯에 보이게 하기
    {
        Debug.Log("12333333");
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                // invenInfo = InfoManager.Ins.GetInvenInfo(i*col+j);
                invenInfo = new InventoryInfo();

                if (InfoManager.Ins.GetInvenInfo((col * i) + j) == null)
                {
                    Debug.Log("123165465");
                    return;     //Debug.Log("//#15-1 GetInvenInfo값이 null임");
                }
                invenInfo.itemType = InfoManager.Ins.GetInvenInfo((col * i) + j).itemType;
                invenInfo.itemCount = InfoManager.Ins.GetInvenInfo((col * i) + j).itemCount;

                Debug.Log(invenInfo.itemType+"//"+ invenInfo.itemCount+"asdasdasd");

                //Debug.Log("//#11-6 JSON에서 가져오는 invenInfo 타입 : " + invenInfo.itemType);
                //Debug.Log("//#11-6 JSON에서 가져오는 invenInfo 타입 : " + invenInfo.itemType);
                // 이건 애초에 저장하는 코드가 따로 있었잖아 -> ChangeSlotData 함수
                /*
                itemInventory[i,j] = invenInfo.itemType;
                itemInventoryCount[i,j] = invenInfo.itemCount;
                */

                switch (invenInfo.itemType)
                {
                    case (int)Enum_DropItemType.NONE:
                        _loadItem = null;   //#11-7 보완 (밑에서 null로 인식을 안해서 ChangeSlotData에서 에러 뜨는 것 같음.)
                        _loadItemType = Enum_DropItemType.NONE;
                        break;  //여기서 return; 하면 하나라도 비어있으면 로드 안되고 함수가 끝나겠지
                    case (int)Enum_DropItemType.FRUIT:
                        _loadItem = objFruit.GetComponent<Item>();
                        _loadItemType = Enum_DropItemType.FRUIT;
                        break;
                    case (int)Enum_DropItemType.STON:
                        _loadItem = objSton.GetComponent<Item>();
                        _loadItemType = Enum_DropItemType.STON;
                        break;
                    case (int)Enum_DropItemType.WOOD:
                        _loadItem = objWood.GetComponent<Item>();
                        _loadItemType = Enum_DropItemType.WOOD;
                        break;
                    case (int)Enum_DropItemType.CARROT:
                        _loadItem = objCarrot.GetComponent<Item>();
                        _loadItemType = Enum_DropItemType.CARROT;
                        break;
                    case (int)Enum_DropItemType.PLAYERWEAPONAXE1:
                        _loadItem = null;
                        Debug.Log("아이템에 웨폰 연결해줘야함");
                        break;
                    case (int)Enum_DropItemType.SHOVEL:
                        _loadItem = eq_Shovel.GetComponent<Item>();
                        _loadItemType = Enum_DropItemType.SHOVEL;
                        break;
                    case (int)Enum_DropItemType.AXE:
                        _loadItem = eq_Axe.GetComponent<Item>();
                        _loadItemType = Enum_DropItemType.AXE;
                        break;
                    case (int)Enum_DropItemType.PICKAXE:
                        _loadItem = eq_PickAxe.GetComponent<Item>();
                        _loadItemType = Enum_DropItemType.PICKAXE;
                        break;
                    case (int)Enum_DropItemType.HOE:
                        _loadItem = eq_Hoe.GetComponent<Item>();
                        _loadItemType = Enum_DropItemType.HOE;
                        break;
                    case (int)Enum_DropItemType.BLOCKSOIL:
                        _loadItem = objBlockSoil.GetComponent<Item>();
                        _loadItemType = Enum_DropItemType.BLOCKSOIL;
                        break;
                    //case Enum_DropItemType.BLUEPRINTTENT:
                    //    Debug.Log("탠트 청사진");
                    //    break;
                    case (int)Enum_DropItemType.BLUEPRINTWATCHFIRE:
                        _loadItem = blueprint_BONFIRE.GetComponent<Item>();
                        _loadItemType = Enum_DropItemType.BLUEPRINTWATCHFIRE;
                        break;
                    case (int)Enum_DropItemType.BLUEPRINTTENT:
                        _loadItem = blueprint_TENT.GetComponent<Item>();
                        _loadItemType = Enum_DropItemType.BLUEPRINTTENT;
                        break;
                    case (int)Enum_DropItemType.HOUSE_CHAIR:
                        _loadItem = blueprint_CHAIR.GetComponent<Item>();
                        _loadItemType = Enum_DropItemType.HOUSE_CHAIR;
                        break;
                    case (int)Enum_DropItemType.HOUSE_TABLE:
                        _loadItem = blueprint_TABLE.GetComponent<Item>();
                        _loadItemType = Enum_DropItemType.HOUSE_TABLE;
                        break;
                    case (int)Enum_DropItemType.BLUEPRINTWORKBENCH:
                        _loadItem = blueprint_WORKBENCH.GetComponent<Item>();
                        _loadItemType = Enum_DropItemType.BLUEPRINTWORKBENCH;
                        break;
                    default:
                        Debug.Log("무슨아이템인지 모르겠음");
                        break;
                }

                Debug.Log("//#11-6 아이템 추가하기 시작 : _loadItemType : " + _loadItemType);
                if (_loadItem != null)   //아이템이 없는 경우에는 그냥 AddSlot 안 타도록
                {
                    //수집하는 원리의 함수를 이용해야 하니까, 위치 그대로는 못가져옴 - 일부러 앞으로 당겨서 넣는 것처럼 하자
                    CollectItemLoad(_loadItemType, invenInfo.itemCount, ((col * i) + j));   //AddSlot 아님! Collect 하기
                }
                else
                {
                    CollectItemLoad(Enum_DropItemType.NONE, 0, ((col * i) + j));
                }
            }
        }
    }

    public void CollectItem(Enum_DropItemType dropItemType, Item _item = null, int _count = 1)  // _item이라는 이름의 아이템을 _count만큼 수집
    {
        bool tmpCheck = false;

        if (_item == null)
        {
            switch (dropItemType)
            {
                case Enum_DropItemType.NONE:
                    Debug.Log("빈칸 -> 손");
                    return;
                case Enum_DropItemType.FRUIT:
                    _item = objFruit.GetComponent<Item>();
                    break;
                case Enum_DropItemType.STON:
                    _item = objSton.GetComponent<Item>();
                    break;
                case Enum_DropItemType.WOOD:
                    _item = objWood.GetComponent<Item>();
                    break;
                case Enum_DropItemType.CARROT:
                    _item = objCarrot.GetComponent<Item>();
                    break;
                case Enum_DropItemType.PLAYERWEAPONAXE1:
                    Debug.Log("아이템에 웨폰 연결해줘야함");
                    break;
                case Enum_DropItemType.SHOVEL:
                    _item = eq_Shovel.GetComponent<Item>();
                    break;
                case Enum_DropItemType.AXE:
                    _item = eq_Axe.GetComponent<Item>();
                    break;
                case Enum_DropItemType.PICKAXE:
                    _item = eq_PickAxe.GetComponent<Item>();
                    break;
                case Enum_DropItemType.HOE:
                    _item = eq_Hoe.GetComponent<Item>();
                    break;
                case Enum_DropItemType.BLOCKSOIL:
                    _item = objBlockSoil.GetComponent<Item>();
                    break;
                //case Enum_DropItemType.BLUEPRINTTENT:
                //    Debug.Log("탠트 청사진");
                //    break;
                case Enum_DropItemType.BLUEPRINTWATCHFIRE:
                    _item = blueprint_BONFIRE.GetComponent<Item>();
                    break;
                case Enum_DropItemType.BLUEPRINTTENT:
                    _item = blueprint_TENT.GetComponent<Item>();
                    break;
                case Enum_DropItemType.HOUSE_CHAIR:
                    _item = blueprint_CHAIR.GetComponent<Item>();
                    break;
                case Enum_DropItemType.HOUSE_TABLE:
                    _item = blueprint_TABLE.GetComponent<Item>();
                    break;
                case Enum_DropItemType.BLUEPRINTWORKBENCH:
                    _item = blueprint_WORKBENCH.GetComponent<Item>();
                    break;
                default:
                    Debug.Log("무슨아이템인지 모르겠음");
                    return;
            }
        }          

        for (int i = 0; i < row && !tmpCheck; i++)
        {
            for (int j = 0; j < col && !tmpCheck; j++)
            {
                if (itemInventory[i, j].Equals(dropItemType))   //슬롯에 해당 아이템이 이미 있었다면
                {
                    if (itemInventoryCount[i, j] + _count <= _item.maxCount)//맥스카운트를 넘지 않았으면 ## (무기의 경우 1개만 들어가고, 다른 아이템들은 2개 이상 999개 이하씩 들어가겠지)
                    {
                        //itemInventoryCount[i, j]+= _count;
                        slots[(col * i) + j].UpdateSlotCount(_count);    //slots[(i% row) + j].UpdateSlotCount(_count);   // 개수 업데이트
                        tmpCheck = true;
                        //Debug.Log((i % row) + j);
                        //Debug.Log("타긴하니2");
                    }
                }
            }
        }            
        
        if (!tmpCheck)
        {
            bool isFind = false;

            for (int i = 0; i < row && !isFind; i++)
            {
                for (int j = 0; j < col && !isFind; j++)
                {
                    if (itemInventory[i, j].Equals(Enum_DropItemType.NONE)) // 슬롯에 sword가 없었다면, 슬롯에 추가
                    {
                        //itemInventory[i, j] = dropItemType;
                        //itemInventoryCount[i, j]+= _count;
                        slots[(col*i)+j].AddSlot(_item, _count);
                        
                        //Debug.Log((i % row) + j);
                        isFind = true;
                        //Debug.Log("타긴하니3");
                    }
                }
            }
        }

        //#12-3 자리가 아예 없다면 땅에 버려지도록
        
    }

    //JSON에서 저장할 아이템을 로드하기 위한 함수 비슷하게 추가로 만듦
    public void CollectItemLoad(Enum_DropItemType dropItemType, int _count = 1, int index=0)  // _item이라는 이름의 아이템을 _count만큼 수집
    {
        Item _item =null;

        switch (dropItemType)
        {
            case Enum_DropItemType.NONE:
                Debug.Log("빈칸 -> 손");
                return;
            case Enum_DropItemType.FRUIT:
                _item = objFruit.GetComponent<Item>();
                break;
            case Enum_DropItemType.STON:
                _item = objSton.GetComponent<Item>();
                break;
            case Enum_DropItemType.WOOD:
                _item = objWood.GetComponent<Item>();
                break;
            case Enum_DropItemType.CARROT:
                _item = objCarrot.GetComponent<Item>();
                break;
            case Enum_DropItemType.PLAYERWEAPONAXE1:
                Debug.Log("아이템에 웨폰 연결해줘야함");
                break;
           case Enum_DropItemType.SHOVEL:
                    _item = eq_Shovel.GetComponent<Item>();
                    break;
            case Enum_DropItemType.AXE:
                _item = eq_Axe.GetComponent<Item>();
                break;
            case Enum_DropItemType.PICKAXE:
                _item = eq_PickAxe.GetComponent<Item>();
                break;
            case Enum_DropItemType.HOE:
                _item = eq_Hoe.GetComponent<Item>();
                break;
            case Enum_DropItemType.BLOCKSOIL:
                _item = objBlockSoil.GetComponent<Item>();
                break;
            //case Enum_DropItemType.BLUEPRINTTENT:
            //    Debug.Log("탠트 청사진");
            //    break;
            case Enum_DropItemType.BLUEPRINTWATCHFIRE:
                _item = blueprint_BONFIRE.GetComponent<Item>();
                break;
            case Enum_DropItemType.BLUEPRINTTENT:
                _item = blueprint_TENT.GetComponent<Item>();
                break;
            case Enum_DropItemType.HOUSE_CHAIR:
                _item = blueprint_CHAIR.GetComponent<Item>();
                break;
            case Enum_DropItemType.HOUSE_TABLE:
                _item = blueprint_TABLE.GetComponent<Item>();
                break;
            case Enum_DropItemType.BLUEPRINTWORKBENCH:
                _item = blueprint_WORKBENCH.GetComponent<Item>();
                break;
            default:
                Debug.Log("무슨아이템인지 모르겠음");
                return;
        }
        
        if(_item==null){
            return;
        }

        slots[index].AddSlot(_item, _count);
    }

    public void AscSorting()
    {
        SortItemsByNumber(true);    // 오름차순
        btnInventory[2].SetActive(true);    //#3-1 누를 수 있도록 버튼 생성
    }

    public void DescSorting()
    {
        SortItemsByNumber(false);   // 내림차순
        btnInventory[2].SetActive(true);    //#3-1 누를 수 있도록 버튼 생성
    }

    public void SortItemsByNumber(bool AscSort)
    {
        if (AscSort)             // 오름차순 정렬을 하는 거라면 
        {
            if (alreadyAsc)      // && 이미 오름차순 정렬을 바로 전에 한 상태라면, 리턴
                return;

            if (!alreadyAsc)     // && 오름차순 정렬 처음 하는 거라면
            {
                alreadyAsc = true;
                alreadyDesc = false;
            }

        }
        else if (!AscSort)       // 내림차순 정렬을 하는 거라면 && 내림차순 정렬 처음 하는 거라면
        {
            if (alreadyDesc)     // && 이미 내림차순 정렬을 바로 전에 한 상태라면, 리턴
                return;

            if (!alreadyDesc)
            {
                alreadyDesc = true;
                alreadyAsc = false;
            }
        }
        sortTimes++;        // 정렬 1번 했음~

        if(sortTimes == 1)  // 정렬 2번 이상 연속으로 하면, 더이상 저장하지 않기. 맨 처음 정렬 버튼 누르기 전의 값만 저장.
            SavePreviousItems();    // 정렬하기 전 아이템 배열 저장

        Array.Sort(slots, delegate (Slot slot1, Slot slot2) 
        {    
            //#9-2 퀵슬롯에 해당하는 슬롯은 정렬 안타도록
            if(slot1.mySlotNumber ==0 || slot1.mySlotNumber ==1 ||slot1.mySlotNumber ==2 ||slot1.mySlotNumber ==3 
            ||slot2.mySlotNumber ==0 ||slot2.mySlotNumber ==1 ||slot2.mySlotNumber ==2 ||slot2.mySlotNumber ==3 )
                return 0;

            if (slot1.item == null && slot2.item == null)
                return 0;
            if (slot1.item == null)
                return 1;
            if (slot2.item == null)
                return -1;

            if (AscSort)
                return slot1.itemTotalSum.CompareTo(slot2.itemTotalSum);
            else
                return slot2.itemTotalSum.CompareTo(slot1.itemTotalSum);

        });

        // 변경한 순서에 따라 슬롯 위치도 변경  //Array.Sort 함수로 배열 정렬 후, 슬롯의 위치를 변경해주는 함수
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].transform.SetSiblingIndex(i);  // 정렬된 함수를 "실제로" 이동시켜주는 함수
        }
    }

    // public void DescSortItemsByNumber()
    // {
    //     SavePreviousItems();    // 정렬하기 전 아이템 배열 저장

    //     Array.Sort(slots, delegate(Slot slot1, Slot slot2)
    //     {
    //         if(slot1.item == null && slot2.item == null)
    //             return 0;
    //         if(slot1.item == null)
    //             return 1;
    //         if(slot2.item == null)
    //             return -1;

    //         return slot2.itemTotalSum.CompareTo(slot1.itemTotalSum);
    //     }  );

    //     // 변경한 순서에 따라 슬롯 위치도 변경  //Array.Sort 함수로 배열 정렬 후, 슬롯의 위치를 변경해주는 함수
    //     for(int i=0; i<slots.Length; i++)       
    //     {   
    //         slots[i].transform.SetSiblingIndex(i);      
    //     }
    // }

    public void SavePreviousItems() // 정렬하기 전 아이템 배열 저장
    {
        Array.Copy(slots, previousSlots, slots.Length);
    }

    public void GetPrevious()   // 정렬 전 저장한 아이템 배열로 다시 세팅하기
    {
        btnInventory[2].SetActive(false);   //#3-1 다시 안 보이게

        if (alreadyAsc)
            alreadyAsc = false;

        if (alreadyDesc)
            alreadyDesc = false;

        Array.Copy(previousSlots, slots, slots.Length);


        // 변경한 순서에 따라 슬롯 위치도 변경  //Array.Sort 함수로 배열 정렬 후, 슬롯의 위치를 변경해주는 함수
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].transform.SetSiblingIndex(i);
        }
    }

    //     public void SortItemsByNumber() 
    //     {
    // //#2-3 정렬(아이템 개수 순으로) =========================
    //         // Item[] items = new Item[slots.Length];

    // // 슬롯에 들어있는 아이템 개수는    //slots[i].itemTotalSum
    //         Array.Sort(slots, (a,b) => a.itemTotalSum.CompareTo(b.itemTotalSum));

    //         // for(int i=0; i<slots.Length; i++)
    //         // {
    //         //     if(! (slots[i].item == null))
    //         //     {
    //         //         items[i] = slots[i].item;
    //         //     }
    //         // }

    // //정렬한 리스트를 slots 변수에 할당해줘야 한다.
    //     }


    public void ChangeSlotData(int slotNum, int count=0 ,Enum_DropItemType type= Enum_DropItemType.NONE)
    {
        //for (int i = 0; i < row ; i++)
        //{
        //    for (int j = 0; j < col ; j++)
        //    {
        //        Debug.Log(itemInventory[i, j]
        //                    + " : " +  itemInventoryCount[i,j]);
        //    }
        //}

        //Debug.Log("110" + slotNum + slots.Length);
        //Debug.Log(slotNum + " ..??.. " + (slotNum / this.col) +","+ (slotNum % this.col));
        //Debug.Log("111" + slots[slotNum].item.ItemType + slots[slotNum].itemTotalSum);

        itemInventory[(slotNum / this.col) , (slotNum%this.col)] = type;
        itemInventoryCount[(slotNum / this.col), (slotNum % this.col)] = count;

        if (slotNum < 4)
        {
            SelectSlot.Ins.ReSetShowItem();
        }

        //Debug.Log("112" + itemInventory[(slotNum / this.x), (slotNum % this.y)] + itemInventoryCount[(slotNum / this.x), (slotNum % this.y)]);
    }

    public void DestroyItemAtAll()  //#5-1 아이템 파기하기  // btnDestruction 버튼에 연결
    {
        DestructionOpt.Ins.RemoveItem();
    }

    public void InvenToQuick(/*Item _item, int _count=1*/)   //#7-1 퀵슬롯으로 아이템 이동   //btnQuickSlot 버튼에 연결
    {
        // DestructionOpt.instance.MoveToQuickSlot();
        // 인벤토리 배열 중 3행에 null 값이 있는지 0열부터 3열까지 검사
        bool _goQuickSlot = false;   // for문 1번만 타도록 검사 장치

        for(int j=0; j<col && !_goQuickSlot; j++)
        {
            if(itemInventory[0, j].Equals(Enum_DropItemType.NONE))
            {
                // 퀵슬롯으로 이동
                Item _tempQuickItem = DestructionOpt.Ins.changeOptSlot.item;
                int _tempQuickCount = DestructionOpt.Ins.changeOptSlot.itemTotalSum;

                DestructionOpt.Ins.RemoveItem();   //기존 위치의 Slot은 Remove

                slots[0+j].AddSlot(_tempQuickItem, _tempQuickCount);  //이동 후 위치의 Slot(0+j)에는 Add  // 그러면 Slot의 AddSlot 스크립트에서 자동으로 배열에도 업데이트 해줌

                
                _goQuickSlot = true;
            }
        }
    }

    public void QuickToInven()  //#9-2 인벤토리로 가는 버튼도 구현
    {
        bool _goInvenSlot = false;  //for문 1번만 타도록 검사 장치

        for(int i=1; i<row && !_goInvenSlot; i++)
        {
            for(int j=0; j<col && !_goInvenSlot; j++)
            {
                if(itemInventory[i, j].Equals(Enum_DropItemType.NONE))
                {
                    Item _tempInvenItem = DestructionOpt.Ins.changeOptSlot.item;
                    int _tempInvenCount = DestructionOpt.Ins.changeOptSlot.itemTotalSum;

                    DestructionOpt.Ins.RemoveItem();

                    slots[col*i + j].AddSlot(_tempInvenItem, _tempInvenCount);

                    _goInvenSlot = true;
                }
            }
        }
    }

    public void UpdateItemInvenData()   //#12-1 제작대 시작할 때 itemInventory, itemInventoryCount 배열의 값을 업데이트 해주기
    {
        for(int i=0; i<row*col; i++)
        {
            ChangeSlotData(i);
        }
    }

    public int CheckItemCount(Enum_DropItemType _findItemType) //#12-1 제작대 - 아이템 개수 뽑기 위해
    {
        //##0501 아이템 슬룻이 꽉차있어서 여러개가 들어있었다면?!!
        int tmpResult = 0;

        for(int i=0; i<row ; i++)   //#12-3 i=1부터 되어있었음
        {
            for(int j=0; j<col; j++)
            {
                if(itemInventory[i, j].Equals(_findItemType))
                {
                    //##0501 합계계산하는거 추가
                    tmpResult += itemInventoryCount[i,j];
                }
            }
        }

        //##0501 처음에 0으로 초기화해서 최소 0이 리턴
        return tmpResult;   // 해당 아이템이 없음 = 0개이다
    }

    public void UseCraftingItem(Enum_DropItemType _useItemType, int _useCount) //제작에서 특정 아이템을 특정 개수만큼 사용하기
    {
        for(int i=0; i<row ; i++)   //#12-3 i=1부터 되어있던 거 고치기
        {
            for(int j=0; j<col; j++)
            {
                if((itemInventory[i, j].Equals(_useItemType)) && itemInventoryCount[i, j]>=_useCount)   //#12-2 어차피 재료 아이템은 한 슬롯에 여러개씩 있으니까 이렇게 검사해도 돼 
                    //#12-2 && 뒷 부분은 이미 CraftingRecipe.cs에서 검사하고 온 거라, 안 해도 되긴 하는데, 대비용으로 해놓자
                {

                    Debug.Log("//#14-3 n번째 슬롯 아이템 빼기 : " + col*i+j + "번째 슬롯. " + _useCount + "개 빼기");
                    slots[col*i + j].UpdateSlotCount(- _useCount);  
                }
            }
        }
    }

    //##0501 인벤토리가 꽉차있는데도 아이템이랑 부딫이면 사라지는 문제가있음. 빈공간이 있는지 확인하는 용도
    public bool CanAddItem(Enum_DropItemType dropItemType, int _count=1)
    {
        Item _item=null;

        switch (dropItemType)
        {
            case Enum_DropItemType.NONE:
                Debug.Log("빈칸 -> 손");
                break;
            case Enum_DropItemType.FRUIT:
                _item = objFruit.GetComponent<Item>();
                break;
            case Enum_DropItemType.STON:
                _item = objSton.GetComponent<Item>();
                break;
            case Enum_DropItemType.WOOD:
                _item = objWood.GetComponent<Item>();
                break;
            case Enum_DropItemType.CARROT:
                _item = objCarrot.GetComponent<Item>();
                break;
            case Enum_DropItemType.PLAYERWEAPONAXE1:
                Debug.Log("아이템에 웨폰 연결해줘야함");
                break;
            case Enum_DropItemType.SHOVEL:
                _item = eq_Shovel.GetComponent<Item>();
                break;
            case Enum_DropItemType.AXE:
                _item = eq_Axe.GetComponent<Item>();
                break;
            case Enum_DropItemType.PICKAXE:
                _item = eq_PickAxe.GetComponent<Item>();
                break;
            case Enum_DropItemType.HOE:
                _item = eq_Hoe.GetComponent<Item>();
                break;
            case Enum_DropItemType.BLOCKSOIL:
                _item = objBlockSoil.GetComponent<Item>();
                break;
            //case Enum_DropItemType.BLUEPRINTTENT:
            //    Debug.Log("탠트 청사진");
            //    break;
            case Enum_DropItemType.BLUEPRINTWATCHFIRE:
                _item = blueprint_BONFIRE.GetComponent<Item>();
                break;
            case Enum_DropItemType.BLUEPRINTTENT:
                _item = blueprint_TENT.GetComponent<Item>();
                break;
            case Enum_DropItemType.HOUSE_CHAIR:
                _item = blueprint_CHAIR.GetComponent<Item>();
                break;
            case Enum_DropItemType.HOUSE_TABLE:
                _item = blueprint_TABLE.GetComponent<Item>();
                break;
            case Enum_DropItemType.BLUEPRINTWORKBENCH:
                _item = blueprint_WORKBENCH.GetComponent<Item>();
                break;
            default:
                Debug.Log("무슨아이템인지 모르겠음");
                break;
        }

        if(_item==null){
            return false;
        }
        
        for (int i = 0; i < row ; i++)
        {
            for (int j = 0; j < col ; j++)
            {
                if (itemInventory[i, j].Equals(dropItemType))   //슬롯에 해당 아이템이 이미 있었다면
                {
                    if (itemInventoryCount[i, j] + _count <= _item.maxCount)//맥스카운트를 넘지 않았으면 ## (무기의 경우 1개만 들어가고, 다른 아이템들은 2개 이상 999개 이하씩 들어가겠지)
                    {
                        return true;
                    }
                }
            }
        }

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (itemInventory[i, j].Equals(Enum_DropItemType.NONE))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HaveEmptySpace() //#12-4 무기 아이템 하나 넣을 자리가 있나 - PlayerCtrl에서 사용
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (itemInventory[i, j].Equals(Enum_DropItemType.NONE)) //빈 공간이 하나라도 있다면
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void CheckAllRecipeState()   //#12-3
    {
        foreach(CraftingRecipe recipes in allRecipe)
        {
            Debug.Log("//#14-3 for문");
            // recipes.SendMessage("CheckRecipeState");
            recipes.CheckRecipeState();
        }
    }

    public void OpenWarningWindow()
    {
        warningWindow.SetActive(true);
        Invoke("CloseWarningWindow", 2.0f);
    }

    void CloseWarningWindow()
    {
        warningWindow.SetActive(false);
    }
    
    public void CloseCaftingUI()    //#15-1 제작대 닫기
    {   
        CraftingUI.SetActive(false);
    }


}