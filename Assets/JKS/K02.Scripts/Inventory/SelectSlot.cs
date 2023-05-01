using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TeamInterface;

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

        if (nowUsingSlot.item == null)
        {
            GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerUseUtem(Enum_PlayerUseItemType.HAND);
            Camera.main.GetComponent<csPlayerUseItem>().SetShowUseItem(Enum_DropItemType.NONE);
        }
        else
        {
            switch (nowUsingSlot.item.ItemType)
            {
                case Enum_DropItemType.BLUEPRINTWATCHFIRE://모닥불 도면
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetBluePrintItme(Enum_PreViewType.FIRE);
                    break;
                case Enum_DropItemType.BLUEPRINTTENT://텐트 도면
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetBluePrintItme(Enum_PreViewType.TENT);
                    break;
                case Enum_DropItemType.HOUSE_CHAIR://의자 도면
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetBluePrintItme(Enum_PreViewType.HOUSE_CHAIR);
                    break;
                case Enum_DropItemType.HOUSE_TABLE://책상 도면
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetBluePrintItme(Enum_PreViewType.HOUSE_TABLE);
                    break;
                case Enum_DropItemType.BLUEPRINTWORKBENCH://작업대 도면
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetBluePrintItme(Enum_PreViewType.WORKBENCH);
                    break;
                case Enum_DropItemType.HOE://괭이
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerUseUtem(Enum_PlayerUseItemType.HOE);
                    break;
                case Enum_DropItemType.AXE://도끼
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerUseUtem(Enum_PlayerUseItemType.AXE);
                    break;
                case Enum_DropItemType.PICKAXE://곡괭이
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerUseUtem(Enum_PlayerUseItemType.PICKAXE);
                    break;
                case Enum_DropItemType.SHOVEL://삽
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerUseUtem(Enum_PlayerUseItemType.SHOVEL);
                    break;
                case Enum_DropItemType.BLOCKSOIL://흙
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerUseUtem(Enum_PlayerUseItemType.BLOCKSOIL);
                    break;
                case Enum_DropItemType.FRUIT://사과
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerHand(Enum_DropItemType.FRUIT);
                    break;
                case Enum_DropItemType.STON://돌
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerHand(Enum_DropItemType.STON);
                    break;
                case Enum_DropItemType.WOOD://나무
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerHand(Enum_DropItemType.WOOD);
                    break;
                case Enum_DropItemType.CARROT://당근
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerHand(Enum_DropItemType.CARROT);
                    break;
            }
            Camera.main.GetComponent<csPlayerUseItem>().SetShowUseItem(nowUsingSlot.item.ItemType);
        }        
        
        if(_show&& !this.gameObject.activeSelf) //하이라이트 true인데 비활성화 상태라면
            this.gameObject.SetActive(_show);   //하이라이트 보여주기
    }

    
    public Enum_DropItemType GetSelectSlotItemType()
    {
        return nowUsingSlot.item.ItemType;
    }

    public void ReSetShowItem()
    {
        if (nowUsingSlot.item == null)
        {
            GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerUseUtem(Enum_PlayerUseItemType.HAND);
            Camera.main.GetComponent<csPlayerUseItem>().SetShowUseItem(Enum_DropItemType.NONE);
        }
        else
        {
            switch (nowUsingSlot.item.ItemType)
            {
                case Enum_DropItemType.BLUEPRINTWATCHFIRE://모닥불 도면
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetBluePrintItme(Enum_PreViewType.FIRE);
                    break;
                case Enum_DropItemType.BLUEPRINTTENT://텐트 도면
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetBluePrintItme(Enum_PreViewType.TENT);
                    break;
                case Enum_DropItemType.HOUSE_CHAIR://의자 도면
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetBluePrintItme(Enum_PreViewType.HOUSE_CHAIR);
                    break;
                case Enum_DropItemType.HOUSE_TABLE://책상 도면
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetBluePrintItme(Enum_PreViewType.HOUSE_TABLE);
                    break;
                case Enum_DropItemType.BLUEPRINTWORKBENCH://작업대 도면
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetBluePrintItme(Enum_PreViewType.WORKBENCH);
                    break;
                case Enum_DropItemType.HOE://괭이
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerUseUtem(Enum_PlayerUseItemType.HOE);
                    break;
                case Enum_DropItemType.AXE://도끼
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerUseUtem(Enum_PlayerUseItemType.AXE);
                    break;
                case Enum_DropItemType.PICKAXE://곡괭이
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerUseUtem(Enum_PlayerUseItemType.PICKAXE);
                    break;
                case Enum_DropItemType.SHOVEL://삽
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerUseUtem(Enum_PlayerUseItemType.SHOVEL);
                    break;
                case Enum_DropItemType.BLOCKSOIL://흙
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerUseUtem(Enum_PlayerUseItemType.BLOCKSOIL);
                    break;
                case Enum_DropItemType.FRUIT://사과
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerHand(Enum_DropItemType.FRUIT);
                    break;
                case Enum_DropItemType.STON://돌
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerHand(Enum_DropItemType.STON);
                    break;
                case Enum_DropItemType.WOOD://나무
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerHand(Enum_DropItemType.WOOD);
                    break;
                case Enum_DropItemType.CARROT://당근
                    GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>().SetPlayerHand(Enum_DropItemType.CARROT);
                    break;
            }
            Camera.main.GetComponent<csPlayerUseItem>().SetShowUseItem(nowUsingSlot.item.ItemType);
        }
    }

}
