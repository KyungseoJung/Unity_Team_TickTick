using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectSlot : csGenericSingleton<SelectSlot> //#11-2
{
    public Slot nowUsingSlot;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
    
    void Start()
    {
        // this.gameObject.SetActive(false);   // 꺼놓고 시작하기
        transform.position = new Vector3(456, 110, 0);
    }

    public void ShowHighLight(bool _show)    // 맨 처음에만 필요
    {
        // transform.position = nowUsingSlot.transform.position;
        
        if(_show&& !this.gameObject.activeSelf) //하이라이트 true인데 비활성화 상태라면
            this.gameObject.SetActive(_show);   //하이라이트 보여주기
    }

    




}
