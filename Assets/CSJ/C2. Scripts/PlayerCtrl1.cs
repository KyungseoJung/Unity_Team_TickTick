using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamInterface;
using UnityEngine.UI;

public class PlayerCtrl1 : MonoBehaviour, IObjectStatus, IPhotonBase, IPhotonInTheRoomCallBackFct
{
    // 스피드 조정 변수
    [SerializeField]
    private float applySpeed;
    //[SerializeField]
    //private float runSpeed;
    //[SerializeField]
    //private float crouchSpeed;

    // 점프 정도
    //[SerializeField]
    //private float jumpForce;

    // 상태 변수
    //private bool isRun = false;
    private bool isGround = true;
    //private bool isCrouch = false;

    // 앉았을 때 얼마나 앉을지 결정하는 변수
    //[SerializeField]
    //private float crouchPosY;
    //private float originPosY;
    //private float applyCrouchPosY;

    // 민감도
    //[SerializeField]
    //private float lookSensitivity;

    // 카메라 한계
    //[SerializeField]
    //private float cameraRotationLimit;
    //private float currentCameraRotationX;

    // 필요한 컴포넌트
    [SerializeField]
    private Camera theCamera;
    public Transform camPos;
    private Rigidbody myRigid;
    private CapsuleCollider capsuleCollider;

    Animator anim;

    float currentHP = 0;
    public float Hp { get { return currentHP; } set { currentHP = value; } }

    float stamina = 0;
    public float Stamina { get { return stamina; } set { stamina = value; } }

    GameObject[] dropItems;
    public GameObject[] DropItems { get { return dropItems; } set { dropItems = value; } }


    //드랍 아이템시 아이템 떨굴 위치 저장용 변수
    public Transform dropItemPos;

    /// <summary>
    /// ///////////////////////////////////////
    /// 포톤 추가
    /// </summary>
    /// 
    //PhotonView 컴포넌트를 할당할 레퍼런스 
    [SerializeField]
    PhotonView pV;
    public PhotonView pv { get { return pV; } set { pV = value; } }

    //위치 정보를 송수신할 때 사용할 변수 선언 및 초기값 설정 
    [SerializeField]
    Vector3 cUrrPos;
    //private Vector3 currPos;
    //public Vector3 CurrPos { get { return currPos; } set { currPos = value; } }

    public Vector3 currPos {
    get { return transform.position; }
    set { transform.position = value; }
}

    [SerializeField]
    Quaternion cUrrRot;
    public Quaternion currRot { get { return cUrrRot; } set { cUrrRot = value; } }

    //Rigidbody myRbody;
    public Transform camPivot;
    
    //private bool isWriting;
    /// <summary>
    /// ///////////////////////////////////////////
    /// </summary>
    //bool isDie = false;

    //float damage;

     [SerializeField]
     public float maxHP;

    [SerializeField]
    private HPBar hpBar;

    private Transform Tr; //자신의 트랜스폼 참조 변수

    float mouseX=0f;
    float mouseY=0f;

    //집에들어가면 움직이는 방법 변경
    public bool inTheHouse = false;
    public bool canTrigger = false;//제작대 앞에 섰을 때 키입력으로 제작창 열 수 있는 체크용
    public bool blockKeyDownE = false;//키 한번 눌리면 0.2초뒤 다시 입력 가능 아니면 겁나 많이 눌림

    //캐릭터 빙글빙글 도는 이슈 해결용
    public Vector3 oldRot;

    //튜토리얼 끝나기전까지 못움직여 ~~
    public bool gameStart = false;


    //플레이어 얼굴 커스터마이징 추가~ 
    //public Material[] faceMaterials = new Material[3]; //플레이어 얼굴 배열

    private int faceindex; //플레이어 얼굴 변수


    //플레이어 백팩 커스터마이징 추가~
    //public Material[] BackMaterials = new Material[2]; //플레이어 백팩 배열
    private int Backindex; //플레이어 백팩 변수

    // private int Dressindex; // 플레이어 옷 변수
    private int Dressindex;

    //public Material[] dressMaterials = new
    private Color32 backPackColor;

    bool isLoadCustom=false;

    public GameObject hideObj;
    public GameObject oculusObj;
    bool isOM = false;

    public Transform smilePos;

    public void SetOulusMode(bool tmpCheck)
    {
        isOM = tmpCheck;
        Debug.Log(tmpCheck + " 셋 오큘러스 버튼 눌림");

        hideObj.SetActive(!tmpCheck);
        oculusObj.SetActive(tmpCheck);
    }

