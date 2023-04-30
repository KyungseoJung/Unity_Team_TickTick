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

    public bool inTheHouse = false;

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
        m_grid2D = GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<csPhotonGame>();

        pv.ObservedComponents[0] = this;
        pv.synchronization = ViewSynchronization.UnreliableOnChange;


        if(pv.isMine) // PhotonNetwork.isMasterClient 마스터 클라이언트는 이런식 체크
        {
            m_grid2D.myPlyerCtrl = this;
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
            myRigid.isKinematic = true;
        }


        currPos = Tr.position; 
        currRot = Tr.rotation;

        //pv.viewID = PhotonNetwork.AllocateViewID();

    }

    IEnumerator Start()
    {
        Debug.Assert(m_grid2D);

        yield return new WaitForSeconds(5.0f);

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
    }

    void Update()
    {
        if (m_grid2D.isUiBlock)
        {
            return;
        }

        if (!pv.isMine)
        {
            MoveAvarta();
            return;
        }

        //임시
        //hpBar.UpdateHPBar(currentHP, maxHP);

        //TryJump();
        //TryRun();
        //TryCrouch();

        IsGround();

        if (inTheHouse)
        {
            Move();
        }

        CharacterRotation();


        //
        //MoveBlock();


        //CameraRotation();


        //float _moveDirX = Input.GetAxisRaw("Horizontal") *0.5f;
        //float _moveDirZ = Input.GetAxisRaw("Vertical") * 0.5f;

        //Vector3 _moveHorizontal = transform.right * _moveDirX;
        //Vector3 _moveVertical = transform.forward * _moveDirZ;
        //Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

        //transform.position += _velocity * Time.deltaTime;

    }

    private void LateUpdate()
    {
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

    //A*
    [Header("A* 관련-플레이어가 찾아갈 길")]
    [SerializeField]
    private List<Node> m_path = new List<Node>();

    [Header("A* 관련-패스파인딩")]
    public csPhotonGame m_grid2D;
    public List<Node> m_closedList = new List<Node>();
    public List<Node> m_openList = new List<Node>();
    public Node m_currNode;
    public Node m_startNode;
    public Node m_targetNode;
    public Node m_prevNode;
    public List<Node> m_pathNode;
    public List<Node> m_currNeighbours = new List<Node>();
    public bool m_execute = false;

    public void SetPath(List<Node> path)
    {
        if (path == null)
        {
            return;
        }

        m_path.Clear();

        foreach (Node p in path)
        {
            m_path.Add(p);
        }
    }

    public void MoveBlock()//목적지에 도착했을 때 다음 노드 탐색해야 함
    {
        if (m_path.Count > 0)
        {
            anim.SetBool("isWalk", true);
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
            anim.SetBool("isWalk", false);
        }
    }

    ///패스파인딩
    public int GetDistance(Node a, Node b)
    {
        int x = Mathf.Abs(a.Col - b.Col);
        int y = Mathf.Abs(a.Row - b.Row);

        return 14 * Mathf.Min(x, y) + 10 * Mathf.Abs(x - y);
    }

    public List<Node> RetracePath(Node currNode)
    {
        List<Node> nodes = new List<Node>();

        while (currNode != null)
        {
            nodes.Add(currNode);
            currNode = currNode.Parent;
        }

        nodes.Reverse();

        return nodes;
    }

    public void ResetNode()
    {
        m_currNode = null;
        m_startNode = null;
        m_targetNode = null;
        m_prevNode = null;

        m_pathNode.Clear();
        m_currNeighbours.Clear();
        m_openList.Clear();
        m_closedList.Clear();

        m_grid2D.ResetNode();
    }

    public void ResetNode2()
    {
        m_currNode = null;
        m_startNode = null;
        m_targetNode = null;
        m_prevNode = null;

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
    }

    public void Ready(Vector3 player, Vector3 target)//시작 좌표와 목표 좌표를 전달 받는다 ^^3
    {
        m_execute = true;//길찾기 시작했음 ^^4

        m_openList.Clear();//열린 노드를 담은 리스트 초기화 ^^5
        m_closedList.Clear();

        m_startNode = m_grid2D.FindNode(new Vector3(Mathf.Round(player.x), player.y, Mathf.Round(player.z)));
        m_targetNode = m_grid2D.FindNode(new Vector3(Mathf.Round(target.x), target.y, Mathf.Round(target.z)));

        m_targetNode.SetParent(null);
        m_startNode.SetParent(null);

        m_currNode = m_startNode;

        m_startNode.SetGCost(0);
        m_startNode.SetHCost(GetDistance(m_startNode, m_targetNode));
    }

    public void FindPathCoroutine(Vector3 Target)//target은 마우스 찍힌 타일의 좌표 ^^1
    {
        if (m_path.Count > 0)
        {
            m_path.Clear();
        }
        //if (!m_execute)//지금 길찾기 중이 아닌가?
        {
            Ready(transform.position, Target);//시작 좌표와 목표 좌표를 전해준다 ^^2

            StartCoroutine(IEStep());
        }
        //길찾기중일때 경로가 수정되면 바뀌어야 함
    }

    public IEnumerator IEStep()
    {
        Node[] neighbours = m_grid2D.Neighbours(m_currNode);

        m_currNeighbours.Clear();

        m_currNeighbours.AddRange(neighbours);

        for (int i = 0; i < neighbours.Length; ++i)
        {
            if (m_closedList.Contains(neighbours[i]))
            {
                continue;
            }

            if (neighbours[i].NType == NodeType.Water || neighbours[i].NType == NodeType.Obstacle)
            {
                continue;
            }

            int gCost = m_currNode.GCost + GetDistance(neighbours[i], m_currNode);

            if (m_openList.Contains(neighbours[i]) == false || gCost < neighbours[i].GCost)
            {
                int hCost = GetDistance(neighbours[i], m_targetNode);

                neighbours[i].SetGCost(gCost);
                neighbours[i].SetHCost(hCost);
                neighbours[i].SetParent(m_currNode);

                if (!m_openList.Contains(neighbours[i]))
                {
                    m_openList.Add(neighbours[i]);
                }
            }
        }

        m_closedList.Add(m_currNode);

        if (m_openList.Contains(m_currNode))
        {
            m_openList.Remove(m_currNode);
        }

        if (m_openList.Count > 0)
        {
            m_openList.Sort(delegate (Node x, Node y)
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

            if (m_currNode != null)
            {
                m_prevNode = m_currNode;
            }

            m_currNode = m_openList[0];
        }

        yield return null;

        if (m_currNode == m_targetNode)
        {
            List<Node> nodes = RetracePath(m_currNode);
            m_pathNode = nodes;
            m_execute = false;
            
            SetPath(m_pathNode);
        }
        else
        {
            StartCoroutine(IEStep());
        }
    }
}

