using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//(UI 버전에서 사용)
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*
 * 포톤 네트워크 게임 엔진
 * 1) 제품 종류는 크게 Photon Server/ Photon PUN 으로 나뉨 (참고 : Photon Cloude=>Photon PUN으로 이름이 바뀜) 
 *  ■ Photon Server : 물리적인 서버를 직접 운영하는 것으로서 운영하려면 서버의 보안 및 백업/네트워크 트래픽/로드밸런싱/유지보수 등의
 *  관리를 위한 전문 네트워크 관련 인력이 필요함 
 *  (제품 종류 <Photon Server 계열> : Photon Server)
 *  
 *  ■ Photon PUN    : 우리가 사용할 방식으로 소프트웨어를 임대해 사용하는 방식인 SaaS(Software as a Service)의 개념으로 
 *  서버를 임대하여 사용함으로써 Photon Server의 제한적 사항을 전혀 신경 쓰지 않아도 됨
 *  또한 Photon PUN(Photon Unity Networking)은 게임 개발 테스트를 위하여 동시 접속 사용자( CCU(Concurrent User) )수 20명까지 무료로 제공
 *  (제품 종류 <Photon Cloud 계열> : Photon PUN(우리가 사용할 제품), Photon RealTime, Photon Bolt, Photon Thunder, Photon TrueSync,
 *   Photon Chat, Photon Voice)
 *  
 *  ■ 비교
 *                           Photon Server                                Photon PUN
 *  라이선스                 추가 서버당 과금 체계                        동시 접속 사용자(CCU)별 과금 체계
 *  서버 운영 및 관리            ○                                           ×
 *  서버 사이드 게임 로직    커스텀 마이징((EX) DB 서버 연동) 가능              ×
 *  로드밸런싱(확장성)       직접 관리(병목현상을 줄여주는)                     ×
 *  
 *  cf) 네트워크 병목 현상 : 처리량 이상의 네트워크 트래픽이 몰리거나 특정 서버에 트래픽이 집중되는 경우 
 *      네트워크 병목 현상이 발생함.=>로드밸런싱으로 병목현상을 줄여줌(스케줄링,라운드로빈,해싱)
 *      
 *      로드밸런싱 : 여러대의 서버를 미리 준비해 작업을 분산해두면 병목 현상을 예방할 수 있다. 이렇게 부하를 분산하는
 *      방식을 로드밸런싱 이라고 한다.(Photon Server) 
 *      
 *      클라우드 로드밸런싱 : 클라우드에서 인스턴스(가상 서버)로 들어오는 트래픽을 자동 분산해
 *      트래픽 병목 현상을 예방하고 네트워크가 효율적으로 동작하도록 하는 것을 말한다.(Photon PUN)
 *      
 * 2) 회원가입 : 포톤 클라우드 서비스를 위해 http://www.exitgames.com 에서 회원가입을 하자~
 *      접속 => 신규 가입 메뉴 => 회원가입 페이지 => 이메일 주소 입력 => 가입 완료 => 가입 시 입력한
 *      이메일로 Confirm 메일 수신 및 (비밀번호 설정)링크 확인 후 계정 비밀번호 설정 => 계정 만들기 클릭
 *      => 메뉴에 사용자 설정(사람모양) 클릭 => 퍼블릭 클라우드(어플리케이션 리스트) 클릭
 *      => 가입하자마자 바로 사용 할 수 있는 Application Id(게임당 하나씩 부여되는 고유넘버로서 
 *      포톤 서버와 통신하기 위함)가 자동으로 할당 즉 자동으로 Apllication 정보가 생성됨(우리 게임에 연동해야함)
 *      => (포톤 클라우드를 이용하기 위해) 메뉴에 제품 클릭 => PUN 클릭 => Photon Unity Networking (플러그인) 에셋 스토어 패키지 에서 
 *      PUN FREE 다운 및 PUN 플러그인 설치 (유니티 에셋스토어 에서 다운 가능 : PUN(Photon Unity Networking) )
 *      
 *  3) PUN(Photon Unity Networking) 플러그인 설정
 *     PUN 플러그인 임포트 완료시 또는 메뉴 => Window => Photon Unity Networking => PUN WIzard 클릭 시 PUN WIzard 창 오픈
 *     PUN WIzard(여기서 회원가입 가능 => Cloud Dashboard Login)에서 => Setup Project 클릭 => AppId(포톤 사이트에 등록된...)  or Email 입력(=>Cloud Dashboard Login 후 AppId 얻어와서 셋팅) 
 *     후 => Setup Project 클릭 
 *     =>
 *     PhotonServerSettings 플러그인 세팅을 자동 선택 및 인스펙터의 요소들이 나열 된다.(나중에 직접 수정시..폴더 경로로 찾아가도 되지만..
 *     PUN Wizard => Locate PhotonServerSettings 클릭으로 찾아가도 됨)
 *     
 *     =>
 *     인스펙터의 속성 중 Hosting 속성 : 각 옵션을 선택하여 포톤 구동 방식을 선택하는 것으로  Photon Cloude(우리가 사용할 Photon PUN), Selef Hosted(Photon Server : 물리적인 서버)
 *     정도로 나뉘며 각 항목중 Photon Cloude 옵션 선택시 하단에 Region 속성이 활성화 된다. 이것은 현재 대륙별 서버를ㄴ 운영 중인 지역을 선택 하는것으로 가장 가까운 서버를 선택해야 함
 *     또는 Hosting 옵션중에 Best Region 선택시 현재 App 접속 지역을 기준으로 각 대륙 서버로 Ping 테스트를 한다(RTT : Round Trip Time) 즉 가장 빠른 속도로 응답이 오는 지역 서버로
 *     자동 접속된다.(우린 이걸 사용~!) 또한 하단에 Enabled Regions 설정으로 대륙을 선택 할 수 있다. 즉, Ping 테스트를 제외 시킬수 있다.(그냥 Everything 으로 셋팅!)
 *     Protocol : udp/tcp 중 우리 게임에 유리한 udp 프로토콜 셋팅!!! 
 *     Client Settings => Auto-Join Lobby 체크시 : 마스터 접속후 자동으로 로비로 조인한다.  => PUN 플러그인 설정 완료!!!
 *     
 *     (참고) PUN 플러그인은 Project 뷰 => Photon Unity Networking 안에 설치된다..또한 PhotonServerSettings 플러그인은 => Resources 안에 저장 되어있다. PhotonServerSettings 클릭시
 *     인스펙터 뷰에 포톤 클라우드 접속 정보 및 RPC 목록을 확인 할 수 있다. 
 *     즉, 인스펙터 뷰에 호스팅 방법(포톤 서버, 포톤 클라우드, 오프라인 모드), AppId(Application Id), Protocol 정보를 확인 및 수정할 수 있다. 
 *     
 *     % RPC( 원격 프로시저 호출, Remote Procedure Call, 리모트 프로시저 콜) ??? 
 *     별도의 원격 제어를 위한 코딩 없이 다른 주소 공간에서 함수나 
 *     프로시저(프로시저는 특정 작업을 수행하기 위한 프로그램의 일부== (프로그래밍에선) 함수)를 실행할 수 있게하는 프로세스(컴퓨터 내에서 실행중인 프로그램) 간 통신 기술이다.
 *     
 *     
 *     
 */

/* 포톤 클라우드의 주요 흐름
 * 1. 서버접속
 * 2. 로비접속
 * 3. 방으로 접근
 * 4. 방으로 접근시도후 실패시 방만듬
 * 5. 방입장
 * 6. "방장"의 명령에 따라 Object들을 Sync
*/

