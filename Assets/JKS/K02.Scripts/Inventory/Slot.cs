using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;     //#2-2

using TeamInterface;

public class Slot : MonoBehaviour   //#2-1 인벤토리 중 슬롯 하나하나의 관리
                            /* ,IPointerClickHandler*/ , IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField]
    private int mySlotNumber=0;

    private GameObject inventoryObject;
    public Inventory inventory;         //#3-1 // alreadyAsc, alreadyDesc 체크 목적 

    public Item item;                   // 획득한 아이템        //플레이 중에 할당 (AddItem 함수를 통해)
    public int itemTotalSum;            // 획득한 아이템 개수   //플레이 중에 할당
    public Image ImgSlotItem;           // 슬롯에 띄울 아이템 이미지    

    [SerializeField]
    private Text txtCount;              //아이템 개수 텍스트
    [SerializeField]
    private GameObject ImgCount;        //아이템 개수 이미지


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

        inventory.ChangeSlotData(mySlotNumber, itemTotalSum, item.ItemType);

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
    private void RemoveSlot()
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

    public void OnEndDrag(PointerEventData eventData)   // 드래그 끝날 때 호출되는 이벤트 함수
    {
        DragItem.instance.SetAlpha(0);          // 드래그 앤 드롭 색깔 투명하게
        DragItem.instance.dragStartSlot = null;      //드래그 앤 드롭 끝~!
        //Debug.Log(107);
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

        inventory.btnInventory[2].SetActive(false);        //#3-1 원위치로 돌아가는 버튼 다시 사라지도록(정렬 버튼 눌렀을 때에만 나오게)


        Item originItem = item;                   // 이동 후의 위치에 있던 아이템의 복사본 만들어놓기 
        int originItemTotalSum = itemTotalSum;

        Item dragItem = DragItem.instance.dragStartSlot.item; // 드래그 하고 있는 아이템의 정보를 넣기
        int dragItemTotalSum = DragItem.instance.dragStartSlot.itemTotalSum;
        
        AddSlot(dragItem, dragItemTotalSum);

        if(originItem != null)   //이동 후의 위치에 무언가 있었다면, 바꿔치기 (드래그 시작 위치에 originItem 갖다놓아)
            DragItem.instance.dragStartSlot.AddSlot(originItem, originItemTotalSum);
        else    // 이동 후 위치에 아무것도 없었다면, 그냥 추가만~
            DragItem.instance.dragStartSlot.RemoveSlot();

       // Debug.Log(101);
    }

    public void DropItemOnGround(Transform dropPos) //#3-2
    {
        Instantiate(item.itemPrefab, dropPos.position, Quaternion.identity);  //RemoveSlot에서 item이 null이 되기 전에~

        RemoveSlot();       // 드래그 시작했던 아이템의 슬롯 위치를 지워

    }
}
