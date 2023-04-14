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
    private GameObject gridInventory;

    private Slot[] slots;                           // 슬롯 배열로 모두 가져오기
    private Slot[] previousSlots;

    [Header("테스트 목적 아이템 추가")]
    [Space(10)]
    public GameObject item1;
    public GameObject item2;
    public GameObject item3;

    public Item equipment1;

    [Header("정렬 임시 버튼")]
    [Space(10)]
    public GameObject[] btnInventory;
    // 0번부터 순서대로 오름차순 정렬, 내림차순 정렬, 원위치로 돌아가는 버튼 

    public Button btnAscSorting;    // 오름차순 정렬
    public Button btnDescSorting;   // 내림차순 정렬
    public Button btnGetPrevious;   // 원위치로

    public Enum_DropItemType[,] itemInventory;
    public int[,] itemInventoryCount;

    [SerializeField]
    int x=5;
    [SerializeField]
    int y=5;

    void Start()
    {
        slots = gridInventory.GetComponentsInChildren<Slot>();

        previousSlots = new Slot[slots.Length];    // 크기가 할당되어 있지 않으면, null로 초기화 됨.

        itemInventory = new Enum_DropItemType[x, y];
        itemInventoryCount = new int[x, y];

        btnAscSorting.onClick.AddListener(AscSorting);
        btnDescSorting.onClick.AddListener(DescSorting);
        btnGetPrevious.onClick.AddListener(GetPrevious);

        btnInventory[2].SetActive(false);

        ImgInventory.SetActive(false);  //인벤토리는 비활성화 상태로 시작
    }

    void Update()
    {
        // 테스트용 아이템 줍기 =========================
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("1 클릭");
            CollectItem(item1.GetComponent<Item>().ItemType, item1.GetComponent<Item>());
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("2 클릭");
            CollectItem(item2.GetComponent<Item>().ItemType, item2.GetComponent<Item>());
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("3 클릭");
            CollectItem(item3.GetComponent<Item>().ItemType, item3.GetComponent<Item>());
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
        }


    }

    public void CollectItem(Enum_DropItemType dropItemType, Item _item = null, int _count = 1)  // _item이라는 이름의 아이템을 _count만큼 수집
    {
        bool tmpCheck = false;

        if (!_item.ItemType.Equals(Enum_DropItemType.WEAPON_SWORD))
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

           

            for (int i = 0; i < x && !tmpCheck; i++)
            {
                for (int j = 0; j < y && !tmpCheck; j++)
                {
                    if (itemInventory[i, j].Equals(dropItemType))
                    {
                        itemInventoryCount[i, j]+= _count;
                        slots[(i% x) + j].UpdateSlotCount(_count);   // 개수 업데이트
                        tmpCheck = true;
                        //Debug.Log((i % x) + j);
                        //Debug.Log("타긴하니2");
                    }
                }
            }            
        }
        else if (_item.ItemType.Equals(Enum_DropItemType.WEAPON_SWORD))
        {
            tmpCheck = false;
            _count = 1;
        }

        if (!tmpCheck)
        {
            bool isFind = false;

            for (int i = 0; i < x && !isFind; i++)
            {
                for (int j = 0; j < y && !isFind; j++)
                {
                    if (itemInventory[i, j].Equals(Enum_DropItemType.NONE))
                    {
                        itemInventory[i, j] = dropItemType;
                        itemInventoryCount[i, j]+= _count;
                        slots[(i%x)+j].AddSlot(_item, _count);

                        //Debug.Log((i % x) + j);
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


}