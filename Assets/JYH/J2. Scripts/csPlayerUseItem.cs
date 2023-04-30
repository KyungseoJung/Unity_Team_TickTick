using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TeamInterface;

public class csPlayerUseItem : MonoBehaviour
{
    public GameObject[] useItems;

    private void Awake()
    {
        for (int i = 0; i < useItems.Length; i++)
        {
            useItems[i].SetActive(false);
        }
    }

    public void SetShowUseItem(Enum_DropItemType type)
    {
        for (int i = 0; i < useItems.Length; i++)
        {
            useItems[i].SetActive(false);
        }

        switch (type)
        {
            case Enum_DropItemType.NONE:
                break;
            case Enum_DropItemType.AXE:
                useItems[0].SetActive(true);
                break;
            case Enum_DropItemType.PICKAXE:
                useItems[1].SetActive(true);
                break;
            case Enum_DropItemType.HOE:
                useItems[2].SetActive(true);
                break;
            case Enum_DropItemType.SHOVEL:
                useItems[3].SetActive(true);
                break;
            case Enum_DropItemType.BLOCKSOIL:
                useItems[4].SetActive(true);
                break;
            case Enum_DropItemType.FRUIT:
                useItems[5].SetActive(true);
                break;
            case Enum_DropItemType.STON:
                useItems[6].SetActive(true);
                break;
            case Enum_DropItemType.WOOD:
                useItems[7].SetActive(true);
                break;
            case Enum_DropItemType.CARROT:
                useItems[8].SetActive(true);
                break;
            case Enum_DropItemType.BLUEPRINTWATCHFIRE:
            case Enum_DropItemType.BLUEPRINTTENT:
            case Enum_DropItemType.HOUSE_CHAIR:
            case Enum_DropItemType.HOUSE_TABLE:
            case Enum_DropItemType.BLUEPRINTWORKBENCH:
                useItems[9].SetActive(true);
                break;

        }
    }
}
