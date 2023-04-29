using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TeamInterface;

public class Item : MonoBehaviour
{
    public string itemName;     // 아이템 이름
    public Enum_DropItemType ItemType;   // 아이템 유형
    public Sprite itemImage;    //아이템 이미지(Inventory 안에서 띄울 것)
    public GameObject itemPrefab;   //아이템 프리팹 (아이템 생성시 프리팹으로 찍어낼 것)    //실제 아이템을 의미하는 듯
    public int count=1;
    public int maxCount=1;
}