    public void SetInTheHouse(bool a)
    {
        inTheHouse = a;
    }
    private void Awake()
    {   

        Tr = GetComponent<Transform>();  
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        pV = GetComponent<PhotonView>();
        anim = GetComponent<Animator>();        

        pv.ObservedComponents[0] = this;
        pv.synchronization = ViewSynchronization.UnreliableOnChange;


        if(pv.isMine) // PhotonNetwork.isMasterClient 마스터 클라이언트는 이런식 체크
        {
            GameObject[] tmpObj = GameObject.FindGameObjectsWithTag("PhotonGameManager");

            foreach (GameObject obj in tmpObj)
            {
                if (obj.GetComponent<csPhotonGame>().GetOwnerID() == pv.photonView.ownerId)
                {
                    Debug.Log(pv.photonView.ownerId + "이건왜안타?" + obj.GetComponent<csPhotonGame>().GetOwnerID());
                    m_grid2D = GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>();
                    m_grid2D.myPlyerCtrl = this;
                    break;
                }
            }

            m_grid2D.myPlyerCtrl = this;

            //메인 카메라에 추가된 SmoothFollowCam 스크립트(컴포넌트)에 추적 대상을 연결 
            //Camera.main.GetComponent<SmoothFollowCam>().target = camPivot;
         }
        else //자신의 네트워크 객체가 아닐때...
        {
            gameObject.GetComponent<CapsuleCollider>().enabled = false;
            myRigid.isKinematic = true;
        }


        currPos = Tr.position; 
        currRot = Tr.rotation;

        //pv.viewID = PhotonNetwork.AllocateViewID();
    }

    IEnumerator Start()
    {
       // Debug.Assert(m_grid2D);

        yield return new WaitForSeconds(7.0f);

        //  if(pv.isMine)   //#20-1
        // {
        //     // 일정 간격으로 주변의 가장 가까운 Enemy를 찾는 코루틴 
        //     StartCoroutine(this.TargetSetting());

        // // // 가장 가까운 적을 찾아 발사...
        //     StartCoroutine(this.ShotSetting());
        // }

        // 컴포넌트 할당

        if (pv.isMine)
        {
            theCamera = Camera.main;

            Debug.Assert(theCamera);

            theCamera.transform.SetParent(camPos);
            theCamera.transform.position = camPos.transform.position;
            theCamera.transform.localRotation = camPos.transform.localRotation;
        }

       //originPosY = theCamera.transform.localPosition.y;
       // applyCrouchPosY = originPosY;
        currentHP = maxHP;

        //Debug.Log(currentHP +"..."+ maxHP);
        //hpBar.SetMaxHealth(maxHP);
        //hpBar.UpdateHPBar(currentHP, maxHP);

        //PhotonView pV = GetComponent<PhotonView>();

        // if(pv.isMine){

        //         // isLoadCustom=true;

        //플레이어 얼굴 커스터마이징 추가
        faceindex = InfoManager.Ins.clothesNum; //얼굴 초기화
        UpdateFace(faceindex); //얼굴 업데이트 함수    //JSON 데이터 연결


        //플레이어 백팩 커스터마이징 추가
        Backindex = 0; //백팩 초기화
        switch(ColorToHex(InfoManager.Ins.clothesColor))        //JSON 데이터 연결
        {
            case ("#7ED67F") : // 초록색
                Backindex = 0;
                break;
            case ("#FF8676") :  // 빨간색
                Backindex = 1;
                break;
            case ("#FFF884") :   // 노란색
                Backindex = 2;
                break;
        }
        UpdateBack(Backindex);  //백팩 업데이트 함수 


        //플레이어 옷 커스터마이징 추가
        // Dressindex = 0; //옷 초기화
        Dressindex = Random.Range(0, dressMat.Length);
        UpdateDress(Dressindex);  //옷 업데이트


        // }


    }

    void Update()
    {
        if (!pv.isMine && gameStart)
        {
            MoveAvarta();
            return;
        }
        else if (!pv.isMine)
        {
            return;
        }

        if (isOM)
        {
            return;
        }

        //튜토리얼 안끝났으면 아무것도 안해~
        if (!gameStart)
        {
            gameStart = m_grid2D.GetGameStart();//언제 트루되는지 계속 물어봄
            return;
        }

        if (m_grid2D == null)
        {
            return;
        }


        if (m_grid2D.isUiBlock)
        {
            transform.eulerAngles = oldRot;

            return;
        }

        //임시
        //hpBar.UpdateHPBar(currentHP, maxHP);
        IsGround();
        //TryJump();
        //TryRun();
        //TryCrouch();

        if (inTheHouse)
        {
            Move();
        }

        //Move();
        //MoveBlock();


        //CameraRotation();
        CharacterRotation();

       
        //float _moveDirX = Input.GetAxisRaw("Horizontal") *0.5f;
        //float _moveDirZ = Input.GetAxisRaw("Vertical") * 0.5f;

        //Vector3 _moveHorizontal = transform.right * _moveDirX;
        //Vector3 _moveVertical = transform.forward * _moveDirZ;
        //Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

        //transform.position += _velocity * Time.deltaTime;
        
        if(canTrigger && !blockKeyDownE && Input.GetKey(KeyCode.E))
        {
            blockKeyDownE = true;
            Invoke("ResetBlockKeyDownE", 0.2f);
            //GameObject.FindGameObjectWithTag("CraftingUI").SetActive(true);
            //GameObject.Find("CraftingCanvas").SetActive(true);

            if (m_grid2D.tPlayer.HaveEmptySpace())   // 남은 자리 있어야 제작대 켜지도록
            {
                if(m_grid2D.tPlayer == null)
                {
                    m_grid2D.tPlayer = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
                }

                if (m_grid2D.craftingUI == null)
                {
                    m_grid2D.craftingUI = m_grid2D.tPlayer.CraftingUI;
                }
                else
                {
                    m_grid2D.craftingUI.SetActive(true);
                }
            }
            else
            {
                m_grid2D.tPlayer.OpenWarningWindow();
            }
        }      
    }

