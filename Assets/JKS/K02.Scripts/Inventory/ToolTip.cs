using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;   //#8-1 툴팁

public class ToolTip : MonoBehaviour    //#8-1 툴팁 구현
{
    static public ToolTip instance;    // 1개의 슬롯에만 해당되도록

    [HideInInspector]
    public Slot toolTipSlot;            // 툴팁 설명 나타낼 슬롯

    [SerializeField]
    private Text txtToolTipName;        // 툴팁 - 아이템 이름  //txtToolTipName 연결

    [SerializeField]
    private Text txtToolTipExplain;        // 툴팁 - 아이템 설명  //txtToolTipExplain 연결

    void Start()
    {
        instance = this;

        if(this.gameObject.activeSelf)      // 비활성화 상태로 시작해야 돼
            this.gameObject.SetActive(false);   
    }

    public void OpenToolTip(bool _openTrue, string _itemName)
    {
        if(_openTrue && (!this.gameObject.activeSelf))  // 툴팁을 켜고 싶은데, 현재 오브젝트가 비활성화 되어 있다면
        {
            this.gameObject.SetActive(_openTrue);       // 켠다
            txtToolTipName.text = _itemName;
        }
        
        if(!_openTrue && (this.gameObject.activeSelf))  // 툴팁을 끄고 싶은데, 현재 오브젝트가 활성화 되어 있다면 
            this.gameObject.SetActive(_openTrue);       // 끈다
    }
}
