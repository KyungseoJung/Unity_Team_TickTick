using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TeamInterface;
using JinscObjectBase;


public class csField : csObjectBase, IGrowth
{
    [SerializeField]
    bool haveGrowth;//흔들기 했을 때 뭐가 떨어지는앤가?

    [SerializeField]
    GameObject dropItem;//흔들기 했을 때 떨어지는 애

    [SerializeField]
    GameObject childObj;//성장단계에따라 오브젝트 껏다 켰다
    [SerializeField]
    GameObject fakeChildObj;

    [SerializeField]
    Enum_ObjectGrowthLevel growthLevel;//성장단계표현

    public PhotonView pVPG;

    public override void Awake()
    {
        base.Awake();
        pVPG = GameObject.FindGameObjectWithTag("PhotonGameManager").GetComponent<PhotonView>();
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Shake()//흔들기 당했을 때
    {
        if (haveGrowth && growthLevel.Equals(Enum_ObjectGrowthLevel.TWO))
        {
            StartCoroutine(DropItem());

            childObj.SetActive(false);
            fakeChildObj.SetActive(false);
            growthLevel = Enum_ObjectGrowthLevel.ZERO;
        }

        base.Shake();
    }

    public override void DropItemFct()//채집 당했을 때
    {
        if (haveGrowth && growthLevel.Equals(Enum_ObjectGrowthLevel.TWO))
        {
            StartCoroutine(DropItem());

            childObj.SetActive(false);
            fakeChildObj.SetActive(false);
            growthLevel = Enum_ObjectGrowthLevel.ZERO;
        }

        //base.DropItemFct();

        if (isDie)
        {
            transform.parent.GetComponent<ICubeInfo>().CubeInfo.haveChild = false;
            gameObject.SetActive(false);
            Invoke("DelObj", 0.5f);
        }
    }

    void DelObj()
    {
        Destroy(gameObject);
    }

    IEnumerator DropItem()
    {
        //if (!PhotonNetwork.isMasterClient)
        //{
        //    pV.RPC("CreateFieldRPC", PhotonTargets.MasterClient, new Vector3(transform.position.x, transform.position.y + 0.7f, transform.position.z));
        //}
        //else
        //{
        //    CreateFieldRPC(new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z));
        //}
        PG.CreateDropItem(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), dropItem.name);

        yield return null;
    }

    public override void GrowthDay()//시간에 따른 흐름마다 일어나는 일
    {
        if (growthLevel.Equals(Enum_ObjectGrowthLevel.ZERO))
        {
            childObj.SetActive(true);
            fakeChildObj.SetActive(false);
            growthLevel = Enum_ObjectGrowthLevel.ONE;
        }
        else if (growthLevel.Equals(Enum_ObjectGrowthLevel.ONE))
        {
            childObj.SetActive(true);
            fakeChildObj.SetActive(true);
            growthLevel = Enum_ObjectGrowthLevel.TWO;
        }
        else if (growthLevel.Equals(Enum_ObjectGrowthLevel.TWO))
        {
            if (haveGrowth && Random.Range(0, 5) == 0)
            {
                StartCoroutine(DropItem());

                childObj.SetActive(false);
                fakeChildObj.SetActive(false);
                growthLevel = Enum_ObjectGrowthLevel.ZERO;
            }           
        }
     }

}
