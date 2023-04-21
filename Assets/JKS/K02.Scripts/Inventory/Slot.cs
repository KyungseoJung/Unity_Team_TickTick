using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;     //#2-2

using TeamInterface;

public class Slot : MonoBehaviour   //#2-1 인벤토리 중 슬롯 하나하나의 관리
                             ,IPointerClickHandler , IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
                             ,IPointerEnterHandler, IPointerExitHandler //#8-1 툴팁 구현
{
    [SerializeField]
    public int mySlotNumber=0;  //#9-2 정렬에서 사용

    private GameObject inventoryObject;
    public Inventory inventory;         //#3-1 // alreadyAsc, alreadyDesc 체크 목적 

    public Item item;                   // 획득한 아이템        //플레이 중에 할당 (AddItem 함수를 통해)
    public int itemTotalSum;            // 획득한 아이템 개수   //플레이 중에 할당
    public Image ImgSlotItem;           // 슬롯에 띄울 아이템 이미지    

    [SerializeField]
    private Text txtCount;              //아이템 개수 텍스트
    // [SerializeField]
    // private GameObject ImgCount;        //아이템 개수 이미지

    private bool toolTipOpen = false;           //#8-1 툴팁 오픈 한번만 타도록


    void Awake()
    {
        inventoryObject = transform.root.gameObject.GetComponentInChildren<Inventory>().gameObject;
        inventory = inventoryObject.GetComponent<Inventory>();
        
    }


    // 아이템 이미지 투명도 조절 목적
    private void SetAlpha(float _alpha)
    {
        Color color = ImgSlotItem.color;
        color.a = _alpha;
        ImgSlotItem.color = color;
    }

    // 인벤토리에 새로운 아이템(슬롯) 추가
    public void AddSlot(Item _item, int _count /* =1 */ )
    {
        //Debug.Log(_item.itemName);
        item = _item;
        itemTotalSum = _count; /*_count;*/
        ImgSlotItem.sprite = item.itemImage;        // 슬롯에 각 아이템 고유의 이미지 띄우기

        inventory.ChangeSlotData(mySlotNumber, itemTotalSum, item.ItemType);    //# 용훈님 추가 내용. 슬롯 데이터 저장 목적

        if (!item.ItemType.Equals(Enum_DropItemType.WEAPON_SWORD))    //무기가 아니라면 개수와 함께 슬롯에 추가
        {
            // ImgCount.SetActive(true);
            txtCount.text = itemTotalSum.ToString();
        }
        else                                        // 무기라면 개수 적을 필요 없이 슬롯에 추가
        {
            txtCount.text = "";    
            // ImgCount.SetActive(false);
        }

        SetAlpha(1);    //불투명하게 보이게
    }

    //해당 슬롯의 아이템 개수 업데이트
    public void UpdateSlotCount(int _count)
    {
        itemTotalSum += _count;
        txtCount.text = itemTotalSum.ToString();    //바뀐 개수로 텍스트 업데이트

        inventory.ChangeSlotData(mySlotNumber, itemTotalSum, item.ItemType);

        if (itemTotalSum <=0)
            RemoveSlot();
    }

    // 해당 슬롯 하나 삭제
    public void RemoveSlot()
    {
        inventory.ChangeSlotData(mySlotNumber);

        item = null;
        itemTotalSum = 0;
        ImgSlotItem.sprite = null;        

        txtCount.text = itemTotalSum.ToString();    //#3-1 
        SetAlpha(0);    // 투명하게 보이도록
    }

//#2-2 드래그 앤 드롭 ===========================
    public void OnBeginDrag(PointerEventData eventData) // 마우스 드래그 시작할 때 호출되는 이벤트 함수
    {
        if(item != null)
        {
            DragItem.instance.dragStartSlot = this;  // 이 아이템을 드래그 앤 드롭할 거다
            DragItem.instance.DragSetImage(ImgSlotItem);
            DragItem.instance.transform.position = eventData.position;  // DragItem의 이미지를 끌고 다녀

           // Debug.Log("dragStart");
        }
    }

    public void OnDrag(PointerEventData eventData)  // 드래그 하는 동안 호출되는 이벤트 함수
    {
        if (item != null)
        {
            DragItem.instance.transform.position = eventData.position;

            //Debug.Log(103);
        }
    }

    public void OnEndDrag(PointerEventData eventData)   // (D&D 가장 마짐가에 호출됨. OnDrop보다 나중 호출) 드래그 끝날 때 호출되는 이벤트 함수
    {
        DragItem.instance.SetAlpha(0);          // 드래그 앤 드롭 색깔 투명하게
        DragItem.instance.dragStartSlot = null;      //드래그 앤 드롭 끝~!
        //Debug.Log(107);
        inventory.ChangeSlotData(mySlotNumber, itemTotalSum, item.ItemType);    //#9-3 질문
    }

    public void OnDrop(PointerEventData eventData)  // 내 자신한테 무언가가 드롭되었을 때 호출되는 이벤트 함수 (OnEndDrag 보다 먼저 호출된대)
    {
        //Debug.Log(106);
        // 나한테 뭔가 드롭된 그 무언가가 null이 아니면
        if (DragItem.instance.dragStartSlot != null)
        {
            ChangeSlotItem();

            //Debug.Log(102);
        }
        //Debug.Log(105);
    }

    private void ChangeSlotItem()
    {
        //        inventory.SavePreviousItems();            //#3-1 변경 전 아이템 위치 저장
        
        if (inventory.alreadyAsc)
            inventory.alreadyAsc = false;
        if (inventory.alreadyDesc)
            inventory.alreadyDesc = false;

        inventory.sortTimes = 0;                            //#9-2 정렬 횟수 0으로 초기화

        inventory.btnInventory[2].SetActive(false);        //#3-1 원위치로 돌아가는 버튼 다시 사라지도록(정렬 버튼 눌렀을 때에만 나오게)


        Item originItem = item;                   // 이동 후의 위치에 있던 아이템의 복사본 만들어놓기 
        int originItemTotalSum = itemTotalSum;

        Item dragItem = DragItem.instance.dragStartSlot.item; // 드래그 하고 있는 아이템의 정보를 넣기
        int dragItemTotalSum = DragItem.instance.dragStartSlot.itemTotalSum;
        
        AddSlot(dragItem, dragItemTotalSum);        // D&D 을 마지막으로 실행한 슬롯의 AddSlot이 실행돼

        if(originItem != null)   //이동 후의 위치에 무언가가(originItem) 있었다면, 바꿔치기 (드래그 시작 위치에 originItem 갖다놓아)
            DragItem.instance.dragStartSlot.AddSlot(originItem, originItemTotalSum);
        else    // 이동 후 위치에 아무것도 없었다면, 그냥 추가만~ // 드래그 시작 위치에 그냥 Remove 실행
            DragItem.instance.dragStartSlot.RemoveSlot();

       // Debug.Log(101);
    }

    public void DropItemOnGround(Transform dropPos) //#3-2
    {
        Instantiate(item.itemPrefab, dropPos.position, Quaternion.identity);  //RemoveSlot에서 item이 null이 되기 전에~

        RemoveSlot();       // 드래그 시작했던 아이템의 슬롯 위치를 지워

    }
//#5-1 마우스 우클릭 - 파기하기 창 생성 ========================

    public void OnPointerClick(PointerEventData eventData)
    {
        if((item != null) && (eventData.button == PointerEventData.InputButton.Right))  // 아이템이 있는 슬롯에 && 우측 마우스 클릭했을 때
        {
            Vector2 finalPos = eventData.position;

            //0,1 열은 우측에 상 생성   //2,3 열은 좌측에 창 생성
            switch(mySlotNumber % 4)  //나머지가 0, 1이면 0열, 1열 //나머지가 2,3이면 2열, 3열
            {
                case 0 : 
                case 1 :
                    finalPos.x -= 70f;  //자연스럽게 보이기 위한 위치 조정
                    finalPos.y -= 30f;
                    break;
                case 2 :
                case 3 :
                    finalPos.x += 70f;  //자연스럽게 보이기 위한 위치 조정
                    finalPos.y -= 30f;
                    break;
            }
            

            DestructionOpt.instance.transform.position = finalPos;    // '파기하기' 창이 마우스 위치한 곳에 나타나도록
            DestructionOpt.instance.changeOptSlot = this;             //# 7-1 뭔가 변화를 줄(파기하기 or 퀵슬롯에) 슬롯 선택
            
            if(mySlotNumber<4)  //#7-1 만약 퀵슬롯에서 우클릭을 한 거라면
            {
                DestructionOpt.instance.OpenDestrucionOpt(true, false, true); //'퀵 슬롯' 버튼 닫고, 인벤토리 이동 버튼 활성화
                return;
            }
            else if(mySlotNumber>=4)    //#9-2 인벤토리 슬롯에서 우클릭을 한 거라면
                DestructionOpt.instance.OpenDestrucionOpt(true, true, false);
        }
        else    // ??? // 아이템이 없는 곳에 or 좌측 마우스 클릭하면 창 닫히도록
        {
            DestructionOpt.instance.OpenDestrucionOpt(false);
        }
    }

    // private void DestroyItemAtAll()  //아이템 아예 파기
    // {
    //     DestructionOpt.instance.OpenDestrucionOpt(false);           // 파기하기 창 닫기
    //     DestructionOpt.instance.destructionOptSlot.RemoveSlot();    // 아이템 아예 파기
    // }

// //#7-1 퀵슬롯으로 이동 ========================
//     public void PushToQuickSlot()
//     {
//         //
//         // 우클릭한 퀵슬롯 DestructionOpt.instance.changeOptSlot 을 mySlotNumber : 12번 이상의 배열로 이동시켜

//     }

// #8-1 툴팁 구현   ========================
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(item != null && !toolTipOpen)
        {
            Vector2 finalPos = eventData.position;

            //0,1 열은 우측에 상 생성   //2,3 열은 좌측에 창 생성
            switch(mySlotNumber % 4)  //나머지가 0, 1이면 0열, 1열 //나머지가 2,3이면 2열, 3열
            {
                case 0 : 
                case 1 :
                    finalPos.x -= 150f;  //자연스럽게 보이기 위한 위치 조정
                    break;
                case 2 :
                case 3 :
                    finalPos.x += 150f;  //자연스럽게 보이기 위한 위치 조정
                    break;
            }
            
            ToolTip.instance.toolTipSlot = this;
            ToolTip.instance.transform.position = finalPos;
            ToolTip.instance.OpenToolTip(true, item.itemName);

            Debug.Log("Slot 스크립트 - toolTip 오픈");
            toolTipOpen= true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(item != null && toolTipOpen)
        {
            ToolTip.instance.OpenToolTip(false, item.itemName);

            Debug.Log("Slot 스크립트 - toolTip 닫기");
            toolTipOpen = false;
        }
    }


}
