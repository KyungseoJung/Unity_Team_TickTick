using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//이 선언이 있어야 UI관련 컴포넌트를 연결 및 사용 가능.
using UnityEngine.UI;
using UnityEngine.SceneManagement;		

//현재 스크립트에서 넓게는 현재 게임오브젝트에서 반드시 필요로하는 컴포넌트를 Attribute로 명시하여 해당 컴포넌트의 자동 생성 및 삭제되는 것을 막는다.
[RequireComponent(typeof(AudioSource))]

public class OptionManager : csGenericSingleton<OptionManager>
{
    //오디오 클립 저장 배열 선언
	public AudioClip[] soundFile;
    //사운드 Volume 설정 변수
	public float soundVolume = 1.0f;
	//사운드 Mute 설정 변수 
	public bool isSoundMute = false;
	//슬라이더 컴포넌트 연결 변수 
	public Slider sl;
	//토글 컴포넌트 연결 변수 
	public Toggle tg;
	//Sound 오브젝트 연결 변수 
	public GameObject Option;


	public GameObject OpenOptionBtn;    //Sound Ui버튼 오브젝트 연결 변수 
    AudioSource audio;


    public AudioClip clickClip;     // 클릭 효과음

// 버튼 연결하기 ======================
    [Header("버튼 연결하기 추가")]
    [Space(10)]    
	public Button btnVolumeDown;
	public Button btnVolumeUp;
	public Button btnCloseOption;
	
	[Header("토글, 슬라이더 연결하기 추가")]
	public Toggle tgVolume;
	public Slider slVolume;


    protected override void Awake()
    {
		base.Awake();
		
        audio = GetComponent<AudioSource>();

        LoadData();
    }