/*
 * 포톤 클라우드 콜백 함수 
 * 
 * 1 서버 접속
 * 
 * Method (PhotonNetwork) 
 * PhotonNetwork.ConnectUsingSettings(string version); 
 *
 * Event :서버접속후 발생하는 이벤트
 * void OnConnectedToPhoton(){ ~이벤트처리 로직~ }=> Event들을 이렇게 사용
 * 
 * - OnConnectedToPhoton : 포톤에 접속되었을 때 
 * - OnLeftRoom : 방에서 나갔을 때
 * - OnMasterClientSwitched : 마스터클라이언트가 바뀌었을 때
 * - OnPhotonCreateRoomFailed : 방만들기 실패
 * - OnPhotonJoinRoomFailed : 방에 들어가기 실패
 * - OnCreatedRoom : 방이 만들어 졌을 때
 * - OnJoinedLobby : 로비에 접속했을 때
 * - OnLeftLobby : 로비에서 나갔을 때
 * - OnDisconnectedFromPhoton : 포톤 접속 종료
 * - OnConnectionFail : 연결실패 void OnConnectionFail(DisconnectCause cause){ ... }
 * - OnFailedToConnectToPhoton : 포톤에 연결 실패 시  void OnFailedToConnectToPhoton(DisconnectCause cause){... }
 * - OnReceivedRoomList : 방목록 수신시 
 * - OnReceivedRoomListUpdate : 방목록 업데이트 수신시
 * - OnJoinedRoom : 방에 들어갔을 때 
 * - OnPhotonPlayerConnected : 다른 플레이어가 방에 접속했을 때 void OnPhotonPlayerConnected(PhotonPlayer newPlayer){ ... }
 * - OnPhotonPlayerDisconnected : 다른 플레이어가 방에서 접속 종료시  void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer){ ... }
 * - OnPhotonRandomJoinFailed : 렌덤하게 방으로 입장하는게 실패했을 때
 * - OnConnectedToMaster : 마스터로 접속했을 때   "Master_Join"
 * - OnPhotonSerializeView : 네트워크싱크시  void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){ ...}
 * - OnPhotonInstantiate : 네트워크 오브젝트 생성시 void OnPhotonInstantiate(PhotonMessageInfo info){ ... } "NetworkObj"
 * 
 * 2 로비관련 :로비 접속에 성공하면, 이제부터는 방목록을 얻어 온다던가... 아니면 방을 만든다던가... 이것도 아니라면 아무 방에 들어가기 위한 노력
 * 
 * Method
 *  RoomInfo [] PhotonNetwork.GetRoomList ( ) //방목록을 가져옴
 *  void PhotonNetwork.CreateRoom ( string roomName ) //방을 이름으로만 만듬
 *  void PhotonNetwork.CreateRoom ( string roomName, bool isVisible, bool isOpen, int maxPlayers ) //방을 만들되 이름, 보임여부, 오픈여부, 최대 플레이어 수를 지정함.
 *  void PhotonNetwork.JoinRandomRoom ( ) //아무방이나 들어감.              //#18 수업시간에 해본 것
 *  void PhotonNetwork.JoinRoom ( string roomName ) //지정한 방으로 들어감.
 *  [RPC]도 호출이 가능하니 로비에서 채팅 가능
 *  
 *  
 * 3 방에서 
 * 
 * Method 
 * //resources폴더에 있는 prefab의 이름으로 게임오브젝트를 생성.
 * GameObject PhotonNetwork.Instantiate ( string prefabName, Vector3 position, Quaternion rotation, int group) 
 * //씬에 종속된 게임오브젝트를 생성.(마스터만이 네트워크 게임오브젝트를 생성가능함)
 * GameObject PhotonNetwork.InstantiateSceneObject ( string prefabName, Vector3 position, Quaternion rotation, int group, object[] data ) 
 * 
 * 위 2개의 Method 차이는 플레이어에 종속된 GameObject를 만들것인가, 플레이어의 접속이 끝나도 계속 살아남을 Object를 만들것인가...
 * 
 * Event :방에서 PhotonView로 설정한 객체는 서로 데이터를 Sync할 수 있다. 이때 발생하는 이벤트는 딱 1개라고 생각해도 됨
 * void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){ ...}
 * 
 * 
 * 네트워크상에서의 동기화는 다음과 같이 수행됨.: 게임오브젝트에 반드시 PhotonView(Component)가 추가 되야한다.
 *
 *   void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
 *       {
 *           if (stream.isWriting)
 *           {
 *               stream.SendNext(transform.position);
 *               stream.SendNext(transform.rotation);
 *           }
 *           else
 *           {
 *               this.correctPlayerPos = (Vector3)stream.ReceiveNext();
 *               this.correctPlayerRot = (Quaternion)stream.ReceiveNext();
 *           }
 *   }
 *   
 * 위 코드에서 사용가능한 데이터형
 * - Bool
 * - Int
 * - string
 * - char
 * - short
 * - float
 * - PhotonPlayer
 * - Vector3
 * - Vector2
 * - Quaternion
 * - PhotonViewID
 */

public class csPhotonInit : MonoBehaviour { //#6-1 팀플 포톤 //#19-2 (UI버전에서 사용) 주석 추가 - 동적 생성한 방 띄우기 & 입장하기까지

    //App의 버전 정보 (번들 버전과 일치 시키자...)
    public string version = "Ver 0.1.0";

    //개발하는 동안 PUN 으로 개발하는 것이 처음이면 최대한 Unity 콘솔에 로그를 많이 찍어 어떤 
    //사항이 발생하는지 파악하는 것을 권장. 
    //예상하는 대로 동작하는 것에 대하여 확신이 서면 로그 레벨을 Informational 으로 변경 하자.
    public PhotonLogLevel LogLevel = PhotonLogLevel.Full;   //#18 콘솔창으로 모든 정보를 다 보겠다

    //플레이어의 이름을 입력하는 UI 항목 연결을 위한 레퍼런스 (using UnityEngine.UI 추가해야함) (UI 버전에서 사용)
    private string userId;   //#6-1 JSON에서 데이터 가져올 거니까 string으로 지정. 형식도 바꿔주기(아래에서 코드 변경함. .text 지워)    //public InputField userId;
    //룸 이름을 입력받을 UI 항목 연결 레퍼런스 (UI 버전에서 사용)
    public InputField roomName;
    //RoomItem이 차일드로 생성될 Parent 객체의 레퍼런스 (UI 버전에서 사용)
    public GameObject scrollContents;
    //룸 목록만큼 생성될 RoomItem 프리팹 연결 레퍼런스 (UI 버전에서 사용)
    public GameObject roomItem; //ImgRoomItem 프리팹 연결 - CreateRoom 하면 다른 플레이어들도 들어올 수 있도록

//#10-1 ==================================
// 룸 속성 설정할 해쉬테이블 저장
    [Space(20)]
    [Header(" 해쉬테이블 ======= ")]
    [HideInInspector]
    public ExitGames.Client.Photon.Hashtable targetHash;

    [Space(20)]
    [Header(" 방 생성")]
    public Toggle tgIsScretRoom;  //비밀방인지 체크하는 토글
    public InputField createRoomPassWord;
    private RoomData targetRoom;    //public이어야 하나?

//#10-1 ==================================
    [Space(20)]
    [Header(" 방 입장")]
    // public InputField inputEntranceRoomPassWord; // 방 입장할 때 적을 비밀번호

// ==================================
    //플레어의 생성 위치 저장 레퍼런스
    public Transform playerPos;

