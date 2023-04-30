using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;   //텍스트, 버튼 가져오기 목적

using TeamInterface;


public enum RecipeType
{
    CAMPFIRE,
    CHAIR,
    TABLE,
    TENT,
    WORKBENCH,
    AXE,
    HOE,
    PICKAXE,
    SHOVEL
}
public class CraftingRecipe : MonoBehaviour   //#12-1 제작대 레시피
{
    [SerializeField]
    private RecipeType recipeType = RecipeType.CAMPFIRE; //인스펙터 창에서 선택하도록

    [SerializeField]
    private int needEle1Count;  //레시피에 필요한 1번째 아이템의 개수
    [SerializeField]
    private int needEle2Count;  //레시피에 필요한 2번째 아이템의 개수

    private Enum_DropItemType craftItemType;    // 레시피로 만들어질 아이템 - recipeType에 따라 다르게 입력

// 화면에 띄울 텍스트
    public Text txtEle1Count;    // 처음에만 테스트 목적으로 public
    public Text txtEle2Count;
// 화면에 띄울 '제작' 버튼
    public GameObject objStartCraft; // 처음에만 테스트 목적으로 public // 활성화, 비활성화 목적(활성화, 비활성화는 오브젝트 통째로 이용하는 게 정석이라길래)
    public Button btnStartCraft;    // 함수 연결 목적
// 참조 함수용
    GameObject child0;
    GameObject child1;
    GameObject child2;
// 아이쳄의 개수를 찾기 위해
    public Inventory inventory; //처음에만 테스트 목적으로 public
    private int haveEle1Count;
    private int haveEle2Count;

    void Awake()
    {
        child0 = transform.GetChild(0).gameObject;
        child1 = transform.GetChild(1).gameObject;
        child2 = transform.GetChild(2).gameObject;

        txtEle1Count = child0.transform.GetChild(1).gameObject.GetComponent<Text>();
        txtEle2Count = child1.gameObject.transform.GetChild(1).gameObject.GetComponent<Text>();

        objStartCraft = child2.gameObject.transform.GetChild(0).gameObject; // 활성화, 비활성화 목적
        btnStartCraft = objStartCraft.GetComponent<Button>();   // 함수 연결 목적

        //텍스트1, 텍스트2, 버튼 동적으로 연결해주기
        // txtEle1Count = transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.GetComponent<Text>();
        // txtEle1Count = transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.GetComponent<Text>();
        // objStartCraft = transform.GetChild(2).gameObject.transform.GetChild(0).gameObject;

        inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
    }

    void Start()
    {
        btnStartCraft.onClick.AddListener(StartCraftingItem);   // 각 레시피 [제작] 버튼에 함수 연결 - 동적으로 하는 게 빠르니까

        switch(recipeType)
        {
            case RecipeType.CAMPFIRE : 
                needEle1Count = 1;
                needEle2Count = 1;
                craftItemType = Enum_DropItemType.BLUEPRINTWATCHFIRE;
                break;
            case RecipeType.CHAIR : 
                needEle1Count = 1;
                needEle2Count = 2;
                craftItemType = Enum_DropItemType.HOUSE_CHAIR;
                break;
            case RecipeType.TABLE : 
                needEle1Count = 2;
                needEle2Count = 1;
                craftItemType = Enum_DropItemType.HOUSE_TABLE;
                break;
            case RecipeType.TENT :
                needEle1Count = 2;
                needEle2Count = 2;
                craftItemType = Enum_DropItemType.BLUEPRINTTENT;
                break;
            case RecipeType.WORKBENCH : 
                needEle1Count = 3;
                needEle2Count = 3;
                craftItemType = Enum_DropItemType.BLUEPRINTWORKBENCH;
                break;
            case RecipeType.AXE : 
                needEle1Count = 1;
                needEle2Count = 1;
                craftItemType = Enum_DropItemType.AXE;
                break;
            case RecipeType.HOE : 
                needEle1Count = 1;
                needEle2Count = 2;
                craftItemType = Enum_DropItemType.HOE;
                break;
            case RecipeType.PICKAXE : 
                needEle1Count = 2;
                needEle2Count = 1;
                craftItemType = Enum_DropItemType.PICKAXE;
                break;
            case RecipeType.SHOVEL : 
                needEle1Count = 2;
                needEle2Count = 2;
                craftItemType = Enum_DropItemType.SHOVEL;
                break;
        }
        // inventory.UpdateItemInvenData();
        
        Invoke("CheckRecipeState", 2.0f);   //# 이 함수가 Inventory.cs 대부분의 함수들보다 먼저 실행되어서 null로 뜨는 문제가 발생함
        // CheckRecipeState();
    }

    // 현재 만들 수 있는 레시피들 확인하기 - 아이템의 개수 Text(불가능 : 빨간색, 가능 : 초록색), 제작 버튼(불가능 : 안 보여, 가능 : 클릭 가능)
    public void CheckRecipeState() 
    {
        haveEle1Count = inventory.CheckItemCount(Enum_DropItemType.STON);
        haveEle2Count = inventory.CheckItemCount(Enum_DropItemType.WOOD);

// 텍스트 띄우기
        txtEle1Count.text = haveEle1Count + "/" + needEle1Count;
        txtEle2Count.text = haveEle2Count + "/" + needEle2Count;

// 텍스트 색 - 1번째 아이템 요소
        if(haveEle1Count >= needEle1Count)
            txtEle1Count.color = Color.green;
        else
            txtEle1Count.color = Color.red;

// 텍스트 색 - 2번째 아이템 요소
        if(haveEle2Count >= needEle1Count)
            txtEle2Count.color = Color.green;
        else
            txtEle2Count.color = Color.red;


// 버튼 활성화/ 비활성화
        if(haveEle1Count >= needEle1Count)
        {
            if(haveEle2Count >= needEle2Count)
            {
                // 둘다 만족하면 버튼 활성화
                objStartCraft.SetActive(true);
            }
        }
        else    // 둘 중 하나라도 만족 못 하면 버튼 비활성화
            objStartCraft.SetActive(false);

    }

    void OnEnable() // 비활성화에서 활성화 될 때, 매번 호출되는 콜백함수
    {
        Debug.Log("//#12-1 OnEnable 호출 확인");
        StartCraftingItem();
    }

    void StartCraftingItem()    //버튼에 연결 - 동적으로 하는 게 빠르겠다.
    {
        // 아이템 개수 감소 - Slot 스크립트 내 UpdateSlotCount 함수 이용

        // 제작 결과 아이템도 수집되도록 - Inventory 내 CollectItem 함수 이용
        inventory.CollectItem(craftItemType);      //1개만 만들어

        CheckRecipeState();
    }


 

}
