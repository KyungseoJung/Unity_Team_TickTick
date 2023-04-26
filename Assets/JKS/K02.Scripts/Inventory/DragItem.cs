using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEngine.UI;           //#2-2

public class DragItem : csGenericSingleton<DragItem>   //#2-2 인벤토리 드래그 앤 드롭 구현
{
    public Slot dragStartSlot;           // 플레이 중 (각 Slot 스크립트에서) 할당될 예정(드래그가 발생할 때)

    [SerializeField]
    private Image ImgItem;          // dragItem 본인의 이미지 컴포넌트 (드래그 대상이 되는 슬롯의 Sprite 이미지가 할당될 얘정)

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void DragSetImage(Image _ImgItem)
    {
        ImgItem.sprite = _ImgItem.sprite;
        SetAlpha(1);
    }

    public void SetAlpha(float _alpha)
    {
        Color color = ImgItem.color;
        color.a = _alpha;
        ImgItem.color = color;
    }



}
