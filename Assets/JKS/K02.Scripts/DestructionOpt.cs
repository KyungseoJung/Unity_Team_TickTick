using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructionOpt : MonoBehaviour //#5-1
{
    static public DestructionOpt instance;
    public Slot destructionOptSlot;           // '파기하기' 선택할 슬롯

// '파기하기' 이미지
    void Start()
    {
        instance = this;
    }

    // public void DragSetImage(Image _ImgItem)
    // {
    //     ImgItem.sprite = _ImgItem.sprite;
    //     SetAlpha(1);
    // }

    // public void SetAlpha(float _alpha)
    // {
    //     Color color = ImgItem.color;
    //     color.a = _alpha;
    //     ImgItem.color = color;
    // }
}
