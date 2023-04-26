using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DestructionOpt : csGenericSingleton<DestructionOpt>     //#5-1
{
    // static public DestructionOpt instance;    // 1개의 슬롯에만 해당되도록
    
    [HideInInspector]
    public Slot changeOptSlot;           // Slot 스크립트에서 instance로 지정함
                                        // '파기하기' 선택할 슬롯 //Slot 스크립트에서 참조하기 때문에 public
                                        //#7-1 퀵슬롯으로 이동할 슬롯
    public GameObject btnQuickSlot;     //btnQuickSlot 버튼 //Inventory 스크립트를 포함한 오브젝트의 QuickToInven 함수 연결
    public GameObject btnInvenSlot;       //btnInvenSlot 버튼 //Inventory 스크립트를 포함한 오브젝트의 InvenToQuick 함수 연결

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
    
// '파기하기' 이미지
    void Start()
    {

        if(this.gameObject.activeSelf)  //처음엔 창 닫힌채로 시작되도록
            OpenDestrucionOpt(false);
    }

    public void OpenDestrucionOpt(bool _openTrue, bool _quickSlotOpen = true, bool _invenSlotOpen = false)   //파기하기 창, 퀵슬롯 버튼 동시에 관리 
    {
        if((_openTrue) && (!gameObject.activeSelf))  // 반복 행위 방지(이미 활성화 되어있는데 또 활성화 시키는 거 방지)
            this.gameObject.SetActive(true);
        
        if((!_openTrue) && (gameObject.activeSelf))  // 반복 행위 방지
            this.gameObject.SetActive(false);    

//퀵슬롯으로 이동 버튼
        if(_quickSlotOpen && (!btnQuickSlot.activeSelf)) // true인데 비활성화 상태라면
            btnQuickSlot.SetActive(true);   // 퀵슬롯 버튼 활성화
        else if(!_quickSlotOpen && btnQuickSlot.activeSelf)  // false인데 활성화 상태라면
            btnQuickSlot.SetActive(false);  // 퀵슬롯 버튼만 비활성화 하고자 한다.

//#9-2 인벤토리 슬롯으로 이동 버튼
        if(_invenSlotOpen && (!btnInvenSlot.activeSelf))
            btnInvenSlot.SetActive(true);
        else if(!_invenSlotOpen && btnInvenSlot.activeSelf)
            btnInvenSlot.SetActive(false);
    }

    public void CloseDestructionOpt()   // 창 닫기 - btnExit 버튼에 연결 - 버튼에는 매개변수 조정 못해서 함수 따로 추가함
    {
        this.gameObject.SetActive(false);    
    }

    public void RemoveItem()  //아이템 아예 파기
    {
        OpenDestrucionOpt(false);           // 파기하기 창 닫기
        changeOptSlot.RemoveSlot();    // 아이템 아예 파기
    }

//     //#7-1 퀵슬롯 아이템 이동 
//     public void MoveToQuickSlot()
//     {
//         OpenDestrucionOpt(false);           // 파기하기 창 닫기
        
// //        changeOptSlot.PushToQuickSlot();    // 퀵슬롯으로 이동시키기
//     }

}