    //App 인증 및 로비연결
    void Awake()
    {
        /*
         * 포톤 클라우드
         * 
         * 포톤 클라우드는 로비/룸의 개념이 있다. 포톤 서버에 접속에 성공하면 로비에 자동 입장 된다.
         * 로비에 입장한 다음 룸을 생성하면 다수의 유저들이 해당 룸에 입장하여 네트워크 게임을 진행하는 방식
         * 이며, 처음 룸을 생성한 유저가 Master Client가 된다.=> 우리가 알고있는 방장에 해당하는 역할(몬스터 스폰)과 권한을 갖는다.(부여된다..)
         * 유니티 빌트인 네트워크 API를 사용하여 만든 네트워크 온라인 게임은 실행중 Server Peer(방장)의 접속이 끊기면 
         * 게임 자체가 성립이 안된다. 그러나 포톤 클라우드의 경우 Master Client가 룸에서 나가면 다음으로 입장한 
         * 다른 네트워크 유저가 Master Client를 인계받는다. 따라서 유니티 빌트인 네트워크 API를 이용한 네트워크 게임 처럼
         * 게임이 도중에 없어져서 중단되지 않는다.
         */
        // 룸 -> 로비로 씬 전환시 반복해서 다시 포톤 클라우드 서버로 접속하는 로직이 다시 실행되는것을 막음
        if (!PhotonNetwork.connected)
        {
            //버전 정보를 전달하며 포톤 클라우드에 접속 ( 과정: 지역 서버 접속 -> 사용자 인증 -> 로비 입장 )
            //포톤서버 접속 시도 => 인증 => 포톤서버 접속 => 포톤 마스터서버 접속 시도 => 인증 => 포톤 마스터서버 접속 및 Lobby 조인
            //인자로 전달하는 version은 동일 버전의 게임이 설치된 유저만 같은 로비에 접속할 수 있다(1.0 버전의 게임이 설치된
            //유저와 2.0 버전의 게임이 설치된 유저는 네트워크상에서 만날 수 없다 . 따라서 서로 다른 버전의 게임으로 인한 
            //error 및 오동작을 일으실 수 있는것을 원천적으로 막는다.) 
            //EX) 1.0 버전 총 Damage 100  2.0 버전 총 Damage 200)   //#18 버전이 바뀌면서 총의 타격감이 바뀌었다면~ 총의 타격감이 같은 유저들끼리만 만나서 게임해야겠지
            PhotonNetwork.ConnectUsingSettings(version);

            //개발하는 동안 PUN 으로 개발하는 것이 처음이면 최대한 Unity 콘솔에 로그를 많이 찍어 어떤 
            //사항이 발생하는지 파악하는 것을 권장. 
            //예상하는 대로 동작하는 것에 대하여 확신이 서면 로그 레벨을 Informational 으로 변경 하자.
            PhotonNetwork.logLevel = LogLevel;


            //현재 클라이언트 유저의 이름을 포톤에 설정
            //PhotonView 컴포넌트의 요소 Owner의 값이 된다.
            PhotonNetwork.playerName = "GUEST " + Random.Range(1, 9999);    //#18 실제 게임에서는 JSON데이터로 연결해줘야 겠지

        }

        // 현재 접속 지역을 기준으로 핑 타임이 가장 빠른 클라우드 서버에 자동으로 접속 (RTT (Round Trip Time) )
        // PhotonNetwork.ConnectToBestCloudServer( version );

        // 특정 클라우드 서버에 직접 접속 하는 함수로, 인자는 포톤 클라우드 서버 IP 주소, Port 번호, AppID, 버전    //# 쓸 일 거의 없대
        // PhotonNetwork.ConnectToMaster( "string serverAddress", 3306, "asdafasdda01091207", version  );

         //룸 이름을 무작위로 설정 (UI 버전에서 사용)
         roomName.text = "ROOM_" + Random.Range(0,999).ToString("000");
         // ScrollContents의 Pivot 좌표를 Top, Left로 설정 하자. (UI 버전에서 사용) 
         scrollContents.GetComponent<RectTransform>().pivot = new Vector2(0.0f, 1.0f);  //# 피봇 동적 조작 가능
    }

    void Start()
    {
        roomName.onValueChanged.AddListener(OnInputRoomNameChange);
    }

    void OnInputRoomNameChange(string _input)
    {
        if(_input.Length > 3)
        {
            roomName.text = _input.Substring(0,3);      // 3글자 넘어가도 값이 바뀌지 않도록
        }
    }
    /*
     * 포톤은 로비 기반으로 서버가 이루어져 있음 따라서 연결은 로비로 연결되는 것임
     * 로비에서 방을 만들거나, 특정 방에 연결하거나, 랜덤으로 열결하게 됨
     */
    //포톤 클라우드에 정상적으로 접속한 후 로비에 입장하면 호출되는 콜백 함수
    //즉, 로비 입장 후 포톤 클라우드 서버가 클라이언트에게 정상적으로 로비에 입장했다는 콜백 함수를 호출~
    void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby !!!");

        /*
         * 클라이언트의 룸 찾기 : 클라이언트는 게임(방) 이름을 통해 참여하거나 Photon 에게 나에게 맞는 게임을 찾아 달라고 
         * 요청 하여 참여한다.
         *  •무작위: 무작위로 플레이어를 매칭. 마음대로 플레이어들을 룸에 입장 시키거나 동등하게 분배 후 입장하게 함.
         *  •필터: 좀 더 나에게 맞도록 매칭 시키기 위하여 무작위 매칭에 필터를 사용.
         *  •리스팅: 플레이어는 로비에 나열된 룸 중에서 하나를 선택 후 입장.
          * •파라미터화: 예측되는 속성 정의를 통한 커스터마이징된 무작위 매칭
         */
        
        // 로비 입장후 이미 생성된 룸(방) 중에서 무작위로 선택해 입장하는 (Random Match Making) 함수
//        PhotonNetwork.JoinRandomRoom(); // (UI 버전에서는 주석 처리)
//        PhotonNetwork.JoinRoom("MyRoom");
//        Debug.Log("공부용 : MyRoom 들어가기 성공했나?");

        // 유저 아이디를 가져와 셋팅 (UI 버전에서 사용)
        userId = GetUserId();   //# JSON에 저장된 데이터 가져오기 //userId.text = GetUserId();

/*
* Method
 *  RoomInfo [] PhotonNetwork.GetRoomList ( ) //방목록을 가져옴
 *  void PhotonNetwork.CreateRoom ( string roomName ) //방을 이름으로만 만듬
 *  void PhotonNetwork.CreateRoom ( string roomName, bool isVisible, bool isOpen, int maxPlayers ) //방을 만들되 이름, 보임여부, 오픈여부, 최대 플레이어 수를 지정함.
 *  void PhotonNetwork.JoinRandomRoom ( ) //아무방이나 들어감.              //#18 수업시간에 해본 것
 *  void PhotonNetwork.JoinRoom ( string roomName ) //지정한 방으로 들어감.
 *  [RPC]도 호출이 가능하니 로비에서 채팅 가능
*/
        //특정 조건을 만족하는 룸을 대상으로 무작위로 추출해 입장하는 오버로딩 된 함수 호출 방식 

        /* 사용 방법 : PhotonNetwork.JoinRandomRoom(Hsshtable 룸 속성, byte 최대접속자수);
         * 
         *  (EX 1) 
         *  
         * //using System.Collections; 주석 처리    //# Hashtable 이름 겹치니까 둘 중 하나는 주석처리 해라.~ using 주석처리 하고 나서는 풀로 적어줘야 겠지
         * using ExitGames.Client.Photon;
         * 
         * ...
         * ...
         * 
         * //무작위 추출할 룸의 조건을 Hashtable로 정의
         * //생성된 룸 중 맵은 1번을 사용하고 접속 가능한 레벨은 10 LV로 제한된 속성의 Hsshtable 생성
         * Hashtable  roomProperties = new Hashtable() { { "map", 1 }, { "minLevel", 10 } };
         * 
         * 위에 정의한 룸 속성과 최대 플레이어 수가 7명인 룸을 검색해 선택 랜덤 접속
         * PhotonNetwork.JoinRandomRoom(roomProperties, 7);
         * 
         *  (EX 2) 
         *  
         * using System.Collections;
         * //using ExitGames.Client.Photon; 주석 처리
         *  
         *  생성된 룸 중 맵은 3번을 사용하고 접속 가능한 최소 레벨은 10 LV로 제한된 속성의 Hsshtable 생성
         *  
         *  ExitGames.Client.Photon.Hashtable  roomProperties = new ExitGames.Client.Photon.Hashtable() { { "map", 3 }, { "minLevel", 10 } };
         *  
         *  위에 정의한 제한된 룸 속성과 최대 플레이어 수가 10명인 룸을 검색해 선택 랜덤 접속
         *  PhotonNetwork.JoinRandomRoom(roomProperties, 10);
         */

    }
