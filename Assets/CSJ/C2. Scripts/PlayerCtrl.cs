using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamInterface;



public class PlayerCtrl : MonoBehaviour, IObjectStatus
{
    // 스피드 조정 변수
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;
    private float applySpeed;

    // 점프 정도
    [SerializeField]
    private float jumpForce;

    // 상태 변수
    private bool isRun = false;
    private bool isGround = true;
    private bool isCrouch = false;

    // 앉았을 때 얼마나 앉을지 결정하는 변수
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

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

    float hp = 0;
    public float Hp { get { return hp; } set { hp = value; } }

    float stamina = 0;
    public float Stamina { get { return stamina; } set { stamina = value; } }

    GameObject[] dropItems;
    public GameObject[] DropItems { get { return dropItems; } set { dropItems = value; } }

    bool isDie = false;

    float currentHp = 0;

    float maxHp;

    private HPBar hpBar;

    float damage;

    [SerializeField]
    private float maxHP;
    private float currentHP;

    [SerializeField]
    private HPBar hPBar;


    private void Awake()
    {
        theCamera = Camera.main;
        
    }

    void Start()
    {
        // 컴포넌트 할당
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();

        // 초기화
        applySpeed = walkSpeed;

        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY;

        anim = GetComponent<Animator>();

        theCamera.transform.SetParent(camPos);
        theCamera.transform.position = camPos.transform.position;
        theCamera.transform.localRotation = camPos.transform.localRotation;

        currentHP = maxHP;
        hpBar.UpdateHPBar(currentHP, maxHP);


        hpBar = GetComponent<HPBar>();
    }

    void Update()
    {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        CameraRotation();
        CharacterRotation();
        
    }

    // 지면 체크
    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
    }

    // 점프 시도
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
        }
    }

    // 점프
    private void Jump()
    {
        if (isCrouch)
            Crouch();

        myRigid.velocity = transform.up * jumpForce;
    }

    // 달리기 시도
    private void TryRun()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }

       
    }

    // 달리기
    private void Running()
    {
        if (isCrouch)
            Crouch();

        isRun = true;
        applySpeed = runSpeed;

        
    }

    // 달리기 취소
    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;
    }

    // 앉기 시도
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }

    }

    // 앉기 동작
    private void Crouch()
    {
        isCrouch = !isCrouch;
        if (isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        StartCoroutine(CrouchCoroutine());

    }

    // 부드러운 앉기 동작
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while (_posY != applyCrouchPosY)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.2f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (count > 15)
                break;
            yield return null;
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0);
    }

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
        anim.SetBool("isRun", isRun);
       
    }

    private void CameraRotation()
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = _xRotation * lookSensitivity;

        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }

    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
    }

    public void DamageProcess(int damage)
    {
        Debug.Log("Player damaged by " + damage + " points.");
        // 여기에 플레이어가 데미지를 입었을 때 처리할 내용을 추가할 수 있습니다.
    }

    public void HealProcess(int healAmount)
    {
        Debug.Log("Player healed by " + healAmount + " points.");
        // 여기에 플레이어가 회복 아이템을 사용했을 때 처리할 내용을 추가할 수 있습니다.

    }

    //회복 아이템이 존재..? 하나? 농사를 해서 농작물을 먹으면 체력이 회복되려나..

    public void DeathProcess()
    {
        Debug.Log("Player is dead.");
        // 여기에 플레이어가 죽었을 때 처리할 내용을 추가할 수 있습니다.


    }

    public void SetHpDamaged(float damage, Enum_PlayerUseItemType Type)
    {
        //구현

        currentHP -= damage;
        if (currentHP < 0)
        {
            currentHP = 0;
        }

        // HPBar 업데이트
        hpBar.UpdateHPBar(currentHP, maxHP);
    }


    public float HpFill()
    {

        // HP 채우기
        //hp = maxHp;

        // HPBar 업데이트
        hpBar.UpdateHPBar(hp, maxHp);

        return currentHp / maxHp;
    }




    

    //Attack, Die, Walk (얜 만들었는데 이상해서 바꾸는 게 나을 듯), trace 
    //(낚시 게임 만들자고 했으니까 혹시 모르니..)fishig 맞나?
    //공격 모션이랑 일하는 모션은 똑같아서 별로 만들 필요 없을 듯..
    //가장 문제점은 자연스러운 애니메이션 만들기..
    //잠수, 수영 등의 모션은 잠수기능과 수영기능을 구현할 건지 안할건지부터 알아야됨
    //또 뭐가 잇을까나..
    //점프 애니메이션은 만들었는데 애니메이터가 연결 안되있음 이거 바꾸기.


    //포톤 각자 방 만들어서 해보는 거 !!! 그거 이번 주까지 해보는 걸로 !!! 
    //근데 경서님이 만들어주셔야 할 수 잇을듯

    

}

