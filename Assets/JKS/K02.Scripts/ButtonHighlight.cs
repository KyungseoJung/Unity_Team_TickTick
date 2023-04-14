using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
                             // 버튼 하이라이트 효과 //인터페이스 함수는 public으로 선언해야 함
{
    Button button;
    Image buttonImage;
    Color changeColor;
    Color originColor;


    public void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = button.targetGraphic.GetComponent<Image>();

        changeColor = new Color(1.0f, 0.64f, 0.28f);
        originColor = buttonImage.color;
    }    
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.color = changeColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.color = originColor;    //원래의 색깔로 원상복구
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        buttonImage.color = originColor;
    }

    
}
