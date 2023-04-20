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

    //bool isDie = false;

    float currentHp = 0;

    float maxHp = 0;

    private HPBar hpBar;

    float damage;

     [SerializeField]
     public float maxHP;
     public float currentHP;

    [SerializeField]
    private HPBar hPBar;


    private void Awake()
    {
        
        
    }

    void Start()
    {
        // 컴포넌트 할당
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        theCamera = Camera.main;

        Debug.Assert(theCamera);
        
        // 초기화
        applySpeed = walkSpeed;

        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY;

        anim = GetComponent<Animator>();

        theCamera.transform.SetParent(camPos);
        theCamera.transform.position = camPos.transform.position;
        theCamera.transform.localRotation = camPos.transform.localRotation;

        currentHP = maxHP;
        //hpBar.UpdateHPBar(currentHP, maxHP);

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

        // HP 채우기
        hp = maxHp;

        // HPBar 업데이트
        hpBar.UpdateHPBar(hp, maxHp);

        return currentHp / maxHp;
    }




    

}

