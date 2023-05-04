using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleJSON;
using System.IO;

using TeamInterface;    //#11-6 인벤토리 데이터 저장하기 위함. Enum_DropItemType



[System.Serializable]   
public class PlayerInfo
{
    public string playerName;
    public string islandName;
    public int clothesNum;
    public Color32 clothesColor;   
}

[System.Serializable]   
public class InventoryInfo  //#11-6
{
    // public int itemIndex;       //n번째 슬롯
    public int /* Enum_DropItemType */ itemType;
    public int itemCount;           //개수
}



public class InfoManager : csGenericSingleton<InfoManager>        //#5-1 플레이어 정보 저장 싱글톤
{
    private PlayerInfo playerInfo;
    private List<InventoryInfo> invenList;  //#11-6
    private InventoryInfo invenInfo;        //#11-6

    private InventoryInfo invenInfo2;       //#11-6 JSON데이터 로드용 - 이거 안 하면, 뭔가 꼬여서 초기화되어버림    
    // public void Print()
    // {
    //     InventoryInfo abc = new InventoryInfo();
    //     abc.itemType = Enum_DropItemType.NONE;
    //     abc.itemCount = 0;

    //     SetInvenInfo(0, abc);
    // }
    // private static InfoManager info = null; //싱글톤 객체(인스턴스)
    // public static InfoManager Info          //싱글톤 프로퍼티
    // {
    //     get
    //     {
    //         if(info == null)
    //         {
    //             info = GameObject.FindObjectOfType(typeof(InfoManager)) as InfoManager; 
    //                 //이런 타입을 가진 오브젝트가 있다면, 그 오브젝트를 InfoManager로서 객체화 해라
    //             if(info == null)
    //             {
    //                 info = new GameObject("Singleton_InfoManager", typeof(InfoManager)).GetComponent<InfoManager>();
    //                 DontDestroyOnLoad(info);
    //             }
    //         }
    //         return info;
    //     }
    // }
    
//Start에 적으면 다른 것들보다 늦게 실행돼서 Null 에러 뜬다.
    protected override void Awake()
    {
        base.Awake();

        playerInfo = new PlayerInfo();
        // playerInfo.playerName = "";     //객체를 초기화 해줘야 null Reference 오류가 발생하지 않아
        invenList = new List<InventoryInfo>();  //#11-6
        invenInfo = new InventoryInfo();          //#11-6


        LoadJSONData();

        // Debug.Log("JSON 테스트용 : " + playerInfo.playerName);
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public string playerName
    {
        get { return playerInfo.playerName; }
        set { playerInfo.playerName = value; }
    }

    public string islandName
    {
        get {return playerInfo.islandName; }
        set {playerInfo.islandName = value; }
    }
    public int clothesNum
    {
        get {return playerInfo.clothesNum; }
        set {playerInfo.clothesNum = value; }
    }    
    public Color32 clothesColor
    {
        get { return playerInfo.clothesColor; }
        set {playerInfo.clothesColor = value; }
    }

//#11-6 인벤토리 정보 ===============================
    public InventoryInfo GetInvenInfo(int index)    //#11-6 인벤토리 정보
    {
        // foreach(InventoryInfo aaa in invenList){
        //     // Debug.Log(aaa.itemCount+"////"+aaa.itemType);
        // }
        //  Debug.Log("//#11-6 인벤토리 Get 카운트 : " +invenList.Count);
        //  Debug.Log("//#11-6 인벤토리 Index 번호 : " + index);

        //Debug.Log("//#15-1 invenList.Count : " + invenList.Count);

        if(index >= invenList.Count)
        {
            // Debug.Log("//#11-6  인벤토리 인덱스 범위 초과");
            return null;
        }
        // Debug.Log("//#11-6 InfoManager 클래스의 List1 : " + invenList[index].itemType);
        // Debug.Log("//#11-6 InfoManager 클래스의 List2 : " + invenList[index].itemCount);
        return invenList[index];
    }

    public void SetInvenInfo(/*int index,*/ InventoryInfo invenInfo)    //#11-6
    {
        // if(index >= invenList.Count)
        // {
        //     Debug.Log("//#11-6 인벤토리 인덱스 범위 초과");
        //     return;
        // }
        // invenList[index] = invenInfo;
        invenList.Add(invenInfo);
    }


    public void LoadJSONData()     //JSON 데이터 로드하기(JSON 파일 -> 클래스로)
    {
        
        TextAsset jsonData = Resources.Load<TextAsset>("player_info");
        string StrJsonData = jsonData.text;                             //# 데이터를 문자열로 가져와서
        var json = JSON.Parse(StrJsonData); //배열 형태로 자동 파싱         //# SimpleJSON을 통해 객체로 생성
//플레이어 정보 파싱

        playerInfo.playerName = json["플레이어 이름"].ToString();
        playerInfo.islandName = json["섬 이름"].ToString();
        
        playerInfo.clothesNum = json["옷 종류"].AsInt;
          
        string hex = json["옷 색"].Value;
        Color32 color = HexToColor32(hex);
        playerInfo.clothesColor = color;

// Debug.Log("플레이어 이름" + json["플레이어 이름"].ToString());
// Debug.Log("섬 이름" + json["섬 이름"].ToString());
// Debug.Log("옷 종류" + json["옷 종류"]);
// Debug.Log("테스트용 string hex = json 풍선색 value : " + json["옷 색"].Value );

    }
    
    public void LoadInvenJSONData() //#11-6 리스트 자체는 한번 싹 Clear하고 JSON 데이터로 리스트 값 채워넣기
    {
        invenList.Clear();  //싱글톤 데이터 넣기 전에 안에 싹 비우기

        TextAsset invenJsonData = Resources.Load<TextAsset>("inventory_info");
        string invenStrJsonData = invenJsonData.text;
        var invenJson = JSON.Parse(invenStrJsonData);

        for(int i=0; i<invenJson["인벤토리"].Count; i++)
        {
            invenInfo2 = new InventoryInfo();     
            //invenInfo.itemType = invenJson["인벤토리"][i]["종류"].ToString();
            //#11-6 문자열 데이터 -> ENUM형으로 변환하기 (System선언해서 Enum.Parse 함수 이용해도 O)
            invenInfo2.itemType = /* (Enum_DropItemType)System.Enum.Parse
                    (typeof(Enum_DropItemType), */ invenJson["인벤토리"][i]["종류"].AsInt;
            invenInfo2.itemCount = invenJson["인벤토리"][i]["개수"].AsInt;

            invenList.Add(invenInfo2);   //리스트에 객체 차곡차곡 저장
        }

    }
    public void SaveJSONData()  //데이터 저장. (클래스 -> JSON 파일)
    {
        //수정 및 업데이트 - JSON 파일에 저장하기
            // 수정된 데이터를 JSON 파일에 저장하기
        JSONObject json = new JSONObject();

        // 플레이어 정보     ===========================
        json.Add("플레이어 이름", playerInfo.playerName);
        json.Add("섬 이름", playerInfo.islandName);
        json.Add("옷 종류", playerInfo.clothesNum);

        Color32 color = playerInfo.clothesColor;
        string hex = ColorToHex(color);
        json.Add("옷 색", hex);

        // JSON 파일로 저장     ===========================
        string jsonString = json.ToString();
        System.IO.File.WriteAllText(Application.dataPath + "/Resources/player_info.json", jsonString);
    }

    public void SaveInvenJSONData() //#11-6
    {
        JSONObject invenJson = new JSONObject();

        //인벤토리 정보 ========================
        JSONArray invenArray = new JSONArray();
        foreach(InventoryInfo inven in invenList)
        {
            JSONObject invenObject = new JSONObject();
            invenObject.Add("종류", inven.itemType); //.ToString()); // ENUM형을 문자열로 변환
            invenObject.Add("개수", inven.itemCount);

            invenArray.Add(invenObject);


            Debug.Log("//#11-7 저장되는중~ : " + inven.itemType + "//" + inven.itemCount);
        }
        invenJson.Add("인벤토리", invenArray);

        // JSON 파일로 저장     ===========================
        string invenJsonString = invenJson.ToString();
        System.IO.File.WriteAllText(Application.dataPath + "/Resources/inventory_info.json", invenJsonString);
        //# 이미 덮어쓰는 코드인가?
    }

    public void InitializeJSONData()    //#9-1 JSON 데이터 초기화하기 - 모두 원래의 null 상태처럼
    {
        playerInfo.playerName = "";     // 그냥 null로 저장하면 안돼. null 자체로 저장이 되어버림!
        playerInfo.islandName = "";
        playerInfo.clothesNum = 0;
        playerInfo.clothesColor = HexToColor32("#7ED67F");

        SaveJSONData();

        //#15-1 플레이어 데이터 삭제하면, 인벤토리 데이터도 삭제되도록
        InitializeInvenJSONData();
    }

    public void InitializeInvenJSONData()   //#11-6 인벤토리 JSON 데이터 초기화 하기
    {
        invenList.Clear();
        invenInfo = new InventoryInfo();

        SaveInvenJSONData();    //초기화 한 걸로 싱글톤 데이터에 싹 넣기
    }


    private Color32 HexToColor32(string hex)
    {
        // HEX 문자열을 RGB 값으로 분리
        byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

        // Color32로 변환하여 반환
        return new Color32(r, g, b, 255);
    }

    private string ColorToHex(Color32 color)
    {
        // R, G, B 값을 HEX 문자열로 변환
        string r = color.r.ToString("X2");
        string g = color.g.ToString("X2");
        string b = color.b.ToString("X2");

        // '#' 문자열과 결합하여 반환
        return "#" + r + g + b;
    }
}

/*
    public TextAsset jsonData = null;
    public string StrJsonData = null;

    void Start()
    {
        jsonData = Resources.Load<TextAsset>("player_info");
        StrJsonData = jsonData.text;
        var json = JSON.Parse(StrJsonData); //배열형태로 자동 파싱.

//플레이어 정보 파싱
        PlayerInfo playerInfo = new PlayerInfo();

        playerInfo.playerName = json["플레이어 이름"].ToString();
        playerInfo.playType = json["플레이 타입"].ToString();
        
        playerInfo.skills = new List<string>();
        for(int i=0; i<json["Inhale/ Exhale 스킬 적용 범위"].Count; i++)
        {
            playerInfo.skills.Add(json["Inhale/ Exhale 스킬 적용 범위"][i].ToString());
        }

        playerInfo.topScore = json["스코어 최고 기록"].AsInt;
        playerInfo.numberOfStars = json["스타 개수"].AsInt;

//아이템 정보 파싱
        List<ItemInfo> itemList = new List<ItemInfo>();
        for(int i=0; i<json["옷장"].Count; i++)
        {
            ItemInfo itemInfo = new ItemInfo();
            itemInfo.name = json["옷장"][i]["이름"].ToString();
            itemInfo.type = json["옷장"][i]["타입"].ToString();
            itemInfo.price = json["옷장"][i]["가격"].AsInt;
            itemInfo.description = json["옷장"][i]["설명"].AsInt;

            itemList.Add(itemInfo);
        }
        
        //파싱된 정보 출력
        Debug.Log("플레이어 정보: " + playerInfo.playerName + ", " + playerInfo.playType + ", " + 
                  string.Join(", ", playerInfo.skills) + ", " + playerInfo.topScore + ", " + 
                  playerInfo.numberOfStars);
        
        foreach(string skill in playerInfo.skills)
        {
            Debug.Log("스킬 : " + skill);
        }
            
        foreach(ItemInfo item in itemList)
        {
            Debug.Log("Name : " + item.name);
            Debug.Log("Type : " + item.type);
            Debug.Log("Price : " + item.price);
            Debug.Log("Description : " + item.description);

        }



    }
*/