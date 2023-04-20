using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DestructionOpt : MonoBehaviour     //#5-1
{
    static public DestructionOpt instance;    // 1개의 슬롯에만 해당되도록
    
    [HideInInspector]
    public Slot changeOptSlot;           // Slot 스크립트에서 instance로 지정함
                                        // '파기하기' 선택할 슬롯 //Slot 스크립트에서 참조하기 때문에 public
                                        //#7-1 퀵슬롯으로 이동할 슬롯
    public GameObject btnQuickSlot;     //btnQuickSlot 버튼
// '파기하기' 이미지
    void Start()
    {
        instance = this;

        if(this.gameObject.activeSelf)  //처음엔 창 닫힌채로 시작되도록
            OpenDestrucionOpt(false);
    }

    public void OpenDestrucionOpt(bool _openTrue, bool _quickSlotOpen = true)   //파기하기 창, 퀵슬롯 버튼 동시에 관리 
    {
        if((_openTrue == true) && (!gameObject.activeSelf))  // 반복 행위 방지(이미 활성화 되어있는데 또 활성화 시키는 거 방지)
            this.gameObject.SetActive(_openTrue);
        
        if((_openTrue == false) && (gameObject.activeSelf))  // 반복 행위 방지
            this.gameObject.SetActive(_openTrue);    

        if(_quickSlotOpen && (!btnQuickSlot.activeSelf)) // true인데 비활성화 상태라면
        {
            btnQuickSlot.SetActive(true);   // 퀵슬롯 버튼 활성화
        }   
        else if(!_quickSlotOpen)  // 퀵슬롯 버튼만 비활성화 하고자 한다.
            btnQuickSlot.SetActive(false);     
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