//# 공부용
    void OnReceivedRoomList()
    {
        Debug.Log("방목록 수신중입니다~");  //호출안됨
    }

    //로컬에 저장된 플레이어 이름을 반환하거나 랜덤 생성하는 함수 (UI 버전에서 사용)
    string GetUserId()  // 이건 원래 있던 함수이고, 안에 내용을 JSON으로 가져오도록 하는 부분만 내가 수정한 것
    {
        //(참고) 구글플레이 연동시 구글 아이디로 유저 아이디 가져오자.
        // string userId = InfoManager.Info.playerName;    //PlayerPrefs.GetString("USER_ID");
        string userId = InfoManager.Ins.playerName;    //PlayerPrefs.GetString("USER_ID"); //#11-6
        Debug.Log("아이디 확인1 : " + userId );

        //유저 아이디가 NULL일 경우 랜덤 아이디 생성 
        if(string.IsNullOrEmpty(userId))
        {
            // 자릿수 맞춰서 반환
            userId = "USER_" + Random.Range(0, 999).ToString("000");
            //공부 하라구 
            //Debug.Log(9999999999.ToString("0,000"));
            //Debug.Log(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            // Debug.Log(System.DateTime.Now.Millisecond.ToString("000"));
            Debug.Log("아이디 확인2 : " + userId );
        }
        Debug.Log("아이디 확인3 : " + userId );

        return userId;
    }

    //포톤 클라우드는 Random Match Making 기능 제공
    //무작위 룸 접속(입장)에 실패한 경우 호출되는 콜백 함수
    void OnPhotonRandomJoinFailed()
    {
        //랜텀 매치 메이킹이 실패한 후 Console 뷰에 나타나는 메시지 설정
        Debug.Log("No Rooms !!!");

        /*
         * 룸 생성 함수
         * PhotonNetwork.CreateRoom( string roomName );
         * PhotonNetwork.CreateRoom( string roomName, RoomOptions roomOptions, TypedLobby typedLobby);
         * PhotonNetwork.CreateRoom( string roomName, RoomOptions roomOptions, TypedLobby typedLobby, string[] expectedUsers);
         */

        //룸(Room) : 동일한 공간(영역에) 접속하여 서로 네트워크 게임을 할 수 있는 Space!!!
        //(CF)포톤 클라우드 모니터링 웹페이지에선 Room을 Game이란 용어로 사용
        //룸을 생성하는 함수 (룸이 없으니깐 내가 방장이되서 룸 생성) 
        bool isSucces = PhotonNetwork.CreateRoom("MyRoom");     //#테스트 주석
        /*
        // 방 정보 옵션
        RoomOptions rOpt = new RoomOptions();

        // 룸 입장 가능한 최대 접속자 수
        rOpt.MaxPlayers = 10;

//#18 IsVisible과 IsOpen 짝꿍! 둘다 true 또는 둘다 false =================

        // 방의 활성 여부(목록 상태) 즉, 로비에 룸 노출 여부
        // 즉 룸 목록에 표시할지 여부(값이 false이면 룸 이름이 외부에 노출되지 않는다)
        rOpt.IsVisible = true; // 공개방

        // 방의 공개 여부 즉, 입장(Join) 가능 여부
        rOpt.IsOpen = true;  //리스트에 보여주세요

        rOpt.CleanupCacheOnLeave = true; //방 떠나면 정보를 날려주세요(사용자가 룸에서 나갔을 때 사용자의 이벤트와 프로퍼티들을 제거)

        //커스텀 룸 속성은 지도 이름, 레벨 또는 라운드 시간 등과 같은 값을 저장.
        //커스텀 프로퍼티에 대한 키는 반드시 문자열 이어야 함. 
        //물론 커스텀 속성 값은 룸안에서도 설정하고 변경할 수 있다.
        //Map이 5번이고 게임 타입이 팀 데쓰 매치 속성으로 설정 ( 맵 = 5번 맵, 게임 타입 = TMD )
        rOpt.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "Map", 5 }, { "GameType", "TDM" } };

        //커스텀 프로퍼티들의 이름 배열을 설정 하여 로비에서 커스텀 프로퍼티들의 표시 여부를 결정 할 수 있다.
        //로비에서 볼 수 있는 프로퍼티들은 매치메이킹과 랜덤 매치에서 필터로 사용될 수 있다. 
        //customProperties의 속성 명칭을 배열로 전달
        rOpt.CustomRoomPropertiesForLobby = new string[] { "Map", "GameType" };

        //TypedLobby.Default 는 새로운 룸이 기본 로비에 나타나는 것을 정의함. TypedLobby.Default는 룸을 나열하며 
        //다른 형태의 매치메이킹으로 사용 될 수 있다. 
        //(생성하려는 룸의 이름)"MyRoom"이란 룸을 생성하면서 룸 옵션(rOpt)에 설정한 정보를 함께 저장한다.
        bool isSucces = PhotonNetwork.CreateRoom("MyRoom", rOpt, TypedLobby.Default);
        */

        /* RoopOptions 클래스를 통해 룸의 속성을 셋팅 1.

        // 방 정보 옵션
        RoomOptions rOpt = new RoomOptions();

        // 룸 입장 가능한 최대 접속자 수
        rOpt.MaxPlayers = 10;

        // 방의 활성 여부(목록 상태) 즉, 로비에 룸 노출 여부
        rOpt.IsVisible = true; // 공개방

        // 방의 공개 여부 즉, 입장 가능 여부
        rOpt.IsOpen = true;  //리스트에 보여주세요

        rOpt.CleanupCacheOnLeave = true; //방 떠나면 정보를 날려주세요(사용자가 룸에서 나갔을 때 사용자의 이벤트와 프로퍼티들을 제거)

        //커스텀 룸 속성은 지도 이름, 레벨 또는 라운드 시간 등과 같은 값을 저장.
        //커스텀 프로퍼티에 대한 키는 반드시 문자열 이어야 함. 
        //물론 커스텀 속성 값은 룸안에서도 설정하고 변경할 수 있다.
        //Map이 5번이고 게임 타입이 팀 데쓰 매치 속성으로 설정 ( 맵 = 5번 맵, 게임 타입 = TMD )
        rOpt.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "Map", 5 }, { "GameType", "TDM" } };

        //커스텀 프로퍼티들의 이름 배열을 설정 하여 로비에서 커스텀 프로퍼티들의 표시 여부를 결정 할 수 있다.
        //로비에서 볼 수 있는 프로퍼티들은 매치메이킹과 랜덤 매치에서 필터로 사용될 수 있다. 
        //customProperties의 속성 명칭을 배열로 전달
        rOpt.CustomRoomPropertiesForLobby = new string[] { "Map", "GameType" };

        //TypedLobby.Default 는 새로운 룸이 기본 로비에 나타나는 것을 정의함. TypedLobby.Default는 룸을 나열하며 
        //다른 형태의 매치메이킹으로 사용 될 수 있습니다. 
        //"MyRoom"이란 룸을 생성하면서 룸 옵션(rOpt)에 설정한 정보를 함께 저장한다.
        bool isSucces = PhotonNetwork.CreateRoom("MyRoom", rOpt, TypedLobby.Default );

        cf) //로비 타입 설정
            TypedLobby lobby = new TypedLobby(); 
            //로비 이름
            lobby.Name = "Test Lobby";
            //로비는 일반적인 로비
            lobby.Type = LobbyType.Default;
            //이 로비에, 이 옵션으로 MyRoom이란 방을 만들어주세요!!!!
            bool isSucces = PhotonNetwork.CreateRoom("MyRoom", rOpt, TypedLobby.Default );

        */

        /* RoopOptions 클래스를 통해 룸의 속성을 셋팅 2.
         
        bool isSucces = PhotonNetwork.JoinOrCreateRoom(
            "MyRoom",
             new RoomOptions()           
             {
                 MaxPlayers = 10,        
                 IsOpen = true,          
                 IsVisible = true,       
             },
            null
        );

        */

        /* ( 추가 설명 )
         * PhotonNetwork.CreateRoom 함수는 위에 두가지 방법 말고 또 하나 오버로딩 되어있다.
         * PhotonNetwork.CreateRoom( string roomName, RoomOptions roomOptions, TypedLobby typedLobby, string[] expectedUsers);
         * 이 오버로딩 함수의 마지막 전달 인자는 함께할 친구들의 접속 아이디 목록이다.한마디로 친구끼리만 게임을 할 경우 사용.
         * PhotonNetwork.JoinRoom(string roomName) 와 PhotonNetwork.JoinRoom(string roomName, string[] expectedUsers); 이 두 함수가
         * 오버로딩 되어 있으며, 후자를 사용해서 접속 할 때 사용
        */

        Debug.Log("[정보] 게임 방 생성 완료 : " + isSucces);        //#테스트 주석
    }

    //룸(방) 만들기에 실패한 경우 호출되는 콜백 함수 
    /* PhotonNetwork.CreateRoom 함수로 룸 생성시 실패한 경우는 거의 대부분 룸의 이름의 중복에 있다.
     * 이런 경우 다음과 같이 OnPhotonCreateRoomFailed 콜백 함수가 호출되며, 이 함수에서 예외 처리 로직을 수행하게 하자.
     * 또한 다음과 같이 세부적인 오류 메시지를 받아올 수 있다. 사용자에게 전송 해주어 오류 정보를 알리자.
     * 
    */
    void OnPhotonCreateRoomFailed(object[] codeAndMsg)
    {
        //오류 코드( ErrorCode Class )
        Debug.Log(codeAndMsg[0].ToString());
        //오류 메시지
        Debug.Log(codeAndMsg[1].ToString());
        // 여기선 디버그로 표현했지만 릴리즈 버전에선 사용자에게 메시지 전달    //#18 실제 게임에서는 에러창(이미지 같이) 직접 떠야겠지
        Debug.Log("Create Room Failed = " + codeAndMsg[1]);
    }

