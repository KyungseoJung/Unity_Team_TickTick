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
            GameObject[] tmpPlayer = GameObject.FindGameObjectsWithTag("Player");

            Debug.Log(tmpPlayer.Length +"asdasdasd");

            foreach(GameObject obj in tmpPlayer)
            {
                PhotonView tmpPV = obj.GetPhotonView();

                if (tmpPV != null && tmpPV.isMine)
                {
                    //Debug.Log("드롭2");
                    DragItem.instance.dragStartSlot.DropItemOnGround(obj.GetComponent<PlayerCtrl>().dropItemPos);    // 플레이어 위치 넣기
                }
                else
                {
                    Debug.Log("포톤뷰 미아");
                }
            }            
        }
    }

}
