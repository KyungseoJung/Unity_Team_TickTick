using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour   //#1-1 
{
    private OptionManager _oMgr;    // 효과음 사용을 위해 옵션 메니저 가져옴
    public Button goBackBtn;           //'뒤로가기 버튼'
    string _playerName;             //#9-1 플레이어가 값을 입력했는지 확인하는 목적
    string _islandName;
    public static bool playSingleGame = true;       //#10-1 싱글플레이 or 멀티 플레이인지 확인용(csPhotonInit에서 가져갈 예정) - 경우에 따라 csPhotonInit에서 방을 만들거나 or 안 만들거나
// '로비' 화면 ======================
    [Header("로비 화면")]
    [Space(10)]                         //변수들의 간격을 위한 어트리뷰트 선언(보기 좋다)
    public Button[] lobbyBtns;       // '로비' 화면 버튼 배열   //[0]부터 차례대로 BtnLoadGame, BtnNewGame, BtnMultiPlay, BtnGameExit
    public GameObject lobby;            // '로비' 화면
    public GameObject loadGame;         // '게임 불러오기' 화면
    public GameObject newGame;          // '새로운 게임' 화면
    public GameObject multiGame;        // '멀티 플레이' 화면

    public GameObject loadGameBtn;      //#9-1 '게임 불러오기' 버튼 - 활성화/ 비활성화 목적
    public GameObject multiPlayBtn;     //#9-4 '멀티 플레이' 버튼 - 활성화/ 비활성화 목적
// '게임 불러오기' 화면 ======================
    [Header("게임 불러오기 화면")]
    [Space(10)]
    public Button[] loadGameBtns;    // '게임 불러오기' 화면 내 버튼 배열
    public GameObject loadScrollContents;       // '게임 불러오기' 화면 내 스크롤
    public GameObject singlePlayerInfoBox;

// '새로운 게임' 화면 ======================    //변수 바꿔서 헷갈림 -> clothes = eye라고 생각
    [Header("새로운 게임 화면")]
    [Space(10)]
    private bool inputAllInfo;      // #9-1 모든 정보를 입력했나 확인용 (플레이어 옷 종류, 옷 색, 이름, 섬 이름)
    public Button[] newGameBtns;     // '새로운 게임' 화면 내 버튼 배열 //[0] : btnRandom
    public Button[] chooseClothesBtns;  // '옷'-> '눈동자' 선택 버튼   //[0]부터 차례대로 : btnClothes0, btnClothes1, btnClothes2
    public Button[] chooseColorsBtns;   // '색' > '가방 색' 선택 버튼   //[0]부터 차례대로 : btnColor0, btnColor1, btnColor2
    public GameObject[] chooseScreen;       //[0] : 옷 선택하는 panel, [1] : 색 선택하는 panel, [2] : 플레이어 이름, 섬 이름 작성하는 panel

    public Image playerClothes;     // ImgPlayerClothes 오브젝트    //#14-1 JSON 저장 위해서 이미지 오브젝트는 일단 살려두자
    public Image[] newClothes;
    public Image[] newColor;
// #13-1 커스터마이징 플레이어와 Material 연결하기
    // public SkinnedMeshRenderer skinnedScout;    // 눈 접근용
    // public SkinnedMeshRenderer skinnedBackPack; // 백팩 접근용

    // public GameObject objScout;    // 눈 접근용
    // public GameObject objBackPack; // 백팩 접근용

    public Material matSkin;            //  원래 피부색 - 눈만 바꾸는 거 안 돼서
    public Material[] matCustomEyes;    // 3가지 종류의 눈 
    public Material[] matCustomBackPacks;   // 3가지 색의 백팩

    // public Material matPlayerEye;   //테스트용
    // public Material matPlayerBackPack;  //테스트용
    public Renderer materialEye;        // 바꾸고자 하는 오브젝트의 Renderer
    public Renderer materialBackPack;   // 바꾸고자 하는 오브젝트의 Renderer
// #13-2 회전 ==========================
    public GameObject customPlayer;     //회전 목적
    private float rotateTime = 1.0f;
    private float rotationAngle = 180f;
    private float currRotateTime = 0f;  //현재 회전하고 있는 시간
    private Quaternion startRotation;
    private Quaternion endRotation;



    public InputField inputName;        //#4-1 
    public InputField inputIslandName;  //#4-1 
    private int clothesNum;         //#4-1 싱글톤, JSON 데이터 저장
    private Color clothesColor;     //#4-1 싱글톤, JSON 데이터 저장   

    public Text[] loadInfoName;      //#9-1  플레이어 이름 //[0]부터 : txtPlayerName, txtIslandName

// '멀티 플레이' 화면 ======================
    [Header("멀티 플레이 화면")]
    [Space(10)]
    public Button[] multiGameBtns;   // '멀티 플레이' 화면 내 버튼 배열
    //0부터 순서대로 : btnStart, btnDestroy, btnHostGame, btnOpenGames, // btnConnect
    public GameObject[] multiScreen;       
    // 0부터 순서대로 : (pnlPlayerList 오브젝트), (pnlMulti 오브젝트), (HostGame 오브젝트), (OpenGames 오브젝트)
    public Toggle[] tgHostOption;   // 방 선택 옵션 토글    [0]부터 : tgPublic, tgInviteOnly, tgPublic, tgPrivate
    public Toggle[] tgOpenOption;   
    public Text hostOptExplain;     // 호스트 옵션 설명 텍스트
    public Text[] loadMInfoName;      //#9-4  플레이어 이름 //[0]부터 : txtPlayerName, txtIslandName
    public GameObject inputFieldPassWord;   //#10-1  '비밀방' 토글(tgHostOption[3]) 체크 되어있을 때에만 활성화 되도록
// 임시 버튼 ======================
    [Header("다음 씬 넘어가기 임시 버튼")]
    [Space(10)]
    public Button btnGoNextScene;

//버튼 연결 ========================================================
    
    void Start()
    {
        StartCoroutine(LoadJSONDataFct());

        _oMgr = GameObject.Find("OptionManager").GetComponent<OptionManager>();
//        _oMgr.PlayBackground(stage);

// '돌아가기' 버튼 ======================
        goBackBtn.onClick.AddListener(OnClickGoBack);
        goBackBtn.gameObject.SetActive(false);

// '로비' 화면 버튼 ======================
        lobbyBtns[0].onClick.AddListener(OnClickLoad);
        lobbyBtns[1].onClick.AddListener(OnClickNewGame);
        lobbyBtns[2].onClick.AddListener(OnClickMultiGame);
        lobbyBtns[3].onClick.AddListener(OnClickExit);
// '게임 불러오기' 화면 내 버튼 배열 ======================
        loadGameBtns[0].onClick.AddListener(OnClickSingleStart);
        loadGameBtns[1].onClick.AddListener(OnClickDestroyData);   
        // ScrollContents의 Pivot 좌표를 Top, Left로 설정 하자. (UI 버전에서 사용) 
        loadScrollContents.GetComponent<RectTransform>().pivot = new Vector2(0.0f, 1.0f);  //# 피봇 동적 조작 가능
// '새로운 게임' 화면 내 버튼 배열 ======================
        newGameBtns[0].onClick.AddListener(OnClickRandomCustomize);
        newGameBtns[1].onClick.AddListener(OnClickOpenClothes);
        newGameBtns[2].onClick.AddListener(OnClickOpenColors);
        newGameBtns[3].onClick.AddListener(OnClickCheckName);
        newGameBtns[4].onClick.AddListener(OnClickCreateRole);

        chooseClothesBtns[0].onClick.AddListener(OnClickClothes0);
        chooseClothesBtns[1].onClick.AddListener(OnClickClothes1);
        chooseClothesBtns[2].onClick.AddListener(OnClickClothes2);

        chooseColorsBtns[0].onClick.AddListener(OnClickColors0);
        chooseColorsBtns[1].onClick.AddListener(OnClickColors1);
        chooseColorsBtns[2].onClick.AddListener(OnClickColors2);

        inputName.onValueChanged.AddListener(OnInputChange1);        // #9-1 최대 글자수 제한
        inputIslandName.onValueChanged.AddListener(OnInputChange2);
 // '멀티 플레이' 화면 내 버튼 배열 ======================
        multiGameBtns[0].onClick.AddListener(OnClickMultiStart);
        multiGameBtns[1].onClick.AddListener(OnClickDestroyData);
        multiGameBtns[2].onClick.AddListener(OnClickHostGame);
        multiGameBtns[3].onClick.AddListener(OnClickOpenGame);
        // multiGameBtns[4].onClick.AddListener(OnClickLANGame);

        for(int i=0; i<tgHostOption.Length ; i++)
        {
            int hostindex = i;  // 이렇게 안 하면 문제 발생 : C#의 closure 개념 때문에 for문이 모두 실행된 후 i 값이 그대로 클로저에 저장되어 계속 4를 가리키게 됩니다
            tgHostOption[i].onValueChanged.AddListener(delegate {OnClickTgHost(hostindex);} );
        }

        for(int i=0; i<tgOpenOption.Length; i++)
        {
            int openindex = i;
            tgOpenOption[i].onValueChanged.AddListener(delegate {OnClickTgOpen(openindex);}); 
        }


// 정리 목적으로 처음에 한번 실행하고 시작 ======================
        OnClickGoBack();    

// 다음씬 넘어가기 임시 버튼 =========================
        // btnGoNextScene.onClick.AddListener(OnClickGoNextScene);  //#14-1

//#4-1 
        // Debug.Log("//#4-2 Start 끝");
//#13-1
        // matEyes = skinnedScout.materials[1];    // 직접 프리팹 확인 -> 1번째가 눈이거든~
        // matBackPack = skinnedBackPack.materials[0];
        // skinnedScout = objScout.GetComponent<SkinnedMeshRenderer>();
        // skinnedBackPack = objBackPack.GetComponent<SkinnedMeshRenderer>();


        // matPlayerEye = skinnedScout.materials[1];
        // matPlayerBackPack = skinnedBackPack.materials[0];
    }

    IEnumerator LoadJSONDataFct()
    {
        Debug.Log("//#4-2 JSON 불러옴");
        // InfoManager.Info.LoadJSONData();    //#4-2 JSON 데이터 로드   //#4-4 나중에 주석 풀 것
        InfoManager.Ins.LoadJSONData();    //#11-6
        yield return new WaitForSeconds(0.5f);
    }
// #9-1 Input Field 글자 수 제한
    void OnInputChange1(string _input1)
    {
        if(_input1.Length > 3)
        {
            inputName.text = _input1.Substring(0,3);      // 3글자 넘어가도 값이 바뀌지 않도록
        }
    }
    void OnInputChange2(string _input2)
    {
        if(_input2.Length > 3)
        {
            inputName.text = _input2.Substring(0,3);      // 3글자 넘어가도 값이 바뀌지 않도록
        }
    }
// #9-1 현재 색 확인용
    // private Color32 HexToColor32(string hex)
    // {
    //     // HEX 문자열을 RGB 값으로 분리
    //     byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
    //     byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
    //     byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

    //     // Color32로 변환하여 반환
    //     return new Color32(r, g, b, 255);
    // }

// '뒤로 가기' 버튼 ======================
    void OnClickGoBack()
    {
        //#9-1 입력한 플레이어 정보 없으면 [게임 불러오기] 버튼 비활성화
        // string _playerName = InfoManager.Info.playerName.Trim('"');
        // string _islandName = InfoManager.Info.islandName.Trim('"');
        string _playerName = InfoManager.Ins.playerName.Trim('"'); //#11-6
        string _islandName = InfoManager.Ins.islandName.Trim('"');


        if(string.IsNullOrEmpty(_playerName) || string.IsNullOrEmpty(_islandName))    // 하나라도 값이 없으면
            inputAllInfo = false;
        else
            inputAllInfo = true;

        Debug.Log("모든 정보 적혀있나요? : " + inputAllInfo);

        loadGameBtn.SetActive(inputAllInfo);    // 플레이어가 입력한 정보가 있을 때에만 [게임 불러오기] 버튼 활성화 시키도록
        multiPlayBtn.SetActive(inputAllInfo);   //#9-4 위와 동일

        loadGame.SetActive(false);

        chooseScreen[2].SetActive(false);   // pnlName 비활성화
        chooseScreen[1].SetActive(false);   // pnlColor 비활성화
        chooseScreen[0].SetActive(true);    // pnlClothes 활성화
        newGame.SetActive(false);

        for(int i=1; i<tgHostOption.Length; i++)    // 토글 모두 꺼놓고 시작
        {
            // Debug.Log("토글1 길이는 : " + tgHostOption.Length);
            tgHostOption[i].isOn = false;
        }
        tgHostOption[0].isOn = true;    // 첫 토글은 체크된 상태로 시작
        for(int i=1; i<tgOpenOption.Length; i++)    // 토글 모두 꺼놓고 시작
        {
            // Debug.Log("토글2");
            tgOpenOption[i].isOn = false;
        }
        tgOpenOption[0].isOn = true;

        multiScreen[3].SetActive(false);    // OpenGames 비활성화
        multiScreen[2].SetActive(true);     // HostGame 활성화 (켜두고)
        multiScreen[1].SetActive(false);    // pnlMulti 비활성화 (마지막으로 끄기~)
        multiScreen[0].SetActive(true);     // pnlPlayerList 활성화  (켜두고)
        multiGame.SetActive(false);
        inputFieldPassWord.SetActive(false);    //비밀번호 누르는 (inputFieldPassWord) 비활성화

        lobby.SetActive(true);

        goBackBtn.gameObject.SetActive(false);
    }

// '로비' 화면 버튼 ======================
    void OnClickLoad()
    {
        if(!playSingleGame)      //#10-1 멀티플레이일 때에만, 다른 플레이어도 들어올 수 있는 방 IngRoomItem 만들기
            playSingleGame = true;

        //#9-1 [게임 불러오기 - 플레이어 박스에 데이터 연결, 저장하기]
        // InfoManager.Info.LoadJSONData();    // 데이터 가져오기
        // loadInfoName[0].text = InfoManager.Info.playerName.Trim('"');   
        // loadInfoName[1].text = InfoManager.Info.islandName.Trim('"');
        loadInfoName[0].text = InfoManager.Ins.playerName.Trim('"');   //#11-6
        loadInfoName[1].text = InfoManager.Ins.islandName.Trim('"');


        Debug.Log("#9-1 플레이어 이름 : " +InfoManager.Ins.playerName);        //#11-6

        string name = InfoManager.Ins.playerName.Trim('"');    //# 큰 따옴표를 제외한 문자열 부분만 가져오기    //#11-6
                        // name = name.Substring(1, name.Length - 2);   //# 수정 확인 필요
        Debug.Log("#9-1 플레이어 이름 name : " + name);

        if(string.IsNullOrEmpty(name))  //#9-1 플레이어 정보 없으면 안 눌리도록
        {
            Debug.Log("정보 없음");
            return;
        }
        
        lobby.SetActive(false);
        loadGame.SetActive(true);

        goBackBtn.gameObject.SetActive(true);
    }
    void OnClickNewGame()
    {
        lobby.SetActive(false);
        newGame.SetActive(true);

        goBackBtn.gameObject.SetActive(true);

        //#4-1 JSON 테스트용    //#4-4 나중에 주석 풀 것
        int num = InfoManager.Ins.clothesNum;   //#11-6
        // playerClothes.sprite = newClothes[num].sprite;

        // playerClothes.color = InfoManager.Ins.clothesColor; //#11-6

    }


    void OnClickMultiGame()
    {
        if(playSingleGame)          //#10-1 멀티플레이일 때에만, 다른 플레이어도 들어올 수 있는 방 IngRoomItem 만들기
            playSingleGame = false;

        loadMInfoName[0].text = InfoManager.Ins.playerName.Trim('"');   //#11-6 
        loadMInfoName[1].text = InfoManager.Ins.islandName.Trim('"');   //#11-6

        lobby.SetActive(false);
        multiGame.SetActive(true);

        goBackBtn.gameObject.SetActive(true);
    }
    void OnClickExit()
    {
        Debug.Log("종료");
        Application.Quit();
    }

// '게임 불러오기' 화면 내 버튼 배열 ======================
    void OnClickSingleStart()
    {
        //싱글 플레이 시작
    }   
    void OnClickDestroyData()
    {
        //싱글 플레이 JSON 데이터 초기화하고, 다시 로비로 이동 
        // InfoManager.Info.InitializeJSONData();   
        InfoManager.Ins.InitializeJSONData();   //#11-6   

        OnClickGoBack();
    }

// '새로운 게임' 화면 내 버튼 배열 ======================
    void OnClickRandomCustomize()
    {
        // 랜덤 커스터마이징 생성
        int randomClothes = UnityEngine.Random.Range(0, newClothes.Length);
        int randomColor = UnityEngine.Random.Range(0, newColor.Length);

        // playerClothes.sprite = newClothes[randomClothes].sprite; //#13-2
        // playerClothes.color = newColor[randomColor].color;

        clothesNum = randomClothes;         //#4-1 //#13-2 JSON 저장 위해 필요함

        materialEye.materials = new Material[2] {matSkin, matCustomEyes[randomClothes] };   //#13-2 커스터마이징 material 적용
        materialBackPack.material = matCustomBackPacks[randomColor];    //#13-2 커스터마이징 material 적용

    }
    void OnClickOpenClothes()
    {
        chooseScreen[2].SetActive(false);
        chooseScreen[1].SetActive(false);   
        chooseScreen[0].SetActive(true);    // 옷 고르기 화면
    }
    void OnClickOpenColors()
    {
        chooseScreen[0].SetActive(false);
        chooseScreen[2].SetActive(false);
        chooseScreen[1].SetActive(true);    // 색 고르기 화면
    }
    void OnClickCheckName()
    {
        //플레이어 이름, 섬 이름 작성하고 역할 생성
        chooseScreen[0].SetActive(false);
        chooseScreen[1].SetActive(false);
        chooseScreen[2].SetActive(true);    // 이름 작성 화면
    }
    void OnClickCreateRole()    //#4-1  //#9-1
    {
        //#4-4 나중에 주석 풀 것
        //역할 생성 
        //JSON 데이터 저장 - 플레이어 이름, 섬 이름, 옷 종류, 옷 색
        InfoManager.Ins.playerName = inputName.text;    //#11-6
        InfoManager.Ins.islandName = inputIslandName.text;  //#11-6
        InfoManager.Ins.clothesNum = clothesNum;           // 옷 종류는 옷 누를 때마다 들어가게 해서    //#11-6

        clothesColor = playerClothes.color;                 //옷 색깔은 한번에 저장 가능하겠다~
        InfoManager.Ins.clothesColor = clothesColor;        //#11-6

        //#9-1 하나라도 입력 안 하면, 확인 버튼 안 눌리도록 return
//         Color _color = HexToColor32("#FFFFFF");
// Debug.Log("내가 선택한 컬러 : " + InfoManager.Info.clothesColor);
// Debug.Log("그냥 백색 컬러 : " + _color);

//#9-1 입력한 플레이어 정보 없으면 [게임 불러오기] 버튼 비활성화
        _playerName = InfoManager.Ins.playerName.Trim('"'); //#11-6
        _islandName = InfoManager.Ins.islandName.Trim('"'); //#11-6

        if(string.IsNullOrEmpty(_playerName) || string.IsNullOrEmpty(_islandName))    // 하나라도 값이 없으면
            inputAllInfo = false;
        else if(! string.IsNullOrEmpty(_playerName) && ! string.IsNullOrEmpty(_islandName))
            inputAllInfo = true;

        if(!inputAllInfo)
            return;
        
// 입력한 데이터 있으면 통과~ JSON에 저장하고, 로비로 이동
        InfoManager.Ins.SaveJSONData();        //작성한 데이터를 JSON에 저장하기    //#11-6
        inputName.text = null;                  
        inputIslandName.text = null;

        Debug.Log("플레이어 이름 : " + InfoManager.Ins.playerName); //#11-6

        // StartCoroutine(LoadJSONDataFct());  // [새로운 게임 눌렀을 때 들어가지도록] - Save하고 바로 로드하니까 저장되기 전 값이 로드됨

        //#9-1 로비로 이동하도록 - ESC와 같은 역할 하면 되겠다~
        OnClickGoBack();

    }
    void OnClickClothes0()
    {
        // 옷0로 변경
        // playerClothes.sprite = newClothes[0].sprite; //#13-2
        //imageToChange.SetNativeSize(); // 이미지 크기를 원래 크기로 설정

        clothesNum = 0;     //#4-1  //#13-2 JSON 저장 위해 필요함
        
        materialEye.materials = new Material[2] {matSkin, matCustomEyes[0] };
        // materialEye.materials[0] = matCustomEyes[0]; //#13-1 커스터마이징 - 눈 종류 0번째
        // matPlayerEye = matCustomEyes[0];
    }
    void OnClickClothes1()
    {   
        // 옷1 로 변경
        // playerClothes.sprite = newClothes[1].sprite; //#13-2
        clothesNum = 1;     //#4-1  //#13-2 JSON 저장 위해 필요함

        materialEye.materials = new Material[2] {matSkin, matCustomEyes[1] };
        // materialEye.materials[0] = matCustomEyes[1]; //#13-1 커스터마이징 - 눈 종류 1번째
        // matPlayerEye = matCustomEyes[1];

    }
    void OnClickClothes2()
    {
        // 옷2 로 변경
        // playerClothes.sprite = newClothes[2].sprite; //#13-2
        clothesNum = 2;     //#4-1  //#13-2 JSON 저장 위해 필요함
        
        materialEye.materials = new Material[2] {matSkin, matCustomEyes[2] };
        // materialEye.materials[0] = matCustomEyes[2]; //#13-1 커스터마이징 - 눈 종류 2번째
        // matPlayerEye = matCustomEyes[2];
    }

    void OnClickColors0()
    {
        // 색0 로 변경
        playerClothes.color = newColor[0].color;    //JSON 위해서 일단 남겨두자 //#13-2

        materialBackPack.material = matCustomBackPacks[0];    //#13-1 커스터마이징 - 옷 색 0번째
        // matPlayerBackPack = matCustomBackPacks[0];
    }
    void OnClickColors1()
    {
        // 색1 로 변경
        playerClothes.color = newColor[1].color;    //JSON 위해서 일단 남겨두자 //#13-2

        materialBackPack.material = matCustomBackPacks[1];    //#13-1 커스터마이징 - 옷 색 1번째
        // matPlayerBackPack = matCustomBackPacks[1];

    }
    void OnClickColors2()
    {
        // 색2 로 변경
        playerClothes.color = newColor[2].color;    //JSON 위해서 일단 남겨두자 //#13-2

        materialBackPack.material = matCustomBackPacks[2];    //#13-1 커스터마이징 - 옷 색 2번째
        // matPlayerBackPack = matCustomBackPacks[2];
    }

    public void OnClickRotation()  //#13-2 회전하기 빙글빙글
    {
        startRotation = customPlayer.transform.rotation;    // 시작 회전값 지정
        
        Vector3 euler = startRotation.eulerAngles;          // 도착 회전값 지정
        euler.y += rotationAngle;
        endRotation = Quaternion.Euler(euler);

        StartCoroutine(RotateCustomPlayer(customPlayer, startRotation, endRotation));
    }

    IEnumerator RotateCustomPlayer(GameObject target, Quaternion start, Quaternion end)
    {
        while(currRotateTime < rotateTime)  //while 해도 괜찮은가?
        {
            float time = currRotateTime / rotateTime;
            target.transform.rotation = Quaternion.Lerp(start, end, time);

            yield return null;

            currRotateTime += Time.deltaTime;
        }

        target.transform.rotation = end;

        currRotateTime = 0f;    //초기화 - 나가면서 한번 초기화 하고 끝내기
    }


 // '멀티 플레이' 화면 내 버튼 배열 ======================
    
    void OnClickMultiStart()
    {
        // pnlMulti 열기 (멀티 플레이 Info 박스 눌렀을 때)
        multiScreen[0].SetActive(false);
        multiScreen[1].SetActive(true);
    }
    void OnClickHostGame()
    {
        // 호스트 게임 창 열기
        multiScreen[3].SetActive(false);
        multiScreen[2].SetActive(true);
    }
    void OnClickOpenGame()
    {
        // 게임 참여하기 창 열기
        multiScreen[2].SetActive(false);
        multiScreen[3].SetActive(true);
    }
    // void OnClickLANGame()
    // {
    //     //LAN 게임 창 열기
    // }

    //토글 =====================
    void OnClickTgHost(int index)   
    {
        // 클릭한 토글 이외의 모든 토글의 체크를 해제한다.
        Debug.Log( index + "번 // 클릭한 토글 이외의 모든 토글의 체크를 해제한다.");
        for(int i=0; i< tgHostOption.Length; i++)
        {
            if( i != index)
            {
                tgHostOption[i].SetIsOnWithoutNotify(false);    
                // isOn으로 켜고 끄는 게 아닌, 값을 직접 바꾸는 SetIsOnWithoutNotify함수를 사용함으로써 이벤트 함수 무한 호출 방지
            }
            if(i == index)
            {
                tgHostOption[i].SetIsOnWithoutNotify(true);
            }
        }

        switch (index)
        {
            case 0 : 
                hostOptExplain.text = "공개방입니다.";
                inputFieldPassWord.SetActive(false);
                break;
            case 1 :
                hostOptExplain.text = "초대만~~ 설명내용";
                inputFieldPassWord.SetActive(false);
                break;
            case 2 :
                hostOptExplain.text = "공개~~ 설명내용";
                inputFieldPassWord.SetActive(false);
                break;
            case 3 : 
                hostOptExplain.text = "비밀방입니다. \n 설정할 비밀번호를 입력하세요.";
                inputFieldPassWord.SetActive(true);
                break;
            default : 
                break;
        }
    }

    void OnClickTgOpen(int index)  
    {
        // 클릭한 토글 이외의 모든 토글의 체크를 해제한다.
        for(int i=0; i< tgOpenOption.Length; i++)
        {
            if( i != index)
            {
                tgOpenOption[i].SetIsOnWithoutNotify(false);    
                // isOn으로 켜고 끄는 게 아닌, 값을 직접 바꾸는 SetIsOnWithoutNotify함수를 사용함으로써 이벤트 함수 무한 호출 방지
            }
            if(i == index)
            {
                tgOpenOption[i].SetIsOnWithoutNotify(true);
            }
        }
    }

    // void OnClickGoNextScene()    //#14-1
    // {
    //     GameObject.Find("OptionManager").GetComponent<AudioSource>().Stop();     //1 노래 멈춰
    //     GameObject.Find("OptionCanvas").GetComponent<Canvas>().enabled = false;  //2 캔버스 숨겨

    //     SceneManager.LoadScene("scPlayUI0");    //씬 이동
    // }


//포톤 관련 - 나중에
// void OnReceivedRoomListUpdate()
//     {
//         // 포톤 클라우드 서버에서는 룸 목록의 변경이 발생하면 클라이언트로 룸 목록을 재전송하기
//         // 때문에 밑에 로직이 없으면 다른 클라이언트에서 룸을 나갈때마다 룸 목록이 쌓인다.
//         // 룸 목록을 다시 받았을 때 새로 갱신하기 위해 기존에 생성된 RoomItem을 삭제  
//         foreach(GameObject obj in GameObject.FindGameObjectsWithTag("ROOM_ITEM"))
//         {
//             Destroy(obj);
//         }

// //#19 제로베이스로 만들어주고~
//         //Grid Layout Group 컴포넌트의 Constraint Count 값을 증가시킬 변수
//         int rowCount = 0;
//         //스크롤 영역 초기화
//         //scrollContents.GetComponent<RectTransform>().sizeDelta = new Vector2(0 ,0);
//         loadScrollContents.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

// //# 방정보를 바탕으로 프리팹 만들기
//         //수신받은 룸 목록의 정보로 RoomItem 프리팹 객체를 생성
//         //GetRoomList 함수는 RoomInfo 클래스 타입의 배열을 반환
//         foreach(RoomInfo _room in PhotonNetwork.GetRoomList())
//         {
//             Debug.Log(_room.name);
//             //RoomItem 프리팹을 동적으로 생성 하자
//             GameObject room = (GameObject)Instantiate(roomItem);
//             //생성한 RoomItem 프리팹의 Parent를 지정    //# 지금까지 하던 함수 말고 이렇게도 Parent 지정이 가능하구나~
//             room.transform.SetParent(loadScrollContents.transform, false);

//             /*
//              * room.transform.parent = scrollContents.transform;
//              * 
//              * (자식 게임오브젝트).transform.parent = (부모 게임오브젝트).transform;
//              * 이 방법보다는 UI 항목을 차일드화할 때는 스케일과 관련된 문제가 발생할
//              * 수 있기 때문에 앞에서 사용한 방법보다 SetParent 메서드를 사용하는 것이 
//              * 편리함. worldPositionStays 인자를 false로 설정하면 로컬 기준의 정보를 유지한 채
//              * 차일드화 된다. (그냥 전 경고 메시지가 안떠서 이걸로 하는게 좋은거 같아요)
//              * 
//              */

//              //생성한 RoomItem에 룸 정보를 표시하기 위한 텍스트 정보 전달
//             RoomData roomData = room.GetComponent<RoomData>();
//             roomData.roomName = _room.Name;
//             roomData.connectPlayer = _room.PlayerCount;
//             roomData.maxPlayers = _room.MaxPlayers;

//             //텍스트 정보를 표시 
//             roomData.DisplayRoomData();

// //#19 버튼과 동적으로 연결
//             //RoomItem의  Button 컴포넌트에 클릭 이벤트를 동적으로 연결 
//             roomData.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { OnClickRoomItem(roomData.roomName); Debug.Log("Room Click " + roomData.roomName); });

//             /*
//              * delegate (인자) { 실행코드 };  => 인자는 생략 가능하다
//              * delegate (room.name) { OnClickRoomItem( room.name ); Debug.Log("Room Click " + room.name); };
//              * delegate { OnClickRoomItem( roomData.roomName ); };
//              */
// //#19 스크롤바 Contents 위치 맞춰주기
//             //Grid Layout Group 컴포넌트의 Constraint Count 값을 증가시키자
//             loadScrollContents.GetComponent<GridLayoutGroup>().constraintCount = ++rowCount;
//             //스크롤 영역의 높이를 증가시키자
//             loadScrollContents.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 20);
//         }

//     }   



}
