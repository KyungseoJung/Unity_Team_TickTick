using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamInterface;
using UnityEngine.UI;




public class PlayerCtrl : MonoBehaviour, IObjectStatus, IPhotonBase, IPhotonInTheRoomCallBackFct
{
    // 스피드 조정 변수
    [SerializeField]
    private float walkSpeed;
    //[SerializeField]
    //private float runSpeed;
    //[SerializeField]
    //private float crouchSpeed;
    private float applySpeed;

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
    [SerializeField]
    private float lookSensitivity;

    // 카메라 한계
    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX;

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

    Rigidbody myRbody;
    public Transform camPivot;
    
    //private bool isWriting;
    /// <summary>
    /// ///////////////////////////////////////////
    /// </summary>
    //bool isDie = false;

    float damage;

     [SerializeField]
     public float maxHP;

    [SerializeField]
    private HPBar hpBar;

    private Transform Tr; //자신의 트랜스폼 참조 변수

    float mouseX=0f;
    float mouseY=0f;

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
        //메인 카메라에 추가된 SmoothFollowCam 스크립트(컴포넌트)에 추적 대상을 연결 
            //Camera.main.GetComponent<SmoothFollowCam>().target = camPivot;
         }
        else    //자신의 네트워크 객체가 아닐때...
        {
        //원격 네트워크 유저의 아바타는 물리력을 안받게 처리하고
        //또한, 물리엔진으로 이동 처리하지 않고(Rigidbody로 이동 처리시...)
        //실시간 위치값을 전송받아 처리 한다 그러므로 Rigidbody 컴포넌트의
        //isKinematic 옵션을 체크해주자. 한마디로 물리엔진의 영향에서 벗어나게 하여
        //불필요한 물리연산을 하지 않게 해주자...(만약 수십명의 플레이어가 접속 한다면???)

        //원격 네트워크 플레이어의 아바타는 물리력을 이용하지 않음 
        //(원래 게임이 이렇다는거다...우리건 안해도 체크 돼있음...)
            myRbody.isKinematic = true;
        }


        currPos = Tr.position; 
        currRot = Tr.rotation;

        pv.viewID = PhotonNetwork.AllocateViewID();

    }

    IEnumerator Start()
    {   
        yield return new WaitForSeconds(5.0f);

        //  if(pv.isMine)   //#20-1
        // {
        //     // 일정 간격으로 주변의 가장 가까운 Enemy를 찾는 코루틴 
        //     StartCoroutine(this.TargetSetting());

        // // // 가장 가까운 적을 찾아 발사...
        //     StartCoroutine(this.ShotSetting());
        // }

        // 컴포넌트 할당
        theCamera = Camera.main;

        Debug.Assert(theCamera);
        
        // 초기화
        applySpeed = walkSpeed;

       //originPosY = theCamera.transform.localPosition.y;
       // applyCrouchPosY = originPosY;

    
        theCamera.transform.SetParent(camPos);
        theCamera.transform.position = camPos.transform.position;
        theCamera.transform.localRotation = camPos.transform.localRotation;

        currentHP = maxHP;

        //Debug.Log(currentHP +"..."+ maxHP);
        //hpBar.SetMaxHealth(maxHP);
        //hpBar.UpdateHPBar(currentHP, maxHP);

        //PhotonView pV = GetComponent<PhotonView>();
    }

    void Update()
    {
        //임시
        //hpBar.UpdateHPBar(currentHP, maxHP);
        IsGround();
        //TryJump();
        //TryRun();
        //TryCrouch();
        Move();
        //CameraRotation();
        CharacterRotation();

        if(pv.isMine)
        {
            float _moveDirX = Input.GetAxisRaw("Horizontal");
            float _moveDirZ = Input.GetAxisRaw("Vertical");

             Vector3 _moveHorizontal = transform.right * _moveDirX;
            Vector3 _moveVertical = transform.forward * _moveDirZ;
            Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

            transform.position += _velocity * Time.deltaTime;
        }
        
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

        mouseX += Input.GetAxis("Mouse X") * 5;
        mouseY += Input.GetAxis("Mouse Y") * 5;
        //transform.eulerAngles = new Vector3(-mouseY,mouseX, 0);
        transform.eulerAngles = new Vector3(0,mouseX, 0); // 캐릭터 좌우 회전
        theCamera.transform.eulerAngles = new Vector3(Mathf.Clamp((-mouseY), -35f, 50f), mouseX, 0);// 카메라 상하, 좌우 회전
       

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
        Debug.Log("Player is dead.");
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




    /// <summary>
    /// ///////////////////
    /// 포톤추가
    /// </summary>
    /// 

    //PhotonView 컴포넌트의 Observe 속성이 스크립트 컴포넌트로 지정되면 PhotonView
    //컴포넌트는 데이터를 송수신할 때, 해당 스크립트의 OnPhotonSerializeView 콜백 함수를 호출한다.
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //...


     if (stream.isWriting)
    {
        // 내가 보내는 경우 
        stream.SendNext(Tr.position);
        stream.SendNext(Tr.rotation);
    }
    else
    {
        // 다른 플레이어가 보내는 경우 
        currPos = (Vector3)stream.ReceiveNext();
        currRot = (Quaternion)stream.ReceiveNext();
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


}

