using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;       //#19-1

//포톤 추가
public class RoomData : MonoBehaviour   //#19-1 포톤 로비 UX/UI 연결
{
    //외부 접근을 위해 public으로 선언했지만 Inspector에 노출하고 싶지 않을때...
    [HideInInspector]
    //방 이름
    public string roomName = "";

    //현재 접속 유저수
    [HideInInspector]
    public int connectPlayer = 0;

    //룸의 최대 접속자수
    [HideInInspector]
    public int maxPlayers = 0;

//#10-1 해쉬테이블 ===========================
    public Image imgIsSecret;
    public GameObject entranceRoomPW; // 방 입장할 때 적는 비밀번호 InputField
    public InputField inputentranceRoomPW;
    
    [HideInInspector]
    public bool isSecret = false;

    [HideInInspector]
    public ExitGames.Client.Photon.Hashtable myRoomHashT;

// ===========================

    //룸 이름 표시할 Text UI 항목 연결 레퍼런스
    public Text textRoomName;
    //룸 최대 접속자 수와 현재 접속자 수를 표시할 Text UI 항목 연결 레퍼런스
    public Text textConnectInfo;

    //룸 정보를 전달한 후 Text UI 항목에 룸 정보를 표시하는 함수 
    public void DisplayRoomData()
    {
        textRoomName.text = roomName;
        textConnectInfo.text = "(" + connectPlayer.ToString() + "/" + maxPlayers.ToString() + ")";
    }

//#10-1 비밀방
    public void SetSecretImage(bool _isSecret)
    {
        isSecret = _isSecret;
        if(_isSecret)
        {
            imgIsSecret.color = new Color(0,1,1);
            entranceRoomPW.SetActive(true);   // 비밀번호 치는 창 활성화
        }
        else if(!_isSecret)
        {
            imgIsSecret.color = new Color(1,0,1);
            entranceRoomPW.SetActive(false);   // 비밀번호 치는 창 비활성화
        }
    }

    public bool CheckPassword() //비밀번호 맞으면 true
    {
        
        string _password = (string)myRoomHashT["password"];
        
        if(_password.Equals(inputentranceRoomPW.text))
        {
            inputentranceRoomPW.text = "";
            return true;
        }
        else
        {
            inputentranceRoomPW.text = "";
            return false;
        }

    }


}
