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

    [SerializeField]
    private GameObject ImgInventory;
    [SerializeField]
    private GameObject SortingButtonsObj;   //#4-3 정렬 버튼 모음 오브젝트 (Sorting 오브젝트)
    [SerializeField]
    private GameObject DropItemZone;   //#4-3 아이템 드랍 존
    [SerializeField]
    private GameObject gridInventory;

    [SerializeField]
    private Slot[] slots;                           // 슬롯 배열로 모두 가져오기
    private Slot[] previousSlots;

    [Header("테스트 목적 아이템 추가")]
    [Space(10)]
    public GameObject objFruit;
    public GameObject objSton;
    public GameObject objWood;

    public Item equipment1;

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

    void Start()
    {
        //slots = gridInventory.GetComponentsInChildren<Slot>();

        previousSlots = new Slot[slots.Length];    // 크기가 할당되어 있지 않으면, null로 초기화 됨.

        itemInventory = new Enum_DropItemType[row, col];
        itemInventoryCount = new int[row, col];

        btnAscSorting.onClick.AddListener(AscSorting);
        btnDescSorting.onClick.AddListener(DescSorting);
        btnGetPrevious.onClick.AddListener(GetPrevious);

        btnInventory[2].SetActive(false);

        ImgInventory.SetActive(false);  //인벤토리는 비활성화 상태로 시작
        SortingButtonsObj.SetActive(false); //#4-3 비활성화 상태로 시작
        DropItemZone.SetActive(false);  //#4-3
    }

    void Update()
    {
        // 테스트용 아이템 줍기 =========================
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("1 클릭");
            CollectItem(objFruit.GetComponent<Item>().ItemType, objFruit.GetComponent<Item>());
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("2 클릭");
            CollectItem(objSton.GetComponent<Item>().ItemType, objSton.GetComponent<Item>());
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("3 클릭");
            CollectItem(objWood.GetComponent<Item>().ItemType, objWood.GetComponent<Item>());
        }

        //if (Input.GetKey(KeyCode.Alpha4))
        //{
        //    Debug.Log("4 클릭");
        //    CollectItem(Enum_DropItemType.WEAPON_SWORD, equipment1);
        //}

        if (Input.GetKeyDown(KeyCode.Tab))       // Tab 누르면 인벤토리 활성화 (On/ Off)
        {
            //Debug.Log("Tab 클릭");
            inventoryOpen = !inventoryOpen;

            ImgInventory.SetActive(inventoryOpen);
            SortingButtonsObj.SetActive(inventoryOpen); //#4-3
            DropItemZone.SetActive(inventoryOpen);  //#4-3
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
                    Debug.Log("무슨아이템인지 모르겠음");
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
                case Enum_DropItemType.WEAPON_SWORD:
                    Debug.Log("아이템에 웨폰 연결해줘야함");
                    break;
                default:
                    Debug.Log("무슨아이템인지 모르겠음");
                    return;

            }
        }

        if (!_item.ItemType.Equals(Enum_DropItemType.WEAPON_SWORD)) // SWORD 외의 아이템을 collect 했을 때
        {
            //for (int i = 0; i < slots.Length; i++)  //획득한 아이템이 슬롯에 이미 있는 아이템인가 확인
            //{
            //    if (slots[i].item != null)   //비어있는 슬롯이 아니라면 (Null 에러 방지)
            //    {
            //        if (_item.itemName == slots[i].item.itemName)    //슬롯에 해당 아이템이 이미 있었다면
            //        {
            //            Debug.Log("UpdateItemCount");
            //            slots[i].UpdateSlotCount(_count);   // 개수 업데이트
            //            return;
            //        }
            //    }
            //}

           

            for (int i = 0; i < row && !tmpCheck; i++)
            {
                for (int j = 0; j < col && !tmpCheck; j++)
                {
                    if (itemInventory[i, j].Equals(dropItemType))   //슬롯에 해당 아이템이 이미 있었다면
                    {
                        //itemInventoryCount[i, j]+= _count;
                        slots[(col*i) + j].UpdateSlotCount(_count);    //slots[(i% row) + j].UpdateSlotCount(_count);   // 개수 업데이트
                        tmpCheck = true;
                        //Debug.Log((i % row) + j);
                        //Debug.Log("타긴하니2");
                    }
                }
            }            
        }
        else if (_item.ItemType.Equals(Enum_DropItemType.WEAPON_SWORD)) // sword를 collect 했을 때
        {
            tmpCheck = false;
            _count = 1;
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

        ////위 if문에서 return 되지 않았다 == 해당 아이템이 슬롯에 없다
        //for (int i = 0; i < slots.Length; i++)
        //{
        //    if (slots[i].item == null)   //슬롯에 남은 자리 있으면
        //    {
        //        Debug.Log("AddItem");
        //        slots[i].AddSlot(_item, _count);
        //        return;
        //    }
        //}
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

        SavePreviousItems();    // 정렬하기 전 아이템 배열 저장

        Array.Sort(slots, delegate (Slot slot1, Slot slot2)
        {
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
            slots[i].transform.SetSiblingIndex(i);
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


    public void ChangeSlotData(int slotNum, int count=0,Enum_DropItemType type= Enum_DropItemType.NONE)
    { 
        //Debug.Log("110" + slotNum + slots.Length);

        //Debug.Log("111" + slots[slotNum].item.ItemType + slots[slotNum].itemTotalSum);
        itemInventory[(slotNum / this.col) , (slotNum%this.col)] = type;
        itemInventoryCount[(slotNum / this.col), (slotNum % this.col)] = count;
        //Debug.Log("112" + itemInventory[(slotNum / this.x), (slotNum % this.y)] + itemInventoryCount[(slotNum / this.x), (slotNum % this.y)]);
    }

    public void DestroyItemAtAll()  //#5-1 아이템 파기하기  // btnDestruction 버튼에 연결
    {
        DestructionOpt.instance.RemoveItem();
    }

    public void MoveToQuickSlot(/*Item _item, int _count=1*/)   //#7-1 퀵슬롯으로 아이템 이동   //btnQuickSlot 버튼에 연결
    {
        // DestructionOpt.instance.MoveToQuickSlot();
        // 인벤토리 배열 중 3행에 null 값이 있는지 0열부터 3열까지 검사
        bool goQuickSlot = false;   // for문 1번만 타도록 검사 장치

        for(int j=0; j<col && !goQuickSlot; j++)
        {
            if(itemInventory[0, j].Equals(Enum_DropItemType.NONE))
            {
                // 퀵슬롯으로 이동
                Item _tempItem = DestructionOpt.instance.changeOptSlot.item;
                int _tempCount = DestructionOpt.instance.changeOptSlot.itemTotalSum;

                DestructionOpt.instance.RemoveItem();   //기존 위치의 Slot은 Remove

                slots[0+j].AddSlot(_tempItem, _tempCount);  //이동 후 위치의 Slot(0+j)에는 Add  // 그러면 Slot의 AddSlot 스크립트에서 자동으로 배열에도 업데이트 해줌

                
                goQuickSlot = true;
            }
        }
        
    }
}