//#18 공부용
    void OnCreatedRoom()
    {
        Debug.Log("공부용 : 방 만들어짐");  //# 실행됨.
    }
    

//# 테스트용 주석    
    // 룸에 입장하면 호출되는 콜백 함수 
    // PhotonNetwork.CreateRoom 함수로 룸을 생성한 후 입장하거나, PhotonNetwork.JoinRandomRoom, PhotonNetwork.JoinRoom 함수를 통해 입장해도 호출 된다.
    void OnJoinedRoom()
    {
        Debug.Log("Enter Room");
        //여기까지 게임을 실행하면 로비 입장, 랜덤 매치 메이킹, 룸 생성, 룸 입장의 과정을 거치며 Console 뷰에 
        //Joined Lobby !!!, No Rooms !!!, Enter Room 메시지가 출력~ 즉 순서대로 룸 입장까지 완료된 로그 메시지를 확인하자~!

        //플레이어를 생성하는 함수 호출
//        CreatePlayer();
        //룸 씬으로 전환하는 코루틴 실행 (UI 버전에서 사용)
        StartCoroutine(this.LoadStage());
    }

//#18 공부용
    void OnConnectedToMaster()
    {
        Debug.Log("Master_Join / 마스터 입장"); //#18 실행 안돼
    }

    /*
     * PhotonNetwork.Instantiate
     * 
     * 게임오브젝트 또는 프리팹을 로컬 및 네트워크상 동적으로 생성
     * 하려면 PhotonNetwork.Instantiate 이 함수를 사용해야 함
     * 
     * PhotonNetwork.Instantiate() 함수를 사용하여 생성한 게임오브젝트
     * 나 프리팹은 현재 동일 룸에 접속해 있는 모든 네트워크 플레이어에게
     * 객체를 동시에 생성 시킴 (cf)이 함수는 유니티 빌트인 네트워크의 
     * Network.Instantiate 함수와 동일 기능(역할)을 하는 함수
     * 
     * PhotonNetwork.Instantiate(string prefabName, Vector3 position, Quaternion rotation, byte group);
     * 
     * 이 함수의 첫 번째 인자는 Network.Instantiate 함수와 다르게 string형 타입으로서 생성하려하는 
     * 애셋(프리팹)의 이름을 전달 해야함 즉, 예약 폴더인 Resources 폴더에서 찾겠다!!!(이 함수는 해당 애셋을 Resources 폴더에서 로드)
     * 그러므로,  PhotonNetwork.Instantiate 함수를 통하여 생성하려는 애셋(프리팹)은 무조건 Resources 폴더에 위치해야함
     * PhotonNetwork.Instantiate("프리팹, 위치, 각도, 그룹"); 함수는 현재 게임에 접속한 모든 유저에게 프리팹을
     * 생성해주며 내부적으론 Buffered RPC를 호출하여 나중에 접속한 플레이어도 미리 생성된 프리팹을 볼 수 있음.
     * 또한, group을 지정하면 동일 group의 사용자들에게만 생성
     * 
     * PhotonNetwork.Instantiate(string prefabName, Vector3 position, Quaternion rotation, byte group, object[] data);
     * 
     * 다음과 같이 오버로딩 되어있음...마지막에 데이타를 전달 가능하다~^^
     * 위에 두 함수로 생성된 오브젝트는 클라이언트가 접속 종료하면 네트워크상의 모든 오브젝트는 사라짐...
     * 즉, 위의 두 Method는 로컬 플레이어에 종속된 Object를 생성하는 것 이다. PhotonView 컴포넌트의 요소 Owner의 값은 PhotonNetwork.playerName
     * 
     * PhotonNetwork.InstantiateSceneObject(string prefabName, Vector3 position, Quaternion rotation, byte group, object[] data);
     * 이 함수는 위 두함수와 마찬가지로 네트워크 상에 프리팹을 동시에 생성시키지만 Master Client(방장)만 생성 및 삭제할 수 있음
     * 확인해보면, 생성된 프리팹의 PhotonView 컴포넌트의 요소 Owner는 Scene이 된다.
     * 즉, 위의 Method는 Scene에 종속된 Object를 생성하는 것 이다.
     * 
     */

    //플레이어를 생성하는 함수 (UI 버전에서는 주석 처리)
    /*void CreatePlayer()
    {
        //float pos = Random.Range(-100.0f, 100.0f);
        //포톤네트워크를 이용한 동적 네트워크 객체는 다음과 같이 Resources 폴더 안에 애셋의 이름을 인자로 전달 해야한다. 
        //PhotonNetwork.Instantiate( "MainPlayer", new Vector3(pos, 20.0f, pos), Quaternion.identity, 0 );
        PhotonNetwork.Instantiate( "MainPlayer", playerPos.position, playerPos.rotation, 0 );
        
        
        //PhotonNetwork.InstantiateSceneObject(string prefabName, Vector3 position, Quaternion rotation, byte group, object[] data);
        //이 함수도 PhotonNetwork.Instantiate와 마찬가지로 네트워크 상에 프리팹을 동시에 생성시키지만, Master Client 만 생성 및 삭제 가능.
        //생성된 프리팹 오브젝트의 PhotonView 컴포넌트의 Owner는 Scene이 된다.
        


    }*/

    //룸 씬으로 이동하는 코루틴 함수 (UI 버전에서 사용) //#19 씬 넘어가면서 연결이 끊기게 되는데, 에러 안 나도록 잠시 네트워크 끊자~
    IEnumerator LoadStage()
    {
        //씬을 전환하는 동안 포톤 클라우드 서버로부터 네트워크 메시지 수신 중단
        //(Instantiate, RPC 메시지 그리고 모든 네트워크 이벤트를 안받음 )
        //차후 전환된 scene의 초기화 설정 작업이 완료후 이 속성을 true로 변경
        PhotonNetwork.isMessageQueueRunning = false;        //#19 잠시 네트워크 끊기

        //백그라운드로 씬 로딩
        AsyncOperation ao = SceneManager.LoadSceneAsync("scPlayUI0");


        //#19 아래 코드를 이용한 것 = 위 코드 2줄과 같음~
        /*
         * 참고) PhotonNetwork.LoadLevelAsync =>  PhotonNetwork.isMessageQueueRunning = false 로 
         *       변경 후 인자로 전달된 씬을 로드
         */

        // 씬 로딩이 완료 될때까기 대기...
        yield return ao;

        Debug.Log("로딩 완료");
    }

