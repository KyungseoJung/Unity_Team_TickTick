using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DestructionOpt : MonoBehaviour     //#5-1
{
    static public DestructionOpt instance;    // 1개의 슬롯에만 해당되도록
    public Slot destructionOptSlot;           // '파기하기' 선택할 슬롯

// '파기하기' 이미지
    void Start()
    {
        instance = this;

        if(this.gameObject.activeSelf)  //처음엔 창 닫힌채로 시작되도록
            OpenDestrucionOpt(false);
    }

    public void OpenDestrucionOpt(bool openTrue)
    {
        if((openTrue == true) && (!gameObject.activeSelf))  // 반복 행위 방지(이미 활성화 되어있는데 또 활성화 시키는 거 방지)
            this.gameObject.SetActive(openTrue);
        
        if((openTrue == false) && (gameObject.activeSelf))  // 반복 행위 방지
            this.gameObject.SetActive(openTrue);    
        
    }

    public void DestroyItemAtAll()  //아이템 아예 파기
    {
        OpenDestrucionOpt(false);           // 파기하기 창 닫기
        destructionOptSlot.RemoveSlot();    // 아이템 아예 파기
    }

}
