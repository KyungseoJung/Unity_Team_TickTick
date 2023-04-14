using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;     //#3-2

public class DropItemZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)          // 아이템을 땅에 놓기
    {
        //Debug.Log("드롭1");
        if(DragItem.instance.dragStartSlot != null)
        {
            //Debug.Log("드롭2");
            DragItem.instance.dragStartSlot.DropItemOnGround(transform);    // 플레이어 위치 넣기
        }
    }

}