    void Start()
    {
        if(Option.activeSelf)
			Option.SetActive(false);
        
        if(!OpenOptionBtn.activeSelf)
			OpenOptionBtn.SetActive (true);

        soundVolume = sl.value;
		isSoundMute = tg.isOn;

        AudioSet ();
// 버튼 연결 ========================================================
		btnVolumeDown.onClick.AddListener(OnClickVolumeDown);
		btnVolumeUp.onClick.AddListener(OnClickVolumeUp);
		btnCloseOption.onClick.AddListener(OnClickCloseOption);
// 토글 및 슬라이더 연결
		tgVolume.onValueChanged.AddListener(OnClickTgSetSound);
		slVolume.onValueChanged.AddListener(OnClickSlSetSound);


    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0)) // 마우스 클릭시마다 효과음
        {
            Debug.Log("마우스 클릭");
            PlayEffect(transform.position, clickClip);
        }
    }
	//AudioSource 셋팅 (사운드 UI에서 설정 한 값의 적용 )
	void AudioSet(){
		//AudioSource의 볼륨 셋팅 
		//GetComponent<AudioSource>().volume = soundVolume;
        audio.volume = soundVolume;

		//AudioSource의 Mute 셋팅 
		//GetComponent<AudioSource>().mute = isSoundMute;
        audio.mute = isSoundMute;

        Debug.Log("sl.value : " + sl.value); //숫자 확인용
	}


    //@3-4
	//스테이지 시작시 호출되는 함수
	public void PlayBackground(int stage)
	{
		// GetComponent<AudioSource>().clip = soundFile[stage-1];
		// AudioSet();
		// GetComponent<AudioSource>().Play();

		//위에 처럼 적으면 꿀밤~ GetComponent는 Awake에 따로 레퍼런스 선언해주자
		    // AudioSource의 사운드 연결
		audio.clip = soundFile[stage-1];
			// AudioSource 셋팅 
		AudioSet();
		    // 사운드 플레이. Mute 설정시 사운드 안나옴
		audio.Play();
	}

	//@3-5 함수 추가 PlayEffect ========================================================
	//사운드 공용함수 정의(어디서든 동적으로 사운드 게임오브젝트 생성)
	public void PlayEffect(Vector3 pos, AudioClip sfx)
	{
		//Mute 옵션 설정시 이 함수를 바로 빠져나가자.
		if(isSoundMute)
		{
			return;
		}

		//게임오브젝트의 동적 생성하자.
		GameObject _soundObj = new GameObject("sfx");
		//사운드 발생 위치 지정하자. 
		_soundObj.transform.position = pos;	//@ 위치를 몬스터의 위치로 잡기 위해(월드 좌표(?))
		//생성한 게임오브젝트에 AudioSource 컴포넌트를 추가하자.
		AudioSource _audioSource = _soundObj.AddComponent<AudioSource>();
		//AudioSource 속성을 설정 
		//사운드 파일 연결하자.
		_audioSource.clip = sfx;	//@사운드 클립(파일)을 sfx로 연결하자
		//설정되어있는 볼륨을 적용시키자. 즉 soundVolume 으로 게임전체 사운드 볼륨 조절.
		_audioSource.volume = soundVolume;	//@볼륨도 설정한 볼륨으로 맞추자
		//사운드 3d 셋팅에 최소 범위를 설정하자.
		_audioSource.minDistance = 15.0f;	//@사운드 작은 구 범위 = 15로 맞추겠다~
		//사운드 3d 셋팅에 최대 범위를 설정하자.
		_audioSource.maxDistance = 30.0f;	

		//사운드를 실행시키자.
		_audioSource.Play();
		
		//모든 사운드가 플레이 종료되면 동적 생성된 게임오브젝트 삭제하자.
		Destroy(_soundObj, sfx.length + 0.2f ); //@3-5 팁 : 여기에 0.2f정도 더해줘야 약간 끊기는 거 해결

	}

    public void SaveData()
    {
        PlayerPrefs.SetFloat("SOUNDVOLUME", soundVolume);	//float 그대로 "SOUNDVOLUME"에 저장
  		//PlayerPrefs 클래스 내부 함수에는 bool형을 저장해주는 함수가 없다.
        //bool형 데이타는 형변환을 해야  PlayerPrefs.SetInt() 함수를 사용가능
		PlayerPrefs.SetInt("ISSOUNDMUTE", System.Convert.ToInt32(isSoundMute));	//bool형을 정수형으로 "ISSOUNDMUTE"에 저장하고 있음
    }
    public void LoadData()
    {
        sl.value = PlayerPrefs.GetFloat("SOUNDVOLUME");	//float형 그대로 sl.value에 로드
		  //int 형 데이타는 bool 형으로 형변환.
		tg.isOn = System.Convert.ToBoolean(PlayerPrefs.GetInt("ISSOUNDMUTE")); //bool형은 다시 int형으로 전환해서 tg.isOn에 로드

		 // 첫 세이브시 설정 -> 이 로직없으면 첫 시작시 사운드 볼륨 0
		int isSave = PlayerPrefs.GetInt("ISSAVE");	//처음으로 저장하는 거면 0으로 저장되겠지
		if(isSave ==0)	//처음 로드하는 거라면, 아래 값으로 로드되도록
		{	
			sl.value = 1.0f;
			tg.isOn = false;
			// 첫 세이브는 soundVolume = 1.0f; isSoundMute = false; 이 디폴트 값으로 저장 된다.
			SaveData();
			PlayerPrefs.SetInt("ISSAVE",1);	//이제 1로 저장했음. 다음부턴 if문 안 타
		}
    }
// 여기까지는 기본 함수 =================================================================
//Slider 와 Toggle 컴포넌트에서 이벤트 발생시 호출해줄 함수를 선언 (public 키워드에 의해 외부접근 가능)
	public void OnClickTgSetSound(bool value)
    {
		Debug.Log("토글 value : " + value);
		soundVolume = sl.value;
		isSoundMute = tg.isOn;
		AudioSet ();
	}
	public void OnClickSlSetSound(float value)
	{
		Debug.Log("슬라이더 value : " + value);
		soundVolume = sl.value;
		isSoundMute = tg.isOn;
		AudioSet ();
	}
    //사운드 UI 창 오픈 
	public void OnClickOpenOption()
    {
		// 사운드 UI 활성화 
		Option.SetActive (true); 
		// 사운드 UI 오픈 버튼 비활성화 
		OpenOptionBtn.SetActive(false);
	}

	//사운드 UI 창 닫음
	public void OnClickCloseOption(){
		// 사운드 UI 비 활성화 
		Option.SetActive (false);
		// 사운드 UI 오픈 버튼 활성화 
		OpenOptionBtn.SetActive (true);

		SaveData();	//@4-1
	}

    //볼륨 Down
    public void OnClickVolumeDown()
    {
        if(sl.value >0)
        {
            sl.value -= 0.1f;
		    AudioSet ();
        }
        
    }
    //볼륨 Up
    public void OnClickVolumeUp()
    {
        if(sl.value <1)
        {
            sl.value += 0.1f;
		    AudioSet ();
        }
        
    }



}
