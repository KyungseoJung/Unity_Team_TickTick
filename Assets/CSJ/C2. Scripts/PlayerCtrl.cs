using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamInterface;



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
    public Vector3 currPos { get { return currPos; } set { currPos = value; } }

    [SerializeField]
    Quaternion cUrrRot;
    public Quaternion currRot { get { return cUrrRot; } set { cUrrRot = value; } }
    /// <summary>
    /// ///////////////////////////////////////////
    /// </summary>
    //bool isDie = false;

    float damage;

     [SerializeField]
     public float maxHP;

    [SerializeField]
    private HPBar hpBar;



    float mouseX=0f;
    float mouseY=0f;

    private void Awake()
    {        
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // 컴포넌트 할당
        
        theCamera = Camera.main;

        Debug.Assert(theCamera);
        
        // 초기화
        applySpeed = walkSpeed;

       //originPosY = theCamera.transform.localPosition.y;
       // applyCrouchPosY = originPosY;

        anim = GetComponent<Animator>();

        theCamera.transform.SetParent(camPos);
        theCamera.transform.position = camPos.transform.position;
        theCamera.transform.localRotation = camPos.transform.localRotation;

        currentHP = maxHP;

        //Debug.Log(currentHP +"..."+ maxHP);
        //hpBar.SetMaxHealth(maxHP);
        //hpBar.UpdateHPBar(currentHP, maxHP);
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

        // Animator 컴포넌트 가져오기
        Animator animator = GetComponent<Animator>();

        anim.SetBool("isWalk", _velocity != Vector3.zero);
        //anim.SetBool("isRun", isRun);
       
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
        transform.eulerAngles = new Vector3(0,mouseX, 0);
        //좌우 회전

        //theCamera.transform.eulerAngles = new Vector3(mouseY,0, 0);
        Vector3 tpmCamPos =new Vector3( -mouseY, transform.rotation.y,0);
        camPos.transform.rotation = Quaternion.Euler(tpmCamPos);
        theCamera.transform.LookAt(camPos);
        //위아래 회전
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