    public void ResetBlockKeyDownE()
    {
        blockKeyDownE = false;
    }

    private void LateUpdate()
    {
        if (!pv.isMine)
        {
            return;
        }

        if (m_grid2D.isUiBlock)
        {
            return;
        }

        MoveBlock();
    }

    // 지면 체크
    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
    }

    // 점프 시도
    // private void TryJump()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space) && isGround)
    //     {
    //         Jump();
    //     }
    // }

    // // 점프
    // private void Jump()
    // {
    //     if (isCrouch)
    //         Crouch();

    //     myRigid.velocity = transform.up * jumpForce;
    // }

    // // 달리기 시도
    // private void TryRun()
    // {
    //     if (Input.GetKey(KeyCode.LeftShift))
    //     {
    //         Running();
    //     }
    //     if (Input.GetKeyUp(KeyCode.LeftShift))
    //     {
    //         RunningCancel();
    //     }

       
    // }

    // // 달리기
    // private void Running()
    // {
    //     if (isCrouch)
    //         Crouch();

    //     isRun = true;
    //     applySpeed = runSpeed;

        
    // }

    // // 달리기 취소
    // private void RunningCancel()
    // {
    //     isRun = false;
    //     applySpeed = walkSpeed;
    // }

    // // 앉기 시도
    // private void TryCrouch()
    // {
    //     if (Input.GetKeyDown(KeyCode.LeftControl))
    //     {
    //         Crouch();
    //     }

    // }

    // // 앉기 동작
    // private void Crouch()
    // {
    //     isCrouch = !isCrouch;
    //     if (isCrouch)
    //     {
    //         applySpeed = crouchSpeed;
    //         applyCrouchPosY = crouchPosY;
    //     }
    //     else
    //     {
    //         applySpeed = walkSpeed;
    //         applyCrouchPosY = originPosY;
    //     }

    //     StartCoroutine(CrouchCoroutine());

    // }

    // // 부드러운 앉기 동작
    // IEnumerator CrouchCoroutine()
    // {
    //     float _posY = theCamera.transform.localPosition.y;
    //     int count = 0;

    //     while (_posY != applyCrouchPosY)
    //     {
    //         count++;
    //         _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.2f);
    //         theCamera.transform.localPosition = new Vector3(0, _posY, 0);
    //         if (count > 15)
    //             break;
    //         yield return null;
    //     }
    //     theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0);
    // }

    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);

        anim.SetBool("isWalk", _velocity != Vector3.zero);
     }

    public void MoveAvarta()
    {
        //원격 플레이어의 아바타를 수신받은 위치까지 부드럽게 이동시키자
        Tr.position = Vector3.Lerp(Tr.position, currPos, Time.deltaTime * 3.0f);
        //원격 플레이어의 아바타를 수신받은 각도만큼 부드럽게 회전시키자
        Tr.rotation = Quaternion.Slerp(Tr.rotation, currRot, Time.deltaTime * 3.0f);

    }


    private void CameraRotation()
    {
        // float _xRotation = Input.GetAxisRaw("Mouse Y");
        // float _cameraRotationX = _xRotation * lookSensitivity;

        // currentCameraRotationX -= _cameraRotationX;
        // currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        // theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }

    private void CharacterRotation()
    {
        // float _yRotation = Input.GetAxisRaw("Mouse X");
        // Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        // myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
        //transform.Rotate(0, Input.GetAxis("Horizontal")*5, 0);

        if (theCamera == null)
        {
            return;
        }

        mouseX += Input.GetAxis("Mouse X") * 2;
        mouseY += Input.GetAxis("Mouse Y") * 2;


        mouseX %= 360f;
        mouseY = Mathf.Clamp((mouseY), -50f, 35f);
        //Debug.Log(mouseX + "//" + mouseY + "마우스 xy");

        //transform.eulerAngles = new Vector3(-mouseY,mouseX, 0);
        transform.eulerAngles = new Vector3(0,mouseX, 0); // 캐릭터 좌우 회전
        theCamera.transform.eulerAngles = new Vector3(-mouseY, mouseX, 0);// 카메라 상하, 좌우 회전


        if (oldRot != transform.eulerAngles)
        {
            oldRot = transform.eulerAngles;
        }//현재 회전 정보 저장

        //theCamera.transform.eulerAngles = new Vector3(mouseY,0, 0);
        //Vector3 tpmCamPos =new Vector3( -mouseY, transform.rotation.y,0);
        //camPos.transform.rotation = Quaternion.Euler(tpmCamPos);
        //theCamera.transform.LookAt(camPos);
        //위아래 회전

        //camPos.localRotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.rotation.z);
        //camPos.transform.eulerAngles = new Vector3(Mathf.Clamp((-mouseY),-35f,50f), 0, 0);
    }

    // public void DamageProcess(int damage)
    // {
    //     Debug.Log("Player damaged by " + damage + " points.");
    //     // 여기에 플레이어가 데미지를 입었을 때 처리할 내용을 추가할 수 있습니다.
    // }

    // public void HealProcess(int healAmount)
    // {
    //     Debug.Log("Player healed by " + healAmount + " points.");
    //     // 여기에 플레이어가 회복 아이템을 사용했을 때 처리할 내용을 추가할 수 있습니다.

    // }

    public void DeathProcess()
    {
        //Debug.Log("Player is dead.");
        // 여기에 플레이어가 죽었을 때 처리할 내용을 추가할 수 있습니다.
    }

    public void SetHpDamaged(float damage, Enum_PlayerUseItemType Type)
    {
        currentHP -= damage;
        if (currentHP < 0)
        {
            currentHP = 0;
        }

        //HPBar 업데이트
        hpBar.UpdateHPBar(currentHP, maxHP);
    }

    public float HpFill()
    {
        return currentHP / maxHP;
    }

    //커스터마이징 적용
    public Renderer backpack;
    public Material[] backpackMat;

    public Renderer dress;
    public Material[] dressMat;

    public Renderer face;
    public Material originBody; 
    public Material[] faceMat;

    //public Renderer snow;
    //public Material[] snowMat;

    //플레이어 얼굴 커스터마이징 추가
    // private void UpdateFace(int index=0)
    // {   
    //     Material[] tmpMats = face.materials;
    //     tmpMats[1] = snowMat[index];
        //플레이어 얼굴 오브젝트 찾기! 플레이어 오브젝트의 3번째 자식이기 때문에 find로 못찾음!!
        // Transform faceTransform = transform.GetChild(2);

        // if (faceTransform == null) //얼굴 위치가 안불러와진다면 디버그 출력!
        // {
        //     Debug.LogError ("얼굴 안불러와져~");

        //     return;

        // }

        // SkinnedMeshRenderer faceRenderer = faceTransform.GetComponent<SkinnedMeshRenderer>(); //플레이어 얼굴 랜더러 컴포넌트 연결
        // if (faceRenderer != null && faceindex >= 0 && faceindex < faceMaterials.Length)
        // {   
        //     Material[] materials = faceRenderer.materials; //얼굴 머트리얼 배열을 연결

        //     //materials[0] = materials[0]; //첫 번째 요소는 그대로 유지! (옷 텍스쳐라서)
        //     materials[1] = faceMaterials[faceindex]; //두 번째 요소만 바꿔주기

        //     faceRenderer.materials = materials;
        //     //faceRenderer.material = faceMaterials[faceindex];
        // }
        // else
        // {
        //     Debug.LogError ("얼굴 머트리얼 못찾겠어~");
        // }
    //}

    // //플레이어 얼굴 커스터마이징 추가
    // public void ChangeFace(int index) //선택한 얼굴로 바꿔주기
    // {
    //     faceindex = index;
    //     UpdateFace(); //얼굴을 업데이트 해준다~
    // }


    //플레이어 얼굴 커스터마이징 추가
    private void UpdateFace (int index = 0)
    {
        face.materials = new Material[2] {originBody, faceMat[index]};
    }

    //플레이어 옷 커스터마이징 추가
    private void UpdateDress(int index = 0)
    {   
        dress.material = dressMat[index];
    }

    //플레이어 백팩 커스터마이징 추가
    private void UpdateBack(int index=0)
    {
        backpack.material = backpackMat[index];
        // Transform BackTransform = transform.Find("BackPack"); //백팩은 플레이어의 첫 번째 자식이기 때문에 find 함수 사용 가능

        // if (BackTransform == null)
        // {
        //     Debug.LogError ("백팩 안불러와져~");

        //     return;
        // }

        // SkinnedMeshRenderer BackRenderer = BackTransform.GetComponent<SkinnedMeshRenderer>(); //플레이어 백팩 랜더러 컴포넌트 연결

        // if(BackRenderer == null )
        // {
        //     Debug.Log("//#13-1 BackRenderer 없음");
        // }

        // if(Backindex < BackMaterials.Length)
        // {
        //     Debug.Log("//#13-2 범위 버그");
        // }

        // if (BackRenderer != null && Backindex < BackMaterials.Length)
        // {   
        //     Material[] materials = BackRenderer.materials;
        //     materials[0] = BackMaterials[Backindex]; //얘는 0번째 인덱스 바꿔도 상관없으니까 바꿔줌.
        //     BackRenderer.materials = materials;
        //     //faceRenderer.material = faceMaterials[faceindex];
        // }
        // else
        // {
        //     Debug.LogError ("백팩 머트리얼 못찾겠어~");
        // }
        
    }


    //플레이어 옷 추가....
    // public void ChangeDrees(int index)
    // {
    //     Dressindex = index;
    //     UpdateDress();
    // }

    // //플레이어 백팩 커스터마이징 추가
    // public void ChangeBack(int index)
    // {
    //     Backindex = index;
    //     UpdateBack();
    // }

    // //플레이어 얼굴 커스터마이징 추가
    // public void ChangeFace(int index)
    // {
    //     faceindex = index;
    //     UpdateFace();
    // }




    /// <summary>
    /// ///////////////////
    /// 포톤추가
    /// </summary>
    /// 

    //PhotonView 컴포넌트의 Observe 속성이 스크립트 컴포넌트로 지정되면 PhotonView
    //컴포넌트는 데이터를 송수신할 때, 해당 스크립트의 OnPhotonSerializeView 콜백 함수를 호출한다.
   public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // 내가 보내는 경우 
            stream.SendNext(Tr.position);
            stream.SendNext(Tr.rotation);
            
            // stream.SendNext(faceindex);
            // stream.SendNext(Backindex);
        }
        else
        {
            // 다른 플레이어가 보내는 경우 
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();

          
                // isLoadCustom=true;
                // faceindex = (int)stream.ReceiveNext();
                // Backindex = (int)stream.ReceiveNext();
          
               //if(!isLoadCustom){ UpdateFace(faceindex);   UpdateBack(Backindex); isLoadCustom = true;  }
        }
    }

    // 네트워크 객체 생성 완료시 자동 호출되는 함수
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        //...
    }

    // 마스터 클라이언트가 변경되면 호출
    public void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        //...
    }

    ////////////////////////////////////
    ///

    //A*
    [Header("A* 관련-플레이어가 찾아갈 길")]
    [SerializeField]
    private List<Node> m_path = new List<Node>(); //경로를 나타내기 위한 노드 목록을 저장

    [Header("A* 관련-패스파인딩")]
    public csPhotonGame m_grid2D; //2d 그리드
    public List<Node> m_closedList = new List<Node>(); //닫힌 노드를 담은 리스트
    public List<Node> m_openList = new List<Node>(); //열린 노드를 담은 리스트
    public Node m_currNode; //현재 노드
    public Node m_startNode; //시작 노드
    public Node m_targetNode; //목표 노드
    public Node m_prevNode; //직전에 탐색한 노드 (이전노드)
    public List<Node> m_pathNode; //현재 탐색된 경로를 저장
    public List<Node> m_currNeighbours = new List<Node>(); //현재 노드의 이웃 노드들을 저장
    public bool m_execute = false; //길찾기가 실행 중인지 여부를 저장

    public void SetPath(List<Node> path) //입력 받은 노드 목록을 이용해 경로를 설정하는 함수
    {
        if (path == null) //입력값이 null이면 실행 종료
        {
            return;
        }

        m_path.Clear(); //노드 목록 초기화

        foreach (Node p in path) //입력받은 path의 모든 노드를 리스트에 추가
        {
            m_path.Add(p);
        }
    }

    public void MoveBlock()//목적지에 도착했을 때 다음 노드 탐색해야 함
    {
        if (m_path.Count > 0) //path 리스트에 노드가 존재하는 경우
        {
            anim.SetBool("isWalk", true); //걷기 애니메이션 실행

            //Vector3 dir = m_path[0].transform.position - transform.position;

            //dir.Normalize();

            //transform.Translate(dir * Time.deltaTime * (applySpeed));

            /*
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(m_path[0].transform.position.x, transform.position.y + 0.284f, m_path[0].transform.position.z), 0.1f);

            float distance = Vector3.Distance(new Vector3(m_path[0].transform.position.x,0, m_path[0].transform.position.z), new Vector3(transform.position.x,0, transform.position.z));

            if (distance <= 0.2f)
            {
                transform.position = new Vector3(m_path[0].transform.position.x, transform.position.y, m_path[0].transform.position.z);
                m_path.RemoveAt(0);
            }
            */

            /*
            // 다음 목적지로 이동하기
            Vector3 direction = (new Vector3(m_path[0].transform.position.x, transform.position.y + 0.284f, m_path[0].transform.position.z) - transform.position).normalized;
            transform.position += direction * applySpeed * Time.fixedDeltaTime;

            // 목적지에 도착했는지 검사하기
            if (Vector3.Distance(new Vector3(m_path[0].transform.position.x, 0, m_path[0].transform.position.z), new Vector3(transform.position.x, 0, transform.position.z)) < 0.1f)
            {
                m_path.RemoveAt(0);
            }
            */


            //// 다음 목적지로 이동하기
            //Vector3 movement = transform.forward * applySpeed * Time.fixedDeltaTime;
            //myRigid.MovePosition(myRigid.position + movement);

            // 이동할 방향 벡터 구하기
            Vector3 direction = (new Vector3(m_path[0].transform.position.x, transform.position.y + 0.01f, m_path[0].transform.position.z) - myRigid.position).normalized;

            // Rigidbody의 이동 방향과 속력 설정하기
            myRigid.MovePosition(myRigid.position + direction * applySpeed * Time.fixedDeltaTime);

            // 목적지에 도착했는지 검사하기
            if (Vector3.Distance(new Vector3(m_path[0].transform.position.x, 0.01f, m_path[0].transform.position.z), new Vector3(transform.position.x, 0, transform.position.z)) < 0.1f)
            {
                m_path.RemoveAt(0);
            }

        }
        else
        {
            anim.SetBool("isWalk", false); //아니라면 애니메이션 중지!
        }
    }

    ///패스파인딩
    public int GetDistance(Node a, Node b) //두 노드 사이의 거리를 계산
    {
        int x = Mathf.Abs(a.Col - b.Col); //현재 노드와 목표 노드의 열 (가로) 좌표 차이를 구함
        int y = Mathf.Abs(a.Row - b.Row); //현재 노드와 목표 노드의 행 (세로) 좌표 차이를 구함

        //대각선 방향으로 이동한 경우의 계산
        return 14 * Mathf.Min(x, y) + 10 * Mathf.Abs(x - y);
    }

    public List<Node> RetracePath(Node currNode) //최단 경로 상의 리스트를 반환한다~
    {
        List<Node> nodes = new List<Node>(); //최단 경로 상의 노드를 저장할 리스트 생성

        while (currNode != null) //현재 노드가 더 이상 부모 노드를 가지지 않을 떄까지 루프
        {
            nodes.Add(currNode); //현재 노드를 최단 경로 상의 노드 리스트에 추가
            currNode = currNode.Parent; //다음 노드는 현재 노드의 부모 노드
        }

        nodes.Reverse(); //최단 경로 상의 노드 리스트를 역순으로 바꿔서 반환

        return nodes; //최단 경로 상의 노드 리스트 반환
    }

    public void ResetNode()
    {   
        //StopAllCoroutines();
       // Debug.Log("리셋1");

        //현재 노드, 시작 노드, 목표 노드. 이전 노드를 null로 초기화
        m_currNode = null;
        m_startNode = null;
        m_targetNode = null;
        m_prevNode = null;

        //경로 노드 리스트, 현재 이웃 노드 리스트, 열린 노드 리스트, 닫힌 노드 리스트 비우기
        m_pathNode.Clear();
        m_currNeighbours.Clear();
        m_openList.Clear();
        m_closedList.Clear();

        m_grid2D.ResetNode(); //그리드 초기화 함수 호출

        m_execute = false;
    }

    public void ResetNode2()
    {   
        //StopAllCoroutines(); //모든 코루틴 중지
       // Debug.Log("리셋2");
        //또 null로 초기화!!
        m_currNode = null;
        m_startNode = null;
        m_targetNode = null;
        m_prevNode = null;

        //경로 노드 리스트가 null이 아니면서 요소가 있는 경우를 검사
        //이 조건문을 사용하여 먼저 null 여부를 검사하고
        //null이 아닌 경우에는 리스트의 요소 개수가 0보다 큰 지도 같이 검사.
        if (m_pathNode != null && m_pathNode.Count > 0) 
        {
            m_pathNode.Clear();
        }
        if (m_currNeighbours.Count > 0)
        {
            m_currNeighbours.Clear();
        }
        if (m_openList.Count > 0)
        {
            m_openList.Clear();
        }
        if (m_closedList.Count > 0)
        {
            m_closedList.Clear();
        }

        m_grid2D.StartSetNode();

        m_execute = false;
    }

    public void Ready(Vector3 player, Vector3 target) //시작 좌표와 목표 좌표를 전달 받는다 ^^3
    {
        m_execute = true; //길찾기 시작했음 ^^4

        m_openList.Clear(); //열린 노드를 담은 리스트 초기화 ^^5
        m_closedList.Clear(); //닫힌 노드를 담은 리스트 초기화 ^^6

        m_startNode = m_grid2D.FindNode(new Vector3(Mathf.Round(player.x), player.y, Mathf.Round(player.z)));
        //시작 노드를 찾아서 변수에 할당  
        m_targetNode = m_grid2D.FindNode(new Vector3(Mathf.Round(target.x), target.y, Mathf.Round(target.z)));

        if (m_startNode == m_targetNode)
        {
            ResetNode();
            return;
        }

        //목표 노드를 찾아서 변수에 할당 
        //'FindNode' 함수는 입력된 3d 좌표를 기반으로 해당 좌표에 해당하는 2d 그리드 맵 상의 노드를 찾아서 반환하는 함수

        //목표 노드의 타입이 장애물이나 물과 같은 경우 목표 노드를 초기화.
        //if(m_targetNode.m_nodeType.Equals(NodeType.Obstacle) || m_targetNode.m_nodeType.Equals(NodeType.Water))
        //{
        //    //StopCoroutine(IEStep());
        //    ResetNode();
        //    return;
        //}

        m_targetNode.SetParent(null);
        //타겟 노드의 부모 노드를 null로 초기화 
        m_startNode.SetParent(null);
        //시작 노드의 부모 노드를 null로 초기화 

        m_currNode = m_startNode;
        // 현재 노드를 시작 노드로 초기화 

        m_startNode.SetGCost(0);
        //시작 노드의 Gcost를 0으로 초기화 

        m_startNode.SetHCost(GetDistance(m_startNode, m_targetNode));
        //시작 노드와 타겟 노드 사이의 H cost 값을 구해서 초기화         
    }

    public void FindPathCoroutine(Vector3 Target)//target은 마우스 찍힌 타일의 좌표 ^^1
    {
        // if (m_path.Count > 0)   //이미 경로가 계산 되어 있는 지 확인
        // {
        //     m_path.Clear(); //경로가 계산 되어 있다면 이전 경로를 지우기~
        // }

        //ResetNode(); //노드 초기화~

        //if (!m_execute)//지금 길찾기 중이 아닌가?

        //목표 노드가 어떤 타입인지 로그로 출력
       // Debug.Log(m_grid2D.m_nodeArr[(int)Mathf.Round(Target.x), (int)Mathf.Round(Target.z)].m_nodeType +"["+ (int)Mathf.Round(Target.x)+","+ (int)Mathf.Round(Target.z)+"]" +Target);

        //목표 지점이 none 타입이라면 길찾기 실행
        if (!m_execute)
        {
            if (m_grid2D.m_nodeArr[(int)Mathf.Round(Target.x), (int)Mathf.Round(Target.z)].m_nodeType.Equals(NodeType.None))
            {
                Ready(transform.position, Target);//시작 좌표와 목표 좌표를 전해준다 ^^2

                StartCoroutine(IEStep()); //길찾기를 수행하는 코루틴 함수를 실행
            }
        }

        //길찾기중일때 경로가 수정되면 바뀌어야 함
    }

    public IEnumerator IEStep()
    {
        Node[] neighbours = m_grid2D.Neighbours(m_currNode);
        //현재 노드의 이웃 노드를 구함

        if (neighbours != null)
        {
            m_currNeighbours.Clear();
            //현재 이웃노드 리스트를 초기화

            m_currNeighbours.AddRange(neighbours);
            //현재 이웃 노드 리스트에 이웃 노드 배열을 추가

            for (int i = 0; i < neighbours.Length; ++i) //이웃 노드들을 순회
            {
                if (m_closedList.Contains(neighbours[i])) //이미 닫힌 리스트에 있는 노드면 다음 노드로 넘어감!
                {
                    continue;
                }

                if (neighbours[i].NType == NodeType.Obstacle || neighbours[i].NType == NodeType.Water) //물이나 장애물 노드면 다음 노드로 넘어감
                {
                    continue;
                }

                int gCost = m_currNode.GCost + GetDistance(neighbours[i], m_currNode); //G cost를 계산!

                if (m_openList.Contains(neighbours[i]) == false || gCost < neighbours[i].GCost) //이웃 노드가 열린 리스트에 없거나, G cost가 더 작을 경우 이웃 노드를 업데이트
                {
                    int hCost = GetDistance(neighbours[i], m_targetNode); //H cost 계산

                    //Gcost, Hcost, 부모 노드를 설정
                    neighbours[i].SetGCost(gCost);
                    neighbours[i].SetHCost(hCost);
                    neighbours[i].SetParent(m_currNode);

                    if (!m_openList.Contains(neighbours[i])) //열린 리스트에 이웃 노드를 추가
                    {
                        m_openList.Add(neighbours[i]);
                    }
                }
            }

            m_closedList.Add(m_currNode); //현재 노드를 닫힌 리스트에 추가

            if (m_openList.Contains(m_currNode)) //열린 리스트에 현재 노드가 있다면 현재 노드를 제거
            {
                m_openList.Remove(m_currNode);
            }

            if (m_openList.Count > 0) //열린 목록이 존재하는 경우
            {
                m_openList.Sort(delegate (Node x, Node y) //열린 목록을 정렬! 리스트 내의 노드들을 Fcost와 Hcost 값을 기준으로 오름차순으로 정렬
                {
                    if (x.FCost < y.FCost)
                    {
                        return -1;
                    }
                    else if (x.FCost > y.FCost)
                    {
                        return 1;
                    }
                    else if (x.FCost == y.FCost)
                    {
                        if (x.HCost < y.HCost)
                        {
                            return -1;
                        }
                        else if (x.HCost > y.HCost)
                        {
                            return 1;
                        }
                    }
                    return 0;
                });

                if (m_currNode != null) //현재 노드가 null이 아니면
                {
                    m_prevNode = m_currNode;//현재 노드를 이전 노드로 저장
                }

                m_currNode = m_openList[0];//열린 목록에서 첫 번째 노드를 현재 노드로 선택
            }

            yield return null; //코루틴 멈추고 넘어가기~

            if (m_currNode == m_targetNode) //현재 노드가 목표 노드인지 확인
            {
                List<Node> nodes = RetracePath(m_currNode); //현재 노드에서 시작해 역으로 경로를 구하기!
                m_pathNode = nodes; //구한 경로를 변수에 저장
                m_execute = false; //길찾기 실행 종료

                SetPath(m_pathNode); //구한 경로를 이용해 유니티에서 경로를 표시
                ResetNode();
            }
            else
            {
                if (m_closedList.Count > 300) //닫힌 노드 리스트가 300개 초과면
                {
                    ResetNode();    //노드 초기화 함수 실행
                }
                else
                {
                    StartCoroutine(IEStep()); //현재 노드가 목표 노드가 아니라면 다시 길찾기 시작
                }
            }
        }       
    }

     private void OnCollisionEnter(Collision collision)
    {
        if (!pv.isMine)
        {
            return;
        }

        if (collision.transform.tag.Equals("Building"))
        {
            ResetNode2();
        }
    }

    //제작대 사용
    private void OnTriggerStay(Collider other)
    {
        if (!pv.isMine)
        {
            return;
        }

        if (other.CompareTag("WorkBench") && !canTrigger)
        {
            other.transform.parent.GetComponent<csWorkBench>().ShowText();
            canTrigger = true;
            //GameObject.FindGameObjectWithTag("CraftingUI").SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!pv.isMine)
        {
            return;
        }

        if (other.CompareTag("WorkBench") && canTrigger)
        {
            other.transform.parent.GetComponent<csWorkBench>().HideText();
            canTrigger = false;
            //GameObject.FindGameObjectWithTag("CraftingUI").SetActive(false);
            //GameObject.Find("CraftingCanvas").SetActive(false);
            m_grid2D.craftingUI.SetActive(false);
        }
    }



    // private Color32 HexToColor32(string hex)
    // {
    //     // HEX 문자열을 RGB 값으로 분리
    //     byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
    //     byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
    //     byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

    //     // Color32로 변환하여 반환
    //     return new Color32(r, g, b, 255);
    // }

    private string ColorToHex(Color32 color)
    {
        // R, G, B 값을 HEX 문자열로 변환
        string r = color.r.ToString("X2");
        string g = color.g.ToString("X2");
        string b = color.b.ToString("X2");

        // '#' 문자열과 결합하여 반환
        return "#" + r + g + b;
    }

    //스마일~~

    public GameObject smile;

    public void StartSmile()
    {
        if (pV.isMine)
        {
            pV.RPC("StartSmileRPC", PhotonTargets.All, null);
        }
    }

    [PunRPC]
    public void StartSmileRPC()
    {
        StartCoroutine(Smile());
    }

    IEnumerator Smile()
    {
        smile.SetActive(true);
        yield return new WaitForSeconds(2f);
        smile.SetActive(false);
    }
}

//RetracePath() -> a* 알고리즘을 사용해 찾은 경로를 역추적해서 node 리스트로 반환하는 함수
//SetPath() -> a* 알고리즘으로 찾아낸 경로를 이용해 플레이어가 이동할 경로를 그려주는 함수

