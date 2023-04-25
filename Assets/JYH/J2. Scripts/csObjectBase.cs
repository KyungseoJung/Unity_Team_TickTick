using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TeamInterface;

// 나무 오브젝트 시간에 따라 성장/ 열매생성
// 흙 위에 씨앗을 뿌리고 흙을 덮으면 나무가 자라야한다...

namespace JinscObjectBase
{
    public class csObjectBase : MonoBehaviour, IObjectStatus, IDropItem
    {
        [SerializeField]
        float hp = 0;
        public float Hp { get { return hp; } set { hp = value; } }

        [SerializeField]
        float stamina = 0;
        public float Stamina { get { return stamina; } set { stamina = value; } }

        public float maxHP;

        public float HpFill() { return hp / maxHP; }


        [SerializeField]
        GameObject[] dropItems;
        public GameObject[] DropItems { get { return dropItems; } set { dropItems = value; } }

        protected bool isDie = false;

        public Enum_ObjectType ObjType;

        public GameObject hpBarObj;
        public float haBarAddY;
        HPBar hpBar;

        bool isDamaged = false;

        public virtual void Awake()
        {
            
        }

        public virtual void Start()
        {
            //GameObject tmpObj = Instantiate(hpBarObj);            
            //tmpObj.transform.SetParent(this.transform);
            //Debug.Log(transform.position);
            //tmpObj.transform.position = new Vector3(transform.root.position.x, transform.root.position.y+haBarAddY, transform.root.position.z);
            //tmpObj.transform.localScale = new Vector3(0.08f, 0.01f, 1f);
            //tmpObj.SetActive(false);

            if (hpBarObj != null)
            {
                hpBar = hpBarObj.GetComponent<HPBar>();

                hpBarObj.transform.position = new Vector3(transform.parent.position.x, transform.parent.position.y + haBarAddY, transform.parent.position.z);
                hpBarObj.transform.localScale = new Vector3(0.08f, 0.01f, 1f);

                hpBarObj.SetActive(false);
                // hpBar.SetMaxHealth(maxHP);
            }
        }

        public void SetHpDamaged(float dmg, Enum_PlayerUseItemType useItemType)
        {
            switch (ObjType)
            {
                case Enum_ObjectType.NONE:
                case Enum_ObjectType.FLOWER:
                case Enum_ObjectType.GRASS:
                    if (useItemType == Enum_PlayerUseItemType.HAND)
                    {
                        Shake();
                    }
                    else
                    {
                        hp -= dmg;
                        if (maxHP!=1 && hpBar != null)
                        {
                            hpBar.UpdateHPBar(hp,maxHP);
                        }
                    }
                    break;
                case Enum_ObjectType.TREE:

                    if (useItemType == Enum_PlayerUseItemType.AXE)
                    {
                        hp -= dmg;
                        if (maxHP != 1 && hpBar != null)
                        {
                            hpBar.UpdateHPBar(hp, maxHP);
                        }
                    }
                    else
                    {
                        Shake();
                    }
                    break;
                case Enum_ObjectType.ROCK:
                    if (useItemType == Enum_PlayerUseItemType.PICKAXE)
                    {
                        hp -= dmg;
                        if (maxHP != 1 && hpBar != null)
                        {
                            hpBar.UpdateHPBar(hp, maxHP);
                        }
                    }
                    else
                    {
                        Shake();
                    }
                    break;
                case Enum_ObjectType.FIELD:
                    if (useItemType == Enum_PlayerUseItemType.SHOVEL)
                    {
                        //Debug.Log(2222222);
                        hp -= dmg;
                        if (maxHP != 1 && hpBar != null)
                        {
                            hpBar.UpdateHPBar(hp, maxHP);
                        }
                    }
                    else
                    {
                        Shake();
                    }

                    
                    break;
            }
        }

        public virtual void Shake()
        {
            Debug.Log("shake~~");
        }

        public virtual void DropItemFct()
        {
            StartCoroutine(Drop());
        }

        IEnumerator Drop()
        {
            Collider tmpcol = this.GetComponent<Collider>();

            if (tmpcol != null)
            {
                tmpcol.isTrigger = true;
            }

            if (dropItems.Length > 0)//처음자리에 무조건 이펙트 파티클 넣으면 이쁠듯
            {
                for (int j = 0; j < Random.Range(1, 3); j++)
                {
                    for (int i = 0; i < dropItems.Length; i++)
                    {
                        GameObject tmp = Instantiate(dropItems[i], new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z), Quaternion.identity);
                        tmp.GetComponent<Rigidbody>().AddForce(Vector3.up * Time.deltaTime * (Random.Range(2, 5) * 5000f));
                        tmp.transform.SetParent(null);
                        //Debug.Log("drop");

                        //사운드 재생?

                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }

            transform.parent.GetComponent<ICubeInfo>().CubeInfo.haveChild = false;

            gameObject.SetActive(false);
            Invoke("DelObj", 0.5f);
        }

        void DelObj()
        {
            Destroy(gameObject);
        }

        public virtual void Update()
        {
            if (hp <= 0 && !isDie)
            {
                //Debug.Log("die check");
                isDie = true;

                DropItemFct();
            }
            else if( hp < maxHP)
            {
                hpBar.gameObject.SetActive(true);

                if (!isDamaged)
                {
                    //Debug.Log(111567);
                    isDamaged = true;
                    StartCoroutine(RegenHp());
                }
            }
            else if(hp >= maxHP)
            {
                if (isDamaged)
                {
                   // Debug.Log(111345);
                    StopCoroutine(RegenHp());
                    hp = maxHP;
                    hpBar.gameObject.SetActive(false);
                    isDamaged = false;
                }
            }
        }

        IEnumerator RegenHp()
        {       
            while (hp < maxHP)
            {
                yield return new WaitForSeconds(5f);
                //Debug.Log(111123);
                hp++;                
            }

            if (hp > maxHP)
            {
                hp = maxHP;
            }
        }

        public virtual void GrowthDay() { }
        public virtual void GrowthRain() { }
        public virtual void SetGrowthLevel(Enum_ObjectGrowthLevel gl) { }
    }
}