/**************************************************************************************************
// 5.3 이후 
SceneManager.LoadScene(0);                                          // 로드. 
SceneManager.LoadScene("SceneName");
AsyncOperation ao = SceneManager.LoadSceneAsync(0);                 // 로드. (비동기)
AsyncOperation ao = SceneManager.LoadSceneAsync("SceneName");
SceneManager.LoadScene(0, LoadSceneMode.Additive);                  // 씬 병합 추가. 
SceneManager.LoadScene("SceneName", LoadSceneMode.Additive);
SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);             // 씬 병합 추가. (비동기)
SceneManager.LoadSceneAsync("SceneName", LoadSceneMode.Additive);
SceneManager.UnloadScene(0);                                        // 언로드. 
SceneManager.UnloadScene("SceneName");
SceneManager.sceneCount;                                            // 현재 로드 된 씬 개수. 
SceneManager.sceneCountInBuildSettings;                             // BuildSetting 에 등록 된 씬 개수. 
SceneManager.GetActiveScene().buildIndex;                           // 현재 씬 인덱스. 
SceneManager.GetActiveScene().name;                                 // 현재 씬 이름. 

// 씬 정보 조회. 
Scene activeScene = SceneManager.GetActiveScene();
Scene scene1 = SceneManager.GetSceneAt(0);
Scene scene2 = SceneManager.GetSceneByName("SceneName");
Scene scene3 = SceneManager.GetSceneByPath("Assets/01. Scenes/SceneName.unity");
Scene[] loadedScenes = SceneManager.GetAllScenes();

// Scene 구조체. 
int buildIndex;
string name;
string path;
bool isLoaded;
bool isDirty;       // 씬을 변경(수정)했는지 여부. 
int rootCount;      // 씬의 Root에 있는 GameObject 개수. 
bool IsValid();     // 유효한 씬인지 여부. 

// 기타. 
Scene scene = gameObject.scene;                             // 게임오브젝트가 속해있는 씬을 가져오기. 
GameObject go = new GameObject("New Object");               // 게임오브젝트를 생성하면 현재 씬에 추가 됨. 
SceneManager.MoveGameObjectToScene(go, scene);              // 게임오브젝트를 다른 씬으로 이동. 
SceneManager.MergeScenes(sourceScene, destinationScene);    // 씬을 병합. 

// SceneManager.Get~() 으로 가져올 수 있는 것은 로드가 끝난 씬만 가능. 
Scene scene = SceneManager.GetSceneByName("SceneName");
bool isValid = scene.IsValid();     // false 가 리턴 됨.

//ex 1
SceneManager.LoadScene("03. Test", LoadSceneMode.Additive);
Scene scene = SceneManager.GetSceneByName("03. Test");
GameObject go = new GameObject("New Object");
SceneManager.MoveGameObjectToScene(go, scene);

//ex 2
Scene scene = SceneManager.GetSceneAt(1);
GameObject go = new GameObject("New Object");

yield return new WaitForSeconds(3.0f);

SceneManager.MoveGameObjectToScene(go, scene);
SceneManager.MoveGameObjectToScene(gameObject, scene);

yield return new WaitForSeconds(1.0f);

SceneManager.UnloadSceneAsync("02. Room");


[참고] PhotonNetwork.LoadLevel() 함수는 PhotonNetwork.isMessageQueueRunning를 false로 변경하고
        인자로 전달된 씬을 로드.

**********************************************************************************************************/


    //Join Random Room 버튼 클릭 시 호출할 함수 (UI 버전에서 사용)
    //(참고) 사용자 정의 이벤트 함수의 접두사(Prefix)는 항상 On을 붙혀주는 것이 관례
    public void OnClickJoinRandomRoom()
    {
        //로컬 플레이어의 이름을 설정
        PhotonNetwork.player.NickName = userId; //userId.text;
        //PhotonNetwork.player.name = userId.text; (사라질듯...)

        //플레이어의 이름을 로컬 저장 (저장 싫어서...)
        //PlayerPrefs.SetString("USER_ID", userId.text);

        //무작위로 추출된 룸으로 입장 
        PhotonNetwork.JoinRandomRoom();
    }

    //Make Room 버튼 클릭 시 호출될 함수 (UI 버전에서 사용) //#10-1 btnHostGameStart 버튼과 연결  
    public void OnClickCreateRoom()
    {
        Debug.Log("//#10 호스트 게임 클릭");
        
//#11-1 방 이름 중복 방지
// 방 리스트 받아오기
        RoomInfo[] roomList = PhotonNetwork.GetRoomList();
        
        foreach(RoomInfo room in roomList)
        {
            if(room.Name == roomName.text)   //방 리스트들 속 room.Name 과 내가 적은 방 이름(roomName)이 같은 것이 있다면
            {
                Debug.Log("방 이름 중복. 리턴");
                roomName.text = ""; //다시 적도록 텍스트창도 초기화
                return;
            }
        }        
        string _roomName = roomName.text;
        string _roomPassword = createRoomPassWord.text;   //#10-1 패스워드 
        
        if(string.IsNullOrEmpty(roomName.text)) //룸 이름이 없거나 Null일 경우 룸 이름 지정
        {
            _roomName = "ROOM_" + Random.Range(0, 999).ToString("000"); //자릿수 맞춰서 반환
        }

//#10-1 '비밀방' 토글 켜져 있는데, 입력한 값 없으면 _roomPassWord = "";
        if(tgIsScretRoom.isOn)
        {
        Debug.Log("//#10 토글 클릭되어 있네");

            if(string.IsNullOrEmpty(createRoomPassWord.text))
            {
                tgIsScretRoom.isOn = false;
                _roomPassword = ""; 
            }
        }
        else    // 꺼져 있으면 = "";
        {
        Debug.Log("//#10 토글 꺼져 있네");

            _roomPassword = "";
        }
        

        //로컬 플레이어의 이름을 설정
        PhotonNetwork.player.NickName = userId;     //userId.text;

        //플레이어의 이름을 로컬 저장
        //PlayerPrefs.SetString("USER_ID", userId.text);

        //생성할 룸의 조건 설정 1
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        
        if(LobbyManager.playSingleGame)
            roomOptions.MaxPlayers = 1;
        else
            roomOptions.MaxPlayers = 4;

        //생성할 룸의 조건 설정 2 (객체 생성과 동시에 멤버변수 초기화)
        //RoomOptions roomOptions = new RoomOptions() { IsOpen=true, IsVisible=true, MaxPlayers=50 };

// #10-1
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()  //#각종 방 설정 의미
        {
            {"password", _roomPassword}
            //,{"RoomOpenSettings", }
        };

        roomOptions.CustomRoomPropertiesForLobby = new string[] //# 로비에서 표시되는 방의 정보 의미
        {
            "password",
        };

        //지정한 조건에 맞는 룸 생성 함수 
        PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);
    }

    //생성된 룸 목록이 변경됐을 때 호출되는 콜백 함수 (최초 룸 접속시 호출) (UI 버전에서 사용)
    private void OnReceivedRoomListUpdate()  //BtnMultiPlay에서 사용하기 위함 -> 연결 해제함
    {
        // if(LobbyManager.playSingleGame) //#10-1 싱글 플레이라면, 다른 플레이어가 들어올 수 있는 버튼 만들지 않도록.
        //     return;



        // 포톤 클라우드 서버에서는 룸 목록의 변경이 발생하면 클라이언트로 룸 목록을 재전송하기
        // 때문에 밑에 로직이 없으면 다른 클라이언트에서 룸을 나갈때마다 룸 목록이 쌓인다.
        // 룸 목록을 다시 받았을 때 새로 갱신하기 위해 기존에 생성된 RoomItem을 삭제  
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("ROOM_ITEM"))
        {
            Debug.Log("//#6-3 기존 ROOM_ITEM 삭제하고~");
            Destroy(obj);
        }

//#19 제로베이스로 만들어주고~
        //Grid Layout Group 컴포넌트의 Constraint Count 값을 증가시킬 변수
        int rowCount = 0;
        //스크롤 영역 초기화
        //scrollContents.GetComponent<RectTransform>().sizeDelta = new Vector2(0 ,0);
        scrollContents.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

