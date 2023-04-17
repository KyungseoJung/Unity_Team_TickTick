using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleJSON;
using System.IO;


[System.Serializable]   
public class PlayerInfo
{
    public string playerName;
    public string islandName;
    public int clothesNum;
    public Color32 clothesColor;   
}

public class InfoManager : MonoBehaviour        //#5-1 플레이어 정보 저장 싱글톤
{
    private PlayerInfo playerInfo;


    private static InfoManager info = null; //싱글톤 객체(인스턴스)
    public static InfoManager Info          //싱글톤 프로퍼티
    {
        get
        {
            if(info == null)
            {
                info = GameObject.FindObjectOfType(typeof(InfoManager)) as InfoManager; 
                    //이런 타입을 가진 오브젝트가 있다면, 그 오브젝트를 InfoManager로서 객체화 해라
                if(info == null)
                {
                    info = new GameObject("Singleton_InfoManager", typeof(InfoManager)).GetComponent<InfoManager>();
                    DontDestroyOnLoad(info);
                }
            }
            return info;
        }
    }

    void Awake()    //Start에 적으면 다른 것들보다 늦게 실행돼서 Null 에러 뜬다.
    {
        playerInfo = new PlayerInfo();
        // playerInfo.playerName = "";     //객체를 초기화 해줘야 null Reference 오류가 발생하지 않아

        LoadJSONData();

        Debug.Log("JSON 테스트용 : " + playerInfo.playerName);
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


    public void LoadJSONData()     //JSON 데이터 로드하기(JSON 파일 -> 클래스로)
    {
        
        TextAsset jsonData = Resources.Load<TextAsset>("player_info");
        string StrJsonData = jsonData.text;                             //# 데이터를 문자열로 가져와서
        var json = JSON.Parse(StrJsonData); //배열 형태로 자동 파싱         //# SimpleJSON을 통해 객체로 생성



Debug.Log("플레이어 이름" + json["플레이어 이름"].ToString());
Debug.Log("테스트용 string hex = json 풍선색 value : " + json["풍선 색"].Value );


//플레이어 정보 파싱

        playerInfo.playerName = json["플레이어 이름"].ToString();
        playerInfo.islandName = json["섬 이름"].ToString();
        
        playerInfo.clothesNum = json["옷 종류"].AsInt;
          
        string hex = json["옷 색"].Value;
        Color32 color = HexToColor32(hex);
        playerInfo.clothesColor = color;
    }
    
    public void SaveJSONData()  //데이터 저장. (클래스 -> JSON 파일)
    {
        //수정 및 업데이트 - JSON 파일에 저장하기
            // 수정된 데이터를 JSON 파일에 저장하기
        JSONObject json = new JSONObject();

        // 플레이어 정보     ===========================
        json.Add("플레이어 이름", playerInfo.playerName);
        json.Add("플레이 타입", playerInfo.islandName);
        json.Add("옷 종류", playerInfo.clothesNum);

        Color32 color = playerInfo.clothesColor;
        string hex = ColorToHex(color);
        json.Add("옷 색", hex);

        // JSON 파일로 저장     ===========================
        string jsonString = json.ToString();
        System.IO.File.WriteAllText(Application.dataPath + "/Resources/player_info.json", jsonString);
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