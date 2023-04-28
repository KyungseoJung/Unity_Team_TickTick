using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamInterface;


public class EnemyAttack : MonoBehaviour, IObjectStatus, IPhotonInTheRoomCallBackFct
{
    public float Hp { get { return energy; } set { energy = value; } }

    [SerializeField]
     float stamina = 0;
    public float Stamina { get { return stamina; } set { stamina = value; } }
    public float maxHP = 5;
    public float attackRange = 2f;      // 일정 범위
    public float energy = 5f;         // 에너지 초기값
    public float damagePerHit = 1f;    // 공격 데미지
    

    private Animator animator;
    int net_Aim = 0;
    private Transform myTr;
    public GameObject hpBarObj;
    public float haBarAddY;
    HPBar hpBar;
    //EnemyAttack enemyAttack = new EnemyAttack();

    public bool isAttackNow = false;
    
    public virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {

        Debug.Assert(animator);

        if (hpBarObj != null)
        {
            hpBar = hpBarObj.GetComponent<HPBar>();

            hpBarObj.transform.position = new Vector3(transform.position.x, transform.position.y + haBarAddY, transform.position.z);
            hpBarObj.transform.localScale = new Vector3(0.08f, 0.01f, 1f);

            hpBarObj.SetActive(false);
            // hpBar.SetMaxHealth(maxHP);
        }
    }


    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player" &&!isAttackNow)
        {
            float distance = Vector3.Distance(transform.position, other.transform.position);
            if (distance <= attackRange)
            {
                //isAttackNow = true;
                //animator.SetBool("Walk", false);
                //animator.SetBool("Attack", true);
                
                // 플레이어가 일정 범위 내에 있을 때 공격 애니메이션 실행
                animator.SetTrigger("Attack");
                animator.SetBool("Walk", false);
                net_Aim = 1;
                

                isAttackNow = true;
                Invoke("ReSetAttackNow", 0.2f);
                // 플레이어 캐릭터의 TakeDamage 함수 호출
                //other.gameObject.GetComponent<PlayerController>().TakeDamage(damagePerHit);
            }
        }
    }

    void ReSetAttackNow()
    {
        isAttackNow = false;
    }

     public float HpFill()
     {
         return Hp / maxHP;
     }

     public void SetHpDamaged(float dmg,Enum_PlayerUseItemType useItemType)
    {
        switch(useItemType){
            case Enum_PlayerUseItemType.PLAYERWEAPONAXE1:
            energy -= dmg;
            break;
            default:
            energy -= 1;
            break;
        }
        animator.SetTrigger("Stunned");
        net_Aim = 3;

        if (energy <= 0)
        {
            // 에너지가 없으면 사망 애니메이션 실행
            animator.SetTrigger("Die");
            net_Aim = 2;
            // 이후, 다른 액션을 막기 위해 스크립트를 비활성화
            enabled = false;
        }
    }
   
      
        //PhotonView 컴포넌트를 할당할 레퍼런스 
        public PhotonView pv { get; set; }        

        //위치 정보를 송수신할 때 사용할 변수 선언 및 초기값 설정 
        public Vector3 currPos { get; set; }
        public Quaternion currRot {get; set;}
        
    

        /*
      PhotonView 컴포넌트의 Observe 속성이 스크립트 컴포넌트로 지정되면 PhotonView
      컴포넌트는 데이터를 송수신할 때, 해당 스크립트의 OnPhotonSerializeView 콜백 함수를 호출한다. 
     */
       public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
       {
            //로컬 플레이어의 위치 정보를 송신
        if (stream.isWriting)
        {
            //박싱
            stream.SendNext(myTr.position);
            stream.SendNext(myTr.rotation); 
            stream.SendNext(net_Aim);
        }
        //원격 플레이어의 위치 정보를 수신
        else
        {
            //언박싱
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();
            net_Aim = (int)stream.ReceiveNext();
        }
       }
        
        public void OnPhotonInstantiate(PhotonMessageInfo info)// 네트워크 객체 생성 완료시 자동 호출되는 함수
        {
        
        }
        public void OnMasterClientSwitched(PhotonPlayer newMasterClient)// 마스터 클라이언트가 변경되면 호출
        {
        
        }
    
}