//# 방정보를 바탕으로 프리팹 만들기
        //수신받은 룸 목록의 정보로 RoomItem 프리팹 객체를 생성
        //GetRoomList 함수는 RoomInfo 클래스 타입의 배열을 반환
        foreach(RoomInfo _room in PhotonNetwork.GetRoomList())
        {
            Debug.Log("//#11-1 방 생성");
            // Debug.Log("//#6-4 방 이름을 이걸로 해서 만들어 : " + _room.name);
            //RoomItem 프리팹을 동적으로 생성 하자
            GameObject room = (GameObject)Instantiate(roomItem);
            //생성한 RoomItem 프리팹의 Parent를 지정    //# 지금까지 하던 함수 말고 이렇게도 Parent 지정이 가능하구나~
            room.transform.SetParent(scrollContents.transform, false);

            /*
             * room.transform.parent = scrollContents.transform;
             * 
             * (자식 게임오브젝트).transform.parent = (부모 게임오브젝트).transform;
             * 이 방법보다는 UI 항목을 차일드화할 때는 스케일과 관련된 문제가 발생할
             * 수 있기 때문에 앞에서 사용한 방법보다 SetParent 메서드를 사용하는 것이 
             * 편리함. worldPositionStays 인자를 false로 설정하면 로컬 기준의 정보를 유지한 채
             * 차일드화 된다. (그냥 전 경고 메시지가 안떠서 이걸로 하는게 좋은거 같아요)
             * 
             */

             //생성한 RoomItem에 룸 정보를 표시하기 위한 텍스트 정보 전달
            RoomData roomData = room.GetComponent<RoomData>();
            roomData.roomName = _room.Name;
            roomData.connectPlayer = _room.PlayerCount;
            roomData.maxPlayers = _room.MaxPlayers;
//#10-1 해쉬
            roomData.myRoomHashT = _room.CustomProperties;
            string hashPassword = (string)roomData.myRoomHashT["password"];
            if(hashPassword == "")  
            {
                roomData.SetSecretImage(false);
            }
            else
            {
                roomData.SetSecretImage(true);
            }

            //텍스트 정보를 표시 
            roomData.DisplayRoomData();
//#19 버튼과 동적으로 연결
            //RoomItem의  Button 컴포넌트에 클릭 이벤트를 동적으로 연결 
            // if(roomData != null)
            //     Debug.Log("//#6 roomData는 null이 아님");
//#6-1 오류 잡기 잠깐 주석 - 원인 해결 - 버튼이 한 단계 아래에 연결되어 있었음
            Transform buttonTransform = roomData.transform.GetChild(0); // 0번째 자식 오브젝트의 Transform 컴포넌트 가져오기
            buttonTransform.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { OnClickRoomItem(roomData.roomName, roomData); Debug.Log("Room Click " + roomData.roomName); });

            // UnityEngine.UI.Button button = buttonTransform.GetComponent<UnityEngine.UI.Button>();
            // if(button != null)
            //     Debug.Log("//#6 button은 null이 아님");
            // else
            //     Debug.Log("//#6 Button은 null임");
            /*
             * delegate (인자) { 실행코드 };  => 인자는 생략 가능하다
             * delegate (room.name) { OnClickRoomItem( room.name ); Debug.Log("Room Click " + room.name); };
             * delegate { OnClickRoomItem( roomData.roomName ); };
             */
//#19 스크롤바 Contents 위치 맞춰주기
            //Grid Layout Group 컴포넌트의 Constraint Count 값을 증가시키자
            scrollContents.GetComponent<GridLayoutGroup>().constraintCount = ++rowCount;
            //스크롤 영역의 높이를 증가시키자
            scrollContents.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 20);

//#10-1 용훈님 과거 파일 OnClickRoomItem 이라는 별도의 함수 만들어서 호출함
            OnShowRoomItem(roomData);
        }

    }   
    /* RoomInfo 클래스는 생성된 룸의 다양한 정보를 담는다. 
     * 주요 속성 정리 (RoomInfo 클래스의 속성)
     *  
     *             속성                                       설명
     * Hashtable CustomProperties { get; }          룸에 설정된 사용자 정의 속성
     * string Name { get; }                         룸 이름
     * int PlayerCount { get; private set; }        현재 룸에 접속한 사용자 수
     * byte MaxPlayers { get; }                     최대 접속 가능한 사용자 수
     * bool IsOpen { get; }                         룸에 접속 허용 여부 
     * bool IsVisible { get; }                      로비에서 룸의 노출 여부
     * 
     * cf) 룸 목록 전송 주기
     * 포톤서버 개발사에서는 룸 목록이 새롭게 갱신된 후 클라이언트에게 전달되는 시간은 약 5초간의 간격(delay)이 있다고
     * 설명하고 있으나, 네트워크 환경에 따라서 오차가 있을 수 있다. 따라서 실시간으로 룸 정보가 갱신되지
     * 않는다는 것을 기억하라.
     */
    void OnShowRoomItem(RoomData _roomData) //#10-1 =======================================
    {
        // 방의 정보 보여주는 코드
    }
    
    //RoomItem이 클릭되면 호출될 이벤트 연결 함수 (UI 버전에서 사용)
    void OnClickRoomItem(string roomName, RoomData _data)   // 각 ImgRoomItem의 비밀번호 Ok 버튼에 연결하기
    {
//#10-1 비밀번호 있는 방일 때, 비밀번호 맞추면 그때 JOIN 하도록
        targetRoom = _data;
        targetHash = _data.myRoomHashT;

        Debug.Log("//#10 방 클릭");
        
        if((string)targetHash["password"] != "")    // 비밀번호가 존재할 때
        {
            Debug.Log("//#10 비밀번호 존재");
            
            if(!targetRoom.CheckPassword()) //(비밀번호가 같지 않다면)
            {
                Debug.Log("//#10 비밀번호 달라");
                return;
            }
            Debug.Log("//#10 비밀번호 같아");
        }
        //존재하지 않는다면, 그냥 바로 입장
        
        //로컬 플레이어의 이름을 설정
        PhotonNetwork.player.NickName = userId; //userId.text;

        Debug.Log("//#10 조인할래ㅠㅠ");
        //플레이어 이름을 저장
        //PlayerPrefs.SetString( "USER_ID", userId.text );

        //인자로 전달된 이름에 해당하는 룸으로 입장
        PhotonNetwork.JoinRoom(roomName);
    }



//#18 이건 로비에서만 볼 수 있음
/*
So, 테스트는 2개의 방을 파서 해야 돼
	- 하나는 방 상태(빌드한 버전)
    - 하나는 로비에 머문 상태(UNITY에서 스크립트에 주석하고) - (createRoom 함수 주석)
*/

    //포톤 클라우드 서버로 접속하는 과정에 대한 로그 메시지 출력을 위한 콜백함수 
    //마지막 JoinedLobby 로그 메시지가 표시되면 정상적으로 포톤 클라우드에 접속하여 로비에 입장한 상태임.
    void OnGUI()
    {

        //화면 좌측 상단에 접속 과정에 대한 로그를 출력(포톤 클라우드 접속 상태 메시지 출력)
        // PhotonNetwork.ConnectUsingSettings 함수 호출시 속성 PhotonNetwork.connectionStateDetailed는
        //포톤 클라우드 서버에 접속하는 단계별 메시지를 반환함.
        //Joined Lobby 메시지시 포톤 클라우드 서버로 접속해 로비에 안전하게 입장했다는 뜻
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());

/*
        //만약 포톤네트워크에 연결 되었다면...
        if (PhotonNetwork.connected)
        {
            GUI.Label(new Rect(0, 50, 200, 100), "Connected");

            //룸 리스트를 배열로 받아온다.
            RoomInfo[] roomList = PhotonNetwork.GetRoomList();

            if (roomList.Length > 0)
            {
                foreach (RoomInfo info in roomList)
                {
                    GUI.Label(new Rect(0, 80, 400, 100), "Room: " + info.Name
                        + " PlayerCount/MaxPlayer :" + info.PlayerCount + "/" + info.MaxPlayers //현재 플레이어/최대 플레이어
                        + " CustomProperties Count " + info.CustomProperties.Count // 설정한 CustomProperties 수 
                        + " Map ???: " + info.CustomProperties.ContainsKey("Map") //키로 설정한 Map이 있나
                        + " Map Count " + info.CustomProperties["Map"] // 설정한 키 값 
                        + " GameType ??? " + info.CustomProperties.ContainsKey("GameType") //키로 설정한 GameType이 있나
                        + " GameType " + info.CustomProperties["GameType"]);// 설정한 키 값 
                }
            }
            else
            {
                GUI.Label(new Rect(0, 80, 400, 100), "No Room List");
            }
        }
        //PhotonServerSettings 값 가져오기
        {
            GUI.Label(new Rect(0, 170, 400, 100), "AppID  :  " +
                PhotonNetwork.PhotonServerSettings.AppID);
            GUI.Label(new Rect(0, 200, 200, 100), "HostType  :  " +
                PhotonNetwork.PhotonServerSettings.HostType);
            GUI.Label(new Rect(0, 230, 200, 100), "ServerAddress  :  " +
                PhotonNetwork.PhotonServerSettings.ServerAddress);
            GUI.Label(new Rect(0, 260, 200, 100), "ServerPort  :  " +
                PhotonNetwork.PhotonServerSettings.ServerPort);
            //PhotonNetwork.PhotonServerSettings.UseCloud(); 

            //핑 테스트
            int pingTime = PhotonNetwork.GetPing();
            GUI.Label(new Rect(0, 310, 200, 100), "Ping: " + pingTime.ToString());
        }
*/

    }
}
// 참고 https://doc-api.photonengine.com/ko-kr/pun/current/class_room_options.html


/*
 * = 네트워크 기초 =
 *                 
 * 1) 네트워크 게임??
 * 물리적/공간적 떨어져 있는 다른 유저와 통신망(LAN, 인터넷 망)을 통해서 서로 데이타를 
 * 주고받고 게임하는 것.
 * 
 * 2)네트워크 게임의 물리적 구조 (네트워크 구조를 그림으로 그릴줄 알아야함...기본!!!)
 * 
 * ■ P2P(Peer to Peer) 방식 => 서버/클라이언트 모델에 대응되는 모델
 * 
 *  => 유저끼리 별도의 서버 없이 네트워크 연결하여 데이타를 송수신하는 구조를 말함
 *     접속자가 적은 게임에 적용되며, 네트워크 상 유저(사용자)가 직접 접속해서 게임을 함.
 * 
 *    ○  응용 중심에 따른 분류(사용 용도에 따라 구분)
 *
 *       정보 공유형 응용
 *
 *          - 파일이나 데이터 등을 공유하거 메시지의 교환을 통하여 정보를 공유 (메신저)
 *
 *          - mp3 음악 파일을 공유하는 넵스터, 그누텔라, 인스턴트 메시지를 교환하는 ICQ 등 (파일 공유 프로그램)
 *
 *       자원 공유형 응용
 *
 *          - 하나의 커다란 처리를 세분하여 분산 클라이언트가 처리한 후 최종적으로 중심이 되는
 *
 *             서버에게 처리 결과를 전송하여 결합하는 시스템
 *
 *          - 각 컴퓨터의 처리능력(CPU)을 하나의 컴퓨팅 자원으로 취급하여 여러 컴퓨터가
 *
 *             필요에 따라 공유
 *             
 * ■ 서버/클라이언트 모델(방식) => 우리가 할거...(거의 대부분의 온라인 게임 방식)
 * 
 *  => 게임 서버를 구축하고 여러 유저(클라이언트)가 접속해서 상호간의 데이터를 
 *     게임 서버를 이용해 송/수신하는 방식으로 게임 서버의 기능적 역할은 접속한 클라이언트 사이의 데이타를 Relay하고
 *     게임 데이터를 DB 서버에 저장한다. 
 *     
 *     (참고) 유니티 빌트인 네트워크(Built in Network))로 이러한 서버/클라이언트 네트워크
 *     게임을 깊이 있고 복잡한 네트워크 지식없이 손쉽게 개발할 수 있다. 하지만 우린 포톤을 이용한 네트워크
 *     게임을 개발할것이다...그러나 포톤 하나만 알면 유니티 빌트인 네트워크는 껌이다...이유는
 *     포톤 개발사는 Built in Network를 맞춰서 (98% 이상(내 생각..) ) 용어, 사용법, 
 *     주요 기능(Network View, State Synchronization, RPC: Remote Procedure Call) 등을 제공한다
 *     (즉 포톤 클라우드는 유니티 빌트인 네트워크 API의 부족한 기능을 보완하고 네트워크 게임에 필요한 필수적인 기능을 확장했기 때문에 API 사용법이 거의 동일)
 *     따라서 포톤만 완벽히 알면 Built in Network 뿐만 아니라 유니티를 지원하는 검증된 여러 네트워크 게임 엔진(서드파티)을 
 *     살만 약간 붙혀서 손쉽게 사용 가능~!
 *     
 *     또한, 온라인 네트워크 게임 개발을 위해선 물리적 서버(돈) + 네트워크 게임 서버(기술력)가 구축되야 한다.
 *     네트워크 게임을 직접 구현하는것은 숙련된 네트워크 개발 경험 및 스킬을 갖추어야 한다.
 *     그리고 수많은 알파/베타 테스트등을 거쳐 네트워크 속도/안정성/최적화등의 작업이 이루어져야 한다.
 *     소규모/인디개발자에겐 현실적으로 어렵다...따라서 검증된 네트워크 게임 엔진을 사용하자.
 *     우린 여기서 서드파티(유니티를 베이스로...) 네트워크 게임 엔진인 포톤을 활용!~
 *     
 *     유니티를 지원하는 서드파티 네트워크 게임 엔진(서버) (가격/성능/서버의 운영체제 등을 고려하여 네트워크 게임 서버 선택!!!)
 *     Photon           http://www.exitgames.com (유니티에서 가장 오래 사랑받고 검증된 엔진)
 *     프라우드넷       http://www.nettention.com
 *     ElectroServer    http://www.electrotank.com
 *     MuchDefferent    http://www.muchdifferent.co.kr
 *     SmartFox         http://www.smartfoxserver.com
 *     
 *     (추가) 유니티에선 Unity 5.3이상 버전부터 대규모 네트워크 게임 개발이 가능한 UNET을 제공한다.
 *     Built in Network API를 대체하는 API로서 MMO등의 대규모 네트워크 게임 개발을 위한 필요한 기능을
 *     쉽고 편리하게 구현해 놓은 네트워크 게임 엔진이다. 아직 안정화 단계지만 우리기 포톤만 열심히 한다면
 *     문제없이 사용가능하다(구글링등..)
 *     
 * 3) 네트워크 통신 프로토콜
 * => 프로토콜이란 네트워크 상에서 데이터를 통신하기 위한 규약(약속)으로 게임에선
 *    TCP/IP(정확성 위주), UDP(속도 위주) 프로토콜 사용
 * 
 * ■ Packet(패킷) 
 * => 프로토콜에 따라 정해진 송/수신하는 데이터의 단위
 *     
 * ■ TCP/IP 프로토콜    //#18 정확성!!
 * => 정합성을 위해 탄생한 통신 규약으로 데이터의 유실이 없다. 즉 100개의 데이터를 보내면
 *    상대편에서 100개를 다 받을 수 있다.
 *    Packet에 보내는 Data 순서를 정확하게 지켜 전송하며 만약에 송신중 Packet이 유실됐을 경우
 *    수신측은 전송된 패킷의 유실된 부분을 알고 재전송을 송신측에 요구한다. 따라서 송신측은
 *    다시 패킷을 재전송한다. 그러므로 속도가 UDP 프로토콜에 보다 느리다.(전송/대기/응답/재전송 등의 이유로...)
 *    ( 카드 게임처럼 속도감 보단 정합성의 비중이 큰 게임에서는 주로 TCP/IP 프로토콜 사용 )
 *    
 * ■ UDP 프로토콜       //#18 정확성이 필요 없는 게임(FPS. 총을 무한번 쏘는)
 * => TCP/IP와 다르게 수신측이 받을 준비가 돼 있는지 확인하는 절차 없이 무조건 패킷을 보내는 프로토콜로서
 *    패킷의 순서를 보증하지 않으며 네트워크 상황에 따라 전송도중 중간에 Packet이 유실 되어도 재전송 절차가 없다
 *    그러므로 정합성은 무시되나 매우 빠른 전송 속도를 보장. 따라서 송신 중간에 패킷이 하나씩 유실돼도 크게 상관없는
 *    환경에 주로 사용된다.( FPS,액션,대결 게임처럼 정합성 보단 액션의 비중이 크고 속도감 있는 게임에서는 주로 UDP 프로토콜 사용)
 *    
 *    
 * 
 */